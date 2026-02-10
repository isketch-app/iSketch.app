using System;
using System.Threading.Tasks;
using static iSketch.app.Classes.Database;
using static iSketch.app.Classes.Photo;

namespace iSketch.app.Data;

public static class Photo
{
    public static async Task<byte[]> GetPhoto(TableAndRow TAR, Guid ID)
    {
        return await ExecuteScalar<byte[]>(
            CommandText: $@"
                SELECT {TAR.PhotoColumnName}
                FROM {TAR.TableName}
                WHERE {TAR.GuidColumnName} = @ROWID@
            ",
            Parameters: [
                new("@ROWID@", ID)
            ]
        );
    }
}