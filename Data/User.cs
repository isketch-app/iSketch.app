using System;
using System.Threading.Tasks;
using static iSketch.app.Classes.Static.User;
using static iSketch.app.Classes.Static.Database;

namespace iSketch.app.Data;

public static class User
{
    public static async Task<UserProperties> GetProperties(Guid UserID, UserProperty[] Properties)
    {
        return await ExecuteReader(
            CommandText: $"SELECT {Properties.ToSelectString()} FROM [Security.Users] WHERE UserID = @USERID@",
            Parameters: [
                new("@USERID@", UserID)
            ],
            Reader: async (rdr) =>
            {
                await rdr.ReadAsync();
                UserProperties up = new();
                foreach (var prop in Properties)
                {
                    up.Add(prop, rdr[prop.ToSqlString()]);
                }
                return up;
            }
        );
    }
    private static string ToSelectString(this UserProperty[] Properties)
    {
        var first = true;
        var select = string.Empty;
        foreach (var prop in Properties)
        {
            if (first) 
            {
                first = false;
            }
            else
            {
                select += ", ";
            }
            select += $"[{prop.ToSqlString()}]";
        }
        return select;
    }
    private static string ToSqlString(this UserProperty Property)
    {
        return Property.ToString().Replace('_', '.');
    }
}