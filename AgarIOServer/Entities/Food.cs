using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkAgarServer.Entities
{
    /// <summary>
    /// Represents the food entity.
    /// </summary>
    /// <seealso cref="DarkAgarServer.Entities.Entity" />
    [ProtoBuf.ProtoContract]
    class Food : Entity
    {
        /// <summary>
        /// Gets or sets the color.
        /// Color is in the format [red, green, blue] in the array.
        /// </summary>
        /// <value>The color.</value>
        [ProtoBuf.ProtoMember(1)]
        public byte[] Color { get; set; }

        /// <summary>
        /// Prevents a default instance of the <see cref="Food"/> class from being created.
        /// </summary>
        private Food() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Food"/> class.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="mass">The mass.</param>
        public Food(float x, float y, int mass)
        {
            this.X = x;
            this.Y = y;
            this.Mass = mass;
            // random
            this.Color = new byte[3];
            GameServer.RandomGenerator.NextBytes(this.Color);
        }
    }
}
