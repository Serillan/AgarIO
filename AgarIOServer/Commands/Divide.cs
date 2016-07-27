using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgarIOServer.Entities;

namespace AgarIOServer.Commands
{
    [ProtoBuf.ProtoContract]
    class Divide : Command
    {
        public override void Process(GameServer game, string playerName)
        {
            Player player = null;
            lock (game.GameState.Players)
            {
                player = game.GameState.Players.Find(p => p.Name == playerName);
            }

            lock (player)
            { 
                if (player.Parts.Count > 16)
                    return;

                List<PlayerPart> newParts = new List<PlayerPart>();
                var freeIdentifiers = Enumerable.Range(0, 40).Where(n => !player.Parts.Exists(p => p.Identifier == n)).ToList();
                var i = 0;

                foreach (var part in player.Parts)
                {
                    if (part.Mass >= 200)
                    {
                        newParts.Add(new PlayerPart()
                        {
                            DivisionTime = 0,
                            Identifier = (byte)freeIdentifiers[i++],
                            IsNewDividedPart = false,
                            Mass = part.Mass / 2,
                            X = part.X,
                            Y = part.Y,
                            MergeTime = (int)Math.Round((0.02 * (part.Mass / 2) + 5) * 1000 / GameServer.GameLoopInterval),
                        });

                        newParts.Add(new PlayerPart()
                        {
                            DivisionTime = PlayerPart.DefaulDivisionTime,
                            Identifier = (byte)freeIdentifiers[i++],
                            IsNewDividedPart = true,
                            Mass = part.Mass / 2,
                            X = part.X,
                            Y = part.Y,
                            MergeTime = (int)Math.Round((0.02 * (part.Mass / 2) + 5) * 1000 / GameServer.GameLoopInterval),
                        });

                    }
                    else
                        newParts.Add(part);
                }
                if (player.Parts.Count != newParts.Count)
                {
                    player.Parts = newParts;
                    game.ConnectionManager.SendToClient(playerName, new Invalidate("Division"));
                }
            }
        }
    }
}
