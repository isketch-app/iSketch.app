using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace iSketch.app.Data
{
    public static class Dynamic
    {
        public static async Task Delegate(HttpContext con)
        {
            
            await con.Response.WriteAsync("Test");
        }
    }
}