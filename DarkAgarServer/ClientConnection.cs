using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using ProtoBuf;
using System.IO;

namespace DarkAgarServer
{

    /// <summary>
    /// Login Server that controls the connecting of players to the game server.
    /// </summary>
    /// <seealso cref="System.Net.Sockets.UdpClient" />
    class LoginServer : UdpClient
    {
        /// <summary>
        /// The login server port
        /// </summary>
        public const int LoginServerPort = 11028;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginServer"/> class.
        /// </summary>
        /// <param name="ipEndPoint">The ip end point.</param>
        private LoginServer(IPEndPoint ipEndPoint) : base(ipEndPoint) { }

        /// <summary>
        /// Gets the new instance.
        /// </summary>
        /// <returns>LoginServer.</returns>
        public static LoginServer GetNewInstance()
        {
            var server = new LoginServer(new IPEndPoint(IPAddress.Any, LoginServerPort));
            return server;
        }

        /// <summary>
        /// Sends the message to the client.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>Task.</returns>
        public async Task SendAsync(string message)
        {
            var bytes = Encoding.Default.GetBytes(message);
            await SendAsync(bytes, bytes.Length);
        }
    }

    /// <summary>
    /// Controls the client connection.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    class ClientConnection : IDisposable
    {
        /// <summary>
        /// Gets or sets the name of the player.
        /// </summary>
        /// <value>The name of the player.</value>
        public string PlayerName { get; set; }

        /// <summary>
        /// Gets or sets the last movement time.
        /// </summary>
        /// <value>The last movement time.</value>
        public int LastMovementTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is closed.
        /// </summary>
        /// <value><c>true</c> if this instance is closed; otherwise, <c>false</c>.</value>
        public bool IsClosed { get; set; }

        /// <summary>
        /// Gets or sets the UDP client.
        /// </summary>
        /// <value>The UDP client.</value>
        UdpClient UdpClient { get; set; }

        /// <summary>
        /// Delegate that is called for Authorizing new connected client.
        /// </summary>
        /// <param name="playerName">Name of the player.</param>
        /// <param name="endPoint">The player end point.</param>
        /// <param name="outputMessage">The output message.</param>
        /// <returns><c>true</c> if client is authorized to connect, <c>false</c> otherwise.</returns>
        public delegate bool ClientAuthorizer(string playerName, IPEndPoint endPoint, out string outputMessage);

        /// <summary>
        /// Accepts a client.
        /// </summary>
        /// <param name="clientAuthorizer">The client authorizer.</param>
        /// <returns>Task&lt;ClientConnection&gt;.</returns>
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
                    var message = GetMessageFromUdpReceiveResult(connectionResult);

                    if (message.Split().Length >= 2 && message.Split()[0] == "CONNECT")
                    {
                        var tokens = message.Split();
                        var name = message.Substring(8);

                        string authorizerOutputMessage;

                        if (clientAuthorizer(name, connectionResult.RemoteEndPoint, out authorizerOutputMessage))
                        {
                            loginServer.Connect(connectionResult.RemoteEndPoint);
                            conn.PlayerName = name;
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
                        var message = GetMessageFromUdpReceiveResult(connectionResult.Result);

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

        /// <summary>
        /// Sends the message to the client.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>Task.</returns>
        public async Task SendAsync(string message)
        {
            var bytes = Encoding.Default.GetBytes(message);
            await UdpClient.SendAsync(bytes, bytes.Length);
        }

        /// <summary>
        /// Sends the data to the client.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Task.</returns>
        public async Task SendAsync(byte[] data)
        {
            await UdpClient.SendAsync(data, data.Length);
        }

        /// <summary>
        /// Sends the command to the client.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>Task.</returns>
        public async Task SendAsync(Commands.Command command)
        {
            var stream = new MemoryStream();
            Serializer.Serialize(stream, command);
            stream.Seek(0, SeekOrigin.Begin);
            await SendAsync(stream.ToArray());
        }

        /// <summary>
        /// Receives message from the client.
        /// </summary>
        /// <returns>Task&lt;System.String&gt;.</returns>
        public async Task<string> ReceiveAsync()
        {
            var res = await UdpClient.ReceiveAsync();
            return GetMessageFromUdpReceiveResult(res);
        }

        /// <summary>
        /// Receives data from the client.
        /// </summary>
        /// <returns>Task&lt;System.Byte[]&gt;.</returns>
        public async Task<byte[]> ReceiveBinaryAsync()
        {
            byte[] res = null;
            res = (await UdpClient.ReceiveAsync()).Buffer;
            return res;
        }

        /// <summary>
        /// Receives command from the client.
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
                    var command = Serializer.Deserialize<Commands.Command>(stream);
                   // Console.WriteLine("Received {0} command from {1}", command.GetType(), PlayerName);
                    return command;
                }
                catch (ProtoBuf.ProtoException ex)
                {
                    // ignore this type of exception (multiple ACK ...), wait for first command
                    //Console.WriteLine(ex.ToString());
                }
            }
        }

        /// <summary>
        /// Closes and disposes current server connection.
        /// </summary>
        public void Dispose()
        {
            if (UdpClient != null)
                UdpClient.Dispose();
        }

        /// <summary>
        /// Gets the message from the specified UDP receive result.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns>System.String.</returns>
        public static string GetMessageFromUdpReceiveResult(UdpReceiveResult result)
        {
            var message = Encoding.Default.GetString(result.Buffer);
            return message;
        }

    }
}