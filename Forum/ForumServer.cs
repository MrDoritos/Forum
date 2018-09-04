using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HTTP_Server;
using HTTP_Server.HTTP;
using System.Net.Sockets;
using HtmlAgilityPack;
using System.IO;

namespace Forum
{
    class ForumServer
    {
        public HTTP_Server.Server server;
        public Forum forum;

        static public string someHTML =
           "<html><title>Test</title><body><p>Hello World</p></body></html>";

        static public string loginHTML = "" +
            "<html>" +
            "<head>" +
            "<title>" +
            "Login" +
            "</title>" +
            "</head>" +
            "<body>" +
            "<form action=\"/login\" method=\"POST\">" +
            "Username" +
            "<input id=\"username\" name=\"username\" type=\"text\">" +
            "Password" +
            "<input id=\"password\" name=\"password\" type=\"text\">" +
            "<output for=\"username password\"></output>" +
            "<input type=\"submit\">" +
            "</form>" +
            "</body>";

        
        static public string SetSessionCookieJavascript(string token)
        {
            return $"document.cookie = \"token={token ?? ""}; expires=2147483647; path=/\"";
        }

        static public string Something(string token)
        {
            return $"<html><head><title>Main Page</title></head><body><script>{SetSessionCookieJavascript(token)}</script><p>Main page</p></body>";
        }

        static public async Task SendSetSessionCookieJavascript(TcpClient client, string token)
        {
            HttpResponse httpResponse = new HttpResponse
            {
                Content = new HTTP_Server.HTTP.Message.Content() { Buffer = Encoding.UTF8.GetBytes(Something(token)) },
                Header = new HTTP_Server.HTTP.Message.Header() { SetContentType = HTTP_Server.HTTP.Message.Header.ContentTypes.JAVASCRIPT},
            };
            client.Client.Send(httpResponse.Serialize());
            await Task.Delay(1);
        }

        public enum Request
        {
            Main = 1,
            Thread = 2,
            User = 3,
            Login = 4,
        }

        public ForumServer()
        {
            forum = new Forum();
            server = new HTTP_Server.Server();
            server.RequestRecieved += RequestRecieved;
            forum.userManager.CreateUser("cool guy", "password");
            forum.userManager.CreateUser("someone", "password");
            var usr = forum.userManager.Authenticate("cool guy", "password");
            var otherusr = forum.userManager.Authenticate("someone", "password");
            forum.threadManager.CreateThread(usr, new Thread.Header() { title = "first thread!", content = "first thread on the site", tags = new string[] { "tag" } });
            forum.threadManager.AppendThread(forum.threadManager.GetThread(1), new Thread.Message.Message(usr, "this is the first message!"));
            forum.threadManager.AppendThread(forum.threadManager.GetThread(1), new Thread.Message.Message(otherusr, "best forum ever"));
            forum.threadManager.AppendThread(forum.threadManager.GetThread(1), new Thread.Message.Message(usr, "agreed"));
            forum.threadManager.CreateThread(new User.User("other guy", "password"), new Thread.Header() { title = "second thread", content = "nothing", tags = new string[] { "" } });
        }
        
        public void StartForumServer()
        {            
            server.StartServer();
        }

        public async Task RequestRecieved(HttpRequest request, TcpClient client)
        {
            if (request.Header.RequestURI == "/login")
            {
                if (request.Header.Method == HTTP_Server.HTTP.Message.Header.Methods.POST)
                {
                    var creds = Server.Login.GetUser(request);
                    if (forum.userManager.UserExists(creds.username, creds.password))
                    {
                        //Set cookie token
                    }
                    else
                    {
                        forum.userManager.CreateUser(creds.username, creds.password);
                        //Set cookie token
                    }
                    var usr = forum.userManager.Authenticate(creds.username, creds.password);
                    Console.WriteLine($"User login request {creds.username} : {creds.password}, token {usr.token}");
                    await SendSetSessionCookieJavascript(client, usr.token);
                }
                else
                {
                    await SendLoginScreen(client);
                }
            }
            else if (request.Header.RequestURI == "/mainpage" || request.Header.RequestURI == "/")
            {
                SendMainPage(client);
            }
            else if (request.Header.RequestURI.StartsWith("/thread&"))
            {
                int.TryParse(request.Header.RequestURI.Split('&').Last(), out int id);
                var thread = forum.threadManager.GetThread(id);
                if (thread != null)
                {
                    SendThreadPage(thread, client);
                }
                else
                {
                    await SendNotFound(client);
                }
            }
            else
            {
                await SendNotFound(client);
            }
        }

        private void SendThreadPage(Thread.Thread thread, TcpClient client)
        {
            HttpResponse httpResponse = new HttpResponse
            {
                Content = new HTTP_Server.HTTP.Message.Content
                {
                    Buffer = Encoding.UTF8.GetBytes(GetThreadPage(thread))
                },
                Header = new HTTP_Server.HTTP.Message.Header()
            };
            client.Client.Send(httpResponse.Serialize());
        }

