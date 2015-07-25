namespace ScpControl
{
    partial class BthDongle
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
            this.HCI_Worker = new System.ComponentModel.BackgroundWorker();
            this.L2CAP_Worker = new System.ComponentModel.BackgroundWorker();
            // 
            // HCI_Worker
            // 
            this.HCI_Worker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.HCI_Worker_Thread);
            // 
            // L2CAP_Worker
            // 
            this.L2CAP_Worker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.L2CAP_Worker_Thread);

        }

        #endregion

        private System.ComponentModel.BackgroundWorker HCI_Worker;
        private System.ComponentModel.BackgroundWorker L2CAP_Worker;
    }
}
