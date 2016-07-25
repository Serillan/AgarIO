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

            if (player.Parts.Count > 16)
                return;

            List<PlayerPart> newParts = new List<PlayerPart>();
            byte i = 0;
            foreach (var part in player.Parts)
            {
                if (part.Mass > 20)
                {
                    newParts.Add(new PlayerPart()
                    {
                        DivisionTime = 0,
                        Identifier = i++,
                        IsOutOfOtherParts = false,
                        Mass = part.Mass / 2,
                        X = part.X,
                        Y = part.Y,
                        MergeTime = (int)Math.Round((0.02 * (part.Mass / 2) + 5) * 1000 / GameServer.GameLoopInterval),
                    });

                    newParts.Add(new PlayerPart()
                    {
                        DivisionTime = PlayerPart.DefaulDivisionTime,
                        Identifier = i++,
                        IsOutOfOtherParts = false,
                        Mass = part.Mass / 2,
                        X = part.X,
                        Y = part.Y,
                        MergeTime = (int)Math.Round((0.02 * (part.Mass / 2) + 5) * 1000 / GameServer.GameLoopInterval),

                    });
                }

                else
                    newParts.Add(part);
            }
            player.Parts = newParts;
            game.ConnectionManager.SendToClient(playerName, new Invalidate("Division"));
        }
    }
}
