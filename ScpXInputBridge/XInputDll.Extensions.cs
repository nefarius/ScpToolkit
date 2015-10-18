using System.Runtime.InteropServices;
using RGiesecke.DllExport;
using ScpControl.Shared.XInput;

namespace ScpXInputBridge
{
    public partial class XInputDll
    {
        #region SCP extension function

        [DllExport("XInputGetExtended", CallingConvention.StdCall)]
        public static uint XInputGetExtended(uint dwUserIndex, ref SCP_EXTN pPressure)
        {
            // TODO: add error handling
            pPressure = _scpProxy.GetExtended(dwUserIndex);

            return 0; // success
        }

        #endregion
    }
}
