using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIO.Entities
{
    [ProtoBuf.ProtoContract]
    [ProtoBuf.ProtoInclude(5, typeof(Food))]
    [ProtoBuf.ProtoInclude(6, typeof(Player))]
    [ProtoBuf.ProtoInclude(7, typeof(PlayerPart))]
    [ProtoBuf.ProtoInclude(8, typeof(Virus))]
    abstract class Entity
    {
        [ProtoBuf.ProtoMember(1)]
        public int Mass { get; set; }
        [ProtoBuf.ProtoIgnore]
        public double Radius
        {
            get
            {
                return Math.Sqrt(Mass);
            }
        }
        [ProtoBuf.ProtoMember(3)]
        public double X { get; set; }
        [ProtoBuf.ProtoMember(4)]
        public double Y { get; set; }
    }
}
