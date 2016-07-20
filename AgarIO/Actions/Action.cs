using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIO.Actions
{
    abstract class Action
    {
        public double X { get; set; }
        public double Y { get; set; }
        public string Player { get; set; }

        public Action(System.Drawing.Point MousePosition)
        {
            this.X = MousePosition.X;
            this.Y = MousePosition.Y;
        }

        public abstract void Process(GameState CurrentState);
    }
}
