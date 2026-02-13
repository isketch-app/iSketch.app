using System;
using System.Threading.Tasks;
using static iSketch.app.Classes.Database;

namespace iSketch.app.Data;

public static class OpenID
{
    public static async Task<int> SetIdpAndSubject(Guid UserID, Guid IdpID, string Subject)
    {
        return await ExecuteNonQuery(
            CommandText: @"
                UPDATE [Security.Users] SET
                [OpenID.IdpID] = @IDPID@,
                [OpenID.Subject] = @SUBJECT@
                WHERE UserID = @USERID@
            ",
            Parameters: [
                new("@USERID@", UserID),
                new("@IDPID@", IdpID),
                new("@SUBJECT@", Subject)
            ]
        );
    }
}