namespace ScpControl
{
    partial class ScpProxy
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
            this.NativeFeed_Worker = new System.ComponentModel.BackgroundWorker();
            // 
            // NativeFeed_Worker
            // 
            this.NativeFeed_Worker.WorkerReportsProgress = true;
            this.NativeFeed_Worker.WorkerSupportsCancellation = true;
            this.NativeFeed_Worker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.NativeFeed_Worker_DoWork);
        }

        #endregion

        protected System.ComponentModel.BackgroundWorker NativeFeed_Worker;

    }
}
