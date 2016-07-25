using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIOServer.Entities
{
    [ProtoBuf.ProtoContract]
    class PlayerPart : Entity
    {
        [ProtoBuf.ProtoMember(1)]
        public byte Identifier { get; set; }

        [ProtoBuf.ProtoMember(2)]
        public byte DivisionTime { get; set; }

        [ProtoBuf.ProtoMember(3)]
        public int MergeTime { get; set; }

        [ProtoBuf.ProtoIgnore]
        public const byte DefaulDivisionTime = 20;

        [ProtoBuf.ProtoMember(4)]
        public bool IsOutOfOtherParts { get; set; }

        [ProtoBuf.ProtoIgnore]
        public float Speed
        {
            get
            {
                return ((DivisionTime > 0 ? 40 + NthRoot(Mass, 5) : 20 / NthRoot(Mass, 5)));
            }
        }

        private float NthRoot(float A, int N)
        {
            return (float)(Math.Pow(A, 1.0 / N));
        }

        public PlayerPart()
        {
            DivisionTime = 0;
        }

    }
}
