using System;
using Microsoft.AspNetCore.Http;
using iSketch.app.Services;
using Microsoft.Extensions.Primitives;
using System.Net;

namespace iSketch.app.Data
{
    public static class SessionHandler
    {
        public static TimeSpan CookieMaxAge = TimeSpan.FromDays(90);
        private const string CookieName = "IS-SessionID";
        public static Session InitializeSession(this HttpContext con)
        {
            Session session = new Session();
            session.db = (Database)con.RequestServices.GetService(typeof(Database));
            string host;
            string proto;
            if (con.Request.Headers.ContainsKey("X-Forwarded-Host") &&
                con.Request.Headers.TryGetValue("X-Forwarded-Host", out StringValues hostVal))
            {
                host = hostVal;
            }
            else
            {
                host = con.Request.Host.ToString();
            }
            if (con.Request.Headers.ContainsKey("X-Forwarded-Proto") &&
                con.Request.Headers.TryGetValue("X-Forwarded-Proto", out StringValues protoVal))
            {
                proto = protoVal;
            }
            else
            {
                proto = con.Request.Scheme;
            }
            session.BaseURI = new Uri(proto + "://" + host);
            if (con.Request.Headers.ContainsKey("X-Forwarded-For") &&
                con.Request.Headers.TryGetValue("X-Forwarded-For", out StringValues ipVal) &&
                IPAddress.TryParse(ipVal[0].Split(',')[0], out IPAddress xForIP))
            {
                session.IPAddress = xForIP;
            }
            else
            {
                session.IPAddress = con.Connection.RemoteIpAddress;
            }
            if (!con.Request.Cookies.ContainsKey(CookieName))
            {
                session.SessionID = Guid.NewGuid();
                con.Response.Cookies.Append(CookieName, session.SessionID.ToString(), new CookieOptions() { 
                    HttpOnly = true,
                    IsEssential = true,
                    MaxAge = CookieMaxAge,
                    SameSite = SameSiteMode.Lax,
                    Secure = true
                });
            }
            else
            {
                if (
                    con.Request.Cookies.TryGetValue(CookieName, out string cookieValStr) &&
                    Guid.TryParse(cookieValStr, out Guid cookieVal)
                ) {
                    session.SessionID = cookieVal;
                    session.Existing = true;
                }
            }
            session.RegisterSession();
            return session;
        }
    }
}