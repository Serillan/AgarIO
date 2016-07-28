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
        public const int GameLoopInterval = 16;
        public const int ServerLoopInterval = 30; // not used right now 
        public const int MaxLocationX = 9000;
        public const int MaxLocationY = 9000;
        public const int PlayerStartSize = 400;
        public const int MinimumDivisionSize = 200;
        public const int DefaultVirusSize = 100;
        public const int MaxVirusSize = 800;
        public const int MaxNumberOfFood = 300;
        public const int MaxNumberOfViruses = 100;
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

            // generate food
            lock (state.Food)
            {
                for (int i = 0; i < MaxNumberOfFood; i++)
                {
                    state.Food.Add(new Food(RandomG.Next(MaxLocationX), RandomG.Next(MaxLocationY), RandomG.Next(10, 70)));
                }
            }

            // generate viruses
            var playersParts = state.Players.SelectMany(p => p.Parts).ToList();
            lock (state.Viruses)
            {
                for (int i = 0; i < MaxNumberOfViruses; i++)
                    state.Viruses.Add(new Virus(playersParts));
            }

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
            lock (GameState) // TODO - with higher server load - it can be done in loop
            {
                ConnectionManager.SendToAllClients(new UpdateState(GameState));
            }
        }

        private void AddNewPlayer(string playerName)
        {
            Player newPlayer = new Player(playerName, GameState);
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

    }
}
