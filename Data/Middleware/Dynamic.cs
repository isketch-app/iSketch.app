using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.IO;

namespace iSketch.app.Data.Middleware
{
    public static class Dynamic
    {
        private static Dictionary<string, string> Replacements = new Dictionary<string, string>()
        {
            { "$VERSION$", Assembly.GetExecutingAssembly().GetName().Version.ToString() }
        };
        private static Dictionary<string, string> MIME = new Dictionary<string, string>()
        {
            { "js", "text/javascript" },
            { "json", "application/json" }
        };
        public static async Task Delegate(HttpContext con)
        {
            string file = con.Request.RouteValues["File"].ToString();
            string ext = con.Request.RouteValues["Ext"].ToString();
            string path = Path.Combine("./wwwroot/dynamic/", file + '.' + ext);
            con.Response.Headers.Add("Service-Worker-Allowed", "/");
            if (MIME.ContainsKey(ext))
            {
                con.Response.ContentType = MIME[ext];
            }
            if (File.Exists(path))
            {
                string data = await File.ReadAllTextAsync(path);
                foreach (KeyValuePair<string, string> replacement in Replacements)
                {
                    data = data.Replace(replacement.Key, replacement.Value);
                }
                await con.Response.WriteAsync(data);
            }
            else
            {
                await con.Response.WriteAsync("Nothing here!");
            }
        }
    }
}