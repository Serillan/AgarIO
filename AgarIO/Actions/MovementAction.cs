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
        public override void Process(Game game)
        {
            var state = game.GameState;

            if (state?.CurrentPlayer?.Parts == null)
                return;

            var command = new Commands.Move();
            command.Time = game.Time;
            command.Movement = new List<Tuple<int, float, float>>();

            foreach (var part in state.CurrentPlayer.Parts)
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

                command.Movement.Add(new Tuple<int, float, float>(part.Identifier, nextX, nextY));
                
                //apply (prediction)
                part.X = nextX;
                part.Y = nextY;
            }

            game.ServerConnection.SendAsync(command);
        }
    }
}
