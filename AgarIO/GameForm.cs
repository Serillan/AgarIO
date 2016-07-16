using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AgarIO
{
    public partial class GameForm : Form
    {
        Game CurrentGame;

        public GameForm(Game game)
        {
            this.CurrentGame = game;
            InitializeComponent();
        }

        private void GameForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            CurrentGame.Close();
        }
    }
}