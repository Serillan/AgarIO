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
        GamePanel GamePanel;
        Graphics PanelGraphics;
        Graphics ImageGraphics;

        public GraphicsEngine(GameForm gameForm)
        {
            GameForm = gameForm;
            GamePanel = GameForm.GamePanel;

            GamePanel.Resize += GamePanel_Resize;
            GamePanel.Buffer = new Bitmap(GamePanel.Width, GamePanel.Height);
            ImageGraphics = Graphics.FromImage(GamePanel.Buffer);
        }

        public void StartGraphics()
        {
            GameForm.Show();
        }

        private void GamePanel_Resize(object sender, EventArgs e)
        {
            lock (GamePanel.Buffer)
                GamePanel.Buffer = new Bitmap(GamePanel.Width, GamePanel.Height);
            ImageGraphics = Graphics.FromImage(GamePanel.Buffer);
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
            var parts = state.Players.SelectMany(p => p.Parts).OrderBy(p => p.Radius).ToList();

            long a = 0;
            lock (GamePanel.Buffer)
            {
                foreach (var part in parts)
                {
                    var x = part.X - state.CurrentPlayer.X + GamePanel.Width / 2;
                    var y = part.Y - state.CurrentPlayer.Y + GamePanel.Height / 2;

                    var r = GameToViewResize(part.Radius, state);
                    ImageGraphics.FillEllipse(Brushes.Black, (float)(x - r),
                        (float)(y - r), (float)(2 * r), (float)(2 * r));
                }
            }
            GamePanel.Invalidate();
            /*
            GamePanel.Invoke(new MethodInvoker(() => {
                GamePanel.Refresh();
            }));
            */
        }

        private double GameToViewResize(double value, GameState state)
        {
            double coefficient = (0.02 * GamePanel.Width) / state.CurrentPlayer.Radius;
            return value * coefficient;
        }
    }
}
