using iSketch.app.Data;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net.Mail;

namespace iSketch.app.Services
{
    public class User
    {
        public Session Session;
        public Permissions Permissions = new();
        public string UserName;
        public Guid ProfilePictureID;
        private Database Database;
        private PassHashQueue PHQ;
        private EventHookScoped EHS;
        public User(Session Session, Database Database, PassHashQueue PHQ, EventHookScoped EHS)
        {
            this.Database = Database;
            this.Session = Session;
            this.PHQ = PHQ;
            this.EHS = EHS;
            Init();
        }
        public void Init()
        {
            ReloadPermissionsFromDatabase();
            ReloadUserData();
        }
        public void ReloadPermissionsFromDatabase()
        {
            Permissions = Database.ReadPermissionsFromDatabase(Session.UserID);
        }
        public void ReloadUserData()
        {
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
            bool success = UserTools.Logon(Session, UserID);
            Init();
            EHS.OnLoginLogoutStatusChanged();
            return success;
        }
        public bool Logon(string UserName, string Password)
        {
            Guid UserID = UserTools.GetUserID(Database, UserName);
            if (UserTools.TestPassword(Database, PHQ, UserID, Password))
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
            bool success = UserTools.Logoff(Session);
            Init();
            EHS.OnLoginLogoutStatusChanged();
            return success;
        }
        public bool ChangePassword(string NewPassword = null)
        {
            return UserTools.ChangePassword(Database, PHQ, Session.UserID, NewPassword);
        }
        public bool TestPassword(string Password)
        {
            return UserTools.TestPassword(Database, PHQ, Session.UserID, Password);
        }
        public bool SetProperty(UserProperties Property, string Value)
        {
            return UserTools.SetUserProperty(Database, Session.UserID, Property, Value);
        }
        public bool SetProperty(UserProperties Property, Guid Value)
        {
            return UserTools.SetUserProperty(Database, Session.UserID, Property, Value);
        }
        public object GetProperty(UserProperties Property)
        {
            return UserTools.GetUserProperty(Database, Session.UserID, Property);
        }
    }
    public static class UserTools
    {
        private static string UserNameRegex = "^[a-zA-Z0-9~!@#$%^&*()_+{}|:\"<>?`\\-=[\\]\\\\;',./]{3,25}$";
        public static bool Logon(Session session, Guid UserID)
        {
            SqlCommand cmd = session.db.NewConnection.CreateCommand();
            try
            {
                cmd.Parameters.AddWithValue("@USERID@", UserID);
                cmd.Parameters.AddWithValue("@SESSIONID@", session.SessionID);
                cmd.CommandText = "UPDATE [Security.Sessions] SET [UserID] = @USERID@ WHERE SessionID = @SESSIONID@";
                int affected = cmd.ExecuteNonQuery();
                if (affected != 1) return false;
                cmd.CommandText = "UPDATE [Security.Users] SET [LastLogonTime] = SYSDATETIME() WHERE UserID = @USERID@";
                cmd.ExecuteNonQuery();
                session.UpdateInDatabase();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                cmd.Connection.Close();
            }
        }
        public static bool Logoff(Session session)
        {
            SqlCommand cmd = session.db.NewConnection.CreateCommand();
            try
            {
                cmd.Parameters.AddWithValue("@SESSIONID@", session.SessionID);
                cmd.CommandText = "UPDATE [Security.Sessions] SET UserID = NULL WHERE SessionID = @SESSIONID@";
                int affected = cmd.ExecuteNonQuery();
                if (affected != 1) return false;
                session.UpdateInDatabase();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                cmd.Connection.Close();
            }
        }
        public static bool TestPassword(Database Database, PassHashQueue PHQ, Guid UserID, string Password)
        {
            if (Password == "") Password = null;
            SqlCommand cmd = Database.NewConnection.CreateCommand();
            try
            {
                cmd.Parameters.AddWithValue("@USERID@", UserID);
                cmd.CommandText = "SELECT Password, PasswordSalt, [OpenID.IdpID] FROM [Security.Users] WHERE UserID = @USERID@";
                SqlDataReader rdr = cmd.ExecuteReader();
                if (!rdr.HasRows) return false;
                rdr.Read();
                bool IsPasswordNull = rdr.IsDBNull(0);
                bool IsIdpIDNull = rdr.IsDBNull(2);
                if (IsPasswordNull && !IsIdpIDNull) return false;
                if (!IsPasswordNull)
                {
                    if (Password == null) return false;
                    byte[] dbHash = new byte[128];
                    byte[] dbSalt = new byte[128];
                    rdr.GetBytes(0, 0, dbHash, 0, 128);
                    rdr.GetBytes(1, 0, dbSalt, 0, 128);
                    Task<PassHashResult> PHQTask = PHQ.GenerateHash(new() { Pass = Password, Salt = dbSalt });
                    PHQTask.Wait();
                    string hash1 = Convert.ToBase64String(PHQTask.Result.Hash);
                    string hash2 = Convert.ToBase64String(dbHash);
                    if (hash1 != hash2) return false;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                cmd.Connection.Close();
            }
        }
        public static bool ChangePassword(Database Database, PassHashQueue PHQ, Guid UserID, string NewPassword = null)
        {
            if (NewPassword == "") NewPassword = null;
            SqlCommand cmd = Database.NewConnection.CreateCommand();
            try
            {
                cmd.Parameters.AddWithValue("@USERID@", UserID);
                if (NewPassword == null)
                {
                    cmd.CommandText = "UPDATE [Security.Users] SET Password = NULL, PasswordSalt = NULL WHERE UserID = @USERID@";
                }
                else
                {
                    Task<PassHashResult> PHQTask = PHQ.GenerateHash(new() { Pass = NewPassword });
                    PHQTask.Wait();
                    cmd.Parameters.AddWithValue("@PASSWORD@", PHQTask.Result.Hash);
                    cmd.Parameters.AddWithValue("@PASSWORDSALT@", PHQTask.Result.Salt);
                    cmd.CommandText = "UPDATE [Security.Users] SET Password = @PASSWORD@, PasswordSalt = @PASSWORDSALT@ WHERE UserID = @USERID@";
                }
                int affected = cmd.ExecuteNonQuery();
                if (affected != 1) return false;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                cmd.Connection.Close();
            }
        }
        public static Guid CreateUser(Database Database, string UserName = null)
        {
            SqlCommand cmd = Database.NewConnection.CreateCommand();
            try
            {
                Guid UserID = Guid.NewGuid();
                if (UserName == null || UserName == "") UserName = UserID.ToString();
                cmd.Parameters.AddWithValue("@USERID@", UserID);
                cmd.Parameters.AddWithValue("@USERNAME@", UserName);
                cmd.CommandText = "INSERT INTO [Security.Users] ([UserID], [UserName]) VALUES (@USERID@, @USERNAME@)";
                int affected = cmd.ExecuteNonQuery();
                if (affected != 1) return Guid.Empty;
                return UserID;
            }
            catch (Exception)
            {
                return Guid.Empty;
            }
            finally
            {
                cmd.Connection.Close();
            }
        }
        public static bool SetUserProperty(Database Database, Guid UserID, UserProperties Property, object Value)
        {
            SqlCommand cmd = Database.NewConnection.CreateCommand();
            try
            {
                if (Value == null) Value = DBNull.Value;
                if (Value.GetType() == typeof(Guid) && (Guid)Value == Guid.Empty) Value = DBNull.Value;
                cmd.Parameters.AddWithValue("@USERID@", UserID);
                cmd.Parameters.AddWithValue("@VALUE@", Value);
                cmd.CommandText = "UPDATE [Security.Users] SET [" + Property.ToString().Replace('_', '.') + "] = @VALUE@ WHERE UserID = @USERID@";
                int affected = cmd.ExecuteNonQuery();
                return (affected == 1);
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                cmd.Connection.Close();
            }
        }
        public static object GetUserProperty(Database Database, Guid UserID, UserProperties Property)
        {
            SqlCommand cmd = Database.NewConnection.CreateCommand();
            try
            {
                cmd.Parameters.AddWithValue("@USERID@", UserID);
                cmd.CommandText = "SELECT [" + Property.ToString().Replace('_', '.') + "] FROM [Security.Users.Splice] WHERE UserID = @USERID@";
                object result = cmd.ExecuteScalar();
                if (result.GetType() == typeof(DBNull)) return null;
                return result;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                cmd.Connection.Close();
            }
        }
        public static Guid GetUserID(Database Database, string UserName)
        {
            if (UserName == null) return Guid.Empty;
            SqlCommand cmd = Database.NewConnection.CreateCommand();
            try
            {
                cmd.Parameters.AddWithValue("@USERNAME@", UserName);
                cmd.CommandText = "SELECT UserID FROM [Security.Users] WHERE UserName = @USERNAME@";
                object oUserID = cmd.ExecuteScalar();
                if (oUserID == null)
                {
                    return Guid.Empty;
                }
                else
                {
                    return (Guid)oUserID;
                }
            }
            catch (Exception)
            {
                return Guid.Empty;
            }
            finally
            {
                cmd.Connection.Close();
            }
        }
        public static bool IsValidUserIDString(string UserName)
        {
            return Regex.IsMatch(UserName, UserNameRegex);
        }
        public static UserAuthMethodsResult GetUserAuthenticationMethods(Database Database, Guid UserID)
        {
            SqlCommand cmd = Database.NewConnection.CreateCommand();
            try
            {
                UserAuthMethodsResult result = new();
                cmd.Parameters.AddWithValue("@USERID@", UserID);
                cmd.CommandText = "SELECT Password, [OpenID.IdpID] FROM [Security.Users] WHERE UserID = @USERID@";
                SqlDataReader rdr = cmd.ExecuteReader();
                rdr.Read();
                if (!rdr.IsDBNull(0))
                {
                    result.Methods |= UserAuthMethods.Password;
                }
                if (!rdr.IsDBNull(1))
                {
                    result.Methods |= UserAuthMethods.OpenID;
                    result.IdpID = rdr.GetGuid(1);
                }
                return result;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                cmd.Connection.Close();
            }
        }
        public static bool SetUserEmail(Database Database, Guid UserID, MailAddress Email)
        {
            try
            {
                string sEmail = (string)GetUserProperty(Database, UserID, UserProperties.Email);
                if (sEmail != null && MailAddress.TryCreate(sEmail, out MailAddress dbEmail))
                {
                    if (Email.Address == dbEmail.Address) return true;
                }
                SetUserProperty(Database, UserID, UserProperties.EmailVerified, "false");
                SetUserProperty(Database, UserID, UserProperties.Email, Email.Address);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static SetUserNameResult SetUserName(Database Database, Guid UserID, string UserName)
        {
            try
            {
                if (!IsValidUserIDString(UserName)) return SetUserNameResult.UserNameInvalid;
                Guid uid = GetUserID(Database, UserName);
                if (uid != Guid.Empty)
                {
                    UserAuthMethodsResult methods = GetUserAuthenticationMethods(Database, uid);
                    if (methods.Methods != UserAuthMethods.None) return SetUserNameResult.UserNameAlreadyTaken;
                }
                SetUserProperty(Database, UserID, UserProperties.UserName, UserName);
                return SetUserNameResult.Success;
            }
            catch
            {
                return SetUserNameResult.UnknownFailure;
            }
        }
    }
    public class UserAuthMethodsResult
    {
        public Guid IdpID;
        public UserAuthMethods Methods;
    }
    public enum SetUserNameResult
    {
        Success,
        UserNameAlreadyTaken,
        UserNameInvalid,
        UnknownFailure
    }
    public enum UserProperties
    {
        UserName,
        Email,
        EmailVerified,
        Settings_DarkMode,
        Biography,
        IdpName,
        CreatedTime,
        LastLogonTime,
        ProfilePictureID
    }
    public enum UserAuthMethods
    {
        None = 0x0,
        Password = 0x1,
        OpenID = 0x2
    }
}