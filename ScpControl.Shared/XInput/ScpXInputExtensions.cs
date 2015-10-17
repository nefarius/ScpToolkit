using System.Runtime.InteropServices;

namespace ScpControl.Shared.XInput
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SCP_EXTN
    {
        public float SCP_UP;
        public float SCP_RIGHT;
        public float SCP_DOWN;
        public float SCP_LEFT;

        public float SCP_LX;
        public float SCP_LY;

        public float SCP_L1;
        public float SCP_L2;
        public float SCP_L3;

        public float SCP_RX;
        public float SCP_RY;

        public float SCP_R1;
        public float SCP_R2;
        public float SCP_R3;

        public float SCP_T;
        public float SCP_C;
        public float SCP_X;
        public float SCP_S;

        public float SCP_SELECT;
        public float SCP_START;

        public float SCP_PS;
    };
}
