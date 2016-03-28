namespace MacomberMapIntegratedService
{
    partial class ProjectInstaller
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
            this.MM_Server_Installer_Process = new System.ServiceProcess.ServiceProcessInstaller();
            this.MM_Server_Installer = new System.ServiceProcess.ServiceInstaller();
            // 
            // MM_Server_Installer_Process
            // 
            this.MM_Server_Installer_Process.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.MM_Server_Installer_Process.Password = null;
            this.MM_Server_Installer_Process.Username = null;
            // 
            // MM_Server_Installer
            // 
            this.MM_Server_Installer.Description = "Installer for the Macomber Map Server";
            this.MM_Server_Installer.DisplayName = "Macomber Map Server Installer";
            this.MM_Server_Installer.ServiceName = "Macomber Map Server";
            this.MM_Server_Installer.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.MM_Server_Installer_Process,
            this.MM_Server_Installer});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller MM_Server_Installer_Process;
        private System.ServiceProcess.ServiceInstaller MM_Server_Installer;
    }
}