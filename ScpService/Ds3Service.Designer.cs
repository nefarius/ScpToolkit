namespace ScpService
{
    partial class Ds3Service
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
                m_Timer.Dispose();
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
            this.rootHub = new ScpControl.RootHub(this.components);
            // 
            // rootHub
            // 
            this.rootHub.Debug += new System.EventHandler<ScpControl.DebugEventArgs>(this.OnDebug);
            // 
            // Ds3Service
            // 
            this.AutoLog = false;
            this.CanHandlePowerEvent = true;
            this.CanShutdown = true;
            this.ServiceName = "Ds3Service";

        }

        #endregion

        private ScpControl.RootHub rootHub;
    }
}
