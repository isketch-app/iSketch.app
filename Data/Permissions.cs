﻿using iSketch.app.Services;
using System;
using System.Data.SqlClient;

namespace iSketch.app.Data
{
    public enum PermissionsA : ulong
    {
        None = 0x0,
        Administration = 0x1,
        PlaceHolder = 0x2
    }
    public enum PermissionsB : ulong 
    {
        None = 0x0
    }
    public class Permissions
    {
        public PermissionsA PermissionsA = PermissionsA.None;
        public PermissionsB PermissionsB = PermissionsB.None;
        public bool HasEachPermission(PermissionsA a)
        {
            return (PermissionsA & a) == a;
        }
        public bool HasEachPermission(PermissionsB b)
        {
            return (PermissionsB & b) == b;
        }
        public bool HasEachPermission(PermissionsA a, PermissionsB b)
        {
            return HasEachPermission(a) && HasEachPermission(b);
        }
        public bool HasAnyPermission(PermissionsA a)
        {
            return (PermissionsA & a) != 0x0;
        }
        public bool HasAnyPermission(PermissionsB b)
        {
            return (PermissionsB & b) != 0x0;
        }
        public bool HasAnyPermission(PermissionsA a, PermissionsB b)
        {
            return HasAnyPermission(a) || HasAnyPermission(b);
        }
    }
    public static class PermissionsStatic
    {
        public static Permissions ReadPermissionsFromDatabase(this Database db, Guid UserID)
        {
            Permissions perms = new Permissions();
            SqlCommand sCmd = db.NewConnection.CreateCommand();
            try
            {
                sCmd.Parameters.AddWithValue("@USERID@", UserID);
                sCmd.CommandText = "SELECT PermissionsA, PermissionsB FROM [Security.Users.Permissions.Splice] WHERE [UserID] = @USERID@";
                SqlDataReader sRead = sCmd.ExecuteReader();
                if (sRead.HasRows)
                {
                    while (sRead.Read())
                    {
                        byte[] bytes;
                        bytes = new byte[8];
                        if (!sRead.IsDBNull(0)) sRead.GetBytes(0, 0, bytes, 0, 8);
                        perms.PermissionsA |= (PermissionsA)BitConverter.ToUInt64(bytes, 0);
                        bytes = new byte[8];
                        if (!sRead.IsDBNull(1)) sRead.GetBytes(1, 0, bytes, 0, 8);
                        perms.PermissionsB |= (PermissionsB)BitConverter.ToUInt64(bytes, 0);
                    }
                }
                return perms;
            }
            finally
            {
                sCmd.Connection.Close();
            }
        }
    }
}