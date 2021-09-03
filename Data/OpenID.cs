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
    public static class Endpoints
    {
        public static async Task Login(HttpContext con)
        {
            Session session = con.InitializeSession();

            idP asdf = OpenID.GetIDP(session.db, Guid.Parse(con.Request.RouteValues["IdpID"].ToString()));

            await con.Response.WriteAsync(asdf.GetRequestURI(session) + "\r\n\r\n");
            await con.Response.WriteAsync(con.Request.Query["code"]);

        }
    }

}
