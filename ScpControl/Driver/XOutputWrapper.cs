using System.Runtime.InteropServices;
using ScpControl.Shared.XInput;

namespace ScpControl.Driver
{
    public class XOutputWrapper : NativeLibraryWrapper<XOutputWrapper>
    {
        #region Ctor

        private XOutputWrapper()
        {
            LoadNativeLibrary("XOutput1_1", @"XOutput\x86\XOutput1_1.dll", @"XOutput\amd64\XOutput1_1.dll");
        }

        #endregion

        #region Public methods

        public void SetState(uint userIndex, XINPUT_GAMEPAD gamepad)
        {
            XOutputSetState(userIndex, ref gamepad);
        }

        public bool GetState(uint userIndex, ref byte largeMotor, ref byte smallMotor)
        {
            byte vibrate = 0;

            return (XOutputGetState(userIndex, ref vibrate, ref largeMotor, ref smallMotor) == 0 && vibrate == 0x01);
        }

        public uint GetRealIndex(uint userIndex)
        {
            uint realIndex = 0;

            XOutputGetRealUserIndex(userIndex, ref realIndex);

            return realIndex;
        }

        public bool PlugIn(int userIndex)
        {
            return (XOutputPlugIn((uint) userIndex) == 0);
        }

        public bool UnPlug(int userIndex)
        {
            return (XOutputUnPlug((uint) userIndex) == 0);
        }

        public void UnPlugAll()
        {
            XOutputUnPlugAll();
        }
        
        #endregion
        
        #region P/Invoke

        [DllImport("XOutput1_1.dll")]
        private static extern uint XOutputSetState(uint userIndex, ref XINPUT_GAMEPAD gamepad);

        [DllImport("XOutput1_1.dll")]
        private static extern uint XOutputGetState(uint userIndex, ref byte vibrate, ref byte largeMotor, ref byte smallMotor);

        [DllImport("XOutput1_1.dll")]
        private static extern uint XOutputGetRealUserIndex(uint userIndex, ref uint realIndex);

        [DllImport("XOutput1_1.dll")]
        private static extern uint XOutputPlugIn(uint userIndex);

        [DllImport("XOutput1_1.dll")]
        private static extern uint XOutputUnPlug(uint userIndex);

        [DllImport("XOutput1_1.dll")]
        private static extern uint XOutputUnPlugAll();

        #endregion
    }
}
