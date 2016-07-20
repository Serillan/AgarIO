using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgarIO.Forms;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

namespace AgarIO
{
    class InputManager
    {
        GameForm GameForm;
        Game Game;

        public Point MousePosition { get; private set; }

        public InputManager(GameForm gameForm, Game game)
        {
            this.GameForm = gameForm;
            this.Game = game;

            Init();
        }

        private void Init()
        {
            GameForm.FormClosed += GameForm_FormClosed;
            GameForm.GamePanel.MouseMove += GamePanel_MouseMove;
        }

        private void GamePanel_MouseMove(object sender, MouseEventArgs e)
        {
            Debug.WriteLine("change");
            MousePosition = GameForm.GamePanel.PointToClient(Cursor.Position);
        }

        private void GameForm_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
        {
            if (Game.IsRunning)
                Game.Close("");
            // otherwise Game is being closed by application
        }
    }
}
