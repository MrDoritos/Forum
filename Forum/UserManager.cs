using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forum
{
    public class UserManager
    {
        public List<User.User> Users { get; private set; }

        public UserManager() { Users = new List<User.User>(); }  
        
        public bool TokenExists(string token)
        {
            return (Users.Any(n => n.token == token));
        }

        public bool UserExists(string username, string password)
        {
            return (Users.Any(n => n.username == username && n.password == password));
        }

        public bool UsernameExists(string username)
        {
            return (Users.Any(n => n.username == username));
        }

        public User.User Authenticate(string token)
        {
            return (Users.First(n => n.token == token));
        }

        public User.User Authenticate(string username, string password)
        {
            return (Users.First(n => n.username == username && n.password == password));
        }

        public void CreateUser(string username, string password)
        {
            Users.Add(new User.User(username, password));
        }        

        public User.User TryAuth(string token)
        {
            if (token.Length > 0)
            {
                if (TokenExists(token))
                {
                    return Authenticate(token);
                }
                else
                {
                    return new User.User("anonymous", "password");
                }
            }
            else
            {
                return new User.User("anonymous", "password");
            }
        }
    }
}
