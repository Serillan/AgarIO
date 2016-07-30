using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkAgarServer.Entities
{
    /// <summary>
    /// Represents the virus entity.
    /// </summary>
    /// <seealso cref="DarkAgarServer.Entities.Entity" />
    [ProtoBuf.ProtoContract]
    class Virus : Entity
    {
        /// <summary>
        /// Gets or sets the start x position for the movement.
        /// </summary>
        /// <value>The start x.</value>
        [ProtoBuf.ProtoIgnore]
        public float StartX { get; set; }

        /// <summary>
        /// Gets or sets the start y position for the movement.
        /// </summary>
        /// <value>The start y.</value>
        [ProtoBuf.ProtoIgnore]
        public float StartY { get; set; }

        /// <summary>
        /// Gets or sets the end x position for the movement.
        /// </summary>
        /// <value>The end x.</value>
        [ProtoBuf.ProtoIgnore]
        public float EndX { get; set; }

        /// <summary>
        /// Gets or sets the end y position for the movement.
        /// </summary>
        /// <value>The end y.</value>
        [ProtoBuf.ProtoIgnore]
        public float EndY { get; set; }

        /// <summary>
        /// Gets or sets the movement start time.
        /// </summary>
        /// <value>The the movement start time.</value>
        [ProtoBuf.ProtoIgnore]
        public long MovementStartTime { get; set; }

        /// <summary>
        /// Gets or sets the the movement end time.
        /// </summary>
        /// <value>The the movement end time.</value>
        [ProtoBuf.ProtoIgnore]
        public long MovementEndTime { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Virus"/> class.
        /// </summary>
        public Virus() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Virus"/> class.
        /// </summary>
        /// <param name="playersParts">The players parts on which the virus shouldn't be placed</param>
        public Virus(List<PlayerPart> playersParts)
        {
            Mass = GameServer.DefaultVirusSize;
            do
            {
                X = GameServer.RandomGenerator.Next(0, GameServer.MaxLocationX);
                Y = GameServer.RandomGenerator.Next(0, GameServer.MaxLocationY);
            } while (playersParts.Exists(p => AreInCollision(this, p)));
        }

        /// <summary>
        /// Determines whether the specified <paramref name="virus"/> is in collision with the specified <paramref name="part"/>.
        /// </summary>
        /// <param name="part1">The part1.</param>
        /// <param name="part2">The part2.</param>
        /// <returns><c>true</c> if the specified <paramref name="virus"/> is in collision with the specified <paramref name="part2"/>, <c>false</c> otherwise.</returns>
        private bool AreInCollision(Virus virus, PlayerPart part)
        {
            var dx = part.X - virus.X;
            var dy = part.Y - virus.Y;
            var distance = Math.Sqrt(dx * dx + dy * dy);
            return distance < virus.Radius + part.Radius;
        }
    }
}
