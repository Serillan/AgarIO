using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIOServer.Actions
{
    class MovementAction : Action
    {
        public MovementAction(double x, double y, string playerName) : base(x, y, playerName)
        {
        }

        public override void Process(GameState currentState)
        {
            var player = currentState.Players.Find(p => p.Name == PlayerName);
            foreach (var part in player.Parts)
            {
                part.X = X;
                part.Y = Y;
            }
        }
    }
}
