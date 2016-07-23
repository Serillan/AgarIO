using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIOServer.Commands
{
    [ProtoBuf.ProtoContract]
    class UpdateState : Command
    {
        [ProtoBuf.ProtoMember(1)]
        public GameState GameState { get; set; }

        public UpdateState(GameState state)
        {
            this.GameState = state;
        }

        private UpdateState() { }
    }
}
