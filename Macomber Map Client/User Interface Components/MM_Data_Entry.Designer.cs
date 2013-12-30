namespace Macomber_Map.User_Interface_Components
{
    partial class MM_Data_Entry
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MM_Data_Entry));
            this.tcDataEntry = new System.Windows.Forms.TabControl();
            this.tabValueUpdates = new System.Windows.Forms.TabPage();
            this.tabViolations = new System.Windows.Forms.TabPage();
            this.splMain = new System.Windows.Forms.SplitContainer();
            this.srMain = new Macomber_Map.User_Interface_Components.Search_Results();
            this.tcDataEntry.SuspendLayout();
            this.splMain.Panel1.SuspendLayout();
            this.splMain.Panel2.SuspendLayout();
            this.splMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // tcDataEntry
            // 
            this.tcDataEntry.Controls.Add(this.tabValueUpdates);
            this.tcDataEntry.Controls.Add(this.tabViolations);
            this.tcDataEntry.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcDataEntry.Location = new System.Drawing.Point(0, 0);
            this.tcDataEntry.Name = "tcDataEntry";
            this.tcDataEntry.SelectedIndex = 0;
            this.tcDataEntry.Size = new System.Drawing.Size(514, 353);
            this.tcDataEntry.TabIndex = 0;
            // 
            // tabValueUpdates
            // 
            this.tabValueUpdates.Location = new System.Drawing.Point(4, 22);
            this.tabValueUpdates.Name = "tabValueUpdates";
            this.tabValueUpdates.Padding = new System.Windows.Forms.Padding(3);
            this.tabValueUpdates.Size = new System.Drawing.Size(506, 327);
            this.tabValueUpdates.TabIndex = 0;
            this.tabValueUpdates.Text = "Value Updates";
            this.tabValueUpdates.UseVisualStyleBackColor = true;
            // 
            // tabViolations
            // 
            this.tabViolations.Location = new System.Drawing.Point(4, 22);
            this.tabViolations.Name = "tabViolations";
            this.tabViolations.Padding = new System.Windows.Forms.Padding(3);
            this.tabViolations.Size = new System.Drawing.Size(539, 237);
            this.tabViolations.TabIndex = 1;
            this.tabViolations.Text = "Violations";
            this.tabViolations.UseVisualStyleBackColor = true;
            // 
            // splMain
            // 
            this.splMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splMain.Location = new System.Drawing.Point(0, 0);
            this.splMain.Name = "splMain";
            // 
            // splMain.Panel1
            // 
            this.splMain.Panel1.Controls.Add(this.srMain);
            // 
            // splMain.Panel2
            // 
            this.splMain.Panel2.Controls.Add(this.tcDataEntry);
            this.splMain.Size = new System.Drawing.Size(834, 353);
            this.splMain.SplitterDistance = 316;
            this.splMain.TabIndex = 1;
            // 
            // srMain
            // 
            this.srMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.srMain.Location = new System.Drawing.Point(0, 0);
            this.srMain.Name = "srMain";
            this.srMain.Size = new System.Drawing.Size(316, 353);
            this.srMain.TabIndex = 0;
            // 
            // MM_Data_Entry
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(834, 353);
            this.Controls.Add(this.splMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MM_Data_Entry";
            this.Text = "Manual Data Entry Form";
            this.tcDataEntry.ResumeLayout(false);
            this.splMain.Panel1.ResumeLayout(false);
            this.splMain.Panel2.ResumeLayout(false);
            this.splMain.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tcDataEntry;
        private System.Windows.Forms.TabPage tabValueUpdates;
        private System.Windows.Forms.TabPage tabViolations;
        private System.Windows.Forms.SplitContainer splMain;
        private Search_Results srMain;
    }
}