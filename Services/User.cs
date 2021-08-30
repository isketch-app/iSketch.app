﻿using iSketch.app.Data;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

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
            Permissions = Database.Connection.ReadPermissionsFromDatabase(Session.UserID);
        }
        public void ReloadUserData()
        {
            SqlCommand cmd = Database.Connection.CreateCommand();
            cmd.Parameters.AddWithValue("@USERID@", Session.UserID);
            cmd.CommandText = "SELECT [UserName], [Settings.DarkMode] FROM [Security.Users] WHERE UserID = @USERID@";
            SqlDataReader rdr = cmd.ExecuteReader();
            if (!rdr.HasRows)
            {
                rdr.Close();
                return;
            }
            rdr.Read();
            UserName = rdr.GetString(0);
            rdr.Close();
        }
        public bool Logon(string UserName, string Password = null)
        {
            if (Password == "") Password = null;
            try
            {
                SqlCommand cmd = Database.Connection.CreateCommand();
                cmd.Parameters.AddWithValue("@USERNAME@", UserName);
                cmd.CommandText = "SELECT UserID, Password, PasswordSalt FROM [Security.Users] WHERE UserName = @USERNAME@";
                SqlDataReader rdr = cmd.ExecuteReader();
                if (!rdr.HasRows)
                {
                    rdr.Close();
                    return false;
                }
                rdr.Read();
                Guid UserID = rdr.GetGuid(0);
                if (!rdr.IsDBNull(1))
                {
                    if (Password == null)
                    {
                        rdr.Close();
                        return false;
                    }
                    byte[] dbHash = new byte[128];
                    byte[] dbSalt = new byte[128];
                    rdr.GetBytes(1, 0, dbHash, 0, 128);
                    rdr.GetBytes(2, 0, dbSalt, 0, 128);
                    rdr.Close();
                    Task<PassHashResult> PHQTask = PHQ.GenerateHash(new() { Pass = Password, Salt = dbSalt });
                    PHQTask.Wait();
                    string hash1 = Convert.ToBase64String(PHQTask.Result.Hash);
                    string hash2 = Convert.ToBase64String(dbHash);
                    if (hash1 != hash2) return false;
                }
                else
                {
                    rdr.Close();
                }
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@USERID@", UserID);
                cmd.Parameters.AddWithValue("@SESSIONID@", Session.SessionID);
                cmd.CommandText = "UPDATE [Security.Sessions] SET [UserID] = @USERID@ WHERE SessionID = @SESSIONID@";
                int affected = cmd.ExecuteNonQuery();
                if (affected != 1) return false;
                Session.RegisterSession();
                Init();
                EHS.OnLoginLogoutStatusChanged();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool Logoff()
        {
            try
            {
                SqlCommand cmd = Database.Connection.CreateCommand();
                cmd.Parameters.AddWithValue("@SESSIONID@", Session.SessionID);
                cmd.CommandText = "UPDATE [Security.Sessions] SET UserID = NULL WHERE SessionID = @SESSIONID@";
                int affected = cmd.ExecuteNonQuery();
                if (affected != 1) return false;
                Session.RegisterSession();
                Init();
                EHS.OnLoginLogoutStatusChanged();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool ChangePassword(string NewPassword = null)
        {
            return UserTools.ChangePassword(Database, PHQ, Session.UserID, NewPassword);
        }
    }
    public static class UserTools
    {
        public static bool ChangePassword(Database Database, PassHashQueue PHQ, Guid UserID, string NewPassword = null)
        {
            if (NewPassword == "") NewPassword = null;
            try
            {
                SqlCommand cmd = Database.Connection.CreateCommand();
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
        }
        public static Guid CreateUser(Database Database, string UserName)
        {
            try
            {
                Guid UserID = Guid.NewGuid();
                SqlCommand cmd = Database.Connection.CreateCommand();
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
        }
        public static Guid GetUserID(Database Database, string UserName)
        {
            try
            {
                SqlCommand cmd = Database.Connection.CreateCommand();
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
        }
        public static bool IsValidUserIDString(string UserName)
        {
            int maxLength = 25;
            if (UserName.Length > maxLength) return false;
            string regexPolicy = "[a-zA-Z0-9~!@#$%^&*()_+{}|:\"<>?`\\-=[\\]\\\\;',./]";
            return Regex.IsMatch(UserName, regexPolicy);
        }
    }
    public class UserSettings
    {
        public bool DarkMode;
    }
}