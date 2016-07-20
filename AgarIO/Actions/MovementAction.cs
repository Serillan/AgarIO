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

        public override void Process(GameState CurrentState)
        {
            if (CurrentState?.CurrentPlayer?.Parts == null)
                return;
            foreach (var part in CurrentState.CurrentPlayer.Parts)
            {
                var vX = X - GraphicsEngine.GamePanelWidth / 2;
                var vY = Y - GraphicsEngine.GamePanelHeight / 2;

                // normalize
                var size = Math.Sqrt(vX * vX + vY * vY);
                if (size == 0)
                    return;
                vX *= (1 / size);
                vY *= (1 / size);

                //apply
                part.X += vX * part.Speed;
                part.Y += vY * part.Speed;
                //Game.ServerConnection.SendAsync($"MOVE {X} {Y}");

                Game.ServerConnection.SendAsync($"MOVE {part.X + vX * part.Speed} {part.Y + vY * part.Speed}");
            }
        }
    }
}
