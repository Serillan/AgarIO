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

namespace DarkAgar.Forms
{
    /// <summary>
    /// Game Form used for displaying the game.
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form" />
    partial class GameForm : Form
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="GameForm"/> class.
        /// </summary>
        public GameForm()
        {
            InitializeComponent();
        }
    }

    /// <summary>
    /// Game Panel used for displaying the game.
    /// </summary>
    /// <seealso cref="System.Windows.Forms.PictureBox" />
    class GamePanel : PictureBox
    {

    }
}