namespace MacomberMapClient.User_Interfaces.Startup
{
    partial class MM_Startup_Form
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MM_Startup_Form));
            this.lblMMVersion = new System.Windows.Forms.Label();
            this.lblMacomberMap = new System.Windows.Forms.Label();
            this.picLogo = new System.Windows.Forms.PictureBox();
            this.tcMain = new System.Windows.Forms.TabControl();
            this.tpMacomberMapServers = new System.Windows.Forms.TabPage();
            this.lvMacomberMapServers = new System.Windows.Forms.ListView();
            this.colServerName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colUsers = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colDescription = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colPingTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ilImages = new System.Windows.Forms.ImageList(this.components);
            this.tsServers = new System.Windows.Forms.ToolStrip();
            this.btnOptimalServer = new System.Windows.Forms.ToolStripButton();
            this.tpSavecase = new System.Windows.Forms.TabPage();
            this.lvSavecases = new System.Windows.Forms.ListView();
            this.colName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSavedOn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tpProgress = new System.Windows.Forms.TabPage();
            this.lstProgress = new System.Windows.Forms.ListBox();
            this.fileSystemWatcher1 = new System.IO.FileSystemWatcher();
            this.tmrUpdate = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            this.tcMain.SuspendLayout();
            this.tpMacomberMapServers.SuspendLayout();
            this.tsServers.SuspendLayout();
            this.tpSavecase.SuspendLayout();
            this.tpProgress.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcher1)).BeginInit();
            this.SuspendLayout();
            // 
            // lblMMVersion
            // 
            this.lblMMVersion.ForeColor = System.Drawing.Color.White;
            this.lblMMVersion.Location = new System.Drawing.Point(117, 37);
            this.lblMMVersion.Name = "lblMMVersion";
            this.lblMMVersion.Size = new System.Drawing.Size(160, 21);
            this.lblMMVersion.TabIndex = 5;
            this.lblMMVersion.Text = "v{App.Major}.{App.Minor}.{Revision}";
            this.lblMMVersion.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblMacomberMap
            // 
            this.lblMacomberMap.AutoSize = true;
            this.lblMacomberMap.Font = new System.Drawing.Font("Arial", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMacomberMap.ForeColor = System.Drawing.Color.White;
            this.lblMacomberMap.Location = new System.Drawing.Point(113, 13);
            this.lblMacomberMap.Name = "lblMacomberMap";
            this.lblMacomberMap.Size = new System.Drawing.Size(179, 24);
            this.lblMacomberMap.TabIndex = 4;
            this.lblMacomberMap.Text = "Macomber Map®";
            // 
            // picLogo
            // 
            this.picLogo.Image = global::MacomberMapClient.Properties.Resources.CompanyLogo;
            this.picLogo.Location = new System.Drawing.Point(12, 12);
            this.picLogo.Name = "picLogo";
            this.picLogo.Size = new System.Drawing.Size(95, 38);
            this.picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picLogo.TabIndex = 3;
            this.picLogo.TabStop = false;
            // 
            // tcMain
            // 
            this.tcMain.Controls.Add(this.tpMacomberMapServers);
            this.tcMain.Controls.Add(this.tpSavecase);
            this.tcMain.Controls.Add(this.tpProgress);
            this.tcMain.Location = new System.Drawing.Point(12, 61);
            this.tcMain.Name = "tcMain";
            this.tcMain.SelectedIndex = 0;
            this.tcMain.Size = new System.Drawing.Size(280, 260);
            this.tcMain.TabIndex = 6;
            // 
            // tpMacomberMapServers
            // 
            this.tpMacomberMapServers.BackColor = System.Drawing.Color.Black;
            this.tpMacomberMapServers.Controls.Add(this.lvMacomberMapServers);
            this.tpMacomberMapServers.Controls.Add(this.tsServers);
            this.tpMacomberMapServers.Location = new System.Drawing.Point(4, 22);
            this.tpMacomberMapServers.Name = "tpMacomberMapServers";
            this.tpMacomberMapServers.Padding = new System.Windows.Forms.Padding(3);
            this.tpMacomberMapServers.Size = new System.Drawing.Size(272, 234);
            this.tpMacomberMapServers.TabIndex = 0;
            this.tpMacomberMapServers.Text = "MM Servers";
            // 
            // lvMacomberMapServers
            // 
            this.lvMacomberMapServers.BackColor = System.Drawing.Color.Black;
            this.lvMacomberMapServers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colServerName,
            this.colUsers,
            this.colDescription,
            this.colPingTime});
            this.lvMacomberMapServers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvMacomberMapServers.ForeColor = System.Drawing.Color.White;
            this.lvMacomberMapServers.LargeImageList = this.ilImages;
            this.lvMacomberMapServers.Location = new System.Drawing.Point(3, 28);
            this.lvMacomberMapServers.Name = "lvMacomberMapServers";
            this.lvMacomberMapServers.Size = new System.Drawing.Size(266, 203);
            this.lvMacomberMapServers.SmallImageList = this.ilImages;
            this.lvMacomberMapServers.TabIndex = 0;
            this.lvMacomberMapServers.TileSize = new System.Drawing.Size(216, 70);
            this.lvMacomberMapServers.UseCompatibleStateImageBehavior = false;
            this.lvMacomberMapServers.View = System.Windows.Forms.View.Tile;
            this.lvMacomberMapServers.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lvMacomberMapServers_MouseDoubleClick);
            // 
            // colServerName
            // 
            this.colServerName.Text = "Server";
            // 
            // colUsers
            // 
            this.colUsers.Text = "Users";
            // 
            // colPingTime
            // 
            this.colPingTime.Text = "Ping";
            // 
            // ilImages
            // 
            this.ilImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilImages.ImageStream")));
            this.ilImages.TransparentColor = System.Drawing.Color.Transparent;
            this.ilImages.Images.SetKeyName(0, "Network_ConnectTo.png");
            // 
            // tsServers
            // 
            this.tsServers.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnOptimalServer});
            this.tsServers.Location = new System.Drawing.Point(3, 3);
            this.tsServers.Name = "tsServers";
            this.tsServers.Size = new System.Drawing.Size(266, 25);
            this.tsServers.TabIndex = 1;
            this.tsServers.Text = "toolStrip1";
            // 
            // btnOptimalServer
            // 
            this.btnOptimalServer.ForeColor = System.Drawing.Color.Black;
            this.btnOptimalServer.Image = global::MacomberMapClient.Properties.Resources.Network_ConnectTo;
            this.btnOptimalServer.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnOptimalServer.Name = "btnOptimalServer";
            this.btnOptimalServer.Size = new System.Drawing.Size(164, 22);
            this.btnOptimalServer.Text = "Connect to optimal server";
            this.btnOptimalServer.Click += new System.EventHandler(this.btnOptimalServer_Click);
            // 
            // tpSavecase
            // 
            this.tpSavecase.BackColor = System.Drawing.Color.Black;
            this.tpSavecase.Controls.Add(this.lvSavecases);
            this.tpSavecase.Location = new System.Drawing.Point(4, 22);
            this.tpSavecase.Name = "tpSavecase";
            this.tpSavecase.Padding = new System.Windows.Forms.Padding(3);
            this.tpSavecase.Size = new System.Drawing.Size(272, 234);
            this.tpSavecase.TabIndex = 1;
            this.tpSavecase.Text = "Savecase";
            // 
            // lvSavecases
            // 
            this.lvSavecases.BackColor = System.Drawing.Color.Black;
            this.lvSavecases.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName,
            this.colSize,
            this.colSavedOn});
            this.lvSavecases.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvSavecases.ForeColor = System.Drawing.Color.White;
            this.lvSavecases.FullRowSelect = true;
            this.lvSavecases.Location = new System.Drawing.Point(3, 3);
            this.lvSavecases.Name = "lvSavecases";
            this.lvSavecases.Size = new System.Drawing.Size(266, 228);
            this.lvSavecases.TabIndex = 0;
            this.lvSavecases.UseCompatibleStateImageBehavior = false;
            this.lvSavecases.View = System.Windows.Forms.View.Details;
            // 
            // colName
            // 
            this.colName.Text = "Name";
            // 
            // colSize
            // 
            this.colSize.Text = "Size";
            // 
            // colSavedOn
            // 
            this.colSavedOn.Text = "Saved On";
            // 
            // tpProgress
            // 
            this.tpProgress.BackColor = System.Drawing.Color.Black;
            this.tpProgress.Controls.Add(this.lstProgress);
            this.tpProgress.Location = new System.Drawing.Point(4, 22);
            this.tpProgress.Name = "tpProgress";
            this.tpProgress.Padding = new System.Windows.Forms.Padding(3);
            this.tpProgress.Size = new System.Drawing.Size(272, 234);
            this.tpProgress.TabIndex = 2;
            this.tpProgress.Text = "Progress";
            // 
            // lstProgress
            // 
            this.lstProgress.BackColor = System.Drawing.Color.Black;
            this.lstProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstProgress.ForeColor = System.Drawing.Color.White;
            this.lstProgress.FormattingEnabled = true;
            this.lstProgress.Location = new System.Drawing.Point(3, 3);
            this.lstProgress.Name = "lstProgress";
            this.lstProgress.Size = new System.Drawing.Size(266, 228);
            this.lstProgress.TabIndex = 1;
            // 
            // fileSystemWatcher1
            // 
            this.fileSystemWatcher1.EnableRaisingEvents = true;
            this.fileSystemWatcher1.SynchronizingObject = this;
            // 
            // tmrUpdate
            // 
            this.tmrUpdate.Enabled = true;
            this.tmrUpdate.Interval = 500;
            this.tmrUpdate.Tick += new System.EventHandler(this.tmrUpdate_Tick);
            // 
            // MM_Startup_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(307, 333);
            this.Controls.Add(this.tcMain);
            this.Controls.Add(this.lblMMVersion);
            this.Controls.Add(this.lblMacomberMap);
            this.Controls.Add(this.picLogo);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = global::MacomberMapClient.Properties.Resources.CompanyIcon;
            this.Name = "MM_Startup_Form";
            this.Text = "Macomber Map®";
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.tcMain.ResumeLayout(false);
            this.tpMacomberMapServers.ResumeLayout(false);
            this.tpMacomberMapServers.PerformLayout();
            this.tsServers.ResumeLayout(false);
            this.tsServers.PerformLayout();
            this.tpSavecase.ResumeLayout(false);
            this.tpProgress.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcher1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblMMVersion;
        private System.Windows.Forms.Label lblMacomberMap;
        private System.Windows.Forms.PictureBox picLogo;
        private System.Windows.Forms.TabControl tcMain;
        private System.Windows.Forms.TabPage tpMacomberMapServers;
        private System.Windows.Forms.TabPage tpSavecase;
        private System.Windows.Forms.ListView lvMacomberMapServers;
        private System.IO.FileSystemWatcher fileSystemWatcher1;
        private System.Windows.Forms.ImageList ilImages;
        private System.Windows.Forms.ColumnHeader colServerName;
        private System.Windows.Forms.ColumnHeader colUsers;
        private System.Windows.Forms.ColumnHeader colPingTime;
        private System.Windows.Forms.ListView lvSavecases;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colSize;
        private System.Windows.Forms.ColumnHeader colSavedOn;
        private System.Windows.Forms.ToolStrip tsServers;
        private System.Windows.Forms.ToolStripButton btnOptimalServer;
        private System.Windows.Forms.TabPage tpProgress;
        private System.Windows.Forms.Timer tmrUpdate;
        private System.Windows.Forms.ListBox lstProgress;
        private System.Windows.Forms.ColumnHeader colDescription;
    }
}