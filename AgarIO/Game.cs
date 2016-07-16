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
        }

        public void Close()
        {
            GraphicsEngine.StopGraphics();
            ServerConnection.Dispose();
            LoginManager.Show();
        }
    }
}
