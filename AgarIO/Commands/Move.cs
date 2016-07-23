using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgarIO.Entities;

namespace AgarIO.Commands
{
    [ProtoBuf.ProtoContract]
    class Move : Command
    {
        /// <summary>
        /// Specifies where player parts should be moved.
        /// First argument of the tuple is part identifier and other two arguments are new coordinates.
        /// </summary>
        [ProtoBuf.ProtoMember(1)]
        public List<Tuple<int, float, float>> Movement { get; set; }

        [ProtoBuf.ProtoMember(2)]
        public int Time { get; set; }

    }
}
