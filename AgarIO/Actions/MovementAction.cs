using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIO.Actions
{
    class MovementAction : Action
    {
        public MovementAction(Point MousePosition) : base(MousePosition)
        {
        }
        // TODO: pre zlepsenie sa bude posielat movement a ignorovat co vrati server
        // jedine ak posle FALSE a poziciu hraca, tak si zmenim vlastnu poziciu
        public override void Process(GameState CurrentState)
        {
            if (CurrentState?.CurrentPlayer?.Parts == null)
                return;
            foreach (var part in CurrentState.CurrentPlayer.Parts)
            {
                float vX = (float)(X - GraphicsEngine.GamePanelWidth / 2);
                float vY = (float)(Y - GraphicsEngine.GamePanelHeight / 2);

                // normalize
                float size = (float)(Math.Sqrt(vX * vX + vY * vY));
                if (size == 0)
                    return;
                vX *= (1 / size);
                vY *= (1 / size);

                var nextX = part.X + vX * part.Speed;
                var nextY = part.Y + vY * part.Speed;
                if (nextX > Game.MaxLocationX || nextX < 0)
                    nextX = part.X;
                if (nextY > Game.MaxLocationY || nextY < 0)
                    nextY = part.Y;


                //Game.ServerConnection.SendAsync($"MOVE {X} {Y}");

                Game.ServerConnection.SendAsync($"MOVE {Game.Time} {nextX} {nextY}");
                //apply
                part.X = nextX;
                part.Y = nextY;
            }
        }
    }
}
