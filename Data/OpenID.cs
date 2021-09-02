using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using iSketch.app.Data;
using iSketch.app.Services;
using System.Collections.Generic;
using System.Web;
using System;

namespace iSketch.app.OpenID
{
    public class OpenID
    {
        Database db;
        public OpenID(Database db)
        {
            this.db = db;
        }
        public List<idP> GetIDPs()
        {
            return null;
        }
    }
    public class idP
    {
        public string AuthorizationEndpointPrefix;
        public string ClientID;
        public string RedirectURI;
        public string AuthorizationEndpoint { 
            get
            {
                return 
                AuthorizationEndpointPrefix +
                "?response_type=code&scope=openid&client_id=" +
                HttpUtility.UrlEncode(ClientID) +
                "&redirect_uri=" +
                HttpUtility.UrlEncode(RedirectURI);
            }
        }
    }
    public static class Endpoints
    {
        public static async Task Login(HttpContext con)
        {
            Session session = con.InitializeSession();
            Database db = (Database)con.RequestServices.GetService(typeof(Database));



            await con.Response.WriteAsync(con.Request.RouteValues["IdpID"].ToString() + "\r\n");
            await con.Response.WriteAsync(con.Request.Query["code"]);

        }
    }

}
