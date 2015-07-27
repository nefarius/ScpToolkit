using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ScpControl.Utilities
{
    public static class ThemeUtil
    {
        private const int WM_CHANGEUISTATE = 0x127;
        private const int HIDEFOCUS = 0x10001;

        [DllImport("UxTheme", CharSet = CharSet.Auto)]
        private static extern int SetWindowTheme(IntPtr hWnd, string appName, string partList);

        [DllImport("User32", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        public static void SetTheme(ListView lv)
        {
            try
            {
                SetWindowTheme(lv.Handle, "Explorer", null);
                SendMessage(lv.Handle, WM_CHANGEUISTATE, HIDEFOCUS, 0);
            }
            catch
            {
            }
        }

        public static void SetTheme(TreeView tv)
        {
            try
            {
                SetWindowTheme(tv.Handle, "Explorer", null);
                SendMessage(tv.Handle, WM_CHANGEUISTATE, HIDEFOCUS, 0);
            }
            catch
            {
            }
        }

        public static void UpdateFocus(IntPtr Handle)
        {
            try
            {
                SendMessage(Handle, WM_CHANGEUISTATE, HIDEFOCUS, 0);
            }
            catch
            {
            }
        }
    }
}
