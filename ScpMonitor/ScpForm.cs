using System;
using System.Windows.Forms;
using System.Text;
using System.Drawing;
using System.Configuration;

using System.Net;
using System.Net.Sockets;

using ScpControl;

namespace ScpMonitor 
{
    public partial class ScpForm : Form 
    {
        protected RegistrySettings m_Config = new RegistrySettings();

        protected Boolean FormSaved, ConfSaved, ProfSaved, FormVisible;
        protected Int32   FormX, FormY, ConfX, ConfY, ProfX, ProfY;

        protected Boolean m_Connected = false;

        protected IPEndPoint m_ServerEp = new IPEndPoint(IPAddress.Loopback, 26760);
        protected UdpClient  m_Server   = new UdpClient();

        protected Byte[] m_Buffer = new Byte[2];
        protected Char[] m_Delim  = new Char[] { '^' };

        protected SettingsForm Settings = new SettingsForm();
        protected ProfilesForm Profiles = new ProfilesForm();

        public ScpForm() 
        {
            InitializeComponent();
            btnUp_1.Tag = (Byte) 1;
            btnUp_2.Tag = (Byte) 2;
            btnUp_3.Tag = (Byte) 3;

            m_Server.Client.ReceiveTimeout = 250;
            m_Buffer[1] = 0x02;

            FormVisible = m_Config.Visible;

            FormSaved = m_Config.FormSaved;
            FormX = m_Config.FormX;
            FormY = m_Config.FormY;

            ConfSaved = m_Config.ConfSaved;
            ConfX = m_Config.ConfX;
            ConfY = m_Config.ConfY;

            ProfSaved = m_Config.ProfSaved;
            ProfX = m_Config.ProfX;
            ProfY = m_Config.ProfY;

            if (FormSaved)
            {
                StartPosition = FormStartPosition.Manual;
                Location = new System.Drawing.Point(FormX, FormY);
            }

            if (!FormVisible)
            {
                WindowState = FormWindowState.Minimized;
                Visible = false;
            }

            if (ConfSaved)
            {
                Settings.StartPosition = FormStartPosition.Manual;
                Settings.Location = new System.Drawing.Point(ConfX, ConfY);
            }

            if (ProfSaved)
            {
                Profiles.StartPosition = FormStartPosition.Manual;
                Profiles.Location = new System.Drawing.Point(ProfX, ProfY);
            }

            lblHost.Text  = "Host Address : 00:00:00:00:00:00\r\n\r\n0\r\n\r\n0\r\n\r\n0";
            lblPad_1.Text = "Pad 1 : DS3 00:00:00:00:00:00 - USB FFFFFFFF Charging";

            Int32 SizeX = 50 + lblHost.Width + lblPad_1.Width;
            Int32 SizeY = 20 + lblHost.Height;

            lblPad_1.Location = new Point(new Size(40 + lblHost.Width, 10 + lblHost.Height / 7 * 0));
            lblPad_2.Location = new Point(new Size(40 + lblHost.Width, 10 + lblHost.Height / 7 * 2));
            lblPad_3.Location = new Point(new Size(40 + lblHost.Width, 10 + lblHost.Height / 7 * 4));
            lblPad_4.Location = new Point(new Size(40 + lblHost.Width, 10 + lblHost.Height / 7 * 6));

            btnUp_1.Location = new Point(lblPad_2.Location.X - 26, lblPad_2.Location.Y - 6);
            btnUp_2.Location = new Point(lblPad_3.Location.X - 26, lblPad_3.Location.Y - 6);
            btnUp_3.Location = new Point(lblPad_4.Location.X - 26, lblPad_4.Location.Y - 6);

            ClientSize = new Size(SizeX, SizeY);
        }

        public void Reset() 
        {
            CenterToScreen();
        }

