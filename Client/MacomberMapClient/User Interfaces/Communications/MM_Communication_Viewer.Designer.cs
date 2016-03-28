using MacomberMapClient.Properties;
namespace MacomberMapClient.User_Interfaces.Communications
{
    partial class MM_Communication_Viewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MM_Communication_Viewer));
            this.lvComm = new System.Windows.Forms.ListView();
            this.tmrUpdate = new System.Windows.Forms.Timer(this.components);
            this.ssLower = new System.Windows.Forms.StatusStrip();
            this.lblServer = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblCPU = new System.Windows.Forms.ToolStripStatusLabel();
            this.ssMem = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblUptime = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsSpring = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnSpeech = new System.Windows.Forms.ToolStripStatusLabel();
            this.lvQueryStatus = new System.Windows.Forms.ListView();
            this.colQuery = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colLastUpdate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lbLog = new System.Windows.Forms.ListBox();
            this.tcMain = new System.Windows.Forms.TabControl();
            this.tpServer = new System.Windows.Forms.TabPage();
            this.lvMacomberMapServers = new System.Windows.Forms.ListView();
            this.colServerName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colUsers = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colPingTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ilImages = new System.Windows.Forms.ImageList(this.components);
            this.tpQueries = new System.Windows.Forms.TabPage();
            this.tpProcesses = new System.Windows.Forms.TabPage();
            this.lbProcesses = new System.Windows.Forms.ListBox();
            this.tpComm = new System.Windows.Forms.TabPage();
            this.tpLog = new System.Windows.Forms.TabPage();
            this.colDescription = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ssLower.SuspendLayout();
            this.tcMain.SuspendLayout();
            this.tpServer.SuspendLayout();
            this.tpQueries.SuspendLayout();
            this.tpProcesses.SuspendLayout();
            this.tpComm.SuspendLayout();
            this.tpLog.SuspendLayout();
            this.SuspendLayout();
            // 
            // lvComm
            // 
            this.lvComm.BackColor = System.Drawing.Color.Black;
            this.lvComm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvComm.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.lvComm.Location = new System.Drawing.Point(3, 3);
            this.lvComm.Name = "lvComm";
            this.lvComm.ShowItemToolTips = true;
            this.lvComm.Size = new System.Drawing.Size(454, 253);
            this.lvComm.TabIndex = 0;
            this.lvComm.TileSize = new System.Drawing.Size(50, 20);
            this.lvComm.UseCompatibleStateImageBehavior = false;
            this.lvComm.View = System.Windows.Forms.View.Tile;
            // 
            // tmrUpdate
            // 
            this.tmrUpdate.Enabled = true;
            this.tmrUpdate.Interval = 1000;
            this.tmrUpdate.Tick += new System.EventHandler(this.UpdateDisplay);
            // 
            // ssLower
            // 
            this.ssLower.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblServer,
            this.lblCPU,
            this.ssMem,
            this.lblUptime,
            this.tsSpring,
            this.btnSpeech});
            this.ssLower.Location = new System.Drawing.Point(0, 285);
            this.ssLower.Name = "ssLower";
            this.ssLower.ShowItemToolTips = true;
            this.ssLower.Size = new System.Drawing.Size(468, 24);
            this.ssLower.TabIndex = 5;
            // 
            // lblServer
            // 
            this.lblServer.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.lblServer.BorderStyle = System.Windows.Forms.Border3DStyle.RaisedOuter;
            this.lblServer.Name = "lblServer";
            this.lblServer.Size = new System.Drawing.Size(46, 19);
            this.lblServer.Text = "Server:";
            // 
            // lblCPU
            // 
            this.lblCPU.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.lblCPU.BorderStyle = System.Windows.Forms.Border3DStyle.RaisedOuter;
            this.lblCPU.Name = "lblCPU";
            this.lblCPU.Size = new System.Drawing.Size(37, 19);
            this.lblCPU.Text = "CPU:";
            // 
            // ssMem
            // 
            this.ssMem.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.ssMem.BorderStyle = System.Windows.Forms.Border3DStyle.RaisedOuter;
            this.ssMem.Name = "ssMem";
            this.ssMem.Size = new System.Drawing.Size(42, 19);
            this.ssMem.Text = "Mem:";
            this.ssMem.DoubleClick += new System.EventHandler(this.ssMem_DoubleClick);
            // 
            // lblUptime
            // 
            this.lblUptime.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.lblUptime.BorderStyle = System.Windows.Forms.Border3DStyle.RaisedOuter;
            this.lblUptime.Name = "lblUptime";
            this.lblUptime.Size = new System.Drawing.Size(53, 19);
            this.lblUptime.Text = "Uptime:";
            // 
            // tsSpring
            // 
            this.tsSpring.Name = "tsSpring";
            this.tsSpring.Size = new System.Drawing.Size(202, 19);
            this.tsSpring.Spring = true;
            // 
            // btnSpeech
            // 
            this.btnSpeech.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.btnSpeech.BorderStyle = System.Windows.Forms.Border3DStyle.RaisedOuter;
            this.btnSpeech.Name = "btnSpeech";
            this.btnSpeech.Size = new System.Drawing.Size(73, 19);
            this.btnSpeech.Text = "Speech OFF";
            this.btnSpeech.Click += new System.EventHandler(this.btnSpeech_Click);
            // 
            // lvQueryStatus
            // 
            this.lvQueryStatus.BackColor = System.Drawing.Color.Black;
            this.lvQueryStatus.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colQuery,
            this.colLastUpdate});
            this.lvQueryStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvQueryStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.lvQueryStatus.ForeColor = System.Drawing.Color.White;
            this.lvQueryStatus.Location = new System.Drawing.Point(3, 3);
            this.lvQueryStatus.Name = "lvQueryStatus";
            this.lvQueryStatus.Size = new System.Drawing.Size(454, 253);
            this.lvQueryStatus.TabIndex = 1;
            this.lvQueryStatus.UseCompatibleStateImageBehavior = false;
            this.lvQueryStatus.View = System.Windows.Forms.View.Details;
            this.lvQueryStatus.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lvQueryStatus_MouseDoubleClick);
            // 
            // colQuery
            // 
            this.colQuery.Text = "Query";
            this.colQuery.Width = 332;
            // 
            // colLastUpdate
            // 
            this.colLastUpdate.Text = "Last Update";
            this.colLastUpdate.Width = 107;
            // 
            // lbLog
            // 
            this.lbLog.BackColor = System.Drawing.Color.WhiteSmoke;
            this.lbLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbLog.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.lbLog.FormattingEnabled = true;
            this.lbLog.Location = new System.Drawing.Point(3, 3);
            this.lbLog.Name = "lbLog";
            this.lbLog.Size = new System.Drawing.Size(454, 253);
            this.lbLog.TabIndex = 0;
            this.lbLog.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lbLog_MouseDoubleClick);
            // 
            // tcMain
            // 
            this.tcMain.Controls.Add(this.tpServer);
            this.tcMain.Controls.Add(this.tpQueries);
            this.tcMain.Controls.Add(this.tpProcesses);
            this.tcMain.Controls.Add(this.tpComm);
            this.tcMain.Controls.Add(this.tpLog);
            this.tcMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcMain.Location = new System.Drawing.Point(0, 0);
            this.tcMain.Name = "tcMain";
            this.tcMain.SelectedIndex = 0;
            this.tcMain.Size = new System.Drawing.Size(468, 285);
            this.tcMain.TabIndex = 9;
            // 
            // tpServer
            // 
            this.tpServer.BackColor = System.Drawing.Color.Transparent;
            this.tpServer.Controls.Add(this.lvMacomberMapServers);
            this.tpServer.ForeColor = System.Drawing.Color.White;
            this.tpServer.Location = new System.Drawing.Point(4, 22);
            this.tpServer.Name = "tpServer";
            this.tpServer.Padding = new System.Windows.Forms.Padding(3);
            this.tpServer.Size = new System.Drawing.Size(460, 259);
            this.tpServer.TabIndex = 0;
            this.tpServer.Text = "Server";
            this.tpServer.UseVisualStyleBackColor = true;
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
            this.lvMacomberMapServers.Location = new System.Drawing.Point(3, 3);
            this.lvMacomberMapServers.Name = "lvMacomberMapServers";
            this.lvMacomberMapServers.Size = new System.Drawing.Size(454, 253);
            this.lvMacomberMapServers.SmallImageList = this.ilImages;
            this.lvMacomberMapServers.TabIndex = 1;
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
            // tpQueries
            // 
            this.tpQueries.Controls.Add(this.lvQueryStatus);
            this.tpQueries.Location = new System.Drawing.Point(4, 22);
            this.tpQueries.Name = "tpQueries";
            this.tpQueries.Padding = new System.Windows.Forms.Padding(3);
            this.tpQueries.Size = new System.Drawing.Size(460, 259);
            this.tpQueries.TabIndex = 2;
            this.tpQueries.Text = "Queries";
            this.tpQueries.UseVisualStyleBackColor = true;
            // 
            // tpProcesses
            // 
            this.tpProcesses.Controls.Add(this.lbProcesses);
            this.tpProcesses.Location = new System.Drawing.Point(4, 22);
            this.tpProcesses.Name = "tpProcesses";
            this.tpProcesses.Padding = new System.Windows.Forms.Padding(3);
            this.tpProcesses.Size = new System.Drawing.Size(460, 259);
            this.tpProcesses.TabIndex = 3;
            this.tpProcesses.Text = "Processes";
            this.tpProcesses.UseVisualStyleBackColor = true;
            // 
            // lbProcesses
            // 
            this.lbProcesses.BackColor = System.Drawing.Color.Black;
            this.lbProcesses.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbProcesses.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.lbProcesses.FormattingEnabled = true;
            this.lbProcesses.Location = new System.Drawing.Point(3, 3);
            this.lbProcesses.Name = "lbProcesses";
            this.lbProcesses.Size = new System.Drawing.Size(454, 253);
            this.lbProcesses.TabIndex = 2;
            // 
            // tpComm
            // 
            this.tpComm.Controls.Add(this.lvComm);
            this.tpComm.Location = new System.Drawing.Point(4, 22);
            this.tpComm.Name = "tpComm";
            this.tpComm.Padding = new System.Windows.Forms.Padding(3);
            this.tpComm.Size = new System.Drawing.Size(460, 259);
            this.tpComm.TabIndex = 1;
            this.tpComm.Text = "ICCP Comm";
            this.tpComm.UseVisualStyleBackColor = true;
            // 
            // tpLog
            // 
            this.tpLog.Controls.Add(this.lbLog);
            this.tpLog.Location = new System.Drawing.Point(4, 22);
            this.tpLog.Name = "tpLog";
            this.tpLog.Padding = new System.Windows.Forms.Padding(3);
            this.tpLog.Size = new System.Drawing.Size(460, 259);
            this.tpLog.TabIndex = 4;
            this.tpLog.Text = "Log";
            this.tpLog.UseVisualStyleBackColor = true;
            // 
            // MM_Communication_Viewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(468, 309);
            this.Controls.Add(this.tcMain);
            this.Controls.Add(this.ssLower);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = Properties.Resources.CompanyIcon;
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "MM_Communication_Viewer";
            this.Text = " Communications Status";
            this.ssLower.ResumeLayout(false);
            this.ssLower.PerformLayout();
            this.tcMain.ResumeLayout(false);
            this.tpServer.ResumeLayout(false);
            this.tpQueries.ResumeLayout(false);
            this.tpProcesses.ResumeLayout(false);
            this.tpComm.ResumeLayout(false);
            this.tpLog.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer tmrUpdate;
        private System.Windows.Forms.StatusStrip ssLower;
        private System.Windows.Forms.ToolStripStatusLabel ssMem;
        private System.Windows.Forms.ToolStripStatusLabel lblCPU;
        private System.Windows.Forms.ListBox lbLog;
        private System.Windows.Forms.ToolStripStatusLabel lblUptime;
        private System.Windows.Forms.ListView lvComm;
        private System.Windows.Forms.ListView lvQueryStatus;
        private System.Windows.Forms.ColumnHeader colQuery;
        private System.Windows.Forms.ColumnHeader colLastUpdate;
        private System.Windows.Forms.TabControl tcMain;
        private System.Windows.Forms.TabPage tpServer;
        private System.Windows.Forms.TabPage tpComm;
        private System.Windows.Forms.TabPage tpQueries;
        private System.Windows.Forms.TabPage tpProcesses;
        private System.Windows.Forms.TabPage tpLog;
        private System.Windows.Forms.ToolStripStatusLabel btnSpeech;
        private System.Windows.Forms.ToolStripStatusLabel tsSpring;
        private System.Windows.Forms.ListBox lbProcesses;
        private System.Windows.Forms.ToolStripStatusLabel lblServer;
        private System.Windows.Forms.ListView lvMacomberMapServers;
        private System.Windows.Forms.ColumnHeader colServerName;
        private System.Windows.Forms.ColumnHeader colUsers;
        private System.Windows.Forms.ColumnHeader colPingTime;
        private System.Windows.Forms.ImageList ilImages;
        private System.Windows.Forms.ColumnHeader colDescription;
    }
}