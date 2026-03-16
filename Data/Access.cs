using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static iSketch.app.Classes.Static.Access;
using static iSketch.app.Classes.Static.Database;

namespace iSketch.app.Data;

public static class Access
{
    public static async Task<Permission[]> GetPermissionsFromUserID(Guid UserID)
    {
        return await ExecuteReader(
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
                List<Permission> access = new();
                while (await rdr.ReadAsync())
                {
                    access.Add(Enum.Parse<Permission>((string)rdr["Permission"]));
                }
                return access.ToArray();
            }
        );
    }
}