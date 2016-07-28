using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using AgarIO.Entities;
using AgarIO.Actions;
using AgarIO.Commands;
using System.Timers;


namespace AgarIO
{
    class Game
    {
        public ServerConnection ServerConnection { get; set; }
        public int Time { get; set; }
        public GameState GameState { get; set; }
        public bool IsPredictionValid { get; set; }
        public string PlayerName { get; set; }

        LoginManager LoginManager;
        GraphicsEngine GraphicsEngine;
        InputManager InputManager;

        public const int MaxLocationX = 9000;
        public const int MaxLocationY = 9000;
        public const int GameLoopInterval = 16;

        /// <summary>
        /// Used for avoiding multiple game closes.
        /// </summary>
        public bool IsRunning { get; private set; }

        public void Init(LoginManager loginManager, GraphicsEngine graphicsEngine, 
            InputManager inputManager, ServerConnection connection, string playerName)
        {
            this.LoginManager = loginManager;
            this.GraphicsEngine = graphicsEngine;
            this.InputManager = inputManager;
            ServerConnection = connection;
            this.PlayerName = playerName;
        }

        public void Start()
        {
            GraphicsEngine.StartGraphics();
            ServerConnection.StartReceiving(OnReceiveCommand);
            IsRunning = true;
            Time = -1;
            StartLoop();
        }

        private async Task StartLoop()
        {
            await Task.Factory.StartNew((System.Action)Loop, TaskCreationOptions.LongRunning);
        }

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
                    if (GameState != null)
                    {
                        lock (this)
                        {
                            Time++;
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

                            GraphicsEngine.Render(GameState);
                        }
                    }
                    a = Stopwatch.GetTimestamp();
                }
            }
        }

        private void OnReceiveCommand(Command command)
        {
            lock (this)
            {
                command.Process(this);
            }
        }

        public void Close(string msg)
        {
            IsRunning = false;
            //GameTimer.Stop();
            ServerConnection.SendAsync(new Stop(msg)).ContinueWith(new Action<Task>(t => {
                ServerConnection.Dispose();
                Debug.WriteLine("stopped");
            }));

            GraphicsEngine.StopGraphics();
            LoginManager.Show(msg); 
        }
    }
}
