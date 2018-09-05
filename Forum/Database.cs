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

        public List<FileHook> fileHooks;
        public int port;
        public string hostname;

        public Database(string databaseName) { saveDatabase = true; this.databaseName = databaseName; GenerateDatabase(); if (File.Exists(databaseName)) ParseConfig(); else SaveConfig(); }

        public Database() { GenerateDatabase(); }

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
            try
            {
                File.WriteAllText(databaseName, JsonConvert.SerializeObject(json));
            }
            catch (Exception) { }
        }
    }
}
