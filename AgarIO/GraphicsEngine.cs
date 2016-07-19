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

namespace AgarIO
{
    class GraphicsEngine
    {
        GameForm GameForm;
        Panel GamePanel;
        Image BufferedImage;

        public GraphicsEngine(GameForm gameForm)
        {
            GameForm = gameForm;
            GamePanel = GameForm.GamePanel;
            GameForm.GamePanel.Paint += GamePanel_Paint;
            GamePanel.Resize += GamePanel_Resize;
            BufferedImage = new Bitmap(GamePanel.Width, GamePanel.Height);

        }

        public void StartGraphics()
        {
            GameForm.Show();
        }

        private void GamePanel_Resize(object sender, EventArgs e)
        {
            BufferedImage = new Bitmap(GamePanel.Width, GamePanel.Height);
        }

        private void GamePanel_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            Debug.WriteLine("rendering");

            /*
            using (Graphics g = Graphics.FromImage(BufferedImage))
            {
                g.DrawEllipse(Pens.Black, 0, 0, 50, 50);
            }
            */
            e.Graphics.DrawImage(BufferedImage, new Point(0, 0));
        }

        public void StopGraphics()
        {
            GameForm.BeginInvoke(new Action(() =>
            {
                GameForm.Close();
            }));
        }

        public async Task RenderAsync(GameState state)
        {
            var parts = state.Players.SelectMany(p => p.Parts).OrderBy(p => p.Radius).ToList();

            foreach (var part in parts)
            {
                part.X -= state.CurrentPlayer.X - GamePanel.Width / 2;
                part.Y -= state.CurrentPlayer.Y - GamePanel.Height / 2;
            }
            
            using (Graphics g = Graphics.FromImage(BufferedImage))
            {
                foreach (var part in parts)
                {
                    var r = GameToViewResize(part.Radius, state);
                    g.DrawEllipse(Pens.Black, (float)(part.X - r), 
                        (float)(part.Y - r), (float)(2 * r), (float)(2 * r));
                }

            }
            
            /*
            using (Graphics g = Graphics.FromImage(BufferedImage))
            {
                g.DrawEllipse(Pens.Black, 0, 0, 50, 50);
            }
            */
            GamePanel.Invalidate();
        }

        private double GameToViewResize(double value, GameState state)
        {
            double coefficient = (0.1 * GamePanel.Width) / state.CurrentPlayer.Radius;
            return value * coefficient;
        }
    }
}
