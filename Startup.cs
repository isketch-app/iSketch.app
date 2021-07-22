using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using iSketch.app.Data;

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
            services.AddAuthentication(opts => {
                opts.AddScheme("iSketch.app", builder => {
                    builder.DisplayName = "iSketch.app";
                    builder.HandlerType = typeof(Data.Authentication.iSketchAuthenticationHandler);
                    builder.Build();
                });
                opts.DefaultScheme = "iSketch.app";
            });
            services.AddAuthorization(opts => {
                opts.AddPolicy("Player", opts => {
                    opts.RequireAuthenticatedUser();
                    opts.Build();
                });
                opts.DefaultPolicy = opts.GetPolicy("Player");
            });
            services.AddHealthChecks();
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSingleton(new Services.EventHook());
            services.AddSingleton(new Services.Database());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseAuthentication();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/dynamic/{File}.{Ext}", Dynamic.Delegate);
                endpoints.MapHealthChecks("/_Health");
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
            app.UseStaticFiles();
        }
    }
}
