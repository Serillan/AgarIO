using DarkAgar.Actions;
using DarkAgar.Commands;
using DarkAgar.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DarkAgar
{
    /// <summary>
    /// The main game class.
    /// </summary>
    class Game
    {
        /// <summary>
        /// Gets or sets the server connection.
        /// </summary>
        /// <value>The server connection.</value>
        public ServerConnection ServerConnection { get; set; }

        /// <summary>
        /// The current game Time.
        /// Used for movement synchronization.
        /// </summary>
        public long Time;

        /// <summary>
        /// Gets or sets the state of the game.
        /// </summary>
        /// <value>The state of the game.</value>
        public GameState GameState { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether prediction is valid.
        /// </summary>
        /// <value><c>true</c> if prediction is valid ; otherwise, <c>false</c>.</value>
        public bool IsPredictionValid { get; set; }

        /// <summary>
        /// Gets or sets the name of the current player.
        /// </summary>
        /// <value>The name of the player.</value>
        public string PlayerName { get; set; }

        /// <summary>
        /// Gets or sets the login manager.
        /// </summary>
        /// <value>The login manager.</value>
        LoginManager LoginManager { get; set; }

        /// <summary>
        /// Gets or sets the graphics engine.
        /// </summary>
        /// <value>The graphics engine.</value>
        GraphicsEngine GraphicsEngine { get; set; }

        /// <summary>
        /// The input manager
        /// </summary>
        InputManager InputManager;

        /// <summary>
        /// The maximum x location.
        /// </summary>
        public const int MaxLocationX = 2400;

        /// <summary>
        /// The maximum y location.
        /// </summary>
        public const int MaxLocationY = 2400;

        /// <summary>
        /// The game loop interval in miliseconds.
        /// </summary>
        public const int GameLoopInterval = 16;

        /// <summary>
        /// Gets a value indicating whether this instance is running.
        /// </summary>
        /// <value><c>true</c> if this instance is running; otherwise, <c>false</c>.</value>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Initializes the current game.
        /// </summary>
        /// <param name="loginManager">The login manager.</param>
        /// <param name="graphicsEngine">The graphics engine.</param>
        /// <param name="inputManager">The input manager.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="playerName">Name of the player.</param>
        public void Initialize(LoginManager loginManager, GraphicsEngine graphicsEngine,
            InputManager inputManager, ServerConnection connection, string playerName)
        {
            this.LoginManager = loginManager;
            this.GraphicsEngine = graphicsEngine;
            this.InputManager = inputManager;
            ServerConnection = connection;
            this.PlayerName = playerName;
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            GraphicsEngine.StartGraphics();
            ServerConnection.StartReceiving(OnReceivedCommand);
            IsRunning = true;
            Time = -1;
            StartLoop();
        }

        /// <summary>
        /// Starts the loop.
        /// </summary>
        /// <returns>Task.</returns>
        private async Task StartLoop()
        {
            await Task.Factory.StartNew((System.Action)Loop, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// The main game loop.
        /// </summary>
        private void Loop()
        {
            long delta = 0;
            long a = 0, b = 0;

            while (true)
            {
                if (!IsRunning)
                    break;
                b = Stopwatch.GetTimestamp();
                delta = 1000 * (b - a) / Stopwatch.Frequency;
                if (delta >= GameLoopInterval)
                {
                    if (GameState?.CurrentPlayer != null)
                    {
                        lock (this)
                        {
                            Interlocked.Add(ref Time, 1);

                            new MovementAction(InputManager.MousePosition).Process(this);

                            if (InputManager.DivisionRequested)
                            {
                                new DivideAction(InputManager.MousePosition).Process(this);
                                InputManager.DivisionRequested = false;
                            }

                            if (InputManager.EjectionRequested)
                            {
                                new EjectAction(InputManager.MousePosition).Process(this);
                                InputManager.EjectionRequested = false;
                            }

                            // deep clone of prediction
                            GameState gameStateForRendering = GameState.DeepClonePrediction();

                            GraphicsEngine.Render(gameStateForRendering);
                        }
                    }
                    a = Stopwatch.GetTimestamp();
                }
            }
        }

        /// <summary>
        /// Called when the <paramref name="command"/> is received.
        /// </summary>
        /// <param name="command">The received command.</param>
        private void OnReceivedCommand(Command command)
        {
            lock (this)
            {
                command.Process(this);
            }
        }

        /// <summary>
        /// Closes the game.
        /// </summary>
        /// <param name="closingMessage">The closing message.</param>
        /// TODO Edit XML Comment Template for Close
        public void Close(string closingMessage)
        {
            IsRunning = false;
            ServerConnection.SendAsync(new Stop(closingMessage)).ContinueWith(new Action<Task>(t =>
            {
                ServerConnection.Dispose();
                Debug.WriteLine("stopped");
            }));

            GraphicsEngine.StopGraphics();
            LoginManager.ShowMessage(closingMessage);
        }
    }
}
