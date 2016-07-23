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
        public Action<string, string> PlayerMessageHandler { get; set; }

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
                    PlayerMessageHandler(newConnection.PlayerName, "CONNECTED");
                }
                ProcessClientAsync(newConnection);
            }
        }

        private async Task ProcessClientAsync(ClientConnection conn)
        {
            while (!conn.IsClosed)
            {
                var receiveTask = conn.ReceiveAsync();
                var task = await Task.WhenAny(receiveTask, Task.Delay(5000));
                if (task == receiveTask)
                {
                    var msg = receiveTask.Result;
                    //Console.WriteLine("Player {0} sent: {1}", conn.PlayerName, msg);
                    PlayerMessageHandler(conn.PlayerName, msg);
                }
                else // timeout
                {
                    conn.SendAsync("STOP TIMEOUT");
                    Console.WriteLine("Stopping player {0} because of timeout!", conn.PlayerName);
                    PlayerMessageHandler(conn.PlayerName, "STOP");
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
                    client.SendBinaryAsync(data);
                }
            }
        }

        public void SendToClient(string name, string msg)
        {
            ClientConnection conn = null;
            lock (Connections)
                conn = Connections.Find(p => p.PlayerName == name);
            conn.SendAsync(msg);
        }

        public void EndClientConnection(ClientConnection client)
        {
            client.IsClosed = true;
            lock (Connections)
                Connections.Remove(client);
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

        // TODO : state size is higher than 500B -> fragmentation needed!
        public void SendToAllClients(GameState state)
        {
            var command = new Commands.Move();
            //var command = new Commands.UpdateState(state);
            //var message = new Commands.CommandMessage(command);
            var stream = new MemoryStream();

            Serializer.Serialize(stream, command);
            stream.Seek(0, SeekOrigin.Begin);

            var commandD = Serializer.Deserialize<Commands.Command>(stream);
            Console.WriteLine(commandD.GetType());

            SendToAllClients(stream.ToArray());
        }
    }

}
