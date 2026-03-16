using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using static iSketch.app.Data.Photo;
using System;
using System.Threading.Tasks;

namespace iSketch.app.Endpoints {

    public static class Photo
    {
        public static async Task Endpoint(HttpContext con)
        {
            byte[] rawPhoto = await GetPhoto(
                con.Request.RouteValues["TableAndRow"].ToString(),
                Guid.Parse((string)con.Request.RouteValues["RowID"])
            );
            if (rawPhoto == null)
            {
                await con.Response.WriteAsync("Photo not found.");
                return;
            }
            IImageFormat format = Image.DetectFormat(rawPhoto);
            if (format != null) con.Response.ContentType = format.DefaultMimeType;
            if (con.Request.Query.Keys.Contains("download"))
            {
                con.Response.Headers.Append("Content-Disposition", "attachment");
            }
            if (con.Request.Query.Keys.Contains("no-cache"))
            {
                con.Response.Headers.Append("Cache-Control", "no-cache");
            }
            else
            {
                con.Response.Headers.Append("Cache-Control", "public, max-age=2592000, immutable");
            }
            await con.Response.Body.WriteAsync(rawPhoto, 0, rawPhoto.Length);
        }
    }
}