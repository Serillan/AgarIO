using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using AgarIOServer.Entities;
using AgarIOServer.Actions;
using System.Timers;
using System.Diagnostics;
using System.Threading;

namespace AgarIOServer
{
    class Game
    {
        const int ServerLoopInterval = 8; // ms
        public const int MaxLocationX = 2400;
        public const int MaxLocationY = 2400;
        public const int ClientLoopInterval = 16;
        public static Random RandomG = new Random();
        ConnectionManager ConnectionManager;
        GameState GameState;


        public Game(ConnectionManager connectionManager)
        {
            this.ConnectionManager = connectionManager;
        }

        public void Start()
        {
            GameState = GenerateNewGameState();
            ConnectionManager.PlayerMessageHandler = ProcessClientMessage;
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
        long a = 0, b = 0;
        private void ServerLoop()
        {
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
                            ConnectionManager.SendToAllClients(GameState);
                        }
                    }
                    //Console.WriteLine(1000 * (b - a) / Stopwatch.Frequency);
                    a = Stopwatch.GetTimestamp();
                }
            }

        }
        private void ProcessClientMessage(string playerName, string msg)
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
                    ConnectionManager.SendToAllClients(GameState);
            }
            
        }
    }
}
