using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ScpControl.Utilities
{
    [Obsolete]
    public static class KbmPost
    {
        public enum MouseButtons
        {
            Left = 0x0002,
            Right = 0x0008,
            Middle = 0x0020
        };

        private const int MOUSE_VWHEEL = 0x0800;
        private const int MOUSE_HWHEEL = 0x1000;
        private const int WHEEL_DELTA = 120;
        private const int MOUSE_MOVE = 1;
        private const int VK_STANDARD = 0;
        private const int VK_EXTENDED = 1;
        private const int VK_KEYDOWN = 0;
        private const int VK_KEYUP = 2;

        [DllImport("User32", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true,
            CallingConvention = CallingConvention.Winapi)]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, IntPtr dwExtraInfo);

        [DllImport("User32", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true,
            CallingConvention = CallingConvention.Winapi)]
        private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, IntPtr dwExtraInfo);

        [DllImport("User32", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true,
            CallingConvention = CallingConvention.Winapi)]
        private static extern uint MapVirtualKeyW(uint uCode, uint uMapType);

        public static void Key(Keys Key, bool bExtended, bool bDown)
        {
            keybd_event((byte)Key, (byte)MapVirtualKeyW((uint)Key, 0),
                (bDown ? VK_KEYDOWN : VK_KEYUP) | (bExtended ? VK_EXTENDED : VK_STANDARD), IntPtr.Zero);
        }

        public static void Move(int dx, int dy)
        {
            mouse_event(MOUSE_MOVE, dx, dy, 0, IntPtr.Zero);
        }

        public static void Button(MouseButtons Button, bool bDown)
        {
            mouse_event(bDown ? (int)Button : (int)Button << 1, 0, 0, 0, IntPtr.Zero);
        }

        public static void Wheel(bool bVertical, int Clicks)
        {
            mouse_event(bVertical ? MOUSE_VWHEEL : MOUSE_HWHEEL, 0, 0, Clicks * WHEEL_DELTA, IntPtr.Zero);
        }
    }
}
