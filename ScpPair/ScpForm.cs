using System;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ScpControl;
using ScpControl.Shared.Core;
using ScpControl.Usb.Ds3;
using ScpControl.Utilities;
using ScpPair.Properties;

namespace ScpPair
{
    public partial class ScpForm : Form
    {
        protected IntPtr m_UsbNotify = IntPtr.Zero;

        protected byte[] Master = new byte[6];

        public ScpForm()
        {
            InitializeComponent();
        }

        private void tmEnable_Tick(object sender, EventArgs e)
        {
            if (usbDevice.State == DsState.Connected)
            {
                var Split = tbMaster.Text.Split(new[] {":"}, StringSplitOptions.RemoveEmptyEntries);

                if (Split.Length == 6)
                {
                    var Ok = true;

                    for (var Index = 0; Index < 6 && Ok; Index++)
                    {
                        if (Split[Index].Length != 2 ||
                            !byte.TryParse(Split[Index], NumberStyles.HexNumber, null, out Master[Index]))
                        {
                            Ok = false;
                        }
                    }

                    btnSet.Enabled = Ok;
                }

                lblMac.Text = usbDevice.DeviceAddress.AsFriendlyName();
                lblMaster.Text = usbDevice.HostAddress.AsFriendlyName();
            }
            else
            {
                lblMac.Text = string.Empty;
                lblMaster.Text = string.Empty;
            }
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            usbDevice.Pair(new PhysicalAddress(Master));
        }

        private void Form_Load(object sender, EventArgs e)
        {
            Icon = Resources.Scp_All;

            if (usbDevice.Open()) usbDevice.Start();

            ScpDevice.RegisterNotify(Handle, UsbDs3.DeviceClassGuid, ref m_UsbNotify);
        }

        private void Form_Close(object sender, FormClosingEventArgs e)
        {
            if (m_UsbNotify != IntPtr.Zero) ScpDevice.UnregisterNotify(m_UsbNotify);

            if (usbDevice.State == DsState.Connected) usbDevice.Close();
        }

        protected override void WndProc(ref Message m)
        {
            try
            {
                if (m.Msg == ScpDevice.WM_DEVICECHANGE)
                {
                    string Path;
                    ScpDevice.DEV_BROADCAST_HDR hdr;
                    var Type = m.WParam.ToInt32();

                    hdr =
                        (ScpDevice.DEV_BROADCAST_HDR)
                            Marshal.PtrToStructure(m.LParam, typeof (ScpDevice.DEV_BROADCAST_HDR));

                    if (hdr.dbch_devicetype == ScpDevice.DBT_DEVTYP_DEVICEINTERFACE)
                    {
                        ScpDevice.DEV_BROADCAST_DEVICEINTERFACE_M deviceInterface;

                        deviceInterface =
                            (ScpDevice.DEV_BROADCAST_DEVICEINTERFACE_M)
                                Marshal.PtrToStructure(m.LParam, typeof (ScpDevice.DEV_BROADCAST_DEVICEINTERFACE_M));

                        Path = new string(deviceInterface.dbcc_name);
                        Path = Path.Substring(0, Path.IndexOf('\0')).ToUpper();

                        switch (Type)
                        {
                            case ScpDevice.DBT_DEVICEARRIVAL:

                                if (usbDevice.State != DsState.Connected)
                                {
                                    usbDevice.Close();
                                    usbDevice = new UsbDs3();

                                    if (usbDevice.Open(Path)) usbDevice.Start();
                                }
                                break;

                            case ScpDevice.DBT_DEVICEREMOVECOMPLETE:

                                if (Path == usbDevice.Path && usbDevice.State == DsState.Connected)
                                {
                                    usbDevice.Close();
                                }

                                break;
                        }
                    }
                }
            }
            catch
            {
            }

            base.WndProc(ref m);
        }

        protected void Button_Enter(object sender, EventArgs e)
        {
            ThemeUtil.UpdateFocus(((Button) sender).Handle);
        }
    }
}