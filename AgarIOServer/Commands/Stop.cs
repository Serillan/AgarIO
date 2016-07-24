using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIOServer.Commands
{
    [ProtoBuf.ProtoContract]
    class Stop : Command
    {
        [ProtoBuf.ProtoMember(1)]
        public string StopMessage { get; set; }

        public override void Process(GameServer gameServer, string playerName)
        {
            /*
            lock (gameServer.GameState)
                gameServer.GameState.Players.RemoveAll(p => p.Name == playerName);
            gameServer.ConnectionManager.EndClientConnection(playerName);
            */
            gameServer.RemovePlayer(playerName, StopMessage);
        }

        public Stop() { }

        public Stop(string msg)
        {
            StopMessage = msg;
        }

    }
}
