using System;
using System.Drawing;
using System.Windows.Forms;
using ScpControl;
using System.Runtime.InteropServices;

namespace ScpPair 
{
    public partial class ScpForm : Form 
    {
        protected IntPtr m_UsbNotify = IntPtr.Zero;

        protected Byte[] Master = new Byte[6];

        public ScpForm() 
        {
            InitializeComponent();
        }

        private void tmEnable_Tick(object sender, EventArgs e) 
        {
            if (usbDevice.State == DsState.Connected)
            {
                String[] Split = tbMaster.Text.Split(new String[] { ":" }, StringSplitOptions.RemoveEmptyEntries);

                if (Split.Length == 6)
                {
                    Boolean Ok = true;

                    for (Int32 Index = 0; Index < 6 && Ok; Index++)
                    {
                        if (Split[Index].Length != 2 || !Byte.TryParse(Split[Index], System.Globalization.NumberStyles.HexNumber, null, out Master[Index]))
                        {
                            Ok = false;
                        }
                    }

                    btnSet.Enabled = Ok;
                }

                lblMac.Text    = usbDevice.Local;
                lblMaster.Text = usbDevice.Remote;
            }
            else
            {
                lblMac.Text    = String.Empty;
                lblMaster.Text = String.Empty;
            }
        }

        private void btnSet_Click(object sender, EventArgs e) 
        {
            usbDevice.Pair(Master);
        }

        private void Form_Load(object sender, EventArgs e) 
        {
            Icon = Properties.Resources.Scp_All;

            if (usbDevice.Open()) usbDevice.Start();

            ScpDevice.RegisterNotify(Handle, new Guid(UsbDs3.USB_CLASS_GUID), ref m_UsbNotify);
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
                    String Path;
                    ScpDevice.DEV_BROADCAST_HDR hdr;
                    Int32 Type = m.WParam.ToInt32();

                    hdr = (ScpDevice.DEV_BROADCAST_HDR)Marshal.PtrToStructure(m.LParam, typeof(ScpDevice.DEV_BROADCAST_HDR));

                    if (hdr.dbch_devicetype == ScpDevice.DBT_DEVTYP_DEVICEINTERFACE)
                    {
                        ScpDevice.DEV_BROADCAST_DEVICEINTERFACE_M deviceInterface;

                        deviceInterface = (ScpDevice.DEV_BROADCAST_DEVICEINTERFACE_M)Marshal.PtrToStructure(m.LParam, typeof(ScpDevice.DEV_BROADCAST_DEVICEINTERFACE_M));

                        Path = new String(deviceInterface.dbcc_name);
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
            catch { }

            base.WndProc(ref m);
        }

        protected void Button_Enter(object sender, EventArgs e) 
        {
            ThemeUtil.UpdateFocus(((Button) sender).Handle);
        }
    }
}
