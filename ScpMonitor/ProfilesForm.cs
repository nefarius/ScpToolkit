using System;
using System.Linq;
using System.Windows.Forms;
using ScpControl.Profiler;
using ScpControl.ScpCore;
using ScpMonitor.Properties;
using Ds3Axis = ScpControl.Profiler.Ds3Axis;
using Ds3Button = ScpControl.Profiler.Ds3Button;
using Ds4Axis = ScpControl.Profiler.Ds4Axis;
using Ds4Button = ScpControl.Profiler.Ds4Button;

namespace ScpMonitor
{
    public partial class ProfilesForm : Form
    {
        private const String Default = "Default";
        protected Boolean m_CanEdit, m_Editing, m_CanSave = true, m_PropsActive;
        protected DsDetail m_Detail;
        protected ProfileProperties m_PropForm;
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
            m_SelectedProfile = m_Active = scpProxy.ActiveProfile;

            cbProfile.Items.Clear();
            cbProfile.Items.AddRange(scpProxy.Mapper.Profiles);
            cbProfile.SelectedItem = m_SelectedProfile;

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

                    //scpProxy.Remap(m_SelectedProfile, e);

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

                if (m_PropsActive) m_PropForm.Close();
            }
        }

        private void Profile_Selected(object sender, EventArgs e)
        {
            lock (this)
            {
                m_SelectedProfile = cbProfile.SelectedItem.ToString();

                if (m_PropsActive)
                {
                    btnAdd.Enabled = btnDel.Enabled = btnEdit.Enabled = btnView.Enabled = false;
                }
                else
                {
                    btnView.Enabled = btnAdd.Enabled = true;
                    btnEdit.Enabled = btnDel.Enabled = m_SelectedProfile != Default;
                }
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

        private void btnActivate_Click(object sender, EventArgs e)
        {
            scpProxy.Select(scpProxy.Mapper.Map[m_SelectedProfile]);
            m_Active = m_SelectedProfile;

            btnDel.Enabled = btnEdit.Enabled = m_CanEdit = false;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            scpProxy.Save();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var Profile = new Profile("<New Profile>");

            m_Detail = scpProxy.Detail((DsPadId)m_SelectedPad);
            m_PropForm = new ProfileProperties(Profile, m_Detail.Pad, m_Detail.Local, true);

            m_PropForm.FormClosed += Props_Close;
            m_PropForm.VisibleChanged += Props_Visible;

            m_PropForm.Show(this);
        }

        private void btnDel_Click(object sender, EventArgs e)
        {
            scpProxy.Mapper.Map.Remove(m_SelectedProfile);

            var Index = cbProfile.SelectedIndex;

            cbProfile.Items.Remove(m_SelectedProfile);

            while (Index >= cbProfile.Items.Count) Index--;

            cbProfile.SelectedIndex = Index;
        }

        private void btnView_Click(object sender, EventArgs e)
        {
            m_Detail = scpProxy.Detail((DsPadId)m_SelectedPad);
            m_PropForm = new ProfileProperties(scpProxy.Mapper.Map[m_SelectedProfile], m_Detail.Pad, m_Detail.Local,
                false);

            m_PropForm.FormClosed += Props_Close;
            m_PropForm.VisibleChanged += Props_Visible;

            m_PropForm.Show(this);
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            m_Detail = scpProxy.Detail((DsPadId)m_SelectedPad);
            m_PropForm = new ProfileProperties(scpProxy.Mapper.Map[m_SelectedProfile], m_Detail.Pad, m_Detail.Local,
                true);

            m_PropForm.FormClosed += Props_Close;
            m_PropForm.VisibleChanged += Props_Visible;

            m_PropForm.Show(this);
        }

        private void Props_Close(object sender, FormClosedEventArgs e)
        {
            if (m_PropForm.Saved)
            {
                var Profile = m_PropForm.Profile;

                if (!scpProxy.Mapper.Map.ContainsKey(Profile.Name))
                {
                    cbProfile.Items.Add(Profile.Name);
                }

                scpProxy.Mapper.Map[Profile.Name] = Profile;
                cbProfile.SelectedItem = Profile.Name;
            }

            m_PropsActive = false;

            btnView.Enabled = btnAdd.Enabled = true;
            btnEdit.Enabled = btnDel.Enabled = m_SelectedProfile != Default;
        }

        private void Props_Visible(object sender, EventArgs e)
        {
            m_PropsActive = m_PropForm.Visible;

            if (m_PropsActive)
            {
                btnAdd.Enabled = btnDel.Enabled = btnEdit.Enabled = btnView.Enabled = false;
            }
            else
            {
                btnView.Enabled = btnAdd.Enabled = true;
                btnEdit.Enabled = btnDel.Enabled = m_SelectedProfile != Default;
            }
        }
    }
}