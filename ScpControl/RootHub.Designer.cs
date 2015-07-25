namespace ScpControl
{
    partial class RootHub
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
            this.UDP_Worker = new System.ComponentModel.BackgroundWorker();
            this.scpMap = new ScpControl.ScpMapper(this.components);
            // 
            // UDP_Worker
            // 
            this.UDP_Worker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.UDP_Worker_Thread);
            // 
            // scpMapper
            // 
            this.scpMap.Active = "";
            this.scpMap.Xml = "System.Xml.XmlDocument";

        }

        #endregion

        private System.ComponentModel.BackgroundWorker UDP_Worker;
        private ScpMapper scpMap;
    }
}
