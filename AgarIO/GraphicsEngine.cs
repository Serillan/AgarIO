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
        static GamePanel GamePanel;
        Image MatrixImage;
        Graphics PanelGraphics;
        Graphics ImageGraphics;
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

            GamePanel.Resize += GamePanel_Resize;
            GamePanel.Buffer = new Bitmap(GamePanel.Width, GamePanel.Height);
            ImageGraphics = Graphics.FromImage(GamePanel.Buffer);
            MatrixPen = new Pen(Color.FromArgb(10, 10, 10), 10);
            GamePanel.Paint += GamePanel_Paint;

        }

        private void GamePanel_Paint(object sender, PaintEventArgs e)
        {
            if (state == null)
                return;
            var parts = state.Players.SelectMany(p => p.Parts).OrderBy(p => p.Radius).ToList();

            long a = 0;

            e.Graphics.Clear(Color.Black);
            DrawMatrix(state, e.Graphics);

            foreach (var part in parts)
            {
                var x = part.X - state.CurrentPlayer.X + GamePanel.Width / 2.0;
                var y = part.Y - state.CurrentPlayer.Y + GamePanel.Height / 2.0;
                var r = GameToViewResize(part.Radius, state);
                e.Graphics.FillEllipse(Brushes.DarkGoldenrod, (float)(x - r),
                    (float)(y - r), (float)(2 * r), (float)(2 * r));
                //Font myFont = new Font("Arial", 14);
                //if (state.CurrentPlayer.Parts.Contains(part))
                //    e.Graphics.DrawString($"{part.X} {part.Y}", myFont, Brushes.Black, 10, 10);
            }

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
            this.state = state;
            GamePanel.Invalidate();
            //GameForm.Invalidate();
            /*
            var parts = state.Players.SelectMany(p => p.Parts).OrderBy(p => p.Radius).ToList();

            long a = 0;
            lock (GamePanel.Buffer)
            {
                ImageGraphics.Clear(Color.AliceBlue);
                DrawMatrix(state, ImageGraphics);

                foreach (var part in parts)
                {
                    var x = part.X - state.CurrentPlayer.X + GamePanel.Width / 2.0;
                    var y = part.Y - state.CurrentPlayer.Y + GamePanel.Height / 2.0;
                    var r = GameToViewResize(part.Radius, state);
                    ImageGraphics.FillEllipse(Brushes.Black, (float)(x - r),
                        (float)(y - r), (float)(2 * r), (float)(2 * r));
                    //Font myFont = new Font("Arial", 14);
                    //if (state.CurrentPlayer.Parts.Contains(part))
                     //   ImageGraphics.DrawString($"{part.X} {part.Y}", myFont, Brushes.Black, 10, 10);
                }
                
            }
            if (GamePanel.Display == null)
                GamePanel.Display = new Bitmap(GamePanel.Width, GamePanel.Height);
            lock (GamePanel.Display)
                GamePanel.Display = (Bitmap)GamePanel.Buffer.Clone();
            GamePanel.Invalidate();
            
            /*
            GamePanel.Invoke(new MethodInvoker(() => {
                GamePanel.Refresh();
            }));
            */
        }

        private void DrawMatrix(GameState state, Graphics g)
        {

            var dx = (float)(-state.CurrentPlayer.X + GamePanel.Width / 2);
            var dy = (float)(-state.CurrentPlayer.Y + GamePanel.Height / 2);

            for (int x = 0; x < Game.MaxLocationX; x += 500)
                if (x + dx >= 0 && x + dx <= GamePanel.Width)
                    g.DrawLine(MatrixPen, x + dx, 0, x + dx, GamePanel.Height);

            for (int y = 0; y < Game.MaxLocationY; y += 500)
                if (y + dy >= 0 && y + dy <= GamePanel.Height)
                    g.DrawLine(MatrixPen, 0, y + dy, GamePanel.Width, y + dy);
        }

        private double GameToViewResize(double value, GameState state)
        {
            double coefficient = (0.02 * GamePanel.Width) / state.CurrentPlayer.Radius;
            return value * coefficient;
        }
    }
}
