using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Text;
using System.Windows.Forms;
using SpiralEdge.Helper;

namespace EMGU
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ShapDetectionFrm());
        }
    }
}
