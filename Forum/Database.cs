using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Forum
{
    class Database
    {
        public bool saveDatabase;
        public string databaseName;

        public Forum forum;
        public List<FileHook> fileHooks;
        public int port;
        public string hostname;

        public Database(string databaseName, Forum forum) { this.forum = forum; saveDatabase = true; this.databaseName = databaseName; GenerateDatabase(); if (File.Exists(databaseName)) ParseConfig(); else SaveConfig(); }

        public Database(Forum forum) { this.forum = forum; GenerateDatabase(); }

        public void GenerateDatabase()
        {
            fileHooks = new List<FileHook>();
            AddFileHook(new FileHook("mainpage.html", ".\\mainpage.html", "/"));
            port = 8080;
            hostname = "localhost";
        }

        public void AddFileHook(FileHook fileHook)
        {
            fileHooks.Add(fileHook);
        }

        public void RemoveFileHook(FileHook fileHook)
        {
            fileHooks.RemoveAll(n => n == fileHook);
        }

        public void ParseConfig()
        {
            if (!File.Exists(databaseName)) { return; }
            try
            {
                JObject file = JObject.Parse(File.ReadAllText(databaseName));
                if (file.ContainsKey("hostname")) { hostname = (string)file["hostname"]; }
                if (file.ContainsKey("port")) { port = (int)file["port"]; }
                if (file.ContainsKey("hooks")) { fileHooks = GetHooks((JArray)file["hooks"]).ToList(); }
                if (file.ContainsKey("threads")) { forum.threadManager.AddMany(GetThreads((JArray)file["threads"])); }
            }
            catch { return; }
        }

        public IEnumerable<FileHook> GetHooks(JArray hooks)
        {
            List<FileHook> fileHooks = new List<FileHook>();
            foreach (var value in hooks)
            {
                string fileName = (string)value["fileName"];
                string filePath = (string)value["filePath"];
                string URI = (string)value["URI"];
                fileHooks.Add(new FileHook(fileName, filePath, URI));
            }
            return fileHooks;
        }

        public IEnumerable<User.User> GetUsers(JArray users)
        {
            List<User.User> usersList = new List<User.User>();
            return usersList;
        }

        public IEnumerable<Thread.Thread> GetThreads(JArray threads)
        {
            List<Thread.Thread> threadsList = new List<Thread.Thread>();
            foreach (var value in threads)
            {
                try
                {
                    Thread.Thread thread = new Thread.Thread();
                    thread.id = (int)value["id"];
                    thread.messages = GetMessages(value["comments"].Value<JArray>()).ToList();
                    thread.author = GetUser(value["author"].Value<JObject>());
                    thread.header = GetHeader(value["header"].Value<JObject>());
                    threadsList.Add(thread);
                }
                catch (Exception)
                {
                    Console.WriteLine("Could not retrieve thread from database");
                }
            }
            return threadsList;
        }

        public Thread.Header GetHeader(JObject header)
        {
            Thread.Header nheader = new Thread.Header();
            nheader.content = (string)header["content"];
            nheader.tags = new string[] { "" };
            nheader.title = (string)header["title"];
            return nheader;
        }

        public IEnumerable<Thread.Message.Message> GetMessages(JArray messages)
        {
            if (messages == null) { return new List<Thread.Message.Message>(); }
            List<Thread.Message.Message> messagesList = new List<Thread.Message.Message>();
            foreach (var value in messages)
            {
                Thread.Message.Message message = new Thread.Message.Message(GetUser(value["author"].Value<JObject>()), (string)value["content"]);
                messagesList.Add(message);
            }
            return messagesList;
        }

        public User.User GetUser(JObject user)
        {
            User.User nuser = new User.User() { username = (string)user["username"], password = (string)user["password"], token = (string)user["token"] };
            return nuser;
        }

        public void SaveConfig()
        {
            if (!saveDatabase) { return; }
            JObject json = new JObject();

            json.Add("hostname", hostname);
            json.Add("port", port);

            JArray hooks = new JArray();
            foreach (var hook in fileHooks)
            {
                JObject fhook = new JObject();
                fhook.Add("fileName", hook.fileName);
                fhook.Add("filePath", hook.filePath);
                fhook.Add("URI", hook.URI);
                hooks.Add(fhook);
            }
            json.Add("hooks", hooks);

            JArray threads = new JArray();
            foreach (var thread in forum.threadManager.Threads)
            {
                JObject tt = new JObject();
                tt.Add("id", thread.id);
                JArray comments = new JArray();
                foreach (var comment in thread.messages)
                {
                    JObject message = new JObject();
                    message.Add("content", comment.contents);
                    JObject author = new JObject();
                    author.Add("username", comment.author.username);
                    author.Add("password", comment.author.password);
                    author.Add("token", comment.author.token);
                    message.Add("author", author);
                    comments.Add(message);                    
                }
                tt.Add("comments", comments);
                JObject tauthor = new JObject();
                tauthor.Add("username", thread.author.username);
                tauthor.Add("password", thread.author.password);
                tauthor.Add("token", thread.author.token);
                tt.Add("author", tauthor);


                JObject header = new JObject();
                header.Add("title", thread.header.title);
                header.Add("content", thread.header.content);
                tt.Add("header", header);
                threads.Add(tt);
            }
            json.Add("threads", threads);
            try
            {
                File.WriteAllText(databaseName, JsonConvert.SerializeObject(json));
            }
            catch (Exception) { }
        }
    }
}
