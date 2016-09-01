using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkAgar.Actions
{
    /// <summary>
    /// Represents the division action.
    /// </summary>
    /// <seealso cref="DarkAgar.Actions.Action" />
    class DivideAction : Action
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="DivideAction"/> class.
        /// </summary>
        /// <param name="mousePosition">Mouse position that will be used for the action.</param>
        public DivideAction(Point mousePosition) : base(mousePosition)
        {
        }

        /// <summary>
        /// Processes player action. It will create appropriate command and
        /// send to the server and also updates predictions.
        /// </summary>
        /// <param name="game">Game in which the player action takes place.</param>
        public override void Process(Game game)
        {
            game.ServerConnection.SendAsync(new Commands.Divide());
        }
    }
}
