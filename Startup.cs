using iSketch.app.Data.Middleware;
using iSketch.app.Data.Photo;
using iSketch.app.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace iSketch.app
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks();
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSingleton<EventHookGlobal>();
            services.AddScoped<EventHookScoped>();
            services.AddSingleton<Database>();
            services.AddScoped<Session>();
            services.AddScoped<User>();
            services.AddScoped<Header>();
            services.AddSingleton<Jobs>();
            services.AddSingleton(new PassHashQueue());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseHttpsRedirection();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/_Error");
            }
            app.UseWhen(
                (con) =>
                {
                    if (con.Request.Path == "/") return false;
                    return Directory.Exists(Path.Join(env.WebRootPath, con.Request.Path));
                },
                (app) =>
                {
                    app.UseDirectoryBrowser();
                }
            );
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/dynamic/{File}.{Ext}", Dynamic.Delegate);
                endpoints.MapGet("/_OpenID/{IdpID:guid}/Login", OpenID.Endpoints.Login);
                endpoints.MapGet("/_Photo/{TableAndRow}/{RowID:guid}", Photo.Endpoint);
                endpoints.MapHealthChecks("/_Health");
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
            app.UseStaticFiles();
        }
    }
}