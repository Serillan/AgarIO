using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using AgarIO.Entities;
using AgarIO.Actions;

namespace AgarIO
{
    public class Game
    {
        ServerConnection ServerConnection;
        LoginManager LoginManager;
        GraphicsEngine GraphicsEngine;
        InputManager InputManager;
        GameState GameState;
        string PlayerName;
        

        /// <summary>
        /// Used for avoiding multiple game closes.
        /// </summary>
        public bool IsRunning { get; private set; }

        public void Init(LoginManager loginManager, GraphicsEngine graphicsEngine, 
            InputManager inputManager, ServerConnection connection)
        {
            this.LoginManager = loginManager;
            this.GraphicsEngine = graphicsEngine;
            this.InputManager = inputManager;
            this.ServerConnection = connection;
        }

        public void Start()
        {
            GraphicsEngine.StartGraphics();
            ServerConnection.StartReceiving(OnReceiveMessage);
            IsRunning = true;
        }

        private async Task GameLoop()
        {
            while (true)
            {
                new MovementAction(InputManager.MousePosition).Process(GameState);
                GraphicsEngine.RenderAsync();
                await Task.Delay(30);
            }
        }

        private void OnReceiveMessage(string msg)
        {
            var tokens = msg.Split();
            Debug.WriteLine($"MSG: {msg}");
            switch (tokens[0])
            {
                case "STOP":
                    Close(msg.Substring(5));
                    break;
                default:       // it might be serialized game state
                    TryLoadState(msg);
                    break;
            }
        }

        public void TryLoadState(string msg)
        {
            byte[] data = Encoding.Default.GetBytes(msg);
            BinaryFormatter formatter = new BinaryFormatter ();
            MemoryStream stream = new MemoryStream(data);
            try
            {
                var State = (GameState)Serializer.Deserialize(typeof(GameState), stream);
                this.GameState = State;
                // TODO - server has to add player to the state!
                //this.GameState.CurrentPlayer = State.Players.Find(p => p.Name == PlayerName);
                Debug.WriteLine("Received new state!");
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

            ServerConnection.SendAsync("STOP").ContinueWith(new Action<Task>(t => {
                ServerConnection.Dispose();
            }));

            GraphicsEngine.StopGraphics();
            LoginManager.Show(msg); 
        }
    }
}
