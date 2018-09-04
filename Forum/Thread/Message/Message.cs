using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forum.User;

namespace Forum.Thread.Message
{
    public class Message
    {
        public Message(User.User author, string contents) { this.author = author; this.contents = contents; }
        public User.User author;
        public readonly DateTime creationTime = DateTime.Now;
        public readonly string contents;    
    }
}
