using System;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;

using ScpControl;

namespace ScpServer 
{
    public partial class ScpForm : Form 
    {
        protected String m_Log = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName + "\\" + Assembly.GetExecutingAssembly().GetName().Name + ".log";

        delegate void LogDebugDelegate(DateTime Time, String Data);

        protected void LogDebug(DateTime Time, String Data) 
        {
            if (lvDebug.InvokeRequired)
            {
                LogDebugDelegate d = new LogDebugDelegate(LogDebug);
                try
                {
                    this.Invoke(d, new Object[] { Time, Data });
                }
                catch { }
            }
            else
            {
                String Posted = Time.ToString("yyyy-MM-dd HH:mm:ss.fff");

                lvDebug.Items.Add(new ListViewItem(new String[] { Posted, Data })).EnsureVisible();

                try
                {
                    using (StreamWriter fs = new StreamWriter(m_Log, true))
                    {
                        fs.Write(String.Format("{0} {1}\r\n", Posted, Data));
                        fs.Flush();
                    }
                }
                catch { }
            }
        }

        protected IntPtr m_Ds3Notify = IntPtr.Zero;
        protected IntPtr m_Ds4Notify = IntPtr.Zero;
        protected IntPtr m_BthNotify = IntPtr.Zero;

        protected RadioButton[] Pad = new RadioButton[4];

        public ScpForm() 
        {
            InitializeComponent();
            ThemeUtil.SetTheme(lvDebug);

            Pad[0] = rbPad_1;
            Pad[1] = rbPad_2;
            Pad[2] = rbPad_3;
            Pad[3] = rbPad_4;
        }

        protected void Form_Load(object sender, EventArgs e) 
        {
            Icon = Properties.Resources.Scp_All;

            ScpDevice.RegisterNotify(Handle, new Guid(UsbDs3.USB_CLASS_GUID   ), ref m_Ds3Notify);
            ScpDevice.RegisterNotify(Handle, new Guid(UsbDs4.USB_CLASS_GUID   ), ref m_Ds4Notify);
            ScpDevice.RegisterNotify(Handle, new Guid(BthDongle.BTH_CLASS_GUID), ref m_BthNotify);

            LogDebug(DateTime.Now, String.Format("++ {0}  {1}", Assembly.GetExecutingAssembly().Location, Assembly.GetExecutingAssembly().GetName().Version.ToString()));

            tmrUpdate.Enabled = true;
            btnStart_Click(sender, e);
        }

        protected void Form_Close(object sender, FormClosingEventArgs e) 
        {
            rootHub.Stop();
            rootHub.Close();

            if (m_Ds3Notify != IntPtr.Zero) ScpDevice.UnregisterNotify(m_Ds3Notify);
            if (m_Ds4Notify != IntPtr.Zero) ScpDevice.UnregisterNotify(m_Ds4Notify);
            if (m_BthNotify != IntPtr.Zero) ScpDevice.UnregisterNotify(m_BthNotify);
        }

        protected void btnStart_Click(object sender, EventArgs e) 
        {
            if (rootHub.Open() && rootHub.Start())
            {
                btnStart.Enabled = false;
                btnStop.Enabled  = true;
            }
        }

        protected void btnStop_Click(object sender, EventArgs e) 
        {
            if (rootHub.Stop())
            {
                btnStart.Enabled = true;
                btnStop.Enabled  = false;
            }
        }

        protected void btnClear_Click(object sender, EventArgs e) 
        {
            lvDebug.Items.Clear();
        }

        protected void btnMotor_Click(object sender, EventArgs e) 
        {
            Button Target = (Button) sender;
            Byte Left = 0x00, Right = 0x00;

            if (Target == btnBoth)
            {
                Left = 0xFF; Right = 0xFF;
            }
            else if (Target == btnLeft ) Left  = 0xFF;
            else if (Target == btnRight) Right = 0xFF;

            for (int Index = 0; Index < 4; Index++)
            {
                if (Pad[Index].Enabled && Pad[Index].Checked)
                {
                    rootHub.Pad[Index].Rumble(Left, Right);
                }
            }
        }

