using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Common.Maths;
using Common.Gui;
using System.Drawing;
using System.Diagnostics;

namespace Common
{
    internal static class Program
    {
        /// <summary>
        /// Punto di ingresso principale dell'applicazione.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (var form = new GuiForm())
            {
                Application.Run(form);
            }
        }
    }
}
