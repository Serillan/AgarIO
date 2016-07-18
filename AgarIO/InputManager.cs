using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgarIO.Forms;
using System.Windows.Forms;

namespace AgarIO
{
    public class InputManager
    {
        GameForm GameForm;
        Game Game;

        public System.Drawing.Point MousePosition
        {
            get {
                return Cursor.Position;
            }
        }

        public InputManager(GameForm gameForm, Game game)
        {
            this.GameForm = gameForm;
            this.Game = game;

            Init();
        }

        private void Init()
        {
            GameForm.FormClosed += GameForm_FormClosed;
        }

        private void GameForm_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
        {
            if (Game.IsRunning)
                Game.Close("");
            // otherwise Game is being closed by application
        }
    }
}
