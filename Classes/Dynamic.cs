using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace iSketch.app.Classes
{
    public static class Dynamic
    {
        public static Dictionary<string, string> Replacements = new Dictionary<string, string>()
        {
            { "$VERSION$", Helpers.Version },
            { "$COMMIT$", Helpers.MiniCommit },
            { "$HASH$", Helpers.MiniHash },
            { "$STATICJSON$", GetStaticJSON() }
        };
        public static Dictionary<string, string> MIME = new Dictionary<string, string>()
        {
            { "js", "text/javascript" },
            { "json", "application/json" }
        };
        public static string GetStaticJSON()
        {
            if (!Directory.Exists("./wwwroot/static")) return "[]";
            List<string> files = Directory.EnumerateFiles("./wwwroot/static", "*", SearchOption.AllDirectories).ToList();
            files.FindAll((file) =>
            {
                if (file.EndsWith(".br")) return true;
                if (file.EndsWith(".gz")) return true;
                return false;
            }).ForEach((file) =>
            {
                files.Remove(file);
            });
            for (int i = 0; files.Count > i; i++)
            {
                string newPath = Path.GetRelativePath("./wwwroot/", files[i]);
                newPath = newPath.Replace('\\', '/');
                files[i] = '/' + newPath;
            }
            files.Add("/iSketch.app.styles.css");
            files.Add("/_framework/blazor.server.js");
            return JsonSerializer.Serialize(files);
        }
    }
}