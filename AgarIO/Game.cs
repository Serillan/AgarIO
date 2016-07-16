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
        ServerConnection Connection;
        GameForm GameForm;
        LoginForm LoginForm;

        public Game(LoginForm loginForm)
        {
            this.LoginForm = loginForm;
        }

        public async void StartAsync()
        {
            ServerConnection conn = await ServerConnection.ConnectAsync(IPAddress.Loopback, LoginForm.Text);
            this.GameForm = new GameForm(this);
            this.Connection = conn;
            LoginForm.Visible = false;

            GameForm.Show();
        }

        public void Close()
        {
            GameForm.BeginInvoke(new Action(() => GameForm.Close()));
            LoginForm.BeginInvoke(new Action(() => LoginForm.Visible = true));
        }
    }
}
