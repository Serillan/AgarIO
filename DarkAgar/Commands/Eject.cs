using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkAgar.Commands
{
    /// <summary>
    /// Represents the ejection command.
    /// </summary>
    /// <seealso cref="DarkAgar.Commands.Command" />
    [ProtoBuf.ProtoContract]
    class Eject : Command
    {
        /// <summary>
        /// Gets or sets the x value (in game coordinates) of the position to which the part should be ejected.
        /// </summary>
        /// <value>The x.</value>
        [ProtoBuf.ProtoMember(1)]
        public float X { get; set; }

        /// <summary>
        /// Gets or sets the y value (in game coordinates) of the position to which the part should be ejected.
        /// </summary>
        /// <value>The y.</value>
        [ProtoBuf.ProtoMember(2)]
        public float Y { get; set; }

        /// <summary>
        /// Used for deserialization.
        /// </summary>
        private Eject() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Eject"/> class.
        /// </summary>
        /// <param name="X">X value (in game coordinates) of the position to which the part should be ejected.</param>
        /// <param name="Y">Y value (in game coordinates) of the position to which the part should be ejected.</param>
        public Eject(float X, float Y)
        {
            this.X = X;
            this.Y = Y;
        }
    }
}
