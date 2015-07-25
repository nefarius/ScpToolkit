namespace ScpPair
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
            this.components = new System.ComponentModel.Container();
            this.usbDevice = new ScpControl.UsbDs3(this.components);
            this.tmEnable = new System.Windows.Forms.Timer(this.components);
            this.tbMaster = new System.Windows.Forms.TextBox();
            this.lblLocal = new System.Windows.Forms.Label();
            this.lblRemote = new System.Windows.Forms.Label();
            this.lblMac = new System.Windows.Forms.Label();
            this.lblMaster = new System.Windows.Forms.Label();
            this.btnSet = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // usbDevice
            // 
            this.usbDevice.IsShutdown = false;
            this.usbDevice.PadId = ScpControl.DsPadId.One;
            // 
            // tmEnable
            // 
            this.tmEnable.Enabled = true;
            this.tmEnable.Tick += new System.EventHandler(this.tmEnable_Tick);
            // 
            // tbMaster
            // 
            this.tbMaster.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbMaster.Location = new System.Drawing.Point(12, 84);
            this.tbMaster.Name = "tbMaster";
            this.tbMaster.Size = new System.Drawing.Size(194, 20);
            this.tbMaster.TabIndex = 0;
            // 
            // lblLocal
            // 
            this.lblLocal.AutoSize = true;
            this.lblLocal.Font = new System.Drawing.Font("Lucida Console", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLocal.Location = new System.Drawing.Point(12, 9);
            this.lblLocal.Name = "lblLocal";
            this.lblLocal.Size = new System.Drawing.Size(61, 12);
            this.lblLocal.TabIndex = 1;
            this.lblLocal.Text = "Local  :";
            // 
            // lblRemote
            // 
            this.lblRemote.AutoSize = true;
            this.lblRemote.Font = new System.Drawing.Font("Lucida Console", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRemote.Location = new System.Drawing.Point(12, 38);
            this.lblRemote.Name = "lblRemote";
            this.lblRemote.Size = new System.Drawing.Size(61, 12);
            this.lblRemote.TabIndex = 2;
            this.lblRemote.Text = "Remote :";
            // 
            // lblMac
            // 
            this.lblMac.AutoSize = true;
            this.lblMac.Font = new System.Drawing.Font("Lucida Console", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMac.Location = new System.Drawing.Point(79, 9);
            this.lblMac.Name = "lblMac";
            this.lblMac.Size = new System.Drawing.Size(0, 12);
            this.lblMac.TabIndex = 3;
            // 
            // lblMaster
            // 
            this.lblMaster.AutoSize = true;
            this.lblMaster.Font = new System.Drawing.Font("Lucida Console", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMaster.Location = new System.Drawing.Point(79, 38);
            this.lblMaster.Name = "lblMaster";
            this.lblMaster.Size = new System.Drawing.Size(0, 12);
            this.lblMaster.TabIndex = 4;
            // 
            // btnSet
            // 
            this.btnSet.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSet.Enabled = false;
            this.btnSet.Location = new System.Drawing.Point(131, 119);
            this.btnSet.Name = "btnSet";
            this.btnSet.Size = new System.Drawing.Size(75, 23);
            this.btnSet.TabIndex = 5;
            this.btnSet.Text = "Set";
            this.btnSet.UseVisualStyleBackColor = true;
            this.btnSet.Click += new System.EventHandler(this.btnSet_Click);
            this.btnSet.Enter += new System.EventHandler(this.Button_Enter);
            // 
            // ScpForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(221, 154);
            this.Controls.Add(this.btnSet);
            this.Controls.Add(this.lblMaster);
            this.Controls.Add(this.lblMac);
            this.Controls.Add(this.lblRemote);
            this.Controls.Add(this.lblLocal);
            this.Controls.Add(this.tbMaster);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ScpForm";
            this.Text = "SCP Pair Tool";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_Close);
            this.Load += new System.EventHandler(this.Form_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ScpControl.UsbDs3 usbDevice;
        private System.Windows.Forms.Timer tmEnable;
        private System.Windows.Forms.TextBox tbMaster;
        private System.Windows.Forms.Label lblLocal;
        private System.Windows.Forms.Label lblRemote;
        private System.Windows.Forms.Label lblMac;
        private System.Windows.Forms.Label lblMaster;
        private System.Windows.Forms.Button btnSet;

    }
}

