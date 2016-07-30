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
using DarkAgar;

namespace DarkAgar.Forms
{
    /// <summary>
    /// Login Form used for displaying the login screen.
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form" />
    partial class LoginForm : Form
    {
        /// <summary>
        /// Gets or sets the login manager.
        /// </summary>
        /// <value>The login manager.</value>
        LoginManager LoginManager { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginForm"/> class.
        /// </summary>
        public LoginForm()
        {
            InitializeComponent();
            LoginManager = new LoginManager(this);
            ServerListBox.SelectedIndex = 0;
        }

        /// <summary>
        /// Handles the Click event of the LoginButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void LoginButton_Click(object sender, EventArgs e)
        {
            InfoLabel.Text = "Logging in ...";
            LoginManager.StartGameAsync();
        }

        /// <summary>
        /// Handles the MouseClick event of the IPAddressTextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void IPAddressTextBox_MouseClick(object sender, MouseEventArgs e)
        {
            ServerListBox.ClearSelected();
            ServerListBox.SetSelected(2, true);
        }

    }
}
