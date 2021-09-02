using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using iSketch.app.Data;
using iSketch.app.Services;
using System.Collections.Generic;
using System.Web;
using System;
using System.Data.SqlClient;

namespace iSketch.app.OpenID
{
    public static class OpenID
    {
        public static List<idP> GetIDPs(Session session, bool includeDisabled = false)
        {
            SqlCommand cmd = session.db.Connection.CreateCommand();
            cmd.CommandText = "SELECT IdpID, DisplayName, DisplayIcon, Enabled, ClientID, AuthorizationEndpoint FROM [Security.OpenID]";
            if (!includeDisabled)
            {
                cmd.CommandText += " WHERE Enabled = 1";
            }
            cmd.CommandText += " ORDER BY DisplayOrder";
            SqlDataReader rdr = cmd.ExecuteReader();
            List<idP> list = new();
            while (rdr.Read())
            {
                list.Add(new()
                {
                    IdpID = rdr.GetGuid(0),
                    DisplayName = rdr.GetString(1),
                    DisplayIcon = (byte[])rdr.GetValue(2),
                    Enabled = rdr.GetBoolean(3),
                    ClientID = rdr.GetString(4),
                    AuthorizationEndpointPrefix = rdr.GetString(5)
                });
            }
            return list;
        }
    }
    public class idP
    {
        public Guid IdpID;
        public string DisplayName;
        public byte[] DisplayIcon;
        public bool Enabled;
        public string AuthorizationEndpointPrefix;
        public string ClientID;
        public string GetRedirectURI(Session session) 
        {
            return session.BaseURI.ToString() + "_OpenID/" + IdpID.ToString() + "/Login";
        }
        public string GetRequestURI(Session session)
        {
            return 
            AuthorizationEndpointPrefix +
            "?response_type=code&scope=openid&client_id=" +
            HttpUtility.UrlEncode(ClientID) +
            "&redirect_uri=" +
            HttpUtility.UrlEncode(GetRedirectURI(session));
        }
    }
    public static class Endpoints
    {
        public static async Task Login(HttpContext con)
        {
            Session session = con.InitializeSession();



            await con.Response.WriteAsync(con.Request.RouteValues["IdpID"].ToString() + "\r\n");
            await con.Response.WriteAsync(con.Request.Query["code"]);

        }
    }

}
