using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DarkAgarServer.Entities;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace DarkAgarServer.Commands
{
    /// <summary>
    /// Represents the movement command.
    /// </summary>
    /// <seealso cref="DarkAgarServer.Commands.Command" />
    [ProtoBuf.ProtoContract]
    class Move : Command
    {
        /// <summary>
        /// Specifies where player parts should be moved.
        /// First argument of the tuple is part identifier and other two arguments are new coordinates.
        /// </summary>
        /// <value>The movement.</value>
        [ProtoBuf.ProtoMember(1)]
        public List<Tuple<int, float, float, float>> Movement { get; set; }

        /// <summary>
        /// Gets or sets the time that describes in which cycle of the game loop the movement took place.
        /// Used for synchronization with the server.
        /// </summary>
        /// <value>The time.</value>
        [ProtoBuf.ProtoMember(2)]
        public long Time { get; set; }

        /// <summary>
        /// Processes the command received from the client.
        /// </summary>
        /// <param name="gameServer">The Game Server in which the command takes place.</param>
        /// <param name="playerName">Name of the player.</param>
        public override void Process(GameServer gameServer, string playerName)
        {
            var gameState = gameServer.GameState;
            Player player = null;

            lock (gameState.Players)
            {
                player = gameState.Players.Find(p => p.Name == playerName);
            }

            if (player == null)
                return;

            lock (player)
            {
                if (Time < player.LastMovementTime)
                    return;

                if (player.FirstMovementServerTime == 0)
                {
                    player.FirstMovementServerTime = Stopwatch.GetTimestamp();
                    player.LastMovementTime = Time;
                    player.FirstMovementTime = Time;
                }

                switch (Check(player))
                {
                    case MovementCheckResult.Overspeed:
                        gameServer.ConnectionManager.SendToClient(playerName, new Invalidate("Invalid movement"));

                        break;

                    case MovementCheckResult.InvalidLocation:
                        gameServer.ConnectionManager.SendToClient(playerName, new Invalidate("Invalid movement"));

                        break;

                    case MovementCheckResult.OK:

                        bool toBeInvalidated = false;

                        var partsStillInsideOtherParts = new List<PlayerPart>();
                        var partsToBeMerged = new List<PlayerPart>();
                        var ejectedPartsToBeRemoved = new List<PlayerPart>();

                        foreach (var part in player.Parts)
                        {
                            var movement = Movement.Find(t => t.Item1 == part.Identifier);

                            part.X = movement.Item2;
                            part.Y = movement.Item3;

                            if (part.DivisionTime > 0)
                                part.DivisionTime--;
                            if (part.MergeTime > 0)
                                part.MergeTime--;

                            if (part.MergeTime == 0)
                                partsToBeMerged.Add(part);
                            part.IsNewDividedPart = false;

                            if (part.DivisionTime == 0 && part.IsBeingEjected)
                            {
                                ejectedPartsToBeRemoved.Add(part);
                                lock (gameState.Food)
                                {
                                    gameState.Food.Add(new Food(part.X, part.Y, part.Mass)
                                    {
                                        Color = player.Color
                                    });
                                }
                            }
                        }

                        if (player.Parts.RemoveAll(p => ejectedPartsToBeRemoved.Contains(p)) > 0)
                            toBeInvalidated = true;

                        // merging
                        if (ProcessMerging(player, partsToBeMerged))
                            toBeInvalidated = true;

                        // viruses
                        if (ProcessViruses(player, gameServer.GameState))
                            toBeInvalidated = true;

                        // eating food
                        if (ProcessEatingFood(player, gameServer.GameState))
                            toBeInvalidated = true;

                        // eating players
                        if (ProcessEatingPlayers(player, gameServer))
                            toBeInvalidated = true;

                        if (toBeInvalidated)
                            gameServer.ConnectionManager.SendToClient(playerName, new Invalidate("Invalid movement"));
                        break;

                    case MovementCheckResult.OtherError:
                        // invalidate here would have effect on ejecting teleportation etc. (in case of higher latency)
                        //server.ConnectionManager.SendToClient(playerName, new Invalidate("Invalid movement"));
                        break;
                }
                player.LastMovementTime = Time;
            }
        }

        /// <summary>
        /// Represents the result of the movement check.
        /// </summary>
        private enum MovementCheckResult
        {
            Overspeed, InvalidLocation, OK, OtherError
        }

        /// <summary>
        /// Checks the correctness of the movement.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <returns>MovementCheckResult.</returns>
        private MovementCheckResult Check(Player player)
        {
            var res = MovementCheckResult.OK;

            var currentServerTime = Stopwatch.GetTimestamp();
            var deltaMovementTime = Time - player.LastMovementTime; // 1 tick per movement

            var deltaFromFirstServerTime = 1000 * (currentServerTime - player.FirstMovementServerTime) / Stopwatch.Frequency; // (ms)

            // check whether Game is not running too fast
            var deltaGameTime = deltaMovementTime * GameServer.GameLoopInterval;
            if (deltaFromFirstServerTime > 10 && (Time - player.FirstMovementTime) > deltaFromFirstServerTime / GameServer.GameLoopInterval) // too many ticks in such a time
            {
                res = MovementCheckResult.Overspeed;  // not yet returning because OtherError has higher priority..
                                                      // because of fixing prediction in Process method
                                                      // -> OtherError could be a problem there!
                Console.WriteLine("Allowed number of ticks : {0}, actual number : {1}",
                    deltaFromFirstServerTime / GameServer.GameLoopInterval, Time - player.FirstMovementTime);
            }

            // 1. check whether such a movement can happen
            foreach (var part in player.Parts)
            {
                // apply for check
                part.DivisionTime--;
                part.MergeTime--;

                var movement = Movement.Find(t => t.Item1 == part.Identifier);
                if (movement == null) // movement of certain part is missing
                {
                    res = MovementCheckResult.OtherError;
                    part.DivisionTime++;
                    part.MergeTime++;
                    Console.WriteLine("Missing movement of some part");
                    break;
                }

                if (movement.Item2 > GameServer.MaxLocationX || movement.Item2 < 0 || movement.Item3 > GameServer.MaxLocationY || movement.Item3 < 0)
                    res = MovementCheckResult.InvalidLocation;

                float dx = movement.Item2 - part.X;
                float dy = movement.Item3 - part.Y;

                double distance = Math.Sqrt(dx * dx + dy * dy);

                if (distance == 0 && !IsOnEdge(movement.Item2, movement.Item3))
                {
                    //Console.WriteLine("edge error");
                    //res = MovementCheckResult.OtherError;
                }

                distance = distance / deltaMovementTime; // distance per movement
                if (player.FirstMovementTime == Time) // first movement - next test is bypassed
                {
                    part.DivisionTime++;
                    part.MergeTime++;
                    continue;
                }

                if (distance > part.Speed * 1.01 && res == MovementCheckResult.OK &&  // *3 error rate allowed
                    !player.Parts.Exists(p => p != part && AreInCollision(p, part))) // when it is in collision, it can be moved faster!
                {
                    Console.WriteLine("Distance error, Distance : {0}, Max Allowed : {1} | deltaMovementTime : {2}",
                       distance, part.Speed, deltaMovementTime);
                    res = MovementCheckResult.Overspeed;
                }

                // rollback
                part.MergeTime++;
                part.DivisionTime++;
            }
            /*
            if (res == MovementCheckResult.Overspeed)
                Console.WriteLine("Ended with over speed");
            else if (res == MovementCheckResult.InvalidLocation)
                Console.WriteLine("Invalid location");
            else if (res == MovementCheckResult.OtherError)
                Console.WriteLine("other error");
            */
            return res;
        }

        /// <summary>
        /// Determines whether (<paramref name="x"/>, <paramref name="y"/>) is on the edge.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns><c>true</c> if (<paramref name="x"/>, <paramref name="y"/>) is on the edge; otherwise, <c>false</c>.</returns>
        private static bool IsOnEdge(float x, float y)
        {
            bool isX = Math.Abs(x - GameServer.MaxLocationX) < 0.001 || x == 0;
            bool isY = Math.Abs(y - GameServer.MaxLocationY) < 0.001 || y == 0;
            return isX || isY;
        }

        /// <summary>
        /// Determines whether <paramref name="part1"/> is in collision with <paramref name="part2"/>.
        /// </summary>
        /// <param name="part1">The part1.</param>
        /// <param name="part2">The part2.</param>
        /// <returns><c>true</c> if <paramref name="part1"/> is in collision with <paramref name="part2"/>, <c>false</c> otherwise.</returns>
        private static bool AreInCollision(PlayerPart part1, PlayerPart part2)
        {
            var dx = part2.X - part1.X;
            var dy = part2.Y - part1.Y;
            var distance = Math.Sqrt(dx * dx + dy * dy);
            return distance <= 0.9999 * (part1.Radius + part2.Radius);
        }

        /// <summary>
        /// Generates new food.
        /// </summary>
        /// <returns>Food.</returns>
        private static Food GenerateNewFood()
        {
            var random = GameServer.RandomGenerator;
            return new Food(random.Next(GameServer.MaxLocationX), random.Next(GameServer.MaxLocationY),
                random.Next(GameServer.MinSizeOfFood, GameServer.MaxSizeOfFood));
        }

        /// <summary>
        /// Processes the merging.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="partsToBeMerged">The parts to be merged.</param>
        /// <returns><c>true</c> if player's prediction should be invalidated, <c>false</c> otherwise.</returns>
        private bool ProcessMerging(Player player, List<PlayerPart> partsToBeMerged)
        {
            var toBeInvalidated = false;

            partsToBeMerged.ForEach(p => p.IsMerged = false);

            foreach (var part in partsToBeMerged)
            {
                if (part.IsMerged)
                    continue;

                var mergingPart = partsToBeMerged.Find(p => p != part && !p.IsMerged && CanBeMerged(part, p));
                if (mergingPart != null) // merge
                {
                    part.Mass += mergingPart.Mass;
                    part.MergeTime = (int)Math.Round((0.02 * part.Mass + 5) * 1000 / GameServer.GameLoopInterval);
                    player.Parts.Remove(mergingPart);
                    mergingPart.IsMerged = true;
                    toBeInvalidated = true;
                }
            }

            return toBeInvalidated;
        }

        /// <summary>
        /// Processes the eating of food.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="gameState">State of the game.</param>
        /// <returns><c>true</c> if player's prediction should be invalidated, <c>false</c> otherwise.</returns>
        private bool ProcessEatingFood(Player player, GameState gameState)
        {
            var toBeInvalidated = false;

            var eatenFood = new HashSet<Food>();
            var newFood = new List<Food>();
            lock (gameState.Food)
            {
                foreach (var food in gameState.Food)
                    foreach (var part in player.Parts)
                        if (CanBeEaten(food, part))
                        {
                            var movement = Movement.Find(t => t.Item1 == part.Identifier);
                            part.Mass += food.Mass;

                            if (movement.Item4 != part.Mass) // invalid prediction
                            {
                                Console.WriteLine($"Invalid food prediction mass before eating - {part.Mass - food.Mass} correct - {part.Mass} got - {movement.Item4}");
                                toBeInvalidated = true;
                            }

                            eatenFood.Add(food);
                            // new food
                            newFood.Add(GenerateNewFood());
                        }

                gameState.Food.RemoveAll(f => eatenFood.Contains(f));
                newFood.ForEach(f => gameState.Food.Add(f));

                return toBeInvalidated;
            }
        }

        /// <summary>
        /// Processes viruses.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="gameState">State of the game.</param>
        /// <returns><c>true</c> if player's prediction should be invalidated, <c>false</c> otherwise.</returns>
        private bool ProcessViruses(Player player, GameState gameState)
        {
            var res = false;
            lock (gameState.Viruses)
            {
                var newParts = new List<PlayerPart>();
                var freeIdentifiers = Enumerable.Range(0, 40).Where(n => !player.Parts.Exists(p => p.Identifier == n)).ToList();
                var i = 0;

                foreach (var part in player.Parts)
                {
                    Virus virus;

                    if (player.Parts.Count < GameServer.PlayerMaximumNumberOfPartsForDivision && 
                        gameState.Viruses.Exists(v => CanBeDividedByVirus(v, part)))
                    {
                        res = true;

                        newParts.Add(new PlayerPart()
                        {
                            DivisionTime = 0,
                            Identifier = (byte)freeIdentifiers[i++],
                            IsNewDividedPart = false,
                            Mass = part.Mass / 2,
                            X = part.X,
                            Y = part.Y,
                            MergeTime = (int)Math.Round((0.02 * (part.Mass / 2) + 5) * 1000 / GameServer.GameLoopInterval),
                        });

                        newParts.Add(new PlayerPart()
                        {
                            DivisionTime = 0,
                            Identifier = (byte)freeIdentifiers[i++],
                            IsNewDividedPart = false,
                            Mass = part.Mass / 2,
                            X = part.X,
                            Y = part.Y,
                            MergeTime = (int)Math.Round((0.02 * (part.Mass / 2) + 5) * 1000 / GameServer.GameLoopInterval),
                        });

                    }
                    else if (player.Parts.Count >= GameServer.PlayerMaximumNumberOfPartsForDivision &&
                        (virus = gameState.Viruses.Find(v => CanBeEaten(v, part))) != null) // eat virus
                    {
                        res = true;
                        gameState.Viruses.Remove(virus);
                        part.Mass += virus.Mass;
                        newParts.Add(part);

                        // generate new virus
                        var playersParts = new List<PlayerPart>();
                        lock (gameState.Players)
                        {
                            foreach (var otherPlayer in gameState.Players)
                            {
                                if (Monitor.TryEnter(otherPlayer)) // -> virus sometimes might spawn on the player
                                {
                                    foreach (var otherPlayerPart in otherPlayer.Parts)
                                    {
                                        playersParts.Add(otherPlayerPart); // working with other player fields is thread safe
                                    }

                                    Monitor.Exit(otherPlayer);
                                }
                            }

                            gameState.Viruses.Add(new Virus(playersParts));
                        }
                    }
                    else if (part.IsBeingEjected && (virus = gameState.Viruses.Find(v => CanBeEatenByVirus(v, part))) != null)
                    {
                        res = true;
                        virus.Mass += part.Mass;

                        if (virus.Mass > GameServer.MaxVirusSize)
                            DivideVirus(virus, gameState);

                        // part wont be added to newParts -> therefore will be removed
                    }
                    else
                        newParts.Add(part);
                }
                player.Parts = newParts;

                // virus movement
                foreach (var virus in gameState.Viruses)
                {
                    if (virus.MovementEndTime == virus.MovementStartTime)
                        continue; // no movement

                    var movementTime = ((double)Stopwatch.GetTimestamp() - virus.MovementStartTime) /
                        (virus.MovementEndTime - virus.MovementStartTime);

                    if (movementTime >= 1)
                    {
                        virus.X = virus.EndX;
                        virus.Y = virus.EndY;
                        virus.MovementStartTime = virus.MovementEndTime = 0;
                    }
                    else
                    {
                        virus.X = (float)(virus.StartX + movementTime * (virus.EndX - virus.StartX));
                        virus.Y = (float)(virus.StartY + movementTime * (virus.EndY - virus.StartY));
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Processes the eating of players.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="gameServer">The server.</param>
        /// <returns><c>true</c> if player's prediction should be invalidated, <c>false</c> otherwise.</returns>
        private bool ProcessEatingPlayers(Player player, GameServer gameServer)
        {
            var toBeInvalidated = false;
            var gameState = gameServer.GameState;

            var eatenPlayers = new HashSet<Player>();
            lock (gameState.Players)
            {
                foreach (var otherPlayer in gameState.Players)
                {
                    if (Monitor.TryEnter(otherPlayer)) // deadlock prevention - cycles are fast and other player move might
                    {                                  // process this situation -> no problems
                        if (player.Name == otherPlayer.Name)
                        {
                            Monitor.Exit(otherPlayer);
                            continue;
                        }

                        var partsToBeRemoved = new HashSet<PlayerPart>();

                        foreach (var part in player.Parts)
                            foreach (var otherPlayerPart in otherPlayer.Parts)
                            {
                                if (CanBeEaten(part, otherPlayerPart) && !partsToBeRemoved.Contains(otherPlayerPart)
                                    && !partsToBeRemoved.Contains(part))
                                {
                                    Console.WriteLine("1");
                                    part.Mass += otherPlayerPart.Mass;
                                    partsToBeRemoved.Add(otherPlayerPart);
                                }
                                if (CanBeEaten(otherPlayerPart, part) && !partsToBeRemoved.Contains(otherPlayerPart)
                                    && !partsToBeRemoved.Contains(part))
                                {
                                    Console.WriteLine("2");
                                    otherPlayerPart.Mass += part.Mass;
                                    partsToBeRemoved.Add(part);
                                }
                            }
                        if (partsToBeRemoved.Count > 0)
                        {
                            toBeInvalidated = true;
                            gameServer.ConnectionManager.SendToClient(otherPlayer.Name, new Invalidate("Eating"));
                        }

                        otherPlayer.Parts.RemoveAll(p => partsToBeRemoved.Contains(p));
                        player.Parts.RemoveAll(p => partsToBeRemoved.Contains(p));

                        if (otherPlayer.Parts.Count == 0)
                            eatenPlayers.Add(otherPlayer); // cannot be removed while enumerated!

                        if (player.Parts.Count == 0)
                            eatenPlayers.Add(player);

                        Monitor.Exit(otherPlayer);
                    }
                }

                foreach (var eatenPlayer in eatenPlayers)
                {
                    gameServer.RemovePlayer(eatenPlayer.Name, "You have been eaten!");
                }
            }

            return toBeInvalidated;
        }

        /// <summary>
        /// Divides the virus.
        /// </summary>
        /// <param name="virus">The virus.</param>
        /// <param name="gameState">State of the game.</param>
        private void DivideVirus(Virus virus, GameState gameState)
        {
            var numberOfNewViruses = virus.Mass / GameServer.DefaultVirusSize;
            var newViruses = new List<Virus>();

            for (int i = 0; i < numberOfNewViruses; i++)
            {
                var newVirus = new Virus {Mass = GameServer.DefaultVirusSize};

                var maxDiff = virus.Radius * 6;

                var minX = (int)Math.Round(Math.Max(0, virus.X - maxDiff));
                var maxX = (int)Math.Round(Math.Min(GameServer.MaxLocationX, virus.X + maxDiff));
                var minY = (int)Math.Round(Math.Max(0, virus.Y - maxDiff));
                var maxY = (int)Math.Round(Math.Min(GameServer.MaxLocationY, virus.Y + maxDiff));

                do
                {
                    newVirus.EndX = GameServer.RandomGenerator.Next(minX, maxX);
                    newVirus.EndY = GameServer.RandomGenerator.Next(minY, maxY);
                } while (newViruses.Exists(v => WillBeInCollision(newVirus, v)));

                newVirus.MovementStartTime = Stopwatch.GetTimestamp();
                newVirus.MovementEndTime = Stopwatch.GetTimestamp() + Stopwatch.Frequency * 1; // 5s animation
                newVirus.StartX = virus.X;
                newVirus.StartY = virus.Y;
                newViruses.Add(newVirus);
                gameState.Viruses.Add(newVirus);
            }

            gameState.Viruses.Remove(virus);
        }

        /// <summary>
        /// Determines whether <paramref name="virus1"/> will be at the end 
        /// of the movement (division movement) in collision with <paramref name="virus2"/>.
        /// </summary>
        /// <param name="virus1">The virus1.</param>
        /// <param name="virus2">The virus2.</param>
        /// <returns><c>true</c> if <paramref name="virus1"/> will be at the end 
        /// of the movement (division movement) in collision with <paramref name="virus2"/>, <c>false</c> otherwise.</returns>
        private static bool WillBeInCollision(Virus virus1, Virus virus2)
        {
            var dx = virus2.EndX - virus1.EndX;
            var dy = virus2.EndY - virus1.EndY;
            var distance = Math.Sqrt(dx * dx + dy * dy);
            return distance < virus1.Radius + virus2.Radius;
        }

        /// <summary>
        /// Determines whether the specified part can be divided by the specified virus.
        /// </summary>
        /// <param name="virus">The virus.</param>
        /// <param name="part">The part.</param>
        /// <returns><c>true</c> if the specified part can be divided by the specified virus; otherwise, <c>false</c>.</returns>
        private static bool CanBeDividedByVirus(Virus virus, PlayerPart part)
        {
            if (part.IsBeingEjected)
                return false;

            if (part.Mass < GameServer.MinimumDivisionSize)
                return false;

            var dx = virus.X - part.X;
            var dy = virus.Y - part.Y;
            var distance = Math.Sqrt(dx * dx + dy * dy);

            return distance <= 0.8 * virus.Radius + part.Radius &&    // 20% inside
                part.Mass > 1.25 * virus.Mass;
        }

        /// <summary>
        /// Determines whether the specified part can be eaten by the specified virus.
        /// </summary>
        /// <param name="virus">The virus.</param>
        /// <param name="part">The part.</param>
        /// <returns><c>true</c> if the specified part can be eaten by the specified virus; otherwise, <c>false</c>.</returns>
        private static bool CanBeEatenByVirus(Virus virus, PlayerPart part)
        {
            if (!part.IsBeingEjected)
                return false;

            var dx = virus.X - part.X;
            var dy = virus.Y - part.Y;
            var distance = Math.Sqrt(dx * dx + dy * dy);

            return distance <= virus.Radius;
        }

        /// <summary>
        /// Determines whether the specified virus can be eaten by the specified part.
        /// </summary>
        /// <param name="virus">The virus.</param>
        /// <param name="part">The part.</param>
        /// <returns><c>true</c> if the specified virus can be eaten by the specified part; otherwise, <c>false</c>.</returns>
        private static bool CanBeEaten(Virus virus, PlayerPart part)
        {
            if (part.IsBeingEjected)
                return false;

            var dx = virus.X - part.X;
            var dy = virus.Y - part.Y;
            var distance = Math.Sqrt(dx * dx + dy * dy);

            return distance < part.Radius - virus.Radius &&
                part.Mass > 1.25 * virus.Mass;
        }

        /// <summary>
        /// Determines whether <paramref name="part1"/> can be merged with <paramref name="part2"/>.
        /// </summary>
        /// <param name="part1">The part1.</param>
        /// <param name="part2">The part2.</param>
        /// <returns><c>true</c> if <paramref name="part1"/> can be merged with <paramref name="part2"/>; otherwise, <c>false</c>.</returns>
        private static bool CanBeMerged(PlayerPart part1, PlayerPart part2)
        {
            if (part1.MergeTime > 0 || part2.MergeTime > 0)
                return false;
            var dx = part2.X - part1.X;
            var dy = part2.Y - part1.Y;
            var distance = Math.Sqrt(dx * dx + dy * dy);
            return distance < 0.5 * (part1.Radius + part2.Radius);
        }

        /// <summary>
        /// Determines whether the specified <paramref name="partToBeEaten"/> can be eaten by the specified <paramref name="eatingPart"/>.
        /// </summary>
        /// <param name="eatingPart">The eating part.</param>
        /// <param name="partToBeEaten">The part to be eaten.</param>
        /// <returns><c>true</c> if the specified <paramref name="partToBeEaten"/> can be eaten by the specified <paramref name="eatingPart"/>; otherwise, <c>false</c>.</returns>
        private static bool CanBeEaten(PlayerPart eatingPart, PlayerPart partToBeEaten)
        {
            if (eatingPart.IsBeingEjected)
                return false;

            var dx = partToBeEaten.X - eatingPart.X;
            var dy = partToBeEaten.Y - eatingPart.Y;
            var distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance < eatingPart.Radius - partToBeEaten.Radius &&    // is completely inside
                eatingPart.Mass > 1.25 * partToBeEaten.Mass)
                return true;

            return false;
        }

        /// <summary>
        /// Determines whether the specified food can be eaten by the specified part.
        /// </summary>
        /// <param name="food">The food.</param>
        /// <param name="playerPart">The player part.</param>
        /// <returns><c>true</c> if the specified food can be eaten by the specified part; otherwise, <c>false</c>.</returns>
        private static bool CanBeEaten(Food food, PlayerPart playerPart)
        {
            var dx = food.X - playerPart.X;
            var dy = food.Y - playerPart.Y;
            var distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance < playerPart.Radius - food.Radius &&    // is completely inside
                playerPart.Mass > 1.25 * food.Mass)
                return true;

            return false;
        }
    }
}