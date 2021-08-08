using iSketch.app.Data;

namespace iSketch.app.Services
{
    public class User
    {
        public Session Session;
        public Permissions Permissions = new Permissions();
        private Database Database;
        public User(Session Session, Database Database)
        {
            this.Database = Database;
            this.Session = Session;
            ReloadPermissionsFromDatabase();
        }
        public void ReloadPermissionsFromDatabase()
        {
            Permissions = Database.Connection.ReadPermissionsFromDatabase(Session.UserID);
        }
    }
}