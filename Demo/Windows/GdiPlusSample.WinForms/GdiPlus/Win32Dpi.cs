using System;
namespace SampleWinForms
{
    static class Win32DPI
    {

        //-------
        //https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setprocessdpiaware?redirectedfrom=MSDN
        [System.Runtime.InteropServices.DllImport("user32")]
        public static extern bool SetProcessDPIAware();

        //UINT GetDpiForWindow(
        //  HWND hwnd
        //);
        //https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getdpiforwindow

        [System.Runtime.InteropServices.DllImport("user32")]
        public static extern uint GetDpiForWindow(IntPtr ptr);

        //UINT GetDpiForSystem();
        //https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getdpiforsystem
        [System.Runtime.InteropServices.DllImport("user32")]
        public static extern uint GetDpiForSystem();
        //------- 
    }

}