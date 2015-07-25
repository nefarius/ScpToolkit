namespace ScpMonitor
{
    partial class ProfilesForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProfilesForm));
            this.tbOutput = new System.Windows.Forms.TextBox();
            this.msAxis = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.dS3ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.l1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.l2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.l1ToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.l2ToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.r1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.r2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lXToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.lYToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.rXToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.rYToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.dS4ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.noneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.l2ToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.r3ToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.lXToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lYToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rXToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rYToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ttAxBar = new System.Windows.Forms.ToolTip(this.components);
            this.btnView = new ScpMonitor.ScpButton();
            this.btnDel = new ScpMonitor.ScpButton();
            this.btnAdd = new ScpMonitor.ScpButton();
            this.btnEdit = new ScpMonitor.ScpButton();
            this.btnActivate = new ScpMonitor.ScpButton();
            this.btnSave = new ScpMonitor.ScpButton();
            this.axRY = new ScpMonitor.AxisControl();
            this.axRX = new ScpMonitor.AxisControl();
            this.cbProfile = new System.Windows.Forms.ComboBox();
            this.cbPad = new System.Windows.Forms.ComboBox();
            this.scpProxy = new ScpControl.ScpProxy(this.components);
            this.axL = new ScpMonitor.AxisControl();
            this.axD = new ScpMonitor.AxisControl();
            this.axR = new ScpMonitor.AxisControl();
            this.axU = new ScpMonitor.AxisControl();
            this.axTP = new ScpMonitor.AxisControl();
            this.axPS = new ScpMonitor.AxisControl();
            this.axOP = new ScpMonitor.AxisControl();
            this.axSH = new ScpMonitor.AxisControl();
            this.axS = new ScpMonitor.AxisControl();
            this.axX = new ScpMonitor.AxisControl();
            this.axC = new ScpMonitor.AxisControl();
            this.axT = new ScpMonitor.AxisControl();
            this.axR3 = new ScpMonitor.AxisControl();
            this.axL3 = new ScpMonitor.AxisControl();
            this.axR1 = new ScpMonitor.AxisControl();
            this.axL1 = new ScpMonitor.AxisControl();
            this.axR2 = new ScpMonitor.AxisControl();
            this.axL2 = new ScpMonitor.AxisControl();
            this.axLY = new ScpMonitor.AxisControl();
            this.axLX = new ScpMonitor.AxisControl();
            this.msAxis.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbOutput
            // 
            this.tbOutput.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.tbOutput.Location = new System.Drawing.Point(22, 439);
            this.tbOutput.Name = "tbOutput";
            this.tbOutput.ReadOnly = true;
            this.tbOutput.Size = new System.Drawing.Size(417, 20);
            this.tbOutput.TabIndex = 15;
            this.ttAxBar.SetToolTip(this.tbOutput, "Debug");
            this.tbOutput.Visible = false;
            // 
            // msAxis
            // 
            this.msAxis.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dS3ToolStripMenuItem,
            this.dS4ToolStripMenuItem});
            this.msAxis.Name = "msAxis";
            this.msAxis.Size = new System.Drawing.Size(95, 48);
            // 
            // dS3ToolStripMenuItem
            // 
            this.dS3ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.l1ToolStripMenuItem,
            this.l2ToolStripMenuItem,
            this.l1ToolStripMenuItem1,
            this.l2ToolStripMenuItem1,
            this.r1ToolStripMenuItem,
            this.r2ToolStripMenuItem,
            this.lXToolStripMenuItem1,
            this.lYToolStripMenuItem1,
            this.rXToolStripMenuItem1,
            this.rYToolStripMenuItem1});
            this.dS3ToolStripMenuItem.Name = "dS3ToolStripMenuItem";
            this.dS3ToolStripMenuItem.Size = new System.Drawing.Size(94, 22);
            this.dS3ToolStripMenuItem.Text = "DS3";
            // 
            // l1ToolStripMenuItem
            // 
            this.l1ToolStripMenuItem.Name = "l1ToolStripMenuItem";
            this.l1ToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.l1ToolStripMenuItem.Text = "Reset";
            // 
            // l2ToolStripMenuItem
            // 
            this.l2ToolStripMenuItem.Name = "l2ToolStripMenuItem";
            this.l2ToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.l2ToolStripMenuItem.Text = "None";
            // 
            // l1ToolStripMenuItem1
            // 
            this.l1ToolStripMenuItem1.Name = "l1ToolStripMenuItem1";
            this.l1ToolStripMenuItem1.Size = new System.Drawing.Size(103, 22);
            this.l1ToolStripMenuItem1.Text = "L1";
            // 
            // l2ToolStripMenuItem1
            // 
            this.l2ToolStripMenuItem1.Name = "l2ToolStripMenuItem1";
            this.l2ToolStripMenuItem1.Size = new System.Drawing.Size(103, 22);
            this.l2ToolStripMenuItem1.Text = "L2";
            // 
            // r1ToolStripMenuItem
            // 
            this.r1ToolStripMenuItem.Name = "r1ToolStripMenuItem";
            this.r1ToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.r1ToolStripMenuItem.Text = "R1";
            // 
            // r2ToolStripMenuItem
            // 
            this.r2ToolStripMenuItem.Name = "r2ToolStripMenuItem";
            this.r2ToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.r2ToolStripMenuItem.Text = "R2";
            // 
            // lXToolStripMenuItem1
            // 
            this.lXToolStripMenuItem1.Name = "lXToolStripMenuItem1";
            this.lXToolStripMenuItem1.Size = new System.Drawing.Size(103, 22);
            this.lXToolStripMenuItem1.Text = "LX";
            // 
            // lYToolStripMenuItem1
            // 
            this.lYToolStripMenuItem1.Name = "lYToolStripMenuItem1";
            this.lYToolStripMenuItem1.Size = new System.Drawing.Size(103, 22);
            this.lYToolStripMenuItem1.Text = "LY";
            // 
            // rXToolStripMenuItem1
            // 
            this.rXToolStripMenuItem1.Name = "rXToolStripMenuItem1";
            this.rXToolStripMenuItem1.Size = new System.Drawing.Size(103, 22);
            this.rXToolStripMenuItem1.Text = "RX";
            // 
            // rYToolStripMenuItem1
            // 
            this.rYToolStripMenuItem1.Name = "rYToolStripMenuItem1";
            this.rYToolStripMenuItem1.Size = new System.Drawing.Size(103, 22);
            this.rYToolStripMenuItem1.Text = "RY";
            // 
            // dS4ToolStripMenuItem
            // 
            this.dS4ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resetToolStripMenuItem,
            this.noneToolStripMenuItem,
            this.l2ToolStripMenuItem2,
            this.r3ToolStripMenuItem1,
            this.lXToolStripMenuItem,
            this.lYToolStripMenuItem,
            this.rXToolStripMenuItem,
            this.rYToolStripMenuItem});
            this.dS4ToolStripMenuItem.Name = "dS4ToolStripMenuItem";
            this.dS4ToolStripMenuItem.Size = new System.Drawing.Size(94, 22);
            this.dS4ToolStripMenuItem.Text = "DS4";
            // 
            // resetToolStripMenuItem
            // 
            this.resetToolStripMenuItem.Name = "resetToolStripMenuItem";
            this.resetToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.resetToolStripMenuItem.Text = "Reset";
            // 
            // noneToolStripMenuItem
            // 
            this.noneToolStripMenuItem.Name = "noneToolStripMenuItem";
            this.noneToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.noneToolStripMenuItem.Text = "None";
            // 
            // l2ToolStripMenuItem2
            // 
            this.l2ToolStripMenuItem2.Name = "l2ToolStripMenuItem2";
            this.l2ToolStripMenuItem2.Size = new System.Drawing.Size(103, 22);
            this.l2ToolStripMenuItem2.Text = "L2";
            // 
            // r3ToolStripMenuItem1
            // 
            this.r3ToolStripMenuItem1.Name = "r3ToolStripMenuItem1";
            this.r3ToolStripMenuItem1.Size = new System.Drawing.Size(103, 22);
            this.r3ToolStripMenuItem1.Text = "R2";
            // 
            // lXToolStripMenuItem
            // 
            this.lXToolStripMenuItem.Name = "lXToolStripMenuItem";
            this.lXToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.lXToolStripMenuItem.Text = "LX";
            // 
            // lYToolStripMenuItem
            // 
            this.lYToolStripMenuItem.Name = "lYToolStripMenuItem";
            this.lYToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.lYToolStripMenuItem.Text = "LY";
            // 
            // rXToolStripMenuItem
            // 
            this.rXToolStripMenuItem.Name = "rXToolStripMenuItem";
            this.rXToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.rXToolStripMenuItem.Text = "RX";
            // 
            // rYToolStripMenuItem
            // 
            this.rYToolStripMenuItem.Name = "rYToolStripMenuItem";
            this.rYToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.rYToolStripMenuItem.Text = "RY";
            // 
            // ttAxBar
            // 
            this.ttAxBar.AutomaticDelay = 1000;
            this.ttAxBar.IsBalloon = true;
            // 
            // btnView
            // 
            this.btnView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnView.Glassy = true;
            this.btnView.Location = new System.Drawing.Point(381, 348);
            this.btnView.Name = "btnView";
            this.btnView.Size = new System.Drawing.Size(30, 21);
            this.btnView.TabIndex = 29;
            this.btnView.Text = "?";
            this.ttAxBar.SetToolTip(this.btnView, "View the current Profile.");
            this.btnView.UseVisualStyleBackColor = true;
            this.btnView.Click += new System.EventHandler(this.btnView_Click);
            // 
            // btnDel
            // 
            this.btnDel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDel.Enabled = false;
            this.btnDel.Glassy = true;
            this.btnDel.Location = new System.Drawing.Point(151, 348);
            this.btnDel.Name = "btnDel";
            this.btnDel.Size = new System.Drawing.Size(30, 21);
            this.btnDel.TabIndex = 25;
            this.btnDel.Text = "-";
            this.ttAxBar.SetToolTip(this.btnDel, "Delete the current Profile.");
            this.btnDel.UseVisualStyleBackColor = true;
            this.btnDel.Click += new System.EventHandler(this.btnDel_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAdd.Glassy = true;
            this.btnAdd.Location = new System.Drawing.Point(121, 348);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(30, 21);
            this.btnAdd.TabIndex = 24;
            this.btnAdd.Text = "+";
            this.ttAxBar.SetToolTip(this.btnAdd, "Add a new Profile.");
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnEdit
            // 
            this.btnEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnEdit.Enabled = false;
            this.btnEdit.Glassy = true;
            this.btnEdit.Location = new System.Drawing.Point(411, 348);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(30, 21);
            this.btnEdit.TabIndex = 26;
            this.btnEdit.Text = ">";
            this.ttAxBar.SetToolTip(this.btnEdit, "Edit the current Profile.");
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // btnActivate
            // 
            this.btnActivate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnActivate.Glassy = true;
            this.btnActivate.Location = new System.Drawing.Point(597, 348);
            this.btnActivate.Name = "btnActivate";
            this.btnActivate.Size = new System.Drawing.Size(75, 21);
            this.btnActivate.TabIndex = 0;
            this.btnActivate.Text = "Activate";
            this.ttAxBar.SetToolTip(this.btnActivate, "Activate the current Profile on the Server.");
            this.btnActivate.UseVisualStyleBackColor = true;
            this.btnActivate.Click += new System.EventHandler(this.btnActivate_Click);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Glassy = true;
            this.btnSave.Location = new System.Drawing.Point(522, 348);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 21);
            this.btnSave.TabIndex = 28;
            this.btnSave.Text = "Save";
            this.ttAxBar.SetToolTip(this.btnSave, "Save the Profile Map to the Server.");
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // axRY
            // 
            this.axRY.BackColor = System.Drawing.Color.Transparent;
            this.axRY.Color = System.Drawing.Color.DodgerBlue;
            this.axRY.ContextMenuStrip = this.msAxis;
            this.axRY.Enabled = false;
            this.axRY.Location = new System.Drawing.Point(410, 281);
            this.axRY.Name = "axRY";
            this.axRY.Orientation = ScpMonitor.Orientation.Left;
            this.axRY.Size = new System.Drawing.Size(115, 15);
            this.axRY.TabIndex = 22;
            this.axRY.Text = "RY";
            this.axRY.Value = ((byte)(0));
            // 
            // axRX
            // 
            this.axRX.BackColor = System.Drawing.Color.Transparent;
            this.axRX.Color = System.Drawing.Color.DodgerBlue;
            this.axRX.ContextMenuStrip = this.msAxis;
            this.axRX.Enabled = false;
            this.axRX.Location = new System.Drawing.Point(410, 260);
            this.axRX.Name = "axRX";
            this.axRX.Orientation = ScpMonitor.Orientation.Left;
            this.axRX.Size = new System.Drawing.Size(115, 15);
            this.axRX.TabIndex = 21;
            this.axRX.Text = "RX";
            this.axRX.Value = ((byte)(0));
            // 
            // cbProfile
            // 
            this.cbProfile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbProfile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbProfile.FormattingEnabled = true;
            this.cbProfile.Location = new System.Drawing.Point(187, 348);
            this.cbProfile.Name = "cbProfile";
            this.cbProfile.Size = new System.Drawing.Size(188, 21);
            this.cbProfile.TabIndex = 27;
            this.cbProfile.SelectedIndexChanged += new System.EventHandler(this.Profile_Selected);
            // 
            // cbPad
            // 
            this.cbPad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbPad.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbPad.FormattingEnabled = true;
            this.cbPad.Items.AddRange(new object[] {
            "Pad 1",
            "Pad 2",
            "Pad 3",
            "Pad 4"});
            this.cbPad.Location = new System.Drawing.Point(12, 348);
            this.cbPad.Name = "cbPad";
            this.cbPad.Size = new System.Drawing.Size(94, 21);
            this.cbPad.TabIndex = 23;
            this.cbPad.SelectedIndexChanged += new System.EventHandler(this.Pad_Selected);
            // 
            // scpProxy
            // 
            this.scpProxy.Packet += new System.EventHandler<ScpControl.DsPacket>(this.Parse);
            // 
            // axL
            // 
            this.axL.BackColor = System.Drawing.Color.Transparent;
            this.axL.Color = System.Drawing.Color.DodgerBlue;
            this.axL.Enabled = false;
            this.axL.Location = new System.Drawing.Point(12, 104);
            this.axL.Name = "axL";
            this.axL.Orientation = ScpMonitor.Orientation.Right;
            this.axL.Size = new System.Drawing.Size(50, 15);
            this.axL.TabIndex = 11;
            this.axL.Text = "L";
            this.axL.Value = ((byte)(0));
            // 
            // axD
            // 
            this.axD.BackColor = System.Drawing.Color.Transparent;
            this.axD.Color = System.Drawing.Color.DodgerBlue;
            this.axD.Enabled = false;
            this.axD.Location = new System.Drawing.Point(69, 123);
            this.axD.Name = "axD";
            this.axD.Orientation = ScpMonitor.Orientation.Bottom;
            this.axD.Size = new System.Drawing.Size(25, 30);
            this.axD.TabIndex = 10;
            this.axD.Text = "D";
            this.axD.Value = ((byte)(0));
            // 
            // axR
            // 
            this.axR.BackColor = System.Drawing.Color.Transparent;
            this.axR.Color = System.Drawing.Color.DodgerBlue;
            this.axR.Enabled = false;
            this.axR.Location = new System.Drawing.Point(98, 104);
            this.axR.Name = "axR";
            this.axR.Orientation = ScpMonitor.Orientation.Left;
            this.axR.Size = new System.Drawing.Size(50, 15);
            this.axR.TabIndex = 9;
            this.axR.Text = "R";
            this.axR.Value = ((byte)(0));
            // 
            // axU
            // 
            this.axU.BackColor = System.Drawing.Color.Transparent;
            this.axU.Color = System.Drawing.Color.DodgerBlue;
            this.axU.Enabled = false;
            this.axU.Location = new System.Drawing.Point(69, 68);
            this.axU.Name = "axU";
            this.axU.Orientation = ScpMonitor.Orientation.Top;
            this.axU.Size = new System.Drawing.Size(25, 30);
            this.axU.TabIndex = 8;
            this.axU.Text = "U";
            this.axU.Value = ((byte)(0));
            // 
            // axTP
            // 
            this.axTP.BackColor = System.Drawing.Color.Transparent;
            this.axTP.Color = System.Drawing.Color.DodgerBlue;
            this.axTP.Enabled = false;
            this.axTP.Location = new System.Drawing.Point(333, 32);
            this.axTP.Name = "axTP";
            this.axTP.Orientation = ScpMonitor.Orientation.Bottom;
            this.axTP.Size = new System.Drawing.Size(25, 30);
            this.axTP.TabIndex = 4;
            this.axTP.Text = "TP";
            this.axTP.Value = ((byte)(0));
            // 
            // axPS
            // 
            this.axPS.BackColor = System.Drawing.Color.Transparent;
            this.axPS.Color = System.Drawing.Color.DodgerBlue;
            this.axPS.Enabled = false;
            this.axPS.Location = new System.Drawing.Point(333, 211);
            this.axPS.Name = "axPS";
            this.axPS.Orientation = ScpMonitor.Orientation.Top;
            this.axPS.Size = new System.Drawing.Size(25, 30);
            this.axPS.TabIndex = 16;
            this.axPS.Text = "PS";
            this.axPS.Value = ((byte)(0));
            // 
            // axOP
            // 
            this.axOP.BackColor = System.Drawing.Color.Transparent;
            this.axOP.Color = System.Drawing.Color.DodgerBlue;
            this.axOP.Enabled = false;
            this.axOP.Location = new System.Drawing.Point(432, 32);
            this.axOP.Name = "axOP";
            this.axOP.Orientation = ScpMonitor.Orientation.Bottom;
            this.axOP.Size = new System.Drawing.Size(25, 30);
            this.axOP.TabIndex = 5;
            this.axOP.Text = "OP";
            this.axOP.Value = ((byte)(0));
            // 
            // axSH
            // 
            this.axSH.BackColor = System.Drawing.Color.Transparent;
            this.axSH.Color = System.Drawing.Color.DodgerBlue;
            this.axSH.Enabled = false;
            this.axSH.Location = new System.Drawing.Point(233, 32);
            this.axSH.Name = "axSH";
            this.axSH.Orientation = ScpMonitor.Orientation.Bottom;
            this.axSH.Size = new System.Drawing.Size(25, 30);
            this.axSH.TabIndex = 3;
            this.axSH.Text = "SH";
            this.axSH.Value = ((byte)(0));
            // 
            // axS
            // 
            this.axS.BackColor = System.Drawing.Color.Transparent;
            this.axS.Color = System.Drawing.Color.DodgerBlue;
            this.axS.Enabled = false;
            this.axS.Location = new System.Drawing.Point(536, 104);
            this.axS.Name = "axS";
            this.axS.Orientation = ScpMonitor.Orientation.Right;
            this.axS.Size = new System.Drawing.Size(50, 15);
            this.axS.TabIndex = 15;
            this.axS.Text = "S";
            this.axS.Value = ((byte)(0));
            // 
            // axX
            // 
            this.axX.BackColor = System.Drawing.Color.Transparent;
            this.axX.Color = System.Drawing.Color.DodgerBlue;
            this.axX.Enabled = false;
            this.axX.Location = new System.Drawing.Point(593, 123);
            this.axX.Name = "axX";
            this.axX.Orientation = ScpMonitor.Orientation.Bottom;
            this.axX.Size = new System.Drawing.Size(25, 30);
            this.axX.TabIndex = 14;
            this.axX.Text = "X";
            this.axX.Value = ((byte)(0));
            // 
            // axC
            // 
            this.axC.BackColor = System.Drawing.Color.Transparent;
            this.axC.Color = System.Drawing.Color.DodgerBlue;
            this.axC.Enabled = false;
            this.axC.Location = new System.Drawing.Point(622, 104);
            this.axC.Name = "axC";
            this.axC.Orientation = ScpMonitor.Orientation.Left;
            this.axC.Size = new System.Drawing.Size(50, 15);
            this.axC.TabIndex = 13;
            this.axC.Text = "C";
            this.axC.Value = ((byte)(0));
            // 
            // axT
            // 
            this.axT.BackColor = System.Drawing.Color.Transparent;
            this.axT.Color = System.Drawing.Color.DodgerBlue;
            this.axT.Enabled = false;
            this.axT.Location = new System.Drawing.Point(593, 68);
            this.axT.Name = "axT";
            this.axT.Orientation = ScpMonitor.Orientation.Top;
            this.axT.Size = new System.Drawing.Size(25, 30);
            this.axT.TabIndex = 12;
            this.axT.Text = "T";
            this.axT.Value = ((byte)(0));
            // 
            // axR3
            // 
            this.axR3.BackColor = System.Drawing.Color.Transparent;
            this.axR3.Color = System.Drawing.Color.DodgerBlue;
            this.axR3.Enabled = false;
            this.axR3.Location = new System.Drawing.Point(410, 224);
            this.axR3.Name = "axR3";
            this.axR3.Orientation = ScpMonitor.Orientation.Top;
            this.axR3.Size = new System.Drawing.Size(25, 30);
            this.axR3.TabIndex = 18;
            this.axR3.Text = "R3";
            this.axR3.Value = ((byte)(0));
            // 
            // axL3
            // 
            this.axL3.BackColor = System.Drawing.Color.Transparent;
            this.axL3.Color = System.Drawing.Color.DodgerBlue;
            this.axL3.Enabled = false;
            this.axL3.Location = new System.Drawing.Point(250, 224);
            this.axL3.Name = "axL3";
            this.axL3.Orientation = ScpMonitor.Orientation.Top;
            this.axL3.Size = new System.Drawing.Size(25, 30);
            this.axL3.TabIndex = 17;
            this.axL3.Text = "L3";
            this.axL3.Value = ((byte)(0));
            // 
            // axR1
            // 
            this.axR1.BackColor = System.Drawing.Color.Transparent;
            this.axR1.Color = System.Drawing.Color.DodgerBlue;
            this.axR1.Enabled = false;
            this.axR1.Location = new System.Drawing.Point(561, 32);
            this.axR1.Name = "axR1";
            this.axR1.Orientation = ScpMonitor.Orientation.Left;
            this.axR1.Size = new System.Drawing.Size(75, 15);
            this.axR1.TabIndex = 7;
            this.axR1.Text = "R1";
            this.axR1.Value = ((byte)(0));
            // 
            // axL1
            // 
            this.axL1.BackColor = System.Drawing.Color.Transparent;
            this.axL1.Color = System.Drawing.Color.DodgerBlue;
            this.axL1.Enabled = false;
            this.axL1.Location = new System.Drawing.Point(49, 32);
            this.axL1.Name = "axL1";
            this.axL1.Orientation = ScpMonitor.Orientation.Right;
            this.axL1.Size = new System.Drawing.Size(75, 15);
            this.axL1.TabIndex = 2;
            this.axL1.Text = "L1";
            this.axL1.Value = ((byte)(0));
            // 
            // axR2
            // 
            this.axR2.BackColor = System.Drawing.Color.Transparent;
            this.axR2.Color = System.Drawing.Color.DodgerBlue;
            this.axR2.ContextMenuStrip = this.msAxis;
            this.axR2.Enabled = false;
            this.axR2.Location = new System.Drawing.Point(561, 12);
            this.axR2.Name = "axR2";
            this.axR2.Orientation = ScpMonitor.Orientation.Left;
            this.axR2.Size = new System.Drawing.Size(75, 15);
            this.axR2.TabIndex = 6;
            this.axR2.Text = "R2";
            this.axR2.Value = ((byte)(0));
            // 
            // axL2
            // 
            this.axL2.BackColor = System.Drawing.Color.Transparent;
            this.axL2.Color = System.Drawing.Color.DodgerBlue;
            this.axL2.ContextMenuStrip = this.msAxis;
            this.axL2.Enabled = false;
            this.axL2.Location = new System.Drawing.Point(49, 12);
            this.axL2.Name = "axL2";
            this.axL2.Orientation = ScpMonitor.Orientation.Right;
            this.axL2.Size = new System.Drawing.Size(75, 15);
            this.axL2.TabIndex = 1;
            this.axL2.Text = "L2";
            this.axL2.Value = ((byte)(0));
            // 
            // axLY
            // 
            this.axLY.BackColor = System.Drawing.Color.Transparent;
            this.axLY.Color = System.Drawing.Color.DodgerBlue;
            this.axLY.ContextMenuStrip = this.msAxis;
            this.axLY.Enabled = false;
            this.axLY.Location = new System.Drawing.Point(160, 281);
            this.axLY.Name = "axLY";
            this.axLY.Orientation = ScpMonitor.Orientation.Right;
            this.axLY.Size = new System.Drawing.Size(115, 15);
            this.axLY.TabIndex = 20;
            this.axLY.Text = "LY";
            this.axLY.Value = ((byte)(0));
            // 
            // axLX
            // 
            this.axLX.BackColor = System.Drawing.Color.Transparent;
            this.axLX.Color = System.Drawing.Color.DodgerBlue;
            this.axLX.ContextMenuStrip = this.msAxis;
            this.axLX.Enabled = false;
            this.axLX.Location = new System.Drawing.Point(160, 260);
            this.axLX.Name = "axLX";
            this.axLX.Orientation = ScpMonitor.Orientation.Right;
            this.axLX.Size = new System.Drawing.Size(115, 15);
            this.axLX.TabIndex = 19;
            this.axLX.Text = "LX";
            this.axLX.Value = ((byte)(0));
            // 
            // ProfilesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ClientSize = new System.Drawing.Size(684, 381);
            this.Controls.Add(this.btnView);
            this.Controls.Add(this.axL);
            this.Controls.Add(this.axD);
            this.Controls.Add(this.axR);
            this.Controls.Add(this.axU);
            this.Controls.Add(this.btnDel);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.cbPad);
            this.Controls.Add(this.cbProfile);
            this.Controls.Add(this.axTP);
            this.Controls.Add(this.axPS);
            this.Controls.Add(this.axOP);
            this.Controls.Add(this.axSH);
            this.Controls.Add(this.axS);
            this.Controls.Add(this.axX);
            this.Controls.Add(this.axC);
            this.Controls.Add(this.axT);
            this.Controls.Add(this.axR3);
            this.Controls.Add(this.axL3);
            this.Controls.Add(this.btnEdit);
            this.Controls.Add(this.btnActivate);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.tbOutput);
            this.Controls.Add(this.axR1);
            this.Controls.Add(this.axL1);
            this.Controls.Add(this.axR2);
            this.Controls.Add(this.axL2);
            this.Controls.Add(this.axRY);
            this.Controls.Add(this.axRX);
            this.Controls.Add(this.axLY);
            this.Controls.Add(this.axLX);
            this.DoubleBuffered = true;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(700, 420);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(700, 420);
            this.Name = "ProfilesForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Profile Manager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_Close);
            this.Load += new System.EventHandler(this.Form_Load);
            this.VisibleChanged += new System.EventHandler(this.Form_Visible);
            this.msAxis.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected ScpControl.ScpProxy scpProxy;
        private AxisControl axLX;
        private AxisControl axLY;
        private AxisControl axRX;
        private AxisControl axRY;
        private AxisControl axL2;
        private AxisControl axR2;
        private AxisControl axL1;
        private AxisControl axR1;
        private System.Windows.Forms.TextBox tbOutput;
        private System.Windows.Forms.ContextMenuStrip msAxis;
        private System.Windows.Forms.ToolStripMenuItem dS3ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem l1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem l2ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem l1ToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem l2ToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem r1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem r2ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lXToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem lYToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem rXToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem rYToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem dS4ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem noneToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem l2ToolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem r3ToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem lXToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lYToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rXToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rYToolStripMenuItem;
        private System.Windows.Forms.ToolTip ttAxBar;
        private ScpButton btnSave;
        private ScpButton btnActivate;
        private ScpButton btnEdit;
        private AxisControl axL3;
        private AxisControl axR3;
        private AxisControl axS;
        private AxisControl axX;
        private AxisControl axC;
        private AxisControl axT;
        private AxisControl axSH;
        private AxisControl axOP;
        private AxisControl axPS;
        private AxisControl axTP;
        private System.Windows.Forms.ComboBox cbProfile;
        private System.Windows.Forms.ComboBox cbPad;
        private ScpButton btnAdd;
        private ScpButton btnDel;
        private AxisControl axL;
        private AxisControl axD;
        private AxisControl axR;
        private AxisControl axU;
        private ScpButton btnView;
    }
}

