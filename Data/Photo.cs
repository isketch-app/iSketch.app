using System;
using System.Threading.Tasks;
using static iSketch.app.Classes.Static.Database;
using static iSketch.app.Classes.Static.Photo;

namespace iSketch.app.Data;

public static class Photo
{
    public static async Task<byte[]> GetPhoto(string Endpoint, Guid ID)
    {
        if (!Classes.Static.Photo.Endpoints.ContainsKey(Endpoint)) return null;
        PhotoTable table = Classes.Static.Photo.Endpoints[Endpoint];
        return await ExecuteScalar<byte[]>(
            CommandText: $@"
                SELECT {table.PhotoColumnName}
                FROM {table.TableName}
                WHERE {table.GuidColumnName} = @ID@
            ",
            Parameters: [
                new("@ID@", ID)
            ]
        );
    }
}