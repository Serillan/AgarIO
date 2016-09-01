//#define ServerLoop // for higher server load it might be better

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DarkAgarServer.Entities;
using DarkAgarServer.Commands;
using System.Timers;
using System.Diagnostics;
using System.Threading;


namespace DarkAgarServer
{
    /// <summary>
    /// The main game server class.
    /// </summary>
    class GameServer
    {
        #region Game Constants
        public const int GameLoopInterval = 16;
        public const int ServerLoopInterval = 16; // not used right now 
        public const int MaxLocationX = 2400; // 9000
        public const int MaxLocationY = 2400;
        public const int PlayerStartSize = 100; // 400
        public const int PlayerMaximumNumberOfPartsForDivision = 16; // player can have more parts (max 2 * this - 1)
        public const int MinimumDivisionSize = 200;
        public const int DefaultVirusSize = 600;
        public const int MaxVirusSize = 1000;
        public const int BlobSize = 20;
        public const int MinSizeOfFood = 5;
        public const int MaxSizeOfFood = 15;
        public const int MaxNumberOfFood = 40; // 300
        public const int MaxNumberOfViruses = 10; // 100
        #endregion

        /// <summary>
        /// The random generator.
        /// </summary>
        public static Random RandomGenerator = new Random();

        /// <summary>
        /// Gets or sets the connection manager.
        /// </summary>
        /// <value>The connection manager.</value>
        public ConnectionManager ConnectionManager { get; set; }

        /// <summary>
        /// Gets or sets the state of the game.
        /// </summary>
        /// <value>The state of the game.</value>
        public GameState GameState { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameServer"/> class.
        /// </summary>
        /// <param name="connectionManager">The connection manager.</param>
        public GameServer(ConnectionManager connectionManager)
        {
            this.ConnectionManager = connectionManager;
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            GameState = GenerateNewGameState();
            ConnectionManager.PlayerCommandHandler = ProcessClientCommand;
            ConnectionManager.NewPlayerHandler = AddNewPlayer;

#if ServerLoop
            Task.Factory.StartNew((System.Action) ServerLoop, TaskCreationOptions.LongRunning);
#endif
        }

        /// <summary>
        /// Generates the new state of the game.
        /// </summary>
        /// <returns>GameState.</returns>
        public GameState GenerateNewGameState()
        {
            var state = new GameState();

            // generate food
            lock (state.Food)
            {
                for (int i = 0; i < MaxNumberOfFood; i++)
                {
                    state.Food.Add(new Food(RandomGenerator.Next(MaxLocationX), RandomGenerator.Next(MaxLocationY), RandomGenerator.Next(GameServer.MinSizeOfFood, GameServer.MaxSizeOfFood)));
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

#if ServerLoop
        /// <summary>
        /// The main server loop used for sending <see cref="UpdateState"/> command to all
        /// connected players.
        /// </summary>
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
                        GameState.StateLock.EnterWriteLock(); // while state is being serialized, nothing should be done with it (global lock on state)
                        ConnectionManager.SendToAllClients(new UpdateState(GameState));
                        GameState.StateLock.ExitWriteLock();
                    a = Stopwatch.GetTimestamp();
                }
            }
        }
#endif

        /// <summary>
        /// Processes the client command.
        /// </summary>
        /// <param name="playerName">Name of the player.</param>
        /// <param name="command">The command.</param>
        private void ProcessClientCommand(string playerName, Command command)
        {
            Interlocked.Add(ref GameState.Version, 1);

            GameState.GameStateLock.EnterReadLock(); // parallel processing is allowed
            Console.WriteLine("entering read");
            command.Process(this, playerName);
            Console.WriteLine("exiting read");
            GameState.GameStateLock.ExitReadLock();

#if !ServerLoop
            GameState.GameStateLock.EnterWriteLock(); // while state is being serialized, nothing should be done with it (global lock on state)
            Console.WriteLine("entering write");
            ConnectionManager.SendToAllClients(new UpdateState(GameState));
            Console.WriteLine("exiting write");
            GameState.GameStateLock.ExitWriteLock();
#endif
        }

        /// <summary>
        /// Adds the new player.
        /// </summary>
        /// <param name="playerName">Name of the player.</param>
        private void AddNewPlayer(string playerName)
        {
            var newPlayer = new Player(playerName, GameState);

            Interlocked.Add(ref GameState.Version, 1);
#if !ServerLoop
            GameState.GameStateLock.EnterWriteLock(); // while state is being serialized, nothing should be done with it (global lock on state)
            Console.WriteLine("entering write2");
            GameState.Players.Add(newPlayer);
            ConnectionManager.SendToAllClients(new UpdateState(GameState));
            Console.WriteLine("exiting write2");
            GameState.GameStateLock.ExitWriteLock();
#endif
        }

        /// <summary>
        /// Removes the player.
        /// </summary>
        /// <param name="playerName">Name of the player.</param>
        /// <param name="stopMessage">The stop message.</param>
        public void RemovePlayer(string playerName, string stopMessage)
        {
            ConnectionManager.SendToClient(playerName, new Stop(stopMessage));

            lock (GameState.Players)
            {
                GameState.Players.RemoveAll(p => p.Name == playerName);
            }

            ConnectionManager.EndClientConnection(playerName);

        }

    }
}
