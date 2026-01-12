using System.Collections.Generic;

namespace iSketch.app.Classes;

public static class Photo
{
    public static Dictionary<string, TableAndRow> Endpoints = new()
    {
        {
            "idP-icon",
            new()
            {
                TableName = "[Security.OpenID]",
                PhotoColumnName = "DisplayIcon",
                GuidColumnName = "IdpID"
            }
        }, {
            "profile-picture",
            new()
            {
                TableName = "[System.ProfilePictures]",
                PhotoColumnName = "Picture",
                GuidColumnName = "ProfilePictureID"
            }
        }
    };
    public class TableAndRow
    {
        public string PhotoColumnName;
        public string GuidColumnName;
        public string TableName;
    }
}
