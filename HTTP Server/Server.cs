using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using HTTP_Server.HTTP;

namespace HTTP_Server
{
    public class Server
    {
        public event Func<HttpRequest, TcpClient, Task> RequestRecieved;
        public TcpListener Socket { get; private set; }
        public IPEndPoint Bind { get; private set; }
        
        public String Hostname
        {
            get { return Bind.Address.ToString(); }
        }
        public int Port
        {
            get { return Bind.Port; }
        }
        public Server()
        {
            Bind = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 8080); Socket = new TcpListener(Bind);
        }

        private async Task Listener()
        {
            while (true)
            {
                try
                {
                    var client = await Socket.AcceptTcpClientAsync();
                    await Task.Run(() => ClientHandler(client));
                }
                catch (Exception)
                {
                    Console.WriteLine("New client caused exception");
                }
            }
        }
        
        public async Task ClientHandler(TcpClient client)
        {
            Console.WriteLine($"New client {client.Client.RemoteEndPoint.ToString()}");
            NetworkStream networkStream;
            byte[] buffer = new byte[4096];
            byte[] recieved = new byte[0];
            int i;
            networkStream = client.GetStream();
            while (client.Connected)
            {
                if (networkStream.DataAvailable)
                {
                    while (networkStream.DataAvailable && (i = await networkStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                        recieved = Extensions.Append(recieved, buffer);
                    var request = HttpRequest.Parse(recieved);
                    if (request == null) { client.Close(); }
                    if (request.Header.IsRequest)
                    {
                        Console.WriteLine($"Recieved request from client {client.Client.RemoteEndPoint.ToString()}\n{request.Header.Method.ToString()} {request.Header.RequestURI}");
                        var invoclist = RequestRecieved.GetInvocationList();
                        var handtasks = new Task[invoclist.Length];
                        for (int an = 0; an < invoclist.Length; an++)
                            handtasks[an] = ((Func<HttpRequest, TcpClient, Task>)invoclist[an])(request, client);
                        await Task.WhenAll(handtasks);
                        if (client.Connected) client.Close();
                    }
                    else
                        Console.WriteLine($"Recieved response from expected request");
                }
                await Task.Delay(10);
            }
        }



        public void StartServer(IPEndPoint bind)
        {
            if (bind == null) { return; }
            try
            {
                Socket = new TcpListener(bind);
                Socket.Start();
                Bind = bind;
                Task.Run(Listener);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not start server {e.Message}");
            }
        }

        public void StartServer()
        {
            try
            {
                Socket.Start();
                Task.Run(Listener);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not start server {e.Message}");
            }
        }
    }

    public static class Extensions
    {
        public static byte[] Append(this byte[] source, byte[] data)
        {
            var ss = new List<byte>(source);
            ss.AddRange(data);
            return ss.ToArray();
        }
    }
}

