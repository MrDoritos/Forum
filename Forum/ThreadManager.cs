using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forum.Thread;

namespace Forum
{
    public class ThreadManager
    {
        public List<Thread.Thread> Threads { get; private set; } = new List<Thread.Thread>();

        public void CreateThread(User.User requestuser, Header header)
        {
            Thread.Thread thread = new Thread.Thread
            {
                author = requestuser,
                id = NextId(),
                header = header,
            };
            Threads.Add(thread);
        }

        public bool ThreadExists(int id)
        {
            return (Threads.Any(n => n.id == id));
        }

        public Thread.Thread GetThread(int id)
        {
            return (Threads.FirstOrDefault(n => n.id == id));
        }

        public void AppendThread(Thread.Thread thread, Thread.Message.Message message)
        {
            GetThread(thread.id).messages.Add(message);
        }

        public int NextId()
        {
            if (Threads.Count() < 1)
            {
                return 1;
            }
            else
            {
                return Threads.Select(n => n.id).Max() + 1;
            }
        }
    }
}
