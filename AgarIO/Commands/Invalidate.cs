using AgarIO.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIO.Commands
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

        public override void Process(Game game)
        {
            //Debug.WriteLine("... INVALIDATE : " + ErrorMessage);
            game.IsPredictionValid = false;
        }
    }
}
