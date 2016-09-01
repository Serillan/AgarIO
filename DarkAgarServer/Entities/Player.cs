using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkAgarServer.Entities
{
    /// <summary>
    /// Represents the Player entity.
    /// </summary>
    /// <seealso cref="DarkAgarServer.Entities.Entity" />
    [ProtoBuf.ProtoContract]
    class Player : Entity
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [ProtoBuf.ProtoMember(1)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the parts of the player.
        /// </summary>
        /// <value>The parts.</value>
        [ProtoBuf.ProtoMember(2)]
        public List<PlayerPart> Parts { get; set; }

        /// <summary>
        /// Gets or sets the color of the player parts.
        /// Color is in the format [red, green, blue] in the array.
        /// </summary>
        /// <value>The color.</value>
        [ProtoBuf.ProtoMember(3)]
        public byte[] Color { get; set; }

        /// <summary>
        /// Gets or sets the creation time (in server time).
        /// </summary>
        /// <value>The creation time.</value>
        [ProtoBuf.ProtoIgnore]
        public long CreationTime { get; set; }

        /// <summary>
        /// Gets or sets the first movement server time.
        /// </summary>
        /// <value>The first movement server time.</value>
        [ProtoBuf.ProtoIgnore]
        public long FirstMovementServerTime { get; set; }

        /// <summary>
        /// Gets or sets the first movement time (in player time).
        /// </summary>
        /// <value>The first movement time.</value>
        [ProtoBuf.ProtoIgnore]
        public long FirstMovementTime { get; set; }

        /// <summary>
        /// Gets or sets the last movement time (in player time).
        /// </summary>
        /// <value>The last movement time.</value>
        [ProtoBuf.ProtoIgnore]
        public long LastMovementTime { get; set; }

        /// <summary>
        /// Gets or sets the x coordinate.
        /// </summary>
        /// <value>The x.</value>
        [ProtoBuf.ProtoIgnore]
        public new float X
        {
            get
            {
                float x = Parts.Where(p => !p.IsBeingEjected).Sum(part => part.X);
                return x / Parts.Count(p => !p.IsBeingEjected);
            }
        }
        
        /// <summary>
        /// Gets or sets the y coordinate.
        /// </summary>
        /// <value>The y.</value>
        [ProtoBuf.ProtoIgnore]
        public new float Y
        {
            get
            {
                float y = Parts.Where(p => !p.IsBeingEjected).Sum(part => part.Y);
                return y / Parts.Count(p => !p.IsBeingEjected);
            }
        }
       
        /// <summary>
        /// Gets or sets the mass.
        /// </summary>
        /// <value>The mass.</value>
        [ProtoBuf.ProtoIgnore]
        public new int Mass => Parts.Where(p => !p.IsBeingEjected).Sum(part => part.Mass);

        /// <summary>
        /// Gets the radius.
        /// </summary>
        /// <value>The radius.</value>
        [ProtoBuf.ProtoIgnore]
        public new float Radius => 10 * (float)Math.Sqrt(Mass / Math.PI);

        /// <summary>
        /// Initializes a new instance of the <see cref="Player" /> class.
        /// </summary>
        /// <param name="playerName">Name of the player.</param>
        /// <param name="gameState">State of the game.</param>
        public Player(string playerName, GameState gameState)
        {
            this.Name = playerName;
            this.Parts = new List<PlayerPart>();
            var part = new PlayerPart {Mass = GameServer.PlayerStartSize};

            List<PlayerPart> otherPlayerParts;

            lock (gameState.Players)
            {
                otherPlayerParts = (from player in gameState.Players
                                    where player.Name != playerName
                                    from otherPlayerPart in player.Parts
                                    select otherPlayerPart).ToList();
            }
            do
            {
                part.X = GameServer.RandomGenerator.Next((int)(Math.Round(this.Radius)), GameServer.MaxLocationX);
                part.Y = GameServer.RandomGenerator.Next((int)(Math.Round(this.Radius)), GameServer.MaxLocationY);
            } while (otherPlayerParts.Exists(p => !IsInSafeDistance(part, p)) || 
                     gameState.Viruses.Exists(v => !IsInSafeDistance(part, v)));

            part.Identifier = 1;
            Parts.Add(part);
            this.FirstMovementServerTime = Stopwatch.GetTimestamp();
            this.FirstMovementTime = 0;
            this.Color = new byte[3];
            GameServer.RandomGenerator.NextBytes(this.Color);
        }

        /// <summary>
        /// Determines whether the specified <paramref name="part1"/> is in safe distance for creation 
        /// from the specified <paramref name="part2"/>.
        /// </summary>
        /// <param name="part1">The part1.</param>
        /// <param name="part2">The part2.</param>
        /// <returns><c>true</c> if <paramref name="part1"/> is in safe distance for creation 
        /// from <paramref name="part2"/>; otherwise, <c>false</c>.</returns>
        private static bool IsInSafeDistance(PlayerPart part1, PlayerPart part2)
        {
            var dx = part2.X - part1.X;
            var dy = part2.Y - part1.Y;
            var distance = Math.Sqrt(dx * dx + dy * dy);
            return distance > 2 * (part1.Radius + part2.Radius);
        }

        /// <summary>
        /// Determines whether the specified <paramref name="part"/> is in safe distance for creation 
        /// from the specified <paramref name="virus"/>.
        /// </summary>
        /// <param name="part">The part.</param>
        /// <param name="virus">The virus.</param>
        /// <returns><c>true</c> if the specified <paramref name="part"/> is in safe distance for creation 
        /// from the specified <paramref name="virus"/>; otherwise, <c>false</c>.</returns>
        private static bool IsInSafeDistance(PlayerPart part, Virus virus)
        {
            var dx = virus.X - part.X;
            var dy = virus.Y - part.Y;
            var distance = Math.Sqrt(dx * dx + dy * dy);

            return distance > 2 * (virus.Radius + part.Radius);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Player"/> class.
        /// </summary>
        public Player() { }
    }
}
