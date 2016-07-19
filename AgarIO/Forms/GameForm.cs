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

    }

    class GamePanel : Panel
    {
        public Image Buffer { get; set; }

        public GamePanel()
        {
            this.DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //var a = Stopwatch.GetTimestamp();
            base.OnPaint(e);
            if (Buffer != null)
                e.Graphics.DrawImage(Buffer, 0, 0);
            //Debug.WriteLine("---- {0}ms",
            //    1000*(Stopwatch.GetTimestamp() - a)/Stopwatch.Frequency);
        }
    }
}