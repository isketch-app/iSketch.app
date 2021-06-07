using System;
using System.Data.SqlClient;
using iSketch.app.Data;

namespace iSketch.app.Services
{
    public class Database
    {
        public SqlConnection Connection;
        public Database()
        {
            try
            {
                Connection = new SqlConnection()
                {
                    ConnectionString = new SqlConnectionStringBuilder()
                    {
                        DataSource = Environment.GetEnvironmentVariable("IS_SQL_ServerHost"),
                        InitialCatalog = Environment.GetEnvironmentVariable("IS_SQL_DatabaseName"),
                        UserID = Environment.GetEnvironmentVariable("IS_SQL_User"),
                        Password = Environment.GetEnvironmentVariable("IS_SQL_Pass")
                    }.ToString()
                };
                Connection.Open();
                this.SetProperty("TEST", null);
            }
            catch(ArgumentNullException e)
            {
                throw new Exception(
                    "iSketch.app needs the following Environment Variables set in order to connect to the database:\n" +
                    "IS_SQL_ServerHost\n" +
                    "IS_SQL_DatabaseName\n" +
                    "IS_SQL_User\n" +
                    "IS_SQL_Pass"
                , e);
            }
            catch(Exception e)
            {
                throw new Exception("iSketch.app failed to open a connection to the database!", e);
            }
        }
    }
}