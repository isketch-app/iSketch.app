using iSketch.app.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace iSketch.app.Data
{
    public enum Permission
    {
        Administrator, //Main administrator page.
        Words //Words administrator page.
    }
    public struct Access
    {
        public Access(Permission Permission, bool Read = false, bool Write = false, bool Create = false, bool Delete = false)
        {
            this.Permission = Permission;
            this.Read = Read;
            this.Write = Write;
            this.Create = Create;
            this.Delete = Delete;
        }
        public Permission Permission;
        public bool Read;
        public bool Write;
        public bool Create;
        public bool Delete;
    }
    public static class AccessStatic
    {
        public static ILogger Logger = Program.Host.Services.GetService<ILoggerFactory>().CreateLogger(typeof(AccessStatic).FullName);
        public static Access[] ReadUserAccessFromDatabase(this Database db, Guid UserID)
        {
            SqlCommand sCmd = db.NewConnection.CreateCommand();
            try
            {
                List<Access> access = new List<Access>();
                sCmd.Parameters.AddWithValue("@USERID@", UserID);
                sCmd.CommandText = @"
                    SELECT P.Permission, PM.[Read], PM.[Write], PM.[Create], PM.[Delete] FROM [Security.Groups.Membership] GM
                    JOIN [Security.Groups] G ON GM.GroupID = G.GroupID
                    JOIN [Security.Permissions.Membership] PM ON GM.GroupID = PM.GroupID
                    JOIN [Security.Permissions] P ON PM.PermissionID = P.PermissionID
                    WHERE GM.UserID = @USERID@
                    AND G.[Enabled] = 1 AND P.[Enabled] = 1
                ";
                SqlDataReader sRead = sCmd.ExecuteReader();
                if (sRead.HasRows)
                {
                    while (sRead.Read())
                    {
                        try
                        {
                            access.Add(
                                new(
                                    Enum.Parse<Permission>((string)sRead["Permission"]),
                                    (bool)sRead["Read"],
                                    (bool)sRead["Write"],
                                    (bool)sRead["Create"],
                                    (bool)sRead["Delete"]
                                )
                            );
                        }
                        catch (Exception e)
                        {
                            Logger.LogWarning(e, (string)sRead["Permission"] + ", is not a defined permission.");
                        }
                    }
                };
                return access.ToArray();
            }
            finally
            {
                sCmd.Connection.Close();
            }
        }
    }
}