        protected void Parse(Byte[] Buffer) 
        {
            if (!m_Connected)
            {
                m_Connected       = true;
                tmConfig.Enabled  = true;
                tmProfile.Enabled = true;

                niTray.BalloonTipText = "Server Connected";
                niTray.ShowBalloonTip(3000);
            }

            String   Data  = Encoding.Unicode.GetString(Buffer);
            String[] Split = Data.Split(m_Delim, StringSplitOptions.RemoveEmptyEntries);

            lblHost.Text  = Split[0];

            lblPad_1.Text = Split[1];
            lblPad_2.Text = Split[2]; btnUp_1.Enabled = !Split[2].Contains("Disconnected");
            lblPad_3.Text = Split[3]; btnUp_2.Enabled = !Split[3].Contains("Disconnected");
            lblPad_4.Text = Split[4]; btnUp_3.Enabled = !Split[4].Contains("Disconnected");
        }

        protected void Clear() 
        {
            if (m_Connected)
            {
                m_Connected       = false;
                tmConfig.Enabled  = false;
                tmProfile.Enabled = false;

                niTray.BalloonTipText = "Server Disconnected";
                niTray.ShowBalloonTip(3000);
            }

            if (Settings.Visible) Settings.Hide();
            if (Profiles.Visible) Profiles.Hide();

            lblHost.Text = "Host Address : Disconnected";

            lblPad_1.Text = "Pad 1 : Disconnected";
            lblPad_2.Text = "Pad 2 : Disconnected";
            lblPad_3.Text = "Pad 3 : Disconnected";
            lblPad_4.Text = "Pad 4 : Disconnected";

            btnUp_1.Enabled = btnUp_2.Enabled = btnUp_3.Enabled = false;
        }

        protected void tmrUpdate_Tick(object sender, EventArgs e) 
        {
            lock (this)
            {
                tmrUpdate.Enabled = false;

                try
                {
                    if (Visible && Location.X != -32000 && Location.Y != -32000)
                    {
                        FormVisible = true;

                        FormX = Location.X;
                        FormY = Location.Y;
                        FormSaved = true;
                    }
                    else
                    {
                        FormVisible = false;
                    }

                    if (Settings.Visible && Settings.Location.X != -32000 && Settings.Location.Y != -32000)
                    {
                        ConfX = Settings.Location.X;
                        ConfY = Settings.Location.Y;
                        ConfSaved = true;
                    }

                    if (Profiles.Visible && Profiles.Location.X != -32000 && Profiles.Location.Y != -32000)
                    {
                        ProfX = Profiles.Location.X;
                        ProfY = Profiles.Location.Y;
                        ProfSaved = true;
                    }

                    if (m_Server.Send(m_Buffer, m_Buffer.Length, m_ServerEp) == m_Buffer.Length)
                    {
                        IPEndPoint ReferenceEp = new IPEndPoint(IPAddress.Loopback, 0);

                        Byte[] Buffer = m_Server.Receive(ref ReferenceEp);

                        if (Buffer.Length > 0) Parse(Buffer);
                    }
                }
                catch
                {
                    Clear();
                }

                tmrUpdate.Enabled = true;
            }
        }

        protected void Form_Load(object sender, EventArgs e) 
        {
            Icon = niTray.Icon = Properties.Resources.Scp_All;

            tmrUpdate.Enabled = true;
        }

        protected void Form_Closing(object sender, FormClosingEventArgs e) 
        {
            if (e.CloseReason == CloseReason.UserClosing && niTray.Visible)
            {
                e.Cancel = true;

                if (Settings.Visible) Settings.Hide();
                if (Profiles.Visible) Profiles.Hide();

                Visible = false;
                WindowState = FormWindowState.Minimized;
            }
            else
            {
                tmrUpdate.Enabled = false;

                m_Config.Visible = FormVisible;

                m_Config.FormSaved = FormSaved;
                m_Config.FormX = FormX;
                m_Config.FormY = FormY;

                m_Config.ConfSaved = ConfSaved;
                m_Config.ConfX = ConfX;
                m_Config.ConfY = ConfY;

                m_Config.ProfSaved = ProfSaved;
                m_Config.ProfX = ProfX;
                m_Config.ProfY = ProfY;

                m_Config.Save();
            }
        }

