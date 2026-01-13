using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace iSketch.app.Classes;

public static class Icons
{
    private static ILogger Logger = Program.LoggerFactory.CreateLogger(typeof(Icons).FullName);
    private static Dictionary<string, int> Table = null;
    private const string IconsDataFile = @"./Data/icons.json";
    public static string Get(string Name)
    {
        if (Table == null)
        {
            Logger.LogInformation($"Loading icons via: {IconsDataFile}...");
            var file = File.OpenRead(@"./Data/icons.json");
            Table = JsonSerializer.Deserialize<Dictionary<string, int>>(
                file
            );
            file.Close();
            Logger.LogInformation("Icons loaded.");
        }
        if (Name == null || !Table.ContainsKey(Name)) return Get("question_mark");
        return char.ConvertFromUtf32(Table[Name]);
    }
}