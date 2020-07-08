//MIT, 2016-present, WinterDev
using System;
using System.IO;
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
            //---------
            //user can skip this, and set it by app-manifest
            //but here I, set DPI-aware by API :)
            //-------


            bool dpi_result = SetProcessDPIAware();
            Typeface.DefaultDpi = GetDpiForSystem();

            OurOpenFontSystemSetup.Setup();           


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }


        //-------
        //https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setprocessdpiaware?redirectedfrom=MSDN
        [System.Runtime.InteropServices.DllImport("user32")]
        static extern bool SetProcessDPIAware();

        //UINT GetDpiForWindow(
        //  HWND hwnd
        //);
        //https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getdpiforwindow

        [System.Runtime.InteropServices.DllImport("user32")]
        static extern uint GetDpiForWindow(IntPtr ptr);

        //UINT GetDpiForSystem();
        //https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getdpiforsystem
        [System.Runtime.InteropServices.DllImport("user32")]
        static extern uint GetDpiForSystem();
        //------- 
    }




}
