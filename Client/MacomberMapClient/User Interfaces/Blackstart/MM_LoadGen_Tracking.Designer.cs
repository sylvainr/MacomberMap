namespace MacomberMapClient.User_Interfaces.Blackstart
{
    partial class MM_LoadGen_Tracking
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MM_LoadGen_Tracking));
            this.tsMain = new System.Windows.Forms.ToolStrip();
            this.btnTrendHistory = new System.Windows.Forms.ToolStripDropDownButton();
            this.byOperatorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.byCountyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.flwMain = new System.Windows.Forms.FlowLayoutPanel();
            this.tsMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // tsMain
            // 
            this.tsMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnTrendHistory});
            this.tsMain.Location = new System.Drawing.Point(0, 0);
            this.tsMain.Name = "tsMain";
            this.tsMain.Size = new System.Drawing.Size(767, 25);
            this.tsMain.TabIndex = 0;
            this.tsMain.Text = "toolStrip1";
            // 
            // btnTrendHistory
            // 
            this.btnTrendHistory.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.byOperatorToolStripMenuItem,
            this.byCountyToolStripMenuItem});
            this.btnTrendHistory.ForeColor = System.Drawing.Color.Black;
            this.btnTrendHistory.Image = global::MacomberMapClient.Properties.Resources.Log;
            this.btnTrendHistory.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnTrendHistory.Name = "btnTrendHistory";
            this.btnTrendHistory.Size = new System.Drawing.Size(106, 22);
            this.btnTrendHistory.Text = "Trend history";
            this.btnTrendHistory.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.btnTrendHistory_DropDownItemClicked);
            // 
            // byOperatorToolStripMenuItem
            // 
            this.byOperatorToolStripMenuItem.Name = "byOperatorToolStripMenuItem";
            this.byOperatorToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.byOperatorToolStripMenuItem.Text = "By Operator";
            // 
            // byCountyToolStripMenuItem
            // 
            this.byCountyToolStripMenuItem.Name = "byCountyToolStripMenuItem";
            this.byCountyToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.byCountyToolStripMenuItem.Text = "By County";
            // 
            // flwMain
            // 
            this.flwMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flwMain.Location = new System.Drawing.Point(0, 25);
            this.flwMain.Name = "flwMain";
            this.flwMain.Size = new System.Drawing.Size(767, 373);
            this.flwMain.TabIndex = 1;
            // 
            // MM_LoadGen_Tracking
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(767, 398);
            this.Controls.Add(this.flwMain);
            this.Controls.Add(this.tsMain);
            this.ForeColor = System.Drawing.Color.White;
            this.Icon = Properties.Resources.CompanyIcon;
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "MM_LoadGen_Tracking";
            this.Text = "Element Tracking";
            this.tsMain.ResumeLayout(false);
            this.tsMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip tsMain;
        private System.Windows.Forms.ToolStripDropDownButton btnTrendHistory;
        private System.Windows.Forms.FlowLayoutPanel flwMain;
        private System.Windows.Forms.ToolStripMenuItem byOperatorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem byCountyToolStripMenuItem;
    }
}