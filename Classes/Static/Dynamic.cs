using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace iSketch.app.Classes.Static;

public static class Dynamic
{
    public static readonly Dictionary<string, string> MIME = new() {
            { "js", "text/javascript" },
            { "json", "application/json" }
        };
    private static readonly List<string> StaticFiles = [
        "/dynamic/sw.manifest.json",
            "/iSketch.app.styles.css",
            "/_framework/blazor.server.js",
            "/_Static/Offline"
    ];
    public static readonly Dictionary<string, string> Replacements = new() {
            { "$VERSION$", Helpers.Version },
            { "$COMMIT$", Helpers.MiniCommit },
            { "$HASH$", Helpers.MiniHash },
            { "$STATICJSON$", GetStaticJSON() }
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
        files.AddRange(StaticFiles);
        return JsonSerializer.Serialize(files);
    }
}