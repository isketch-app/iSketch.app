using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using iSketch.app.Data;
using iSketch.app.Services;
using System.Collections.Generic;
using System.Web;
using System;
using System.Data.SqlClient;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.IO;
using System.Net.Mail;

namespace iSketch.app.OpenID
{
    public static class OpenID
    {
        public static List<idP> GetIDPs(Database db, bool includeDisabled = false)
        {
            SqlCommand cmd = db.NewConnection.CreateCommand();
            cmd.CommandText = "SELECT IdpID FROM [Security.OpenID] ";
            if (!includeDisabled)
            {
                cmd.CommandText += "WHERE Enabled = 1 ";
            }
            cmd.CommandText += "ORDER BY DisplayOrder";
            List<Guid> IdpIDs = new();
            SqlDataReader rdr = cmd.ExecuteReader();
            try
            {
                while (rdr.Read())
                {
                    IdpIDs.Add(rdr.GetGuid(0));
                }
            }
            finally
            {
                cmd.Connection.Close();
            }
            List<idP> IDPs = new(); 
            foreach(Guid IdpID in IdpIDs)
            {
                IDPs.Add(GetIDP(db, IdpID));
            }
            return IDPs;
        }
        public static idP GetIDP(Database db, Guid IdpID)
        {
            idP idp;
            SqlCommand cmd = db.NewConnection.CreateCommand();
            cmd.Parameters.AddWithValue("@IDPID@", IdpID);
            cmd.CommandText = 
            "SELECT " +
            "IdpID, " +
            "DisplayName, " +
            "Enabled, " +
            "ClientID, " +
            "ClientSecret, " +
            "ExtraScopes, " +
            "[Endpoint.Authorization], " +
            "[Endpoint.Token], " +
            "[Endpoint.Logout], " +
            "[Claims.Email] " +
            "FROM [Security.OpenID] WHERE IdpID = @IDPID@";
            SqlDataReader rdr = cmd.ExecuteReader();
            try
            {
                rdr.Read();
                idp = new();
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
            finally
            {
                cmd.Connection.Close();
            }
            return idp;
        }
        public static TokenHandleResult HandleIdpIdToken(Session session, idP idP, JWT JWT)
        {
            SqlCommand cmd = session.db.NewConnection.CreateCommand();
            try
            {
                cmd.Parameters.AddWithValue("@IDPID@", idP.IdpID);
                cmd.Parameters.AddWithValue("@SUBJECT@", JWT.Payload["sub"].ToString());
                cmd.CommandText = "SELECT UserID FROM [Security.Users] WHERE [OpenID.IdpID] = @IDPID@ AND [OpenID.Subject] = @SUBJECT@";
                SqlDataReader rdr = cmd.ExecuteReader();
                try
                {
                    if (rdr.HasRows)
                    {
                        if (session.UserID == Guid.Empty)
                        {
                            rdr.Read();
                            Guid UserID = rdr.GetGuid(0);
                            rdr.Close();
                            UserTools.Logon(session, UserID);
                            HandleJwtClaims(session, idP, JWT);
                            return TokenHandleResult.Success;
                        }
                        else
                        {
                            return TokenHandleResult.SubjectAlreadyBoundToAnotherAccount;
                        }
                    }
                }
                finally
                {
                    rdr.Close();
                }
                if (session.UserID == Guid.Empty)
                {
                    Guid newUserID = UserTools.CreateUser(session.db);
                    UserTools.Logon(session, newUserID);
                }
                cmd.Parameters.AddWithValue("@USERID@", session.UserID);
                cmd.CommandText = "UPDATE [Security.Users] SET [OpenID.IdpID] = @IDPID@, [OpenID.Subject] = @SUBJECT@ WHERE UserID = @USERID@";
                int affected = cmd.ExecuteNonQuery();
                if (affected != 1)
                {
                    return TokenHandleResult.FailedToBindToCurrentUserAccount;
                }
                HandleJwtClaims(session, idP, JWT);
                return TokenHandleResult.Success;
            }
            finally
            {
                cmd.Connection.Close();
            }
        }
        public enum TokenHandleResult
        {
            Success,
            SubjectAlreadyBoundToAnotherAccount,
            FailedToBindToCurrentUserAccount
        }
        private static bool HandleJwtClaims(Session Session, idP idP, JWT JWT)
        {
            if (
                idP.ClaimsEmail != null &&
                idP.ClaimsEmail != "" &&
                JWT.Payload.TryGetValue(idP.ClaimsEmail, out object oClaimEmail) &&
                MailAddress.TryCreate(oClaimEmail.ToString(), out MailAddress mailAddress)
            ) {
                UserTools.SetUserEmail(Session.db, Session.UserID, mailAddress);
            }
            return true;
        }
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
    public static class Endpoints
    {
        public static async Task Login(HttpContext con)
        {
            Session session = con.InitializeSession();
            idP idP = OpenID.GetIDP(session.db, Guid.Parse(con.Request.RouteValues["IdpID"].ToString()));
            if (idP == null)
            {
                con.Response.Redirect("/_Error/OpenID/idp-does-not-exist");
                return;
            }
            if (!con.Request.Query.ContainsKey("code") || con.Request.Query["code"] == "")
            {
                con.Response.Redirect("/_Error/OpenID/code-missing");
                return;
            }
            string code = con.Request.Query["code"];
            HttpClient hc = new();
            HttpRequestMessage msg = new();
            msg.RequestUri = new(idP.EndpointToken);
            msg.Method = HttpMethod.Post;
            if (idP.ClientSecret != null)
            {
                byte[] secret = Encoding.Default.GetBytes(HttpUtility.UrlEncode(idP.ClientID) + ":" + idP.ClientSecret);
                msg.Headers.Authorization = new("Basic", Convert.ToBase64String(secret));
            }
            FormUrlEncodedContent form = new(new Dictionary<string, string>() {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", idP.GetRedirectURI(session) },
                { "client_id", idP.ClientID }
            });
            msg.Content = new StreamContent(await form.ReadAsStreamAsync());
            msg.Content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            HttpResponseMessage hResponse = await hc.SendAsync(msg);
            try
            {
                hResponse.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                con.Response.Redirect("/_Error/OpenID/token-endpoint-bad-status-response?msg=" + e.Message);
                return;
            }
            Stream sResponse = await hResponse.Content.ReadAsStreamAsync();
            Dictionary<string, object> jResposne;
            try
            {
                jResposne = await JsonSerializer.DeserializeAsync<Dictionary<string, object>>(sResponse);
            }
            catch
            {
                con.Response.Redirect("/_Error/OpenID/jwt-deserialize-error");
                return;
            }
            if (!jResposne.TryGetValue("id_token", out object idToken))
            {
                sResponse.Position = 0;
                con.Response.Redirect("/_Error/OpenID/jwt-missing?idp_response=" + HttpUtility.UrlEncode(await new StreamReader(sResponse).ReadToEndAsync()));
                return;
            }
            JWT JWT;
            try
            {
                JWT = new(idToken.ToString());
            }
            catch
            {
                con.Response.Redirect("/_Error/OpenID/jwt-invalid");
                return;
            }
            OpenID.TokenHandleResult result = OpenID.HandleIdpIdToken(session, idP, JWT);
            if (result != OpenID.TokenHandleResult.Success)
            {

                con.Response.Redirect("/_Error/OpenID/" + result.ToString());
                return;
            }
            con.Response.Redirect("/");
            await Task.CompletedTask;
        }
    }
}
