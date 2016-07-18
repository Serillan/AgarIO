using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgarIO.Forms;

namespace AgarIO
{
    public class GraphicsEngine
    {
        GameForm GameForm;

        public GraphicsEngine(GameForm gameForm)
        {
            GameForm = gameForm;
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

        public async Task RenderAsync()
        {

        }
    }
}
