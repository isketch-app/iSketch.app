using System;
using System.Data.SqlClient;

namespace iSketch.app.Data
{
    public enum PermissionsA : ulong
    {
        None = 0x0,
        Administration = 0x1
    }
    public enum PermissionsB : ulong 
    {
        None = 0x0
    }
    public class Permissions
    {
        public PermissionsA PermissionsA = PermissionsA.None;
        public PermissionsB PermissionsB = PermissionsB.None;
        public bool HasPermission(PermissionsA a)
        {
            return (PermissionsA & a) == a;
        }
        public bool HasPermission(PermissionsB b)
        {
            return (PermissionsB & b) == b;
        }
        public bool HasPermission(PermissionsA a, PermissionsB b)
        {
            return HasPermission(a) && HasPermission(b);
        }
    }
    public static class PermissionsStatic
    {
        public static Permissions ReadPermissionsFromDatabase(this SqlConnection sqlCon, Guid UserID)
        {
            Permissions perms = new Permissions();
            SqlCommand sCmd = sqlCon.CreateCommand();
            sCmd.Parameters.AddWithValue("@USERID@", UserID);
            sCmd.CommandText = "SELECT PermissionsA, PermissionsB FROM [Security.Users.Permissions.Splice] WHERE [UserID] = @USERID@";
            SqlDataReader sRead = sCmd.ExecuteReader();
            if (sRead.HasRows)
            {
                while (sRead.Read())
                {
                    byte[] bytes = new byte[8];
                    sRead.GetBytes(0, 0, bytes, 0, 8);
                    perms.PermissionsA = perms.PermissionsA | (PermissionsA)BitConverter.ToUInt64(bytes, 0);
                    sRead.GetBytes(1, 0, bytes, 0, 8);
                    perms.PermissionsB = perms.PermissionsB | (PermissionsB)BitConverter.ToUInt64(bytes, 0);
                }
            }
            sRead.Close();
            return perms;
        }
    }
}