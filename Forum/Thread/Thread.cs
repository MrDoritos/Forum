using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forum.Thread
{
    public class Thread
    {
        public User.User author;
        public int id;
        public readonly DateTime creationTime = DateTime.Now;
        public List<Message.Message> messages = new List<Message.Message>();
        public Header header;
    }
}
