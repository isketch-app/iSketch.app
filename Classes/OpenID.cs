using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Web;
using iSketch.app.Services;

namespace iSketch.app.Classes;

public static class OpenID
{
    public static List<idP> GetIDPs(bool includeDisabled = false)
    {
        List<Guid> IdpIDs = new();
        Database.ExecuteReader(
            CommandText: "SELECT IdpID FROM [Security.OpenID] ",
            Command: (cmd) =>
            {
                if (!includeDisabled)
                {
                    cmd.CommandText += "WHERE Enabled = 1 ";
                }
                cmd.CommandText += "ORDER BY DisplayOrder";
            },
            Reader: (rdr) =>
            {
                while (rdr.Read())
                {
                    IdpIDs.Add(rdr.GetGuid(0));
                }
            }
        );
        List<idP> IDPs = new();
        foreach (Guid IdpID in IdpIDs)
        {
            IDPs.Add(GetIDP(IdpID));
        }
        return IDPs;
    }
    public static idP GetIDP(Guid IdpID)
    {
        idP idp = new();
        Database.ExecuteReader(
            CommandText: @"
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
                FROM [Security.OpenID] WHERE IdpID = @IDPID@
            ",
            Parameters: [
                new("@IDPID@", IdpID)
            ],
            Reader: (rdr) =>
            {
                try
                {
                    rdr.Read();
                    idp.IdpID = rdr.GetGuid(0);
                    idp.DisplayName = rdr.GetString(1);
                    idp.Enabled = rdr.GetBoolean(2);
                    idp.ClientID = rdr.GetString(3);
                    if (!rdr.IsDBNull(4)) idp.ClientSecret = rdr.GetString(4);
                    if (!rdr.IsDBNull(5)) idp.ExtraScopes = rdr.GetString(5);
                    idp.EndpointAuthorization = rdr.GetString(6);
                    idp.EndpointToken = rdr.GetString(7);
                    if (!rdr.IsDBNull(8)) idp.EndpointLogout = rdr.GetString(8);
                    if (!rdr.IsDBNull(9)) idp.ClaimsEmail = rdr.GetString(9);
                }
                catch (Exception)
                {
                    idp = null;
                }
            }
        );
        return idp;
    }
    public static TokenHandleResult HandleIdpIdToken(Session session, idP idP, JWT JWT)
    {
        string subject = JWT.Payload["sub"].ToString();
        var UserID = Database.ExecuteScalar<Guid>(
            CommandText: @"
                SELECT UserID 
                FROM [Security.Users] 
                WHERE [OpenID.IdpID] = @IDPID@
                AND [OpenID.Subject] = @SUBJECT@
            ",
            Parameters: [
                new("@IDPID@", idP.IdpID),
                new("@SUBJECT@", subject)
            ]
        );
        if (UserID != Guid.Empty)
        {
            if (session.UserID == Guid.Empty)
            {
                User.Logon(session, UserID);
                HandleJwtClaims(session, idP, JWT);
                return TokenHandleResult.Success;
            }
            else
            {
                return TokenHandleResult.SubjectAlreadyBoundToAnotherAccount;
            }
        }
        if (session.UserID == Guid.Empty)
        {
            Guid newUserID = User.CreateUser();
            User.Logon(session, newUserID);
        }
        int affected = Database.ExecuteNonQuery(
            CommandText: @"
                UPDATE [Security.Users] SET
                [OpenID.IdpID] = @IDPID@,
                [OpenID.Subject] = @SUBJECT@
                WHERE UserID = @USERID@
            ",
            Parameters: [
                new("@IDPID@", idP.IdpID),
                new("@SUBJECT@", subject),
                new("@USERID@", session.UserID)
            ]
        );
        if (affected != 1)
        {
            return TokenHandleResult.FailedToBindToCurrentUserAccount;
        }
        HandleJwtClaims(session, idP, JWT);
        return TokenHandleResult.Success;
    }
    private static bool HandleJwtClaims(Session Session, idP idP, JWT JWT)
    {
        if (
            idP.ClaimsEmail != null &&
            idP.ClaimsEmail != "" &&
            JWT.Payload.TryGetValue(idP.ClaimsEmail, out object oClaimEmail) &&
            MailAddress.TryCreate(oClaimEmail.ToString(), out MailAddress mailAddress)
        )
        {
            User.SetUserEmail(Session.UserID, mailAddress);
        }
        return true;
    }
    public class idP
    {
        public Guid IdpID;
        public string DisplayName;
        public byte[] DisplayIcon;
        public bool Enabled;
        public string ClientID;
        public string ClientSecret;
        public string ExtraScopes;
        public string EndpointAuthorization;
        public string EndpointToken;
        public string EndpointLogout;
        public string ClaimsUserName;
        public string ClaimsEmail;
        public string ClaimsUserPhoto;
        public string GetRedirectURI(Session session)
        {
            return session.BaseURI.ToString() + "_OpenID/" + IdpID.ToString() + "/Login";
        }
        public string GetRequestURI(Session session)
        {
            string URI =
            EndpointAuthorization +
            "?response_type=code";
            if (ExtraScopes != null)
            {
                URI += "&scope=openid" + HttpUtility.UrlEncode(" " + ExtraScopes);
            }
            else
            {
                URI += "&scope=openid";
            }
            URI += "&client_id=" +
            HttpUtility.UrlEncode(ClientID) +
            "&redirect_uri=" +
            HttpUtility.UrlEncode(GetRedirectURI(session));
            return URI;
        }
    }
    public class JWT
    {
        public Dictionary<string, object> Header;
        public Dictionary<string, object> Payload;
        public byte[] Signature;
        public JWT(string RawToken)
        {
            try
            {
                string rJsonHeader;
                string rJsonPayload;
                string[] splitToken = RawToken.Split('.');
                rJsonHeader = Encoding.Default.GetString(ConvertFromB64Url(splitToken[0]));
                rJsonPayload = Encoding.Default.GetString(ConvertFromB64Url(splitToken[1]));
                Header = JsonSerializer.Deserialize<Dictionary<string, object>>(rJsonHeader);
                Payload = JsonSerializer.Deserialize<Dictionary<string, object>>(rJsonPayload);
                Signature = ConvertFromB64Url(splitToken[2]);
            }
            catch (Exception)
            {
                throw new Exception("Failed to de-serialize the JsonWebToken.");
            }
        }
        public static byte[] ConvertFromB64Url(string str)
        {
            str = str.Replace('-', '+');
            str = str.Replace('_', '/');
            int padding = str.Length % 4;
            if (padding == 3) str += "=";
            if (padding == 2) str += "==";
            return Convert.FromBase64String(str);
        }
    }
    public enum TokenHandleResult
    {
        Unknown,
        Success,
        SubjectAlreadyBoundToAnotherAccount,
        FailedToBindToCurrentUserAccount
    }
}