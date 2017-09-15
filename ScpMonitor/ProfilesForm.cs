using System;
using System.Linq;
using System.Windows.Forms;
using HidReport.Contract.Enums;
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

        private void Parse(object sender, ScpHidReport scpHidReport)
        {
            lock (this)
            {
                var e = scpHidReport.HidReport;
                if (scpHidReport.PadId == (DsPadId)m_SelectedPad)
                {
                    if (scpHidReport.PadState != DsState.Connected)
                    {
                        ResetControls();
                        return;
                    }

                    switch (scpHidReport.Model)
                    {
                        case DsModel.DS3:
                            {
                                axLX.Value = e[AxesEnum.Lx].Value;
                                axLY.Value = e[AxesEnum.Ly].Value;
                                axRX.Value = e[AxesEnum.Rx].Value;
                                axRY.Value = e[AxesEnum.Ry].Value;

                                axL1.Value = e[AxesEnum.L1].Value;
                                axR1.Value = e[AxesEnum.R1].Value;
                                axL2.Value = e[AxesEnum.L2].Value;
                                axR2.Value = e[AxesEnum.R2].Value;

                                axL3.Value = (Byte)(e[ButtonsEnum.L3].IsPressed ? 255 : 0);
                                axR3.Value = (Byte)(e[ButtonsEnum.R3].IsPressed ? 255 : 0);

                                axSH.Value = (Byte)(e[ButtonsEnum.Select].IsPressed ? 255 : 0);
                                axOP.Value = (Byte)(e[ButtonsEnum.Start].IsPressed ? 255 : 0);

                                axT.Value = e[AxesEnum.Triangle].Value;
                                axC.Value = e[AxesEnum.Circle].Value;
                                axX.Value = e[AxesEnum.Cross].Value;
                                axS.Value = e[AxesEnum.Square].Value;

                                axU.Value = e[AxesEnum.Up].Value;
                                axR.Value = e[AxesEnum.Right].Value;
                                axD.Value = e[AxesEnum.Down].Value;
                                axL.Value = e[AxesEnum.Left].Value;

                                axPS.Value = (Byte)(e[ButtonsEnum.Ps].IsPressed ? 255 : 0);
                            }
                            break;

                        case DsModel.DS4:
                            {
                                axLX.Value = e[AxesEnum.Lx].Value;
                                axLY.Value = e[AxesEnum.Ly].Value;
                                axRX.Value = e[AxesEnum.Rx].Value;
                                axRY.Value = e[AxesEnum.Ry].Value;

                                axL2.Value = e[AxesEnum.L2].Value;
                                axR2.Value = e[AxesEnum.R2].Value;

                                axL1.Value = (Byte)(e[ButtonsEnum.L1].IsPressed ? 255 : 0);
                                axR1.Value = (Byte)(e[ButtonsEnum.R1].IsPressed ? 255 : 0);
                                axL3.Value = (Byte)(e[ButtonsEnum.L3].IsPressed ? 255 : 0);
                                axR3.Value = (Byte)(e[ButtonsEnum.R3].IsPressed ? 255 : 0);

                                axSH.Value = (Byte)(e[ButtonsEnum.Share].IsPressed ? 255 : 0);
                                axOP.Value = (Byte)(e[ButtonsEnum.Options].IsPressed ? 255 : 0);

                                axT.Value = (Byte)(e[ButtonsEnum.Triangle].IsPressed ? 255 : 0);
                                axC.Value = (Byte)(e[ButtonsEnum.Circle].IsPressed ? 255 : 0);
                                axX.Value = (Byte)(e[ButtonsEnum.Cross].IsPressed ? 255 : 0);
                                axS.Value = (Byte)(e[ButtonsEnum.Square].IsPressed ? 255 : 0);

                                axU.Value = (Byte)(e[ButtonsEnum.Up].IsPressed ? 255 : 0);
                                axR.Value = (Byte)(e[ButtonsEnum.Right].IsPressed ? 255 : 0);
                                axD.Value = (Byte)(e[ButtonsEnum.Down].IsPressed ? 255 : 0);
                                axL.Value = (Byte)(e[ButtonsEnum.Left].IsPressed ? 255 : 0);

                                axPS.Value = (Byte)(e[ButtonsEnum.Ps].IsPressed ? 255 : 0);
                                axTP.Value = (Byte)(e[ButtonsEnum.Touchpad].IsPressed ? 255 : 0);
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