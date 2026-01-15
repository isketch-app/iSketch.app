using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace iSketch.app.Classes;

public static class Helpers
{
    public static readonly Assembly Assembly = Assembly.GetExecutingAssembly();
    public static readonly string Version = Assembly.GetName().Version.ToString();
    public static readonly string Commit = new Func<string>(() => {
        using StreamReader sr = new(Assembly.GetManifestResourceStream("iSketch.app.Properties.commit"));
        return sr.ReadToEnd();
    })();
    public static readonly string MiniCommit = Commit.Substring(0, 7);
    public static readonly string Hash = new Func<string>(() => {
        using var sha1 = SHA1.Create();
        using var file = File.OpenRead(Assembly.Location);
        return Convert.ToHexString(sha1.ComputeHash(file)).ToLower();
    })();
    public static readonly string MiniHash = Hash.Substring(0, 7);
}