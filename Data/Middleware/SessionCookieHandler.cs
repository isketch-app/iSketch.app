using System;
using Microsoft.AspNetCore.Http;

namespace iSketch.app.Data.Middleware
{
    public static class SessionCookieHandler
    {
        public static TimeSpan CookieMaxAge = TimeSpan.FromDays(90);
        private const string CookieName = "IS-SessionID";
        public static Guid HandleSessionCookie(this HttpContext con)
        {
            Guid sessionID = Guid.Empty;
            if (!con.Request.Cookies.ContainsKey(CookieName))
            {
                sessionID = Guid.NewGuid();
                con.Response.Cookies.Append(CookieName, sessionID.ToString(), new CookieOptions() { 
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
                    sessionID = cookieVal; 
                }
            }
            return sessionID;
        }
    }
}