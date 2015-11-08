namespace ScpControl.Profiler
{
    public class NativeInputReport
    {
        public byte[] RawBytes { get; private set; }

        public NativeInputReport(byte[] report)
        {
            RawBytes = report;
        }

        public IDsButtonState this[IDsButton button]
        {
            get
            {
                if (button is Ds3Button)
                {
                    var buttons = (uint)((RawBytes[10] << 0) | (RawBytes[11] << 8) | (RawBytes[12] << 16) | (RawBytes[13] << 24));

                    return new DsButtonState()
                    {
                        IsPressed = (buttons & button.RawValue) == button.RawValue,
                        Value = (byte)button.RawValue
                    };
                }

                return null;
            }
        }
    }
}
