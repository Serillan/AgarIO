using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkAgarServer.Commands
{
    /// <summary>
    /// Represents the stop command.
    /// </summary>
    /// <seealso cref="DarkAgarServer.Commands.Command" />
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
        /// Processes the command received from the client.
        /// </summary>
        /// <param name="gameServer">The Game Server in which the command takes place.</param>
        /// <param name="playerName">Name of the player.</param>
        public override void Process(GameServer gameServer, string playerName)
        {
            gameServer.RemovePlayer(playerName, StopMessage);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Stop"/> class.
        /// </summary>
        public Stop() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Stop" /> class.
        /// </summary>
        /// <param name="stopMessage">The stop message.</param>
        public Stop(string stopMessage)
        {
            this.StopMessage = stopMessage;
        }

    }
}
