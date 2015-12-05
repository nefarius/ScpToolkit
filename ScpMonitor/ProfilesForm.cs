using System;
using System.Linq;
using System.Windows.Forms;
using ScpControl.Shared.Core;
using ScpMonitor.Properties;

namespace ScpMonitor
{
    public partial class ProfilesForm : Form
    {
        private const String Default = "Default";
        protected Boolean m_CanEdit, m_Editing, m_CanSave = true, m_PropsActive;
        protected DualShockPadMeta m_Detail;
        protected Int32 m_SelectedPad;
        protected String m_SelectedProfile = Default, m_Active = Default;

        public ProfilesForm()
        {
            InitializeComponent();

            scpProxy.Start();
        }

        private void ResetControls()
        {
            foreach (var child in Controls.OfType<AxisControl>())
            {
                child.Value = 0;
            }
        }

        public void Request()
        {
            cbPad.SelectedIndex = m_SelectedPad = 0;
            m_Detail = scpProxy.Detail((DsPadId)m_SelectedPad);

            ResetControls();
            
            m_Editing = false;
            m_CanSave = true;

        }

        public void Reset()
        {
            CenterToScreen();
        }

        private void Parse(object sender, ScpHidReport e)
        {
            lock (this)
            {
                if (e.PadId == (DsPadId)m_SelectedPad)
                {
                    if (e.PadState != DsState.Connected)
                    {
                        ResetControls();
                        return;
                    }

                    switch (e.Model)
                    {
                        case DsModel.DS3:
                            {
                                axLX.Value = e[Ds3Axis.Lx].Value;
                                axLY.Value = e[Ds3Axis.Ly].Value;
                                axRX.Value = e[Ds3Axis.Rx].Value;
                                axRY.Value = e[Ds3Axis.Ry].Value;

                                axL1.Value = e[Ds3Axis.L1].Value;
                                axR1.Value = e[Ds3Axis.R1].Value;
                                axL2.Value = e[Ds3Axis.L2].Value;
                                axR2.Value = e[Ds3Axis.R2].Value;

                                axL3.Value = (Byte)(e[Ds3Button.L3].IsPressed ? 255 : 0);
                                axR3.Value = (Byte)(e[Ds3Button.R3].IsPressed ? 255 : 0);

                                axSH.Value = (Byte)(e[Ds3Button.Select].IsPressed ? 255 : 0);
                                axOP.Value = (Byte)(e[Ds3Button.Start].IsPressed ? 255 : 0);

                                axT.Value = e[Ds3Axis.Triangle].Value;
                                axC.Value = e[Ds3Axis.Circle].Value;
                                axX.Value = e[Ds3Axis.Cross].Value;
                                axS.Value = e[Ds3Axis.Square].Value;

                                axU.Value = e[Ds3Axis.Up].Value;
                                axR.Value = e[Ds3Axis.Right].Value;
                                axD.Value = e[Ds3Axis.Down].Value;
                                axL.Value = e[Ds3Axis.Left].Value;

                                axPS.Value = (Byte)(e[Ds3Button.Ps].IsPressed ? 255 : 0);
                            }
                            break;

                        case DsModel.DS4:
                            {
                                axLX.Value = e[Ds4Axis.Lx].Value;
                                axLY.Value = e[Ds4Axis.Ly].Value;
                                axRX.Value = e[Ds4Axis.Rx].Value;
                                axRY.Value = e[Ds4Axis.Ry].Value;

                                axL2.Value = e[Ds4Axis.L2].Value;
                                axR2.Value = e[Ds4Axis.R2].Value;

                                axL1.Value = (Byte)(e[Ds4Button.L1].IsPressed ? 255 : 0);
                                axR1.Value = (Byte)(e[Ds4Button.R1].IsPressed ? 255 : 0);
                                axL3.Value = (Byte)(e[Ds4Button.L3].IsPressed ? 255 : 0);
                                axR3.Value = (Byte)(e[Ds4Button.R3].IsPressed ? 255 : 0);

                                axSH.Value = (Byte)(e[Ds4Button.Share].IsPressed ? 255 : 0);
                                axOP.Value = (Byte)(e[Ds4Button.Options].IsPressed ? 255 : 0);

                                axT.Value = (Byte)(e[Ds4Button.Triangle].IsPressed ? 255 : 0);
                                axC.Value = (Byte)(e[Ds4Button.Circle].IsPressed ? 255 : 0);
                                axX.Value = (Byte)(e[Ds4Button.Cross].IsPressed ? 255 : 0);
                                axS.Value = (Byte)(e[Ds4Button.Square].IsPressed ? 255 : 0);

                                axU.Value = (Byte)(e[Ds4Button.Up].IsPressed ? 255 : 0);
                                axR.Value = (Byte)(e[Ds4Button.Right].IsPressed ? 255 : 0);
                                axD.Value = (Byte)(e[Ds4Button.Down].IsPressed ? 255 : 0);
                                axL.Value = (Byte)(e[Ds4Button.Left].IsPressed ? 255 : 0);

                                axPS.Value = (Byte)(e[Ds4Button.Ps].IsPressed ? 255 : 0);
                                axTP.Value = (Byte)(e[Ds4Button.TouchPad].IsPressed ? 255 : 0);
                            }
                            break;
                    }
                }
            }
        }

        private void Form_Load(object sender, EventArgs e)
        {
            Icon = Resources.Scp_All;
        }

        private void Form_Close(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void Form_Visible(object sender, EventArgs e)
        {
            if (!Visible)
            {
                scpProxy.Stop();
            }
        }

        private void Pad_Selected(object sender, EventArgs e)
        {
            lock (this)
            {
                m_SelectedPad = cbPad.SelectedIndex;
                m_Detail = scpProxy.Detail((DsPadId)m_SelectedPad);

                ResetControls();
            }
        }
    }
}