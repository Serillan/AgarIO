using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgarIO.Entities;

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

            List<PlayerPart> doneParts = new List<PlayerPart>();

            state.CurrentPlayer.Parts.Sort((p1, p2) => p1.Identifier.CompareTo(p2.Identifier)); ; // consistence

            foreach (var part in state.CurrentPlayer.Parts)
            {
                float vX = (float)(X - GraphicsEngine.GamePanelWidth / 2);
                float vY = (float)(Y - GraphicsEngine.GamePanelHeight / 2);

                // normalize
                float size = (float)(Math.Sqrt(vX * vX + vY * vY));
                if (size == 0)
                    return;
                vX /= size;
                vY /= size;

                var nextX = part.X + vX * part.Speed;
                var nextY = part.Y + vY * part.Speed;
                
                //apply (prediction)
                part.X = nextX;
                part.Y = nextY;
                /*
                if (part.IsOutOfOtherParts)
                {
                    foreach (var p in doneParts)
                    {
                        if (AreInCollision(part, p) && p.IsOutOfOtherParts) // PREDICTION REQUIRED!
                        {
                            var dx = p.X - part.X;
                            var dy = p.Y - part.Y;
                            var distance = Math.Sqrt(dx * dx + dy * dy);
                            var diff = part.Radius + p.Radius - distance;

                            var nx = dx / distance; // normalized
                            var ny = dy / distance;
                            part.X = (float)(nextX - (p.Radius - distance + part.Radius) * nx);
                            part.Y = (float)(nextY - (p.Radius - distance + part.Radius) * ny);
                        }
                    }
                }
                */
                if (nextX > Game.MaxLocationX)
                    part.X = Game.MaxLocationX;
                if (nextX < 0)
                    part.X = 0;
                if (nextY > Game.MaxLocationY)
                    part.Y = Game.MaxLocationY;
                if (nextY < 0)
                    part.Y = 0;

                doneParts.Add(part);

                command.Movement.Add(new Tuple<int, float, float>(part.Identifier, part.X, part.Y));
            }

            game.ServerConnection.SendAsync(command);
        }

        private bool AreInCollision(PlayerPart part1, PlayerPart part2)
        {
            var dx = part2.X - part1.X;
            var dy = part2.Y - part1.Y;
            var distance = Math.Sqrt(dx * dx + dy * dy);
            return distance < part1.Radius + part2.Radius;
        }

    }
}
