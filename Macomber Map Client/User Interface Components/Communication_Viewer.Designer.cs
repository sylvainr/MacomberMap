namespace Macomber_Map.User_Interface_Components
{
    partial class Communication_Viewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Communication_Viewer));
            this.lvComm = new System.Windows.Forms.ListView();
            this.tmrUpdate = new System.Windows.Forms.Timer(this.components);
            this.ssLower = new System.Windows.Forms.StatusStrip();
            this.lblCPU = new System.Windows.Forms.ToolStripStatusLabel();
            this.ssMem = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblUptime = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsSpring = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnSpeech = new System.Windows.Forms.ToolStripStatusLabel();
            this.lvQueryStatus = new System.Windows.Forms.ListView();
            this.colQuery = new System.Windows.Forms.ColumnHeader();
            this.colState = new System.Windows.Forms.ColumnHeader();
            this.colLastUpdate = new System.Windows.Forms.ColumnHeader();
            this.lbLog = new System.Windows.Forms.ListBox();
            this.ssUpper = new System.Windows.Forms.StatusStrip();
            this.ssMM = new System.Windows.Forms.ToolStripStatusLabel();
            this.ssEMS = new System.Windows.Forms.ToolStripStatusLabel();
            this.ssMMS = new System.Windows.Forms.ToolStripStatusLabel();
            this.ssHist = new System.Windows.Forms.ToolStripStatusLabel();
            this.tcMain = new System.Windows.Forms.TabControl();
            this.tpServer = new System.Windows.Forms.TabPage();
            this.lbServerStatus = new System.Windows.Forms.ListBox();
            this.tpQueries = new System.Windows.Forms.TabPage();
            this.tpProcesses = new System.Windows.Forms.TabPage();
            this.lbProcesses = new System.Windows.Forms.ListBox();
            this.tpComm = new System.Windows.Forms.TabPage();
            this.tpLog = new System.Windows.Forms.TabPage();
            this.ssLower.SuspendLayout();
            this.ssUpper.SuspendLayout();
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
            this.lvComm.Size = new System.Drawing.Size(454, 229);
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
            this.tsSpring.Size = new System.Drawing.Size(248, 19);
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
            this.colState,
            this.colLastUpdate});
            this.lvQueryStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvQueryStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.lvQueryStatus.ForeColor = System.Drawing.Color.White;
            this.lvQueryStatus.Location = new System.Drawing.Point(3, 3);
            this.lvQueryStatus.Name = "lvQueryStatus";
            this.lvQueryStatus.Size = new System.Drawing.Size(454, 229);
            this.lvQueryStatus.TabIndex = 1;
            this.lvQueryStatus.UseCompatibleStateImageBehavior = false;
            this.lvQueryStatus.View = System.Windows.Forms.View.Details;
            // 
            // colQuery
            // 
            this.colQuery.Text = "Query";
            this.colQuery.Width = 66;
            // 
            // colState
            // 
            this.colState.Text = "Last State";
            this.colState.Width = 92;
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
            this.lbLog.Size = new System.Drawing.Size(454, 225);
            this.lbLog.TabIndex = 0;
            this.lbLog.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lbLog_MouseDoubleClick);
            // 
            // ssUpper
            // 
            this.ssUpper.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ssMM,
            this.ssEMS,
            this.ssMMS,
            this.ssHist});
            this.ssUpper.Location = new System.Drawing.Point(0, 261);
            this.ssUpper.Name = "ssUpper";
            this.ssUpper.ShowItemToolTips = true;
            this.ssUpper.Size = new System.Drawing.Size(468, 24);
            this.ssUpper.SizingGrip = false;
            this.ssUpper.TabIndex = 7;
            // 
            // ssMM
            // 
            this.ssMM.BackColor = System.Drawing.Color.DimGray;
            this.ssMM.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.ssMM.BorderStyle = System.Windows.Forms.Border3DStyle.RaisedOuter;
            this.ssMM.Name = "ssMM";
            this.ssMM.Size = new System.Drawing.Size(36, 19);
            this.ssMM.Text = "CIM:";
            // 
            // ssEMS
            // 
            this.ssEMS.BackColor = System.Drawing.Color.DimGray;
            this.ssEMS.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.ssEMS.BorderStyle = System.Windows.Forms.Border3DStyle.RaisedOuter;
            this.ssEMS.Name = "ssEMS";
            this.ssEMS.Size = new System.Drawing.Size(37, 19);
            this.ssEMS.Text = "EMS:";
            // 
            // ssMMS
            // 
            this.ssMMS.BackColor = System.Drawing.Color.DimGray;
            this.ssMMS.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.ssMMS.BorderStyle = System.Windows.Forms.Border3DStyle.RaisedOuter;
            this.ssMMS.Name = "ssMMS";
            this.ssMMS.Size = new System.Drawing.Size(42, 19);
            this.ssMMS.Text = "MMS:";
            this.ssMMS.Visible = false;
            // 
            // ssHist
            // 
            this.ssHist.BackColor = System.Drawing.Color.DimGray;
            this.ssHist.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.ssHist.BorderStyle = System.Windows.Forms.Border3DStyle.RaisedOuter;
            this.ssHist.Name = "ssHist";
            this.ssHist.Size = new System.Drawing.Size(24, 19);
            this.ssHist.Text = "Historic:";
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
            this.tcMain.Size = new System.Drawing.Size(468, 261);
            this.tcMain.TabIndex = 9;
            // 
            // tpServer
            // 
            this.tpServer.BackColor = System.Drawing.Color.Transparent;
            this.tpServer.Controls.Add(this.lbServerStatus);
            this.tpServer.ForeColor = System.Drawing.Color.White;
            this.tpServer.Location = new System.Drawing.Point(4, 22);
            this.tpServer.Name = "tpServer";
            this.tpServer.Padding = new System.Windows.Forms.Padding(3);
            this.tpServer.Size = new System.Drawing.Size(460, 235);
            this.tpServer.TabIndex = 0;
            this.tpServer.Text = "Server";
            this.tpServer.UseVisualStyleBackColor = true;
            // 
            // lbServerStatus
            // 
            this.lbServerStatus.BackColor = System.Drawing.Color.WhiteSmoke;
            this.lbServerStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbServerStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.lbServerStatus.FormattingEnabled = true;
            this.lbServerStatus.Location = new System.Drawing.Point(3, 3);
            this.lbServerStatus.Name = "lbServerStatus";
            this.lbServerStatus.Size = new System.Drawing.Size(454, 225);
            this.lbServerStatus.TabIndex = 1;
            // 
            // tpQueries
            // 
            this.tpQueries.Controls.Add(this.lvQueryStatus);
            this.tpQueries.Location = new System.Drawing.Point(4, 22);
            this.tpQueries.Name = "tpQueries";
            this.tpQueries.Padding = new System.Windows.Forms.Padding(3);
            this.tpQueries.Size = new System.Drawing.Size(460, 235);
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
            this.tpProcesses.Size = new System.Drawing.Size(460, 235);
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
            this.lbProcesses.Size = new System.Drawing.Size(454, 225);
            this.lbProcesses.TabIndex = 2;
            // 
            // tpComm
            // 
            this.tpComm.Controls.Add(this.lvComm);
            this.tpComm.Location = new System.Drawing.Point(4, 22);
            this.tpComm.Name = "tpComm";
            this.tpComm.Padding = new System.Windows.Forms.Padding(3);
            this.tpComm.Size = new System.Drawing.Size(460, 235);
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
            this.tpLog.Size = new System.Drawing.Size(460, 235);
            this.tpLog.TabIndex = 4;
            this.tpLog.Text = "Log";
            this.tpLog.UseVisualStyleBackColor = true;
            // 
            // Communication_Viewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(468, 309);
            this.Controls.Add(this.tcMain);
            this.Controls.Add(this.ssUpper);
            this.Controls.Add(this.ssLower);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "Communication_Viewer";
            this.Text = " Communications Status";
            this.ssLower.ResumeLayout(false);
            this.ssLower.PerformLayout();
            this.ssUpper.ResumeLayout(false);
            this.ssUpper.PerformLayout();
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
        private System.Windows.Forms.StatusStrip ssUpper;
        private System.Windows.Forms.ToolStripStatusLabel ssMM;
        private System.Windows.Forms.ToolStripStatusLabel ssMMS;
        private System.Windows.Forms.ToolStripStatusLabel ssHist;
        private System.Windows.Forms.ListView lvQueryStatus;
        private System.Windows.Forms.ColumnHeader colQuery;
        private System.Windows.Forms.ColumnHeader colState;
        private System.Windows.Forms.ColumnHeader colLastUpdate;
        private System.Windows.Forms.ToolStripStatusLabel ssEMS;
        private System.Windows.Forms.TabControl tcMain;
        private System.Windows.Forms.TabPage tpServer;
        private System.Windows.Forms.TabPage tpComm;
        private System.Windows.Forms.TabPage tpQueries;
        private System.Windows.Forms.TabPage tpProcesses;
        private System.Windows.Forms.TabPage tpLog;
        private System.Windows.Forms.ToolStripStatusLabel btnSpeech;
        private System.Windows.Forms.ToolStripStatusLabel tsSpring;
        private System.Windows.Forms.ListBox lbServerStatus;
        private System.Windows.Forms.ListBox lbProcesses;
    }
}
