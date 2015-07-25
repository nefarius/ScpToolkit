namespace ScpMonitor
{
    partial class SettingsForm
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
            this.gbFlip = new System.Windows.Forms.GroupBox();
            this.cbRY = new System.Windows.Forms.CheckBox();
            this.cbRX = new System.Windows.Forms.CheckBox();
            this.cbLY = new System.Windows.Forms.CheckBox();
            this.cbLX = new System.Windows.Forms.CheckBox();
            this.tbIdle = new System.Windows.Forms.TrackBar();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblIdle = new System.Windows.Forms.Label();
            this.cbLED = new System.Windows.Forms.CheckBox();
            this.cbRumble = new System.Windows.Forms.CheckBox();
            this.cbTriggers = new System.Windows.Forms.CheckBox();
            this.tbLatency = new System.Windows.Forms.TrackBar();
            this.lblLatency = new System.Windows.Forms.Label();
            this.tbLeft = new System.Windows.Forms.TrackBar();
            this.tbRight = new System.Windows.Forms.TrackBar();
            this.gbThreshold = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cbNative = new System.Windows.Forms.CheckBox();
            this.cbSSP = new System.Windows.Forms.CheckBox();
            this.ttSSP = new System.Windows.Forms.ToolTip(this.components);
            this.cbForce = new System.Windows.Forms.CheckBox();
            this.lblBrightness = new System.Windows.Forms.Label();
            this.tbBrightness = new System.Windows.Forms.TrackBar();
            this.gbFlip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbIdle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbLatency)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRight)).BeginInit();
            this.gbThreshold.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbBrightness)).BeginInit();
            this.SuspendLayout();
            // 
            // gbFlip
            // 
            this.gbFlip.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbFlip.Controls.Add(this.cbRY);
            this.gbFlip.Controls.Add(this.cbRX);
            this.gbFlip.Controls.Add(this.cbLY);
            this.gbFlip.Controls.Add(this.cbLX);
            this.gbFlip.Location = new System.Drawing.Point(12, 12);
            this.gbFlip.Name = "gbFlip";
            this.gbFlip.Size = new System.Drawing.Size(270, 44);
            this.gbFlip.TabIndex = 0;
            this.gbFlip.TabStop = false;
            this.gbFlip.Text = " Flip Axis ";
            // 
            // cbRY
            // 
            this.cbRY.AutoSize = true;
            this.cbRY.Location = new System.Drawing.Point(201, 20);
            this.cbRY.Name = "cbRY";
            this.cbRY.Size = new System.Drawing.Size(41, 17);
            this.cbRY.TabIndex = 3;
            this.cbRY.Text = "RY";
            this.cbRY.UseVisualStyleBackColor = true;
            // 
            // cbRX
            // 
            this.cbRX.AutoSize = true;
            this.cbRX.Location = new System.Drawing.Point(141, 20);
            this.cbRX.Name = "cbRX";
            this.cbRX.Size = new System.Drawing.Size(41, 17);
            this.cbRX.TabIndex = 2;
            this.cbRX.Text = "RX";
            this.cbRX.UseVisualStyleBackColor = true;
            // 
            // cbLY
            // 
            this.cbLY.AutoSize = true;
            this.cbLY.Location = new System.Drawing.Point(81, 20);
            this.cbLY.Name = "cbLY";
            this.cbLY.Size = new System.Drawing.Size(39, 17);
            this.cbLY.TabIndex = 1;
            this.cbLY.Text = "LY";
            this.cbLY.UseVisualStyleBackColor = true;
            // 
            // cbLX
            // 
            this.cbLX.AutoSize = true;
            this.cbLX.Location = new System.Drawing.Point(21, 20);
            this.cbLX.Name = "cbLX";
            this.cbLX.Size = new System.Drawing.Size(39, 17);
            this.cbLX.TabIndex = 0;
            this.cbLX.Text = "LX";
            this.cbLX.UseVisualStyleBackColor = true;
            // 
            // tbIdle
            // 
            this.tbIdle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbIdle.AutoSize = false;
            this.tbIdle.Location = new System.Drawing.Point(12, 169);
            this.tbIdle.Maximum = 30;
            this.tbIdle.Name = "tbIdle";
            this.tbIdle.Size = new System.Drawing.Size(270, 34);
            this.tbIdle.TabIndex = 2;
            this.tbIdle.Value = 10;
            this.tbIdle.ValueChanged += new System.EventHandler(this.tbIdle_ValueChanged);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(126, 447);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 16;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(207, 447);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 17;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lblIdle
            // 
            this.lblIdle.AutoSize = true;
            this.lblIdle.Location = new System.Drawing.Point(12, 206);
            this.lblIdle.Name = "lblIdle";
            this.lblIdle.Size = new System.Drawing.Size(125, 13);
            this.lblIdle.TabIndex = 3;
            this.lblIdle.Text = "Idle Timeout : 10 minutes";
            // 
            // cbLED
            // 
            this.cbLED.AutoSize = true;
            this.cbLED.Location = new System.Drawing.Point(12, 373);
            this.cbLED.Name = "cbLED";
            this.cbLED.Size = new System.Drawing.Size(85, 17);
            this.cbLED.TabIndex = 8;
            this.cbLED.Text = "Disable LED";
            this.cbLED.UseVisualStyleBackColor = true;
            // 
            // cbRumble
            // 
            this.cbRumble.AutoSize = true;
            this.cbRumble.Location = new System.Drawing.Point(12, 396);
            this.cbRumble.Name = "cbRumble";
            this.cbRumble.Size = new System.Drawing.Size(100, 17);
            this.cbRumble.TabIndex = 9;
            this.cbRumble.Text = "Disable Rumble";
            this.cbRumble.UseVisualStyleBackColor = true;
            // 
            // cbTriggers
            // 
            this.cbTriggers.AutoSize = true;
            this.cbTriggers.Location = new System.Drawing.Point(153, 419);
            this.cbTriggers.Name = "cbTriggers";
            this.cbTriggers.Size = new System.Drawing.Size(88, 17);
            this.cbTriggers.TabIndex = 10;
            this.cbTriggers.Text = "Map Triggers";
            this.cbTriggers.UseVisualStyleBackColor = true;
            this.cbTriggers.Visible = false;
            // 
            // tbLatency
            // 
            this.tbLatency.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbLatency.AutoSize = false;
            this.tbLatency.LargeChange = 1;
            this.tbLatency.Location = new System.Drawing.Point(12, 231);
            this.tbLatency.Maximum = 16;
            this.tbLatency.Name = "tbLatency";
            this.tbLatency.Size = new System.Drawing.Size(270, 34);
            this.tbLatency.TabIndex = 4;
            this.tbLatency.Value = 8;
            this.tbLatency.ValueChanged += new System.EventHandler(this.tbLatency_ValueChanged);
            // 
            // lblLatency
            // 
            this.lblLatency.AutoSize = true;
            this.lblLatency.Location = new System.Drawing.Point(12, 268);
            this.lblLatency.Name = "lblLatency";
            this.lblLatency.Size = new System.Drawing.Size(151, 13);
            this.lblLatency.TabIndex = 5;
            this.lblLatency.Text = "DS3 Rumble Latency : 128 ms";
            // 
            // tbLeft
            // 
            this.tbLeft.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbLeft.AutoSize = false;
            this.tbLeft.LargeChange = 8;
            this.tbLeft.Location = new System.Drawing.Point(78, 19);
            this.tbLeft.Maximum = 127;
            this.tbLeft.Name = "tbLeft";
            this.tbLeft.Size = new System.Drawing.Size(186, 34);
            this.tbLeft.TabIndex = 1;
            this.tbLeft.TickFrequency = 8;
            // 
            // tbRight
            // 
            this.tbRight.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbRight.AutoSize = false;
            this.tbRight.LargeChange = 8;
            this.tbRight.Location = new System.Drawing.Point(78, 59);
            this.tbRight.Maximum = 127;
            this.tbRight.Name = "tbRight";
            this.tbRight.Size = new System.Drawing.Size(186, 34);
            this.tbRight.TabIndex = 3;
            this.tbRight.TickFrequency = 8;
            // 
            // gbThreshold
            // 
            this.gbThreshold.Controls.Add(this.label2);
            this.gbThreshold.Controls.Add(this.label1);
            this.gbThreshold.Controls.Add(this.tbLeft);
            this.gbThreshold.Controls.Add(this.tbRight);
            this.gbThreshold.Location = new System.Drawing.Point(12, 62);
            this.gbThreshold.Name = "gbThreshold";
            this.gbThreshold.Size = new System.Drawing.Size(270, 101);
            this.gbThreshold.TabIndex = 1;
            this.gbThreshold.TabStop = false;
            this.gbThreshold.Text = "Threshold";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Right Stick";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Left Stick";
            // 
            // cbNative
            // 
            this.cbNative.AutoSize = true;
            this.cbNative.Location = new System.Drawing.Point(153, 373);
            this.cbNative.Name = "cbNative";
            this.cbNative.Size = new System.Drawing.Size(122, 17);
            this.cbNative.TabIndex = 11;
            this.cbNative.Text = "Disable Native Feed";
            this.cbNative.UseVisualStyleBackColor = true;
            // 
            // cbSSP
            // 
            this.cbSSP.AutoSize = true;
            this.cbSSP.Location = new System.Drawing.Point(153, 396);
            this.cbSSP.Name = "cbSSP";
            this.cbSSP.Size = new System.Drawing.Size(85, 17);
            this.cbSSP.TabIndex = 12;
            this.cbSSP.Text = "Disable SSP";
            this.cbSSP.UseVisualStyleBackColor = true;
            // 
            // cbForce
            // 
            this.cbForce.AutoSize = true;
            this.cbForce.Location = new System.Drawing.Point(12, 419);
            this.cbForce.Name = "cbForce";
            this.cbForce.Size = new System.Drawing.Size(81, 17);
            this.cbForce.TabIndex = 18;
            this.cbForce.Text = "DS4 Repair";
            this.ttSSP.SetToolTip(this.cbForce, "Force DS4 to Repair Bluetooth Link Key on USB Connection");
            this.cbForce.UseVisualStyleBackColor = true;
            // 
            // lblBrightness
            // 
            this.lblBrightness.AutoSize = true;
            this.lblBrightness.Location = new System.Drawing.Point(12, 333);
            this.lblBrightness.Name = "lblBrightness";
            this.lblBrightness.Size = new System.Drawing.Size(155, 13);
            this.lblBrightness.TabIndex = 7;
            this.lblBrightness.Text = "DS4 Light Bar Brightness :  128";
            // 
            // tbBrightness
            // 
            this.tbBrightness.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbBrightness.AutoSize = false;
            this.tbBrightness.LargeChange = 16;
            this.tbBrightness.Location = new System.Drawing.Point(12, 296);
            this.tbBrightness.Maximum = 255;
            this.tbBrightness.Name = "tbBrightness";
            this.tbBrightness.Size = new System.Drawing.Size(270, 34);
            this.tbBrightness.TabIndex = 6;
            this.tbBrightness.TickFrequency = 16;
            this.tbBrightness.Value = 128;
            this.tbBrightness.ValueChanged += new System.EventHandler(this.tbBrightness_ValueChanged);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(294, 482);
            this.Controls.Add(this.cbForce);
            this.Controls.Add(this.lblBrightness);
            this.Controls.Add(this.tbBrightness);
            this.Controls.Add(this.cbSSP);
            this.Controls.Add(this.cbNative);
            this.Controls.Add(this.gbThreshold);
            this.Controls.Add(this.lblLatency);
            this.Controls.Add(this.tbLatency);
            this.Controls.Add(this.cbTriggers);
            this.Controls.Add(this.cbRumble);
            this.Controls.Add(this.cbLED);
            this.Controls.Add(this.lblIdle);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.tbIdle);
            this.Controls.Add(this.gbFlip);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Service Configuration";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_Closing);
            this.Load += new System.EventHandler(this.Form_Load);
            this.gbFlip.ResumeLayout(false);
            this.gbFlip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbIdle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbLatency)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbLeft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRight)).EndInit();
            this.gbThreshold.ResumeLayout(false);
            this.gbThreshold.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbBrightness)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox gbFlip;
        private System.Windows.Forms.CheckBox cbRY;
        private System.Windows.Forms.CheckBox cbRX;
        private System.Windows.Forms.CheckBox cbLY;
        private System.Windows.Forms.CheckBox cbLX;
        private System.Windows.Forms.TrackBar tbIdle;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblIdle;
        private System.Windows.Forms.CheckBox cbLED;
        private System.Windows.Forms.CheckBox cbRumble;
        private System.Windows.Forms.CheckBox cbTriggers;
        private System.Windows.Forms.TrackBar tbLatency;
        private System.Windows.Forms.Label lblLatency;
        private System.Windows.Forms.TrackBar tbLeft;
        private System.Windows.Forms.TrackBar tbRight;
        private System.Windows.Forms.GroupBox gbThreshold;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cbNative;
        private System.Windows.Forms.CheckBox cbSSP;
        private System.Windows.Forms.ToolTip ttSSP;
        private System.Windows.Forms.Label lblBrightness;
        private System.Windows.Forms.TrackBar tbBrightness;
        private System.Windows.Forms.CheckBox cbForce;
    }
}