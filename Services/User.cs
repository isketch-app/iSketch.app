using System;
using Microsoft.Data.SqlClient;
using iSketch.app.Classes.Access;
using iSketch.app.Classes.User;
using iSketch.app.Classes;

namespace iSketch.app.Services {
    public class User
    {
        public Session Session;
        public Permission[] Permissions;
        public string UserName;
        public Guid ProfilePictureID;
        private PassHashQueue PHQ;
        private EventHookScoped EHS;
        public User(Session Session, PassHashQueue PHQ, EventHookScoped EHS)
        {
            this.Session = Session;
            this.PHQ = PHQ;
            this.EHS = EHS;
            Init();
        }
        public void Init()
        {
            ReloadUserData();
        }
        public void ReloadUserData()
        {
            Permissions = AccessHelpers.ReadUserPermissionsFromDatabase(Session.UserID);
            UserName = null;
            ProfilePictureID = Guid.Empty;
            SqlCommand cmd = Database.NewConnection.CreateCommand();
            try
            {
                cmd.Parameters.AddWithValue("@USERID@", Session.UserID);
                cmd.CommandText = "SELECT UserName, ProfilePictureID, [Settings.DarkMode] FROM [Security.Users] WHERE UserID = @USERID@";
                SqlDataReader rdr = cmd.ExecuteReader();
                if (!rdr.HasRows)
                {
                    return;
                }
                rdr.Read();
                UserName = rdr.GetString(0);
                if (!rdr.IsDBNull(1)) ProfilePictureID = rdr.GetGuid(1);
            }
            finally
            {
                cmd.Connection.Close();
            }
        }
        public bool Logon(Guid UserID)
        {
            bool success = UserHelpers.Logon(Session, UserID);
            Init();
            EHS.OnLoginLogoutStatusChanged();
            return success;
        }
        public bool Logon(string UserName, string Password)
        {
            Guid UserID = UserHelpers.GetUserID(UserName);
            if (UserHelpers.TestPassword(PHQ, UserID, Password))
            {
                return Logon(UserID);
            }
            else
            {
                return false;
            }
        }
        public bool Logoff()
        {
            bool success = UserHelpers.Logoff(Session);
            Init();
            EHS.OnLoginLogoutStatusChanged();
            return success;
        }
        public bool ChangePassword(string NewPassword = null)
        {
            return UserHelpers.ChangePassword(PHQ, Session.UserID, NewPassword);
        }
        public bool TestPassword(string Password)
        {
            return UserHelpers.TestPassword(PHQ, Session.UserID, Password);
        }
        public bool SetProperty(UserProperties Property, string Value)
        {
            return UserHelpers.SetUserProperty(Session.UserID, Property, Value);
        }
        public bool SetProperty(UserProperties Property, Guid Value)
        {
            return UserHelpers.SetUserProperty(Session.UserID, Property, Value);
        }
        public object GetProperty(UserProperties Property)
        {
            return UserHelpers.GetUserProperty(Session.UserID, Property);
        }
    }
}