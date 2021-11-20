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
        public SqlConnection NewConnection { 
            get { 
                SqlConnection con = new()
                {
                    ConnectionString = new SqlConnectionStringBuilder()
                    {
                        DataSource = Environment.GetEnvironmentVariable("IS_SQL_ServerHost"),
                        UserID = Environment.GetEnvironmentVariable("IS_SQL_User"),
                        Password = Environment.GetEnvironmentVariable("IS_SQL_Pass"),
                        InitialCatalog = Environment.GetEnvironmentVariable("IS_SQL_DatabaseName")
                    }.ToString()
                };
                con.Open();
                return con;
            }
        }
        public Database()
        {
            Logger.Info("Database: Connecting to database & catelog...");
            try
            {
                NewConnection.Close();
                Logger.Info("Database: Success.");
            }
            catch
            {
                string Catelog = Environment.GetEnvironmentVariable("IS_SQL_DatabaseName");
                Logger.Info("Database: Connection failed with catelog, trying without initial catelog...");
                SqlConnection con = new()
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
                    Logger.Info("Database: Connected.");
                    if (con.Database != Catelog)
                    {
                        try
                        {
                            Logger.Info("Database: Opening catelog: " + Catelog + "...");
                            con.ChangeDatabase(Catelog);
                        }
                        catch (Exception e)
                        {
                            Logger.Error("Database: " + e.Message);
                            Logger.Info("Database: Could not connect to catelog, assuming it doesn't yet exist, creating...");
                            SqlCommand cmd = con.CreateCommand();
                            cmd.CommandText = "CREATE DATABASE [" + Catelog + "]";
                            cmd.ExecuteNonQuery();
                            con.ChangeDatabase(Catelog);
                        }
                    }
                    if (!IsDBSetUp()) InitializeDBSchema();
                    if (!IsSchemaUpToDate()) UpdateDBSchema();
                    Logger.Info("Database: Success.");
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
                finally
                {
                    con.Close();
                }
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
            RunSQLScript(new FileInfo("./SQL/Schema/iSketch.app.sql"));
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
                        Logger.Error("Database: " + e.Message);
                        continue;
                    }
                }
            }
            finally
            {
                cmd.Connection.Close();
            }
            Logger.Info("Database: File: " + fileInfo.Name + " done executing.");
        }
    }
}