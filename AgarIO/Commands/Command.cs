using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace AgarIO.Commands
{

    [ProtoContract]
    [ProtoInclude(1, typeof(Divide))]
    [ProtoInclude(2, typeof(Invalidate))]
    [ProtoInclude(3, typeof(Move))]
    [ProtoInclude(4, typeof(Stop))]
    [ProtoInclude(5, typeof(UpdateState))]
    abstract class Command
    {
        public virtual void Process(Game game) { }
    }

}
