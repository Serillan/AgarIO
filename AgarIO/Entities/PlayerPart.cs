using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIO.Entities
{
    [ProtoBuf.ProtoContract]
    class PlayerPart : Entity
    {

        [ProtoBuf.ProtoMember(1)]
        public byte Identifier { get; set; }

        [ProtoBuf.ProtoMember(2)]
        public byte DivisionTime { get; set; }

        [ProtoBuf.ProtoMember(3)]
        public bool IsNewDividedPart { get; set; }

        [ProtoBuf.ProtoMember(4)]
        public bool IsBeingEjected { get; set; }

        [ProtoBuf.ProtoMember(6)]
        public float EjectedVX { get; set; }

        [ProtoBuf.ProtoMember(7)]
        public float EjectedVY { get; set; }

        [ProtoBuf.ProtoMember(8)]
        public float OldRadius { get; set; }

        [ProtoBuf.ProtoIgnore]
        public const byte DefaulDivisionTime = 10;

        [ProtoBuf.ProtoMember(5)]
        public int MergeTime { get; set; }

        [ProtoBuf.ProtoIgnore]
        public float Speed
        {
            get
            {
                if (IsBeingEjected)
                    return (OldRadius * 4 / DefaulDivisionTime) * Game.GameLoopInterval / 30f;
                return (DivisionTime > 0 ? (this.Radius * 8 / DefaulDivisionTime) : 20 / NthRoot(Mass, 5)) * Game.GameLoopInterval / 30f;
            }
        }

        private float NthRoot(float A, int N)
        {
            return (float)(Math.Pow(A, 1.0 / N));
        }
    }
}
