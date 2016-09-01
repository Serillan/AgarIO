using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProtoBuf;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using DarkAgarServer.Commands;

namespace DarkAgarServer
{
    /// <summary>
    /// Connection Manager class that is responsible for controlling all the client connections.
    /// </summary>
    class ConnectionManager
    {
        /// <summary>
        /// Gets or sets the connections.
        /// </summary>
        /// <value>The connections.</value>
        public List<ClientConnection> Connections { get; set; }

        /// <summary>
        /// Gets or sets the player command handler that is called
        /// every time when the command is received from the client.
        /// </summary>
        /// <value>The player command handler.</value>
        public Action<string, Command> PlayerCommandHandler { get; set; }

        /// <summary>
        /// Gets or sets the new player handler.
        /// It is called every time when the new player is successfully connected.
        /// </summary>
        /// <value>The new player handler.</value>
        public Action<string> NewPlayerHandler { get; set; }

        /// <summary>
        /// Starts listening for clients.
        /// </summary>
        /// <returns>Task.</returns>
        public async Task StartListeningAsync()
        {
            Connections = new List<ClientConnection>();
            while (true)
            {
                var newConnection = await ClientConnection.AcceptClientAsync(IsConnectionAllowed);
                lock (Connections)
                {
                    if (Connections.Any(c => c.PlayerName == newConnection.PlayerName))
                        continue;           // already connected (... multiple connect packets from client)

                    Console.WriteLine($"Player {newConnection.PlayerName} has successfully connected!");
                    Connections.Add(newConnection);
                    NewPlayerHandler(newConnection.PlayerName);
                }
                ProcessClientAsync(newConnection);
            }
        }

        /// <summary>
        /// Processes the connected client. When the command is received
        /// it calls the PlayerCommandHandler.
        /// </summary>
        /// <param name="clientConnection">The client connection.</param>
        /// <returns>Task.</returns>
        private async Task ProcessClientAsync(ClientConnection clientConnection)
        {
            while (!clientConnection.IsClosed)
            {
                var receiveTask = clientConnection.ReceiveCommandAsync();
                var task = await Task.WhenAny(receiveTask, Task.Delay(5000));
                if (task == receiveTask)
                {
                    var command = receiveTask.Result;
                    Console.WriteLine("Player {0} sent: {1}", clientConnection.PlayerName, command.GetType());
                    PlayerCommandHandler(clientConnection.PlayerName, command);
                }
                else // timeout
                {
                    if (clientConnection.IsClosed)
                        return;

                    clientConnection.SendAsync(new Commands.Stop());
                    Console.WriteLine($"Stopping player {clientConnection.PlayerName} because of timeout!");
                    PlayerCommandHandler(clientConnection.PlayerName, new Commands.Stop());
                    return;
                }
            }
        }

        /// <summary>
        /// Asynchronously sends string message to all connected clients.
        /// </summary>
        /// <param name="message">Message to be sent to clients.</param>
        public void SendToAllClients(string message)
        {
            lock (Connections)
            {
                foreach (var client in Connections)
                {
                    client.SendAsync(message);
                }
            }
        }

        /// <summary>
        /// Asynchronously sends binary data to all connected clients.
        /// </summary>
        /// <param name="data">Binary data to be sent to clients.</param>
        public void SendToAllClients(byte[] data)
        {
            lock (Connections)
            {
                foreach (var client in Connections)
                {
                    client.SendAsync(data);
                }
            }
        }

        /// <summary>
        /// Sends the specified <paramref name="message"/> to the client with the specified
        /// <paramref name="playerName"/>.
        /// </summary>
        /// <param name="playerName">Name of the player.</param>
        /// <param name="message">The message.</param>
        public void SendToClient(string playerName, string message)
        {
            ClientConnection conn = null;
            lock (Connections)
            {
                conn = Connections.Find(p => p.PlayerName == playerName);
            }
            conn.SendAsync(message);
        }

        /// <summary>
        /// Asynchronously sends the specified <paramref name="data"/> to the client with the specified
        /// <paramref name="playerName"/>.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Task.</returns>
        public void SendToClient(string name, byte[] data)
        {
            ClientConnection conn = null;
            lock (Connections)
            {
                conn = Connections.FirstOrDefault(p => p.PlayerName == name);
            }
            if (conn != null)
                conn.SendAsync(data);
        }

        /// <summary>
        /// Asynchronously sends the specified <paramref name="command"/> to the client with the specified
        /// <paramref name="playerName"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="command">The command.</param>
        public void SendToClient(string name, Command command)
        {
            //Console.WriteLine("Sending {0} to client {1}", command.GetType().ToString(), name);
            var stream = new MemoryStream();
            Serializer.Serialize(stream, command);
            stream.Seek(0, SeekOrigin.Begin);
            SendToClient(name, stream.ToArray());
        }

        /// <summary>
        /// Ends the client connection.
        /// </summary>
        /// <param name="client">The client.</param>
        public void EndClientConnection(ClientConnection client)
        {
            client.IsClosed = true;
            lock (Connections)
            {
                Connections.Remove(client);
            }
            Console.WriteLine($"Stopping player {client.PlayerName}!");
            client.Dispose();
        }

        /// <summary>
        /// Ends the client connection specified by the specified <paramref name="playerName"/>.
        /// </summary>
        /// <param name="playerName">Name of the player from the connection.</param>
        public void EndClientConnection(string playerName)
        {
            lock (Connections)
            {
                var conn = Connections.Find(t => t.PlayerName == playerName);
                EndClientConnection(conn);
            }
        }

        /// <summary>
        /// Determines whether connection for the client with the desired player name is allowed.
        /// </summary>
        /// <param name="playerName">Name of the player.</param>
        /// <param name="playerEndPoint">The player end point.</param>
        /// <param name="outputMessage">The output message.</param>
        /// <returns><c>true</c> if connection is allowed for the client with the desired player name; otherwise, <c>false</c>.</returns>
        private bool IsConnectionAllowed(string playerName, IPEndPoint playerEndPoint, out string outputMessage)
        {
            var isNameAlreadyUsed = false;

            lock (Connections)
            {
                isNameAlreadyUsed = Connections.Exists(p => p.PlayerName == playerName);
            }

            if (playerName.Length > 20) // maximum player name length
            {
                outputMessage = $"Name is too long! Maximum allowed name length is {20}.";
                return false;
            }

            if (isNameAlreadyUsed)
            {
                outputMessage = $"Name {playerName} is already being used by another player!";
                return false;
            }

            // ALLOWED
            outputMessage = "";
            return true;
        }

        /// <summary>
        /// Sends the specified <paramref name="command"/> to all connected clients.
        /// </summary>
        /// <param name="command">The command.</param>
        public void SendToAllClients(Command command)
        {
            var stream = new MemoryStream();
            Serializer.Serialize(stream, command);
            stream.Seek(0, SeekOrigin.Begin);
            SendToAllClients(stream.ToArray());
        }
    }

}
