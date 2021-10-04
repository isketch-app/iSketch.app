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
            SqlCommand cmd = Database.NewConnection.CreateCommand();
            try
            {
                cmd.Parameters.AddWithValue("@USERID@", Session.UserID);
                cmd.CommandText = "SELECT [UserName], [Settings.DarkMode] FROM [Security.Users] WHERE UserID = @USERID@";
                SqlDataReader rdr = cmd.ExecuteReader();
                if (!rdr.HasRows)
                {
                    return;
                }
                rdr.Read();
                UserName = rdr.GetString(0);
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
        public bool Logon(string UserName, string Password = null)
        {
            if (Password == "") Password = null;
            SqlCommand cmd = Database.NewConnection.CreateCommand();
            try
            {
                cmd.Parameters.AddWithValue("@USERNAME@", UserName);
                cmd.CommandText = "SELECT UserID, Password, PasswordSalt, [OpenID.IdpID] FROM [Security.Users] WHERE UserName = @USERNAME@";
                SqlDataReader rdr = cmd.ExecuteReader();
                if (!rdr.HasRows)
                {
                    return false;
                }
                rdr.Read();
                Guid UserID = rdr.GetGuid(0);
                bool IsPasswordNull = rdr.IsDBNull(1);
                bool IsIdpIDNull = rdr.IsDBNull(3);
                if (IsPasswordNull && !IsIdpIDNull)
                {
                    return false;
                }
                if (!IsPasswordNull)
                {
                    if (Password == null)
                    {
                        return false;
                    }
                    byte[] dbHash = new byte[128];
                    byte[] dbSalt = new byte[128];
                    rdr.GetBytes(1, 0, dbHash, 0, 128);
                    rdr.GetBytes(2, 0, dbSalt, 0, 128);
                    Task<PassHashResult> PHQTask = PHQ.GenerateHash(new() { Pass = Password, Salt = dbSalt });
                    PHQTask.Wait();
                    string hash1 = Convert.ToBase64String(PHQTask.Result.Hash);
                    string hash2 = Convert.ToBase64String(dbHash);
                    if (hash1 != hash2) return false;
                }
                return Logon(UserID);
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
    }
    public static class UserTools
    {
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
                session.RegisterSession();
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
                session.RegisterSession();
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
                if (NewPassword == null)
                {
                    cmd.Parameters.AddWithValue("@PASSWORD@", null);
                    cmd.Parameters.AddWithValue("@PASSWORDSALT@", null);
                }
                else
                {
                    Task<PassHashResult> PHQTask = PHQ.GenerateHash(new() { Pass = NewPassword });
                    PHQTask.Wait();
                    cmd.Parameters.AddWithValue("@PASSWORD@", PHQTask.Result.Hash);
                    cmd.Parameters.AddWithValue("@PASSWORDSALT@", PHQTask.Result.Salt);
                }
                cmd.Parameters.AddWithValue("@USERID@", UserID);
                cmd.CommandText = "UPDATE [Security.Users] SET Password = @PASSWORD@, PasswordSalt = @PASSWORDSALT@ WHERE UserID = @USERID@";
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
        public static bool SetUserProperties(Database Database, Guid UserID, UserProperties Setting, string Value)
        {
            SqlCommand cmd = Database.NewConnection.CreateCommand();
            try
            {
                cmd.Parameters.AddWithValue("@USERID@", UserID);
                cmd.Parameters.AddWithValue("@VALUE@", Value);
                cmd.CommandText = "UPDATE [Security.Users] SET [" + Setting.ToString().Replace('_', '.') + "] = @VALUE@ WHERE UserID = @USERID@";
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
        public static string GetUserProperties(Database Database, Guid UserID, UserProperties Setting)
        {
            SqlCommand cmd = Database.NewConnection.CreateCommand();
            try
            {
                cmd.Parameters.AddWithValue("@USERID@", UserID);
                cmd.CommandText = "SELECT [" + Setting.ToString().Replace('_', '.') + "] FROM [Security.Users] WHERE UserID = @USERID@";
                object result = cmd.ExecuteScalar();
                if (result.GetType() == typeof(DBNull)) return null;
                return (string)result;
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
            int maxLength = 25;
            if (UserName.Length > maxLength) return false;
            string regexPolicy = "[a-zA-Z0-9~!@#$%^&*()_+{}|:\"<>?`\\-=[\\]\\\\;',./]";
            return Regex.IsMatch(UserName, regexPolicy);
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
                string sEmail = GetUserProperties(Database, UserID, UserProperties.Email);
                if (sEmail != null && MailAddress.TryCreate(sEmail, out MailAddress dbEmail))
                {
                    if (Email.Address == dbEmail.Address) return true;
                }
                SetUserProperties(Database, UserID, UserProperties.EmailVerified, "false");
                SetUserProperties(Database, UserID, UserProperties.Email, Email.Address);
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
                SetUserProperties(Database, UserID, UserProperties.UserName, UserName);
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
        Settings_DarkMode
    }
    public enum UserAuthMethods
    {
        None = 0x0,
        Password = 0x1,
        OpenID = 0x2
    }
}