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
}