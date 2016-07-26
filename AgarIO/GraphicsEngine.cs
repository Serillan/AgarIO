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

        GameState state;

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
            if (state == null)
                return;
            DrawGame(e.Graphics);
        }

        private void DrawGame(Graphics g)
        {
            if (state == null || state.CurrentPlayer == null)
                return;
            g.Clear(Color.Black);
            TransformScene(g);
            DrawMatrix(g);
            DrawPlayers(g);
        }

        private void TransformScene(Graphics g)
        {
            float q = (float)((0.1 * GamePanel.Width) / state.CurrentPlayer.Radius);
            // opposite order!
            g.TranslateTransform(GamePanel.Width / 2.0f, GamePanel.Height / 2.0f);
            if (q < 1) // don't scale until it reaches max view radius (_k_ * GamePanel.Width)
                g.ScaleTransform(q, q);
            g.TranslateTransform(-state.CurrentPlayer.X, -state.CurrentPlayer.Y); // will be applied 1.
        }

        private void DrawPlayers(Graphics g)
        {
            var playerPartsWithColorAndName = from player in state.Players select new { player.Color, player.Parts, PlayerName = player.Name };

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
            this.state = state;
            GamePanel.Invalidate();
        }
    }
}