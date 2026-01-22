using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Data.SqlClient;

namespace iSketch.app.Classes
{
    public static class Properties
    {
        public static string GetProperty(string Property)
        {
            SqlCommand cmd = new SqlCommand("SELECT Value FROM [System.Properties] WHERE Property = @PROP", Database.NewConnection);
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
        public static void SetProperty(string Property, string Value)
        {
            SqlCommand cmd = Database.NewConnection.CreateCommand();
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
                ClearNull();
                if (GetProperty(Property) == null)
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
        private static void ClearNull()
        {
            SqlConnection con = Database.NewConnection;
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
