using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIOServer
{
    class ConnectionManager
    {
        List<ClientConnection> Connections { get; set; }

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
                }
                ProcessClientAsync(newConnection);
            }
        }

        public async Task ProcessClientAsync(ClientConnection conn)
        {
            while (true)
            {
                var receiveTask = conn.ReceiveAsync();
                var task = await Task.WhenAny(receiveTask, Task.Delay(5000));
                if (task == receiveTask)
                {
                    var msg = receiveTask.Result;
                    Console.WriteLine("Player {0} sent: {1}", conn.PlayerName, msg);
                    // TODO - switch and process events
                    switch (msg)
                    {
                        case "STOP":
                            lock(Connections)
                                Connections.Remove(conn);
                            conn.Dispose();
                            return;
                            break;
                    }
                }
                else // timeout
                {
                    conn.SendAsync("STOP TIMEOUT");
                    Console.WriteLine("Stopping player {0} because of timeout!", conn.PlayerName);
                    conn.Dispose();
                    return;
                }
            }
        }

    }

}
