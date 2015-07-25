namespace ScpMonitor
{
    partial class AxisControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.axBar = new ScpMonitor.AxisBar();
            this.axButton = new ScpMonitor.ScpButton();
            this.SuspendLayout();
            // 
            // axBar
            // 
            this.axBar.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.axBar.BackColor = System.Drawing.Color.Transparent;
            this.axBar.Color = System.Drawing.Color.Green;
            this.axBar.Location = new System.Drawing.Point(25, 0);
            this.axBar.Maximum = 255;
            this.axBar.Minimum = 0;
            this.axBar.Name = "axBar";
            this.axBar.Size = new System.Drawing.Size(135, 15);
            this.axBar.TabIndex = 1;
            this.axBar.Value = 0;
            // 
            // axButton
            // 
            this.axButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.axButton.Glassy = true;
            this.axButton.Location = new System.Drawing.Point(0, 0);
            this.axButton.Name = "axButton";
            this.axButton.Size = new System.Drawing.Size(25, 15);
            this.axButton.TabIndex = 0;
            this.axButton.Text = "--";
            this.axButton.UseVisualStyleBackColor = true;
            this.axButton.Click += new System.EventHandler(this.axButton_Click);
            // 
            // AxisControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.axBar);
            this.Controls.Add(this.axButton);
            this.Name = "AxisControl";
            this.Size = new System.Drawing.Size(160, 15);
            this.ResumeLayout(false);

        }

        #endregion

        private ScpButton axButton;
        private AxisBar axBar;

    }
}
