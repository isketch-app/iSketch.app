using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace iSketch.app.Classes.Access;

public enum Permission
{
    Administrator, //Main administrator page.
    Words //Words administrator page.
}
public static class AccessHelpers
{
    public static ILogger Logger = Program.Host.Services.GetService<ILoggerFactory>().CreateLogger(typeof(AccessHelpers).FullName);
    public static Permission[] ReadUserPermissionsFromDatabase(Guid UserID)
    {
        SqlCommand sCmd = Database.NewConnection.CreateCommand();
        try
        {
            List<Permission> access = new List<Permission>();
            sCmd.Parameters.AddWithValue("@USERID@", UserID);
            sCmd.CommandText = @"
                    SELECT P.Permission FROM [Security.Users/Groups] UG
                    JOIN [Security.Groups] G ON UG.GroupID = G.GroupID
                    JOIN [Security.Groups/Permissions] GP ON UG.GroupID = GP.GroupID
                    JOIN [Security.Permissions] P ON GP.PermissionID = P.PermissionID
                    WHERE UG.UserID = @USERID@
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