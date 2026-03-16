using System.Collections.Generic;

namespace iSketch.app.Classes.Static;

public static class Photo
{
    public static PhotoEndpoint Endpoints = new()
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
    public class PhotoTable
    {
        public string PhotoColumnName;
        public string GuidColumnName;
        public string TableName;
    }
    public class PhotoEndpoint : Dictionary<string, PhotoTable> { }
}