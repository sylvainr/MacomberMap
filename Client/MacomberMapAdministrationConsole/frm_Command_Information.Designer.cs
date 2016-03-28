namespace MacomberMapAdministrationConsole
{
    partial class frm_Command_Information
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frm_Command_Information));
            this.dgvCommandHistory = new System.Windows.Forms.DataGridView();
            this.grpCommands = new System.Windows.Forms.GroupBox();
            this.btnRunSimulation = new System.Windows.Forms.Button();
            this.lblSecPerSec = new System.Windows.Forms.Label();
            this.nudSpeed = new System.Windows.Forms.NumericUpDown();
            this.lblSimulationSpeed = new System.Windows.Forms.Label();
            this.lblEndTime = new System.Windows.Forms.Label();
            this.lblStartTime = new System.Windows.Forms.Label();
            this.dtEndTime = new System.Windows.Forms.DateTimePicker();
            this.dtStartTime = new System.Windows.Forms.DateTimePicker();
            this.splMain = new System.Windows.Forms.SplitContainer();
            this.lblServer = new System.Windows.Forms.Label();
            this.cmbServer = new System.Windows.Forms.ComboBox();
            this.btnLoadCommandsFromLog = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCommandHistory)).BeginInit();
            this.grpCommands.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSpeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splMain)).BeginInit();
            this.splMain.Panel1.SuspendLayout();
            this.splMain.Panel2.SuspendLayout();
            this.splMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvCommandHistory
            // 
            this.dgvCommandHistory.AllowUserToAddRows = false;
            this.dgvCommandHistory.AllowUserToDeleteRows = false;
            this.dgvCommandHistory.AllowUserToOrderColumns = true;
            this.dgvCommandHistory.BackgroundColor = System.Drawing.Color.Black;
            this.dgvCommandHistory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCommandHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvCommandHistory.Location = new System.Drawing.Point(0, 0);
            this.dgvCommandHistory.Name = "dgvCommandHistory";
            this.dgvCommandHistory.ReadOnly = true;
            this.dgvCommandHistory.Size = new System.Drawing.Size(313, 262);
            this.dgvCommandHistory.TabIndex = 0;
            this.dgvCommandHistory.CurrentCellChanged += new System.EventHandler(this.dgvCommandHistory_CurrentCellChanged);
            // 
            // grpCommands
            // 
            this.grpCommands.Controls.Add(this.btnLoadCommandsFromLog);
            this.grpCommands.Controls.Add(this.cmbServer);
            this.grpCommands.Controls.Add(this.lblServer);
            this.grpCommands.Controls.Add(this.btnRunSimulation);
            this.grpCommands.Controls.Add(this.lblSecPerSec);
            this.grpCommands.Controls.Add(this.nudSpeed);
            this.grpCommands.Controls.Add(this.lblSimulationSpeed);
            this.grpCommands.Controls.Add(this.lblEndTime);
            this.grpCommands.Controls.Add(this.lblStartTime);
            this.grpCommands.Controls.Add(this.dtEndTime);
            this.grpCommands.Controls.Add(this.dtStartTime);
            this.grpCommands.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpCommands.ForeColor = System.Drawing.Color.White;
            this.grpCommands.Location = new System.Drawing.Point(0, 0);
            this.grpCommands.Name = "grpCommands";
            this.grpCommands.Size = new System.Drawing.Size(274, 262);
            this.grpCommands.TabIndex = 1;
            this.grpCommands.TabStop = false;
            this.grpCommands.Text = "Simulation";
            // 
            // btnRunSimulation
            // 
            this.btnRunSimulation.ForeColor = System.Drawing.Color.Black;
            this.btnRunSimulation.Location = new System.Drawing.Point(74, 168);
            this.btnRunSimulation.Name = "btnRunSimulation";
            this.btnRunSimulation.Size = new System.Drawing.Size(144, 34);
            this.btnRunSimulation.TabIndex = 7;
            this.btnRunSimulation.Text = "Run Simulation";
            this.btnRunSimulation.UseVisualStyleBackColor = true;
            this.btnRunSimulation.Click += new System.EventHandler(this.btnRunSimulation_Click);
            // 
            // lblSecPerSec
            // 
            this.lblSecPerSec.AutoSize = true;
            this.lblSecPerSec.Location = new System.Drawing.Point(168, 94);
            this.lblSecPerSec.Name = "lblSecPerSec";
            this.lblSecPerSec.Size = new System.Drawing.Size(103, 13);
            this.lblSecPerSec.TabIndex = 6;
            this.lblSecPerSec.Text = "seconds per second";
            // 
            // nudSpeed
            // 
            this.nudSpeed.Location = new System.Drawing.Point(74, 92);
            this.nudSpeed.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudSpeed.Name = "nudSpeed";
            this.nudSpeed.Size = new System.Drawing.Size(88, 20);
            this.nudSpeed.TabIndex = 5;
            this.nudSpeed.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblSimulationSpeed
            // 
            this.lblSimulationSpeed.AutoSize = true;
            this.lblSimulationSpeed.Location = new System.Drawing.Point(10, 94);
            this.lblSimulationSpeed.Name = "lblSimulationSpeed";
            this.lblSimulationSpeed.Size = new System.Drawing.Size(41, 13);
            this.lblSimulationSpeed.TabIndex = 4;
            this.lblSimulationSpeed.Text = "Speed:";
            // 
            // lblEndTime
            // 
            this.lblEndTime.AutoSize = true;
            this.lblEndTime.Location = new System.Drawing.Point(10, 59);
            this.lblEndTime.Name = "lblEndTime";
            this.lblEndTime.Size = new System.Drawing.Size(55, 13);
            this.lblEndTime.TabIndex = 3;
            this.lblEndTime.Text = "End Time:";
            // 
            // lblStartTime
            // 
            this.lblStartTime.AutoSize = true;
            this.lblStartTime.Location = new System.Drawing.Point(10, 23);
            this.lblStartTime.Name = "lblStartTime";
            this.lblStartTime.Size = new System.Drawing.Size(58, 13);
            this.lblStartTime.TabIndex = 2;
            this.lblStartTime.Text = "Start Time:";
            // 
            // dtEndTime
            // 
            this.dtEndTime.CustomFormat = "M/d/yyyy HH:mm:ss";
            this.dtEndTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtEndTime.Location = new System.Drawing.Point(74, 57);
            this.dtEndTime.Name = "dtEndTime";
            this.dtEndTime.ShowCheckBox = true;
            this.dtEndTime.Size = new System.Drawing.Size(189, 20);
            this.dtEndTime.TabIndex = 1;
            // 
            // dtStartTime
            // 
            this.dtStartTime.CustomFormat = "M/d/yyyy HH:mm:ss";
            this.dtStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtStartTime.Location = new System.Drawing.Point(74, 19);
            this.dtStartTime.Name = "dtStartTime";
            this.dtStartTime.ShowCheckBox = true;
            this.dtStartTime.Size = new System.Drawing.Size(189, 20);
            this.dtStartTime.TabIndex = 0;
            // 
            // splMain
            // 
            this.splMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splMain.Location = new System.Drawing.Point(0, 0);
            this.splMain.Name = "splMain";
            // 
            // splMain.Panel1
            // 
            this.splMain.Panel1.Controls.Add(this.dgvCommandHistory);
            // 
            // splMain.Panel2
            // 
            this.splMain.Panel2.Controls.Add(this.grpCommands);
            this.splMain.Size = new System.Drawing.Size(591, 262);
            this.splMain.SplitterDistance = 313;
            this.splMain.TabIndex = 2;
            // 
            // lblServer
            // 
            this.lblServer.AutoSize = true;
            this.lblServer.Location = new System.Drawing.Point(10, 130);
            this.lblServer.Name = "lblServer";
            this.lblServer.Size = new System.Drawing.Size(41, 13);
            this.lblServer.TabIndex = 9;
            this.lblServer.Text = "Server:";
            // 
            // cmbServer
            // 
            this.cmbServer.FormattingEnabled = true;
            this.cmbServer.Location = new System.Drawing.Point(74, 127);
            this.cmbServer.Name = "cmbServer";
            this.cmbServer.Size = new System.Drawing.Size(189, 21);
            this.cmbServer.TabIndex = 10;
            // 
            // btnLoadCommandsFromLog
            // 
            this.btnLoadCommandsFromLog.ForeColor = System.Drawing.Color.Black;
            this.btnLoadCommandsFromLog.Location = new System.Drawing.Point(74, 216);
            this.btnLoadCommandsFromLog.Name = "btnLoadCommandsFromLog";
            this.btnLoadCommandsFromLog.Size = new System.Drawing.Size(144, 34);
            this.btnLoadCommandsFromLog.TabIndex = 11;
            this.btnLoadCommandsFromLog.Text = "Load command list from log";
            this.btnLoadCommandsFromLog.UseVisualStyleBackColor = true;
            this.btnLoadCommandsFromLog.Click += new System.EventHandler(this.btnLoadCommandsFromLog_Click);
            // 
            // frm_Command_Information
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(591, 262);
            this.Controls.Add(this.splMain);
            this.ForeColor = System.Drawing.Color.White;
            this.Icon = Properties.Resources.CompanyIcon;
            this.Name = "frm_Command_Information";
            this.Text = "MM Command Information";
            ((System.ComponentModel.ISupportInitialize)(this.dgvCommandHistory)).EndInit();
            this.grpCommands.ResumeLayout(false);
            this.grpCommands.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSpeed)).EndInit();
            this.splMain.Panel1.ResumeLayout(false);
            this.splMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splMain)).EndInit();
            this.splMain.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvCommandHistory;
        private System.Windows.Forms.GroupBox grpCommands;
        private System.Windows.Forms.DateTimePicker dtStartTime;
        private System.Windows.Forms.SplitContainer splMain;
        private System.Windows.Forms.Label lblSecPerSec;
        private System.Windows.Forms.NumericUpDown nudSpeed;
        private System.Windows.Forms.Label lblSimulationSpeed;
        private System.Windows.Forms.Label lblEndTime;
        private System.Windows.Forms.Label lblStartTime;
        private System.Windows.Forms.DateTimePicker dtEndTime;
        private System.Windows.Forms.Button btnRunSimulation;
        private System.Windows.Forms.ComboBox cmbServer;
        private System.Windows.Forms.Label lblServer;
        private System.Windows.Forms.Button btnLoadCommandsFromLog;
    }
}