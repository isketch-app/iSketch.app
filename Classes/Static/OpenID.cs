using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using iSketch.app.Services;
using static iSketch.app.Data.OpenID;

namespace iSketch.app.Classes.Static;

public static class OpenID
{
    public static async Task<TokenHandleResult> HandleIdpIdToken(Session session, IDP idP, JWT JWT)
    {
        string subject = JWT.Payload["sub"].ToString();
        var UserID = await GetUserIDFromIdpAndSubject(idP.IdpID, subject);
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
        int affected = await SetIdpAndSubject(session.UserID, idP.IdpID, subject);
        if (affected != 1)
        {
            return TokenHandleResult.FailedToBindToCurrentUserAccount;
        }
        HandleJwtClaims(session, idP, JWT);
        return TokenHandleResult.Success;
    }
    private static bool HandleJwtClaims(Session Session, IDP idP, JWT JWT)
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
    public class IDP
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
    public class IDPs : List<IDP> { }
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