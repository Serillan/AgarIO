using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AgarIO
{
    public class Game
    {
        ServerConnection ServerConnection;
        LoginManager LoginManager;
        GraphicsEngine GraphicsEngine;
        InputManager InputManager;

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

        private void OnReceiveMessage(string msg)
        {
            var tokens = msg.Split();
            switch (tokens[0])
            {
                case "STOP":
                    Close(msg.Substring(5));
                    break;
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
