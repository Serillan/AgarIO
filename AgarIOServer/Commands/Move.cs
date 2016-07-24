using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgarIOServer.Entities;
using System.Diagnostics;

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
                    server.RemovePlayer(playerName, "Speed hack detected!");
                    break;
                case MovementCheckResult.InvalidLocation:
                    //server.ConnectionManager.SendToClient(playerName, new Stop("Invalid location!"));
                    //server.ConnectionManager.EndClientConnection(playerName);
                    server.RemovePlayer(playerName, "Invalid location!");
                    break;

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
                case MovementCheckResult.OK:
                    lock (player)
                    {
                        foreach (var part in player.Parts)
                        {
                            var movement = Movement.Find(t => t.Item1 == part.Identifier);
                            //Console.WriteLine("oldX {0} oldY {1} newX {2} newY {3}", part.X, part.Y, movement.Item2, movement.Item3);

                            part.X = movement.Item2;
                            part.Y = movement.Item3;
                        }
                        player.LastMovementTime = Time;
                    }
                    break;

                case MovementCheckResult.OtherError:
                    //server.ConnectionManager.SendToClient(playerName, new Invalidate("Invalid movement command!"));
                    server.RemovePlayer(playerName, "Invalid movement command!");
                    break;
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

                if (distance > part.Speed * 1.1 && res == MovementCheckResult.OK) // 0.1 error rate allowed
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
    }
}
