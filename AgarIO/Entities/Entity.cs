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
        public float Radius
        {
            get
            {
                //return (20 + NthRoot(Mass, 5));
                return 10 * (float) Math.Sqrt(Mass / Math.PI);
            }
        }
        [ProtoBuf.ProtoMember(3)]
        public float X { get; set; }
        [ProtoBuf.ProtoMember(4)]
        public float Y { get; set; }

        private float NthRoot(float A, int N)
        {
            return (float)(Math.Pow(A, 1.0 / N));
        }
    }
}
