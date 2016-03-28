namespace MacomberMapClient.User_Interfaces.Configuration
{
    partial class MM_Display_Options
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
            this.pgMain = new System.Windows.Forms.PropertyGrid();
            this.splMain = new System.Windows.Forms.SplitContainer();
            this.splLeft = new System.Windows.Forms.SplitContainer();
            this.grpViews = new System.Windows.Forms.GroupBox();
            this.tvViews = new System.Windows.Forms.TreeView();
            this.grpParameters = new System.Windows.Forms.GroupBox();
            this.tvParameters = new System.Windows.Forms.TreeView();
            this.ssMain = new System.Windows.Forms.StatusStrip();
            this.lblCPU = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsMem = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsFPS = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsZoom = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsCenter = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsMouse = new System.Windows.Forms.ToolStripStatusLabel();
            this.tmrUpdate = new System.Windows.Forms.Timer(this.components);
            this.tabDisplay = new System.Windows.Forms.TabControl();
            this.tabQuick = new System.Windows.Forms.TabPage();
            this.grpEquipmentMonitoring = new System.Windows.Forms.GroupBox();
            this.lblHighVoltageAlert = new System.Windows.Forms.Label();
            this.lblHighVoltageWarning = new System.Windows.Forms.Label();
            this.lblLowVoltageAlert = new System.Windows.Forms.Label();
            this.lblThermalAlert = new System.Windows.Forms.Label();
            this.lblLowVoltageWarning = new System.Windows.Forms.Label();
            this.lblThermalWarning = new System.Windows.Forms.Label();
            this.lblVoltagePerUnit = new System.Windows.Forms.Label();
            this.lblThermalLimit = new System.Windows.Forms.Label();
            this.grpCounty = new System.Windows.Forms.GroupBox();
            this.grpSubstations = new System.Windows.Forms.GroupBox();
            this.grpLineMVA = new System.Windows.Forms.GroupBox();
            this.lblLineVisibility = new System.Windows.Forms.Label();
            this.lblLineMVAAnimation = new System.Windows.Forms.Label();
            this.chkLineVisibility = new System.Windows.Forms.CheckBox();
            this.rbLessThan = new System.Windows.Forms.RadioButton();
            this.tbLineVisibility = new System.Windows.Forms.TrackBar();
            this.rbGreater = new System.Windows.Forms.RadioButton();
            this.tbLineAnimation = new System.Windows.Forms.TrackBar();
            this.chkLineMVAAnimation = new System.Windows.Forms.CheckBox();
            this.grpLineVisibility = new System.Windows.Forms.GroupBox();
            this.lblMVAFlowSize = new System.Windows.Forms.Label();
            this.lblNormalOpened = new System.Windows.Forms.Label();
            this.lblLineRouting = new System.Windows.Forms.Label();
            this.lblUnknown = new System.Windows.Forms.Label();
            this.lblDeEnergized = new System.Windows.Forms.Label();
            this.lblPartiallyEnergized = new System.Windows.Forms.Label();
            this.lblEnergized = new System.Windows.Forms.Label();
            this.tabOverall = new System.Windows.Forms.TabPage();
            this.tabViolations = new System.Windows.Forms.TabPage();
            this.tabLines = new System.Windows.Forms.TabPage();
            this.tabSubstations = new System.Windows.Forms.TabPage();
            this.tabAdvanced = new System.Windows.Forms.TabPage();
            this.tTip = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splMain)).BeginInit();
            this.splMain.Panel1.SuspendLayout();
            this.splMain.Panel2.SuspendLayout();
            this.splMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splLeft)).BeginInit();
            this.splLeft.Panel1.SuspendLayout();
            this.splLeft.Panel2.SuspendLayout();
            this.splLeft.SuspendLayout();
            this.grpViews.SuspendLayout();
            this.grpParameters.SuspendLayout();
            this.ssMain.SuspendLayout();
            this.tabDisplay.SuspendLayout();
            this.tabQuick.SuspendLayout();
            this.grpEquipmentMonitoring.SuspendLayout();
            this.grpLineMVA.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbLineVisibility)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbLineAnimation)).BeginInit();
            this.grpLineVisibility.SuspendLayout();
            this.tabAdvanced.SuspendLayout();
            this.SuspendLayout();
            // 
            // pgMain
            // 
            this.pgMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pgMain.Location = new System.Drawing.Point(0, 0);
            this.pgMain.Name = "pgMain";
            this.pgMain.Size = new System.Drawing.Size(281, 397);
            this.pgMain.TabIndex = 1;
            // 
            // splMain
            // 
            this.splMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splMain.Location = new System.Drawing.Point(3, 3);
            this.splMain.Name = "splMain";
            // 
            // splMain.Panel1
            // 
            this.splMain.Panel1.Controls.Add(this.splLeft);
            // 
            // splMain.Panel2
            // 
            this.splMain.Panel2.Controls.Add(this.pgMain);
            this.splMain.Size = new System.Drawing.Size(419, 397);
            this.splMain.SplitterDistance = 134;
            this.splMain.TabIndex = 2;
            // 
            // splLeft
            // 
            this.splLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splLeft.Location = new System.Drawing.Point(0, 0);
            this.splLeft.Name = "splLeft";
            this.splLeft.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splLeft.Panel1
            // 
            this.splLeft.Panel1.Controls.Add(this.grpViews);
            // 
            // splLeft.Panel2
            // 
            this.splLeft.Panel2.Controls.Add(this.grpParameters);
            this.splLeft.Size = new System.Drawing.Size(134, 397);
            this.splLeft.SplitterDistance = 196;
            this.splLeft.TabIndex = 2;
            // 
            // grpViews
            // 
            this.grpViews.Controls.Add(this.tvViews);
            this.grpViews.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpViews.Location = new System.Drawing.Point(0, 0);
            this.grpViews.Name = "grpViews";
            this.grpViews.Size = new System.Drawing.Size(134, 196);
            this.grpViews.TabIndex = 1;
            this.grpViews.TabStop = false;
            this.grpViews.Text = "Views";
            // 
            // tvViews
            // 
            this.tvViews.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvViews.HideSelection = false;
            this.tvViews.Location = new System.Drawing.Point(3, 16);
            this.tvViews.Name = "tvViews";
            this.tvViews.Size = new System.Drawing.Size(128, 177);
            this.tvViews.TabIndex = 1;
            this.tvViews.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvViews_AfterSelect);
            // 
            // grpParameters
            // 
            this.grpParameters.Controls.Add(this.tvParameters);
            this.grpParameters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpParameters.Location = new System.Drawing.Point(0, 0);
            this.grpParameters.Name = "grpParameters";
            this.grpParameters.Size = new System.Drawing.Size(134, 197);
            this.grpParameters.TabIndex = 0;
            this.grpParameters.TabStop = false;
            this.grpParameters.Text = "Parameters";
            // 
            // tvParameters
            // 
            this.tvParameters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvParameters.HideSelection = false;
            this.tvParameters.Location = new System.Drawing.Point(3, 16);
            this.tvParameters.Name = "tvParameters";
            this.tvParameters.Size = new System.Drawing.Size(128, 178);
            this.tvParameters.TabIndex = 0;
            this.tvParameters.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.UpdateNodeSelection);
            // 
            // ssMain
            // 
            this.ssMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblCPU,
            this.tsMem,
            this.tsFPS,
            this.tsZoom,
            this.tsCenter,
            this.tsMouse});
            this.ssMain.Location = new System.Drawing.Point(0, 429);
            this.ssMain.Name = "ssMain";
            this.ssMain.Size = new System.Drawing.Size(433, 24);
            this.ssMain.TabIndex = 4;
            this.ssMain.Text = "statusStrip1";
            // 
            // lblCPU
            // 
            this.lblCPU.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.lblCPU.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.lblCPU.Name = "lblCPU";
            this.lblCPU.Size = new System.Drawing.Size(37, 19);
            this.lblCPU.Text = "CPU:";
            // 
            // tsMem
            // 
            this.tsMem.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.tsMem.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.tsMem.Name = "tsMem";
            this.tsMem.Size = new System.Drawing.Size(42, 19);
            this.tsMem.Text = "Mem:";
            // 
            // tsFPS
            // 
            this.tsFPS.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.tsFPS.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.tsFPS.Name = "tsFPS";
            this.tsFPS.Size = new System.Drawing.Size(33, 19);
            this.tsFPS.Text = "FPS:";
            // 
            // tsZoom
            // 
            this.tsZoom.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.tsZoom.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.tsZoom.Name = "tsZoom";
            this.tsZoom.Size = new System.Drawing.Size(46, 19);
            this.tsZoom.Text = "Zoom:";
            // 
            // tsCenter
            // 
            this.tsCenter.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.tsCenter.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.tsCenter.Name = "tsCenter";
            this.tsCenter.Size = new System.Drawing.Size(49, 19);
            this.tsCenter.Text = "Center:";
            // 
            // tsMouse
            // 
            this.tsMouse.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.tsMouse.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.tsMouse.Name = "tsMouse";
            this.tsMouse.Size = new System.Drawing.Size(50, 19);
            this.tsMouse.Text = "Mouse:";
            // 
            // tmrUpdate
            // 
            this.tmrUpdate.Enabled = true;
            this.tmrUpdate.Interval = 200;
            this.tmrUpdate.Tick += new System.EventHandler(this.tmrUpdate_Tick);
            // 
            // tabDisplay
            // 
            this.tabDisplay.Controls.Add(this.tabQuick);
            this.tabDisplay.Controls.Add(this.tabOverall);
            this.tabDisplay.Controls.Add(this.tabViolations);
            this.tabDisplay.Controls.Add(this.tabLines);
            this.tabDisplay.Controls.Add(this.tabSubstations);
            this.tabDisplay.Controls.Add(this.tabAdvanced);
            this.tabDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabDisplay.Location = new System.Drawing.Point(0, 0);
            this.tabDisplay.Name = "tabDisplay";
            this.tabDisplay.SelectedIndex = 0;
            this.tabDisplay.Size = new System.Drawing.Size(433, 429);
            this.tabDisplay.TabIndex = 5;
            // 
            // tabQuick
            // 
            this.tabQuick.Controls.Add(this.grpEquipmentMonitoring);
            this.tabQuick.Controls.Add(this.grpCounty);
            this.tabQuick.Controls.Add(this.grpSubstations);
            this.tabQuick.Controls.Add(this.grpLineMVA);
            this.tabQuick.Controls.Add(this.grpLineVisibility);
            this.tabQuick.Location = new System.Drawing.Point(4, 22);
            this.tabQuick.Name = "tabQuick";
            this.tabQuick.Padding = new System.Windows.Forms.Padding(3);
            this.tabQuick.Size = new System.Drawing.Size(425, 403);
            this.tabQuick.TabIndex = 6;
            this.tabQuick.Text = "Quick";
            this.tabQuick.UseVisualStyleBackColor = true;
            // 
            // grpEquipmentMonitoring
            // 
            this.grpEquipmentMonitoring.AutoSize = true;
            this.grpEquipmentMonitoring.Controls.Add(this.lblHighVoltageAlert);
            this.grpEquipmentMonitoring.Controls.Add(this.lblHighVoltageWarning);
            this.grpEquipmentMonitoring.Controls.Add(this.lblLowVoltageAlert);
            this.grpEquipmentMonitoring.Controls.Add(this.lblThermalAlert);
            this.grpEquipmentMonitoring.Controls.Add(this.lblLowVoltageWarning);
            this.grpEquipmentMonitoring.Controls.Add(this.lblThermalWarning);
            this.grpEquipmentMonitoring.Controls.Add(this.lblVoltagePerUnit);
            this.grpEquipmentMonitoring.Controls.Add(this.lblThermalLimit);
            this.grpEquipmentMonitoring.Location = new System.Drawing.Point(8, 297);
            this.grpEquipmentMonitoring.Name = "grpEquipmentMonitoring";
            this.grpEquipmentMonitoring.Size = new System.Drawing.Size(400, 90);
            this.grpEquipmentMonitoring.TabIndex = 11;
            this.grpEquipmentMonitoring.TabStop = false;
            this.grpEquipmentMonitoring.Text = "Equipment monitoring";
            // 
            // lblHighVoltageAlert
            // 
            this.lblHighVoltageAlert.AutoSize = true;
            this.lblHighVoltageAlert.BackColor = System.Drawing.Color.Red;
            this.lblHighVoltageAlert.ForeColor = System.Drawing.Color.White;
            this.lblHighVoltageAlert.Location = new System.Drawing.Point(343, 39);
            this.lblHighVoltageAlert.Name = "lblHighVoltageAlert";
            this.lblHighVoltageAlert.Size = new System.Drawing.Size(39, 13);
            this.lblHighVoltageAlert.TabIndex = 14;
            this.lblHighVoltageAlert.Text = "High A";
            // 
            // lblHighVoltageWarning
            // 
            this.lblHighVoltageWarning.AutoSize = true;
            this.lblHighVoltageWarning.BackColor = System.Drawing.Color.Yellow;
            this.lblHighVoltageWarning.ForeColor = System.Drawing.Color.Black;
            this.lblHighVoltageWarning.Location = new System.Drawing.Point(296, 39);
            this.lblHighVoltageWarning.Name = "lblHighVoltageWarning";
            this.lblHighVoltageWarning.Size = new System.Drawing.Size(43, 13);
            this.lblHighVoltageWarning.TabIndex = 14;
            this.lblHighVoltageWarning.Text = "High W";
            // 
            // lblLowVoltageAlert
            // 
            this.lblLowVoltageAlert.AutoSize = true;
            this.lblLowVoltageAlert.BackColor = System.Drawing.Color.Red;
            this.lblLowVoltageAlert.ForeColor = System.Drawing.Color.White;
            this.lblLowVoltageAlert.Location = new System.Drawing.Point(208, 39);
            this.lblLowVoltageAlert.Name = "lblLowVoltageAlert";
            this.lblLowVoltageAlert.Size = new System.Drawing.Size(37, 13);
            this.lblLowVoltageAlert.TabIndex = 13;
            this.lblLowVoltageAlert.Text = "Low A";
            // 
            // lblThermalAlert
            // 
            this.lblThermalAlert.AutoSize = true;
            this.lblThermalAlert.BackColor = System.Drawing.Color.Red;
            this.lblThermalAlert.ForeColor = System.Drawing.Color.White;
            this.lblThermalAlert.Location = new System.Drawing.Point(155, 39);
            this.lblThermalAlert.Name = "lblThermalAlert";
            this.lblThermalAlert.Size = new System.Drawing.Size(28, 13);
            this.lblThermalAlert.TabIndex = 12;
            this.lblThermalAlert.Text = "Alert";
            // 
            // lblLowVoltageWarning
            // 
            this.lblLowVoltageWarning.AutoSize = true;
            this.lblLowVoltageWarning.BackColor = System.Drawing.Color.Yellow;
            this.lblLowVoltageWarning.ForeColor = System.Drawing.Color.Black;
            this.lblLowVoltageWarning.Location = new System.Drawing.Point(249, 39);
            this.lblLowVoltageWarning.Name = "lblLowVoltageWarning";
            this.lblLowVoltageWarning.Size = new System.Drawing.Size(41, 13);
            this.lblLowVoltageWarning.TabIndex = 11;
            this.lblLowVoltageWarning.Text = "Low W";
            // 
            // lblThermalWarning
            // 
            this.lblThermalWarning.AutoSize = true;
            this.lblThermalWarning.BackColor = System.Drawing.Color.Yellow;
            this.lblThermalWarning.ForeColor = System.Drawing.Color.Black;
            this.lblThermalWarning.Location = new System.Drawing.Point(103, 39);
            this.lblThermalWarning.Name = "lblThermalWarning";
            this.lblThermalWarning.Size = new System.Drawing.Size(47, 13);
            this.lblThermalWarning.TabIndex = 10;
            this.lblThermalWarning.Text = "Warning";
            // 
            // lblVoltagePerUnit
            // 
            this.lblVoltagePerUnit.AutoSize = true;
            this.lblVoltagePerUnit.Location = new System.Drawing.Point(245, 19);
            this.lblVoltagePerUnit.Name = "lblVoltagePerUnit";
            this.lblVoltagePerUnit.Size = new System.Drawing.Size(81, 13);
            this.lblVoltagePerUnit.TabIndex = 9;
            this.lblVoltagePerUnit.Text = "Voltage per-unit";
            // 
            // lblThermalLimit
            // 
            this.lblThermalLimit.AutoSize = true;
            this.lblThermalLimit.Location = new System.Drawing.Point(116, 19);
            this.lblThermalLimit.Name = "lblThermalLimit";
            this.lblThermalLimit.Size = new System.Drawing.Size(56, 13);
            this.lblThermalLimit.TabIndex = 8;
            this.lblThermalLimit.Text = "% Thermal";
            // 
            // grpCounty
            // 
            this.grpCounty.AutoSize = true;
            this.grpCounty.Location = new System.Drawing.Point(8, 6);
            this.grpCounty.Name = "grpCounty";
            this.grpCounty.Size = new System.Drawing.Size(338, 51);
            this.grpCounty.TabIndex = 10;
            this.grpCounty.TabStop = false;
            this.grpCounty.Text = "County";
            // 
            // grpSubstations
            // 
            this.grpSubstations.AutoSize = true;
            this.grpSubstations.Location = new System.Drawing.Point(8, 63);
            this.grpSubstations.Name = "grpSubstations";
            this.grpSubstations.Size = new System.Drawing.Size(338, 44);
            this.grpSubstations.TabIndex = 9;
            this.grpSubstations.TabStop = false;
            this.grpSubstations.Text = "Substation";
            // 
            // grpLineMVA
            // 
            this.grpLineMVA.Controls.Add(this.lblLineVisibility);
            this.grpLineMVA.Controls.Add(this.lblLineMVAAnimation);
            this.grpLineMVA.Controls.Add(this.chkLineVisibility);
            this.grpLineMVA.Controls.Add(this.rbLessThan);
            this.grpLineMVA.Controls.Add(this.tbLineVisibility);
            this.grpLineMVA.Controls.Add(this.rbGreater);
            this.grpLineMVA.Controls.Add(this.tbLineAnimation);
            this.grpLineMVA.Controls.Add(this.chkLineMVAAnimation);
            this.grpLineMVA.Location = new System.Drawing.Point(8, 175);
            this.grpLineMVA.Name = "grpLineMVA";
            this.grpLineMVA.Size = new System.Drawing.Size(398, 116);
            this.grpLineMVA.TabIndex = 8;
            this.grpLineMVA.TabStop = false;
            this.grpLineMVA.Text = "Line - MVA";
            // 
            // lblLineVisibility
            // 
            this.lblLineVisibility.AutoSize = true;
            this.lblLineVisibility.Location = new System.Drawing.Point(211, 65);
            this.lblLineVisibility.Name = "lblLineVisibility";
            this.lblLineVisibility.Size = new System.Drawing.Size(33, 13);
            this.lblLineVisibility.TabIndex = 8;
            this.lblLineVisibility.Text = "100%";
            // 
            // lblLineMVAAnimation
            // 
            this.lblLineMVAAnimation.AutoSize = true;
            this.lblLineMVAAnimation.Location = new System.Drawing.Point(211, 20);
            this.lblLineMVAAnimation.Name = "lblLineMVAAnimation";
            this.lblLineMVAAnimation.Size = new System.Drawing.Size(33, 13);
            this.lblLineMVAAnimation.TabIndex = 7;
            this.lblLineMVAAnimation.Text = "100%";
            // 
            // chkLineVisibility
            // 
            this.chkLineVisibility.AutoSize = true;
            this.chkLineVisibility.Location = new System.Drawing.Point(6, 65);
            this.chkLineVisibility.Name = "chkLineVisibility";
            this.chkLineVisibility.Size = new System.Drawing.Size(62, 17);
            this.chkLineVisibility.TabIndex = 6;
            this.chkLineVisibility.Text = "Visibility";
            this.chkLineVisibility.UseVisualStyleBackColor = true;
            this.chkLineVisibility.CheckedChanged += new System.EventHandler(this.UpdateLineVisibility);
            // 
            // rbLessThan
            // 
            this.rbLessThan.AutoSize = true;
            this.rbLessThan.Location = new System.Drawing.Point(361, 65);
            this.rbLessThan.Name = "rbLessThan";
            this.rbLessThan.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.rbLessThan.Size = new System.Drawing.Size(31, 17);
            this.rbLessThan.TabIndex = 5;
            this.rbLessThan.Text = "<";
            this.rbLessThan.UseVisualStyleBackColor = true;
            this.rbLessThan.CheckedChanged += new System.EventHandler(this.UpdateLineVisibility);
            // 
            // tbLineVisibility
            // 
            this.tbLineVisibility.BackColor = System.Drawing.SystemColors.Window;
            this.tbLineVisibility.Location = new System.Drawing.Point(111, 65);
            this.tbLineVisibility.Maximum = 125;
            this.tbLineVisibility.Minimum = 1;
            this.tbLineVisibility.Name = "tbLineVisibility";
            this.tbLineVisibility.Size = new System.Drawing.Size(244, 45);
            this.tbLineVisibility.TabIndex = 5;
            this.tbLineVisibility.Value = 1;
            this.tbLineVisibility.Scroll += new System.EventHandler(this.UpdateLineVisibility);
            // 
            // rbGreater
            // 
            this.rbGreater.AutoSize = true;
            this.rbGreater.Checked = true;
            this.rbGreater.Location = new System.Drawing.Point(74, 65);
            this.rbGreater.Name = "rbGreater";
            this.rbGreater.Size = new System.Drawing.Size(31, 17);
            this.rbGreater.TabIndex = 4;
            this.rbGreater.TabStop = true;
            this.rbGreater.Text = "<";
            this.rbGreater.UseVisualStyleBackColor = true;
            this.rbGreater.CheckedChanged += new System.EventHandler(this.UpdateLineVisibility);
            // 
            // tbLineAnimation
            // 
            this.tbLineAnimation.BackColor = System.Drawing.SystemColors.Window;
            this.tbLineAnimation.Location = new System.Drawing.Point(94, 19);
            this.tbLineAnimation.Maximum = 125;
            this.tbLineAnimation.Name = "tbLineAnimation";
            this.tbLineAnimation.Size = new System.Drawing.Size(261, 45);
            this.tbLineAnimation.TabIndex = 2;
            this.tbLineAnimation.Scroll += new System.EventHandler(this.tbLineAnimation_Scroll);
            // 
            // chkLineMVAAnimation
            // 
            this.chkLineMVAAnimation.AutoSize = true;
            this.chkLineMVAAnimation.Location = new System.Drawing.Point(6, 24);
            this.chkLineMVAAnimation.Name = "chkLineMVAAnimation";
            this.chkLineMVAAnimation.Size = new System.Drawing.Size(72, 17);
            this.chkLineMVAAnimation.TabIndex = 1;
            this.chkLineMVAAnimation.Text = "Animation";
            this.chkLineMVAAnimation.UseVisualStyleBackColor = true;
            this.chkLineMVAAnimation.CheckedChanged += new System.EventHandler(this.chkLineMVAAnimation_CheckedChanged);
            // 
            // grpLineVisibility
            // 
            this.grpLineVisibility.AutoSize = true;
            this.grpLineVisibility.Controls.Add(this.lblMVAFlowSize);
            this.grpLineVisibility.Controls.Add(this.lblNormalOpened);
            this.grpLineVisibility.Controls.Add(this.lblLineRouting);
            this.grpLineVisibility.Controls.Add(this.lblUnknown);
            this.grpLineVisibility.Controls.Add(this.lblDeEnergized);
            this.grpLineVisibility.Controls.Add(this.lblPartiallyEnergized);
            this.grpLineVisibility.Controls.Add(this.lblEnergized);
            this.grpLineVisibility.Location = new System.Drawing.Point(8, 113);
            this.grpLineVisibility.Name = "grpLineVisibility";
            this.grpLineVisibility.Size = new System.Drawing.Size(350, 56);
            this.grpLineVisibility.TabIndex = 7;
            this.grpLineVisibility.TabStop = false;
            this.grpLineVisibility.Text = "Line-Visibility";
            // 
            // lblMVAFlowSize
            // 
            this.lblMVAFlowSize.AutoSize = true;
            this.lblMVAFlowSize.Location = new System.Drawing.Point(292, 21);
            this.lblMVAFlowSize.Name = "lblMVAFlowSize";
            this.lblMVAFlowSize.Size = new System.Drawing.Size(52, 13);
            this.lblMVAFlowSize.TabIndex = 18;
            this.lblMVAFlowSize.Text = "Flow Size";
            this.tTip.SetToolTip(this.lblMVAFlowSize, "Whether to highlight normally-opened lines");
            // 
            // lblNormalOpened
            // 
            this.lblNormalOpened.AutoSize = true;
            this.lblNormalOpened.Location = new System.Drawing.Point(256, 21);
            this.lblNormalOpened.Name = "lblNormalOpened";
            this.lblNormalOpened.Size = new System.Drawing.Size(29, 13);
            this.lblNormalOpened.TabIndex = 17;
            this.lblNormalOpened.Text = "N.O.";
            this.tTip.SetToolTip(this.lblNormalOpened, "Whether to highlight normally-opened lines");
            this.lblNormalOpened.Click += new System.EventHandler(this.BulkUpdateByEnergizedState);
            // 
            // lblLineRouting
            // 
            this.lblLineRouting.AutoSize = true;
            this.lblLineRouting.Location = new System.Drawing.Point(220, 21);
            this.lblLineRouting.Name = "lblLineRouting";
            this.lblLineRouting.Size = new System.Drawing.Size(27, 13);
            this.lblLineRouting.TabIndex = 16;
            this.lblLineRouting.Text = "Geo";
            this.tTip.SetToolTip(this.lblLineRouting, "Whether to show geographically-accurate lines (as opposed to straight lines betwe" +
        "en substations)");
            this.lblLineRouting.Click += new System.EventHandler(this.BulkUpdateByEnergizedState);
            // 
            // lblUnknown
            // 
            this.lblUnknown.AutoSize = true;
            this.lblUnknown.Location = new System.Drawing.Point(192, 21);
            this.lblUnknown.Name = "lblUnknown";
            this.lblUnknown.Size = new System.Drawing.Size(13, 13);
            this.lblUnknown.TabIndex = 15;
            this.lblUnknown.Text = "?";
            this.lblUnknown.Click += new System.EventHandler(this.BulkUpdateByEnergizedState);
            // 
            // lblDeEnergized
            // 
            this.lblDeEnergized.AutoSize = true;
            this.lblDeEnergized.Location = new System.Drawing.Point(151, 21);
            this.lblDeEnergized.Name = "lblDeEnergized";
            this.lblDeEnergized.Size = new System.Drawing.Size(22, 13);
            this.lblDeEnergized.TabIndex = 14;
            this.lblDeEnergized.Text = "DE";
            this.lblDeEnergized.Click += new System.EventHandler(this.BulkUpdateByEnergizedState);
            // 
            // lblPartiallyEnergized
            // 
            this.lblPartiallyEnergized.AutoSize = true;
            this.lblPartiallyEnergized.Location = new System.Drawing.Point(115, 21);
            this.lblPartiallyEnergized.Name = "lblPartiallyEnergized";
            this.lblPartiallyEnergized.Size = new System.Drawing.Size(21, 13);
            this.lblPartiallyEnergized.TabIndex = 13;
            this.lblPartiallyEnergized.Text = "PE";
            this.lblPartiallyEnergized.Click += new System.EventHandler(this.BulkUpdateByEnergizedState);
            // 
            // lblEnergized
            // 
            this.lblEnergized.AutoSize = true;
            this.lblEnergized.Location = new System.Drawing.Point(76, 21);
            this.lblEnergized.Name = "lblEnergized";
            this.lblEnergized.Size = new System.Drawing.Size(26, 13);
            this.lblEnergized.TabIndex = 7;
            this.lblEnergized.Text = "Eng";
            this.lblEnergized.Click += new System.EventHandler(this.BulkUpdateByEnergizedState);
            // 
            // tabOverall
            // 
            this.tabOverall.Location = new System.Drawing.Point(4, 22);
            this.tabOverall.Name = "tabOverall";
            this.tabOverall.Padding = new System.Windows.Forms.Padding(3);
            this.tabOverall.Size = new System.Drawing.Size(425, 403);
            this.tabOverall.TabIndex = 5;
            this.tabOverall.Text = "Overall";
            this.tabOverall.UseVisualStyleBackColor = true;
            // 
            // tabViolations
            // 
            this.tabViolations.Location = new System.Drawing.Point(4, 22);
            this.tabViolations.Name = "tabViolations";
            this.tabViolations.Padding = new System.Windows.Forms.Padding(3);
            this.tabViolations.Size = new System.Drawing.Size(425, 403);
            this.tabViolations.TabIndex = 0;
            this.tabViolations.Text = "Violations";
            this.tabViolations.UseVisualStyleBackColor = true;
            // 
            // tabLines
            // 
            this.tabLines.Location = new System.Drawing.Point(4, 22);
            this.tabLines.Name = "tabLines";
            this.tabLines.Padding = new System.Windows.Forms.Padding(3);
            this.tabLines.Size = new System.Drawing.Size(425, 403);
            this.tabLines.TabIndex = 1;
            this.tabLines.Text = "Lines";
            this.tabLines.UseVisualStyleBackColor = true;
            // 
            // tabSubstations
            // 
            this.tabSubstations.Location = new System.Drawing.Point(4, 22);
            this.tabSubstations.Name = "tabSubstations";
            this.tabSubstations.Padding = new System.Windows.Forms.Padding(3);
            this.tabSubstations.Size = new System.Drawing.Size(425, 403);
            this.tabSubstations.TabIndex = 2;
            this.tabSubstations.Text = "Substations";
            this.tabSubstations.UseVisualStyleBackColor = true;
            // 
            // tabAdvanced
            // 
            this.tabAdvanced.Controls.Add(this.splMain);
            this.tabAdvanced.Location = new System.Drawing.Point(4, 22);
            this.tabAdvanced.Name = "tabAdvanced";
            this.tabAdvanced.Padding = new System.Windows.Forms.Padding(3);
            this.tabAdvanced.Size = new System.Drawing.Size(425, 403);
            this.tabAdvanced.TabIndex = 4;
            this.tabAdvanced.Text = "Advanced";
            this.tabAdvanced.UseVisualStyleBackColor = true;
            // 
            // MM_Display_Options
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(433, 453);
            this.Controls.Add(this.tabDisplay);
            this.Controls.Add(this.ssMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = Properties.Resources.CompanyIcon;
            this.Location = new System.Drawing.Point(0, 0);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MM_Display_Options";
            this.Text = " Macomber Map Display Options";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Display_Options_FormClosing);
            this.splMain.Panel1.ResumeLayout(false);
            this.splMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splMain)).EndInit();
            this.splMain.ResumeLayout(false);
            this.splLeft.Panel1.ResumeLayout(false);
            this.splLeft.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splLeft)).EndInit();
            this.splLeft.ResumeLayout(false);
            this.grpViews.ResumeLayout(false);
            this.grpParameters.ResumeLayout(false);
            this.ssMain.ResumeLayout(false);
            this.ssMain.PerformLayout();
            this.tabDisplay.ResumeLayout(false);
            this.tabQuick.ResumeLayout(false);
            this.tabQuick.PerformLayout();
            this.grpEquipmentMonitoring.ResumeLayout(false);
            this.grpEquipmentMonitoring.PerformLayout();
            this.grpLineMVA.ResumeLayout(false);
            this.grpLineMVA.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbLineVisibility)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbLineAnimation)).EndInit();
            this.grpLineVisibility.ResumeLayout(false);
            this.grpLineVisibility.PerformLayout();
            this.tabAdvanced.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PropertyGrid pgMain;
        private System.Windows.Forms.SplitContainer splMain;
        private System.Windows.Forms.TreeView tvParameters;
        private System.Windows.Forms.StatusStrip ssMain;
        private System.Windows.Forms.ToolStripStatusLabel tsMem;
        private System.Windows.Forms.ToolStripStatusLabel tsFPS;
        private System.Windows.Forms.ToolStripStatusLabel tsZoom;
        private System.Windows.Forms.ToolStripStatusLabel tsCenter;
        private System.Windows.Forms.Timer tmrUpdate;
        private System.Windows.Forms.ToolStripStatusLabel tsMouse;
        private System.Windows.Forms.SplitContainer splLeft;
        private System.Windows.Forms.GroupBox grpViews;
        private System.Windows.Forms.TreeView tvViews;
        private System.Windows.Forms.GroupBox grpParameters;
        private System.Windows.Forms.TabControl tabDisplay;
        private System.Windows.Forms.TabPage tabViolations;
        private System.Windows.Forms.TabPage tabLines;
        private System.Windows.Forms.TabPage tabSubstations;
        private System.Windows.Forms.TabPage tabAdvanced;
        private System.Windows.Forms.TabPage tabOverall;
        private System.Windows.Forms.ToolTip tTip;
        private System.Windows.Forms.TabPage tabQuick;
        private System.Windows.Forms.GroupBox grpSubstations;
        private System.Windows.Forms.GroupBox grpLineMVA;
        private System.Windows.Forms.CheckBox chkLineVisibility;
        private System.Windows.Forms.RadioButton rbLessThan;
        private System.Windows.Forms.TrackBar tbLineVisibility;
        private System.Windows.Forms.RadioButton rbGreater;
        private System.Windows.Forms.TrackBar tbLineAnimation;
        private System.Windows.Forms.CheckBox chkLineMVAAnimation;
        private System.Windows.Forms.GroupBox grpLineVisibility;
        private System.Windows.Forms.Label lblUnknown;
        private System.Windows.Forms.Label lblDeEnergized;
        private System.Windows.Forms.Label lblPartiallyEnergized;
        private System.Windows.Forms.Label lblEnergized;
        private System.Windows.Forms.GroupBox grpCounty;
        private System.Windows.Forms.ToolStripStatusLabel lblCPU;
        private System.Windows.Forms.Label lblLineRouting;
        private System.Windows.Forms.Label lblNormalOpened;
        private System.Windows.Forms.GroupBox grpEquipmentMonitoring;
        private System.Windows.Forms.Label lblHighVoltageAlert;
        private System.Windows.Forms.Label lblHighVoltageWarning;
        private System.Windows.Forms.Label lblLowVoltageAlert;
        private System.Windows.Forms.Label lblThermalAlert;
        private System.Windows.Forms.Label lblLowVoltageWarning;
        private System.Windows.Forms.Label lblThermalWarning;
        private System.Windows.Forms.Label lblVoltagePerUnit;
        private System.Windows.Forms.Label lblThermalLimit;
        private System.Windows.Forms.Label lblLineMVAAnimation;
        private System.Windows.Forms.Label lblLineVisibility;
        private System.Windows.Forms.Label lblMVAFlowSize;

    }
}