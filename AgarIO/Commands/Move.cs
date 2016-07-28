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
        /// First argument of the tuple is part identifier, second and third arguments are new coordinates
        /// and fourth is new predicted mass.
        /// </summary>
        [ProtoBuf.ProtoMember(1)]
        public List<Tuple<int, float, float, float>> Movement { get; set; }

        [ProtoBuf.ProtoMember(2)]
        public long Time { get; set; }

    }
}
