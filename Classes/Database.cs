using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
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
    public static async Task ExecuteReader(
        string CommandText,
        [Optional]
        SqlParameter[] Parameters,
        [Optional]
        Action<SqlCommand> Command,
        Func<SqlDataReader, Task> Reader
    ) {
        using SqlConnection connection = NewConnection;
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = CommandText;
        if (Parameters != null) command.Parameters.AddRange(Parameters);
        if (Command != null) Command(command);
        using SqlDataReader reader = await command.ExecuteReaderAsync(); //THIS IS AN ISSUE FOR SOME REASON, MORE TESTING NEEDED (Try a simple call somewhere in startup to test if it hangs there.)
        await Reader(reader);
    }
    public static async Task<T> ExecuteScalar<T>(
        string CommandText,
        [Optional]
        SqlParameter[] Parameters,
        [Optional]
        Action<SqlCommand> Command
    ) {
        using SqlConnection connection = NewConnection;
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = CommandText;
        if (Parameters != null) command.Parameters.AddRange(Parameters);
        if (Command != null) Command(command);
        var result = await command.ExecuteScalarAsync();
        if (result == null)
        {
            return default;
        } 
        else
        {
            return (T)result;
        }
        
    }
    public static async Task<int> ExecuteNonQuery(
        string CommandText,
        [Optional]
        SqlParameter[] Parameters,
        [Optional]
        Action<SqlCommand> Command
    ) {
        using SqlConnection connection = NewConnection;
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = CommandText;
        if (Parameters != null) command.Parameters.AddRange(Parameters);
        if (Command != null) Command(command);
        return await command.ExecuteNonQueryAsync();
    }
}