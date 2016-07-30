using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DarkAgar.Entities;

namespace DarkAgar.Commands
{
    /// <summary>
    /// Represents the movement command.
    /// </summary>
    /// <seealso cref="DarkAgar.Commands.Command" />
    [ProtoBuf.ProtoContract]
    class Move : Command
    {
        /// <summary>
        /// Specifies where player parts should be moved.
        /// First argument of the tuple is part identifier, second and third arguments are new coordinates
        /// and fourth is new predicted mass.
        /// </summary>
        /// <value>The movement.</value>
        [ProtoBuf.ProtoMember(1)]
        public List<Tuple<int, float, float, float>> Movement { get; set; }

        /// <summary>
        /// Gets or sets the time that describes in which cycle of the game loop the movement took place.
        /// Used for synchronization with the server.
        /// </summary>
        /// <value>The time.</value>
        [ProtoBuf.ProtoMember(2)]
        public long Time { get; set; }

    }
}
