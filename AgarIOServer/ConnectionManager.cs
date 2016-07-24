using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProtoBuf;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace AgarIOServer
{
    class ConnectionManager
    {
        public List<ClientConnection> Connections { get; set; }
        public Action<string, Commands.Command> PlayerCommandHandler { get; set; }
        public Action<string> NewPlayerHandler { get; set; }

        public async Task StartListeningAsync()
        {
            Connections = new List<ClientConnection>();
            while (true)
            {
                ClientConnection newConnection = await ClientConnection.AcceptClientAsync(IsConnectionAllowed);
                lock (Connections)
                {
                    if (Connections.Any(c => c.PlayerName == newConnection.PlayerName))
                        continue;           // already connected (... multiple connect packets from client)

                    Console.WriteLine("Player {0} has succesfully connected!",
                       newConnection.PlayerName);
                    Connections.Add(newConnection);
                    NewPlayerHandler(newConnection.PlayerName);
                }
                ProcessClientAsync(newConnection);
            }
        }

        private async Task ProcessClientAsync(ClientConnection conn)
        {
            while (!conn.IsClosed)
            {
                var receiveTask = conn.ReceiveCommandAsync();
                var task = await Task.WhenAny(receiveTask, Task.Delay(50000));
                if (task == receiveTask)
                {
                    var command = receiveTask.Result;
                    //Console.WriteLine("Player {0} sent: {1}", conn.PlayerName, command.GetType());
                    PlayerCommandHandler(conn.PlayerName, command);
                }
                else // timeout
                {
                    conn.SendAsync(new Commands.Stop());
                    Console.WriteLine("Stopping player {0} because of timeout!", conn.PlayerName);
                    PlayerCommandHandler(conn.PlayerName, new Commands.Stop());
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

        public void SendToClient(string name, string msg)
        {
            ClientConnection conn = null;
            lock (Connections)
            {
                conn = Connections.Find(p => p.PlayerName == name);
            }
            conn.SendAsync(msg);
        }

        public void SendToClient(string name, byte[] data)
        {
            ClientConnection conn = null;
            lock (Connections)
            {
                conn = Connections.Find(p => p.PlayerName == name);
            }
            conn.SendAsync(data);
        }

        public void SendToClient(string name, Commands.Command command)
        {
            var stream = new MemoryStream();
            Serializer.Serialize(stream, command);
            stream.Seek(0, SeekOrigin.Begin);
            SendToClient(name, stream.ToArray());
        }

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

        public void EndClientConnection(string playerName)
        {
            lock (Connections)
            {
                var conn = Connections.Find(t => t.PlayerName == playerName);
                EndClientConnection(conn);
            }
            
        }

        private bool IsConnectionAllowed(string playerName, IPEndPoint playerEndPoint, out string outputMessage)
        {
            var nameAlreadyUsed = Connections.Exists(p => p.PlayerName == playerName);
            if (nameAlreadyUsed)
            {
                outputMessage = $"Name {playerName} is already being used by another player!";
                return false;
            }
            else // ALLOWED
            {
                outputMessage = "";
                return true;
            }
        }

        public void SendToAllClients(Commands.Command command)
        {
            var stream = new MemoryStream();
            Serializer.Serialize(stream, command);
            stream.Seek(0, SeekOrigin.Begin);
            SendToAllClients(stream.ToArray());
        }
    }

}
