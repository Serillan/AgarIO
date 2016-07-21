using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIOServer.Actions
{
    abstract class Action
    {
        public float X { get; set; }
        public float Y { get; set; }
        public string PlayerName { get; set; }

        public Action(float x, float y, string playerName)
        {
            this.X = x;
            this.Y = y;
            this.PlayerName = playerName;
        }

        public abstract void Process(GameState CurrentState);
    }
}
