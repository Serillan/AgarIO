using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AgarIO
{

    public class ServerConnection : IDisposable
    {
        UdpClient UdpClient;
        UdpClient UdpServer;
        const int ClientPort = 11020;
        const int LoginServerPort = 11028;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="adress"></param>
        /// <param name="playerName"></param>
        /// <exception cref="TimeoutException">Throws exception if we cannot connect to the Server.</exception>
        /// <returns></returns>
        public static async Task<ServerConnection> ConnectAsync(IPAddress adress, string playerName)
        {
            ServerConnection conn = new ServerConnection();
            conn.UdpClient = new UdpClient(new IPEndPoint(IPAddress.Any, 0)); // listening
            conn.UdpServer = new UdpClient();
            conn.UdpServer.Connect(adress, LoginServerPort);                 // for writing to server
            Debug.WriteLine((conn.UdpClient.Client.LocalEndPoint as IPEndPoint).Port);

            Debug.WriteLine("CONNECTING");

            var task = conn.ReceiveAsync();
            string res;

            for (int i = 0; i < 50; i++)
            {
                await conn.SendAsync(String.Format("CONNECT {0} {1}", (conn.UdpClient.Client.LocalEndPoint as IPEndPoint).Port, playerName));

                if (await Task.WhenAny(task, Task.Delay(100)) == task)
                {
                    res = task.Result;
                    if (res.Split()[0] == "CONNECTED")
                    {
                        conn.UdpServer.Connect(adress, int.Parse(res.Split()[1]));
                        return conn;
                    }
                    else
                    {
                        Debug.WriteLine("error");
                    }
                }
            }
            conn.Dispose();
            throw new TimeoutException("Cannot connect to the server!");

        }

        public async Task SendAsync(string message)
        {
            var bytes = Encoding.Default.GetBytes(message);
            int x = await UdpServer.SendAsync(bytes, bytes.Length);
            return;
        }

        public async Task<string> ReceiveAsync()
        {
            var res = await UdpClient.ReceiveAsync();
            var message = Encoding.Default.GetString(res.Buffer);
            return message;

        }

        public async Task StartReceiving(Action<string> handler)
        {
            while (true)
            {
                var receiveTask = ReceiveAsync();
                if (await Task.WhenAny(receiveTask, Task.Delay(5000)) == receiveTask)
                {
                    handler(receiveTask.Result);
                }
                else
                    handler("STOP Server has stopped responding");
            }
        }

        public void Dispose()
        {
            UdpClient.Close();
            UdpServer.Close();
        }

        private ServerConnection() { }

    }
}
