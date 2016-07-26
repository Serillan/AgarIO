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
                        List<PlayerPart> partToBeMerged = new List<PlayerPart>();

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
                                partToBeMerged.Add(part);
                            if (!part.IsOutOfOtherParts)
                                partsStillInsideOtherParts.Add(part);

                            part.Mass++;
                            toBeInvalidated = true;
                        }

                        foreach (var part in partToBeMerged)
                        {
                            var mergingPart = partToBeMerged.Find(p => p != part && CanBeMerged(part, p));
                            if (mergingPart != null) // merge
                            {
                                part.Mass += mergingPart.Mass;
                                part.MergeTime = (int)Math.Round((0.02 * part.Mass + 5) * 1000 / GameServer.GameLoopInterval);
                                player.Parts.Remove(mergingPart);
                                toBeInvalidated = true;
                            }
                        }

                        // eating

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
                                        toBeInvalidated = true;

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

                            foreach (var part in partsStillInsideOtherParts)
                            {
                                if (!player.Parts.Exists(p => p != part && AreInCollision(part, p)))
                                    part.IsOutOfOtherParts = true;
                            }
                        }
                        if (toBeInvalidated == true)
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
                var movement = Movement.Find(t => t.Item1 == part.Identifier);
                if (movement == null) // movement of certain part is missing
                    res = MovementCheckResult.OtherError;

                if (movement.Item2 > GameServer.MaxLocationX || movement.Item2 < 0 || movement.Item3 > GameServer.MaxLocationY || movement.Item3 < 0)
                    res = MovementCheckResult.InvalidLocation;

                float dx = movement.Item2 - part.X;
                float dy = movement.Item3 - part.Y;

                double distance = Math.Sqrt(dx * dx + dy * dy);

                if (distance == 0 && !IsOnEdge(movement.Item2, movement.Item3))
                    res = MovementCheckResult.OtherError;

                distance = distance / deltaMovementTime; // distance per movement
                if (player.FirstMovementTime == Time) // first movement - next test is bypassed
                    continue;

                if (distance > part.Speed * 1.3 && res == MovementCheckResult.OK &&  // 0.3 error rate allowed
                    !player.Parts.Exists(p => p != part && AreInCollision(p, part))) // when it is in collision, it can be moved faster!
                {
                    Console.WriteLine("Distance error, Distance : {0} | deltaMovementTime : {1}",
                        distance, deltaMovementTime);
                    res = MovementCheckResult.Overspeed;
                }
            }

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
            return distance < part1.Radius + part2.Radius;
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
            var dx = partToBeEaten.X - eatingPart.X;
            var dy = partToBeEaten.Y - eatingPart.Y;
            var distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance < eatingPart.Radius - partToBeEaten.Radius &&    // is completely inside
                eatingPart.Mass > 1.25 * partToBeEaten.Mass)
                return true;

            return false;
        }
    }
}
