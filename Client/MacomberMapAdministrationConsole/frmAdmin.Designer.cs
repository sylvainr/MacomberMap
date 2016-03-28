namespace MacomberMapAdministrationConsole
{
    partial class frmAdmin
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmAdmin));
            this.grpServers = new System.Windows.Forms.GroupBox();
            this.lvMacomberMapServers = new System.Windows.Forms.ListView();
            this.colValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tmrPingServer = new System.Windows.Forms.Timer(this.components);
            this.grpUsers = new System.Windows.Forms.GroupBox();
            this.lvUsers = new System.Windows.Forms.ListView();
            this.tmrUpdateUserInfo = new System.Windows.Forms.Timer(this.components);
            this.cmsSystemInteraction = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.splMain = new System.Windows.Forms.SplitContainer();
            this.tmrRefreshManualServers = new System.Windows.Forms.Timer(this.components);
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnMergeOperatorship = new System.Windows.Forms.ToolStripButton();
            this.btnCommandHistory = new System.Windows.Forms.ToolStripButton();
            this.grpServers.SuspendLayout();
            this.grpUsers.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splMain)).BeginInit();
            this.splMain.Panel1.SuspendLayout();
            this.splMain.Panel2.SuspendLayout();
            this.splMain.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpServers
            // 
            this.grpServers.Controls.Add(this.lvMacomberMapServers);
            this.grpServers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpServers.ForeColor = System.Drawing.Color.White;
            this.grpServers.Location = new System.Drawing.Point(0, 0);
            this.grpServers.Name = "grpServers";
            this.grpServers.Size = new System.Drawing.Size(368, 411);
            this.grpServers.TabIndex = 0;
            this.grpServers.TabStop = false;
            this.grpServers.Text = "Servers";
            // 
            // lvMacomberMapServers
            // 
            this.lvMacomberMapServers.BackColor = System.Drawing.Color.Black;
            this.lvMacomberMapServers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colValue});
            this.lvMacomberMapServers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvMacomberMapServers.ForeColor = System.Drawing.Color.White;
            this.lvMacomberMapServers.Location = new System.Drawing.Point(3, 16);
            this.lvMacomberMapServers.Name = "lvMacomberMapServers";
            this.lvMacomberMapServers.ShowItemToolTips = true;
            this.lvMacomberMapServers.Size = new System.Drawing.Size(362, 392);
            this.lvMacomberMapServers.TabIndex = 1;
            this.lvMacomberMapServers.UseCompatibleStateImageBehavior = false;
            this.lvMacomberMapServers.View = System.Windows.Forms.View.Details;
            this.lvMacomberMapServers.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvMacomberMapServers_ColumnClick);
            // 
            // colValue
            // 
            this.colValue.Text = "";
            // 
            // tmrPingServer
            // 
            this.tmrPingServer.Enabled = true;
            this.tmrPingServer.Interval = 1000;
            this.tmrPingServer.Tick += new System.EventHandler(this.tmrPingServer_Tick);
            // 
            // grpUsers
            // 
            this.grpUsers.Controls.Add(this.lvUsers);
            this.grpUsers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpUsers.ForeColor = System.Drawing.Color.White;
            this.grpUsers.Location = new System.Drawing.Point(0, 0);
            this.grpUsers.Name = "grpUsers";
            this.grpUsers.Size = new System.Drawing.Size(474, 411);
            this.grpUsers.TabIndex = 1;
            this.grpUsers.TabStop = false;
            this.grpUsers.Text = "Users";
            // 
            // lvUsers
            // 
            this.lvUsers.BackColor = System.Drawing.Color.Black;
            this.lvUsers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvUsers.ForeColor = System.Drawing.Color.White;
            this.lvUsers.FullRowSelect = true;
            this.lvUsers.Location = new System.Drawing.Point(3, 16);
            this.lvUsers.Name = "lvUsers";
            this.lvUsers.Size = new System.Drawing.Size(468, 392);
            this.lvUsers.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvUsers.TabIndex = 1;
            this.lvUsers.UseCompatibleStateImageBehavior = false;
            this.lvUsers.View = System.Windows.Forms.View.Details;
            this.lvUsers.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lvUsers_MouseClick);
            // 
            // tmrUpdateUserInfo
            // 
            this.tmrUpdateUserInfo.Enabled = true;
            this.tmrUpdateUserInfo.Interval = 1000;
            this.tmrUpdateUserInfo.Tick += new System.EventHandler(this.tmrUpdateUserInfo_Tick);
            // 
            // cmsSystemInteraction
            // 
            this.cmsSystemInteraction.Name = "cmsSystemInteraction";
            this.cmsSystemInteraction.Size = new System.Drawing.Size(61, 4);
            this.cmsSystemInteraction.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.cmsSystemInteraction_ItemClicked);
            // 
            // splMain
            // 
            this.splMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splMain.Location = new System.Drawing.Point(0, 25);
            this.splMain.Name = "splMain";
            // 
            // splMain.Panel1
            // 
            this.splMain.Panel1.Controls.Add(this.grpServers);
            // 
            // splMain.Panel2
            // 
            this.splMain.Panel2.Controls.Add(this.grpUsers);
            this.splMain.Size = new System.Drawing.Size(846, 411);
            this.splMain.SplitterDistance = 368;
            this.splMain.TabIndex = 2;
            // 
            // tmrRefreshManualServers
            // 
            this.tmrRefreshManualServers.Enabled = true;
            this.tmrRefreshManualServers.Interval = 5000;
            this.tmrRefreshManualServers.Tick += new System.EventHandler(this.tmrRefreshManualServers_Tick);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnMergeOperatorship,
            this.btnCommandHistory});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(846, 25);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnMergeOperatorship
            // 
            this.btnMergeOperatorship.ForeColor = System.Drawing.Color.Black;
            this.btnMergeOperatorship.Image = ((System.Drawing.Image)(resources.GetObject("btnMergeOperatorship.Image")));
            this.btnMergeOperatorship.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnMergeOperatorship.Name = "btnMergeOperatorship";
            this.btnMergeOperatorship.Size = new System.Drawing.Size(155, 22);
            this.btnMergeOperatorship.Text = "Merge operatorship files";
            this.btnMergeOperatorship.Click += new System.EventHandler(this.btnMergeOperatorship_Click);
            // 
            // btnCommandHistory
            // 
            this.btnCommandHistory.ForeColor = System.Drawing.Color.Black;
            this.btnCommandHistory.Image = ((System.Drawing.Image)(resources.GetObject("btnCommandHistory.Image")));
            this.btnCommandHistory.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCommandHistory.Name = "btnCommandHistory";
            this.btnCommandHistory.Size = new System.Drawing.Size(125, 22);
            this.btnCommandHistory.Text = "Command History";
            this.btnCommandHistory.Click += new System.EventHandler(this.btnCommandHistory_Click);
            // 
            // frmAdmin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(846, 436);
            this.Controls.Add(this.splMain);
            this.Controls.Add(this.toolStrip1);
            this.ForeColor = System.Drawing.Color.White;
            this.Icon = Properties.Resources.CompanyIcon;
            this.Name = "frmAdmin";
            this.Text = "Macomber Map Server Administration Console";
            this.grpServers.ResumeLayout(false);
            this.grpUsers.ResumeLayout(false);
            this.splMain.Panel1.ResumeLayout(false);
            this.splMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splMain)).EndInit();
            this.splMain.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox grpServers;
        private System.Windows.Forms.ListView lvMacomberMapServers;
        private System.Windows.Forms.Timer tmrPingServer;
        private System.Windows.Forms.GroupBox grpUsers;
        private System.Windows.Forms.ListView lvUsers;
        private System.Windows.Forms.Timer tmrUpdateUserInfo;
        private System.Windows.Forms.ContextMenuStrip cmsSystemInteraction;
        private System.Windows.Forms.ColumnHeader colValue;
        private System.Windows.Forms.SplitContainer splMain;
        private System.Windows.Forms.Timer tmrRefreshManualServers;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnMergeOperatorship;
        private System.Windows.Forms.ToolStripButton btnCommandHistory;
    }
}

