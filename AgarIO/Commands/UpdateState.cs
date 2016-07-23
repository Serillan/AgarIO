using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIO.Commands
{
    [ProtoBuf.ProtoContract]
    class UpdateState : Command
    {
        [ProtoBuf.ProtoMember(1)]
        public GameState GameState { get; set; }

        public override CommandType CommandType
        {
            get
            {
                return CommandType.Update;
            }
        }

        public UpdateState(GameState state)
        {
            this.GameState = state;
        }


    }
}
