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

namespace DarkAgar
{

    /// <summary>
    /// Controls the server connection.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    class ServerConnection : IDisposable
    {
        /// <summary>
        /// The UDP server.
        /// </summary>
        private UdpClient UdpServer;
        
        /// <summary>
        /// The login server port.
        /// </summary>
        private const int LoginServerPort = 11028;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is closed.
        /// </summary>
        /// <value><c>true</c> if this instance is closed; otherwise, <c>false</c>.</value>
        private bool IsClosed { get; set; }

        /// <summary>
        /// Connects to the server.
        /// </summary>
        /// <param name="address">The server address.</param>
        /// <param name="playerName">Name of the player.</param>
        /// <returns>Task&lt;ServerConnection&gt;.</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="TimeoutException">Throws exception if we cannot connect to the Server.</exception>
        public static async Task<ServerConnection> ConnectAsync(IPAddress address, string playerName)
        {
            var conn = new ServerConnection();
            conn.UdpServer = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
            conn.UdpServer.Connect(address, LoginServerPort);                 // for writing to server
            conn.IsClosed = false;

            Debug.WriteLine("CONNECTING");
            var task = conn.ReceiveAsync();
            string res;

            for (int i = 0; i < 5; i++)
            {
                await conn.SendAsync($"CONNECT {playerName}");

                if (await Task.WhenAny(task, Task.Delay(1000)) == task)
                {
                    res = task.Result;
                    switch (res.Split()[0])
                    {
                        case "CONNECTED":
                            conn.UdpServer.Connect(address, int.Parse(res.Split()[1]));
                            for (int j = 0; j < 3; j++)
                                conn.SendAsync("ACK");
                            return conn;
                        case "ERROR":
                            conn.Dispose();
                            throw new ArgumentException(res.Substring(6).Trim());
                    }
                }
            }

            conn.Dispose();
            throw new TimeoutException("Cannot connect to the server!");

        }

        /// <summary>
        /// Starts the receiving.
        /// </summary>
        /// <param name="handler">The handler that will handle received commands.</param>
        /// <returns>Task.</returns>
        public async Task StartReceiving(Action<Commands.Command> handler)
        {
            while (true)
            {
                if (IsClosed)
                    break;
                var receiveTask = ReceiveCommandAsync();
                if (await Task.WhenAny(receiveTask, Task.Delay(5000)) == receiveTask)
                {
                    handler(receiveTask.Result);
                }
                else
                    handler(new Commands.Stop("Server has stopped responding"));
            }
        }

        /// <summary>
        /// Send binary data to the server.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Task.</returns>
        public async Task SendAsync(byte[] data)
        {
            await UdpServer.SendAsync(data, data.Length);
        }

        /// <summary>
        /// Sends command to the server.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>Task.</returns>
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

        /// <summary>
        /// Send message to the server.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>Task.</returns>
        public async Task SendAsync(string message)
        {
            var bytes = Encoding.Default.GetBytes(message);
            int x = await UdpServer.SendAsync(bytes, bytes.Length);
        }

        /// <summary>
        /// Receives binary data from the server.
        /// </summary>
        /// <returns>Task&lt;System.Byte[]&gt;.</returns>
        public async Task<byte[]> ReceiveBinaryAsync()
        {
            return (await UdpServer.ReceiveAsync()).Buffer;
        }

        /// <summary>
        /// Receives command from the server.
        /// </summary>
        /// <returns>Task&lt;Commands.Command&gt;.</returns>
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

        /// <summary>
        /// Receives message from the server.
        /// </summary>
        /// <returns>Task&lt;System.String&gt;.</returns>
        public async Task<string> ReceiveAsync()
        {
            var res = await ReceiveBinaryAsync();
            var message = Encoding.Default.GetString(res);
            return message;
        }

        /// <summary>
        /// Closes and disposes current server connection.
        /// </summary>
        public void Dispose()
        {
            UdpServer.Dispose();
            IsClosed = true;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="ServerConnection"/> class from being created.
        /// </summary>
        private ServerConnection() { }
    }
}
