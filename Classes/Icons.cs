using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace iSketch.app.Classes;

public static class Icons
{
    private static List<Entry> List;
    private static Dictionary<string, int> Table = new();
    private class Entry
    {
        public string Name {get; set;}
        public int CodePoint {get; set;}
    }
    public static string Get(string Name)
    {
        if (List == null)
        {
            var file = File.OpenRead(@"./Data/icons.json");
            List = JsonSerializer.Deserialize<List<Entry>>(file, new JsonSerializerOptions() {
                PropertyNameCaseInsensitive = true
            });
            file.Close();
            foreach (var item in List)
            {
                if (Table.ContainsKey(item.Name)) continue;
                Table.Add(item.Name, item.CodePoint);
            }
        }
        if (!Table.ContainsKey(Name)) return Get("question_mark");
        return char.ConvertFromUtf32(Table[Name]);
    }
}