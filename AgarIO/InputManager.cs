using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgarIO.Forms;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace AgarIO
{
    class InputManager
    {
        GameForm GameForm;
        Game Game;

        /// <summary>
        /// Mouse position in game coordinates.
        /// </summary>
        public Point MousePosition { get; private set; }

        public bool DivideRequested { get; set; }

        public InputManager(GameForm gameForm, Game game)
        {
            this.GameForm = gameForm;
            this.Game = game;

            Init();
        }

        private void Init()
        {
            GameForm.FormClosed += GameForm_FormClosed;
            //GameForm.GamePanel.MouseMove += GamePanel_MouseMove;
            GameForm.KeyDown += GameForm_KeyDown;
            GameForm.GamePanel.Paint += GamePanel_Paint;
        }

        private void GamePanel_Paint(object sender, PaintEventArgs e)
        {
            if (GameForm.GamePanel.Focused || GameForm.Focused) // TODO - remove condition - just for testing
                UpdateMousePosition();
        }

        private void UpdateMousePosition()
        {
            float posX = 0, posY = 0;

            posX = GameForm.GamePanel.PointToClient(Cursor.Position).X;
            posY = GameForm.GamePanel.PointToClient(Cursor.Position).Y;

            if (Game.GameState == null)
                return;

            // view to game
            float q = (float)((0.1 * GameForm.GamePanel.Width) / Game.GameState.CurrentPlayer.Radius);
            // opposite order!
            posX -= GameForm.GamePanel.Width / 2.0f;
            posY -= GameForm.GamePanel.Height / 2.0f;
            if (q < 1)
            {
                posX *= 1 / q;
                posY *= 1 / q;
            }
            posX += Game.GameState.CurrentPlayer.X;
            posY += Game.GameState.CurrentPlayer.Y;

            /*
            TranslateTransform(GameForm.GamePanel.Width / 2.0f, GameForm.GamePanel.Height / 2.0f);
            if (q < 1) // don't scale until it reaches max view radius (_k_ * GamePanel.Width)
                g.ScaleTransform(q, q);
            g.TranslateTransform(-Game.GameState.CurrentPlayer.X, -Game.GameState.CurrentPlayer.Y);
            */
            MousePosition = new Point(posX, posY);
        }

        private void GameForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.M)
                Game.GameState.CurrentPlayer.Parts[0].Mass += 10;

            if (e.KeyCode == Keys.N && Game.GameState.CurrentPlayer.Parts[0].Mass > 10)
                Game.GameState.CurrentPlayer.Parts[0].Mass -= 10;

            if (e.KeyCode == Keys.Space)
                DivideRequested = true;
        }

        private void GamePanel_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = GameForm.GamePanel.PointToClient(Cursor.Position);
            float posX = pos.X;
            float posY = pos.Y;

            // view to game
            float q = (float)((0.1 * GameForm.GamePanel.Width) / Game.GameState.CurrentPlayer.Radius);
            // opposite order!
            posX -= GameForm.GamePanel.Width / 2.0f;
            posY -= GameForm.GamePanel.Height / 2.0f;
            if (q < 1)
            {
                posX *= 1 / q;
                posY *= 1 / q;
            }
            posX += Game.GameState.CurrentPlayer.X;
            posY += Game.GameState.CurrentPlayer.Y;

            /*
            TranslateTransform(GameForm.GamePanel.Width / 2.0f, GameForm.GamePanel.Height / 2.0f);
            if (q < 1) // don't scale until it reaches max view radius (_k_ * GamePanel.Width)
                g.ScaleTransform(q, q);
            g.TranslateTransform(-Game.GameState.CurrentPlayer.X, -Game.GameState.CurrentPlayer.Y);
            */
            //MousePosition = new Point(posX, posY);
        }

        private void GameForm_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
        {
            if (Game.IsRunning)
                Game.Close("");
            // otherwise Game is being closed by application
        }
    }

    struct Point
    {
        public float X;
        public float Y;

        public Point(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }
    }
}
