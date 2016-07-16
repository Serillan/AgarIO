using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AgarIO
{
    public partial class LoginForm : Form
    {
        LoginManager manager;

        public LoginForm()
        {
            InitializeComponent();
            manager = new LoginManager(this);
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            manager.StartGameAsync();
        }
    }
}
