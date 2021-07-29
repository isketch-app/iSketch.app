using System;
using Microsoft.AspNetCore.Http;
using iSketch.app.Services;

namespace iSketch.app.Data
{
    public static class SessionHandler
    {
        public static TimeSpan CookieMaxAge = TimeSpan.FromDays(90);
        private const string CookieName = "IS-SessionID";
        public static Session InitializeSession(this HttpContext con)
        {
            Session session = new Session();
            if (!con.Request.Cookies.ContainsKey(CookieName))
            {
                session.SessionID = Guid.NewGuid();
                con.Response.Cookies.Append(CookieName, session.SessionID.ToString(), new CookieOptions() { 
                    HttpOnly = true,
                    IsEssential = true,
                    MaxAge = CookieMaxAge,
                    SameSite = SameSiteMode.Strict,
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
            return session;
        }
    }
}