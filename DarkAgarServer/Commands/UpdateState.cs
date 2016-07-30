using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkAgarServer.Commands
{
    /// <summary>
    /// Represents the update of the state command.
    /// </summary>
    /// <seealso cref="DarkAgarServer.Commands.Command" />
    [ProtoBuf.ProtoContract]
    class UpdateState : Command
    {
        /// <summary>
        /// Gets or sets the state of the game.
        /// </summary>
        /// <value>The state of the game.</value>
        [ProtoBuf.ProtoMember(1)]
        public GameState GameState { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateState" /> class.
        /// </summary>
        /// <param name="gameState">State of the game.</param>
        public UpdateState(GameState gameState)
        {
            this.GameState = gameState;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="UpdateState"/> class from being created.
        /// </summary>
        private UpdateState() { }
    }
}
