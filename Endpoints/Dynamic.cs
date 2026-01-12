using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using static iSketch.app.Classes.Dynamic;

namespace iSketch.app.Endpoints;

public static class Dynamic
{
    public static async Task Endpoint(HttpContext con)
    {
        string file = con.Request.RouteValues["File"].ToString();
        string ext = con.Request.RouteValues["Ext"].ToString();
        string path = Path.Combine("./wwwroot/dynamic/", file + '.' + ext);
        con.Response.Headers.Append("Service-Worker-Allowed", "/");
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