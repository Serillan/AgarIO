using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AgarIO
{
    enum ConnectionResult
    {
        Connected, AlreadyUsedName, OtherError
    }

    class ServerConnection
    {
        UdpClient UdpClient;
        const int ListenPort = 11000;
        const int ServerPort = 11028;

        public static async Task<ServerConnection> ConnectAsync(IPAddress adress, string playerName)
        {
            ServerConnection conn = new ServerConnection();
            conn.UdpClient = new UdpClient();
            while(true)
            {
                conn.SendAsync("CONNECT " + playerName);
                var res = await conn.ReceiveAsync();
                // TODO ... if (res == ) ...
            }


            return conn;
        }

        public async Task SendAsync(string message)
        {
            var bytes = Encoding.Default.GetBytes(message);
            await UdpClient.SendAsync(bytes, bytes.Length);
        }

        public async Task<string> ReceiveAsync()
        {
            var res = await UdpClient.ReceiveAsync();
            var message = Encoding.Default.GetString(res.Buffer);
            return message;
        }

        private ServerConnection() { }

    }
}
