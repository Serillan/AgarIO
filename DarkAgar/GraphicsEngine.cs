using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DarkAgar.Forms;
using System.Drawing;
using System.Windows.Forms;
using DarkAgar.Entities;
using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace DarkAgar
{
    /// <summary>
    /// Graphic Engine class used for rendering the game.
    /// </summary>
    class GraphicsEngine
    {
        /// <summary>
        /// Gets or sets the game form.
        /// </summary>
        GameForm GameForm { get; set; }

        /// <summary>
        /// Gets or sets the game panel on which the game is drawn.
        /// </summary>
        /// <value>The game panel.</value>
        static GamePanel GamePanel { get; set; }

        /// <summary>
        /// Gets or sets the matrix pen with which the matrix is drawn.
        /// </summary>
        /// <value>The matrix pen.</value>
        Pen MatrixPen { get; set; }

        /// <summary>
        /// Gets or sets the game state copy. This state is rendered.
        /// We have game state copy because it is thread safe.
        /// </summary>
        /// <value>The game state copy.</value>
        GameState GameStateCopy { get; set; }

        /// <summary>
        /// Gets the width of the game panel.
        /// </summary>
        /// <value>The width of the game panel.</value>
        static public int GamePanelWidth
        {
            get
            {
                return GamePanel.Width;
            }
        }

        /// <summary>
        /// Gets the height of the game panel.
        /// </summary>
        /// <value>The height of the game panel.</value>
        static public int GamePanelHeight
        {
            get
            {
                return GamePanel.Height;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsEngine" /> class.
        /// </summary>
        /// <param name="gameForm">The game form.</param>
        public GraphicsEngine(GameForm gameForm)
        {
            GameForm = gameForm;
            GamePanel = GameForm.GamePanel;

            MatrixPen = new Pen(Color.FromArgb(20, 20, 20), 10);
            GamePanel.Paint += GamePanel_Paint;
        }

        /// <summary>
        /// Handles the Paint event of the GamePanel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
        private void GamePanel_Paint(object sender, PaintEventArgs e)
        {
            if (GameStateCopy == null)
                return;
            DrawGame(e.Graphics);
        }

        /// <summary>
        /// Draws the game.
        /// </summary>
        /// <param name="g">The graphics object used for drawing.</param>
        private void DrawGame(Graphics g)
        {
            if (GameStateCopy == null || GameStateCopy.CurrentPlayer == null)
                return;
            g.Clear(Color.Black);
            TransformScene(g);
            DrawMatrix(g);
            DrawFood(g);
            DrawPlayers(g);
            DrawViruses(g);
            DrawScoreTable(g);
        }

        /// <summary>
        /// Transforms the scene.
        /// </summary>
        /// <param name="g">The graphics object used for drawing.</param>
        private void TransformScene(Graphics g)
        {
            float q = (float)((0.1 * GamePanel.Width) / GameStateCopy.CurrentPlayer.Radius);
            // opposite order!
            g.TranslateTransform(GamePanel.Width / 2.0f, GamePanel.Height / 2.0f);
            if (q < 1) // don't scale until it reaches max view radius (_k_ * GamePanel.Width)
                g.ScaleTransform(q, q);
            g.TranslateTransform(-GameStateCopy.CurrentPlayer.X, -GameStateCopy.CurrentPlayer.Y); // will be applied 1.
        }

        /// <summary>
        /// Draws the players.
        /// </summary>
        /// <param name="g">The graphics object used for drawing.</param>
        private void DrawPlayers(Graphics g)
        {
            var playerPartsWithColorAndName = from player in GameStateCopy.Players select new { player.Color, player.Parts, PlayerName = player.Name };

            var partsWithColorAndName = (
                        from partWithColorAndName in playerPartsWithColorAndName
                        from part in partWithColorAndName.Parts
                        select new { Part = part, partWithColorAndName.Color, partWithColorAndName.PlayerName }
                        ).OrderBy(p => p.Part.Radius).ToList();

            foreach (var partWithColorAndName in partsWithColorAndName)
            {
                var part = partWithColorAndName.Part;
                var radius = part.Radius;
                var color = partWithColorAndName.Color;
                var name = partWithColorAndName.PlayerName;

                var brush = new SolidBrush(Color.FromArgb(color[0], color[1], color[2]));

                g.FillEllipse(brush, part.X - radius,
                   part.Y - radius, 2 * radius, 2 * radius);

                if (!part.IsBeingEjected) // draw name of player in the centre of the part
                {
                    Font myFont = new Font("Arial", 14);

                    var sizeOfText = g.MeasureString(name, myFont);
                    g.DrawString(name, myFont, Brushes.Black, part.X - sizeOfText.Width / 2, part.Y - sizeOfText.Height / 2);
                }
                //if (state.CurrentPlayer.Parts.Contains(part))
                //    e.Graphics.DrawString($"{part.X} {part.Y}", myFont, Brushes.Black, 10, 10);
            }
        }

        /// <summary>
        /// Draws the matrix.
        /// </summary>
        /// <param name="g">The graphics object used for drawing.</param>
        private void DrawMatrix(Graphics g)
        {
            for (int x = 0; x <= Game.MaxLocationX; x += 120)
                g.DrawLine(MatrixPen, x, 0, x, Game.MaxLocationY);

            for (int y = 0; y <= Game.MaxLocationY; y += 120)
                g.DrawLine(MatrixPen, 0, y, Game.MaxLocationX, y);
        }

        /// <summary>
        /// Draws the food.
        /// </summary>
        /// <param name="g">The graphics object used for drawing.</param>
        private void DrawFood(Graphics g)
        {
            if (GameStateCopy.Food == null)
                return;

            foreach (var food in GameStateCopy.Food)
            {
                var brush = new SolidBrush(Color.FromArgb(food.Color[0], food.Color[1], food.Color[2]));
                g.FillEllipse(brush, food.X - food.Radius,
                   food.Y - food.Radius, 2 * food.Radius, 2 * food.Radius);
            }

        }

        /// <summary>
        /// Draws the viruses.
        /// </summary>
        /// <param name="g">The graphics object used for drawing.</param>
        private void DrawViruses(Graphics g)
        {
            if (GameStateCopy.Viruses == null)
                return;
            foreach (var virus in GameStateCopy.Viruses)
            {
                Brush brush = new HatchBrush(HatchStyle.ZigZag, Color.DarkGreen);
                //g.FillEllipse(Brushes.DarkGoldenrod, part.X - r,
                //   part.Y - r, 2 * r, 2 * r);
                var r = virus.Radius;
                g.FillEllipse(brush, virus.X - r,
                   virus.Y - r, 2 * r, 2 * r);
                brush = new SolidBrush(Color.DarkGreen);
                r *= 0.85f;
                g.FillEllipse(brush, virus.X - r,
                   virus.Y - r, 2 * r, 2 * r);
            }
        }

        /// <summary>
        /// Draws the score table.
        /// </summary>
        /// <param name="g">The graphics object used for drawing.</param>
        private void DrawScoreTable(Graphics g)
        {
            if (GameStateCopy?.Players == null)
                return;

            StringBuilder scoreText = new StringBuilder();
            var sortedPlayers = (from player in GameStateCopy.Players
                                 orderby player.Mass descending
                                 select player).ToList();

            for (int i = 0; i < sortedPlayers.Count; i++)
                scoreText.Append($"\n{i + 1}. {sortedPlayers[i].Name} : {sortedPlayers[i].Mass}");

            GameForm.ScoreLabel.Text = "Leaderboard" + scoreText.ToString();
        }

        /// <summary>
        /// Starts the graphics.
        /// </summary>
        public void StartGraphics()
        {
            GameForm.Show();
        }

        /// <summary>
        /// Stops the graphics.
        /// </summary>
        public void StopGraphics()
        {
            GameForm.BeginInvoke(new Action(() =>
            {
                GameForm.Close();
            }));
        }

        /// <summary>
        /// Renders the specified state.
        /// </summary>
        /// <param name="state">The state.</param>
        public void Render(GameState state)
        {
            this.GameStateCopy = state;
            GamePanel.Invalidate();
        }
    }
}