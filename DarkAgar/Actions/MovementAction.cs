using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DarkAgar.Entities;
using System.Diagnostics;

namespace DarkAgar.Actions
{
    /// <summary>
    /// Represents the movement action.
    /// </summary>
    /// <seealso cref="DarkAgar.Actions.Action" />
    class MovementAction : Action
    {
        /// <summary>
        /// Creates new player action.
        /// </summary>
        /// <param name="mousePosition">Mouse position that will be used for the action.</param>
        public MovementAction(Point mousePosition) : base(mousePosition)
        {
        }

        /// <summary>
        /// Processes player action. It will create appropriate command and
        /// send to the server and also updates predictions.
        /// </summary>
        /// <param name="game">Game in which the player action takes place.</param>
        public override void Process(Game game)
        {
            var state = game.GameState;

            if (state?.CurrentPlayer?.Parts == null)
                return;

            var command = new Commands.Move();
            command.Time = game.Time;
            command.Movement = new List<Tuple<int, float, float, float>>();

            var doneParts = new List<PlayerPart>();

            state.CurrentPlayer.Parts.Sort((p1, p2) =>
            {
                var d1 = Math.Sqrt((p1.X - X) * (p1.X - X) + (p1.Y - Y) * (p1.Y - Y));
                var d2 = Math.Sqrt((p2.X - X) * (p2.X - X) + (p2.Y - Y) * (p2.Y - Y));
                return d1.CompareTo(d2);
            }); // consistence

            foreach (var part in state.CurrentPlayer.Parts)
            {

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
                    part.DivisionTime = PlayerPart.DefaultDivisionTime - 1;
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

                foreach (var p in doneParts) // or state.CurrentPlayer.Parts
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

                            //Debug.WriteLine($"Normalizing {p.Identifier} because of collision with {part.Identifier}");
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

                foreach (var food in state.Food)
                    if (CanBeEaten(food, nextX, nextY, part))
                    {
                        part.Mass += food.Mass;
                        eatenFood.Add(food);
                        if (!state.EatenFoodPrediction.Contains(food))
                            state.EatenFoodPrediction.Add(food);
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

        /// <summary>
        /// Return true if <paramref name="part1"/> is in collision with <paramref name="part2"/>.
        /// </summary>
        /// <param name="part1">The part1.</param>
        /// <param name="part2">The part2.</param>
        /// <returns><c>true</c> if <paramref name="part1"/> is in collision with <paramref name="part2"/>, <c>false</c> otherwise.</returns>
        private bool AreInCollision(PlayerPart part1, PlayerPart part2)
        {
            var dx = part2.X - part1.X;
            var dy = part2.Y - part1.Y;
            var distance = Math.Sqrt(dx * dx + dy * dy);
            return distance <= 0.99999 * (part1.Radius + part2.Radius);
        }

        /// <summary>
        /// Return true if the part with coordinates (<paramref name="x1"/>, <paramref name="x2"/>)
        /// and radius <paramref name="radius1"/> is in collision with <paramref name="part2"/>.
        /// </summary>
        /// <param name="x1">The x coordinate of the first part.</param>
        /// <param name="y1">The y coordinate of the first part.</param>
        /// <param name="radius1">The radius of the first part.</param>
        /// <param name="part2">Second part.</param>
        /// <returns><c>true</c> if the part with coordinates (<paramref name="x1"/>, <paramref name="x2"/>)
        /// and radius <paramref name="radius1"/> is in collision with <paramref name="part2"/>, <c>false</c> otherwise.</returns>
        private static bool AreInCollision(float x1, float y1, float radius1, PlayerPart part2)
        {
            var dx = part2.X - x1;
            var dy = part2.Y - y1;
            var distance = Math.Sqrt(dx * dx + dy * dy);
            return distance <= 0.99999 * (radius1 + part2.Radius);
        }

        /// <summary>
        /// Determines whether <paramref name="food"/> can be eaten by <paramref name="part"/>
        /// when the part is on the position (<paramref name="nextX"/>, <paramref name="nextY"/>).
        /// </summary>
        /// <param name="food">The food.</param>
        /// <param name="nextX">The next x.</param>
        /// <param name="nextY">The next y.</param>
        /// <param name="part">The part.</param>
        /// <returns><c>true</c> if  <paramref name="food"/> can be eaten by <paramref name="part"/>
        /// when the part is on the position (<paramref name="nextX"/>, <paramref name="nextY"/>).; otherwise, <c>false</c>.</returns>
        private static bool CanBeEaten(Food food, float nextX, float nextY, PlayerPart part)
        {
            var dx = food.X - nextX;
            var dy = food.Y - nextY;
            var distance = Math.Sqrt(dx * dx + dy * dy);

            return (distance < part.Radius - food.Radius &&    // is completely inside
                part.Mass > 1.25 * food.Mass);
        }

    }
}
