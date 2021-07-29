using System;
using Microsoft.AspNetCore.Http;

namespace iSketch.app.Data.Middleware
{
    public static class SessionCookieHandler
    {
        public static Guid HandleSessionCookie(this HttpContext context)
        {
            Guid userID = Guid.Empty;
            if (!context.Request.Cookies.ContainsKey("IS-Auth-Token"))
            {
                userID = Guid.NewGuid();
                context.Response.Cookies.Append("IS-Auth-Token", userID.ToString());
            }
            else
            {
                if (
                    context.Request.Cookies.TryGetValue("IS-Auth-Token", out string cookieVal) &&
                    Guid.TryParse(cookieVal, out Guid cookieGuid)
                )
                {
                    userID = cookieGuid;
                }
            }
            return userID;
        }
    }
}