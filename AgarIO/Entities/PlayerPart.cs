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
        public int Identifier { get; set; }

        [ProtoBuf.ProtoIgnore]
        public float Speed
        {
            get
            {
                return (10 / NthRoot(Mass, 5));
            }
        }

        private float NthRoot(float A, int N)
        {
            return (float)(Math.Pow(A, 1.0 / N));
        }
    }
}
