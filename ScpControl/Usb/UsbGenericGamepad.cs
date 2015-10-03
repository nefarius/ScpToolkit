using System;
using System.Collections.Generic;
using System.Linq;
using ScpControl.Utilities;

namespace ScpControl.Usb
{
    public class UsbGenericGamepad : UsbDevice
    {
        public static readonly Guid DeviceClassGuid = Guid.Parse("433FA0C6-2BF1-4675-98C6-7F4FC99796FC");
        private DumpHelper _dumper = new DumpHelper("TEST.dump");

        #region Ctors

        public UsbGenericGamepad()
            : base(DeviceClassGuid.ToString())
        {
        }

        #endregion

        public bool CaptureDefault { get; set; }

        private IEnumerable<byte> _listDefault;

        protected override void Parse(byte[] report)
        {
            if (CaptureDefault)
            {
                _dumper.DumpArray("Default", report, report.Length);

                CaptureDefault = false;
            }
        }
    }
}
