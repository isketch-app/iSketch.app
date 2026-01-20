using System;
using Microsoft.Data.SqlClient;

namespace iSketch.app.Classes;

public static class Database
{
    public static SqlConnection NewConnection
    {
        get
        {
            SqlConnection con = new()
            {
                ConnectionString = new SqlConnectionStringBuilder()
                {
                    DataSource = Environment.GetEnvironmentVariable("IS_SQL_ServerHost"),
                    UserID = Environment.GetEnvironmentVariable("IS_SQL_User"),
                    Password = Environment.GetEnvironmentVariable("IS_SQL_Pass"),
                    InitialCatalog = Environment.GetEnvironmentVariable("IS_SQL_DatabaseName"),
                    TrustServerCertificate = true
                }.ToString()
            };
            con.Open();
            return con;
        }
    }
}
