using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgarIOServer.Entities;

namespace AgarIOServer.Commands
{
    [ProtoBuf.ProtoContract]
    class Eject : Command
    {
        [ProtoBuf.ProtoMember(1)]
        public float X { get; set; }

        [ProtoBuf.ProtoMember(2)]
        public float Y { get; set; }

        public override void Process(GameServer game, string playerName)
        {
            Player player = null;
            lock (game.GameState.Players)
            {
                player = game.GameState.Players.Find(p => p.Name == playerName);
            }

            lock (player)
            {

                List<PlayerPart> newParts = new List<PlayerPart>();
                byte i = 0;

                var nearestPart = (from part in player.Parts
                                   where part.Mass > 200
                                   orderby Distance(X, Y, part) ascending
                                   select part).FirstOrDefault();

                if (nearestPart == null)
                    return;


                var freeIdentifier = Enumerable.Range(0, 40).First(n => !player.Parts.Exists(p => p.Identifier == n));

                nearestPart.Mass -= 50;
                player.Parts.Add(new PlayerPart() {
                    Mass = 50,
                    DivisionTime = PlayerPart.DefaulDivisionTime,
                    IsBeingEjected = true,
                    Identifier = (byte)freeIdentifier,
                    X = nearestPart.X,
                    Y = nearestPart.Y,
                    EjectedVX = this.X - nearestPart.X,
                    EjectedVY = this.Y - nearestPart.Y,
                    MergeTime = 100000,
                    IsNewDividedPart = true
                });
            }
            game.ConnectionManager.SendToClient(player.Name, new Invalidate("Ejection"));
        }

        public float Distance(float x, float y, PlayerPart part)
        {
            var dx = x - part.X;
            var dy = y - part.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
