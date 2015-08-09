using System;
using System.Reflection;
using System.Windows.Forms;
using log4net;
using ScpControl;
using ScpControl.ScpCore;
using ScpMonitor.Properties;

namespace ScpMonitor
{
    public partial class SettingsForm : Form
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private GlobalConfiguration _config;
        private readonly ScpProxy _proxy;

        public SettingsForm(ScpProxy proxy)
        {
            _proxy = proxy;

            InitializeComponent();
        }

        public void Reset()
        {
            CenterToScreen();
        }

        public void Request()
        {
            _config = _proxy.ReadConfig();

            tbIdle.Value = _config.IdleTimeout/GlobalConfiguration.IdleTimeoutMultiplier;
            cbLX.Checked = _config.FlipLX;
            cbLY.Checked = _config.FlipLY;
            cbRX.Checked = _config.FlipRX;
            cbRY.Checked = _config.FlipRY;
            cbLED.Checked = _config.DisableLED;
            cbRumble.Checked = _config.DisableRumble;
            cbTriggers.Checked = _config.SwapTriggers;
            tbLatency.Value = _config.Latency/GlobalConfiguration.LatencyMultiplier;
            tbLeft.Value = _config.DeadZoneL;
            tbRight.Value = _config.DeadZoneR;
            cbNative.Checked = _config.DisableNative;
            cbSSP.Checked = _config.DisableSSP;
            tbBrightness.Value = _config.Brightness;
            cbForce.Checked = _config.Repair;
        }

        private void Form_Load(object sender, EventArgs e)
        {
            Icon = Resources.Scp_All;
        }

        private void Form_Closing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            _config.IdleTimeout = tbIdle.Value*GlobalConfiguration.IdleTimeoutMultiplier;
            _config.FlipLX = cbLX.Checked;
            _config.FlipLY = cbLY.Checked;
            _config.FlipRX = cbRX.Checked;
            _config.FlipRY = cbRY.Checked;
            _config.DisableLED = cbLED.Checked;
            _config.DisableRumble = cbRumble.Checked;
            _config.SwapTriggers = cbTriggers.Checked;
            _config.Latency = tbLatency.Value*GlobalConfiguration.LatencyMultiplier;
            _config.DeadZoneL = (byte) tbLeft.Value;
            _config.DeadZoneR = (byte) tbRight.Value;
            _config.DisableNative = cbNative.Checked;
            _config.DisableSSP = cbSSP.Checked;
            _config.Brightness = (byte) tbBrightness.Value;
            _config.Repair = cbForce.Checked;

            _proxy.WriteConfig(_config);

            Log.Info("Saved configuration");

            Hide();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void tbIdle_ValueChanged(object sender, EventArgs e)
        {
            var value = tbIdle.Value;

            if (value == 0)
            {
                lblIdle.Text = "Idle Timeout : Disabled";
            }
            else if (value == 1)
            {
                lblIdle.Text = "Idle Timeout : 1 minute";
            }
            else
            {
                lblIdle.Text = string.Format("Idle Timeout : {0} minutes", value);
            }
        }

        private void tbLatency_ValueChanged(object sender, EventArgs e)
        {
            var value = tbLatency.Value << 4;

            lblLatency.Text = string.Format("DS3 Rumble Latency : {0} ms", value);
        }

        private void tbBrightness_ValueChanged(object sender, EventArgs e)
        {
            var value = tbBrightness.Value;

            lblBrightness.Text = value == 0
                ? string.Format("DS4 Light Bar Brightness : Disabled")
                : string.Format("DS4 Light Bar Brightness : {0}%", ((value*100)/255));
        }
    }
}