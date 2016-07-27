using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;
using System.IO;

namespace AgarIO
{

    class ServerConnection : IDisposable
    {
        UdpClient UdpServer;
        const int LoginServerPort = 11028;
        bool IsClosed;

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
            conn.UdpServer = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
            conn.UdpServer.Connect(adress, LoginServerPort);                 // for writing to server
            conn.IsClosed = false;

            Debug.WriteLine("CONNECTING");
            var task = conn.ReceiveAsync();
            string res;

            for (int i = 0; i < 5; i++)
            {
                await conn.SendAsync(String.Format("CONNECT {0}", playerName)); // TODO - name with whitespace

                if (await Task.WhenAny(task, Task.Delay(100000)) == task)
                {
                    res = task.Result;
                    if (res.Split()[0] == "CONNECTED")
                    {
                        conn.UdpServer.Connect(adress, int.Parse(res.Split()[1]));
                        for (int j = 0; j < 3; j++)  // TODO 3 should be there
                            conn.SendAsync("ACK");
                        return conn;
                    }
                    if (res.Split()[0] == "ERROR")
                    {
                        conn.Dispose();
                        throw new ArgumentException(res.Substring(6).Trim());
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
        }

        public async Task<string> ReceiveAsync()
        {
            var res = await UdpServer.ReceiveAsync();
            var message = Encoding.Default.GetString(res.Buffer);
            return message;

        }

        public async Task<Commands.Command> ReceiveCommandAsync()
        {
            while (true)
            {
                var data = await ReceiveBinaryAsync();
                var stream = new MemoryStream(data);
                try
                {
                    return Serializer.Deserialize<Commands.Command>(stream);
                }
                catch (ProtoException)
                {
                    // ignore this type of exception (multiple ACK ...), wait for first command
                }
            }
        }

        public async Task<byte[]> ReceiveBinaryAsync()
        {
            return (await UdpServer.ReceiveAsync()).Buffer;
        }

        public async Task SendAsync(byte[] data)
        {
            await UdpServer.SendAsync(data, data.Length);
        }

        public async Task SendAsync(Commands.Command command)
        {
            var stream = new MemoryStream();
            Serializer.Serialize(stream, command);
            stream.Seek(0, SeekOrigin.Begin);
            var res = SendAsync(stream.ToArray());
            await res;

            //Debug.WriteLine("Sending {0}", command.GetType());

            if (res.Exception != null)
                Debug.WriteLine(res.Exception);
        }

        public async Task StartReceiving(Action<Commands.Command> handler)
        {
            while (true)
            {
                if (IsClosed)
                    break;
                var receiveTask = ReceiveCommandAsync();
                if (await Task.WhenAny(receiveTask, Task.Delay(500000)) == receiveTask)
                {
                    handler(receiveTask.Result);
                }
                else
                    handler(new Commands.Stop("Server has stopped responding"));
            }
        }

        public void Dispose()
        {
            UdpServer.Dispose();
            IsClosed = true;
        }

        private ServerConnection() { }

    }
}
