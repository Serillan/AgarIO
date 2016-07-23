using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIO.Commands
{
    [ProtoBuf.ProtoContract]
    class Invalidate : Command
    {
        public override void Process(Game game)
        {
            throw new NotImplementedException();
        }
    }
}
