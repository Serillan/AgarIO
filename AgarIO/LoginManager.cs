using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DarkAgar.Forms;
using System.Diagnostics;

namespace DarkAgar
{
    /// <summary>
    /// The Login Manager class responsible for controlling the login to the game.
    /// </summary>
    class LoginManager
    {
        /// <summary>
        /// Gets or sets the login form.
        /// </summary>
        /// <value>The login form.</value>
        LoginForm LoginForm { get; set; }

        /// <summary>
        /// The server address
        /// </summary>
        IPAddress ServerAddress;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginManager"/> class.
        /// </summary>
        /// <param name="loginForm">The login form.</param>
        public LoginManager(LoginForm loginForm)
        {
            this.LoginForm = loginForm;
        }

        /// <summary>
        /// Method that will try to start the game.
        /// Should be called from the UI thread.
        /// </summary>
        /// <returns>The task.</returns>
        public async Task StartGameAsync()
        {
            ServerConnection connection;
            string playerName = LoginForm.LoginTextBox.Text;
            switch (LoginForm.ServerListBox.SelectedIndex)
            {
                case 0:
                    ServerAddress = IPAddress.Loopback;
                    break;
                case 1:
                    ServerAddress = Dns.GetHostAddresses("gameserver.northeurope.cloudapp.azure.com")[0];
                    break;
                case 2:
                    if (!IPAddress.TryParse(LoginForm.IPAddressTextBox.Text, out ServerAddress))
                    {
                        LoginForm.InfoLabel.Text = "Invalid IP Adress";
                        return;
                    }
                    break;
            }

            try
            {
                connection = await ServerConnection.ConnectAsync(ServerAddress, playerName);
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

            game.Initialize(this, graphicsEngine, inputManager, connection, playerName);
            game.Start();
            LoginForm.Visible = false;
        }

        /// <summary>
        /// Shows the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void ShowMessage(string message)
        {
            LoginForm.BeginInvoke(new Action(() =>
            {
                LoginForm.InfoLabel.Text = message;
                LoginForm.Visible = true;
            }));
        }
    }
}
