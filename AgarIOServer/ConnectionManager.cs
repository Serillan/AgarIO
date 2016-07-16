using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIOServer
{
    class ConnectionManager
    {
        List<ClientConnection> connections { get; set; }

        public async Task StartListeningAsync()
        {
            connections = new List<ClientConnection>();

            while (true)
                connections.Add(await ClientConnection.AcceptClientAsync());
            Console.WriteLine("ending");
        }
    }
}
