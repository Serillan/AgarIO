using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgarIO.Entities;
using System.Diagnostics;

namespace AgarIO.Actions
{
    class MovementAction : Action
    {
        public MovementAction(Point MousePosition) : base(MousePosition)
        {
        }

        public override void Process(Game game)
        {
            var state = game.GameState;

            if (state?.CurrentPlayer?.Parts == null)
                return;

            var command = new Commands.Move();
            command.Time = game.Time;
            command.Movement = new List<Tuple<int, float, float, float>>();

            List<PlayerPart> doneParts = new List<PlayerPart>();

            state.CurrentPlayer.Parts.Sort((p1, p2) =>
            {
                var d1 = Math.Sqrt((p1.X - X) * (p1.X - X) + (p1.Y - Y) * (p1.Y - Y));
                var d2 = Math.Sqrt((p2.X - X) * (p2.X - X) + (p2.Y - Y) * (p2.Y - Y));
                return d1.CompareTo(d2);
            }); // consistence

            foreach (var part in state.CurrentPlayer.Parts)
            {
                //float vX = (float)(X - GraphicsEngine.GamePanelWidth / 2);
                //float vY = (float)(Y - GraphicsEngine.GamePanelHeight / 2);

                float vX = X - part.X;
                float vY = Y - part.Y;

                if (part.IsBeingEjected)
                {
                    vX = part.EjectedVX;
                    vY = part.EjectedVY;
                }


                // normalize
                float size = (float)(Math.Sqrt(vX * vX + vY * vY));
                if (size == 0)
                    return;
                vX /= size;
                vY /= size;

                if (part.IsNewDividedPart)
                {
                    part.IsNewDividedPart = false;
                    part.DivisionTime = PlayerPart.DefaulDivisionTime - 1;
                }
                else
                {
                    if (part.DivisionTime > 0)
                        part.DivisionTime--;
                }

                if (part.MergeTime > 0)
                    part.MergeTime--;

                var nextX = part.X + vX * part.Speed;
                var nextY = part.Y + vY * part.Speed;

                if (nextX > Game.MaxLocationX)
                    nextX = Game.MaxLocationX;
                if (nextX < 0)
                    nextX = 0;
                if (nextY > Game.MaxLocationY)
                    nextY = Game.MaxLocationY;
                if (nextY < 0)
                    nextY = 0;

                foreach (var p in doneParts) // state.CurrentPlayer.Parts
                {
                    if (p == part)
                        continue;

                    if (AreInCollision(nextX, nextY, part.Radius, p) && (part.MergeTime > 0 || p.MergeTime > 0) &&
                        part.DivisionTime == 0 && p.DivisionTime == 0 && !p.IsBeingEjected && !part.IsBeingEjected)
                    {
                        var dx = p.X - nextX;
                        var dy = p.Y - nextY;
                        var distance = Math.Sqrt(dx * dx + dy * dy);
                        if (distance != 0)
                        {
                            var diff = part.Radius + p.Radius - distance;

                            var nx = dx / distance; // normalized
                            var ny = dy / distance;
                            nextX -= (float)((p.Radius - distance + part.Radius) * nx);
                            nextY -= (float)((p.Radius - distance + part.Radius) * ny);

                            Debug.WriteLine($"Normalizing {p.Identifier} because of collision with {part.Identifier}");

                            // possible movement inside many parts fix (not working)
                            /*
                            if (doneParts.Exists(p2 =>     // if there is still collision
                            p2 != part && AreInCollision(nextX, nextY, part.Radius, p2) && (part.MergeTime > 0 || p2.MergeTime > 0) &&
                            part.DivisionTime == 0 && p2.DivisionTime == 0))
                            {
                                // then no movement is applied
                                nextX = part.X;
                                nextY = part.Y;
                            }
                            */
                        }
                    }

                }


                if (nextX > Game.MaxLocationX)
                    nextX = Game.MaxLocationX;
                if (nextX < 0)
                    nextX = 0;
                if (nextY > Game.MaxLocationY)
                    nextY = Game.MaxLocationY;
                if (nextY < 0)
                    nextY = 0;

                // food eating prediction
                var eatenFood = new HashSet<Food>();

                foreach (var food in game.GameState.Food)
                    if (CanBeEaten(food, nextX, nextY, part))
                    {
                        part.Mass += food.Mass;
                        eatenFood.Add(food);
                    }
                game.GameState.Food.RemoveAll(f => eatenFood.Contains(f));


                doneParts.Add(part);
                command.Movement.Add(new Tuple<int, float, float, float>(part.Identifier, nextX, nextY, part.Mass));
            }

            // apply prediction
            foreach (var partMove in command.Movement)
            {
                var part = game.GameState.CurrentPlayer.Parts.Find(p => p.Identifier == partMove.Item1);
                part.X = partMove.Item2;
                part.Y = partMove.Item3;
            }

            game.ServerConnection.SendAsync(command);
        }

        private bool AreInCollision(PlayerPart part1, PlayerPart part2)
        {
            var dx = part2.X - part1.X;
            var dy = part2.Y - part1.Y;
            var distance = Math.Sqrt(dx * dx + dy * dy);
            var res = distance <= 0.99999 * (part1.Radius + part2.Radius);
            if (res == true)
                Console.WriteLine("");
            return res;
        }

        private bool AreInCollision(float x1, float y1, float radius1, PlayerPart part2)
        {
            var dx = part2.X - x1;
            var dy = part2.Y - y1;
            var distance = Math.Sqrt(dx * dx + dy * dy);
            var res = distance <= 0.99999 * (radius1 + part2.Radius);
            if (res == true)
                Console.WriteLine("");
            return res;
        }

        private bool CanBeEaten(Food food, float nextX, float nextY, PlayerPart part)
        {
            var dx = food.X - nextX;
            var dy = food.Y - nextX;
            var distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance < part.Radius - food.Radius &&    // is completely inside
                part.Mass > 1.25 * food.Mass)
                return true;

            return false;
        }

    }
}
