using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using iSketch.app.Services;

namespace iSketch.app.Classes.Access;

public enum Permission
{
    Administrator, //Main administrator page.
    Words //Words administrator page.
}
public static class PermissionStatic
{
    public static ILogger Logger = Program.Host.Services.GetService<ILoggerFactory>().CreateLogger(typeof(PermissionStatic).FullName);
    public static Permission[] ReadUserPermissionsFromDatabase(this Database db, Guid UserID)
    {
        SqlCommand sCmd = db.NewConnection.CreateCommand();
        try
        {
            List<Permission> access = new List<Permission>();
            sCmd.Parameters.AddWithValue("@USERID@", UserID);
            sCmd.CommandText = @"
                    SELECT P.Permission FROM [Security.Groups.Membership] GM
                    JOIN [Security.Groups] G ON GM.GroupID = G.GroupID
                    JOIN [Security.Permissions.Membership] PM ON GM.GroupID = PM.GroupID
                    JOIN [Security.Permissions] P ON PM.PermissionID = P.PermissionID
                    WHERE GM.UserID = @USERID@
                ";
            SqlDataReader sRead = sCmd.ExecuteReader();
            if (sRead.HasRows)
            {
                while (sRead.Read())
                {
                    try
                    {
                        access.Add(Enum.Parse<Permission>((string)sRead["Permission"]));
                    }
                    catch (Exception e)
                    {
                        Logger.LogWarning(e, (string)sRead["Permission"] + ", is not a defined permission.");
                    }
                }
            }
            ;
            return access.ToArray();
        }
        finally
        {
            sCmd.Connection.Close();
        }
    }
}