using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forum.User
{
    public class User
    {
        public string username;
        public string password;
        public string token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

        public readonly DateTime creationTime = DateTime.Now;
        
        public User(string username, string password) { this.username = username; this.password = password; }
    }
}
