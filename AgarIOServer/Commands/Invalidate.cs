using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIOServer.Commands
{
    [ProtoBuf.ProtoContract]
    class Invalidate : Command
    {
        [ProtoBuf.ProtoMember(1)]
        public string ErrorMessage { get; set; }

        private Invalidate() { }

        public Invalidate(string msg)
        {
            ErrorMessage = msg;
        }
    }
}
