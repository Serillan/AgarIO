using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkAgarServer.Commands
{
    /// <summary>
    /// Represents the invalidation command.
    /// </summary>
    /// <seealso cref="DarkAgarServer.Commands.Command" />
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
    }
}
