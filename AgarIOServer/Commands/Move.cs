using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgarIOServer.Entities;
using System.Diagnostics;
using System.Threading;

namespace AgarIOServer.Commands
{
    [ProtoBuf.ProtoContract]
    class Move : Command
    {
        /// <summary>
        /// Specifies where player parts should be moved.
        /// First argument of the tuple is part identifier and other two arguments are new coordinates.
        /// </summary>
        [ProtoBuf.ProtoMember(1)]
        public List<Tuple<int, float, float>> Movement { get; set; }

        [ProtoBuf.ProtoMember(2)]
        public int Time { get; set; }

        public override void Process(GameServer server, string playerName)
        {
            var state = server.GameState;
            Player player = null;

            lock (state.Players)
            {
                player = state.Players.Find(p => p.Name == playerName);
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


                //var currentServerTime = Stopwatch.GetTimestamp();
                //var deltaServerTime = 1000 * (currentServerTime - player.LastMovementServerTime) / Stopwatch.Frequency; // (ms)
                //double deltaMovementTime = Time - player.LastMovementTime; // 1 tick per movement
                //var deltaGameTime = deltaMovementTime * GameServer.GameLoopInterval;


                switch (Check(player))
                {
                    // KICK CHEATERS !!!
                    case MovementCheckResult.Overspeed:
                        //server.ConnectionManager.SendToClient(playerName, new Stop("Speed hack detected!"));
                        //server.ConnectionManager.EndClientConnection(playerName);
                        //server.RemovePlayer(playerName, "Speed hack detected!");
                        server.ConnectionManager.SendToClient(playerName, new Invalidate("Invalid movement"));

                        break;
                    case MovementCheckResult.InvalidLocation:
                        //server.ConnectionManager.SendToClient(playerName, new Stop("Invalid location!"));
                        //server.ConnectionManager.EndClientConnection(playerName);
                        //server.RemovePlayer(playerName, "Invalid location!");
                        server.ConnectionManager.SendToClient(playerName, new Invalidate("Invalid movement"));

                        break;
                    #region speed fixing
                    // possible fixing ...
                    /*
                    if (deltaGameTime > deltaServerTime)
                        deltaMovementTime = (double)deltaServerTime / GameServer.GameLoopInterval;

                    foreach (var part in player.Parts)
                    {
                        var movement = Movement.Find(t => t.Item1 == part.Identifier);

                        double dx = movement.Item2 - part.X;
                        double dy = movement.Item3 - part.Y;

                        double size = Math.Sqrt(dx * dx + dy * dy);
                        if (size == 0) // no movement (corner)
                            return;
                        // normalize
                        dx /= size;
                        dy /= size;

                        var nextX = part.X + (float)(dx * part.Speed * deltaMovementTime);
                        var nextY = part.Y + (float)(dy * part.Speed * deltaMovementTime);

                        //Console.WriteLine("oldX {0} oldY {1} newX {2} newY {3}", part.X, part.Y, movement.Item2, movement.Item3);

                        // fix location
                        if (nextX > GameServer.MaxLocationX || nextX < 0)
                            nextX = part.X;
                        if (nextY > GameServer.MaxLocationY || nextY < 0)
                            nextY = part.Y;

                        // apply
                        part.X = nextX;
                        part.Y = nextY;
                    }
                    server.ConnectionManager.SendToClient(playerName, new Invalidate("Movement speed is too fast!"));
                    //server.ConnectionManager.SendToClient(playerName, new UpdateState(server.GameState));

                    break;
                    */
                    #endregion
                    case MovementCheckResult.OK:
                        bool toBeInvalidated = false;
                        //Console.WriteLine("Entering player lock  " + playerName);
                        //Console.WriteLine("Entered player lock  " + playerName);
                        List<PlayerPart> partsStillInsideOtherParts = new List<PlayerPart>();
                        List<PlayerPart> partsToBeMerged = new List<PlayerPart>();
                        List<PlayerPart> ejectedPartsToBeRemoved = new List<PlayerPart>();

                        foreach (var part in player.Parts)
                        {
                            var movement = Movement.Find(t => t.Item1 == part.Identifier);
                            //Console.WriteLine("oldX {0} oldY {1} newX {2} newY {3}", part.X, part.Y, movement.Item2, movement.Item3);

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
                                lock (state.Food)
                                {
                                    state.Food.Add(new Food(part.X, part.Y, part.Mass)
                                    {
                                        Color = player.Color
                                    });
                                }
                            }

                            // part.Mass++;
                            // toBeInvalidated = true;
                        }

                        if (player.Parts.RemoveAll(p => ejectedPartsToBeRemoved.Contains(p)) > 0)
                            toBeInvalidated = true;

                        partsToBeMerged.ForEach(p => p.IsMerged = false );

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

                        // eating food

                        var eatenFood = new HashSet<Food>();
                        var newFood = new List<Food>();
                        lock (server.GameState.Food)
                        {
                            foreach (var food in server.GameState.Food)
                                foreach (var part in player.Parts)
                                    if (CanBeEaten(food, part))
                                    {
                                        part.Mass += food.Mass;
                                        eatenFood.Add(food);
                                        // new food
                                        newFood.Add(GenerateNewFood());
                                    }
                            if (eatenFood.Count > 0)
                                toBeInvalidated = true;

                            server.GameState.Food.RemoveAll(f => eatenFood.Contains(f));
                            newFood.ForEach(f => server.GameState.Food.Add(f));
                        }

                        // eating player

                        var eatenPlayers = new HashSet<Player>();
                        lock (server.GameState.Players)
                        {
                            foreach (var otherPlayer in server.GameState.Players)
                            {
                                //Console.WriteLine("Entering player lock with try  " + playerName);
                                if (Monitor.TryEnter(otherPlayer)) // deadlock prevention
                                {
                                    // Console.WriteLine("Entered player lock with try  " + playerName);
                                    if (player.Name == otherPlayer.Name)
                                    {
                                        Monitor.Exit(otherPlayer);
                                        // Console.WriteLine("Exited player lock with try  " + playerName);
                                        continue;
                                    }

                                    HashSet<PlayerPart> partsToBeRemoved = new HashSet<PlayerPart>();

                                    foreach (var part in player.Parts)
                                        foreach (var otherPlayerPart in otherPlayer.Parts)
                                        {
                                            if (CanBeEaten(part, otherPlayerPart) && !partsToBeRemoved.Contains(otherPlayerPart)
                                                && !partsToBeRemoved.Contains(part))
                                            {
                                                part.Mass += otherPlayerPart.Mass;
                                                partsToBeRemoved.Add(otherPlayerPart);
                                            }
                                            if (CanBeEaten(otherPlayerPart, part) && !partsToBeRemoved.Contains(otherPlayerPart)
                                                && !partsToBeRemoved.Contains(part))
                                            {
                                                otherPlayerPart.Mass += part.Mass;
                                                partsToBeRemoved.Add(part);
                                            }
                                        }
                                    if (partsToBeRemoved.Count > 0)
                                    {
                                        toBeInvalidated = true;
                                        server.ConnectionManager.SendToClient(otherPlayer.Name, new Invalidate("Eating"));
                                    }

                                    otherPlayer.Parts.RemoveAll(p => partsToBeRemoved.Contains(p));
                                    player.Parts.RemoveAll(p => partsToBeRemoved.Contains(p));

                                    if (otherPlayer.Parts.Count == 0)
                                        eatenPlayers.Add(otherPlayer); // cannot be removed while enumerated!

                                    if (player.Parts.Count == 0)
                                        eatenPlayers.Add(player);

                                    Monitor.Exit(otherPlayer);
                                    //Console.WriteLine("Exited player lock with try  " + playerName);
                                }
                            }

                            foreach (var eatenPlayer in eatenPlayers)
                            {
                                server.RemovePlayer(eatenPlayer.Name, "You have been eaten!");
                            }

                        }

                        if (ProcessViruses(player, server))
                            toBeInvalidated = true;

                        if (toBeInvalidated)
                            server.ConnectionManager.SendToClient(playerName, new Invalidate("Invalid movement"));
                        break;

                    case MovementCheckResult.OtherError:
                        //server.ConnectionManager.SendToClient(playerName, new Invalidate("Invalid movement command!"));
                        //server.RemovePlayer(playerName, "Invalid movement command!");
                        server.ConnectionManager.SendToClient(playerName, new Invalidate("Invalid movement"));
                        break;
                }
                player.LastMovementTime = Time;
            }
        }

