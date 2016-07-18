using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIOServer.Actions
{
    abstract class Action
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Player { get; set; }

        public Action(int x, int y, string playerName)
        {
            this.X = x;
            this.Y = y;
            this.Player = playerName;
        }

        public abstract void Process(GameState CurrentState);
    }
}
