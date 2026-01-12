using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.IO;
using iSketch.app.Classes;

namespace iSketch.app.Services
{
    public class Database
    {
        public int SchemaVersion = 0;
        public ILogger<Database> Logger;
        public SqlConnection NewConnection
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
        public Database(ILogger<Database> Logger)
        {
            this.Logger = Logger;
            SqlConnection con = null;
            Logger.LogInformation("Connecting to database & catelog...");
            try
            {
                con = NewConnection;
                Logger.LogInformation("Success.");
            }
            catch
            {
                string Catelog = Environment.GetEnvironmentVariable("IS_SQL_DatabaseName");
                Logger.LogInformation("Connection failed with catelog, trying without initial catelog...");
                con = new()
                {
                    ConnectionString = new SqlConnectionStringBuilder()
                    {
                        DataSource = Environment.GetEnvironmentVariable("IS_SQL_ServerHost"),
                        UserID = Environment.GetEnvironmentVariable("IS_SQL_User"),
                        Password = Environment.GetEnvironmentVariable("IS_SQL_Pass")
                    }.ToString()
                };
                try
                {
                    con.Open();
                    Logger.LogInformation("Connected.");
                    if (con.Database != Catelog)
                    {
                        try
                        {
                            Logger.LogInformation("Opening catelog: " + Catelog + "...");
                            con.ChangeDatabase(Catelog);
                        }
                        catch (Exception e)
                        {
                            Logger.LogError(e, "Exception encountered.");
                            Logger.LogInformation("Could not connect to catelog, assuming it doesn't yet exist, creating...");
                            SqlCommand cmd = con.CreateCommand();
                            cmd.CommandText = "CREATE DATABASE [" + Catelog + "]";
                            cmd.ExecuteNonQuery();
                            con.ChangeDatabase(Catelog);
                        }
                    }
                    Logger.LogInformation("Success.");
                }
                catch (ArgumentNullException e)
                {
                    throw new Exception(
                        "iSketch.app needs the following Environment Variables set in order to connect to the database:\n" +
                        "IS_SQL_ServerHost\n" +
                        "IS_SQL_DatabaseName\n" +
                        "IS_SQL_User\n" +
                        "IS_SQL_Pass"
                    , e);
                }
                catch (Exception e)
                {
                    throw new Exception("iSketch.app failed to open a connection to the database!", e);
                }
            }
            finally
            {
                try
                {
                    if (!IsDBSetUp()) InitializeDBSchema();
                    if (!IsSchemaUpToDate()) UpdateDBSchema();
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Could not initialize the database.");
                }
                con?.Close();
            }
        }
        public bool IsSchemaUpToDate()
        {
            Logger.LogInformation("Checking if schema is up to date...");
            if (int.TryParse(this.GetProperty("IS_SQL_SchemaVersion"), out int sv))
            {
                if (sv == SchemaVersion)
                {
                    Logger.LogInformation("The schema is up to date.");
                    return true;
                }
            }
            Logger.LogInformation("The schema is NOT up to date.");
            return false;
        }
        public bool IsDBSetUp()
        {
            Logger.LogInformation("Checking if database is setup...");
            try
            {
                int.TryParse(this.GetProperty("IS_SQL_SchemaVersion"), out int _);
            }
            catch (Exception)
            {
                Logger.LogInformation("Database is not setup.");
                return false;
            }
            Logger.LogInformation("Database is already setup.");
            return true;
        }
        public void InitializeDBSchema()
        {
            Logger.LogInformation("Initializing schema...");
            RunSQLScript(new FileInfo("./SQL/Schema/iSketch.app.sql"));
            this.SetProperty("IS_SQL_SchemaVersion", SchemaVersion.ToString());
            Logger.LogInformation("Initialization done.");
        }
        public void UpdateDBSchema()
        {
            Logger.LogInformation("Updating schema...");
            this.SetProperty("IS_SQL_SchemaVersion", SchemaVersion.ToString());
            Logger.LogInformation("Schema update done.");
        }
        public void RunSQLScript(FileInfo fileInfo)
        {
            Logger.LogInformation("Running file: " + fileInfo.Name);
            List<string> scripts = new List<string>();
            string file = File.ReadAllText(fileInfo.FullName);
            string scriptGen = "";
            foreach (string line in file.Split("\r\n"))
            {
                if (line.ToLower().StartsWith("use ")) continue;
                if (line.ToLower() == "go")
                {
                    if (scriptGen.Trim() != "") scripts.Add(scriptGen);
                    scriptGen = "";
                    continue;
                }
                scriptGen += line + "\r\n";
            }
            SqlCommand cmd = NewConnection.CreateCommand();
            try
            {
                foreach (string script in scripts)
                {
                    try
                    {
                        cmd.CommandText = script;
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e, "Error running SQL script.");
                        continue;
                    }
                }
            }
            finally
            {
                cmd.Connection.Close();
            }
            Logger.LogInformation("File: " + fileInfo.Name + " done executing.");
        }
    }
}