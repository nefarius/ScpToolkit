using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Reflection;
using System.ServiceModel;
using System.Windows.Forms;
using log4net;
using ScpControl;
using ScpControl.Utilities;
using ScpMonitor.Properties;

namespace ScpMonitor
{
    public partial class ScpForm : Form
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        protected bool FormSaved, ConfSaved, ProfSaved, FormVisible;
        protected int FormX, FormY, ConfX, ConfY, ProfX, ProfY;
        private bool m_Connected;
        private readonly ProfilesForm _profiles = new ProfilesForm();
        private readonly RegistrySettings m_Config = new RegistrySettings();

        public ScpForm()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Log.FatalFormat("Unhandled exception: {0}", args.ExceptionObject);
            };

            InitializeComponent();

            btnUp_1.Tag = (byte)1;
            btnUp_2.Tag = (byte)2;
            btnUp_3.Tag = (byte)3;

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
                Location = new Point(FormX, FormY);
            }

            if (!FormVisible)
            {
                WindowState = FormWindowState.Minimized;
                Visible = false;
            }

            if (ProfSaved)
            {
                _profiles.StartPosition = FormStartPosition.Manual;
                _profiles.Location = new Point(ProfX, ProfY);
            }

            lblHost.Text = "Host Address : 00:00:00:00:00:00\r\n\r\n0\r\n\r\n0\r\n\r\n0";
            lblPad_1.Text = "Pad 1 : DS3 00:00:00:00:00:00 - Usb FFFFFFFF Charging";

            lblPad_1.Location = new Point(new Size(40 + lblHost.Width, 10 + lblHost.Height / 7 * 0));
            lblPad_2.Location = new Point(new Size(40 + lblHost.Width, 10 + lblHost.Height / 7 * 2));
            lblPad_3.Location = new Point(new Size(40 + lblHost.Width, 10 + lblHost.Height / 7 * 4));
            lblPad_4.Location = new Point(new Size(40 + lblHost.Width, 10 + lblHost.Height / 7 * 6));

            btnUp_1.Location = new Point(lblPad_2.Location.X - 26, lblPad_2.Location.Y - 6);
            btnUp_2.Location = new Point(lblPad_3.Location.X - 26, lblPad_3.Location.Y - 6);
            btnUp_3.Location = new Point(lblPad_4.Location.X - 26, lblPad_4.Location.Y - 6);
        }

        public void Reset()
        {
            CenterToScreen();
        }

        /// <summary>
        ///     Root hub disconnected, reset user interface to defaults.
        /// </summary>
        private void Clear()
        {
            if (m_Connected)
            {
                m_Connected = false;
                tmProfile.Enabled = false;

                niTray.BalloonTipText = "Server Disconnected";
                niTray.ShowBalloonTip(3000);
            }

            if (_profiles.Visible) _profiles.Hide();

            lblHost.Text = "Host Address : Disconnected";

            lblPad_1.Text = "Pad 1 : Disconnected";
            lblPad_2.Text = "Pad 2 : Disconnected";
            lblPad_3.Text = "Pad 3 : Disconnected";
            lblPad_4.Text = "Pad 4 : Disconnected";

            btnUp_1.Enabled = btnUp_2.Enabled = btnUp_3.Enabled = false;
        }

        private void Form_Load(object sender, EventArgs e)
        {
            Icon = niTray.Icon = Resources.Scp_All;

            if (!scpProxy.Start())
            {
                MessageBox.Show("Couldn't connect to server, please check if the service is running!",
                    "Fatal error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

            tmrUpdate.Enabled = !tmrUpdate.Enabled;
        }

        private void Form_Closing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && niTray.Visible)
            {
                e.Cancel = true;

                if (_profiles.Visible) _profiles.Hide();

                Visible = false;
                WindowState = FormWindowState.Minimized;
            }
            else
            {
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

                scpProxy.Stop();
            }
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            scpProxy.PromotePad((byte)((Button)sender).Tag);
        }

        private void niTray_Click(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            if (WindowState == FormWindowState.Minimized)
            {
                WindowState = FormWindowState.Normal;
                Visible = true;

                Activate();
            }
            else
            {

                if (_profiles.Visible) _profiles.Hide();

                Visible = false;
                WindowState = FormWindowState.Minimized;
            }
        }

        private void tmProfile_Click(object sender, EventArgs e)
        {
            if (!_profiles.Visible)
            {
                _profiles.Request();
                _profiles.Show(this);
            }

            _profiles.Activate();
        }

        private void tmReset_Click(object sender, EventArgs e)
        {
            Reset();
            _profiles.Reset();
        }

        private void tmExit_Click(object sender, EventArgs e)
        {
            niTray.Visible = false;
            Close();
        }

        private void Button_Enter(object sender, EventArgs e)
        {
            ThemeUtil.UpdateFocus(((Button)sender).Handle);
        }

        private void tmrUpdate_Tick(object sender, EventArgs e)
        {
            tmrUpdate.Enabled = !tmrUpdate.Enabled;

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

            if (_profiles.Visible && _profiles.Location.X != -32000 && _profiles.Location.Y != -32000)
            {
                ProfX = _profiles.Location.X;
                ProfY = _profiles.Location.Y;
                ProfSaved = true;
            }

            IList<string> data;
            try
            {
                data = scpProxy.StatusData;
            }
            catch (CommunicationException) { return; }

            if (data == null)
                return;

            if (!m_Connected)
            {
                m_Connected = true;
                tmProfile.Enabled = true;

                niTray.BalloonTipText = "Server Connected";
                niTray.ShowBalloonTip(3000);
            }

            lblHost.Text = data[0];

            lblPad_1.Text = data[1];
            lblPad_2.Text = data[2];
            btnUp_1.Enabled = !data[2].Contains("Disconnected");
            lblPad_3.Text = data[3];
            btnUp_2.Enabled = !data[3].Contains("Disconnected");
            lblPad_4.Text = data[4];
            btnUp_3.Enabled = !data[4].Contains("Disconnected");

            tmrUpdate.Enabled = !tmrUpdate.Enabled;
        }

        private void scpProxy_RootHubDisconnected(object sender, EventArgs e)
        {
            this.UiThread(() => tmrUpdate.Enabled = !tmrUpdate.Enabled);

            this.UiThread(Clear);
        }
    }

    #region Registry settings

    [SettingsProvider(typeof(RegistryProvider))]
    public class RegistrySettings : ApplicationSettingsBase
    {
        [UserScopedSetting, DefaultSettingValue("true")]
        public bool Visible
        {
            get { return (bool)this["Visible"]; }
            set { this["Visible"] = value; }
        }

        [UserScopedSetting, DefaultSettingValue("false")]
        public bool FormSaved
        {
            get { return (bool)this["FormSaved"]; }
            set { this["FormSaved"] = value; }
        }

        [UserScopedSetting, DefaultSettingValue("false")]
        public bool ConfSaved
        {
            get { return (bool)this["ConfSaved"]; }
            set { this["ConfSaved"] = value; }
        }

        [UserScopedSetting, DefaultSettingValue("false")]
        public bool ProfSaved
        {
            get { return (bool)this["ProfSaved"]; }
            set { this["ProfSaved"] = value; }
        }

        [UserScopedSetting, DefaultSettingValue("-32000")]
        public int FormX
        {
            get { return (int)this["FormX"]; }
            set { this["FormX"] = value; }
        }

        [UserScopedSetting, DefaultSettingValue("-32000")]
        public int FormY
        {
            get { return (int)this["FormY"]; }
            set { this["FormY"] = value; }
        }

        [UserScopedSetting, DefaultSettingValue("-32000")]
        public int ConfX
        {
            get { return (int)this["ConfX"]; }
            set { this["ConfX"] = value; }
        }

        [UserScopedSetting, DefaultSettingValue("-32000")]
        public int ConfY
        {
            get { return (int)this["ConfY"]; }
            set { this["ConfY"] = value; }
        }

        [UserScopedSetting, DefaultSettingValue("-32000")]
        public int ProfX
        {
            get { return (int)this["ProfX"]; }
            set { this["ProfX"] = value; }
        }

        [UserScopedSetting, DefaultSettingValue("-32000")]
        public int ProfY
        {
            get { return (int)this["ProfY"]; }
            set { this["ProfY"] = value; }
        }
    }

    #endregion
}