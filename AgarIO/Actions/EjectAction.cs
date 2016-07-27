using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIO.Actions
{
    class EjectAction : Action
    {
        public EjectAction(Point MousePosition) : base(MousePosition)
        {
        }

        public override void Process(Game game)
        {
            game.ServerConnection.SendAsync(new Commands.Eject(this.X, this.Y));
        }
    }
}