        protected void btnPair_Click(object sender, EventArgs e) 
        {
            for (Int32 Index = 0; Index < Pad.Length; Index++)
            {
                if (Pad[Index].Checked)
                {
                    Byte[]   Master = new Byte[6];
                    String[] Parts  = rootHub.Master.Split(new String[] { ":" }, StringSplitOptions.RemoveEmptyEntries);

                    for (Int32 Part = 0; Part < Master.Length; Part++)
                    {
                        Master[Part] = Byte.Parse(Parts[Part], System.Globalization.NumberStyles.HexNumber);
                    }

                    rootHub.Pad[Index].Pair(Master);
                    break;
                }
            }
        }

        protected void btnDisconnect_Click(object sender, EventArgs e) 
        {
            for (Int32 Index = 0; Index < Pad.Length; Index++)
            {
                if (Pad[Index].Checked)
                {
                    rootHub.Pad[Index].Disconnect();
                    break;
                }
            }
        }

        protected void btnSuspend_Click(object sender, EventArgs e) 
        {
            rootHub.Suspend();
        }

        protected void btnResume_Click(object sender, EventArgs e) 
        {
            rootHub.Resume();
        }

        protected override void WndProc(ref Message m) 
        {
            try
            {
                if (m.Msg == ScpDevice.WM_DEVICECHANGE)
                {
                    Int32 Type = m.WParam.ToInt32();

                    switch (Type)
                    {
                        case ScpDevice.DBT_DEVICEARRIVAL:
                        case ScpDevice.DBT_DEVICEQUERYREMOVE:
                        case ScpDevice.DBT_DEVICEREMOVECOMPLETE:

                            ScpDevice.DEV_BROADCAST_HDR hdr;

                            hdr = (ScpDevice.DEV_BROADCAST_HDR) Marshal.PtrToStructure(m.LParam, typeof(ScpDevice.DEV_BROADCAST_HDR));

                            if (hdr.dbch_devicetype == ScpDevice.DBT_DEVTYP_DEVICEINTERFACE)
                            {
                                ScpDevice.DEV_BROADCAST_DEVICEINTERFACE_M deviceInterface;

                                deviceInterface = (ScpDevice.DEV_BROADCAST_DEVICEINTERFACE_M) Marshal.PtrToStructure(m.LParam, typeof(ScpDevice.DEV_BROADCAST_DEVICEINTERFACE_M));

                                String Class = "{" + new Guid(deviceInterface.dbcc_classguid).ToString().ToUpper() + "}";

                                String Path = new String(deviceInterface.dbcc_name);
                                Path = Path.Substring(0, Path.IndexOf('\0')).ToUpper();

                                rootHub.Notify((ScpDevice.Notified) Type, Class, Path);
                            }
                            break;
                    }
                }
            }
            catch { }

            base.WndProc(ref m);
        }

        protected void tmrUpdate_Tick(object sender, EventArgs e) 
        {
            Boolean bSelected = false, bDisconnect = false, bPair = false;

            lblHost.Text = rootHub.Dongle; lblHost.Enabled = btnStop.Enabled;

            for (Int32 Index = 0; Index < Pad.Length; Index++)
            {
                Pad[Index].Text    = rootHub.Pad[Index].ToString();
                Pad[Index].Enabled = rootHub.Pad[Index].State == DsState.Connected;
                Pad[Index].Checked = Pad[Index].Enabled && Pad[Index].Checked;

                bSelected   = bSelected   || Pad[Index].Checked;
                bDisconnect = bDisconnect || rootHub.Pad[Index].Connection == DsConnection.BTH;

                bPair = bPair || (Pad[Index].Checked && rootHub.Pad[Index].Connection == DsConnection.USB && rootHub.Master != rootHub.Pad[Index].Remote);
            }

            btnBoth.Enabled = btnLeft.Enabled = btnRight.Enabled = btnOff.Enabled = bSelected && btnStop.Enabled;

            btnPair.Enabled = bPair && bSelected && btnStop.Enabled && rootHub.Pairable;

            btnClear.Enabled = lvDebug.Items.Count > 0;
        }

        protected void On_Debug(object sender, ScpControl.DebugEventArgs e) 
        {
            LogDebug(e.Time, e.Data);
        }


        protected void lvDebug_Enter(object sender, EventArgs e) 
        {
            ThemeUtil.UpdateFocus(lvDebug.Handle);
        }

        protected void Button_Enter(object sender, EventArgs e) 
        {
            ThemeUtil.UpdateFocus(((Button) sender).Handle);
        }
    }
}
