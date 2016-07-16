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
                Connections.Add(newConnection);
                ProcessClientAsync(newConnection);
            }
        }

        public async Task ProcessClientAsync(ClientConnection conn)
        {
            while (true)
            {
                var msg = await conn.ReceiveAsync();
                Console.WriteLine("Player {0} sent: {1}", conn.PlayerName, msg);
                // TODO - switch and process events
            }
        }

    }

}
