using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace iSketch.app.Classes;

public static class Access
{
    public enum Permission
    {
        Administrator, //Main administrator page.
        Words //Words administrator page.
    }
    public static ILogger Logger = Program.Host.Services.GetService<ILoggerFactory>().CreateLogger(typeof(Access).FullName);
    public static async Task<Permission[]> ReadUserPermissionsFromDatabase(Guid UserID)
    {
        List<Permission> access = new List<Permission>();
        await Database.ExecuteReader(
            CommandText: @"
                SELECT P.Permission FROM [Security.Users/Groups] UG
                JOIN [Security.Groups] G ON UG.GroupID = G.GroupID
                JOIN [Security.Groups/Permissions] GP ON UG.GroupID = GP.GroupID
                JOIN [Security.Permissions] P ON GP.PermissionID = P.PermissionID
                WHERE UG.UserID = @USERID@
            ",
            Parameters: [
                new("@USERID@", UserID)
            ],
            Reader: async (rdr) =>
            {
                if (rdr.HasRows)
                {
                    while (await rdr.ReadAsync())
                    {
                        try
                        {
                            access.Add(Enum.Parse<Permission>((string)rdr["Permission"]));
                        }
                        catch (Exception e)
                        {
                            Logger.LogWarning(e, (string)rdr["Permission"] + ", is not a defined permission.");
                        }
                    }
                }
            }
        );
        return access.ToArray();
    }
}