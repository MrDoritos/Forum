using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forum
{
    class FileHook
    {       
        public FileHook(string fileName, string filePath, string URI) { this.fileName = fileName; this.filePath = filePath; this.URI = URI; }
        public string fileName;
        public string filePath;
        public string URI;
    }
}
