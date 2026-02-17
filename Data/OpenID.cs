using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using static iSketch.app.Classes.Database;
using static iSketch.app.Classes.OpenID;

namespace iSketch.app.Data;

public static class OpenID
{
    private static readonly string IdpQuery = @"
        SELECT
        IdpID,
        DisplayName,
        Enabled,
        ClientID,
        ClientSecret,
        ExtraScopes,
        [Endpoint.Authorization],
        [Endpoint.Token],
        [Endpoint.Logout],
        [Claims.Email]
        FROM [Security.OpenID]
    ";
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
    public static async Task<Guid> GetUserIDFromIdpAndSubject(Guid IdpID, string Subject)
    {
        return await ExecuteScalar<Guid>(
            CommandText: @"
                SELECT UserID 
                FROM [Security.Users] 
                WHERE [OpenID.IdpID] = @IDPID@
                AND [OpenID.Subject] = @SUBJECT@
            ",
            Parameters: [
                new("@IDPID@", IdpID),
                new("@SUBJECT@", Subject)
            ]
        );
    }
    public static async Task<IDP> GetIDP(Guid IdpID)
    {
        return await ExecuteReader(
            CommandText: $@"{IdpQuery} WHERE IdpID = @IDPID@",
            Parameters: [
                new("@IDPID@", IdpID)
            ],
            Reader: ReadIDP
        );
    }
    public static async Task<IDPs> GetIDPs(bool IncludeDisabled = false)
    {
        return await ExecuteReader(
            CommandText: $@"{IdpQuery}",
            Command: (cmd) =>
            {
                if (!IncludeDisabled)
                {
                    cmd.CommandText += " WHERE Enabled = 1";
                }
            },
            Reader: ReadIDPs
        );
    }
    private static async Task<IDPs> ReadIDPs(SqlDataReader Reader)
    {
        IDPs IDPs = new();
        while(await Reader.ReadAsync()) {
            IDP idp = new() {
                IdpID = (Guid)Reader["IdpID"],
                DisplayName = (string)Reader["DisplayName"],
                Enabled = (bool)Reader["Enabled"],
                ClientID = (string)Reader["ClientID"],
                EndpointAuthorization = (string)Reader["Endpoint.Authorization"],
                EndpointToken = (string)Reader["Endpoint.Token"]
            };
            if (Reader["ClientSecret"].GetType() != typeof(DBNull)) idp.ClientSecret = (string)Reader["ClientSecret"];
            if (Reader["ExtraScopes"].GetType() != typeof(DBNull)) idp.ExtraScopes = (string)Reader["ExtraScopes"];
            if (Reader["Endpoint.Logout"].GetType() != typeof(DBNull)) idp.EndpointLogout = (string)Reader["Endpoint.Logout"];
            if (Reader["Claims.Email"].GetType() != typeof(DBNull)) idp.ClaimsEmail = (string)Reader["Claims.Email"];
            IDPs.Add(idp);
        }
        return IDPs;
    }
    private static async Task<IDP> ReadIDP(SqlDataReader Reader)
    {
        return (await ReadIDPs(Reader))[0];
    }
}