using System;
using System.Runtime.InteropServices;
using RGiesecke.DllExport;

namespace ScpXInputBridge
{
    public partial class XInputDll
    {
        #region SCP extension function

        [DllExport("XInputGetExtended", CallingConvention.StdCall)]
        public static uint XInputGetExtended(uint dwUserIndex, ref SCP_EXTN pPressure)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
