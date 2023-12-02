using System;
using System.Data.SqlClient;
using System.Net;
using System.Net.Sockets;

namespace iSketch.app.Services
{
    public class Session
    {
        public Database db;
        public Guid SessionID = Guid.Empty;
        public Guid UserID = Guid.Empty;
        public IPAddress IPAddress;
        public Uri BaseURI;
        public bool Existing = false;
        public Session(Database db = null)
        {
            this.db = db;
        }
        public bool TrySetExistingSessionID(Guid ExistingSessionID)
        {
            SqlCommand cmd = db.NewConnection.CreateCommand();
            try
            {
                cmd.Parameters.AddWithValue("@SESSID@", ExistingSessionID);
                cmd.CommandText = "SELECT SessionID FROM [Security.Sessions] WHERE SessionID = @SESSID@";
                SqlDataReader reader = cmd.ExecuteReader();
                Existing = reader.HasRows;
                reader.Close();
                if (Existing) SessionID = ExistingSessionID;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return Existing;
        }
        public void RegisterSession()
        {
            SqlCommand cmd = db.NewConnection.CreateCommand();
            try
            {
                cmd.Parameters.AddWithValue("@SESSID@", SessionID);
                cmd.Parameters.AddWithValue("@IPADDR@", IPAddress.ToString());
                if (IPAddress.AddressFamily == AddressFamily.InterNetwork) cmd.Parameters.AddWithValue("@IPVER@", 4);
                if (IPAddress.AddressFamily == AddressFamily.InterNetworkV6) cmd.Parameters.AddWithValue("@IPVER@", 6);
                cmd.CommandText = "SELECT UserID FROM [Security.Sessions] WHERE SessionID = @SESSID@";
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    if (reader.IsDBNull(0))
                    {
                        UserID = Guid.Empty;
                    }
                    else
                    {
                        UserID = reader.GetGuid(0);
                    }
                    cmd.CommandText = "UPDATE [Security.Sessions] SET SessionTime = GETDATE(), SessionIP = @IPADDR@, SessionIPVersion = @IPVER@ WHERE SessionID = @SESSID@";
                }
                else
                {
                    cmd.CommandText = "INSERT INTO [Security.Sessions] (SessionID, SessionIP, SessionIPVersion) VALUES (@SESSID@, @IPADDR@, @IPVER@)";
                }
                reader.Close();
                cmd.ExecuteNonQuery();
            }
            finally
            {
                cmd.Connection.Close();
            }
        }
    }
}