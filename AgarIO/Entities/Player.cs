using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkAgar.Entities
{
    /// <summary>
    /// Represents the Player entity.
    /// </summary>
    /// <seealso cref="DarkAgar.Entities.Entity" />
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
        /// Gets or sets the x coordinate.
        /// </summary>
        /// <value>The x.</value>
        [ProtoBuf.ProtoIgnore]
        new public float X
        {
            get
            {
                float x = 0;
                foreach (var part in Parts.Where(p => !p.IsBeingEjected))
                    x += part.X;
                return x / Parts.Where(p => !p.IsBeingEjected).Count();
            }
        }
    
        /// <summary>
        /// Gets or sets the y coordinate.
        /// </summary>
        /// <value>The y.</value>
        [ProtoBuf.ProtoIgnore]
        new public float Y
        {
            get
            {
                float y = 0;
                foreach (var part in Parts.Where(p => !p.IsBeingEjected))
                    y += part.Y;
                return y / Parts.Where(p => !p.IsBeingEjected).Count();
            }
        }
        
        /// <summary>
        /// Gets the mass of the player.
        /// It is equal to the sum of the masses of player's parts.
        /// </summary>
        /// <value>The mass.</value>
        [ProtoBuf.ProtoIgnore]
        new public int Mass
        {
            get
            {
                var mass = 0;
                foreach (var part in Parts.Where(p => !p.IsBeingEjected))
                    mass += part.Mass;
                return mass;
            }
        }

        /// <summary>
        /// Gets the radius.
        /// </summary>
        /// <value>The radius.</value>
        [ProtoBuf.ProtoIgnore]
        new public float Radius
        {
            get
            {
                return 10 * (float)Math.Sqrt(Mass / Math.PI);
            }
        }
    }
}
