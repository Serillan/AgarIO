using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkAgar.Actions
{

    /// <summary>
    /// Represents the ejection action.
    /// </summary>
    /// <seealso cref="DarkAgar.Actions.Action" />
    class EjectAction : Action
    {
        /// <summary>
        /// Creates new player action.
        /// </summary>
        /// <param name="MousePosition">Mouse position that will be used for the action.</param>
        public EjectAction(Point MousePosition) : base(MousePosition)
        {
        }

        /// <summary>
        /// Processes player action. It will create appropriate command and
        /// send to the server and also updates predictions.
        /// </summary>
        /// <param name="game">Game in which the player action takes place.</param>
        public override void Process(Game game)
        {
            game.ServerConnection.SendAsync(new Commands.Eject(this.X, this.Y));
        }
    }
}
