using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIO.Commands
{
    /// <summary>
    /// Represents the stop command.
    /// </summary>
    /// <seealso cref="AgarIO.Commands.Command" />
    [ProtoBuf.ProtoContract]
    class Stop : Command
    {
        /// <summary>
        /// Gets or sets the stop message.
        /// </summary>
        /// <value>The stop message.</value>
        [ProtoBuf.ProtoMember(1)]
        public string StopMessage { get; set; }

        /// <summary>
        /// Processes the command received from the server.
        /// </summary>
        /// <param name="game">The Game in which the command takes place.</param>
        public override void Process(Game game)
        {
            Debug.WriteLine("stooping");
            game.Close(StopMessage);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Stop"/> class.
        /// </summary>
        public Stop () { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Stop"/> class.
        /// </summary>
        /// <param name="msg">The stop message.</param>
        public Stop(string msg)
        {
            StopMessage = msg;
        }
    }
}
