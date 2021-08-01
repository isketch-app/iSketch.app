using System;
using System.Data.SqlClient;

namespace iSketch.app.Services
{
    public class Session
    {
        public Database db;
        public Guid SessionID = Guid.Empty;
        public Guid UserID = Guid.Empty;
        public bool Existing = false;
        public Session(Database db = null)
        {
            this.db = db;
        }
        public void RegisterSession()
        {
            SqlCommand cmd = db.Connection.CreateCommand();
            cmd.Parameters.AddWithValue("@SESSID@", SessionID);
            cmd.CommandText = "SELECT UserID FROM Sessions WHERE SessionID = @SESSID@";
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                if (!reader.IsDBNull(0)) UserID = reader.GetGuid(0);
                cmd.CommandText = "UPDATE Sessions SET SessionTime = GETDATE() WHERE SessionID = @SESSID@";
            }
            else
            {
                cmd.CommandText = "INSERT INTO Sessions (SessionID) VALUES (@SESSID@)";
            }
            reader.Close();
            cmd.ExecuteNonQuery();
        }
    }
}