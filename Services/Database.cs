using System;
using System.Data.SqlClient;
using iSketch.app.Data;
using System.IO;
using System.Collections.Generic;

namespace iSketch.app.Services
{
    public class Database
    {
        public int SchemaVersion = 0;
        public SqlConnection Connection;
        public Database()
        {
            Logger.Info("Database: Initializing database connnection...");
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
                Logger.Info("Database: Connecting...");
                Connection.Open();
                Logger.Info("Database: Connected.");
                if (!IsDBSetUp()) InitializeDBSchema();
                if (!IsSchemaUpToDate()) UpdateDBSchema();
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
        public bool IsSchemaUpToDate()
        {
            Logger.Info("Database: Checking if schema is up to date...");
            if (int.TryParse(this.GetProperty("IS_SQL_SchemaVersion"), out int sv))
            {
                if (sv == SchemaVersion)
                {
                    Logger.Info("Database: The schema is up to date.");
                    return true; 
                }
            }
            Logger.Info("Database: The schema is NOT up to date.");
            return false;
        }
        public bool IsDBSetUp()
        {
            Logger.Info("Database: Checking if database is setup...");
            try
            {
                int.TryParse(this.GetProperty("IS_SQL_SchemaVersion"), out int _);
            }
            catch(Exception)
            {
                Logger.Info("Database: Database is not setup.");
                return false;
            }
            Logger.Info("Database: Database is already setup.");
            return true;
        }
        public void InitializeDBSchema()
        {
            Logger.Info("Database: Initializing schema...");
            foreach(FileInfo filei in new DirectoryInfo(@".\SQL\Schema").GetFiles())
            {
                RunSQLScript(filei);
            }
            this.SetProperty("IS_SQL_SchemaVersion", SchemaVersion.ToString());
            Logger.Info("Database: Initialization done.");
        }
        public void UpdateDBSchema()
        {
            Logger.Info("Database: Updating schema...");
            this.SetProperty("IS_SQL_SchemaVersion", SchemaVersion.ToString());
            Logger.Info("Database: Schema update done.");
        }
        public void RunSQLScript(FileInfo fileInfo)
        {
            Logger.Info("Database: Running file: " + fileInfo.Name);
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
            foreach (string script in scripts)
            {
                try
                {
                    SqlCommand cmd = Connection.CreateCommand();
                    cmd.CommandText = script;
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Logger.Error("Database: " + e.Message);
                    continue;
                }
            }
            Logger.Info("Database: File: " + fileInfo.Name + " done executing.");
        }
    }
}