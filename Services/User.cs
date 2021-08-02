using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSketch.app.Services
{
    public class User
    {
        public Session Session;
        public User(Session Session)
        {
            this.Session = Session;
        }
    }
}