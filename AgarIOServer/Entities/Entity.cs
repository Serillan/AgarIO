using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIOServer.Entities
{
    [ProtoBuf.ProtoContract]
    abstract class Entity
    {
        [ProtoBuf.ProtoMember(1)]
        public int Mass { get; set; }
        [ProtoBuf.ProtoMember(2)]
        public int Radius { get; set; }
        [ProtoBuf.ProtoMember(3)]
        public int X { get; set; }
        [ProtoBuf.ProtoMember(4)]
        public int Y { get; set; }
    }
}
