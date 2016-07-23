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

        private LoginServer(IPEndPoint ip) : base(ip) { }

        public static LoginServer GetNewInstance()
        {
            var server = new LoginServer(new IPEndPoint(IPAddress.Any, LoginServerPort));
            return server;
        }

        public async Task SendAsync(string message)
        {
            var bytes = Encoding.Default.GetBytes(message);
            //server.Send(bytes, bytes.Length);
            await SendAsync(bytes, bytes.Length);
        }

    }

    class ClientConnection : IDisposable
    {
        public string PlayerName { get; set; }
        public int LastMovementTime { get; set; }
        public Boolean IsClosed { get; set; }

        UdpClient UdpClient;

        public delegate bool ClientAuthorizer(string playerName, IPEndPoint endPoint, out string outputMessage);

        public static async Task<ClientConnection> AcceptClientAsync(ClientAuthorizer clientAuthorizer)
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

                    if (message.Split().Length == 2 && message.Split()[0] == "CONNECT")
                    {
                        var tokens = message.Split();
                        var name = tokens[1];
                        string authorizerOutputMessage;

                        if (clientAuthorizer(name, connectionResult.RemoteEndPoint, out authorizerOutputMessage))
                        {
                            loginServer.Connect(connectionResult.RemoteEndPoint);
                            conn.PlayerName = tokens[1];
                            conn.LastMovementTime = 0;
                            break;
                        }
                        else
                        {
                            loginServer.Connect(connectionResult.RemoteEndPoint);
                            Console.WriteLine("Connection not allowed for {0}:{1} with desired name {2}",
                                connectionResult.RemoteEndPoint.Address, connectionResult.RemoteEndPoint.Port, name);
                            await loginServer.SendAsync($"ERROR {authorizerOutputMessage}");
                            loginServer.Dispose();
                            loginServer = LoginServer.GetNewInstance();
                        }
                    }
                }

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
                            //Console.WriteLine($"{connectionResult.Result.RemoteEndPoint.Address}:{connectionResult.Result.RemoteEndPoint.Port} has connected!");
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
            //UdpClient.Send(bytes, bytes.Length);
            await UdpClient.SendAsync(bytes, bytes.Length);
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
            if (UdpClient != null)
                UdpClient.Close();
        }

        public static string GetMessageFromConnectionResult(UdpReceiveResult res)
        {
            var message = Encoding.Default.GetString(res.Buffer);
            return message;
        }

    }
}