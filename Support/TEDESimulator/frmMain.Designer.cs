namespace TEDESimulator
{
    partial class frmMain
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
            this.splMain = new System.Windows.Forms.SplitContainer();
            this.txtMMServer = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.lblServerAddress = new System.Windows.Forms.Label();
            this.cmbDataType = new System.Windows.Forms.ComboBox();
            this.lblDataType = new System.Windows.Forms.Label();
            this.dgvData = new System.Windows.Forms.DataGridView();
            this.tcMain = new System.Windows.Forms.TabControl();
            this.tpSendData = new System.Windows.Forms.TabPage();
            this.tpReceiveCommands = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.btnOpen = new System.Windows.Forms.Button();
            this.txtListningPort = new System.Windows.Forms.TextBox();
            this.lblListeningPort = new System.Windows.Forms.Label();
            this.lbIncoming = new System.Windows.Forms.ListBox();
            this.tpBuildingModels = new System.Windows.Forms.TabPage();
            this.label24 = new System.Windows.Forms.Label();
            this.btnReExport = new System.Windows.Forms.Button();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.txtBlackstartCorridorProbability = new System.Windows.Forms.TextBox();
            this.txtBusVoltageStdDev = new System.Windows.Forms.TextBox();
            this.txtReactorMVARStdDev = new System.Windows.Forms.TextBox();
            this.txtCapacitorMVARStdDev = new System.Windows.Forms.TextBox();
            this.txtLoadMWStdDev = new System.Windows.Forms.TextBox();
            this.txtReactorOpenProbability = new System.Windows.Forms.TextBox();
            this.txtCapacitorOpenProbability = new System.Windows.Forms.TextBox();
            this.txtReactorMVARAvg = new System.Windows.Forms.TextBox();
            this.txtCapacitorMVARAvg = new System.Windows.Forms.TextBox();
            this.txtLoadMWAvg = new System.Windows.Forms.TextBox();
            this.txtLineLoadStdDev = new System.Windows.Forms.TextBox();
            this.txtLineDistance = new System.Windows.Forms.TextBox();
            this.txtTransformerMWStdDev = new System.Windows.Forms.TextBox();
            this.txtLineMWStdDev = new System.Windows.Forms.TextBox();
            this.txtUnitCapacityStdDev = new System.Windows.Forms.TextBox();
            this.txtTransformerMWAvg = new System.Windows.Forms.TextBox();
            this.txtUnitMWStdDev = new System.Windows.Forms.TextBox();
            this.txtLineMWAvg = new System.Windows.Forms.TextBox();
            this.txtUnitCapacityAvg = new System.Windows.Forms.TextBox();
            this.txtUnitMWAvg = new System.Windows.Forms.TextBox();
            this.txtUnitProbability = new System.Windows.Forms.TextBox();
            this.txtLineLoadAvg = new System.Windows.Forms.TextBox();
            this.txtReactorProbability = new System.Windows.Forms.TextBox();
            this.txtCapacitorProbability = new System.Windows.Forms.TextBox();
            this.txtLoadProbability = new System.Windows.Forms.TextBox();
            this.txtLineProbability = new System.Windows.Forms.TextBox();
            this.txtCompanyCount = new System.Windows.Forms.TextBox();
            this.txtSubstationCount = new System.Windows.Forms.TextBox();
            this.cmbSubstationModel = new System.Windows.Forms.ComboBox();
            this.label25 = new System.Windows.Forms.Label();
            this.btnOneLineFolder = new System.Windows.Forms.Button();
            this.btnTargetModelFile = new System.Windows.Forms.Button();
            this.label33 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.label32 = new System.Windows.Forms.Label();
            this.label29 = new System.Windows.Forms.Label();
            this.label28 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.btnCountyBoundary = new System.Windows.Forms.Button();
            this.label31 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label27 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label36 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label35 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.lblUnits = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label30 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label26 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label34 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnBaseModel = new System.Windows.Forms.Button();
            this.btnStateBoundary = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.lblStateBoundary = new System.Windows.Forms.Label();
            this.btnRetrieveData = new System.Windows.Forms.Button();
            this.cmbMacomberMapServer = new System.Windows.Forms.ComboBox();
            this.tmrUpdate = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splMain)).BeginInit();
            this.splMain.Panel1.SuspendLayout();
            this.splMain.Panel2.SuspendLayout();
            this.splMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.tcMain.SuspendLayout();
            this.tpSendData.SuspendLayout();
            this.tpReceiveCommands.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tpBuildingModels.SuspendLayout();
            this.SuspendLayout();
            // 
            // splMain
            // 
            this.splMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splMain.Location = new System.Drawing.Point(3, 3);
            this.splMain.Name = "splMain";
            this.splMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splMain.Panel1
            // 
            this.splMain.Panel1.Controls.Add(this.cmbMacomberMapServer);
            this.splMain.Panel1.Controls.Add(this.txtMMServer);
            this.splMain.Panel1.Controls.Add(this.btnRetrieveData);
            this.splMain.Panel1.Controls.Add(this.btnSend);
            this.splMain.Panel1.Controls.Add(this.lblServerAddress);
            this.splMain.Panel1.Controls.Add(this.cmbDataType);
            this.splMain.Panel1.Controls.Add(this.lblDataType);
            // 
            // splMain.Panel2
            // 
            this.splMain.Panel2.Controls.Add(this.dgvData);
            this.splMain.Size = new System.Drawing.Size(584, 508);
            this.splMain.SplitterDistance = 59;
            this.splMain.TabIndex = 0;
            // 
            // txtMMServer
            // 
            this.txtMMServer.Location = new System.Drawing.Point(98, 3);
            this.txtMMServer.Name = "txtMMServer";
            this.txtMMServer.Size = new System.Drawing.Size(208, 20);
            this.txtMMServer.TabIndex = 7;
            this.txtMMServer.Text = "localhost:8889";
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(329, 1);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(75, 23);
            this.btnSend.TabIndex = 10;
            this.btnSend.Text = "&Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // lblServerAddress
            // 
            this.lblServerAddress.AutoSize = true;
            this.lblServerAddress.Location = new System.Drawing.Point(10, 6);
            this.lblServerAddress.Name = "lblServerAddress";
            this.lblServerAddress.Size = new System.Drawing.Size(82, 13);
            this.lblServerAddress.TabIndex = 5;
            this.lblServerAddress.Text = "Server Address:";
            // 
            // cmbDataType
            // 
            this.cmbDataType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDataType.FormattingEnabled = true;
            this.cmbDataType.Location = new System.Drawing.Point(98, 29);
            this.cmbDataType.Name = "cmbDataType";
            this.cmbDataType.Size = new System.Drawing.Size(208, 21);
            this.cmbDataType.TabIndex = 8;
            this.cmbDataType.SelectedIndexChanged += new System.EventHandler(this.cmbDataType_SelectedIndexChanged);
            // 
            // lblDataType
            // 
            this.lblDataType.AutoSize = true;
            this.lblDataType.Location = new System.Drawing.Point(10, 32);
            this.lblDataType.Name = "lblDataType";
            this.lblDataType.Size = new System.Drawing.Size(60, 13);
            this.lblDataType.TabIndex = 4;
            this.lblDataType.Text = "Data Type:";
            // 
            // dgvData
            // 
            this.dgvData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvData.Location = new System.Drawing.Point(0, 0);
            this.dgvData.Name = "dgvData";
            this.dgvData.Size = new System.Drawing.Size(584, 445);
            this.dgvData.TabIndex = 0;
            // 
            // tcMain
            // 
            this.tcMain.Controls.Add(this.tpSendData);
            this.tcMain.Controls.Add(this.tpReceiveCommands);
            this.tcMain.Controls.Add(this.tpBuildingModels);
            this.tcMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcMain.Location = new System.Drawing.Point(0, 0);
            this.tcMain.Name = "tcMain";
            this.tcMain.SelectedIndex = 0;
            this.tcMain.Size = new System.Drawing.Size(598, 540);
            this.tcMain.TabIndex = 1;
            // 
            // tpSendData
            // 
            this.tpSendData.Controls.Add(this.splMain);
            this.tpSendData.Location = new System.Drawing.Point(4, 22);
            this.tpSendData.Name = "tpSendData";
            this.tpSendData.Padding = new System.Windows.Forms.Padding(3);
            this.tpSendData.Size = new System.Drawing.Size(590, 514);
            this.tpSendData.TabIndex = 0;
            this.tpSendData.Text = "Sending Data";
            this.tpSendData.UseVisualStyleBackColor = true;
            // 
            // tpReceiveCommands
            // 
            this.tpReceiveCommands.Controls.Add(this.splitContainer1);
            this.tpReceiveCommands.Location = new System.Drawing.Point(4, 22);
            this.tpReceiveCommands.Name = "tpReceiveCommands";
            this.tpReceiveCommands.Padding = new System.Windows.Forms.Padding(3);
            this.tpReceiveCommands.Size = new System.Drawing.Size(454, 514);
            this.tpReceiveCommands.TabIndex = 1;
            this.tpReceiveCommands.Text = "Receiving Commands";
            this.tpReceiveCommands.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(3, 3);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.btnOpen);
            this.splitContainer1.Panel1.Controls.Add(this.txtListningPort);
            this.splitContainer1.Panel1.Controls.Add(this.lblListeningPort);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.lbIncoming);
            this.splitContainer1.Size = new System.Drawing.Size(448, 508);
            this.splitContainer1.SplitterDistance = 65;
            this.splitContainer1.TabIndex = 1;
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point(337, 4);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(75, 23);
            this.btnOpen.TabIndex = 3;
            this.btnOpen.Text = "&Open";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // txtListningPort
            // 
            this.txtListningPort.Location = new System.Drawing.Point(100, 6);
            this.txtListningPort.Name = "txtListningPort";
            this.txtListningPort.Size = new System.Drawing.Size(208, 20);
            this.txtListningPort.TabIndex = 1;
            this.txtListningPort.Text = "9000";
            // 
            // lblListeningPort
            // 
            this.lblListeningPort.AutoSize = true;
            this.lblListeningPort.Location = new System.Drawing.Point(12, 9);
            this.lblListeningPort.Name = "lblListeningPort";
            this.lblListeningPort.Size = new System.Drawing.Size(74, 13);
            this.lblListeningPort.TabIndex = 0;
            this.lblListeningPort.Text = "Listening Port:";
            // 
            // lbIncoming
            // 
            this.lbIncoming.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbIncoming.FormattingEnabled = true;
            this.lbIncoming.Location = new System.Drawing.Point(0, 0);
            this.lbIncoming.Name = "lbIncoming";
            this.lbIncoming.Size = new System.Drawing.Size(448, 439);
            this.lbIncoming.TabIndex = 0;
            // 
            // tpBuildingModels
            // 
            this.tpBuildingModels.Controls.Add(this.label24);
            this.tpBuildingModels.Controls.Add(this.btnReExport);
            this.tpBuildingModels.Controls.Add(this.btnGenerate);
            this.tpBuildingModels.Controls.Add(this.txtBlackstartCorridorProbability);
            this.tpBuildingModels.Controls.Add(this.txtBusVoltageStdDev);
            this.tpBuildingModels.Controls.Add(this.txtReactorMVARStdDev);
            this.tpBuildingModels.Controls.Add(this.txtCapacitorMVARStdDev);
            this.tpBuildingModels.Controls.Add(this.txtLoadMWStdDev);
            this.tpBuildingModels.Controls.Add(this.txtReactorOpenProbability);
            this.tpBuildingModels.Controls.Add(this.txtCapacitorOpenProbability);
            this.tpBuildingModels.Controls.Add(this.txtReactorMVARAvg);
            this.tpBuildingModels.Controls.Add(this.txtCapacitorMVARAvg);
            this.tpBuildingModels.Controls.Add(this.txtLoadMWAvg);
            this.tpBuildingModels.Controls.Add(this.txtLineLoadStdDev);
            this.tpBuildingModels.Controls.Add(this.txtLineDistance);
            this.tpBuildingModels.Controls.Add(this.txtTransformerMWStdDev);
            this.tpBuildingModels.Controls.Add(this.txtLineMWStdDev);
            this.tpBuildingModels.Controls.Add(this.txtUnitCapacityStdDev);
            this.tpBuildingModels.Controls.Add(this.txtTransformerMWAvg);
            this.tpBuildingModels.Controls.Add(this.txtUnitMWStdDev);
            this.tpBuildingModels.Controls.Add(this.txtLineMWAvg);
            this.tpBuildingModels.Controls.Add(this.txtUnitCapacityAvg);
            this.tpBuildingModels.Controls.Add(this.txtUnitMWAvg);
            this.tpBuildingModels.Controls.Add(this.txtUnitProbability);
            this.tpBuildingModels.Controls.Add(this.txtLineLoadAvg);
            this.tpBuildingModels.Controls.Add(this.txtReactorProbability);
            this.tpBuildingModels.Controls.Add(this.txtCapacitorProbability);
            this.tpBuildingModels.Controls.Add(this.txtLoadProbability);
            this.tpBuildingModels.Controls.Add(this.txtLineProbability);
            this.tpBuildingModels.Controls.Add(this.txtCompanyCount);
            this.tpBuildingModels.Controls.Add(this.txtSubstationCount);
            this.tpBuildingModels.Controls.Add(this.cmbSubstationModel);
            this.tpBuildingModels.Controls.Add(this.label25);
            this.tpBuildingModels.Controls.Add(this.btnOneLineFolder);
            this.tpBuildingModels.Controls.Add(this.btnTargetModelFile);
            this.tpBuildingModels.Controls.Add(this.label33);
            this.tpBuildingModels.Controls.Add(this.label19);
            this.tpBuildingModels.Controls.Add(this.label32);
            this.tpBuildingModels.Controls.Add(this.label29);
            this.tpBuildingModels.Controls.Add(this.label28);
            this.tpBuildingModels.Controls.Add(this.label8);
            this.tpBuildingModels.Controls.Add(this.label12);
            this.tpBuildingModels.Controls.Add(this.btnCountyBoundary);
            this.tpBuildingModels.Controls.Add(this.label31);
            this.tpBuildingModels.Controls.Add(this.label16);
            this.tpBuildingModels.Controls.Add(this.label27);
            this.tpBuildingModels.Controls.Add(this.label7);
            this.tpBuildingModels.Controls.Add(this.label22);
            this.tpBuildingModels.Controls.Add(this.label10);
            this.tpBuildingModels.Controls.Add(this.label18);
            this.tpBuildingModels.Controls.Add(this.label36);
            this.tpBuildingModels.Controls.Add(this.label15);
            this.tpBuildingModels.Controls.Add(this.label14);
            this.tpBuildingModels.Controls.Add(this.label20);
            this.tpBuildingModels.Controls.Add(this.label21);
            this.tpBuildingModels.Controls.Add(this.label6);
            this.tpBuildingModels.Controls.Add(this.label35);
            this.tpBuildingModels.Controls.Add(this.label5);
            this.tpBuildingModels.Controls.Add(this.label17);
            this.tpBuildingModels.Controls.Add(this.label9);
            this.tpBuildingModels.Controls.Add(this.lblUnits);
            this.tpBuildingModels.Controls.Add(this.label11);
            this.tpBuildingModels.Controls.Add(this.label30);
            this.tpBuildingModels.Controls.Add(this.label13);
            this.tpBuildingModels.Controls.Add(this.label26);
            this.tpBuildingModels.Controls.Add(this.label4);
            this.tpBuildingModels.Controls.Add(this.label3);
            this.tpBuildingModels.Controls.Add(this.label34);
            this.tpBuildingModels.Controls.Add(this.label2);
            this.tpBuildingModels.Controls.Add(this.btnBaseModel);
            this.tpBuildingModels.Controls.Add(this.btnStateBoundary);
            this.tpBuildingModels.Controls.Add(this.label1);
            this.tpBuildingModels.Controls.Add(this.label23);
            this.tpBuildingModels.Controls.Add(this.lblStateBoundary);
            this.tpBuildingModels.Location = new System.Drawing.Point(4, 22);
            this.tpBuildingModels.Name = "tpBuildingModels";
            this.tpBuildingModels.Padding = new System.Windows.Forms.Padding(3);
            this.tpBuildingModels.Size = new System.Drawing.Size(454, 514);
            this.tpBuildingModels.TabIndex = 2;
            this.tpBuildingModels.Text = "Building models";
            this.tpBuildingModels.UseVisualStyleBackColor = true;
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(12, 416);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(146, 13);
            this.label24.TabIndex = 5;
            this.label24.Text = "Blackstart Corridor probability:";
            // 
            // btnReExport
            // 
            this.btnReExport.Location = new System.Drawing.Point(238, 483);
            this.btnReExport.Name = "btnReExport";
            this.btnReExport.Size = new System.Drawing.Size(149, 23);
            this.btnReExport.TabIndex = 4;
            this.btnReExport.Text = "Re-export";
            this.btnReExport.UseVisualStyleBackColor = true;
            this.btnReExport.Click += new System.EventHandler(this.btnReExport_Click);
            // 
            // btnGenerate
            // 
            this.btnGenerate.Location = new System.Drawing.Point(36, 483);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(149, 23);
            this.btnGenerate.TabIndex = 4;
            this.btnGenerate.Text = "Generate and export";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // txtBlackstartCorridorProbability
            // 
            this.txtBlackstartCorridorProbability.Location = new System.Drawing.Point(175, 413);
            this.txtBlackstartCorridorProbability.Name = "txtBlackstartCorridorProbability";
            this.txtBlackstartCorridorProbability.Size = new System.Drawing.Size(39, 20);
            this.txtBlackstartCorridorProbability.TabIndex = 3;
            this.txtBlackstartCorridorProbability.Text = "20";
            // 
            // txtBusVoltageStdDev
            // 
            this.txtBusVoltageStdDev.Location = new System.Drawing.Point(175, 387);
            this.txtBusVoltageStdDev.Name = "txtBusVoltageStdDev";
            this.txtBusVoltageStdDev.Size = new System.Drawing.Size(39, 20);
            this.txtBusVoltageStdDev.TabIndex = 3;
            this.txtBusVoltageStdDev.Text = "20";
            // 
            // txtReactorMVARStdDev
            // 
            this.txtReactorMVARStdDev.Location = new System.Drawing.Point(307, 349);
            this.txtReactorMVARStdDev.Name = "txtReactorMVARStdDev";
            this.txtReactorMVARStdDev.Size = new System.Drawing.Size(39, 20);
            this.txtReactorMVARStdDev.TabIndex = 3;
            this.txtReactorMVARStdDev.Text = "10";
            // 
            // txtCapacitorMVARStdDev
            // 
            this.txtCapacitorMVARStdDev.Location = new System.Drawing.Point(307, 323);
            this.txtCapacitorMVARStdDev.Name = "txtCapacitorMVARStdDev";
            this.txtCapacitorMVARStdDev.Size = new System.Drawing.Size(39, 20);
            this.txtCapacitorMVARStdDev.TabIndex = 3;
            this.txtCapacitorMVARStdDev.Text = "10";
            // 
            // txtLoadMWStdDev
            // 
            this.txtLoadMWStdDev.Location = new System.Drawing.Point(307, 295);
            this.txtLoadMWStdDev.Name = "txtLoadMWStdDev";
            this.txtLoadMWStdDev.Size = new System.Drawing.Size(39, 20);
            this.txtLoadMWStdDev.TabIndex = 3;
            this.txtLoadMWStdDev.Text = "10";
            // 
            // txtReactorOpenProbability
            // 
            this.txtReactorOpenProbability.Location = new System.Drawing.Point(399, 349);
            this.txtReactorOpenProbability.Name = "txtReactorOpenProbability";
            this.txtReactorOpenProbability.Size = new System.Drawing.Size(39, 20);
            this.txtReactorOpenProbability.TabIndex = 3;
            this.txtReactorOpenProbability.Text = "50";
            // 
            // txtCapacitorOpenProbability
            // 
            this.txtCapacitorOpenProbability.Location = new System.Drawing.Point(399, 323);
            this.txtCapacitorOpenProbability.Name = "txtCapacitorOpenProbability";
            this.txtCapacitorOpenProbability.Size = new System.Drawing.Size(39, 20);
            this.txtCapacitorOpenProbability.TabIndex = 3;
            this.txtCapacitorOpenProbability.Text = "50";
            // 
            // txtReactorMVARAvg
            // 
            this.txtReactorMVARAvg.Location = new System.Drawing.Point(214, 349);
            this.txtReactorMVARAvg.Name = "txtReactorMVARAvg";
            this.txtReactorMVARAvg.Size = new System.Drawing.Size(39, 20);
            this.txtReactorMVARAvg.TabIndex = 3;
            this.txtReactorMVARAvg.Text = "50";
            // 
            // txtCapacitorMVARAvg
            // 
            this.txtCapacitorMVARAvg.Location = new System.Drawing.Point(214, 323);
            this.txtCapacitorMVARAvg.Name = "txtCapacitorMVARAvg";
            this.txtCapacitorMVARAvg.Size = new System.Drawing.Size(39, 20);
            this.txtCapacitorMVARAvg.TabIndex = 3;
            this.txtCapacitorMVARAvg.Text = "50";
            // 
            // txtLoadMWAvg
            // 
            this.txtLoadMWAvg.Location = new System.Drawing.Point(214, 295);
            this.txtLoadMWAvg.Name = "txtLoadMWAvg";
            this.txtLoadMWAvg.Size = new System.Drawing.Size(39, 20);
            this.txtLoadMWAvg.TabIndex = 3;
            this.txtLoadMWAvg.Text = "50";
            // 
            // txtLineLoadStdDev
            // 
            this.txtLineLoadStdDev.Location = new System.Drawing.Point(393, 201);
            this.txtLineLoadStdDev.Name = "txtLineLoadStdDev";
            this.txtLineLoadStdDev.Size = new System.Drawing.Size(39, 20);
            this.txtLineLoadStdDev.TabIndex = 3;
            this.txtLineLoadStdDev.Text = "24";
            // 
            // txtLineDistance
            // 
            this.txtLineDistance.Location = new System.Drawing.Point(224, 167);
            this.txtLineDistance.Name = "txtLineDistance";
            this.txtLineDistance.Size = new System.Drawing.Size(39, 20);
            this.txtLineDistance.TabIndex = 3;
            this.txtLineDistance.Text = "20";
            // 
            // txtTransformerMWStdDev
            // 
            this.txtTransformerMWStdDev.Location = new System.Drawing.Point(217, 444);
            this.txtTransformerMWStdDev.Name = "txtTransformerMWStdDev";
            this.txtTransformerMWStdDev.Size = new System.Drawing.Size(39, 20);
            this.txtTransformerMWStdDev.TabIndex = 3;
            this.txtTransformerMWStdDev.Text = "20";
            // 
            // txtLineMWStdDev
            // 
            this.txtLineMWStdDev.Location = new System.Drawing.Point(213, 201);
            this.txtLineMWStdDev.Name = "txtLineMWStdDev";
            this.txtLineMWStdDev.Size = new System.Drawing.Size(39, 20);
            this.txtLineMWStdDev.TabIndex = 3;
            this.txtLineMWStdDev.Text = "20";
            // 
            // txtUnitCapacityStdDev
            // 
            this.txtUnitCapacityStdDev.Location = new System.Drawing.Point(307, 268);
            this.txtUnitCapacityStdDev.Name = "txtUnitCapacityStdDev";
            this.txtUnitCapacityStdDev.Size = new System.Drawing.Size(39, 20);
            this.txtUnitCapacityStdDev.TabIndex = 3;
            this.txtUnitCapacityStdDev.Text = "25";
            // 
            // txtTransformerMWAvg
            // 
            this.txtTransformerMWAvg.Location = new System.Drawing.Point(130, 444);
            this.txtTransformerMWAvg.Name = "txtTransformerMWAvg";
            this.txtTransformerMWAvg.Size = new System.Drawing.Size(39, 20);
            this.txtTransformerMWAvg.TabIndex = 3;
            this.txtTransformerMWAvg.Text = "60";
            // 
            // txtUnitMWStdDev
            // 
            this.txtUnitMWStdDev.Location = new System.Drawing.Point(307, 243);
            this.txtUnitMWStdDev.Name = "txtUnitMWStdDev";
            this.txtUnitMWStdDev.Size = new System.Drawing.Size(39, 20);
            this.txtUnitMWStdDev.TabIndex = 3;
            this.txtUnitMWStdDev.Text = "100";
            // 
            // txtLineMWAvg
            // 
            this.txtLineMWAvg.Location = new System.Drawing.Point(126, 201);
            this.txtLineMWAvg.Name = "txtLineMWAvg";
            this.txtLineMWAvg.Size = new System.Drawing.Size(39, 20);
            this.txtLineMWAvg.TabIndex = 3;
            this.txtLineMWAvg.Text = "60";
            // 
            // txtUnitCapacityAvg
            // 
            this.txtUnitCapacityAvg.Location = new System.Drawing.Point(214, 268);
            this.txtUnitCapacityAvg.Name = "txtUnitCapacityAvg";
            this.txtUnitCapacityAvg.Size = new System.Drawing.Size(39, 20);
            this.txtUnitCapacityAvg.TabIndex = 3;
            this.txtUnitCapacityAvg.Text = "50";
            // 
            // txtUnitMWAvg
            // 
            this.txtUnitMWAvg.Location = new System.Drawing.Point(214, 243);
            this.txtUnitMWAvg.Name = "txtUnitMWAvg";
            this.txtUnitMWAvg.Size = new System.Drawing.Size(39, 20);
            this.txtUnitMWAvg.TabIndex = 3;
            this.txtUnitMWAvg.Text = "200";
            // 
            // txtUnitProbability
            // 
            this.txtUnitProbability.Location = new System.Drawing.Point(127, 243);
            this.txtUnitProbability.Name = "txtUnitProbability";
            this.txtUnitProbability.Size = new System.Drawing.Size(39, 20);
            this.txtUnitProbability.TabIndex = 3;
            this.txtUnitProbability.Text = "10";
            // 
            // txtLineLoadAvg
            // 
            this.txtLineLoadAvg.Location = new System.Drawing.Point(312, 201);
            this.txtLineLoadAvg.Name = "txtLineLoadAvg";
            this.txtLineLoadAvg.Size = new System.Drawing.Size(39, 20);
            this.txtLineLoadAvg.TabIndex = 3;
            this.txtLineLoadAvg.Text = "25";
            // 
            // txtReactorProbability
            // 
            this.txtReactorProbability.Location = new System.Drawing.Point(127, 349);
            this.txtReactorProbability.Name = "txtReactorProbability";
            this.txtReactorProbability.Size = new System.Drawing.Size(39, 20);
            this.txtReactorProbability.TabIndex = 3;
            this.txtReactorProbability.Text = "35";
            // 
            // txtCapacitorProbability
            // 
            this.txtCapacitorProbability.Location = new System.Drawing.Point(127, 323);
            this.txtCapacitorProbability.Name = "txtCapacitorProbability";
            this.txtCapacitorProbability.Size = new System.Drawing.Size(39, 20);
            this.txtCapacitorProbability.TabIndex = 3;
            this.txtCapacitorProbability.Text = "35";
            // 
            // txtLoadProbability
            // 
            this.txtLoadProbability.Location = new System.Drawing.Point(127, 295);
            this.txtLoadProbability.Name = "txtLoadProbability";
            this.txtLoadProbability.Size = new System.Drawing.Size(39, 20);
            this.txtLoadProbability.TabIndex = 3;
            this.txtLoadProbability.Text = "35";
            // 
            // txtLineProbability
            // 
            this.txtLineProbability.Location = new System.Drawing.Point(126, 167);
            this.txtLineProbability.Name = "txtLineProbability";
            this.txtLineProbability.Size = new System.Drawing.Size(39, 20);
            this.txtLineProbability.TabIndex = 3;
            this.txtLineProbability.Text = "40";
            // 
            // txtCompanyCount
            // 
            this.txtCompanyCount.Location = new System.Drawing.Point(126, 108);
            this.txtCompanyCount.Name = "txtCompanyCount";
            this.txtCompanyCount.Size = new System.Drawing.Size(39, 20);
            this.txtCompanyCount.TabIndex = 3;
            this.txtCompanyCount.Text = "10";
            // 
            // txtSubstationCount
            // 
            this.txtSubstationCount.Location = new System.Drawing.Point(126, 135);
            this.txtSubstationCount.Name = "txtSubstationCount";
            this.txtSubstationCount.Size = new System.Drawing.Size(39, 20);
            this.txtSubstationCount.TabIndex = 3;
            this.txtSubstationCount.Text = "1";
            // 
            // cmbSubstationModel
            // 
            this.cmbSubstationModel.FormattingEnabled = true;
            this.cmbSubstationModel.Items.AddRange(new object[] {
            "per county (random position)",
            "per county (in center)",
            "per state"});
            this.cmbSubstationModel.Location = new System.Drawing.Point(171, 135);
            this.cmbSubstationModel.Name = "cmbSubstationModel";
            this.cmbSubstationModel.Size = new System.Drawing.Size(180, 21);
            this.cmbSubstationModel.TabIndex = 2;
            this.cmbSubstationModel.Text = "per county (in center)";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(214, 416);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(15, 13);
            this.label25.TabIndex = 0;
            this.label25.Text = "%";
            // 
            // btnOneLineFolder
            // 
            this.btnOneLineFolder.Location = new System.Drawing.Point(348, 79);
            this.btnOneLineFolder.Name = "btnOneLineFolder";
            this.btnOneLineFolder.Size = new System.Drawing.Size(98, 23);
            this.btnOneLineFolder.TabIndex = 1;
            this.btnOneLineFolder.Text = "Select Target";
            this.btnOneLineFolder.UseVisualStyleBackColor = true;
            this.btnOneLineFolder.Click += new System.EventHandler(this.btnOneLineFolder_Click);
            // 
            // btnTargetModelFile
            // 
            this.btnTargetModelFile.Location = new System.Drawing.Point(126, 79);
            this.btnTargetModelFile.Name = "btnTargetModelFile";
            this.btnTargetModelFile.Size = new System.Drawing.Size(98, 23);
            this.btnTargetModelFile.TabIndex = 1;
            this.btnTargetModelFile.Text = "Select Target";
            this.btnTargetModelFile.UseVisualStyleBackColor = true;
            this.btnTargetModelFile.Click += new System.EventHandler(this.btnSelectTarget_Click);
            // 
            // label33
            // 
            this.label33.AutoSize = true;
            this.label33.Location = new System.Drawing.Point(357, 352);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(42, 13);
            this.label33.TabIndex = 0;
            this.label33.Text = "% open";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(214, 390);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(15, 13);
            this.label19.TabIndex = 0;
            this.label19.Text = "%";
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Location = new System.Drawing.Point(172, 352);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(30, 13);
            this.label32.TabIndex = 0;
            this.label32.Text = "%, at";
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Location = new System.Drawing.Point(357, 326);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(42, 13);
            this.label29.TabIndex = 0;
            this.label29.Text = "% open";
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(172, 326);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(30, 13);
            this.label28.TabIndex = 0;
            this.label28.Text = "%, at";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(172, 298);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(30, 13);
            this.label8.TabIndex = 0;
            this.label8.Text = "%, at";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(127, 390);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(42, 13);
            this.label12.TabIndex = 0;
            this.label12.Text = "1 pU ± ";
            // 
            // btnCountyBoundary
            // 
            this.btnCountyBoundary.Location = new System.Drawing.Point(126, 44);
            this.btnCountyBoundary.Name = "btnCountyBoundary";
            this.btnCountyBoundary.Size = new System.Drawing.Size(98, 23);
            this.btnCountyBoundary.TabIndex = 1;
            this.btnCountyBoundary.Text = "Open file...";
            this.btnCountyBoundary.UseVisualStyleBackColor = true;
            this.btnCountyBoundary.Click += new System.EventHandler(this.HandleShapefileClick);
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Location = new System.Drawing.Point(259, 352);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(50, 13);
            this.label31.TabIndex = 0;
            this.label31.Text = "MVAR ± ";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(357, 204);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(30, 13);
            this.label16.TabIndex = 0;
            this.label16.Text = "%, ± ";
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(259, 326);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(50, 13);
            this.label27.TabIndex = 0;
            this.label27.Text = "MVAR ± ";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(259, 298);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(39, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "MW ± ";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(250, 28);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(16, 13);
            this.label22.TabIndex = 0;
            this.label22.Text = "or";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(168, 170);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(56, 13);
            this.label10.TabIndex = 0;
            this.label10.Text = "%, miles ± ";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(438, 204);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(10, 13);
            this.label18.TabIndex = 0;
            this.label18.Text = ")";
            // 
            // label36
            // 
            this.label36.AutoSize = true;
            this.label36.Location = new System.Drawing.Point(172, 447);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(39, 13);
            this.label36.TabIndex = 0;
            this.label36.Text = "MW ± ";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(259, 204);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(47, 13);
            this.label15.TabIndex = 0;
            this.label15.Text = " (loading";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(168, 204);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(39, 13);
            this.label14.TabIndex = 0;
            this.label14.Text = "MW ± ";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(151, 271);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(60, 13);
            this.label20.TabIndex = 0;
            this.label20.Text = "Capacity at";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(259, 271);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(27, 13);
            this.label21.TabIndex = 0;
            this.label21.Text = "% ± ";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(172, 246);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(30, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "%, at";
            // 
            // label35
            // 
            this.label35.AutoSize = true;
            this.label35.Location = new System.Drawing.Point(12, 447);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(107, 13);
            this.label35.TabIndex = 0;
            this.label35.Text = "Transformer Loading:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(259, 246);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(39, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "MW ± ";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(8, 208);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(71, 13);
            this.label17.TabIndex = 0;
            this.label17.Text = "Line Loading:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(8, 170);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(81, 13);
            this.label9.TabIndex = 0;
            this.label9.Text = "Line Probability:";
            // 
            // lblUnits
            // 
            this.lblUnits.AutoSize = true;
            this.lblUnits.Location = new System.Drawing.Point(9, 246);
            this.lblUnits.Name = "lblUnits";
            this.lblUnits.Size = new System.Drawing.Size(80, 13);
            this.lblUnits.TabIndex = 0;
            this.lblUnits.Text = "Unit Probability:";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(12, 387);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(66, 13);
            this.label11.TabIndex = 0;
            this.label11.Text = "Bus voltage:";
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Location = new System.Drawing.Point(9, 352);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(95, 13);
            this.label30.TabIndex = 0;
            this.label30.Text = "Reactor probability";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(8, 111);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(62, 13);
            this.label13.TabIndex = 0;
            this.label13.Text = "Companies:";
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(9, 326);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(102, 13);
            this.label26.TabIndex = 0;
            this.label26.Text = "Capacitor probability";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 298);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(84, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Load probability:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 138);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Substations:";
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.Location = new System.Drawing.Point(250, 84);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(85, 13);
            this.label34.TabIndex = 0;
            this.label34.Text = "One-Line Folder:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 84);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Target Model file:";
            // 
            // btnBaseModel
            // 
            this.btnBaseModel.Location = new System.Drawing.Point(307, 28);
            this.btnBaseModel.Name = "btnBaseModel";
            this.btnBaseModel.Size = new System.Drawing.Size(98, 23);
            this.btnBaseModel.TabIndex = 1;
            this.btnBaseModel.Text = "Open file...";
            this.btnBaseModel.UseVisualStyleBackColor = true;
            this.btnBaseModel.Click += new System.EventHandler(this.btrnOpenBaseModel_Click);
            // 
            // btnStateBoundary
            // 
            this.btnStateBoundary.Location = new System.Drawing.Point(126, 9);
            this.btnStateBoundary.Name = "btnStateBoundary";
            this.btnStateBoundary.Size = new System.Drawing.Size(98, 23);
            this.btnStateBoundary.TabIndex = 1;
            this.btnStateBoundary.Text = "Open file...";
            this.btnStateBoundary.UseVisualStyleBackColor = true;
            this.btnStateBoundary.Click += new System.EventHandler(this.HandleShapefileClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(97, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "\"County boundary\"";
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(274, 9);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(140, 13);
            this.label23.TabIndex = 0;
            this.label23.Text = "Base Macomber Map Model";
            // 
            // lblStateBoundary
            // 
            this.lblStateBoundary.AutoSize = true;
            this.lblStateBoundary.Location = new System.Drawing.Point(8, 14);
            this.lblStateBoundary.Name = "lblStateBoundary";
            this.lblStateBoundary.Size = new System.Drawing.Size(89, 13);
            this.lblStateBoundary.TabIndex = 0;
            this.lblStateBoundary.Text = "\"State boundary\"";
            // 
            // btnRetrieveData
            // 
            this.btnRetrieveData.Location = new System.Drawing.Point(329, 27);
            this.btnRetrieveData.Name = "btnRetrieveData";
            this.btnRetrieveData.Size = new System.Drawing.Size(75, 23);
            this.btnRetrieveData.TabIndex = 10;
            this.btnRetrieveData.Text = "&Get";
            this.btnRetrieveData.UseVisualStyleBackColor = true;
            this.btnRetrieveData.Click += new System.EventHandler(this.btnRetrieveData_Click);
            // 
            // cmbMacomberMapServer
            // 
            this.cmbMacomberMapServer.FormattingEnabled = true;
            this.cmbMacomberMapServer.Location = new System.Drawing.Point(410, 29);
            this.cmbMacomberMapServer.Name = "cmbMacomberMapServer";
            this.cmbMacomberMapServer.Size = new System.Drawing.Size(164, 21);
            this.cmbMacomberMapServer.TabIndex = 11;
            // 
            // tmrUpdate
            // 
            this.tmrUpdate.Enabled = true;
            this.tmrUpdate.Interval = 2500;
            this.tmrUpdate.Tick += new System.EventHandler(this.tmrUpdate_Tick);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(598, 540);
            this.Controls.Add(this.tcMain);
            this.Icon = global::TEDESimulator.Properties.Resources.CompanyIcon;
            this.Name = "frmMain";
            this.Text = "Macomber Map TEDE Simulator";
            this.splMain.Panel1.ResumeLayout(false);
            this.splMain.Panel1.PerformLayout();
            this.splMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splMain)).EndInit();
            this.splMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.tcMain.ResumeLayout(false);
            this.tpSendData.ResumeLayout(false);
            this.tpReceiveCommands.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tpBuildingModels.ResumeLayout(false);
            this.tpBuildingModels.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splMain;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.TextBox txtListningPort;
        private System.Windows.Forms.Label lblListeningPort;
        private System.Windows.Forms.TabControl tcMain;
        private System.Windows.Forms.TabPage tpSendData;
        private System.Windows.Forms.TabPage tpReceiveCommands;
        private System.Windows.Forms.ListBox lbIncoming;
        private System.Windows.Forms.Label lblDataType;
        private System.Windows.Forms.ComboBox cmbDataType;
        private System.Windows.Forms.Label lblServerAddress;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.TextBox txtMMServer;
        private System.Windows.Forms.DataGridView dgvData;
        private System.Windows.Forms.TabPage tpBuildingModels;
        private System.Windows.Forms.Button btnCountyBoundary;
        private System.Windows.Forms.Button btnStateBoundary;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblStateBoundary;
        private System.Windows.Forms.Button btnTargetModelFile;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.TextBox txtBusVoltageStdDev;
        private System.Windows.Forms.TextBox txtLoadMWStdDev;
        private System.Windows.Forms.TextBox txtLoadMWAvg;
        private System.Windows.Forms.TextBox txtLineDistance;
        private System.Windows.Forms.TextBox txtUnitMWStdDev;
        private System.Windows.Forms.TextBox txtUnitMWAvg;
        private System.Windows.Forms.TextBox txtUnitProbability;
        private System.Windows.Forms.TextBox txtLoadProbability;
        private System.Windows.Forms.TextBox txtLineProbability;
        private System.Windows.Forms.TextBox txtSubstationCount;
        private System.Windows.Forms.ComboBox cmbSubstationModel;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label lblUnits;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtCompanyCount;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox txtLineMWStdDev;
        private System.Windows.Forms.TextBox txtLineMWAvg;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox txtLineLoadStdDev;
        private System.Windows.Forms.TextBox txtLineLoadAvg;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.TextBox txtUnitCapacityStdDev;
        private System.Windows.Forms.TextBox txtUnitCapacityAvg;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Button btnBaseModel;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.TextBox txtBlackstartCorridorProbability;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.TextBox txtReactorMVARStdDev;
        private System.Windows.Forms.TextBox txtCapacitorMVARStdDev;
        private System.Windows.Forms.TextBox txtReactorOpenProbability;
        private System.Windows.Forms.TextBox txtCapacitorOpenProbability;
        private System.Windows.Forms.TextBox txtReactorMVARAvg;
        private System.Windows.Forms.TextBox txtCapacitorMVARAvg;
        private System.Windows.Forms.TextBox txtReactorProbability;
        private System.Windows.Forms.TextBox txtCapacitorProbability;
        private System.Windows.Forms.Label label33;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.Button btnOneLineFolder;
        private System.Windows.Forms.Label label34;
        private System.Windows.Forms.TextBox txtTransformerMWStdDev;
        private System.Windows.Forms.TextBox txtTransformerMWAvg;
        private System.Windows.Forms.Label label36;
        private System.Windows.Forms.Label label35;
        private System.Windows.Forms.Button btnReExport;
        private System.Windows.Forms.Button btnRetrieveData;
        private System.Windows.Forms.ComboBox cmbMacomberMapServer;
        private System.Windows.Forms.Timer tmrUpdate;
    }
}

