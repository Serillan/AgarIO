using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProtoBuf;
using System.Text;
using System.Threading.Tasks;

namespace AgarIOServer
{
    class ConnectionManager
    {
        List<ClientConnection> Connections { get; set; }
        public Action<string, string> PlayerMessageHandler { get; set; }

        public async Task StartListeningAsync()
        {
            Connections = new List<ClientConnection>();
            while (true)
            {
                ClientConnection newConnection = await ClientConnection.AcceptClientAsync();
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
                    Console.WriteLine("Player {0} sent: {1}", conn.PlayerName, msg);
                    PlayerMessageHandler(conn.PlayerName, msg);
                }
                else // timeout
                {
                    conn.SendAsync("STOP TIMEOUT");
                    PlayerMessageHandler(conn.PlayerName, "STOP");
                    Console.WriteLine("Stopping player {0} because of timeout!", conn.PlayerName);
                    EndClientConnection(conn);
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

        public void EndClientConnection(ClientConnection client)
        {
            lock (Connections)
                Connections.Remove(client);
            client.IsClosed = true;
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

        public async void SendToAllClients(GameState state)
        {
            var stream = new MemoryStream();
            Serializer.Serialize(stream, state);
            //Console.WriteLine("Sending state with size : {0}B", stream.ToArray().Length);
            stream.Seek(0, SeekOrigin.Begin);
            var s = Serializer.Deserialize<GameState>(stream);

            SendToAllClients(stream.ToArray());
        }
    }

}
