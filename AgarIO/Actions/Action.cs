using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIO.Actions
{
    abstract class Action
    {
        public float X { get; set; }
        public float Y { get; set; }
        public string Player { get; set; }

        public Action(System.Drawing.Point MousePosition)
        {
            this.X = MousePosition.X;
            this.Y = MousePosition.Y;
        }

        public abstract void Process(Game game);
    }
}
