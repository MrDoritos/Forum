using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HTTP_Server.HTTP;

namespace Forum.Server
{
    class Login
    {
        static public UserStruct GetUser(HttpRequest httpRequest)
        {
            var content = httpRequest.Content;
            if (content.ContentLength > 0)
            {
                string username = "";
                string password = "";
                var args = Encoding.ASCII.GetString(content.Buffer);
                foreach (var ss in args.Split('&').Select(n => n.Trim('\n', '\r', '\0')))
                {
                    switch (ss.Split('=')[0].ToLower())
                    {
                        case "username":
                            username = ss.Split('=').Last();
                            break;
                        case "password":
                            password = ss.Split('=').Last();
                            break;                            
                    }
                }
                return new UserStruct() { username = username, password = password };
            }
            return new UserStruct() { username = "", password = "" };
        }
    }
}
