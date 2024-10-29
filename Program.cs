using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace iSketch.app
{
    public static class Program
    {
        public static IHost Host;
        public static void Main(string[] args)
        {
            Host = CreateHostBuilder(args).Build();
            Host.Run();
        }
        public static IHostBuilder CreateHostBuilder(string[] args) => Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        });
    }
}