namespace ScpDriver
{
    partial class ScpForm
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
            this.btnUninstall = new System.Windows.Forms.Button();
            this.btnInstall = new System.Windows.Forms.Button();
            this.InstallWorker = new System.ComponentModel.BackgroundWorker();
            this.UninstallWorker = new System.ComponentModel.BackgroundWorker();
            this.tbOutput = new System.Windows.Forms.TextBox();
            this.pbRunning = new System.Windows.Forms.ProgressBar();
            this.btnExit = new System.Windows.Forms.Button();
            this.cbService = new System.Windows.Forms.CheckBox();
            this.cbBluetooth = new System.Windows.Forms.CheckBox();
            this.cbForce = new System.Windows.Forms.CheckBox();
            this.cbDS3 = new System.Windows.Forms.CheckBox();
            this.cbBus = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btnUninstall
            // 
            this.btnUninstall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUninstall.Location = new System.Drawing.Point(416, 377);
            this.btnUninstall.Name = "btnUninstall";
            this.btnUninstall.Size = new System.Drawing.Size(75, 23);
            this.btnUninstall.TabIndex = 3;
            this.btnUninstall.Text = "&Uninstall";
            this.btnUninstall.UseVisualStyleBackColor = true;
            this.btnUninstall.Click += new System.EventHandler(this.btnUninstall_Click);
            // 
            // btnInstall
            // 
            this.btnInstall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnInstall.Location = new System.Drawing.Point(335, 377);
            this.btnInstall.Name = "btnInstall";
            this.btnInstall.Size = new System.Drawing.Size(75, 23);
            this.btnInstall.TabIndex = 2;
            this.btnInstall.Text = "&Install";
            this.btnInstall.UseVisualStyleBackColor = true;
            this.btnInstall.Click += new System.EventHandler(this.btnInstall_Click);
            // 
            // InstallWorker
            // 
            this.InstallWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.InstallWorker_DoWork);
            this.InstallWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.InstallWorker_RunWorkerCompleted);
            // 
            // UninstallWorker
            // 
            this.UninstallWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.UninstallWorker_DoWork);
            this.UninstallWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.UninstallWorker_RunWorkerCompleted);
            // 
            // tbOutput
            // 
            this.tbOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbOutput.BackColor = System.Drawing.SystemColors.Window;
            this.tbOutput.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbOutput.Location = new System.Drawing.Point(13, 13);
            this.tbOutput.Multiline = true;
            this.tbOutput.Name = "tbOutput";
            this.tbOutput.ReadOnly = true;
            this.tbOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbOutput.Size = new System.Drawing.Size(559, 335);
            this.tbOutput.TabIndex = 4;
            this.tbOutput.TabStop = false;
            this.tbOutput.WordWrap = false;
            // 
            // pbRunning
            // 
            this.pbRunning.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbRunning.Location = new System.Drawing.Point(13, 354);
            this.pbRunning.Name = "pbRunning";
            this.pbRunning.Size = new System.Drawing.Size(559, 17);
            this.pbRunning.TabIndex = 5;
            // 
            // btnExit
            // 
            this.btnExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExit.Location = new System.Drawing.Point(497, 377);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(75, 23);
            this.btnExit.TabIndex = 0;
            this.btnExit.Text = "E&xit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // cbService
            // 
            this.cbService.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbService.AutoSize = true;
            this.cbService.Checked = true;
            this.cbService.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbService.Location = new System.Drawing.Point(102, 381);
            this.cbService.Name = "cbService";
            this.cbService.Size = new System.Drawing.Size(110, 17);
            this.cbService.TabIndex = 6;
            this.cbService.Text = "Configure Service";
            this.cbService.UseVisualStyleBackColor = true;
            // 
            // cbBluetooth
            // 
            this.cbBluetooth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbBluetooth.AutoSize = true;
            this.cbBluetooth.Checked = true;
            this.cbBluetooth.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbBluetooth.Location = new System.Drawing.Point(218, 381);
            this.cbBluetooth.Name = "cbBluetooth";
            this.cbBluetooth.Size = new System.Drawing.Size(102, 17);
            this.cbBluetooth.TabIndex = 7;
            this.cbBluetooth.Text = "Bluetooth Driver";
            this.cbBluetooth.UseVisualStyleBackColor = true;
            // 
            // cbForce
            // 
            this.cbForce.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbForce.AutoSize = true;
            this.cbForce.Location = new System.Drawing.Point(13, 381);
            this.cbForce.Name = "cbForce";
            this.cbForce.Size = new System.Drawing.Size(83, 17);
            this.cbForce.TabIndex = 8;
            this.cbForce.Text = "Force Install";
            this.cbForce.UseVisualStyleBackColor = true;
            // 
            // cbDS3
            // 
            this.cbDS3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbDS3.AutoSize = true;
            this.cbDS3.Checked = true;
            this.cbDS3.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbDS3.Location = new System.Drawing.Point(12, 450);
            this.cbDS3.Name = "cbDS3";
            this.cbDS3.Size = new System.Drawing.Size(78, 17);
            this.cbDS3.TabIndex = 9;
            this.cbDS3.Text = "DS3 Driver";
            this.cbDS3.UseVisualStyleBackColor = true;
            // 
            // cbBus
            // 
            this.cbBus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbBus.AutoSize = true;
            this.cbBus.Checked = true;
            this.cbBus.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbBus.Location = new System.Drawing.Point(102, 450);
            this.cbBus.Name = "cbBus";
            this.cbBus.Size = new System.Drawing.Size(75, 17);
            this.cbBus.TabIndex = 10;
            this.cbBus.Text = "Bus Driver";
            this.cbBus.UseVisualStyleBackColor = true;
            // 
            // ScpForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 412);
            this.Controls.Add(this.cbBus);
            this.Controls.Add(this.cbDS3);
            this.Controls.Add(this.cbForce);
            this.Controls.Add(this.cbBluetooth);
            this.Controls.Add(this.cbService);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.pbRunning);
            this.Controls.Add(this.tbOutput);
            this.Controls.Add(this.btnInstall);
            this.Controls.Add(this.btnUninstall);
            this.MinimumSize = new System.Drawing.Size(600, 450);
            this.Name = "ScpForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "SCP Driver Installer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ScpForm_Close);
            this.Load += new System.EventHandler(this.ScpForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnUninstall;
        private System.Windows.Forms.Button btnInstall;
        private System.ComponentModel.BackgroundWorker InstallWorker;
        private System.ComponentModel.BackgroundWorker UninstallWorker;
        private System.Windows.Forms.TextBox tbOutput;
        private System.Windows.Forms.ProgressBar pbRunning;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.CheckBox cbService;
        private System.Windows.Forms.CheckBox cbBluetooth;
        private System.Windows.Forms.CheckBox cbForce;
        private System.Windows.Forms.CheckBox cbDS3;
        private System.Windows.Forms.CheckBox cbBus;
    }
}

