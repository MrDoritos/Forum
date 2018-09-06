using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HTTP_Server.HTTP;
using System.Web;

namespace Forum.Server
{
    class Login
    {
        static public UserStruct GetUser(HTTP_Server.HTTP.HttpRequest httpRequest)
        {
            var content = httpRequest.Content;
            if (content.ContentLength > 0)
            {
                string username = "";
                string password = "";
                var args = Encoding.UTF8.GetString(content.Buffer);
                foreach (var ss in args.Split('&').Select(n => n.Trim('\n', '\r', '\0')))
                {
                    switch (ss.Split('=')[0].ToLower())
                    {
                        case "username":
                            username = HttpUtility.UrlDecode(ss.Remove(0, ss.Split('=')[0].Length + 1));
                            break;
                        case "password":
                            password = HttpUtility.UrlDecode(ss.Remove(0, ss.Split('=')[0].Length + 1));
                            break;                            
                    }
                }
                return new UserStruct() { username = username, password = password };
            }
            return new UserStruct() { username = "", password = "" };
        }
    }
}