        protected void btnUp_Click(object sender, EventArgs e) 
        {
            Byte[] Buffer = { 0, 5, (Byte)((Button)sender).Tag };

            m_Server.Send(Buffer, Buffer.Length, m_ServerEp);
        }

        protected void niTray_Click(object sender, MouseEventArgs e) 
        {
            if (e.Button == MouseButtons.Left)
            {
                if (WindowState == FormWindowState.Minimized)
                {
                    WindowState = FormWindowState.Normal;
                    Visible = true;

                    Activate();
                }
                else
                {
                    if (Settings.Visible) Settings.Hide();
                    if (Profiles.Visible) Profiles.Hide();

                    Visible = false;
                    WindowState = FormWindowState.Minimized;
                }
            }
        }

        protected void tmConfig_Click(object sender, EventArgs e) 
        {
            if (!Settings.Visible)
            {
                Settings.Request();
                Settings.Show(this);
            }

            Settings.Activate();
        }

        protected void tmProfile_Click(object sender, EventArgs e) 
        {
            if (!Profiles.Visible)
            {
                Profiles.Request();
                Profiles.Show(this);
            }

            Profiles.Activate();
        }

        protected void tmReset_Click(object sender, EventArgs e) 
        {
            lock (this)
            {
                tmrUpdate.Enabled = false;

                Reset();
                Settings.Reset();
                Profiles.Reset();

                tmrUpdate.Enabled = true;
            }
        }

        protected void tmExit_Click(object sender, EventArgs e) 
        {
            niTray.Visible = false;
            Close();
        }

        protected void Button_Enter(object sender, EventArgs e) 
        {
            ThemeUtil.UpdateFocus(((Button) sender).Handle);
        }
    }

    [SettingsProvider(typeof(ScpControl.RegistryProvider))]
    public class RegistrySettings : ApplicationSettingsBase 
    {
        [UserScopedSetting, DefaultSettingValue("true")]
        public Boolean Visible 
        {
            get { return (Boolean) this["Visible"]; }
            set { this["Visible"] = value; }
        }

        [UserScopedSetting, DefaultSettingValue("false")]
        public Boolean FormSaved 
        {
            get { return (Boolean) this["FormSaved"]; }
            set { this["FormSaved"] = value; }
        }

        [UserScopedSetting, DefaultSettingValue("false")]
        public Boolean ConfSaved 
        {
            get { return (Boolean) this["ConfSaved"]; }
            set { this["ConfSaved"] = value; }
        }

        [UserScopedSetting, DefaultSettingValue("false")]
        public Boolean ProfSaved 
        {
            get { return (Boolean)this["ProfSaved"]; }
            set { this["ProfSaved"] = value; }
        }

        [UserScopedSetting, DefaultSettingValue("-32000")]
        public Int32 FormX 
        {
            get { return (Int32) this["FormX"]; }
            set { this["FormX"] = value; }
        }

        [UserScopedSetting, DefaultSettingValue("-32000")]
        public Int32 FormY 
        {
            get { return (Int32)this["FormY"]; }
            set { this["FormY"] = value; }
        }

        [UserScopedSetting, DefaultSettingValue("-32000")]
        public Int32 ConfX 
        {
            get { return (Int32)this["ConfX"]; }
            set { this["ConfX"] = value; }
        }

        [UserScopedSetting, DefaultSettingValue("-32000")]
        public Int32 ConfY 
        {
            get { return (Int32)this["ConfY"]; }
            set { this["ConfY"] = value; }
        }

        [UserScopedSetting, DefaultSettingValue("-32000")]
        public Int32 ProfX 
        {
            get { return (Int32)this["ProfX"]; }
            set { this["ProfX"] = value; }
        }

        [UserScopedSetting, DefaultSettingValue("-32000")]
        public Int32 ProfY 
        {
            get { return (Int32)this["ProfY"]; }
            set { this["ProfY"] = value; }
        }
    }
}