        private string GetThreadPage(Thread.Thread thread)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(File.ReadAllText("threadpage.html"));
            if (doc.DocumentNode.DescendantsAndSelf().Any(n => n.HasAttributes && n.Attributes.Any(m => m.Name == "class" && m.Value == "messagetable")))
            {
                GetThreadMessages(thread, doc);
            }
            else
            {
                Console.WriteLine("Invalid ThreadPage HTML");
            }
            MemoryStream memoryStream = new MemoryStream();
            doc.Save(memoryStream);
            return (doc.Encoding.GetString(memoryStream.ToArray()));
        }

        private void GetThreadMessages(Thread.Thread thread, HtmlDocument doc)
        {
            var messagetable = doc.DocumentNode.DescendantsAndSelf().First(n => n.HasAttributes && n.Attributes.Any(m => m.Name == "class" && m.Value == "messagetable"));
            foreach (var message in thread.messages)
            {
                HtmlNode div = doc.CreateElement("div");
                div.Attributes.Add("style", "border: 5px solid black; margin: 5px;");
                HtmlNode content = doc.CreateElement("p");
                HtmlNode author = doc.CreateElement("p");
                author.AppendChild(HtmlTextNode.CreateNode(message.author.username));
                content.AppendChild(HtmlTextNode.CreateNode(message.contents));
                div.AppendChild(author);
                div.AppendChild(content);
                messagetable.AppendChild(div);
            }           
        }

        private void SendMainPage(TcpClient client)
        {
            HttpResponse httpResponse = new HttpResponse
            {
                Content = new HTTP_Server.HTTP.Message.Content
                {
                    Buffer = Encoding.UTF8.GetBytes(GetMainPage())
                },
                Header = new HTTP_Server.HTTP.Message.Header()
            };
            client.Client.Send(httpResponse.Serialize());
        }

        private string GetMainPage()
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(File.ReadAllText("mainpage.html"));
            if (doc.DocumentNode.DescendantsAndSelf().Any(n => n.HasAttributes && n.Attributes.Any(m => m.Name == "class" && m.Value == "threadtable")))
            {
                GetThreadsHTML(doc);
            }
            else
            {
                Console.WriteLine("Invalid Mainpage HTML");
            }
            MemoryStream memoryStream = new MemoryStream();
            doc.Save(memoryStream);
            return doc.Encoding.GetString(memoryStream.ToArray());
        }

        private void GetThreadsHTML(HtmlDocument doc)
        {
            var threadtable = doc.DocumentNode.DescendantsAndSelf().First(n => n.HasAttributes && n.Attributes.Any(m => m.Name == "class" && m.Value == "threadtable"));
            foreach (var thread in forum.threadManager.Threads)
            {
                HtmlNode htmlNode = doc.CreateElement("div");
                htmlNode.Attributes.Add("style", "border: 5px solid black; margin: 5px;");
                HtmlNode title = doc.CreateElement("a");
                
                HtmlNode author = doc.CreateElement("p");
                title.Attributes.Add("href", $"/thread&{thread.id}");
                title.AppendChild(HtmlTextNode.CreateNode(thread.header.title));
                author.AppendChild(HtmlTextNode.CreateNode("Author: " + thread.author.username));
                htmlNode.AppendChild(title);
                htmlNode.AppendChild(author);
                threadtable.AppendChild(htmlNode);
            }
        }

        private async Task SendNotFound(TcpClient client)
        {
            HttpResponse httpResponse = new HttpResponse
            {
                Content = new HTTP_Server.HTTP.Message.Content(),
                Header = new HTTP_Server.HTTP.Message.Header
                {
                    StatusCode = HTTP_Server.HTTP.Message.Header.StatusCodes.NOT_FOUND
                }
            };
            client.Client.Send(httpResponse.Serialize());
            await Task.Delay(1);
        }

        private async Task SendLoginScreen(TcpClient client)
        {
            HttpResponse httpResponse = new HttpResponse
            {
                Content = new HTTP_Server.HTTP.Message.Content() { Buffer = Encoding.UTF8.GetBytes(loginHTML) },
                Header = new HTTP_Server.HTTP.Message.Header(),
            };
            client.Client.Send(httpResponse.Serialize());
            await Task.Delay(1);
        }

        public bool ContainsToken(HTTP_Server.HTTP.Message.Header header)
        {
            return (header.RequestHeaders.Any(n => n.Split(':')[0] == "token"));
        }

        public string GetToken(HTTP_Server.HTTP.Message.Header header)
        {
            return (header.RequestHeaders.First(n => n.Split(':')[0] == "token").Split(':').Last().Trim());
        }
    }
}
