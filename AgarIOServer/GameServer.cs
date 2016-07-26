using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using AgarIOServer.Entities;
using AgarIOServer.Actions;
using AgarIOServer.Commands;
using System.Timers;
using System.Diagnostics;
using System.Threading;

namespace AgarIOServer
{
    class GameServer
    {
        public const int GameLoopInterval = 30;
        public const int ServerLoopInterval = 30; // not used right now 
        public const int MaxLocationX = 2400;
        public const int MaxLocationY = 2400;
        public static Random RandomG = new Random();
        public ConnectionManager ConnectionManager { get; set; }
        public GameState GameState { get; set; }


        public GameServer(ConnectionManager connectionManager)
        {
            this.ConnectionManager = connectionManager;
        }

        public void Start()
        {
            GameState = GenerateNewGameState();
            ConnectionManager.PlayerCommandHandler = ProcessClientCommand;
            ConnectionManager.NewPlayerHandler = AddNewPlayer;
            //Timer timer = new Timer();
            //timer.Interval = ServerLoopInterval;
            //timer.Elapsed += ServerLoop;
            //timer.Start();

            //Task.Factory.StartNew((System.Action) ServerLoop, TaskCreationOptions.LongRunning);
        }

        public GameState GenerateNewGameState()
        {
            GameState state = new GameState();
            return state;
        }

        private void ServerLoop()
        {
            long a = 0, b = 0;
            long delta = 0;
            while (true)
            {
                b = Stopwatch.GetTimestamp();
                delta = 1000 * (b - a) / Stopwatch.Frequency;
                if (delta >= ServerLoopInterval)
                {
                    lock (GameState)
                    {
                        lock (ConnectionManager.Connections)
                        {
                            GameState.Version++;
                            ConnectionManager.SendToAllClients(new UpdateState(GameState));
                        }
                    }
                    //Console.WriteLine(1000 * (b - a) / Stopwatch.Frequency);
                    a = Stopwatch.GetTimestamp();
                }
            }

        }

        private void ProcessClientCommand(string playerName, Command command)
        {
            Interlocked.Add(ref GameState.Version, 1);
            command.Process(this, playerName);
            //Console.WriteLine("sending state");
            lock (GameState) // TODO - with higher server load - it can be done in loop
            {
                ConnectionManager.SendToAllClients(new UpdateState(GameState));
            }
        }

        private void AddNewPlayer(string playerName)
        {
            Player newPlayer = new Player(playerName);
            lock (GameState.Players)
            {
                GameState.Players.Add(newPlayer);
            }

            Interlocked.Add(ref GameState.Version, 1);
            ConnectionManager.SendToAllClients(new UpdateState(GameState));
        }

        public void RemovePlayer(string playerName, string msg)
        {


            ConnectionManager.SendToClient(playerName, new Stop(msg));

            lock (GameState.Players)
            {
                GameState.Players.RemoveAll(p => p.Name == playerName);
            }

            ConnectionManager.EndClientConnection(playerName);
        }

        private void ProcessClientCommand(string playerName, string msg)
        {
            string[] tokens = msg.Split();
            float x = 0, y = 0;
            int time = 0;
            lock (GameState)
            {
                switch (tokens[0])
                {
                    case "STOP":
                        lock (GameState)
                            GameState.Players.RemoveAll(p => p.Name == playerName);
                        ConnectionManager.EndClientConnection(playerName);
                        break;
                    case "CONNECTED":
                        Player newPlayer = new Player(playerName);
                        lock (GameState)
                            GameState.Players.Add(newPlayer);
                        break;
                }

                if (tokens.Length == 4 && int.TryParse(tokens[1], out time) && float.TryParse(tokens[2], out x) && float.TryParse(tokens[3], out y))
                {
                    switch (tokens[0])
                    {
                        case "MOVE":
                            if (ConnectionManager.Connections.Find(p => p.PlayerName == playerName).LastMovementTime < time)
                            {
                                new MovementAction(x, y, playerName).Process(GameState);
                                ConnectionManager.Connections.Find(p => p.PlayerName == playerName).LastMovementTime = time;
                            }
                            break;
                        case "DIVIDE":
                            new DivideAction(x, y, playerName).Process(GameState);
                            break;
                        case "GIVEFOOD":
                            new FoodAction(x, y, playerName).Process(GameState);
                            break;
                    }
                }
                GameState.Version++;
                lock (ConnectionManager.Connections)
                    ConnectionManager.SendToAllClients(new UpdateState(GameState));
            }
            
        }
    }
}
