using System.Runtime.InteropServices;
using ScpControl.Shared.XInput;

namespace ScpControl.XInput
{
    public static class XInputNatives
    {
        [DllImport("xinput1_3.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint XInputGetState(uint dwUserIndex, ref XINPUT_STATE pState);
    }
}
