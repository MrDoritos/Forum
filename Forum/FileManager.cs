using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forum.Server;

namespace Forum
{
    public class FileManager
    {
        public List<File> files = new List<File>();

        public void AddFile(File file)
        {
            files.Add(file);
        }
        public int NextId()
        {
            if (files.Count < 1)
            {
                return 1;
            }
            else
            {
                return files.Select(n => n.id).Max() + 1;
            }
        }
    }
}
