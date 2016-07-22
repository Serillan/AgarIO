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

        private static LoginServer server;

        private LoginServer(IPEndPoint ip) : base(ip) { }

        public static LoginServer GetNewInstance()
        {
            server = new LoginServer(new IPEndPoint(IPAddress.Any, LoginServerPort));
            return server;
        }

        public async Task SendAsync(string message)
        {
            var bytes = Encoding.Default.GetBytes(message);
            server.Send(bytes, bytes.Length);
            //await UdpClient.SendAsync(bytes, bytes.Length);
        }

    }

    class ClientConnection : IDisposable
    {
        public string PlayerName { get; set; }
        public int LastMovementTime { get; set; }
        public Boolean IsClosed { get; set; }

        UdpClient UdpClient;

        public static async Task<ClientConnection> AcceptClientAsync()
        {
            ClientConnection conn = new ClientConnection();
            var loginServer = LoginServer.GetNewInstance();
            conn.IsClosed = false;
            conn.UdpClient = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
            while (true)
            {
                while (true)
                {
                    var connectionResult = await loginServer.ReceiveAsync();
                    var message = GetMessageFromConnectionResult(connectionResult);
                    Console.WriteLine(message.Split().Length);
                    Console.WriteLine(message.Split()[0]);
                    Console.WriteLine(message.Split()[0] == "CONNECT");
                    int port = -1;

                    if (message.Split().Length == 2 && message.Split()[0] == "CONNECT")
                    {
                        // TODO check whether the name is already in use
                        var tokens = message.Split();

                        Console.WriteLine($"{connectionResult.RemoteEndPoint.Address}:{connectionResult.RemoteEndPoint.Port}");
                        loginServer.Connect(connectionResult.RemoteEndPoint);
                        conn.PlayerName = tokens[1];
                        conn.LastMovementTime = 0;
                        Console.WriteLine("OK");
                        break;
                    }
                }

                //Console.WriteLine("Player {0} with IP Adress {1}:{2} has succesfully connected!",
                //    conn.PlayerName, connectionResult.RemoteEndPoint.Address, port);
                for (int i = 0; i < 3; i++)
                {
                    loginServer.SendAsync("CONNECTED " + (conn.UdpClient.Client.LocalEndPoint as IPEndPoint).Port);
                    var connectionResult = conn.UdpClient.ReceiveAsync();
                    if (connectionResult == await Task.WhenAny(Task.Delay(1000), connectionResult))
                    {
                        var message = GetMessageFromConnectionResult(connectionResult.Result);

                        if (message == "ACK")
                        {
                            conn.UdpClient.Connect(connectionResult.Result.RemoteEndPoint);
                            loginServer.Dispose();
                            return conn;
                        }
                    }
                }
            }
        }

        public async Task SendAsync(string message)
        {
            var bytes = Encoding.Default.GetBytes(message);
            UdpClient.Send(bytes, bytes.Length);
            //await UdpClient.SendAsync(bytes, bytes.Length);
        }

        public async Task<string> ReceiveAsync()
        {
            var res = await UdpClient.ReceiveAsync();
            return GetMessageFromConnectionResult(res);
        }

        public async Task<byte[]> ReceiveBinaryAsync()
        {
            return (await UdpClient.ReceiveAsync()).Buffer;
        }

        public async Task SendBinaryAsync(byte[] data)
        {
            await UdpClient.SendAsync(data, data.Length);
        }

        public void Dispose()
        {
            UdpClient.Close();
        }

        public static string GetMessageFromConnectionResult(UdpReceiveResult res)
        {
            var message = Encoding.Default.GetString(res.Buffer);
            return message;
        }

    }
}