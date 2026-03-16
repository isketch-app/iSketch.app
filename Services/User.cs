using System;
using System.Threading.Tasks;
using iSketch.app.Classes.Static;
using Microsoft.Data.SqlClient;
using static iSketch.app.Data.Access;
using static iSketch.app.Classes.Static.Access;

namespace iSketch.app.Services {
    public class User
    {
        public Session Session;
        public Permission[] Permissions;
        public string UserName;
        public Guid ProfilePictureID;
        private EventHookScoped EHS;
        public User(Session Session, EventHookScoped EHS)
        {
            this.Session = Session;
            this.EHS = EHS;
            Task.Run(async () => await ReloadUserData()).Wait();
        }
        public async Task ReloadUserData()
        {
            Permissions = await GetPermissionsFromUserID(Session.UserID);
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
            bool success = Classes.Static.User.Logon(Session, UserID);
            await ReloadUserData();
            EHS.OnLoginLogoutStatusChanged();
            return success;
        }
        public async Task<bool> Logon(string UserName, string Password)
        {
            Guid UserID = Classes.Static.User.GetUserID(UserName);
            if (Classes.Static.User.TestPassword(UserID, Password))
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
            bool success = Classes.Static.User.Logoff(Session);
            await ReloadUserData();
            EHS.OnLoginLogoutStatusChanged();
            return success;
        }
        public bool ChangePassword(string NewPassword = null)
        {
            return Classes.Static.User.ChangePassword(Session.UserID, NewPassword);
        }
        public bool TestPassword(string Password)
        {
            return Classes.Static.User.TestPassword(Session.UserID, Password);
        }
        public bool SetProperty(Classes.Static.User.UserProperty Property, string Value)
        {
            return Classes.Static.User.SetUserProperty(Session.UserID, Property, Value);
        }
        public bool SetProperty(Classes.Static.User.UserProperty Property, Guid Value)
        {
            return Classes.Static.User.SetUserProperty(Session.UserID, Property, Value);
        }
        public object GetProperty(Classes.Static.User.UserProperty Property)
        {
            return Classes.Static.User.GetUserProperty(Session.UserID, Property);
        }
    }
}