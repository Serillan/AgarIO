using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace DarkAgar.Commands
{

    /// <summary>
    /// Represents the command that is send between a client and the server.
    /// </summary>
    [ProtoContract]
    [ProtoInclude(1, typeof(Divide))]
    [ProtoInclude(2, typeof(Invalidate))]
    [ProtoInclude(3, typeof(Move))]
    [ProtoInclude(4, typeof(Stop))]
    [ProtoInclude(5, typeof(UpdateState))]
    [ProtoInclude(6, typeof(Eject))]
    abstract class Command
    {
        /// <summary>
        /// Processes the command received from the server.
        /// </summary>
        /// <param name="game">The Game in which the command takes place.</param>
        public virtual void Process(Game game) { }
    }

}
