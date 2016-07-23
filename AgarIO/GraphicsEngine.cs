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
            var parts = state.Players.SelectMany(p => p.Parts).OrderBy(p => p.Radius).ToList();

            foreach (var part in parts)
            {
                var d = (float)(Math.Sqrt(2 * part.Radius * part.Radius));
                g.FillEllipse(Brushes.DarkGoldenrod, part.X - d,
                    part.Y - d, 2 * d, 2 * d);
                //Font myFont = new Font("Arial", 14);
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