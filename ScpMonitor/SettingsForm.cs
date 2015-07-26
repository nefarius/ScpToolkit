using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Windows.Forms;
using log4net;
using ScpMonitor.Properties;

namespace ScpMonitor
{
    public partial class SettingsForm : Form
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly byte[] m_Buffer = new byte[17];
        private readonly UdpClient m_Server = new UdpClient();
        private readonly IPEndPoint m_ServerEp = new IPEndPoint(IPAddress.Loopback, 26760);

        public SettingsForm()
        {
            InitializeComponent();

            m_Server.Client.ReceiveTimeout = 250;

            ttSSP.SetToolTip(cbSSP, @"Requires Service Restart");
        }

        public void Reset()
        {
            CenterToScreen();
        }

        public void Request()
        {
            try
            {
                m_Buffer[1] = 0x03;

                if (m_Server.Send(m_Buffer, m_Buffer.Length, m_ServerEp) == m_Buffer.Length)
                {
                    var ReferenceEp = new IPEndPoint(IPAddress.Loopback, 0);

                    var Buffer = m_Server.Receive(ref ReferenceEp);

                    tbIdle.Value = Buffer[2];
                    cbLX.Checked = Buffer[3] == 1;
                    cbLY.Checked = Buffer[4] == 1;
                    cbRX.Checked = Buffer[5] == 1;
                    cbRY.Checked = Buffer[6] == 1;
                    cbLED.Checked = Buffer[7] == 1;
                    cbRumble.Checked = Buffer[8] == 1;
                    cbTriggers.Checked = Buffer[9] == 1;
                    tbLatency.Value = Buffer[10];
                    tbLeft.Value = Buffer[11];
                    tbRight.Value = Buffer[12];
                    cbNative.Checked = Buffer[13] == 1;
                    cbSSP.Checked = Buffer[14] == 1;
                    tbBrightness.Value = Buffer[15];
                    cbForce.Checked = Buffer[16] == 1;
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error: {0}", ex);
            }
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
            m_Buffer[1] = 0x04;
            m_Buffer[2] = (byte) tbIdle.Value;
            m_Buffer[3] = (byte) (cbLX.Checked ? 0x01 : 0x00);
            m_Buffer[4] = (byte) (cbLY.Checked ? 0x01 : 0x00);
            m_Buffer[5] = (byte) (cbRX.Checked ? 0x01 : 0x00);
            m_Buffer[6] = (byte) (cbRY.Checked ? 0x01 : 0x00);
            m_Buffer[7] = (byte) (cbLED.Checked ? 0x01 : 0x00);
            m_Buffer[8] = (byte) (cbRumble.Checked ? 0x01 : 0x00);
            m_Buffer[9] = (byte) (cbTriggers.Checked ? 0x01 : 0x00);
            m_Buffer[10] = (byte) tbLatency.Value;
            m_Buffer[11] = (byte) tbLeft.Value;
            m_Buffer[12] = (byte) tbRight.Value;
            m_Buffer[13] = (byte) (cbNative.Checked ? 0x01 : 0x00);
            m_Buffer[14] = (byte) (cbSSP.Checked ? 0x01 : 0x00);
            m_Buffer[15] = (byte) tbBrightness.Value;
            m_Buffer[16] = (byte) (cbForce.Checked ? 0x01 : 0x00);

            m_Server.Send(m_Buffer, m_Buffer.Length, m_ServerEp);
            Hide();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void tbIdle_ValueChanged(object sender, EventArgs e)
        {
            var Value = tbIdle.Value;

            if (Value == 0)
            {
                lblIdle.Text = "Idle Timeout : Disabled";
            }
            else if (Value == 1)
            {
                lblIdle.Text = "Idle Timeout : 1 minute";
            }
            else
            {
                lblIdle.Text = string.Format("Idle Timeout : {0} minutes", Value);
            }
        }

        private void tbLatency_ValueChanged(object sender, EventArgs e)
        {
            var Value = tbLatency.Value << 4;

            lblLatency.Text = string.Format("DS3 Rumble Latency : {0} ms", Value);
        }

        private void tbBrightness_ValueChanged(object sender, EventArgs e)
        {
            var Value = tbBrightness.Value;

            if (Value == 0)
            {
                lblBrightness.Text = string.Format("DS4 Light Bar Brighness : Disabled", Value);
            }
            else
            {
                lblBrightness.Text = string.Format("DS4 Light Bar Brighness : {0}", Value);
            }
        }
    }
}