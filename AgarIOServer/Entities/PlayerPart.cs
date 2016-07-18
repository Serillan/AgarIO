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
        public double Speed { get; set; }
    }
}
