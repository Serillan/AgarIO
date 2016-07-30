using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkAgar.Entities
{
    /// <summary>
    /// Represents the food entity.
    /// </summary>
    /// <seealso cref="DarkAgar.Entities.Entity" />
    [ProtoBuf.ProtoContract]
    class Food : Entity, IEquatable<Food>
    {
        /// <summary>
        /// Gets or sets the color.
        /// Color is in the format [red, green, blue] in the array.
        /// </summary>
        /// <value>The color.</value>
        [ProtoBuf.ProtoMember(1)]
        public byte[] Color { get; set; }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        public bool Equals(Food other)
        {
            if (other.X == this.X && other.Y == this.Y && 
                other.Color[0] == this.Color[0] && other.Color[1] == this.Color[1] && other.Color[2] == this.Color[2]
                && other.Mass == this.Mass)
                return true;
            return false;
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>Food.</returns>
        public Food Clone()
        {
            return (Food)MemberwiseClone();
        }
    }
}
