using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AgarIO
{

    public class ServerConnection : IDisposable
    {
        UdpClient UdpClient;
        UdpClient UdpServer;
        const int ClientPort = 11020;
        const int ServerPort = 11028;

        public static async Task<ServerConnection> ConnectAsync(IPAddress adress, string playerName)
        {
            ServerConnection conn = new ServerConnection();
            conn.UdpClient = new UdpClient(new IPEndPoint(IPAddress.Any, ClientPort)); // listening
            conn.UdpServer = new UdpClient();
            conn.UdpServer.Connect(adress, ServerPort);    // for writing to server

            while(true)
            {
                
                await conn.SendAsync("CONNECT " + playerName);
                Debug.WriteLine("CONNECTING");

                var res = await conn.ReceiveAsync();
                if (res == "CONNECTED")
                {
                    Debug.WriteLine("Received");
                    return conn;
                }
                else
                {
                    Debug.WriteLine("error");
                }
            }
        }

        public async Task SendAsync(string message)
        {
            var bytes = Encoding.Default.GetBytes(message);
            int x = await UdpServer.SendAsync(bytes, bytes.Length);
            return;
        }

        public async Task<string> ReceiveAsync()
        {
            //IPEndPoint ep = new IPEndPoint(IPAddress.Loopback, ServerPort);
            //var res = UdpClient.Receive(ref ep);
            var ress = UdpClient.ReceiveAsync();
            UdpReceiveResult res;
            
            res = ress.Result;
            Debug.WriteLine("Received");
            var message = Encoding.Default.GetString(res.Buffer);
            return message;
        }

        public void Dispose()
        {
            UdpClient.Close();
            UdpServer.Close();
        }

        private ServerConnection() { }

    }
}
