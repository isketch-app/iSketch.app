using iSketch.app.Classes;
using iSketch.app.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using static iSketch.app.Classes.Photo;

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
            SqlCommand cmd = Database.NewConnection.CreateCommand();
            cmd.Parameters.AddWithValue("@ROWID@", context.Request.RouteValues["RowID"]);
            cmd.CommandText =
            "SELECT " + tar.PhotoColumnName + " " +
            "FROM " + tar.TableName + " " +
            "WHERE " + tar.GuidColumnName + " = @ROWID@";
            object rawPhoto = cmd.ExecuteScalar();
            cmd.Connection.Close();
            if (rawPhoto == null || rawPhoto.GetType() == typeof(System.DBNull)) return;
            IImageFormat format = Image.DetectFormat((byte[])rawPhoto);
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
            await context.Response.Body.WriteAsync((byte[])rawPhoto, 0, ((byte[])rawPhoto).Length);
        });
    }
}