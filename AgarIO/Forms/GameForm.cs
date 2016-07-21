using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AgarIO.Forms
{
    partial class GameForm : Form
    {

        public GameForm()
        {
            InitializeComponent();
        }
        /*
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }
        */
    }

    class GamePanel : PictureBox
    {
        public Bitmap Buffer { get; set; }
        public Bitmap Display { get; set; }

        public GamePanel()
        {
            //this.DoubleBuffered = true;
           // SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint |
            //   ControlStyles.UserPaint, true);
            // Enable double duffering to stop flickering.
            //if (!this.IsHandleCreated)
            //{
             //   this.CreateHandle();
            //}
        }




        protected override void OnPaint(PaintEventArgs e)
        {
            var a = Stopwatch.GetTimestamp();
            base.OnPaint(e);
            /*
            if (Display != null)
                lock (Display)
                    e.Graphics.DrawImage(Display, Point.Empty);

            Debug.WriteLine("---- {0}ms",
                1000 * (Stopwatch.GetTimestamp() - a) / Stopwatch.Frequency);
                */
        }

    }
}