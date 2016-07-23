using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIOServer.Commands
{
    [ProtoBuf.ProtoContract]
    class Divide : Command
    {
        [ProtoBuf.ProtoIgnore]
        public override CommandType CommandType
        {
            get
            {
                return CommandType.Divide;
            }
        }
    }
}
