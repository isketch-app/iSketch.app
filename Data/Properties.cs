using iSketch.app.Services;
using System.Data.SqlClient;

namespace iSketch.app.Data
{
    public static class Properties
    {
        public static string GetProperty(this Database db, string Property)
        {
            SqlCommand cmd = new SqlCommand("SELECT Value FROM Properties WHERE Property = @PROP", db.Connection);
            cmd.Parameters.AddWithValue("@PROP", Property);
            return (string)cmd.ExecuteScalar();
        }
        public static void SetProperty(this Database db, string Property, string Value)
        {
            if(Value == null)
            {
                SqlCommand cmd = new SqlCommand("DELETE FROM Properties WHERE Property = @PROP", db.Connection);
                cmd.Parameters.AddWithValue("@PROP", Property);
                cmd.ExecuteNonQuery();
                return;
            }
            ClearNull(db);
            if(db.GetProperty(Property) == null)
            {
                SqlCommand cmd = new SqlCommand("INSERT INTO Properties (Property, Value) VALUES(@PROP, @VAL)", db.Connection);
                cmd.Parameters.AddWithValue("@PROP", Property);
                cmd.Parameters.AddWithValue("@VAL", Value);
                cmd.ExecuteNonQuery();
            }
            else
            {
                SqlCommand cmd = new SqlCommand("UPDATE Properties SET Value = @VAL WHERE Property = @PROP", db.Connection);
                cmd.Parameters.AddWithValue("@PROP", Property);
                cmd.Parameters.AddWithValue("@VAL", Value);
                cmd.ExecuteNonQuery();
            }
        }
        private static void ClearNull(Database db)
        {
            new SqlCommand("DELETE FROM Properties WHERE Value IS NULL", db.Connection).ExecuteNonQuery();
        }
    }
}
