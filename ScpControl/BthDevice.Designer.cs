namespace ScpControl
{
    partial class BthDevice
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
            this.components = new System.ComponentModel.Container();
            this.tmUpdate = new ScpControl.ScpTimer(this.components);
            // 
            // tmUpdate
            // 
            this.tmUpdate.Enabled = false;
            this.tmUpdate.Interval = ((uint)(10u));
            this.tmUpdate.Tag = null;
            this.tmUpdate.Tick += new System.EventHandler(this.On_Timer);

        }

        #endregion

        private ScpTimer tmUpdate;
    }
}
