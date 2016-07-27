using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgarIO.Forms;
using System.Drawing;
using System.Windows.Forms;
using AgarIO.Entities;
using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace AgarIO
{
    class GraphicsEngine
    {
        GameForm GameForm;
        static GamePanel GamePanel;
        Pen MatrixPen;

        GameState State;

        static public int GamePanelWidth
        {
            get
            {
                return GamePanel.Width;
            }
        }

        static public int GamePanelHeight
        {
            get
            {
                return GamePanel.Height;
            }
        }

        public GraphicsEngine(GameForm gameForm)
        {
            GameForm = gameForm;
            GamePanel = GameForm.GamePanel;

            MatrixPen = new Pen(Color.FromArgb(20, 20, 20), 10);
            GamePanel.Paint += GamePanel_Paint;
        }

        private void GamePanel_Paint(object sender, PaintEventArgs e)
        {
            if (State == null)
                return;
            DrawGame(e.Graphics);
        }

        private void DrawGame(Graphics g)
        {
            if (State == null || State.CurrentPlayer == null)
                return;
            g.Clear(Color.Black);
            TransformScene(g);
            DrawMatrix(g);
            DrawFood(g);
            DrawPlayers(g);
            DrawScoreTable(g);
        }

        private void TransformScene(Graphics g)
        {
            float q = (float)((0.1 * GamePanel.Width) / State.CurrentPlayer.Radius);
            // opposite order!
            g.TranslateTransform(GamePanel.Width / 2.0f, GamePanel.Height / 2.0f);
            if (q < 1) // don't scale until it reaches max view radius (_k_ * GamePanel.Width)
                g.ScaleTransform(q, q);
            g.TranslateTransform(-State.CurrentPlayer.X, -State.CurrentPlayer.Y); // will be applied 1.
        }

        private void DrawPlayers(Graphics g)
        {
            var playerPartsWithColorAndName = from player in State.Players select new { player.Color, player.Parts, PlayerName = player.Name };

            var partsWithColorAndName = (
                        from partWithColorAndName in playerPartsWithColorAndName
                        from part in partWithColorAndName.Parts
                        select new { Part = part, partWithColorAndName.Color, partWithColorAndName.PlayerName }
                        ).OrderBy(p => p.Part.Radius).ToList();

            foreach (var partWithColorAndName in partsWithColorAndName)
            {
                //var d = (float)(Math.Sqrt(2 * part.Radius * part.Radius));
                var part = partWithColorAndName.Part;
                var radius = part.Radius;
                var color = partWithColorAndName.Color;
                var name = partWithColorAndName.PlayerName;

                var brush = new SolidBrush(Color.FromArgb(color[0], color[1], color[2]));
                //g.FillEllipse(Brushes.DarkGoldenrod, part.X - r,
                //   part.Y - r, 2 * r, 2 * r);
                g.FillEllipse(brush, part.X - radius,
                   part.Y - radius, 2 * radius, 2 * radius);
                Font myFont = new Font("Arial", 14);

                var sizeOfText = g.MeasureString(name, myFont);
                g.DrawString(name, myFont, Brushes.Black, part.X - sizeOfText.Width / 2, part.Y - sizeOfText.Height / 2);

                //if (state.CurrentPlayer.Parts.Contains(part))
                //    e.Graphics.DrawString($"{part.X} {part.Y}", myFont, Brushes.Black, 10, 10);
            }
        }

        private void DrawMatrix(Graphics g)
        {
            for (int x = 0; x <= Game.MaxLocationX; x += 120)
                g.DrawLine(MatrixPen, x, 0, x, Game.MaxLocationY);

            for (int y = 0; y <= Game.MaxLocationY; y += 120)
                g.DrawLine(MatrixPen, 0, y, Game.MaxLocationX, y);
        }

        private void DrawFood(Graphics g)
        {
            if (State.Food == null)
                return;
            foreach (var food in State.Food)
            {
                var brush = new SolidBrush(Color.FromArgb(food.Color[0], food.Color[1], food.Color[2]));
                //g.FillEllipse(Brushes.DarkGoldenrod, part.X - r,
                //   part.Y - r, 2 * r, 2 * r);
                g.FillEllipse(brush, food.X - food.Radius,
                   food.Y - food.Radius, 2 * food.Radius, 2 * food.Radius);
            }
        }

        private void DrawScoreTable(Graphics g)
        {
            if (State?.Players == null)
                return;

            StringBuilder scoreText = new StringBuilder();
            var sortedPlayers = (from player in State.Players
                             orderby player.Mass
                             select player).ToList();

            for (int i = 0; i < sortedPlayers.Count; i++)
                scoreText.Append($"\n{i + 1}. {sortedPlayers[i].Name} : {sortedPlayers[i].Mass}");

            GameForm.ScoreLabel.Text = "Leaderboard" + scoreText.ToString();
        }

        public void StartGraphics()
        {
            GameForm.Show();
        }

        public void StopGraphics()
        {
            GameForm.BeginInvoke(new Action(() =>
            {
                GameForm.Close();
            }));
        }

        public void Render(GameState state)
        {
            this.State = state;
            GamePanel.Invalidate();
        }
    }
}