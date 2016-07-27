using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIO.Commands
{
    [ProtoBuf.ProtoContract]
    class Eject : Command
    {
        [ProtoBuf.ProtoMember(1)]
        public float X { get; set; }

        [ProtoBuf.ProtoMember(2)]
        public float Y { get; set; }

        private Eject() { }

        public Eject(float X, float Y)
        {
            this.X = X;
            this.Y = Y;
        }
    }
}
