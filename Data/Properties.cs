using iSketch.app.Services;
using System.Data.SqlClient;

namespace iSketch.app.Data
{
    public static class Properties
    {
        public static string GetProperty(this Database db, string Property)
        {
            SqlCommand cmd = new SqlCommand("SELECT Value FROM [System.Properties] WHERE Property = @PROP", db.NewConnection);
            try
            {
                cmd.Parameters.AddWithValue("@PROP", Property);
                return (string)cmd.ExecuteScalar();
            }
            finally
            {
                cmd.Connection.Close();
            }
        }
        public static void SetProperty(this Database db, string Property, string Value)
        {
            SqlCommand cmd = db.NewConnection.CreateCommand();
            try
            {
                cmd.Parameters.AddWithValue("@PROP", Property);
                cmd.Parameters.AddWithValue("@VAL", Value);
                if (Value == null)
                {
                    cmd.CommandText = "DELETE FROM [System.Properties] WHERE Property = @PROP";
                    cmd.ExecuteNonQuery();
                    return;
                }
                ClearNull(db);
                if (db.GetProperty(Property) == null)
                {
                    cmd.CommandText = "INSERT INTO [System.Properties] (Property, Value) VALUES(@PROP, @VAL)";
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    cmd.CommandText = "UPDATE [System.Properties] SET Value = @VAL WHERE Property = @PROP";
                    cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                cmd.Connection.Close();
            }
        }
        private static void ClearNull(Database db)
        {
            SqlConnection con = db.NewConnection;
            try
            {
                new SqlCommand("DELETE FROM [System.Properties] WHERE Value IS NULL", con).ExecuteNonQuery();
            }
            finally
            {
                con.Close();
            }
        }
    }
}
