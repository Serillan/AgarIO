using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarIO
{
    public class InputManager
    {
        GameForm GameForm;
        Game Game;

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
            Game.Close();
        }
    }
}
