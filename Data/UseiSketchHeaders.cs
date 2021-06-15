using Microsoft.AspNetCore.Builder;

namespace iSketch.app.Data
{
    public static class iSketchBuilderExtensions
    {
        public static IApplicationBuilder UseiSketchHeaders(this IApplicationBuilder app)
        {
            app = app.Use(async (con, nex) =>
            {
                con.Response.Headers.Add("Service-Worker-Allowed", "/");
                await nex.Invoke();
            });
            return app;
        }
    }
}
