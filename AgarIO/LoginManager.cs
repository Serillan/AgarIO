using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AgarIO.Forms;
using System.Diagnostics;

namespace AgarIO
{
    class LoginManager
    {
        LoginForm loginForm;

        public LoginManager(LoginForm loginForm)
        {
            this.loginForm = loginForm;
        }

        /// <summary>
        /// Method that will try to start the game.
        /// Should be called from the UI thread.
        /// </summary>
        /// <returns></returns>
        public async Task StartGameAsync()
        {
            ServerConnection connection;
            string playerName = loginForm.LoginTextBox.Text;
            try
            {
                //connection = await ServerConnection.ConnectAsync(IPAddress.Loopback, playerName);
                connection = await ServerConnection.ConnectAsync(Dns.GetHostAddresses("gameserver.northeurope.cloudapp.azure.com")[0], playerName);
                //connection = await ServerConnection.ConnectAsync(IPAddress.Parse("178.40.89.228"), playerName);
            } catch (Exception ex)
            {
                Debug.WriteLine(ex.InnerException != null);
                Debug.WriteLine(ex.Message);

                if (ex.InnerException != null && ex.InnerException.Message ==
                    "An existing connection was forcibly closed by the remote host")
                    loginForm.InfoLabel.Text = "Cannot connect to the server!";
                else
                {
                    loginForm.InfoLabel.Text = ex.Message;
                }
                return;
            }

            Game game = new Game();
            GameForm gameForm = new GameForm();
            GraphicsEngine graphicsEngine = new GraphicsEngine(gameForm);
            InputManager inputManager = new InputManager(gameForm, game);

            game.Init(this, graphicsEngine, inputManager, connection, playerName);
            game.Start();
            loginForm.Visible = false;
        }

        public void Show(string closingMsg)
        {
            loginForm.BeginInvoke(new Action(() =>
            {
                loginForm.InfoLabel.Text = closingMsg;
                loginForm.Visible = true;
            }));
        }
    }
}
