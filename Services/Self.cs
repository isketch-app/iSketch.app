using System;
using System.Threading.Tasks;
using iSketch.app.Classes;
using Microsoft.Data.SqlClient;

namespace iSketch.app.Services {
    public class Self
    {
        public Session Session;
        public Access.Permission[] Permissions;
        public string UserName;
        public Guid ProfilePictureID;
        private EventHookScoped EHS;
        public Self(Session Session, EventHookScoped EHS)
        {
            this.Session = Session;
            this.EHS = EHS;
            Init().Wait();
        }
        public async Task Init()
        {
            await ReloadUserData();
        }
        public async Task ReloadUserData()
        {
            Permissions = await Access.ReadUserPermissionsFromDatabase(Session.UserID);
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
        public async Task<bool> Logon(Guid UserID)
        {
            bool success = User.Logon(Session, UserID);
            await Init();
            EHS.OnLoginLogoutStatusChanged();
            return success;
        }
        public async Task<bool> Logon(string UserName, string Password)
        {
            Guid UserID = User.GetUserID(UserName);
            if (User.TestPassword(UserID, Password))
            {
                return await Logon(UserID);
            }
            else
            {
                return false;
            }
        }
        public async Task<bool> Logoff()
        {
            bool success = User.Logoff(Session);
            await Init();
            EHS.OnLoginLogoutStatusChanged();
            return success;
        }
        public bool ChangePassword(string NewPassword = null)
        {
            return User.ChangePassword(Session.UserID, NewPassword);
        }
        public bool TestPassword(string Password)
        {
            return User.TestPassword(Session.UserID, Password);
        }
        public bool SetProperty(User.UserProperties Property, string Value)
        {
            return User.SetUserProperty(Session.UserID, Property, Value);
        }
        public bool SetProperty(User.UserProperties Property, Guid Value)
        {
            return User.SetUserProperty(Session.UserID, Property, Value);
        }
        public object GetProperty(User.UserProperties Property)
        {
            return User.GetUserProperty(Session.UserID, Property);
        }
    }
}