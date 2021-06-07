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
            if (int.TryParse(this.GetProperty("IS_SQL_SchemaVersion"), out int sv))
            {
                if (sv == SchemaVersion) return true;
            }
            return false;
        }
        public bool IsDBSetUp()
        {
            try
            {
                int.TryParse(this.GetProperty("IS_SQL_SchemaVersion"), out int _);
            }
            catch(Exception)
            {
                return false;
            }
            return true;
        }
        public void InitializeDBSchema()
        {
            foreach(FileInfo filei in new DirectoryInfo(@".\SQL\Schema").GetFiles())
            {
                RunSQLScript(filei);
            }
            this.SetProperty("IS_SQL_SchemaVersion", SchemaVersion.ToString());
        }
        public void UpdateDBSchema()
        {
            this.SetProperty("IS_SQL_SchemaVersion", SchemaVersion.ToString());
        }
        public void RunSQLScript(FileInfo fileInfo)
        {
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
                    Logger.Error(e.Message);
                    continue;
                }
            }
        }
    }
}