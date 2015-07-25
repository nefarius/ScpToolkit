using System;
using System.Windows.Forms;
using System.Text;

using System.Net;
using System.Net.Sockets;

namespace ScpMonitor 
{
    public partial class SettingsForm : Form 
    {
        protected Char[] m_Delim = new Char[] { '^' };

        protected IPEndPoint m_ServerEp = new IPEndPoint(IPAddress.Loopback, 26760);
        protected UdpClient  m_Server   = new UdpClient();

        protected Byte[] m_Buffer = new Byte[17];

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
                    IPEndPoint ReferenceEp = new IPEndPoint(IPAddress.Loopback, 0);

                    Byte[] Buffer = m_Server.Receive(ref ReferenceEp);

                    tbIdle.Value       = Buffer[ 2];
                    cbLX.Checked       = Buffer[ 3] == 1;
                    cbLY.Checked       = Buffer[ 4] == 1;
                    cbRX.Checked       = Buffer[ 5] == 1;
                    cbRY.Checked       = Buffer[ 6] == 1;
                    cbLED.Checked      = Buffer[ 7] == 1;
                    cbRumble.Checked   = Buffer[ 8] == 1;
                    cbTriggers.Checked = Buffer[ 9] == 1;
                    tbLatency.Value    = Buffer[10];
                    tbLeft.Value       = Buffer[11];
                    tbRight.Value      = Buffer[12];
                    cbNative.Checked   = Buffer[13] == 1;
                    cbSSP.Checked      = Buffer[14] == 1;
                    tbBrightness.Value = Buffer[15];
                    cbForce.Checked    = Buffer[16] == 1;
                }
            }
            catch { }
        }

        protected void Form_Load(object sender, EventArgs e) 
        {
            Icon = Properties.Resources.Scp_All;
        }

        protected void Form_Closing(object sender, FormClosingEventArgs e) 
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true; Hide();
            }
        }

        protected void btnOK_Click(object sender, EventArgs e) 
        {
            m_Buffer[ 1] = 0x04;
            m_Buffer[ 2] = (Byte) tbIdle.Value;
            m_Buffer[ 3] = (Byte)(cbLX.Checked       ? 0x01 : 0x00);
            m_Buffer[ 4] = (Byte)(cbLY.Checked       ? 0x01 : 0x00);
            m_Buffer[ 5] = (Byte)(cbRX.Checked       ? 0x01 : 0x00);
            m_Buffer[ 6] = (Byte)(cbRY.Checked       ? 0x01 : 0x00);
            m_Buffer[ 7] = (Byte)(cbLED.Checked      ? 0x01 : 0x00);
            m_Buffer[ 8] = (Byte)(cbRumble.Checked   ? 0x01 : 0x00);
            m_Buffer[ 9] = (Byte)(cbTriggers.Checked ? 0x01 : 0x00);
            m_Buffer[10] = (Byte) tbLatency.Value;
            m_Buffer[11] = (Byte) tbLeft.Value;
            m_Buffer[12] = (Byte) tbRight.Value;
            m_Buffer[13] = (Byte)(cbNative.Checked   ? 0x01 : 0x00);
            m_Buffer[14] = (Byte)(cbSSP.Checked      ? 0x01 : 0x00);
            m_Buffer[15] = (Byte) tbBrightness.Value;
            m_Buffer[16] = (Byte)(cbForce.Checked    ? 0x01 : 0x00);

            m_Server.Send(m_Buffer, m_Buffer.Length, m_ServerEp);
            Hide();
        }

        protected void btnCancel_Click(object sender, EventArgs e) 
        {
            Hide();
        }

        protected void tbIdle_ValueChanged(object sender, EventArgs e) 
        {
            Int32 Value = tbIdle.Value;

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
                lblIdle.Text = String.Format("Idle Timeout : {0} minutes", Value);
            }
        }

        protected void tbLatency_ValueChanged(object sender, EventArgs e) 
        {
            Int32 Value = tbLatency.Value << 4;

            lblLatency.Text = String.Format("DS3 Rumble Latency : {0} ms", Value);
        }

        protected void tbBrightness_ValueChanged(object sender, EventArgs e) 
        {
            Int32 Value = tbBrightness.Value;

            if (Value == 0)
            {
                lblBrightness.Text = String.Format("DS4 Light Bar Brighness : Disabled", Value);
            }
            else
            {
                lblBrightness.Text = String.Format("DS4 Light Bar Brighness : {0}", Value);
            }
        }
    }
}
