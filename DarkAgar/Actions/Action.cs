﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkAgar.Actions
{
    /// <summary>
    /// Represents the player action.
    /// </summary>
    abstract class Action
    {
        /// <summary>
        /// X value (in game coordinates) of the mouse for the action.
        /// </summary>
        /// <value>The x.</value>
        public float X { get; set; }

        /// <summary>
        /// X value (in game coordinates) of the mouse for the action.
        /// </summary>
        /// <value>The y.</value>
        public float Y { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Action"/> class.
        /// </summary>
        /// <param name="mousePosition">Mouse position that will be used for the action.</param>
        protected Action(Point mousePosition)
        {
            this.X = mousePosition.X;
            this.Y = mousePosition.Y;
        }

        /// <summary>
        /// Processes player action. It will create appropriate command and
        /// send to the server and also updates predictions.
        /// </summary>
        /// <param name="game">Game in which the player action takes place.</param>
        public abstract void Process(Game game);
    }
}
