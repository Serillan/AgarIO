using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIO.Entities
{
    [ProtoBuf.ProtoContract]
    class Food : Entity, IEquatable<Food>
    {
        [ProtoBuf.ProtoMember(1)]
        public byte[] Color { get; set; }

        public bool Equals(Food other)
        {
            if (other.X == this.X && other.Y == this.Y && 
                other.Color[0] == this.Color[0] && other.Color[1] == this.Color[1] && other.Color[2] == this.Color[2]
                && other.Mass == this.Mass)
                return true;
            return false;
        }

        public Food Clone()
        {
            return (Food)MemberwiseClone();
        }
    }
}
