using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DarkAgarServer.Entities;

namespace DarkAgarServer.Commands
{
    /// <summary>
    /// Represents the ejection command.
    /// </summary>
    /// <seealso cref="DarkAgarServer.Commands.Command" />
    [ProtoBuf.ProtoContract]
    class Eject : Command
    {
        /// <summary>
        /// Gets or sets the x value (in game coordinates) of the position to which the part should be ejected.
        /// </summary>
        /// <value>The x.</value>
        [ProtoBuf.ProtoMember(1)]
        public float X { get; set; }

        /// <summary>
        /// Gets or sets the y value (in game coordinates) of the position to which the part should be ejected.
        /// </summary>
        /// <value>The y.</value>
        [ProtoBuf.ProtoMember(2)]
        public float Y { get; set; }

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

                var newParts = new List<PlayerPart>();
                byte i = 0;

                var nearestPart = (from part in player.Parts
                                   where part.Mass >= GameServer.MinimumDivisionSize
                                   orderby Distance(X, Y, part) ascending
                                   select part).FirstOrDefault();

                if (nearestPart == null)
                    return;


                var freeIdentifier = Enumerable.Range(0, 40).First(n => !player.Parts.Exists(p => p.Identifier == n));

                player.Parts.Add(new PlayerPart()
                {
                    Mass = GameServer.BlobSize,
                    DivisionTime = PlayerPart.DefaultDivisionTime,
                    IsBeingEjected = true,
                    Identifier = (byte)freeIdentifier,
                    X = nearestPart.X,
                    Y = nearestPart.Y,
                    EjectedVX = this.X - nearestPart.X,
                    EjectedVY = this.Y - nearestPart.Y,
                    MergeTime = 100000,
                    IsNewDividedPart = true,
                    OldRadius = nearestPart.Radius
                });
                nearestPart.Mass -= GameServer.BlobSize;

            }
            gameServer.ConnectionManager.SendToClient(player.Name, new Invalidate("Ejection"));
        }

        /// <summary>
        /// Returns the distance between (<paramref name="x"/>, <paramref name="y"/>) and 
        /// <paramref name="part"/>.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="part">The part.</param>
        /// <returns>The distance between (<paramref name="x"/>, <paramref name="y"/>) and 
        /// <paramref name="part"/>.</returns>
        public float Distance(float x, float y, PlayerPart part)
        {
            var dx = x - part.X;
            var dy = y - part.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
