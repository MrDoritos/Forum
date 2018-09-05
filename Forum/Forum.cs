using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HTTP_Server;

namespace Forum
{
    public class Forum
    {
        public ThreadManager threadManager;
        public UserManager userManager;
        public FileManager fileManager;

        public Forum() { threadManager = new ThreadManager(); userManager = new UserManager(); fileManager = new FileManager(); }
    }
}
