using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Forum.Server
{
    class ModifyRequest
    {
        public string content = "";
        public string title = "";
        public string usertoken = "";
        public DateTime timestamp = DateTime.Now;
        public RequestType requestType;
        public File file;

        public enum RequestType
        {
            AddComment = 1,     
            AddThread = 2,
        }

        static public ModifyRequest Parse(HTTP_Server.HTTP.HttpRequest httpRequest)
        {
            ModifyRequest modifyRequest = new ModifyRequest();
            var token = httpRequest.Header.RequestHeaders.FirstOrDefault(n => n.ToLower().StartsWith("cookie"));
            if (token != null && token.Length > 14) { modifyRequest.usertoken = token.Remove(0, 14).Trim('\r', '\n', '\0'); }
            //var bytes = Encoding.ASCII.GetString(httpRequest.Content.Buffer);
            var args = Encoding.UTF8.GetString(httpRequest.Content.Buffer);
            foreach (var ss in args.Split('&').Select(n => n.Trim('\n', '\r', '\0')))
            {
                switch (ss.Split('=')[0].ToLower())
                {
                    case "type":
                        switch (ss.Split('=').Last())
                        {
                            case "newmessage":
                                modifyRequest.requestType = RequestType.AddComment;
                                break;
                            case "newthread":
                                modifyRequest.requestType = RequestType.AddThread;
                                break;
                        }
                        break;
                    case "title":
                        modifyRequest.title = HttpUtility.UrlDecode(ss.Remove(0, ss.Split('=')[0].Length + 1));                        
                        break;
                    case "content":
                        modifyRequest.content = HttpUtility.UrlDecode(ss.Remove(0, ss.Split('=')[0].Length + 1));
                        break;
                    case "datafile":
                        string name = $"{new Random().Next(10000000, 999999999)}.raw";
                        File file = new File();
                        file.fileName = ss.Remove(0, ss.Split('=')[0].Length + 1);
                        file.filePath = ".\\";                        
                        System.IO.File.WriteAllBytes(file.fileName, httpRequest.Content.Buffer);
                        break;
                }
            }
            return modifyRequest;
        }

        static public string GetToken(HTTP_Server.HTTP.HttpRequest httpRequest)
        {
            return httpRequest.Header.RequestHeaders.FirstOrDefault(n => n.ToLower().StartsWith("cookie")).Remove(0, 14).Trim('\r', '\n', '\0');
        }

        static public bool ContainsToken(HTTP_Server.HTTP.HttpRequest httpRequest)
        {
            return httpRequest.Header.RequestHeaders.Any(n => n.ToLower().StartsWith("cookie"));
        }
    }
}
