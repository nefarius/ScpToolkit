using System;
using System.Linq;
using System.Windows.Forms;
using ScpControl;
using ScpMonitor.Properties;

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
            if (scpProxy.Load())
            {
                m_SelectedProfile = m_Active = scpProxy.Active;

                cbProfile.Items.Clear();
                cbProfile.Items.AddRange(scpProxy.Mapper.Profiles);
                cbProfile.SelectedItem = m_SelectedProfile;

                cbPad.SelectedIndex = m_SelectedPad = 0;
                m_Detail = scpProxy.Detail((DsPadId)m_SelectedPad);

                ResetControls();
            }

            m_Editing = false;
            m_CanSave = true;

        }

        public void Reset()
        {
            CenterToScreen();
        }

        private void Parse(object sender, DsPacket e)
        {
            lock (this)
            {
                if (e.Detail.Pad == (DsPadId)m_SelectedPad)
                {
                    if (e.Detail.State != DsState.Connected)
                    {
                        ResetControls();
                        return;
                    }

                    scpProxy.Remap(m_SelectedProfile, e);

                    switch (e.Detail.Model)
                    {
                        case DsModel.DS3:
                            {
                                axLX.Value = e.Axis(Ds3Axis.LX);
                                axLY.Value = e.Axis(Ds3Axis.LY);
                                axRX.Value = e.Axis(Ds3Axis.RX);
                                axRY.Value = e.Axis(Ds3Axis.RY);

                                axL1.Value = e.Axis(Ds3Axis.L1);
                                axR1.Value = e.Axis(Ds3Axis.R1);
                                axL2.Value = e.Axis(Ds3Axis.L2);
                                axR2.Value = e.Axis(Ds3Axis.R2);

                                axL3.Value = (Byte)(e.Button(Ds3Button.L3) ? 255 : 0);
                                axR3.Value = (Byte)(e.Button(Ds3Button.R3) ? 255 : 0);

                                axSH.Value = (Byte)(e.Button(Ds3Button.Select) ? 255 : 0);
                                axOP.Value = (Byte)(e.Button(Ds3Button.Start) ? 255 : 0);

                                axT.Value = e.Axis(Ds3Axis.Triangle);
                                axC.Value = e.Axis(Ds3Axis.Circle);
                                axX.Value = e.Axis(Ds3Axis.Cross);
                                axS.Value = e.Axis(Ds3Axis.Square);

                                axU.Value = e.Axis(Ds3Axis.Up);
                                axR.Value = e.Axis(Ds3Axis.Right);
                                axD.Value = e.Axis(Ds3Axis.Down);
                                axL.Value = e.Axis(Ds3Axis.Left);

                                axPS.Value = (Byte)(e.Button(Ds3Button.PS) ? 255 : 0);
                            }
                            break;

                        case DsModel.DS4:
                            {
                                axLX.Value = e.Axis(Ds4Axis.LX);
                                axLY.Value = e.Axis(Ds4Axis.LY);
                                axRX.Value = e.Axis(Ds4Axis.RX);
                                axRY.Value = e.Axis(Ds4Axis.RY);

                                axL2.Value = e.Axis(Ds4Axis.L2);
                                axR2.Value = e.Axis(Ds4Axis.R2);

                                axL1.Value = (Byte)(e.Button(Ds4Button.L1) ? 255 : 0);
                                axR1.Value = (Byte)(e.Button(Ds4Button.R1) ? 255 : 0);
                                axL3.Value = (Byte)(e.Button(Ds4Button.L3) ? 255 : 0);
                                axR3.Value = (Byte)(e.Button(Ds4Button.R3) ? 255 : 0);

                                axSH.Value = (Byte)(e.Button(Ds4Button.Share) ? 255 : 0);
                                axOP.Value = (Byte)(e.Button(Ds4Button.Options) ? 255 : 0);

                                axT.Value = (Byte)(e.Button(Ds4Button.Triangle) ? 255 : 0);
                                axC.Value = (Byte)(e.Button(Ds4Button.Circle) ? 255 : 0);
                                axX.Value = (Byte)(e.Button(Ds4Button.Cross) ? 255 : 0);
                                axS.Value = (Byte)(e.Button(Ds4Button.Square) ? 255 : 0);

                                axU.Value = (Byte)(e.Button(Ds4Button.Up) ? 255 : 0);
                                axR.Value = (Byte)(e.Button(Ds4Button.Right) ? 255 : 0);
                                axD.Value = (Byte)(e.Button(Ds4Button.Down) ? 255 : 0);
                                axL.Value = (Byte)(e.Button(Ds4Button.Left) ? 255 : 0);

                                axPS.Value = (Byte)(e.Button(Ds4Button.PS) ? 255 : 0);
                                axTP.Value = (Byte)(e.Button(Ds4Button.TouchPad) ? 255 : 0);
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