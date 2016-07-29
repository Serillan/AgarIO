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
        LoginForm LoginForm { get; set; }
        IPAddress ServerAdress;

        public LoginManager(LoginForm loginForm)
        {
            this.LoginForm = loginForm;
        }

        /// <summary>
        /// Method that will try to start the game.
        /// Should be called from the UI thread.
        /// </summary>
        /// <returns></returns>
        public async Task StartGameAsync()
        {
            ServerConnection connection;
            string playerName = LoginForm.LoginTextBox.Text;
            switch (LoginForm.ServerListBox.SelectedIndex)
            {
                case 0:
                    ServerAdress = IPAddress.Loopback;
                    break;
                case 1:
                    ServerAdress = Dns.GetHostAddresses("gameserver.northeurope.cloudapp.azure.com")[0];
                    break;
                case 2:
                    if (!IPAddress.TryParse(LoginForm.IPAdressTextBox.Text, out ServerAdress))
                    {
                        LoginForm.InfoLabel.Text = "Invalid IP Adress";
                        return;
                    }
                    break;
            }

            try
            {
                connection = await ServerConnection.ConnectAsync(ServerAdress, playerName);
                //connection = await ServerConnection.ConnectAsync(IPAddress.Loopback, playerName);
                //connection = await ServerConnection.ConnectAsync(Dns.GetHostAddresses("gameserver.northeurope.cloudapp.azure.com")[0], playerName);
                //connection = await ServerConnection.ConnectAsync(IPAddress.Parse("178.40.89.228"), playerName);
            } catch (Exception ex)
            {
                Debug.WriteLine(ex.InnerException != null);
                Debug.WriteLine(ex.Message);

                if (ex.InnerException != null && ex.InnerException.Message ==
                    "An existing connection was forcibly closed by the remote host")
                    LoginForm.InfoLabel.Text = "Cannot connect to the server!";
                else
                {
                    LoginForm.InfoLabel.Text = ex.Message;
                }
                return;
            }

            Game game = new Game();
            GameForm gameForm = new GameForm();
            GraphicsEngine graphicsEngine = new GraphicsEngine(gameForm);
            InputManager inputManager = new InputManager(gameForm, game);

            game.Init(this, graphicsEngine, inputManager, connection, playerName);
            game.Start();
            LoginForm.Visible = false;
        }

        public void Show(string closingMsg)
        {
            LoginForm.BeginInvoke(new Action(() =>
            {
                LoginForm.InfoLabel.Text = closingMsg;
                LoginForm.Visible = true;
            }));
        }
    }
}
