using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Typography.OpenFont;

namespace SampleWinForms
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            //if you want DPI aware----
            Win32DPI.SetProcessDPIAware();
            Typeface.DefaultDpi = Win32DPI.GetDpiForSystem();
            //--------------------------

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
