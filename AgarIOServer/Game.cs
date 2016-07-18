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
        const int ServerLoopInterval = 5000; // ms

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
            state.Food.Add(new Food() { Mass = 2, Radius = 1 });
            return state;
        }

        private async Task ServerLoop()
        {
            while (true)
            {
                await Task.Delay(ServerLoopInterval);
                ConnectionManager.SendToAllClients(GameState);
            }
        }

        //
        // PROCESSING ACTIONS
        //

        private void ProcessClientMessage(string playerName, string msg)
        {
            string[] tokens = msg.Split();
            int x, y;

            switch (tokens[0])
            {
                case "STOP":
                    ConnectionManager.EndClientConnection(playerName);
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
