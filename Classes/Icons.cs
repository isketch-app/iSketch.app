using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace iSketch.app.Classes;

public static class Icons
{
    private static ILogger Logger = Program.LoggerFactory.CreateLogger(typeof(Icons).FullName);
    private static Dictionary<string, int> Table = null;
    public static string Get(string Name)
    {
        if (Table == null)
        {
            Logger.LogInformation($"Indexing icons...");
            Table = JsonSerializer.Deserialize<Dictionary<string, int>>(
                Assembly
                .GetExecutingAssembly()
                .GetManifestResourceStream("iSketch.app.Resources.icons.json")
            );
            Logger.LogInformation("Icons indexed.");
        }
        if (Name == null || !Table.ContainsKey(Name)) return Get("question_mark");
        return char.ConvertFromUtf32(Table[Name]);
    }
}