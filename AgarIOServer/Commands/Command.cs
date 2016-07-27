using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace AgarIOServer.Commands
{

    [ProtoContract]
    [ProtoInclude(1, typeof(Divide))]
    [ProtoInclude(2, typeof(Invalidate))]
    [ProtoInclude(3, typeof(Move))]
    [ProtoInclude(4, typeof(Stop))]
    [ProtoInclude(5, typeof(UpdateState))]
    [ProtoInclude(6, typeof(Eject))]
    abstract class Command
    {
        public virtual void Process(GameServer game, string playerName) { }
    }
}
