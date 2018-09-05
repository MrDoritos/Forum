﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HTTP_Server;
using HTTP_Server.HTTP;
using System.Net.Sockets;
using HtmlAgilityPack;
using System.IO;
using Forum.Server;

namespace Forum
{
    class ForumServer
    {
        public HTTP_Server.Server server;
        public Forum forum;
        public Database database;
        public User.User anonymous = new User.User("anonymous", "password");

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

        static public string loginHTMLwrongpass = "" +
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
            "<p><strong>Wrong username or password</strong></p>" +
            "</body>";

        static public string SetSessionCookieJavascript(string token)
        {
            return $"document.cookie = \"token={token ?? ""}; expires=2147483647; path=/\"; window.location = '/'";
        }

        static public string Something(string token)
        {
            return $"<html><head><title>Main Page</title></head><body><script>{SetSessionCookieJavascript(token)}</script><p>Main page</p></body>";
        }

        public async Task SendSetSessionCookieJavascript(TcpClient client, string token)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(GetMainPage());
            var script = doc.CreateElement("script");
            script.AppendChild(HtmlTextNode.CreateNode(SetSessionCookieJavascript(token)));
            doc.DocumentNode.Descendants().First(n => n.Name == "body").AppendChild(script);
            MemoryStream memstr = new MemoryStream();
            doc.Save(memstr);
            HttpResponse httpResponse = new HttpResponse
            {
                Content = new HTTP_Server.HTTP.Message.Content() { Buffer = memstr.ToArray() },
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
            database = new Database("database.json");
            server = new HTTP_Server.Server(new System.Net.IPEndPoint(System.Net.Dns.GetHostAddresses(database.hostname)[0], database.port));
            
            server.RequestRecieved += RequestRecieved;
           
        }
        
        public void StartForumServer()
        {            
            server.StartServer();
        }

