using AgarIO.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIO.Commands
{
    /// <summary>
    /// Represents the invalidation command.
    /// </summary>
    /// <seealso cref="AgarIO.Commands.Command" />
    [ProtoBuf.ProtoContract]
    class Invalidate : Command
    {
        /// <summary>
        /// Gets or sets the invalidation message.
        /// </summary>
        /// <value>The invalidation message.</value>
        [ProtoBuf.ProtoMember(1)]
        public string InvalidationMessage { get; set; }

        /// <summary>
        /// Used for deserialization.
        /// </summary>
        private Invalidate() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Invalidate"/> class.
        /// </summary>
        /// <param name="invalidationMessage">The invalidation message.</param>
        public Invalidate(string invalidationMessage)
        {
            InvalidationMessage = invalidationMessage;
        }

        /// <summary>
        /// Processes the command received from the server.
        /// </summary>
        /// <param name="game">The Game in which the command takes place.</param>
        public override void Process(Game game)
        {
            //Debug.WriteLine("... INVALIDATE : " + ErrorMessage);
            game.IsPredictionValid = false;
        }
    }
}
