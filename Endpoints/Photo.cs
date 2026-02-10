using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using static iSketch.app.Classes.Photo;
using static iSketch.app.Data.Photo;
using System;

namespace iSketch.app.Endpoints {

    public static class Photo
    {
        public static RequestDelegate Endpoint = new(async (context) =>
        {
            if (!Classes.Photo.Endpoints.TryGetValue(context.Request.RouteValues["TableAndRow"].ToString(), out TableAndRow tar))
            {
                await context.Response.WriteAsync("Invalid Photo Location.");
                return;
            }
            byte[] rawPhoto = await GetPhoto(tar, Guid.Parse((string)context.Request.RouteValues["RowID"]));
            if (rawPhoto == null)
            {
                await context.Response.WriteAsync("Photo not found.");
                return;
            }
            IImageFormat format = Image.DetectFormat(rawPhoto);
            if (format != null) context.Response.ContentType = format.DefaultMimeType;
            if (context.Request.Query.Keys.Contains("download"))
            {
                context.Response.Headers.Append("Content-Disposition", "attachment");
            }
            if (context.Request.Query.Keys.Contains("no-cache"))
            {
                context.Response.Headers.Append("Cache-Control", "no-cache");
            }
            else
            {
                context.Response.Headers.Append("Cache-Control", "public, max-age=2592000, immutable");
            }
            await context.Response.Body.WriteAsync(rawPhoto, 0, rawPhoto.Length);
        });
    }
}