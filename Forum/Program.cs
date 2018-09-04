using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forum
{
    class Program
    {
        static ForumServer forumServer;

        public static void Main(string[] args)
        {
            forumServer = new ForumServer();
            forumServer.StartForumServer();
            Task.Delay(-1).GetAwaiter().GetResult();
        }
    }
}
