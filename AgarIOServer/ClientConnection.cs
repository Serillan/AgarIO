using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace AgarIOServer
{
    class LoginServer : UdpClient
    {
        public const int LoginServerPort = 11028;
        public const int ClientPort = 11020;

        private static LoginServer server;

        private LoginServer(IPEndPoint ip) : base (ip) { }

        public static LoginServer GetInstance()
        {
            if (server == null)
                server = new LoginServer(new IPEndPoint(IPAddress.Any, LoginServerPort));
            return server;
        }
    }

    class ClientConnection : IDisposable
    {
        public string PlayerName { get; set; }

        UdpClient UdpClient;
        UdpClient UdpListener;

        public static async Task<ClientConnection> AcceptClientAsync()
        {
            ClientConnection conn = new ClientConnection();
            conn.UdpClient = new UdpClient();

            while (true)
            {
                var connectionResult = await LoginServer.GetInstance().ReceiveAsync();
                var message = GetMessageFromConnectionResult(connectionResult);
                int port = -1;

                if (message.Split().Length == 3 && message.Split()[0] == "CONNECT")
                {
                    // TODO check whether the name is already in use
                    var tokens = message.Split();
                    if (!int.TryParse(tokens[1], out port))
                        continue;
                    
                    conn.UdpClient.Connect(connectionResult.RemoteEndPoint.Address, port);
                    conn.PlayerName = tokens[2];
                    conn.UdpListener = new UdpClient(new IPEndPoint(connectionResult.RemoteEndPoint.Address, 0));
                }
                else
                    continue;


                //Console.WriteLine("Player {0} with IP Adress {1}:{2} has succesfully connected!",
                //    conn.PlayerName, connectionResult.RemoteEndPoint.Address, port);

                conn.SendAsync("CONNECTED " + (conn.UdpListener.Client.LocalEndPoint as IPEndPoint).Port);

                return conn;
            }
        }

        public async Task SendAsync(string message)
        {
            var bytes = Encoding.Default.GetBytes(message);
            await UdpClient.SendAsync(bytes, bytes.Length);
        }

        public async Task<string> ReceiveAsync()
        {
            var res = await UdpListener.ReceiveAsync();
            return GetMessageFromConnectionResult(res);
        }

        public void Dispose()
        {
            UdpClient.Close();
            UdpListener.Close();
        }

        public static string GetMessageFromConnectionResult(UdpReceiveResult res)
        {
            var message = Encoding.Default.GetString(res.Buffer);
            return message;
        }


    }
}