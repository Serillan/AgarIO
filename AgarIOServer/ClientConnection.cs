using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace AgarIOServer
{
    class UdpServer : UdpClient
    {
        public const int ServerPort = 11028;
        public const int ClientPort = 11020;

        private static UdpServer server;

        private UdpServer(IPEndPoint ip) : base (ip) { }

        public static UdpServer GetInstance()
        {
            if (server == null)
                server = new UdpServer(new IPEndPoint(IPAddress.Any, ServerPort));
            return server;
        }
    }

    class ClientConnection
    {
        string PlayerName;

        UdpClient UdpClient;

        public static async Task<ClientConnection> AcceptClientAsync()
        {
            ClientConnection conn = new ClientConnection();
            conn.UdpClient = new UdpClient();

            var connectionResult = await UdpServer.GetInstance().ReceiveAsync();
            var message = GetMessageFromConnectionResult(connectionResult);

            if (message.Split().Length > 1 && message.Split()[0] == "CONNECT")
            {
                // TODO check whether the name is already in use
                conn.UdpClient.Connect(connectionResult.RemoteEndPoint.Address, UdpServer.ClientPort);
                var tokens = message.Split();
                conn.PlayerName = message.Substring(8);
                
            }   

            Console.WriteLine("Player {0} with IP Adress {1} has succesfully connected!", 
                conn.PlayerName, connectionResult.RemoteEndPoint.Address);

            conn.SendAsync("CONNECTED");

            return conn;
        }

        public async Task SendAsync(string message)
        {
            var bytes = Encoding.Default.GetBytes(message);
            await UdpClient.SendAsync(bytes, bytes.Length);
            Console.WriteLine("Sended");
        }

        public async Task<string> ReceiveAsync()
        {
            var res = await UdpServer.GetInstance().ReceiveAsync();
            return GetMessageFromConnectionResult(res);
        }

        public static string GetMessageFromConnectionResult(UdpReceiveResult res)
        {
            var message = Encoding.Default.GetString(res.Buffer);
            return message;
        }

    }
}
