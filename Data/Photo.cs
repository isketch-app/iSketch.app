using System;
using System.Threading.Tasks;
using static iSketch.app.Classes.Database;
using static iSketch.app.Classes.Photo;

namespace iSketch.app.Data;

public static class Photo
{
    public static async Task<byte[]> GetPhoto(string Endpoint, Guid ID)
    {
        if (!Classes.Photo.Endpoints.ContainsKey(Endpoint)) return null;
        PhotoTable table = Classes.Photo.Endpoints[Endpoint];
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