        private enum MovementCheckResult
        {
            Overspeed, InvalidLocation, OK, OtherError
        }

        private MovementCheckResult Check(Player player)
        {
            var res = MovementCheckResult.OK;
            //return res;

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

            // if (deltaServerTime < GameServer.GameLoopInterval)
            //     Console.WriteLine(deltaServerTime);

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
                    Console.WriteLine("edge error");
                   // res = MovementCheckResult.OtherError;
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
                    //res = MovementCheckResult.Overspeed;
                }

                // rollback
                part.MergeTime++;
                part.DivisionTime++;
            }
            if (res == MovementCheckResult.Overspeed)
                Console.WriteLine("Ended with overspeed");
            else if (res == MovementCheckResult.InvalidLocation)
                Console.WriteLine("Invalid location");
            else if (res == MovementCheckResult.OtherError)
                Console.WriteLine("other error");

            return res;
        }

        private bool IsOnEdge(float x, float y)
        {
            bool isX = x == GameServer.MaxLocationX || x == 0;
            bool isY = y == GameServer.MaxLocationY || y == 0;
            return isX || isY;
        }

        private bool AreInCollision(PlayerPart part1, PlayerPart part2)
        {
            var dx = part2.X - part1.X;
            var dy = part2.Y - part1.Y;
            var distance = Math.Sqrt(dx * dx + dy * dy);
            return distance <= 0.9999 * (part1.Radius + part2.Radius);
        }

        private Food GenerateNewFood()
        {
            var random = GameServer.RandomG;
            return new Food(random.Next(GameServer.MaxLocationX), random.Next(GameServer.MaxLocationY),
                random.Next(10, 70));
        }

        private bool ProcessViruses(Player player, GameServer server)
        {
            var res = false;
            lock (server.GameState.Viruses)
            {
                var newParts = new List<PlayerPart>();
                var freeIdentifiers = Enumerable.Range(0, 40).Where(n => !player.Parts.Exists(p => p.Identifier == n)).ToList();
                var i = 0;

                foreach (var part in player.Parts)
                {
                    Virus virus;

                    if (player.Parts.Count < 16 && server.GameState.Viruses.Exists(v => CanBeDividedByVirus(v, part)))
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
                    else if (player.Parts.Count >= 16 && (virus = server.GameState.Viruses.Find(v => CanBeEaten(v, part))) != null) // eat virus
                    {
                        res = true;
                        server.GameState.Viruses.Remove(virus);
                        part.Mass += virus.Mass;
                        newParts.Add(part);

                        // generate new virus
                        var playersParts = new List<PlayerPart>();
                        lock (server.GameState.Players)
                        {
                            foreach (var otherPlayer in server.GameState.Players)
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

                            server.GameState.Viruses.Add(new Virus(playersParts));

                        }
                    }
                    else if (part.IsBeingEjected && (virus = server.GameState.Viruses.Find(v => CanBeEatenByVirus(v, part))) != null)
                    {
                        res = true;
                        virus.Mass += part.Mass;

                        if (virus.Mass > GameServer.MaxVirusSize)
                            DivideVirus(virus, server);

                        // part wont be added to newParts -> therefore will be removed
                    }
                    else
                        newParts.Add(part);
                }
                player.Parts = newParts;

                // virus animation
                foreach (var virus in server.GameState.Viruses)
                {
                    if (virus.AnimationEndTime == virus.AnimationStartTime)
                        continue; // no animation

                    var animationTime = ((double)Stopwatch.GetTimestamp() - virus.AnimationStartTime) /
                        (virus.AnimationEndTime - virus.AnimationStartTime);

                    if (animationTime >= 1)
                    {
                        virus.X = virus.EndX;
                        virus.Y = virus.EndY;
                        virus.AnimationStartTime = virus.AnimationEndTime = 0;
                    }
                    else
                    {
                        virus.X = (float)(virus.StartX + animationTime * (virus.EndX - virus.StartX));
                        virus.Y = (float)(virus.StartY + animationTime * (virus.EndY - virus.StartY));
                    }
                }
            }
            return res;
        }

        private void DivideVirus(Virus virus, GameServer server)
        {
            var numberOfNewViruses = virus.Mass / GameServer.DefaultVirusSize;
            var newViruses = new List<Virus>();

            for (int i = 0; i < numberOfNewViruses; i++)
            {
                var newVirus = new Virus();
                newVirus.Mass = GameServer.DefaultVirusSize;

                var maxDiff = virus.Radius * 6;

                var minX = (int)Math.Round(Math.Max(0, virus.X - maxDiff));
                var maxX = (int)Math.Round(Math.Min(GameServer.MaxLocationX, virus.X + maxDiff));
                var minY = (int)Math.Round(Math.Max(0, virus.Y - maxDiff));
                var maxY = (int)Math.Round(Math.Min(GameServer.MaxLocationY, virus.Y + maxDiff));

                do
                {
                    newVirus.EndX = GameServer.RandomG.Next(minX, maxX);
                    newVirus.EndY = GameServer.RandomG.Next(minY, maxY);
                } while (newViruses.Exists(v => WillBeInCollision(newVirus, v)));

                newVirus.AnimationStartTime = Stopwatch.GetTimestamp();
                newVirus.AnimationEndTime = Stopwatch.GetTimestamp() + Stopwatch.Frequency * 1; // 5s animation
                newVirus.StartX = virus.X;
                newVirus.StartY = virus.Y;
                newViruses.Add(newVirus);
                server.GameState.Viruses.Add(newVirus);
            }

            server.GameState.Viruses.Remove(virus);
        }

        private bool WillBeInCollision(Virus virus1, Virus virus2)
        {
            var dx = virus2.EndX - virus1.EndX;
            var dy = virus2.EndY - virus1.EndY;
            var distance = Math.Sqrt(dx * dx + dy * dy);
            return distance < virus1.Radius + virus2.Radius;
        }

        private bool CanBeDividedByVirus(Virus virus, PlayerPart part)
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

        private bool CanBeEatenByVirus(Virus virus, PlayerPart part)
        {
            if (!part.IsBeingEjected)
                return false;

            var dx = virus.X - part.X;
            var dy = virus.Y - part.Y;
            var distance = Math.Sqrt(dx * dx + dy * dy);

            return distance <= virus.Radius;
        }

        private bool CanBeEaten(Virus virus, PlayerPart part)
        {
            if (part.IsBeingEjected)
                return false;

            var dx = virus.X - part.X;
            var dy = virus.Y - part.Y;
            var distance = Math.Sqrt(dx * dx + dy * dy);

            return distance < part.Radius - virus.Radius &&
                part.Mass > 1.25 * virus.Mass;
        }

        private bool CanBeMerged(PlayerPart part1, PlayerPart part2)
        {
            if (part1.MergeTime > 0 || part2.MergeTime > 0)
                return false;
            var dx = part2.X - part1.X;
            var dy = part2.Y - part1.Y;
            var distance = Math.Sqrt(dx * dx + dy * dy);
            return distance < 0.5 * (part1.Radius + part2.Radius);
        }

        private bool CanBeEaten(PlayerPart eatingPart, PlayerPart partToBeEaten)
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

        private bool CanBeEaten(Food food, PlayerPart playerPart)
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