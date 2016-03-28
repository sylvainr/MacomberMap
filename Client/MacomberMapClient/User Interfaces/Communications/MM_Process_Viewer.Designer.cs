using MacomberMapClient.Properties;
namespace MacomberMapClient.User_Interfaces.Communications
{
    partial class MM_Process_Viewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MM_Process_Viewer));
            this.lvProcesses = new System.Windows.Forms.ListView();
            this.tmr = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // lvProcesses
            // 
            this.lvProcesses.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvProcesses.Location = new System.Drawing.Point(0, 0);
            this.lvProcesses.Name = "lvProcesses";
            this.lvProcesses.Size = new System.Drawing.Size(292, 266);
            this.lvProcesses.TabIndex = 0;
            this.lvProcesses.UseCompatibleStateImageBehavior = false;
            this.lvProcesses.View = System.Windows.Forms.View.Details;
            // 
            // tmr
            // 
            this.tmr.Interval = 1000;
            this.tmr.Tick += new System.EventHandler(this.UpdateTimer);
            // 
            // Process_Viewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Controls.Add(this.lvProcesses);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = Properties.Resources.CompanyIcon;
            this.Name = "Process_Viewer";
            this.Text = " System Processes";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Process_Viewer_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lvProcesses;
        private System.Windows.Forms.Timer tmr;
    }
}