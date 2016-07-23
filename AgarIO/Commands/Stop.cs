using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIO.Commands
{
    [ProtoBuf.ProtoContract]
    class Stop : Command
    {
        [ProtoBuf.ProtoMember(1)]
        public string StopMessage { get; set; }

        public override void Process(Game game)
        {
            Debug.WriteLine("stooping");
            game.Close(StopMessage);
        }

        public Stop () { }

        public Stop(string msg)
        {
            StopMessage = msg;
        }
    }
}
