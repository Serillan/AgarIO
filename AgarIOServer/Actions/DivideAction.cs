using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIOServer.Actions
{
    class DivideAction : Action
    {
        public DivideAction(int x, int y, string playerName) : base(x, y, playerName)
        {
        }

        public override void Process(GameState CurrentState)
        {
            throw new NotImplementedException();
        }
    }
}
