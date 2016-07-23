using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgarIOServer.Entities;

namespace AgarIOServer.Commands
{
    [ProtoBuf.ProtoContract]
    class Move : Command
    {
        /// <summary>
        /// Specifies where player parts should be moved.
        /// First argument of the tuple is part identifier and other two arguments are new coordinates.
        /// </summary>
        [ProtoBuf.ProtoMember(1)]
        public List<Tuple<int, float, float>> Movement { get; set; }

        [ProtoBuf.ProtoMember(2)]
        public int Time { get; set; }

        public override void Process(GameServer server, string playerName)
        {
            var state = server.GameState;
            var player = state.Players.Find(p => p.Name == playerName);
            if (Time < player.LastMovementTime)
                return;

            foreach (var part in player.Parts)
            {
                var movement = Movement.Find(t => t.Item1 == part.Identifier); // TODO if there isn't such a movement
                part.X = movement.Item2;
                part.Y = movement.Item3;
            }

        }
    }
}
