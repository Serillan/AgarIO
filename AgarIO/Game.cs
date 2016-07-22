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
using System.Timers;

namespace AgarIO
{
    class Game
    {
        public static ServerConnection ServerConnection;
        LoginManager LoginManager;
        GraphicsEngine GraphicsEngine;
        InputManager InputManager;
        public GameState GameState;
        string PlayerName;
        Timer GameTimer;

        public const int MaxLocationX = 2400;
        public const int MaxLocationY = 2400;
        const int GameLoopInterval = 16;

        public static int Time;


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
            ServerConnection.StartReceiving(OnReceiveMessage);
            IsRunning = true;
            Time = 0;
            StartLoop();
        }

        private async Task StartLoop()
        {
            Task.Factory.StartNew((System.Action)Loop2, TaskCreationOptions.LongRunning);
            /*
            GameTimer = new Timer();
            GameTimer.Interval = GameLoopInterval;
            GameTimer.Elapsed += Loop;
            GameTimer.Start();
            */
        }

        private void Loop(object sender, ElapsedEventArgs e)
        {
            if (GameState != null)
            {
                Time++;
                new MovementAction(InputManager.MousePosition).Process(GameState);
                GraphicsEngine.Render(GameState);
            }
        }

        long a = 0, b = 0;

        private void Loop2()
        {
            long delta = 0;
            while (true)
            {
                b = Stopwatch.GetTimestamp();
                delta = 1000 * (b - a) / Stopwatch.Frequency;
                if (delta >= GameLoopInterval)
                {
                    if (GameState != null)
                    {
                        Time++;
                        new MovementAction(InputManager.MousePosition).Process(GameState);
                        GraphicsEngine.Render(GameState);
                    }
                    a = Stopwatch.GetTimestamp();
                }
            }
        }

        private void OnReceiveMessage(string msg)
        {
            var tokens = msg.Split();
            //Debug.WriteLine($"MSG: {msg}");
            switch (tokens[0])
            {
                case "STOP":
                    Close(msg.Substring(5));
                    break;
              //  case "SET_POSITION":
              //      GameState.CurrentPlayer.p
              //      break;

                default:       // it might be serialized game state
                    TryLoadState(msg);
                    break;
            }
        }

        public void TryLoadState(string msg)
        {
            byte[] data = Encoding.Default.GetBytes(msg);
            MemoryStream stream = new MemoryStream(data);
            try
            {
                Player oldCurrentPlayer = null;
                if (GameState?.CurrentPlayer != null)
                    oldCurrentPlayer = GameState.CurrentPlayer;

                var state = Serializer.Deserialize<GameState>(stream);
                if (GameState != null && state.Version < GameState.Version)
                    return;
                this.GameState = state;
                
                
                var current = state.Players.Find(p => p.Name == PlayerName);
                if (oldCurrentPlayer != null)
                    current.Parts = oldCurrentPlayer.Parts; // prediction
                state.CurrentPlayer = current;
                

                // TODO - server has to add player to the state!
                //this.GameState.CurrentPlayer = State.Players.Find(p => p.Name == PlayerName);
                //Debug.WriteLine("Received new state!");
            } catch (SerializationException ex)
            {
                Debug.WriteLine("Deserializing error.");
            } catch (ArgumentNullException ex)
            {
                Debug.WriteLine("Couldn't find the current player in the current game state");
                Close("Error");
            }
            catch (NullReferenceException ex)
            {
                Debug.WriteLine("Game State is null after deserialization");
                Close("Error");
            }
        }

        public void Close(string msg)
        {
            IsRunning = false;
            //GameTimer.Stop();
            ServerConnection.SendAsync("STOP").ContinueWith(new Action<Task>(t => {
                ServerConnection.Dispose();
            }));

            GraphicsEngine.StopGraphics();
            LoginManager.Show(msg); 
        }
    }
}
