using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using AgarIOServer.Entities;
using AgarIOServer.Actions;

namespace AgarIOServer
{
    class Game
    {
        const int ServerLoopInterval = 50; // ms
        public const int MaxLocationX = 2000;
        public const int MaxLocationY = 2000;

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
            ServerLoop();
        }

        public GameState GenerateNewGameState()
        {
            GameState state = new GameState();
            return state;
        }

        private async Task ServerLoop()
        {
            while (true)
            {
                await Task.Delay(ServerLoopInterval);
                lock(GameState)
                    ConnectionManager.SendToAllClients(GameState);
            }
        }

  
        private void ProcessClientMessage(string playerName, string msg)
        {
            string[] tokens = msg.Split();
            int x, y;

            switch (tokens[0])
            {
                case "STOP":
                    ConnectionManager.EndClientConnection(playerName);
                    lock(GameState)
                        GameState.Players.RemoveAll(p => p.Name == playerName);
                    break;
                case "CONNECTED":
                    Player newPlayer = new Player(playerName);
                    lock(GameState)
                        GameState.Players.Add(newPlayer);
                    break;
            }

            if (tokens.Length == 3 && int.TryParse(tokens[1], out x) && int.TryParse(tokens[2], out y))
                switch (tokens[0])
                {
                    case "MOVE":
                        new MovementAction(x, y, playerName).Process(GameState);
                        break;
                    case "DIVIDE":
                        new DivideAction(x, y, playerName).Process(GameState);
                        break;
                    case "GIVEFOOD":
                        new FoodAction(x, y, playerName).Process(GameState);
                        break;
                }
        }
    }
}
