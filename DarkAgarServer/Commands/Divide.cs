using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DarkAgarServer.Entities;

namespace DarkAgarServer.Commands
{
    /// <summary>
    /// Represents the division command.
    /// </summary>
    /// <seealso cref="DarkAgarServer.Commands.Command" />
    [ProtoBuf.ProtoContract]
    class Divide : Command
    {

        /// <summary>
        /// Processes the command received from the client.
        /// </summary>
        /// <param name="gameServer">The Game Server in which the command takes place.</param>
        /// <param name="playerName">Name of the player.</param>
        public override void Process(GameServer gameServer, string playerName)
        {
            Player player = null;
            lock (gameServer.GameState.Players)
            {
                player = gameServer.GameState.Players.Find(p => p.Name == playerName);
            }

            lock (player)
            { 
                if (player.Parts.Count >= 16)
                    return;

                var newParts = new List<PlayerPart>();
                var freeIdentifiers = Enumerable.Range(0, 40).Where(n => !player.Parts.Exists(p => p.Identifier == n)).ToList();
                var i = 0;

                foreach (var part in player.Parts)
                {
                    if (part.Mass >= GameServer.MinimumDivisionSize)
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
                            DivisionTime = PlayerPart.DefaultDivisionTime,
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
                    gameServer.ConnectionManager.SendToClient(playerName, new Invalidate("Division"));
                }
            }
        }
    }
}
