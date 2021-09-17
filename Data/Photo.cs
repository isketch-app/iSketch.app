using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using iSketch.app.Services;
using System.Data.SqlClient;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using System.IO;

namespace iSketch.app.Data.Photo
{
    public static class Photo
    {
        public static RequestDelegate Endpoint = new(async (context) => {
            if (!TableAndRows.TryGetValue(context.Request.RouteValues["TableAndRow"].ToString(), out TableAndRow tar))
            {
                await context.Response.WriteAsync("Invalid Photo Location.");
                return;
            }
            Database db = (Database)context.RequestServices.GetService(typeof(Database));
            SqlCommand cmd = db.NewConnection.CreateCommand();
            cmd.Parameters.AddWithValue("@ROWID@", context.Request.RouteValues["RowID"]);
            cmd.CommandText = 
            "SELECT " + tar.PhotoColumnName + " " +
            "FROM " + tar.TableName + " " +
            "WHERE " + tar.GuidColumnName + " = @ROWID@";
            object rawPhoto = cmd.ExecuteScalar();
            cmd.Connection.Close();
            if (rawPhoto.GetType() == typeof(System.DBNull)) return;
            IImageFormat format = Image.DetectFormat((byte[])rawPhoto);
            if (format != null) context.Response.ContentType = format.DefaultMimeType;
            await context.Response.Body.WriteAsync((byte[])rawPhoto, 0, ((byte[])rawPhoto).Length);
        });
        public static Dictionary<string, TableAndRow> TableAndRows = new()
        {
            {
                "idP-icon",
                new()
                {
                    TableName = "[Security.OpenID]",
                    PhotoColumnName = "DisplayIcon",
                    GuidColumnName = "IdpID"
                }
            }
        };
    }
    public class TableAndRow
    {
        public string PhotoColumnName;
        public string GuidColumnName;
        public string TableName;
    }
}
