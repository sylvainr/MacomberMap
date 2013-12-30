namespace Macomber_Map.User_Interface_Components
{
    partial class Display_Options
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Display_Options));
            this.pgMain = new System.Windows.Forms.PropertyGrid();
            this.splMain = new System.Windows.Forms.SplitContainer();
            this.splLeft = new System.Windows.Forms.SplitContainer();
            this.grpViews = new System.Windows.Forms.GroupBox();
            this.tvViews = new System.Windows.Forms.TreeView();
            this.grpParameters = new System.Windows.Forms.GroupBox();
            this.tvParameters = new System.Windows.Forms.TreeView();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tsMem = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsFPS = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsZoom = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsCenter = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsMouse = new System.Windows.Forms.ToolStripStatusLabel();
            this.tmrUpdate = new System.Windows.Forms.Timer(this.components);
            this.tabDisplay = new System.Windows.Forms.TabControl();
            this.tabQuick = new System.Windows.Forms.TabPage();
            this.grpCounty = new System.Windows.Forms.GroupBox();
            this.grpSubstations = new System.Windows.Forms.GroupBox();
            this.grpLineMVA = new System.Windows.Forms.GroupBox();
            this.chkLineVisibility = new System.Windows.Forms.CheckBox();
            this.rbLessThan = new System.Windows.Forms.RadioButton();
            this.tbLineVisibility = new System.Windows.Forms.TrackBar();
            this.rbGreater = new System.Windows.Forms.RadioButton();
            this.tbLineAnimation = new System.Windows.Forms.TrackBar();
            this.chkLineMVAAnimation = new System.Windows.Forms.CheckBox();
            this.grpLineVisibility = new System.Windows.Forms.GroupBox();
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
            this.splMain.Panel1.SuspendLayout();
            this.splMain.Panel2.SuspendLayout();
            this.splMain.SuspendLayout();
            this.splLeft.Panel1.SuspendLayout();
            this.splLeft.Panel2.SuspendLayout();
            this.splLeft.SuspendLayout();
            this.grpViews.SuspendLayout();
            this.grpParameters.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.tabDisplay.SuspendLayout();
            this.tabQuick.SuspendLayout();
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
            this.pgMain.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pgMain.Name = "pgMain";
            this.pgMain.Size = new System.Drawing.Size(540, 715);
            this.pgMain.TabIndex = 1;
            // 
            // splMain
            // 
            this.splMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splMain.Location = new System.Drawing.Point(4, 4);
            this.splMain.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.splMain.Name = "splMain";
            // 
            // splMain.Panel1
            // 
            this.splMain.Panel1.Controls.Add(this.splLeft);
            // 
            // splMain.Panel2
            // 
            this.splMain.Panel2.Controls.Add(this.pgMain);
            this.splMain.Size = new System.Drawing.Size(805, 715);
            this.splMain.SplitterDistance = 260;
            this.splMain.SplitterWidth = 5;
            this.splMain.TabIndex = 2;
            // 
            // splLeft
            // 
            this.splLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splLeft.Location = new System.Drawing.Point(0, 0);
            this.splLeft.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
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
            this.splLeft.Size = new System.Drawing.Size(260, 715);
            this.splLeft.SplitterDistance = 356;
            this.splLeft.SplitterWidth = 5;
            this.splLeft.TabIndex = 2;
            // 
            // grpViews
            // 
            this.grpViews.Controls.Add(this.tvViews);
            this.grpViews.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpViews.Location = new System.Drawing.Point(0, 0);
            this.grpViews.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpViews.Name = "grpViews";
            this.grpViews.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpViews.Size = new System.Drawing.Size(260, 356);
            this.grpViews.TabIndex = 1;
            this.grpViews.TabStop = false;
            this.grpViews.Text = "Views";
            // 
            // tvViews
            // 
            this.tvViews.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvViews.HideSelection = false;
            this.tvViews.Location = new System.Drawing.Point(4, 19);
            this.tvViews.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tvViews.Name = "tvViews";
            this.tvViews.Size = new System.Drawing.Size(252, 333);
            this.tvViews.TabIndex = 1;
            this.tvViews.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvViews_AfterSelect);
            // 
            // grpParameters
            // 
            this.grpParameters.Controls.Add(this.tvParameters);
            this.grpParameters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpParameters.Location = new System.Drawing.Point(0, 0);
            this.grpParameters.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpParameters.Name = "grpParameters";
            this.grpParameters.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpParameters.Size = new System.Drawing.Size(260, 354);
            this.grpParameters.TabIndex = 0;
            this.grpParameters.TabStop = false;
            this.grpParameters.Text = "Parameters";
            // 
            // tvParameters
            // 
            this.tvParameters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvParameters.HideSelection = false;
            this.tvParameters.Location = new System.Drawing.Point(4, 19);
            this.tvParameters.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tvParameters.Name = "tvParameters";
            this.tvParameters.Size = new System.Drawing.Size(252, 331);
            this.tvParameters.TabIndex = 0;
            this.tvParameters.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.UpdateNodeSelection);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsMem,
            this.tsFPS,
            this.tsZoom,
            this.tsCenter,
            this.tsMouse});
            this.statusStrip1.Location = new System.Drawing.Point(0, 753);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 19, 0);
            this.statusStrip1.Size = new System.Drawing.Size(821, 29);
            this.statusStrip1.TabIndex = 4;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tsMem
            // 
            this.tsMem.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.tsMem.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.tsMem.Name = "tsMem";
            this.tsMem.Size = new System.Drawing.Size(50, 24);
            this.tsMem.Text = "Mem:";
            // 
            // tsFPS
            // 
            this.tsFPS.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.tsFPS.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.tsFPS.Name = "tsFPS";
            this.tsFPS.Size = new System.Drawing.Size(39, 24);
            this.tsFPS.Text = "FPS:";
            // 
            // tsZoom
            // 
            this.tsZoom.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.tsZoom.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.tsZoom.Name = "tsZoom";
            this.tsZoom.Size = new System.Drawing.Size(56, 24);
            this.tsZoom.Text = "Zoom:";
            // 
            // tsCenter
            // 
            this.tsCenter.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.tsCenter.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.tsCenter.Name = "tsCenter";
            this.tsCenter.Size = new System.Drawing.Size(59, 24);
            this.tsCenter.Text = "Center:";
            // 
            // tsMouse
            // 
            this.tsMouse.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.tsMouse.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
            this.tsMouse.Name = "tsMouse";
            this.tsMouse.Size = new System.Drawing.Size(60, 24);
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
            this.tabDisplay.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabDisplay.Name = "tabDisplay";
            this.tabDisplay.SelectedIndex = 0;
            this.tabDisplay.Size = new System.Drawing.Size(821, 753);
            this.tabDisplay.TabIndex = 5;
            // 
            // tabQuick
            // 
            this.tabQuick.Controls.Add(this.grpCounty);
            this.tabQuick.Controls.Add(this.grpSubstations);
            this.tabQuick.Controls.Add(this.grpLineMVA);
            this.tabQuick.Controls.Add(this.grpLineVisibility);
            this.tabQuick.Location = new System.Drawing.Point(4, 25);
            this.tabQuick.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabQuick.Name = "tabQuick";
            this.tabQuick.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabQuick.Size = new System.Drawing.Size(813, 724);
            this.tabQuick.TabIndex = 6;
            this.tabQuick.Text = "Quick";
            this.tabQuick.UseVisualStyleBackColor = true;
            // 
            // grpCounty
            // 
            this.grpCounty.Location = new System.Drawing.Point(11, 164);
            this.grpCounty.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpCounty.Name = "grpCounty";
            this.grpCounty.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpCounty.Size = new System.Drawing.Size(428, 107);
            this.grpCounty.TabIndex = 10;
            this.grpCounty.TabStop = false;
            this.grpCounty.Text = "County";
            // 
            // grpSubstations
            // 
            this.grpSubstations.Location = new System.Drawing.Point(11, 7);
            this.grpSubstations.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpSubstations.Name = "grpSubstations";
            this.grpSubstations.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpSubstations.Size = new System.Drawing.Size(428, 149);
            this.grpSubstations.TabIndex = 9;
            this.grpSubstations.TabStop = false;
            this.grpSubstations.Text = "Substation";
            // 
            // grpLineMVA
            // 
            this.grpLineMVA.Controls.Add(this.chkLineVisibility);
            this.grpLineMVA.Controls.Add(this.rbLessThan);
            this.grpLineMVA.Controls.Add(this.tbLineVisibility);
            this.grpLineMVA.Controls.Add(this.rbGreater);
            this.grpLineMVA.Controls.Add(this.tbLineAnimation);
            this.grpLineMVA.Controls.Add(this.chkLineMVAAnimation);
            this.grpLineMVA.Location = new System.Drawing.Point(11, 278);
            this.grpLineMVA.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpLineMVA.Name = "grpLineMVA";
            this.grpLineMVA.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpLineMVA.Size = new System.Drawing.Size(531, 143);
            this.grpLineMVA.TabIndex = 8;
            this.grpLineMVA.TabStop = false;
            this.grpLineMVA.Text = "Line - MVA";
            // 
            // chkLineVisibility
            // 
            this.chkLineVisibility.AutoSize = true;
            this.chkLineVisibility.Location = new System.Drawing.Point(8, 80);
            this.chkLineVisibility.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkLineVisibility.Name = "chkLineVisibility";
            this.chkLineVisibility.Size = new System.Drawing.Size(80, 21);
            this.chkLineVisibility.TabIndex = 6;
            this.chkLineVisibility.Text = "Visibility";
            this.chkLineVisibility.UseVisualStyleBackColor = true;
            this.chkLineVisibility.CheckedChanged += new System.EventHandler(this.UpdateLineVisibility);
            // 
            // rbLessThan
            // 
            this.rbLessThan.AutoSize = true;
            this.rbLessThan.Location = new System.Drawing.Point(481, 80);
            this.rbLessThan.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rbLessThan.Name = "rbLessThan";
            this.rbLessThan.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.rbLessThan.Size = new System.Drawing.Size(37, 21);
            this.rbLessThan.TabIndex = 5;
            this.rbLessThan.Text = "<";
            this.rbLessThan.UseVisualStyleBackColor = true;
            this.rbLessThan.CheckedChanged += new System.EventHandler(this.UpdateLineVisibility);
            // 
            // tbLineVisibility
            // 
            this.tbLineVisibility.BackColor = System.Drawing.SystemColors.Window;
            this.tbLineVisibility.Location = new System.Drawing.Point(148, 80);
            this.tbLineVisibility.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbLineVisibility.Maximum = 200;
            this.tbLineVisibility.Minimum = 1;
            this.tbLineVisibility.Name = "tbLineVisibility";
            this.tbLineVisibility.Size = new System.Drawing.Size(325, 56);
            this.tbLineVisibility.TabIndex = 5;
            this.tbLineVisibility.Value = 1;
            this.tbLineVisibility.Scroll += new System.EventHandler(this.UpdateLineVisibility);
            // 
            // rbGreater
            // 
            this.rbGreater.AutoSize = true;
            this.rbGreater.Checked = true;
            this.rbGreater.Location = new System.Drawing.Point(99, 80);
            this.rbGreater.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rbGreater.Name = "rbGreater";
            this.rbGreater.Size = new System.Drawing.Size(37, 21);
            this.rbGreater.TabIndex = 4;
            this.rbGreater.TabStop = true;
            this.rbGreater.Text = "<";
            this.rbGreater.UseVisualStyleBackColor = true;
            this.rbGreater.CheckedChanged += new System.EventHandler(this.UpdateLineVisibility);
            // 
            // tbLineAnimation
            // 
            this.tbLineAnimation.BackColor = System.Drawing.SystemColors.Window;
            this.tbLineAnimation.Location = new System.Drawing.Point(125, 23);
            this.tbLineAnimation.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbLineAnimation.Maximum = 200;
            this.tbLineAnimation.Name = "tbLineAnimation";
            this.tbLineAnimation.Size = new System.Drawing.Size(348, 56);
            this.tbLineAnimation.TabIndex = 2;
            this.tbLineAnimation.Scroll += new System.EventHandler(this.tbLineAnimation_Scroll);
            // 
            // chkLineMVAAnimation
            // 
            this.chkLineMVAAnimation.AutoSize = true;
            this.chkLineMVAAnimation.Location = new System.Drawing.Point(8, 30);
            this.chkLineMVAAnimation.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkLineMVAAnimation.Name = "chkLineMVAAnimation";
            this.chkLineMVAAnimation.Size = new System.Drawing.Size(92, 21);
            this.chkLineMVAAnimation.TabIndex = 1;
            this.chkLineMVAAnimation.Text = "Animation";
            this.chkLineMVAAnimation.UseVisualStyleBackColor = true;
            this.chkLineMVAAnimation.CheckedChanged += new System.EventHandler(this.chkLineMVAAnimation_CheckedChanged);
            // 
            // grpLineVisibility
            // 
            this.grpLineVisibility.Controls.Add(this.lblUnknown);
            this.grpLineVisibility.Controls.Add(this.lblDeEnergized);
            this.grpLineVisibility.Controls.Add(this.lblPartiallyEnergized);
            this.grpLineVisibility.Controls.Add(this.lblEnergized);
            this.grpLineVisibility.Location = new System.Drawing.Point(447, 7);
            this.grpLineVisibility.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpLineVisibility.Name = "grpLineVisibility";
            this.grpLineVisibility.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpLineVisibility.Size = new System.Drawing.Size(289, 263);
            this.grpLineVisibility.TabIndex = 7;
            this.grpLineVisibility.TabStop = false;
            this.grpLineVisibility.Text = "Line-Visibility";
            // 
            // lblUnknown
            // 
            this.lblUnknown.AutoSize = true;
            this.lblUnknown.Location = new System.Drawing.Point(256, 26);
            this.lblUnknown.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblUnknown.Name = "lblUnknown";
            this.lblUnknown.Size = new System.Drawing.Size(16, 17);
            this.lblUnknown.TabIndex = 15;
            this.lblUnknown.Text = "?";
            this.lblUnknown.Click += new System.EventHandler(this.BulkUpdateByEnergizedState);
            // 
            // lblDeEnergized
            // 
            this.lblDeEnergized.AutoSize = true;
            this.lblDeEnergized.Location = new System.Drawing.Point(201, 26);
            this.lblDeEnergized.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDeEnergized.Name = "lblDeEnergized";
            this.lblDeEnergized.Size = new System.Drawing.Size(27, 17);
            this.lblDeEnergized.TabIndex = 14;
            this.lblDeEnergized.Text = "DE";
            this.lblDeEnergized.Click += new System.EventHandler(this.BulkUpdateByEnergizedState);
            // 
            // lblPartiallyEnergized
            // 
            this.lblPartiallyEnergized.AutoSize = true;
            this.lblPartiallyEnergized.Location = new System.Drawing.Point(153, 26);
            this.lblPartiallyEnergized.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPartiallyEnergized.Name = "lblPartiallyEnergized";
            this.lblPartiallyEnergized.Size = new System.Drawing.Size(26, 17);
            this.lblPartiallyEnergized.TabIndex = 13;
            this.lblPartiallyEnergized.Text = "PE";
            this.lblPartiallyEnergized.Click += new System.EventHandler(this.BulkUpdateByEnergizedState);
            // 
            // lblEnergized
            // 
            this.lblEnergized.AutoSize = true;
            this.lblEnergized.Location = new System.Drawing.Point(101, 26);
            this.lblEnergized.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblEnergized.Name = "lblEnergized";
            this.lblEnergized.Size = new System.Drawing.Size(33, 17);
            this.lblEnergized.TabIndex = 7;
            this.lblEnergized.Text = "Eng";
            this.lblEnergized.Click += new System.EventHandler(this.BulkUpdateByEnergizedState);
            // 
            // tabOverall
            // 
            this.tabOverall.Location = new System.Drawing.Point(4, 25);
            this.tabOverall.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabOverall.Name = "tabOverall";
            this.tabOverall.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabOverall.Size = new System.Drawing.Size(813, 723);
            this.tabOverall.TabIndex = 5;
            this.tabOverall.Text = "Overall";
            this.tabOverall.UseVisualStyleBackColor = true;
            // 
            // tabViolations
            // 
            this.tabViolations.Location = new System.Drawing.Point(4, 25);
            this.tabViolations.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabViolations.Name = "tabViolations";
            this.tabViolations.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabViolations.Size = new System.Drawing.Size(813, 723);
            this.tabViolations.TabIndex = 0;
            this.tabViolations.Text = "Violations";
            this.tabViolations.UseVisualStyleBackColor = true;
            // 
            // tabLines
            // 
            this.tabLines.Location = new System.Drawing.Point(4, 25);
            this.tabLines.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabLines.Name = "tabLines";
            this.tabLines.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabLines.Size = new System.Drawing.Size(813, 723);
            this.tabLines.TabIndex = 1;
            this.tabLines.Text = "Lines";
            this.tabLines.UseVisualStyleBackColor = true;
            // 
            // tabSubstations
            // 
            this.tabSubstations.Location = new System.Drawing.Point(4, 25);
            this.tabSubstations.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabSubstations.Name = "tabSubstations";
            this.tabSubstations.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabSubstations.Size = new System.Drawing.Size(813, 723);
            this.tabSubstations.TabIndex = 2;
            this.tabSubstations.Text = "Substations";
            this.tabSubstations.UseVisualStyleBackColor = true;
            // 
            // tabAdvanced
            // 
            this.tabAdvanced.Controls.Add(this.splMain);
            this.tabAdvanced.Location = new System.Drawing.Point(4, 25);
            this.tabAdvanced.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabAdvanced.Name = "tabAdvanced";
            this.tabAdvanced.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabAdvanced.Size = new System.Drawing.Size(813, 723);
            this.tabAdvanced.TabIndex = 4;
            this.tabAdvanced.Text = "Advanced";
            this.tabAdvanced.UseVisualStyleBackColor = true;
            // 
            // Display_Options
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(821, 782);
            this.Controls.Add(this.tabDisplay);
            this.Controls.Add(this.statusStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Location = new System.Drawing.Point(0, 0);
            this.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.Name = "Display_Options";
            this.Text = " Macomber Map Display Options";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Display_Options_FormClosing);
            this.splMain.Panel1.ResumeLayout(false);
            this.splMain.Panel2.ResumeLayout(false);
            this.splMain.ResumeLayout(false);
            this.splLeft.Panel1.ResumeLayout(false);
            this.splLeft.Panel2.ResumeLayout(false);
            this.splLeft.ResumeLayout(false);
            this.grpViews.ResumeLayout(false);
            this.grpParameters.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabDisplay.ResumeLayout(false);
            this.tabQuick.ResumeLayout(false);
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
        private System.Windows.Forms.StatusStrip statusStrip1;
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

    }
}