        public async Task RequestRecieved(HttpRequest request, TcpClient client)
        {
            //foreach (var he in request.Header.RequestHeaders)
            //{
            //    Console.WriteLine(he);
            //}
            if (request.Header.RequestURI == "/login")
            {
                if (request.Header.Method == HTTP_Server.HTTP.Message.Header.Methods.POST)
                {
                    var creds = Server.Login.GetUser(request);
                    if (creds.username.Length < 1)
                    {
                        await SendLoginScreen(client, true);
                    } else
                    if (forum.userManager.UserExists(creds.username, creds.password))
                    {
                        var usr = forum.userManager.Authenticate(creds.username, creds.password);
                        //Set cookie token
                        await SendSetSessionCookieJavascript(client, usr.token);
                        Console.WriteLine($"User login request {creds.username} : {creds.password}, token {usr.token}");
                    }
                    else if (forum.userManager.UsernameExists(creds.username))
                    {
                        await SendLoginScreen(client, true);
                    }else
                    {
                        forum.userManager.CreateUser(creds.username, creds.password);
                        var usr = forum.userManager.Authenticate(creds.username, creds.password);
                        await SendSetSessionCookieJavascript(client, usr.token);
                        Console.WriteLine($"User created {creds.username} : {creds.password}, token {usr.token}");
                        //Set cookie token
                    }
                    
                }
                else
                {
                    await SendLoginScreen(client);
                }
            }
            else if (request.Header.RequestURI == "/mainpage" || request.Header.RequestURI == "/")
            {
                if (request.Header.Method == HTTP_Server.HTTP.Message.Header.Methods.POST)
                {
                    var req = ModifyRequest.Parse(request);
                    var usr = forum.userManager.TryAuth(req.usertoken);
                    if (req.title.Length > 0 && req.content.Length > 0)
                    forum.threadManager.CreateThread(usr, new Thread.Header { content = req.content, title = req.title, tags = new string[] { "" } });
                    SendRedirect(client, "/");
                } else                
                if (ModifyRequest.ContainsToken(request))
                {
                    var tt = ModifyRequest.GetToken(request);
                    var usr = forum.userManager.TryAuth(tt);
                    SendMainPage(client, usr);
                    
                }
                else
                {
                    SendMainPage(client);
                }
            }
            else if (request.Header.RequestURI.StartsWith("/thread&"))
            {
                int.TryParse(request.Header.RequestURI.Split('&').Last(), out int id);
                var thread = forum.threadManager.GetThread(id);
                if (thread != null)
                {
                    if (request.Header.Method == HTTP_Server.HTTP.Message.Header.Methods.GET)
                    {
                        SendThreadPage(thread, client);
                    }
                    else
                    {
                        var req = Server.ModifyRequest.Parse(request);
                        if (req.content.Length < 1)
                        {

                        }
                        else
                        {
                            Console.WriteLine($"token: {req.usertoken}, POSTing content: {req.content}");
                            if (req.file != null)
                            {
                                req.file.id = forum.fileManager.NextId();
                                forum.fileManager.AddFile(req.file);
                                req.content += $"<a href=\"{req.file.fileName}\"></a>";
                                }
                            if (forum.userManager.TokenExists(req.usertoken))
                            {
                                forum.threadManager.AppendThread(thread, new Thread.Message.Message(forum.userManager.Authenticate(req.usertoken), req.content));
                            }
                            else
                            {
                                forum.threadManager.AppendThread(thread, new Thread.Message.Message(new User.User("anonymous", "password"), req.content));
                            }
                        }
                        SendRedirect(client, request.Header.RequestURI);
                    }
                }
                else
                {
                    //await SendNotFound(client);
                    SendRedirect(client, "/");
                }
            }
            else if (request.Header.RequestURI.StartsWith("/content&"))
            {

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
        
        private void SendRedirect(TcpClient client, string URI)
        {
            HttpResponse httpResponse = new HttpResponse
            {
                Content = new HTTP_Server.HTTP.Message.Content(),
                Header = new HTTP_Server.HTTP.Message.Header
                {
                    location = URI,
                    StatusCode = HTTP_Server.HTTP.Message.Header.StatusCodes.REDIRECT,
                }
            };
            client.Client.Send(httpResponse.Serialize());
        }

        private string GetThreadPage(Thread.Thread thread)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(System.IO.File.ReadAllText("threadpage.html"));
            if (doc.DocumentNode.DescendantsAndSelf().Any(n => n.HasAttributes && n.Attributes.Any(m => m.Name == "class" && m.Value == "threadhead")))
            {
                var messagebox = doc.DocumentNode.DescendantsAndSelf().First(n => n.Attributes.Any(m => m.Name == "class" && m.Value == "threadhead"));
                HtmlNode form = doc.CreateElement("div");
                form.Attributes.Add("style", "border: 5px dashed blue");

                HtmlNode title = doc.CreateElement("h2");
                title.AppendChild(HtmlTextNode.CreateNode(thread.header.title));
                form.AppendChild(title);

                HtmlNode content = doc.CreateElement("p");
                content.AppendChild(HtmlTextNode.CreateNode(thread.header.content));
                form.AppendChild(content);

                messagebox.AppendChild(form);
            }
            if (doc.DocumentNode.DescendantsAndSelf().Any(n => n.HasAttributes && n.Attributes.Any(m => m.Name == "class" && m.Value == "messagetable")))
            {
                GetThreadMessages(thread, doc);
            }
            else
            {
                Console.WriteLine("Invalid ThreadPage HTML");
            }
            if (doc.DocumentNode.DescendantsAndSelf().Any(n => n.HasAttributes && n.Attributes.Any(m => m.Name == "class" && m.Value == "messagebox")))
            {
                var messagebox = doc.DocumentNode.DescendantsAndSelf().First(n => n.Attributes.Any(m => m.Name == "class" && m.Value == "messagebox"));
                HtmlNode form = doc.CreateElement("form");
                form.Attributes.Add("action", $"thread&{thread.id}");
                form.Attributes.Add("method", "POST");
                //form.Attributes.Add("enctype", "multipart/form-data");

                HtmlNode output = doc.CreateElement("output");
                output.Attributes.Add("for", "content datafile");
                //enctype=multipart/form-data
                HtmlNode content = doc.CreateElement("textarea");
                content.Attributes.Add("rows", "5");
                content.Attributes.Add("cols", "80");
                content.Attributes.Add("id", "content");
                content.Attributes.Add("name", "content");
                content.Attributes.Add("type", "text");

                HtmlNode submit = doc.CreateElement("input");
                submit.Attributes.Add("type", "submit");
                submit.Attributes.Add("value", "Add Comment");

                //HtmlNode addfile = doc.CreateElement("input");
                //addfile.Attributes.Add("type", "file");
                //addfile.Attributes.Add("name", "datafile");
                //addfile.Attributes.Add("id", "datafile");
                //addfile.Attributes.Add("value", "Attach File");
                var div1 = doc.CreateElement("div");
                div1.AppendChild(content);
                form.AppendChild(div1);

                var div2 = doc.CreateElement("div");
                div2.AppendChild(submit);
                //form.AppendChild(addfile);
                form.AppendChild(div2);
                form.AppendChild(output);

                messagebox.AppendChild(form);
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

        private void SendMainPage(TcpClient client, User.User user = null)
        {
            HttpResponse httpResponse = new HttpResponse
            {
                Content = new HTTP_Server.HTTP.Message.Content
                {
                    Buffer = Encoding.UTF8.GetBytes(GetMainPage(user ?? anonymous))
                },
                Header = new HTTP_Server.HTTP.Message.Header()
            };
            client.Client.Send(httpResponse.Serialize());
        }

        private string GetMainPage(User.User curUser = null)
        {
            if (curUser == null) { curUser = anonymous; }
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(System.IO.File.ReadAllText("mainpage.html"));
            if (doc.DocumentNode.DescendantsAndSelf().Any(n => n.HasAttributes && n.Attributes.Any(m => m.Name == "class" && m.Value == "threadtable")))
            {
                GetThreadsHTML(doc);
            }
            else
            {
                Console.WriteLine("Invalid Mainpage HTML");
            }
            if (doc.DocumentNode.DescendantsAndSelf().Any(n => n.HasAttributes && n.Attributes.Any(m => m.Name == "class" && m.Value == "threadbox")))
            {
                var messagebox = doc.DocumentNode.DescendantsAndSelf().First(n => n.Attributes.Any(m => m.Name == "class" && m.Value == "threadbox"));
                HtmlNode form = doc.CreateElement("form");
                form.Attributes.Add("action", $"/");
                form.Attributes.Add("method", "POST");

                HtmlNode output = doc.CreateElement("output");
                output.Attributes.Add("for", "content");

                HtmlNode title = doc.CreateElement("input");
                title.Attributes.Add("id", "title");
                title.Attributes.Add("name", "title");
                title.Attributes.Add("type", "text");

                HtmlNode content = doc.CreateElement("textarea");
                content.Attributes.Add("rows", "5");
                content.Attributes.Add("cols", "80");
                content.Attributes.Add("id", "content");
                content.Attributes.Add("name", "content");
                content.Attributes.Add("type", "text");

                HtmlNode submit = doc.CreateElement("input");
                submit.Attributes.Add("type", "submit");
                submit.Attributes.Add("value", "Add Thread");

                var div1 = doc.CreateElement("div");
                var div2 = doc.CreateElement("div");
                var div3 = doc.CreateElement("div");
                var div4 = doc.CreateElement("div");
                div1.AppendChild(HtmlTextNode.CreateNode("Title"));
                div2.AppendChild(title);
                div3.AppendChild(HtmlTextNode.CreateNode("Content"));
                div4.AppendChild(content);

                form.AppendChild(div1);
                form.AppendChild(div2);
                form.AppendChild(div3);
                form.AppendChild(div4);
                form.AppendChild(submit);
                form.AppendChild(output);

                messagebox.AppendChild(form);
            }
            else
            {
                Console.WriteLine("Invalid ThreadPage HTML");
            }
            if (curUser.username != "anonymous" && doc.DocumentNode.DescendantsAndSelf().Any(n => n.HasAttributes && n.Attributes.Any(m => m.Name == "class" && m.Value == "loggedin")))
            {
                var messagebox = doc.DocumentNode.DescendantsAndSelf().First(n => n.Attributes.Any(m => m.Name == "class" && m.Value == "loggedin"));
                messagebox.RemoveAllChildren();
                
                messagebox.AppendChild(HtmlTextNode.CreateNode(curUser.username));
            }
            else
            {
            }
            MemoryStream memoryStream = new MemoryStream();
            doc.Save(memoryStream);
            return doc.Encoding.GetString(memoryStream.ToArray());
        }

        private void GetThreadsHTML(HtmlDocument doc)
        {
            var threadtable = doc.DocumentNode.DescendantsAndSelf().First(n => n.HasAttributes && n.Attributes.Any(m => m.Name == "class" && m.Value == "threadtable"));
            foreach (var thread in forum.threadManager.Threads.OrderBy(n => n.id).AsEnumerable().Reverse())
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

        private async Task SendLoginScreen(TcpClient client, bool wrongpassword = false)
        {
            if (wrongpassword)
            {
                HttpResponse httpResponse = new HttpResponse
                {
                    Content = new HTTP_Server.HTTP.Message.Content() { Buffer = Encoding.UTF8.GetBytes(loginHTMLwrongpass) },
                    Header = new HTTP_Server.HTTP.Message.Header(),
                };
                client.Client.Send(httpResponse.Serialize());
                await Task.Delay(1);
            }
            else
            {
                HttpResponse httpResponse = new HttpResponse
                {
                    Content = new HTTP_Server.HTTP.Message.Content() { Buffer = Encoding.UTF8.GetBytes(loginHTML) },
                    Header = new HTTP_Server.HTTP.Message.Header(),
                };
                client.Client.Send(httpResponse.Serialize());
                await Task.Delay(1);
            }
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
