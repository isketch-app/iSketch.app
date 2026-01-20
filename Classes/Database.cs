using System;
using Microsoft.Data.SqlClient;

namespace iSketch.app.Classes;

public static class Database
{
    public readonly static string ConnectionString = new SqlConnectionStringBuilder() {
        DataSource = Environment.GetEnvironmentVariable("IS_SQL_ServerHost"),
        UserID = Environment.GetEnvironmentVariable("IS_SQL_User"),
        Password = Environment.GetEnvironmentVariable("IS_SQL_Pass"),
        InitialCatalog = Environment.GetEnvironmentVariable("IS_SQL_DatabaseName"),
        TrustServerCertificate = true
    }.ToString();
    public static SqlConnection NewConnection
    {
        get
        {
            SqlConnection con = new(ConnectionString);
            con.Open();
            return con;
        }
    }
    public static void ExecuteReader(Action<SqlCommand> Command, Action<SqlDataReader> Reader)
    {
        SqlConnection connection = NewConnection;
        SqlCommand command = connection.CreateCommand();
        Command.Invoke(command);
        SqlDataReader reader = command.ExecuteReader();
        Reader.Invoke(reader);
        reader.Close();
        command.Dispose();
        connection.Close();
    }
    public static object ExecuteScaler(Action<SqlCommand> Command)
    {
        SqlConnection connection = NewConnection;
        SqlCommand command = connection.CreateCommand();
        Command.Invoke(command);
        object result =  command.ExecuteScalar();
        command.Dispose();
        connection.Close();
        return result;
    }
    public static void ExecuteNonQuery(Action<SqlCommand> Command)
    {
        SqlConnection connection = NewConnection;
        SqlCommand command = connection.CreateCommand();
        Command.Invoke(command);
        command.ExecuteNonQuery();
        command.Dispose();
        connection.Close();
    }
}