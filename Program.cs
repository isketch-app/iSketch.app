using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace iSketch.app
{
    public static class Program
    {
        public static IHost Host;
        public static ILogger DefaultLogger;
        public static ILoggerFactory LoggerFactory;
        public static void Main(string[] args)
        {
            Host = CreateHostBuilder(args).Build();
            LoggerFactory = Host.Services.GetService<ILoggerFactory>();
            DefaultLogger = LoggerFactory.CreateLogger("iSketch.app");
            DefaultLogger.LogInformation("Starting iSketch.app...");
            Host.Run();
        }
        public static IHostBuilder CreateHostBuilder(string[] args) => Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        });
    }
}