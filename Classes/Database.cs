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
    public static async Task<T> ExecuteReader<T>(
        [Optional]
        string CommandText,
        [Optional]
        SqlParameter[] Parameters,
        [Optional]
        Func<SqlCommand, Task> Command,
        Func<SqlDataReader, Task<T>> Reader
    ) {
        using SqlConnection connection = new(ConnectionString);
        await connection.OpenAsync();
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = CommandText;
        if (Parameters != null) command.Parameters.AddRange(Parameters);
        if (Command != null) await Command(command);
        using SqlDataReader reader = await command.ExecuteReaderAsync();
        return await Reader(reader);
    }
    public static async Task<T> ExecuteScalar<T>(
        [Optional]
        string CommandText,
        [Optional]
        SqlParameter[] Parameters,
        [Optional]
        Func<SqlCommand, Task> Command
    ) {
        using SqlConnection connection = new(ConnectionString);
        await connection.OpenAsync();
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = CommandText;
        if (Parameters != null) command.Parameters.AddRange(Parameters);
        if (Command != null) await Command(command);
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
        [Optional]
        string CommandText,
        [Optional]
        SqlParameter[] Parameters,
        [Optional]
        Func<SqlCommand, Task> Command
    ) {
        using SqlConnection connection = new(ConnectionString);
        await connection.OpenAsync();
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = CommandText;
        if (Parameters != null) command.Parameters.AddRange(Parameters);
        if (Command != null) await Command(command);
        return await command.ExecuteNonQueryAsync();
    }
}