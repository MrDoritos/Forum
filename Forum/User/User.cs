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
        private IDictionary<int, DateTime> readThreads = new Dictionary<int, DateTime>();

        public void Read(int id)
        {
            if (readThreads.ContainsKey(id))
            {
                readThreads[id] = DateTime.Now;
            }
            else
            {
                readThreads.Add(id, DateTime.Now);
            }
        }

        public string GetRead(int id, DateTime lastmessagetime)
        {
            if (HaveRead(id, lastmessagetime))
            {
                return "solid black";
            }
            else
            {
                return "dashed blue";
            }
        }

        public bool HaveRead(int id, DateTime lastmessagetime) { return readThreads.Any(n => n.Key.Equals(id) && n.Value.Ticks > lastmessagetime.Ticks); }

        public readonly DateTime creationTime = DateTime.Now;
        
        public User(string username, string password) { this.username = username; this.password = password; }
        public User() { }
    }
}
