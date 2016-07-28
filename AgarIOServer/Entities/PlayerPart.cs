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
        public bool IsNewDividedPart { get; set; }

        [ProtoBuf.ProtoMember(4)]
        public bool IsBeingEjected { get; set; }

        [ProtoBuf.ProtoMember(6)]
        public float EjectedVX { get; set; }

        [ProtoBuf.ProtoMember(7)]
        public float EjectedVY { get; set; }



        [ProtoBuf.ProtoMember(5)]
        public int MergeTime { get; set; }


        [ProtoBuf.ProtoIgnore]
        public const byte DefaulDivisionTime = 15;

        [ProtoBuf.ProtoIgnore]
        public float Speed
        {
            get
            {
                if (IsBeingEjected)
                    return (40 + NthRoot(Mass, 1)) * GameServer.GameLoopInterval / 30f;
                return ((DivisionTime > 0 ? 30 + NthRoot(Mass, 3) : 20 / NthRoot(Mass, 5))) * GameServer.GameLoopInterval / 30f;
            }
        }

        private float NthRoot(float A, int N)
        {
            return (float)(Math.Pow(A, 1.0 / N));
        }

        public PlayerPart()
        {
            DivisionTime = 0;
            IsNewDividedPart = false;
        }

    }
}
