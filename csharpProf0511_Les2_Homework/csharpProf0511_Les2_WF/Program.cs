using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace csharpProf0511_Les2_WF
{
    static class Program
    {
        static void Main(string[] args)
        {
            Form form = new Form();
            form.Width = 800;
            form.Height = 600;
            Game.Init(form);
            form.Show();
            Game.Draw();
            Application.Run(form);
        }
    }
}
