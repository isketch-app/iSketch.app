﻿using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using iSketch.app.Data;
using iSketch.app.Services;
using System.Collections.Generic;
using System.Web;
using System;
using System.Data.SqlClient;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;

namespace iSketch.app.OpenID
{
    public static class OpenID
    {
        public static List<idP> GetIDPs(Database db, bool includeDisabled = false)
        {
            SqlCommand cmd = db.Connection.CreateCommand();
            cmd.CommandText = "SELECT IdpID FROM [Security.OpenID] ";
            if (!includeDisabled)
            {
                cmd.CommandText += "WHERE Enabled = 1 ";
            }
            cmd.CommandText += "ORDER BY DisplayOrder";
            SqlDataReader rdr = cmd.ExecuteReader();
            List<Guid> IdpIDs = new();
            try
            {
                while(rdr.Read())
                {
                    IdpIDs.Add(rdr.GetGuid(0));
                }
            }
            finally
            {
                rdr.Close();
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
            SqlCommand cmd = db.Connection.CreateCommand();
            cmd.Parameters.AddWithValue("@IDPID@", IdpID);
            cmd.CommandText = 
            "SELECT " +
            "IdpID, " +
            "DisplayName, " +
            "Enabled, " +
            "ClientID, " +
            "[Endpoint.Authorization], " +
            "[Endpoint.Token], " +
            "[Endpoint.Logout], " +
            "[Claims.UserName], " +
            "[Claims.Email], " +
            "[Claims.UserPhoto] " +
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
                idp.EndpointAuthorization = rdr.GetString(4);
                idp.EndpointToken = rdr.GetString(5);
                if (!rdr.IsDBNull(6)) idp.EndpointLogout = rdr.GetString(6);
                if (!rdr.IsDBNull(7)) idp.ClaimsUserName = rdr.GetString(7);
                if (!rdr.IsDBNull(8)) idp.ClaimsEmail = rdr.GetString(8);
                if (!rdr.IsDBNull(9)) idp.ClaimsUserPhoto = rdr.GetString(9);
            }
            catch (Exception)
            {
                idp = null;
            }
            finally
            {
                rdr.Close();
            }
            return idp;
        }
    }
    public class idP
    {
        public Guid IdpID;
        public string DisplayName;
        public byte[] DisplayIcon;
        public bool Enabled;
        public string ClientID;
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
            return 
            EndpointAuthorization +
            "?response_type=code&scope=openid&client_id=" +
            HttpUtility.UrlEncode(ClientID) +
            "&redirect_uri=" +
            HttpUtility.UrlEncode(GetRedirectURI(session));
        }
    }
    public class Token
    {
        [JsonPropertyName("id_token")]
        public string IDToken { get; set; }
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
                con.Response.Redirect("/error/openid/idp-does-not-exist");
                return;
            }
            if (!con.Request.Query.ContainsKey("code") || con.Request.Query["code"] == "")
            {
                con.Response.Redirect("/error/openid/code-missing");
                return;
            }
            string code = con.Request.Query["code"];

            HttpClient hc = new();
            HttpRequestMessage msg = new();
            msg.RequestUri = new(idP.EndpointToken);
            msg.Method = HttpMethod.Post;
            FormUrlEncodedContent form = new(new Dictionary<string, string>() {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", idP.GetRedirectURI(session) },
                { "client_id", idP.ClientID }
            });
            msg.Content = new StreamContent(await form.ReadAsStreamAsync());
            msg.Content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            HttpResponseMessage response = await hc.SendAsync(msg);
            Token Token = await response.Content.ReadFromJsonAsync<Token>();
            if (Token.IDToken == null)
            {
                con.Response.Redirect("/error/openid/jwt-missing");
                return;
            }
            JWT JWT;
            try
            {
                JWT = new(Token.IDToken);
            }
            catch (Exception)
            {
                con.Response.Redirect("/error/openid/jwt-invalid");
                return;
            }
            
            foreach (KeyValuePair<string, object> claim in JWT.Payload)
            {
                await con.Response.WriteAsync(claim.Key + ": " + claim.Value.ToString() + "\r\n");
            }

            //con.Response.Redirect("/");
            await Task.CompletedTask;
        }
    }

}
