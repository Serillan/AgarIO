using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DarkAgar.Forms;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace DarkAgar
{
    /// <summary>
    /// The Input Manager class is used for controlling the game input.
    /// </summary>
    class InputManager
    {
        /// <summary>
        /// Gets or sets the game form.
        /// </summary>
        /// <value>The game form.</value>
        private GameForm GameForm { get; set; }

        /// <summary>
        /// Gets or sets the game.
        /// </summary>
        /// <value>The game.</value>
        private Game Game { get; set; }

        /// <summary>
        /// Gets the mouse position in game coordinates.
        /// </summary>
        /// <value>The mouse position.</value>
        public Point MousePosition { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether division is requested.
        /// </summary>
        /// <value><c>true</c> if division is requested ; otherwise, <c>false</c>.</value>
        public bool DivisionRequested { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether ejection is requested.
        /// </summary>
        /// <value><c>true</c> if ejection is requested ; otherwise, <c>false</c>.</value>
        public bool EjectionRequested { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InputManager"/> class.
        /// </summary>
        /// <param name="gameForm">The game form.</param>
        /// <param name="game">The game.</param>
        public InputManager(GameForm gameForm, Game game)
        {
            this.GameForm = gameForm;
            this.Game = game;

            Initialize();
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Initialize()
        {
            GameForm.FormClosed += GameForm_FormClosed;
            GameForm.KeyDown += GameForm_KeyDown;
            GameForm.GamePanel.Paint += GamePanel_Paint;
        }

        /// <summary>
        /// Used for updating the mouse position in game coordinates after every paint.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
        private void GamePanel_Paint(object sender, PaintEventArgs e)
        {
            if (GameForm.GamePanel.Focused || GameForm.Focused) // TODO - remove condition - just for testing
                UpdateMousePosition();
        }

        /// <summary>
        /// Updates the mouse position.
        /// </summary>
        private void UpdateMousePosition()
        {
            float posX = 0, posY = 0;

            posX = GameForm.GamePanel.PointToClient(Cursor.Position).X;
            posY = GameForm.GamePanel.PointToClient(Cursor.Position).Y;

            if (Game.GameState == null)
                return;

            // view to game coefficient
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

            MousePosition = new Point(posX, posY);
        }

        /// <summary>
        /// Handles the KeyDown event of the GameForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void GameForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
                DivisionRequested = true;

            if (e.KeyCode == Keys.W)
                EjectionRequested = true;
        }

        /// <summary>
        /// Handles the MouseMove event of the GamePanel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
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

        }

        /// <summary>
        /// Handles the FormClosed event of the GameForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.FormClosedEventArgs"/> instance containing the event data.</param>
        private void GameForm_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
        {
            if (Game.IsRunning)
                Game.Close("");
            // otherwise Game is being closed by application
        }
    }

    /// <summary>
    /// Struct Point used for storing mouse game coordinates.
    /// </summary>
    struct Point
    {
        /// <summary>
        /// The x.
        /// </summary>
        public float X;
        
        /// <summary>
        /// The y.
        /// </summary>
        public float Y;

        /// <summary>
        /// Initializes a new instance of the <see cref="Point"/> struct.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        public Point(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }
    }
}
