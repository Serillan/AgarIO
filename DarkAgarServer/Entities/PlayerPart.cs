using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkAgarServer.Entities
{
    /// <summary>
    /// Represents player part entity.
    /// </summary>
    /// <seealso cref="DarkAgarServer.Entities.Entity" />
    [ProtoBuf.ProtoContract]
    class PlayerPart : Entity
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        [ProtoBuf.ProtoMember(1)]
        public byte Identifier { get; set; }

        /// <summary>
        /// Gets or sets the division time.
        /// While division time greater than zero, the part is moving faster.
        /// </summary>
        /// <value>The division time.</value>
        [ProtoBuf.ProtoMember(2)]
        public byte DivisionTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is new divided part.
        /// </summary>
        /// <value><c>true</c> if this instance is new divided part; otherwise, <c>false</c>.</value>
        [ProtoBuf.ProtoMember(3)]
        public bool IsNewDividedPart { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is being ejected.
        /// </summary>
        /// <value><c>true</c> if this instance is being ejected; otherwise, <c>false</c>.</value>
        [ProtoBuf.ProtoMember(4)]
        public bool IsBeingEjected { get; set; }

        /// <summary>
        /// Gets or sets the ejected vector x - coordinate.
        /// Ejected vector describes the trajectory of the movement of the ejected part.
        /// </summary>
        /// <value>The ejected vector x - coordinate.</value>
        [ProtoBuf.ProtoMember(6)]
        public float EjectedVX { get; set; }

        /// <summary>
        /// Gets or sets the ejected vector y - coordinate.
        /// Ejected vector describes the trajectory of the movement of the ejected part.
        /// </summary>
        /// <value>The ejected vector y - coordinate.</value>
        [ProtoBuf.ProtoMember(7)]
        public float EjectedVY { get; set; }

        /// <summary>
        /// Gets or sets the merge time describing the time until this part can be merged.
        /// </summary>
        /// <value>The merge time describing the time until this part can be merged.</value>
        [ProtoBuf.ProtoMember(5)]
        public int MergeTime { get; set; }

        /// <summary>
        /// Gets or sets the old radius of the part from which the ejected part was taken.
        /// </summary>
        /// <value>The old radius of the part from which the ejected part was taken.</value>
        [ProtoBuf.ProtoMember(8)]
        public float OldRadius { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is merged.
        /// Used for merge processing.
        /// </summary>
        /// <value><c>true</c> if this instance is merged in the current processing; otherwise, <c>false</c>.</value>
        [ProtoBuf.ProtoIgnore]
        public bool IsMerged { get; set; }

        /// <summary>
        /// The default division time
        /// </summary>
        [ProtoBuf.ProtoIgnore]
        public const byte DefaultDivisionTime = 10;

        /// <summary>
        /// Gets the speed.
        /// </summary>
        /// <value>The speed.</value>
        [ProtoBuf.ProtoIgnore]
        public float Speed
        {
            get
            {
                if (IsBeingEjected)
                    return (OldRadius * 12 / DefaultDivisionTime) * GameServer.GameLoopInterval / 30f;
                return ((DivisionTime > 0 ? (this.Radius * 8 / DefaultDivisionTime) : 20 / NthRoot(Mass, 5))) * GameServer.GameLoopInterval / 30f;
            }
        }

        /// <summary>
        /// Returns the <paramref name="n" />-th root of the <paramref name="number" />.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="n">The n.</param>
        /// <returns>The <paramref name="n" />-th root of the <paramref name="number" />.</returns>
        private static float NthRoot(float number, int n) => (float)(Math.Pow(number, 1.0 / n));

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerPart"/> class.
        /// </summary>
        public PlayerPart()
        {
            DivisionTime = 0;
            IsNewDividedPart = false;
        }

    }
}
