namespace Macomber_Map.User_Interface_Components
{
    partial class Startup_Form
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Startup_Form));
            this.lblMacomberMap = new System.Windows.Forms.Label();
            this.lblMMVersion = new System.Windows.Forms.Label();
            this.picLogo = new System.Windows.Forms.PictureBox();
            this.tmrUpdate = new System.Windows.Forms.Timer(this.components);
            this.btnOK = new System.Windows.Forms.Button();
            this.tabPages = new Macomber_Map.User_Interface_Components.Overrides.MM_TabControl();
            this.tabDataConnections = new Macomber_Map.User_Interface_Components.Overrides.MM_TabPage();
            this.chkDisplayAlarmsStart = new System.Windows.Forms.CheckBox();
            this.chkRecordTelemetryChanges = new System.Windows.Forms.CheckBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.txtUserName = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.lblUserName = new System.Windows.Forms.Label();
            this.chkPipe = new System.Windows.Forms.CheckBox();
            this.chkSpeakViolations = new System.Windows.Forms.CheckBox();
            this.cmbDataSource = new System.Windows.Forms.ComboBox();
            this.lblDataSource = new System.Windows.Forms.Label();
            this.cmbNetworkMap = new System.Windows.Forms.ComboBox();
            this.lblDisplay = new System.Windows.Forms.Label();
            this.cmbHistoryServer = new System.Windows.Forms.ComboBox();
            this.lblHistoryServer = new System.Windows.Forms.Label();
            this.cmbMMServer = new System.Windows.Forms.ComboBox();
            this.lblMMServer = new System.Windows.Forms.Label();
            this.chkWeatherData = new System.Windows.Forms.CheckBox();
            this.chkLog = new System.Windows.Forms.CheckBox();
            this.cmbMMSServer = new System.Windows.Forms.ComboBox();
            this.lblMMS = new System.Windows.Forms.Label();
            this.tabSavecase = new Macomber_Map.User_Interface_Components.Overrides.MM_TabPage();
            this.cmbHistoryServer2 = new System.Windows.Forms.ComboBox();
            this.lblHistoryServer2 = new System.Windows.Forms.Label();
            this.cmbNetworkMap2 = new System.Windows.Forms.ComboBox();
            this.lblNetworkMap2 = new System.Windows.Forms.Label();
            this.btnSavecase = new System.Windows.Forms.Button();
            this.lblLoadSavecase = new System.Windows.Forms.Label();
            this.tabProgress = new Macomber_Map.User_Interface_Components.Overrides.MM_TabPage();
            this.lstProgress = new System.Windows.Forms.ListBox();
            this.grpDiagnostics = new System.Windows.Forms.GroupBox();
            this.btnRemoveSensitiveInformation = new System.Windows.Forms.Button();
            this.lblRemoveSensitiveInformation = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            this.tabPages.SuspendLayout();
            this.tabDataConnections.SuspendLayout();
            this.tabSavecase.SuspendLayout();
            this.tabProgress.SuspendLayout();
            this.grpDiagnostics.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblMacomberMap
            // 
            this.lblMacomberMap.AutoSize = true;
            this.lblMacomberMap.Font = new System.Drawing.Font("Arial", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMacomberMap.ForeColor = System.Drawing.Color.White;
            this.lblMacomberMap.Location = new System.Drawing.Point(113, 8);
            this.lblMacomberMap.Name = "lblMacomberMap";
            this.lblMacomberMap.Size = new System.Drawing.Size(179, 24);
            this.lblMacomberMap.TabIndex = 1;
            this.lblMacomberMap.Text = "Macomber Map®";
            // 
            // lblMMVersion
            // 
            this.lblMMVersion.ForeColor = System.Drawing.Color.White;
            this.lblMMVersion.Location = new System.Drawing.Point(117, 32);
            this.lblMMVersion.Name = "lblMMVersion";
            this.lblMMVersion.Size = new System.Drawing.Size(160, 21);
            this.lblMMVersion.TabIndex = 2;
            this.lblMMVersion.Text = "v{App.Major}.{App.Minor}.{Revision}";
            this.lblMMVersion.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // picLogo
            // 
            this.picLogo.Image = global::Macomber_Map.Properties.Resources.ERCOT_Logo;
            this.picLogo.Location = new System.Drawing.Point(12, 7);
            this.picLogo.Name = "picLogo";
            this.picLogo.Size = new System.Drawing.Size(95, 38);
            this.picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picLogo.TabIndex = 0;
            this.picLogo.TabStop = false;
            this.picLogo.Click += new System.EventHandler(this.picLogo_Click);
            // 
            // tmrUpdate
            // 
            this.tmrUpdate.Enabled = true;
            this.tmrUpdate.Interval = 500;
            this.tmrUpdate.Tick += new System.EventHandler(this.tmrUpdate_Tick);
            // 
            // btnOK
            // 
            this.btnOK.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnOK.ForeColor = System.Drawing.Color.Black;
            this.btnOK.Location = new System.Drawing.Point(201, 279);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 11;
            this.btnOK.Text = "Connect";
            this.btnOK.UseVisualStyleBackColor = false;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // tabPages
            // 
            this.tabPages.Controls.Add(this.tabDataConnections);
            this.tabPages.Controls.Add(this.tabSavecase);
            this.tabPages.Controls.Add(this.tabProgress);
            this.tabPages.Location = new System.Drawing.Point(12, 56);
            this.tabPages.Name = "tabPages";
            this.tabPages.SelectedIndex = 0;
            this.tabPages.Size = new System.Drawing.Size(299, 336);
            this.tabPages.TabIndex = 12;
            // 
            // tabDataConnections
            // 
            this.tabDataConnections.BackColor = System.Drawing.Color.Black;
            this.tabDataConnections.Controls.Add(this.chkDisplayAlarmsStart);
            this.tabDataConnections.Controls.Add(this.chkRecordTelemetryChanges);
            this.tabDataConnections.Controls.Add(this.txtPassword);
            this.tabDataConnections.Controls.Add(this.txtUserName);
            this.tabDataConnections.Controls.Add(this.lblPassword);
            this.tabDataConnections.Controls.Add(this.lblUserName);
            this.tabDataConnections.Controls.Add(this.chkPipe);
            this.tabDataConnections.Controls.Add(this.chkSpeakViolations);
            this.tabDataConnections.Controls.Add(this.cmbDataSource);
            this.tabDataConnections.Controls.Add(this.lblDataSource);
            this.tabDataConnections.Controls.Add(this.cmbNetworkMap);
            this.tabDataConnections.Controls.Add(this.lblDisplay);
            this.tabDataConnections.Controls.Add(this.cmbHistoryServer);
            this.tabDataConnections.Controls.Add(this.lblHistoryServer);
            this.tabDataConnections.Controls.Add(this.cmbMMServer);
            this.tabDataConnections.Controls.Add(this.btnOK);
            this.tabDataConnections.Controls.Add(this.lblMMServer);
            this.tabDataConnections.Controls.Add(this.chkWeatherData);
            this.tabDataConnections.Controls.Add(this.chkLog);
            this.tabDataConnections.Controls.Add(this.cmbMMSServer);
            this.tabDataConnections.Controls.Add(this.lblMMS);
            this.tabDataConnections.Location = new System.Drawing.Point(4, 22);
            this.tabDataConnections.Name = "tabDataConnections";
            this.tabDataConnections.Padding = new System.Windows.Forms.Padding(3);
            this.tabDataConnections.Size = new System.Drawing.Size(291, 310);
            this.tabDataConnections.TabIndex = 0;
            this.tabDataConnections.Text = "Data connections (inactive)";
            this.tabDataConnections.UseVisualStyleBackColor = true;
            // 
            // chkDisplayAlarmsStart
            // 
            this.chkDisplayAlarmsStart.AutoSize = true;
            this.chkDisplayAlarmsStart.Location = new System.Drawing.Point(18, 285);
            this.chkDisplayAlarmsStart.Name = "chkDisplayAlarmsStart";
            this.chkDisplayAlarmsStart.Size = new System.Drawing.Size(94, 17);
            this.chkDisplayAlarmsStart.TabIndex = 25;
            this.chkDisplayAlarmsStart.Text = "Display Alarms";
            this.chkDisplayAlarmsStart.UseVisualStyleBackColor = true;
            // 
            // chkRecordTelemetryChanges
            // 
            this.chkRecordTelemetryChanges.AutoSize = true;
            this.chkRecordTelemetryChanges.Location = new System.Drawing.Point(18, 263);
            this.chkRecordTelemetryChanges.Name = "chkRecordTelemetryChanges";
            this.chkRecordTelemetryChanges.Size = new System.Drawing.Size(154, 17);
            this.chkRecordTelemetryChanges.TabIndex = 24;
            this.chkRecordTelemetryChanges.Text = "Record Telemetry changes";
            this.chkRecordTelemetryChanges.UseVisualStyleBackColor = true;
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(101, 69);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '•';
            this.txtPassword.Size = new System.Drawing.Size(153, 20);
            this.txtPassword.TabIndex = 23;
            // 
            // txtUserName
            // 
            this.txtUserName.Location = new System.Drawing.Point(101, 36);
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.Size = new System.Drawing.Size(153, 20);
            this.txtUserName.TabIndex = 22;
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(42, 72);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(56, 13);
            this.lblPassword.TabIndex = 21;
            this.lblPassword.Text = "Password:";
            // 
            // lblUserName
            // 
            this.lblUserName.AutoSize = true;
            this.lblUserName.Location = new System.Drawing.Point(42, 39);
            this.lblUserName.Name = "lblUserName";
            this.lblUserName.Size = new System.Drawing.Size(32, 13);
            this.lblUserName.TabIndex = 20;
            this.lblUserName.Text = "User:";
            // 
            // chkPipe
            // 
            this.chkPipe.AutoSize = true;
            this.chkPipe.Location = new System.Drawing.Point(162, 240);
            this.chkPipe.Name = "chkPipe";
            this.chkPipe.Size = new System.Drawing.Size(108, 17);
            this.chkPipe.TabIndex = 19;
            this.chkPipe.Text = "EMS->MM Comm";
            this.chkPipe.UseVisualStyleBackColor = true;
            this.chkPipe.CheckedChanged += new System.EventHandler(this.chkPipe_CheckedChanged);
            // 
            // chkSpeakViolations
            // 
            this.chkSpeakViolations.AutoSize = true;
            this.chkSpeakViolations.Location = new System.Drawing.Point(162, 217);
            this.chkSpeakViolations.Name = "chkSpeakViolations";
            this.chkSpeakViolations.Size = new System.Drawing.Size(105, 17);
            this.chkSpeakViolations.TabIndex = 18;
            this.chkSpeakViolations.Text = "Speak Violations";
            this.chkSpeakViolations.UseVisualStyleBackColor = true;
            this.chkSpeakViolations.CheckedChanged += new System.EventHandler(this.chkSpeakViolations_CheckedChanged);
            // 
            // cmbDataSource
            // 
            this.cmbDataSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDataSource.FormattingEnabled = true;
            this.cmbDataSource.Location = new System.Drawing.Point(87, 189);
            this.cmbDataSource.Name = "cmbDataSource";
            this.cmbDataSource.Size = new System.Drawing.Size(189, 21);
            this.cmbDataSource.TabIndex = 17;
            // 
            // lblDataSource
            // 
            this.lblDataSource.AutoSize = true;
            this.lblDataSource.Location = new System.Drawing.Point(11, 192);
            this.lblDataSource.Name = "lblDataSource";
            this.lblDataSource.Size = new System.Drawing.Size(70, 13);
            this.lblDataSource.TabIndex = 16;
            this.lblDataSource.Text = "Data Source:";
            // 
            // cmbNetworkMap
            // 
            this.cmbNetworkMap.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbNetworkMap.FormattingEnabled = true;
            this.cmbNetworkMap.Items.AddRange(new object[] {
            "DirectX",
            "GDI+",
            "GDI+ (Adaptive Rendering)",
            "WPF"});
            this.cmbNetworkMap.Location = new System.Drawing.Point(87, 162);
            this.cmbNetworkMap.Name = "cmbNetworkMap";
            this.cmbNetworkMap.Size = new System.Drawing.Size(189, 21);
            this.cmbNetworkMap.TabIndex = 15;
            // 
            // lblDisplay
            // 
            this.lblDisplay.AutoSize = true;
            this.lblDisplay.Location = new System.Drawing.Point(11, 165);
            this.lblDisplay.Name = "lblDisplay";
            this.lblDisplay.Size = new System.Drawing.Size(73, 13);
            this.lblDisplay.TabIndex = 14;
            this.lblDisplay.Text = "Network map:";
            // 
            // cmbHistoryServer
            // 
            this.cmbHistoryServer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbHistoryServer.FormattingEnabled = true;
            this.cmbHistoryServer.Location = new System.Drawing.Point(87, 135);
            this.cmbHistoryServer.Name = "cmbHistoryServer";
            this.cmbHistoryServer.Size = new System.Drawing.Size(189, 21);
            this.cmbHistoryServer.TabIndex = 13;
            this.cmbHistoryServer.SelectedIndexChanged += new System.EventHandler(this.UpdateDataSources);
            // 
            // lblHistoryServer
            // 
            this.lblHistoryServer.AutoSize = true;
            this.lblHistoryServer.Location = new System.Drawing.Point(11, 138);
            this.lblHistoryServer.Name = "lblHistoryServer";
            this.lblHistoryServer.Size = new System.Drawing.Size(76, 13);
            this.lblHistoryServer.TabIndex = 12;
            this.lblHistoryServer.Text = "History Server:";
            // 
            // cmbMMServer
            // 
            this.cmbMMServer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMMServer.FormattingEnabled = true;
            this.cmbMMServer.Location = new System.Drawing.Point(84, 6);
            this.cmbMMServer.Name = "cmbMMServer";
            this.cmbMMServer.Size = new System.Drawing.Size(192, 21);
            this.cmbMMServer.TabIndex = 4;
            this.cmbMMServer.SelectedIndexChanged += new System.EventHandler(this.UpdateDataSources);
            // 
            // lblMMServer
            // 
            this.lblMMServer.AutoSize = true;
            this.lblMMServer.Location = new System.Drawing.Point(15, 9);
            this.lblMMServer.Name = "lblMMServer";
            this.lblMMServer.Size = new System.Drawing.Size(62, 13);
            this.lblMMServer.TabIndex = 3;
            this.lblMMServer.Text = "MM Server:";
            // 
            // chkWeatherData
            // 
            this.chkWeatherData.AutoSize = true;
            this.chkWeatherData.Location = new System.Drawing.Point(18, 240);
            this.chkWeatherData.Name = "chkWeatherData";
            this.chkWeatherData.Size = new System.Drawing.Size(144, 17);
            this.chkWeatherData.TabIndex = 10;
            this.chkWeatherData.Text = "Download Weather Data";
            this.chkWeatherData.UseVisualStyleBackColor = true;
            // 
            // chkLog
            // 
            this.chkLog.AutoSize = true;
            this.chkLog.Location = new System.Drawing.Point(18, 217);
            this.chkLog.Name = "chkLog";
            this.chkLog.Size = new System.Drawing.Size(90, 17);
            this.chkLog.TabIndex = 9;
            this.chkLog.Text = "Create log file";
            this.chkLog.UseVisualStyleBackColor = true;
            // 
            // cmbMMSServer
            // 
            this.cmbMMSServer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMMSServer.FormattingEnabled = true;
            this.cmbMMSServer.Location = new System.Drawing.Point(87, 108);
            this.cmbMMSServer.Name = "cmbMMSServer";
            this.cmbMMSServer.Size = new System.Drawing.Size(189, 21);
            this.cmbMMSServer.TabIndex = 8;
            this.cmbMMSServer.SelectedIndexChanged += new System.EventHandler(this.UpdateDataSources);
            // 
            // lblMMS
            // 
            this.lblMMS.AutoSize = true;
            this.lblMMS.Location = new System.Drawing.Point(11, 111);
            this.lblMMS.Name = "lblMMS";
            this.lblMMS.Size = new System.Drawing.Size(69, 13);
            this.lblMMS.TabIndex = 7;
            this.lblMMS.Text = "MMS Server:";
            // 
            // tabSavecase
            // 
            this.tabSavecase.BackColor = System.Drawing.Color.Black;
            this.tabSavecase.Controls.Add(this.grpDiagnostics);
            this.tabSavecase.Controls.Add(this.cmbHistoryServer2);
            this.tabSavecase.Controls.Add(this.lblHistoryServer2);
            this.tabSavecase.Controls.Add(this.cmbNetworkMap2);
            this.tabSavecase.Controls.Add(this.lblNetworkMap2);
            this.tabSavecase.Controls.Add(this.btnSavecase);
            this.tabSavecase.Controls.Add(this.lblLoadSavecase);
            this.tabSavecase.Location = new System.Drawing.Point(4, 22);
            this.tabSavecase.Name = "tabSavecase";
            this.tabSavecase.Padding = new System.Windows.Forms.Padding(3);
            this.tabSavecase.Size = new System.Drawing.Size(291, 310);
            this.tabSavecase.TabIndex = 1;
            this.tabSavecase.Text = "Savecase";
            this.tabSavecase.UseVisualStyleBackColor = true;
            // 
            // cmbHistoryServer2
            // 
            this.cmbHistoryServer2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbHistoryServer2.FormattingEnabled = true;
            this.cmbHistoryServer2.Location = new System.Drawing.Point(89, 60);
            this.cmbHistoryServer2.Name = "cmbHistoryServer2";
            this.cmbHistoryServer2.Size = new System.Drawing.Size(196, 21);
            this.cmbHistoryServer2.TabIndex = 19;
            // 
            // lblHistoryServer2
            // 
            this.lblHistoryServer2.AutoSize = true;
            this.lblHistoryServer2.Location = new System.Drawing.Point(6, 63);
            this.lblHistoryServer2.Name = "lblHistoryServer2";
            this.lblHistoryServer2.Size = new System.Drawing.Size(76, 13);
            this.lblHistoryServer2.TabIndex = 18;
            this.lblHistoryServer2.Text = "History Server:";
            // 
            // cmbNetworkMap2
            // 
            this.cmbNetworkMap2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbNetworkMap2.FormattingEnabled = true;
            this.cmbNetworkMap2.Items.AddRange(new object[] {
            "DirectX",
            "GDI+",
            "GDI+ (Adaptive Rendering)",
            "WPF"});
            this.cmbNetworkMap2.Location = new System.Drawing.Point(89, 20);
            this.cmbNetworkMap2.Name = "cmbNetworkMap2";
            this.cmbNetworkMap2.Size = new System.Drawing.Size(196, 21);
            this.cmbNetworkMap2.TabIndex = 17;
            // 
            // lblNetworkMap2
            // 
            this.lblNetworkMap2.AutoSize = true;
            this.lblNetworkMap2.Location = new System.Drawing.Point(6, 23);
            this.lblNetworkMap2.Name = "lblNetworkMap2";
            this.lblNetworkMap2.Size = new System.Drawing.Size(73, 13);
            this.lblNetworkMap2.TabIndex = 16;
            this.lblNetworkMap2.Text = "Network map:";
            // 
            // btnSavecase
            // 
            this.btnSavecase.Image = global::Macomber_Map.Properties.Resources.openHS;
            this.btnSavecase.Location = new System.Drawing.Point(104, 95);
            this.btnSavecase.Name = "btnSavecase";
            this.btnSavecase.Size = new System.Drawing.Size(32, 23);
            this.btnSavecase.TabIndex = 1;
            this.btnSavecase.UseVisualStyleBackColor = true;
            this.btnSavecase.Click += new System.EventHandler(this.btnSavecase_Click);
            // 
            // lblLoadSavecase
            // 
            this.lblLoadSavecase.AutoSize = true;
            this.lblLoadSavecase.Location = new System.Drawing.Point(6, 100);
            this.lblLoadSavecase.Name = "lblLoadSavecase";
            this.lblLoadSavecase.Size = new System.Drawing.Size(92, 13);
            this.lblLoadSavecase.TabIndex = 0;
            this.lblLoadSavecase.Text = "Load a savecase:";
            // 
            // tabProgress
            // 
            this.tabProgress.BackColor = System.Drawing.Color.Black;
            this.tabProgress.Controls.Add(this.lstProgress);
            this.tabProgress.Location = new System.Drawing.Point(4, 22);
            this.tabProgress.Name = "tabProgress";
            this.tabProgress.Size = new System.Drawing.Size(291, 310);
            this.tabProgress.TabIndex = 2;
            this.tabProgress.Text = "Progress";
            this.tabProgress.UseVisualStyleBackColor = true;
            // 
            // lstProgress
            // 
            this.lstProgress.BackColor = System.Drawing.Color.Black;
            this.lstProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstProgress.ForeColor = System.Drawing.Color.White;
            this.lstProgress.FormattingEnabled = true;
            this.lstProgress.Location = new System.Drawing.Point(0, 0);
            this.lstProgress.Name = "lstProgress";
            this.lstProgress.Size = new System.Drawing.Size(291, 310);
            this.lstProgress.TabIndex = 0;
            // 
            // grpDiagnostics
            // 
            this.grpDiagnostics.Controls.Add(this.btnRemoveSensitiveInformation);
            this.grpDiagnostics.Controls.Add(this.lblRemoveSensitiveInformation);
            this.grpDiagnostics.ForeColor = System.Drawing.Color.White;
            this.grpDiagnostics.Location = new System.Drawing.Point(10, 252);
            this.grpDiagnostics.Name = "grpDiagnostics";
            this.grpDiagnostics.Size = new System.Drawing.Size(276, 52);
            this.grpDiagnostics.TabIndex = 20;
            this.grpDiagnostics.TabStop = false;
            this.grpDiagnostics.Text = "Diagnostics";
            // 
            // btnRemoveSensitiveInformation
            // 
            this.btnRemoveSensitiveInformation.Image = global::Macomber_Map.Properties.Resources.openHS;
            this.btnRemoveSensitiveInformation.Location = new System.Drawing.Point(220, 19);
            this.btnRemoveSensitiveInformation.Name = "btnRemoveSensitiveInformation";
            this.btnRemoveSensitiveInformation.Size = new System.Drawing.Size(32, 23);
            this.btnRemoveSensitiveInformation.TabIndex = 22;
            this.btnRemoveSensitiveInformation.UseVisualStyleBackColor = true;
            this.btnRemoveSensitiveInformation.Click += new System.EventHandler(this.btnRemoveSensitiveInformation_Click);
            // 
            // lblRemoveSensitiveInformation
            // 
            this.lblRemoveSensitiveInformation.AutoSize = true;
            this.lblRemoveSensitiveInformation.Location = new System.Drawing.Point(12, 24);
            this.lblRemoveSensitiveInformation.Name = "lblRemoveSensitiveInformation";
            this.lblRemoveSensitiveInformation.Size = new System.Drawing.Size(202, 13);
            this.lblRemoveSensitiveInformation.TabIndex = 21;
            this.lblRemoveSensitiveInformation.Text = "Remove sensitive information from model:";
            // 
            // Startup_Form
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(314, 400);
            this.Controls.Add(this.tabPages);
            this.Controls.Add(this.lblMMVersion);
            this.Controls.Add(this.lblMacomberMap);
            this.Controls.Add(this.picLogo);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Startup_Form";
            this.Text = " ERCOT Macomber Map®";
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.tabPages.ResumeLayout(false);
            this.tabDataConnections.ResumeLayout(false);
            this.tabDataConnections.PerformLayout();
            this.tabSavecase.ResumeLayout(false);
            this.tabSavecase.PerformLayout();
            this.tabProgress.ResumeLayout(false);
            this.grpDiagnostics.ResumeLayout(false);
            this.grpDiagnostics.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picLogo;
        private System.Windows.Forms.Label lblMacomberMap;
        private System.Windows.Forms.Label lblMMVersion;
        private System.Windows.Forms.Label lblMMServer;
        private System.Windows.Forms.ComboBox cmbMMServer;
        private System.Windows.Forms.ComboBox cmbMMSServer;
        private System.Windows.Forms.Label lblMMS;
        private System.Windows.Forms.CheckBox chkLog;
        private System.Windows.Forms.CheckBox chkWeatherData;
        private System.Windows.Forms.Button btnOK;
        private Macomber_Map.User_Interface_Components.Overrides.MM_TabControl tabPages;
        private Macomber_Map.User_Interface_Components.Overrides.MM_TabPage tabDataConnections;
        private Macomber_Map.User_Interface_Components.Overrides.MM_TabPage tabSavecase;
        private System.Windows.Forms.Button btnSavecase;
        private System.Windows.Forms.Label lblLoadSavecase;
        private Macomber_Map.User_Interface_Components.Overrides.MM_TabPage tabProgress;
        private System.Windows.Forms.ListBox lstProgress;
        private System.Windows.Forms.ComboBox cmbNetworkMap;
        private System.Windows.Forms.Label lblDisplay;
        private System.Windows.Forms.ComboBox cmbHistoryServer;
        private System.Windows.Forms.Label lblHistoryServer;
        private System.Windows.Forms.ComboBox cmbNetworkMap2;
        private System.Windows.Forms.Label lblNetworkMap2;
        private System.Windows.Forms.ComboBox cmbDataSource;
        private System.Windows.Forms.Label lblDataSource;
        private System.Windows.Forms.CheckBox chkSpeakViolations;
        private System.Windows.Forms.Timer tmrUpdate;
        private System.Windows.Forms.CheckBox chkPipe;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.TextBox txtUserName;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.Label lblUserName;
        private System.Windows.Forms.CheckBox chkRecordTelemetryChanges;
        private System.Windows.Forms.CheckBox chkDisplayAlarmsStart;
        private System.Windows.Forms.ComboBox cmbHistoryServer2;
        private System.Windows.Forms.Label lblHistoryServer2;
        private System.Windows.Forms.GroupBox grpDiagnostics;
        private System.Windows.Forms.Button btnRemoveSensitiveInformation;
        private System.Windows.Forms.Label lblRemoveSensitiveInformation;
    }
}