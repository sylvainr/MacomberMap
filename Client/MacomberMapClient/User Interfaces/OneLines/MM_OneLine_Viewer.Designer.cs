using System.Windows.Forms;
namespace MacomberMapClient.User_Interfaces.OneLines
{
    partial class MM_OneLine_Viewer
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;


        private ToolStrip tsMain;
        private ToolStripButton tsBack;
        private ToolStripButton tsForward;
        private ToolStripSeparator tss1;
        private ToolStripLabel tsSubstation;
        private ToolStripComboBox cmbSubstation;
        private ToolStripSeparator tss2;
        private ToolStripSeparator tss3;
        private ToolStripSeparator tss4;
        private ToolStripSeparator tss5;
        private ToolStripSeparator tss6;
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
            this.tsMain = new System.Windows.Forms.ToolStrip();
            this.tsBack = new System.Windows.Forms.ToolStripButton();
            this.tsForward = new System.Windows.Forms.ToolStripButton();
            this.tss1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsSubstation = new System.Windows.Forms.ToolStripLabel();
            this.cmbSubstation = new System.Windows.Forms.ToolStripComboBox();
            this.tss2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnDataSource = new System.Windows.Forms.ToolStripDropDownButton();
            this.tss3 = new System.Windows.Forms.ToolStripSeparator();
            this.lblZoom = new System.Windows.Forms.ToolStripLabel();
            this.btnFitToPage = new System.Windows.Forms.ToolStripButton();
            this.btnZoomDown = new System.Windows.Forms.ToolStripButton();
            this.txtZoomLevel = new System.Windows.Forms.ToolStripTextBox();
            this.btnZoomUp = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnFollowMap = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnNames = new System.Windows.Forms.ToolStripButton();
            this.tss4 = new System.Windows.Forms.ToolStripSeparator();
            this.btnPercent = new System.Windows.Forms.ToolStripButton();
            this.btnMVA = new System.Windows.Forms.ToolStripButton();
            this.btnMW = new System.Windows.Forms.ToolStripButton();
            this.btnMVAR = new System.Windows.Forms.ToolStripButton();
            this.tss5 = new System.Windows.Forms.ToolStripSeparator();
            this.btnOtherLine = new System.Windows.Forms.ToolStripButton();
            this.tss6 = new System.Windows.Forms.ToolStripSeparator();
            this.btnVoltage = new System.Windows.Forms.ToolStripButton();
            this.btnAngle = new System.Windows.Forms.ToolStripButton();
            this.btnFrequency = new System.Windows.Forms.ToolStripButton();
            this.lblHighlightDiffs = new System.Windows.Forms.ToolStripDropDownButton();
            this.breakerStateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.switchStateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mWToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mVARToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mVAToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gMWToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gMVToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.analogThresholdToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.txtAnalogThreshold = new System.Windows.Forms.ToolStripTextBox();
            this.tsIgnoreMissing = new System.Windows.Forms.ToolStripMenuItem();
            this.btnSwapDetails = new System.Windows.Forms.ToolStripButton();
            this.btnDisplay = new System.Windows.Forms.ToolStripDropDownButton();
            this.breakersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.capacitorsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.endcapsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuLines = new System.Windows.Forms.ToolStripMenuItem();
            this.loadsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuNodes = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuNodeToElementLines = new System.Windows.Forms.ToolStripMenuItem();
            this.pricingVectorsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reactorsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.switchesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.transformersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.unitsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.shapesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.arrowsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlElements = new MacomberMapClient.User_Interfaces.OneLines.MM_OneLine_Viewer.DoubleBufferedPanel();
            this.tmrUpdate = new System.Windows.Forms.Timer(this.components);
            this.tsMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // tsMain
            // 
            this.tsMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsBack,
            this.tsForward,
            this.tss1,
            this.tsSubstation,
            this.cmbSubstation,
            this.tss2,
            this.btnDataSource,
            this.tss3,
            this.lblZoom,
            this.btnFitToPage,
            this.btnZoomDown,
            this.txtZoomLevel,
            this.btnZoomUp,
            this.toolStripSeparator1,
            this.btnFollowMap,
            this.toolStripSeparator2,
            this.btnNames,
            this.tss4,
            this.btnPercent,
            this.btnMVA,
            this.btnMW,
            this.btnMVAR,
            this.tss5,
            this.btnOtherLine,
            this.tss6,
            this.btnVoltage,
            this.btnAngle,
            this.btnFrequency,
            this.lblHighlightDiffs,
            this.btnSwapDetails,
            this.btnDisplay});
            this.tsMain.Location = new System.Drawing.Point(0, 0);
            this.tsMain.Name = "tsMain";
            this.tsMain.Size = new System.Drawing.Size(1474, 25);
            this.tsMain.TabIndex = 0;
            this.tsMain.Text = "Main Toolstrip";
            // 
            // tsBack
            // 
            this.tsBack.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsBack.Image = global::MacomberMapClient.Properties.Resources.Back;
            this.tsBack.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsBack.Name = "tsBack";
            this.tsBack.Size = new System.Drawing.Size(23, 22);
            this.tsBack.Text = "&Back";
            this.tsBack.Click += new System.EventHandler(this.HandleDirectionalClick);
            // 
            // tsForward
            // 
            this.tsForward.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsForward.Image = global::MacomberMapClient.Properties.Resources.Forward;
            this.tsForward.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsForward.Name = "tsForward";
            this.tsForward.Size = new System.Drawing.Size(23, 22);
            this.tsForward.Text = "&Forward";
            this.tsForward.Click += new System.EventHandler(this.HandleDirectionalClick);
            // 
            // tss1
            // 
            this.tss1.Name = "tss1";
            this.tss1.Size = new System.Drawing.Size(6, 25);
            // 
            // tsSubstation
            // 
            this.tsSubstation.Name = "tsSubstation";
            this.tsSubstation.Size = new System.Drawing.Size(104, 22);
            this.tsSubstation.Text = "Substation History";
            // 
            // cmbSubstation
            // 
            this.cmbSubstation.Name = "cmbSubstation";
            this.cmbSubstation.Size = new System.Drawing.Size(121, 25);
            this.cmbSubstation.SelectedIndexChanged += new System.EventHandler(this.cmbSubstation_SelectedIndexChanged);
            this.cmbSubstation.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cmbSubstation_KeyPress);
            // 
            // tss2
            // 
            this.tss2.Name = "tss2";
            this.tss2.Size = new System.Drawing.Size(6, 25);
            // 
            // btnDataSource
            // 
            this.btnDataSource.Image = global::MacomberMapClient.Properties.Resources.DataSource;
            this.btnDataSource.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnDataSource.Name = "btnDataSource";
            this.btnDataSource.Size = new System.Drawing.Size(69, 22);
            this.btnDataSource.Text = "RTNet";
            this.btnDataSource.ToolTipText = "Data Source";
            this.btnDataSource.Visible = false;
            this.btnDataSource.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.btnDataSource_DropDownItemClicked);
            // 
            // tss3
            // 
            this.tss3.Name = "tss3";
            this.tss3.Size = new System.Drawing.Size(6, 25);
            // 
            // lblZoom
            // 
            this.lblZoom.Name = "lblZoom";
            this.lblZoom.Size = new System.Drawing.Size(39, 22);
            this.lblZoom.Text = "Zoom";
            // 
            // btnFitToPage
            // 
            this.btnFitToPage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnFitToPage.Image = global::MacomberMapClient.Properties.Resources.PageWidth;
            this.btnFitToPage.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnFitToPage.Name = "btnFitToPage";
            this.btnFitToPage.Size = new System.Drawing.Size(23, 22);
            this.btnFitToPage.Text = "Fit to page";
            this.btnFitToPage.Click += new System.EventHandler(this.btnFitToPage_Click);
            // 
            // btnZoomDown
            // 
            this.btnZoomDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnZoomDown.Image = global::MacomberMapClient.Properties.Resources.Expand_large;
            this.btnZoomDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnZoomDown.Name = "btnZoomDown";
            this.btnZoomDown.Size = new System.Drawing.Size(23, 22);
            this.btnZoomDown.Text = "Zoom Down";
            this.btnZoomDown.Click += new System.EventHandler(this.HandleZoomButton);
            // 
            // txtZoomLevel
            // 
            this.txtZoomLevel.Name = "txtZoomLevel";
            this.txtZoomLevel.Size = new System.Drawing.Size(30, 25);
            this.txtZoomLevel.Text = "100";
            // 
            // btnZoomUp
            // 
            this.btnZoomUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnZoomUp.Image = global::MacomberMapClient.Properties.Resources.Collapse_large;
            this.btnZoomUp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnZoomUp.Name = "btnZoomUp";
            this.btnZoomUp.Size = new System.Drawing.Size(23, 22);
            this.btnZoomUp.Text = "Zoom Up";
            this.btnZoomUp.Click += new System.EventHandler(this.HandleZoomButton);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // btnFollowMap
            // 
            this.btnFollowMap.Checked = false;
            this.btnFollowMap.CheckState = System.Windows.Forms.CheckState.Unchecked;
            this.btnFollowMap.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnFollowMap.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnFollowMap.Name = "btnFollowMap";
            this.btnFollowMap.Size = new System.Drawing.Size(73, 22);
            this.btnFollowMap.Text = "Follow Map";
            this.btnFollowMap.Click += new System.EventHandler(this.DisplayDropDown_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // btnNames
            // 
            this.btnNames.Checked = true;
            this.btnNames.CheckState = System.Windows.Forms.CheckState.Checked;
            this.btnNames.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnNames.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNames.Name = "btnNames";
            this.btnNames.Size = new System.Drawing.Size(48, 22);
            this.btnNames.Text = "Names";
            this.btnNames.Click += new System.EventHandler(this.DisplayDropDown_Click);
            // 
            // tss4
            // 
            this.tss4.Name = "tss4";
            this.tss4.Size = new System.Drawing.Size(6, 25);
            // 
            // btnPercent
            // 
            this.btnPercent.Checked = true;
            this.btnPercent.CheckState = System.Windows.Forms.CheckState.Checked;
            this.btnPercent.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnPercent.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnPercent.Name = "btnPercent";
            this.btnPercent.Size = new System.Drawing.Size(23, 22);
            this.btnPercent.Text = "%";
            this.btnPercent.Click += new System.EventHandler(this.DisplayDropDown_Click);
            // 
            // btnMVA
            // 
            this.btnMVA.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnMVA.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnMVA.Name = "btnMVA";
            this.btnMVA.Size = new System.Drawing.Size(37, 22);
            this.btnMVA.Text = "MVA";
            this.btnMVA.Click += new System.EventHandler(this.DisplayDropDown_Click);
            // 
            // btnMW
            // 
            this.btnMW.Checked = true;
            this.btnMW.CheckState = System.Windows.Forms.CheckState.Checked;
            this.btnMW.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnMW.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnMW.Name = "btnMW";
            this.btnMW.Size = new System.Drawing.Size(33, 22);
            this.btnMW.Text = "MW";
            this.btnMW.Click += new System.EventHandler(this.DisplayDropDown_Click);
            // 
            // btnMVAR
            // 
            this.btnMVAR.Checked = true;
            this.btnMVAR.CheckState = System.Windows.Forms.CheckState.Checked;
            this.btnMVAR.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnMVAR.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnMVAR.Name = "btnMVAR";
            this.btnMVAR.Size = new System.Drawing.Size(44, 22);
            this.btnMVAR.Text = "MVAR";
            this.btnMVAR.Click += new System.EventHandler(this.DisplayDropDown_Click);
            // 
            // tss5
            // 
            this.tss5.Name = "tss5";
            this.tss5.Size = new System.Drawing.Size(6, 25);
            // 
            // btnOtherLine
            // 
            this.btnOtherLine.Checked = false;
            this.btnOtherLine.CheckState = System.Windows.Forms.CheckState.Unchecked;
            this.btnOtherLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnOtherLine.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnOtherLine.Name = "btnOtherLine";
            this.btnOtherLine.Size = new System.Drawing.Size(66, 22);
            this.btnOtherLine.Text = "Other Line";
            this.btnOtherLine.Click += new System.EventHandler(this.DisplayDropDown_Click);
            // 
            // tss6
            // 
            this.tss6.Name = "tss6";
            this.tss6.Size = new System.Drawing.Size(6, 25);
            // 
            // btnVoltage
            // 
            this.btnVoltage.Checked = true;
            this.btnVoltage.CheckState = System.Windows.Forms.CheckState.Checked;
            this.btnVoltage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnVoltage.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnVoltage.Name = "btnVoltage";
            this.btnVoltage.Size = new System.Drawing.Size(51, 22);
            this.btnVoltage.Text = "Voltage";
            this.btnVoltage.Click += new System.EventHandler(this.DisplayDropDown_Click);
            // 
            // btnAngle
            // 
            this.btnAngle.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnAngle.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAngle.Name = "btnAngle";
            this.btnAngle.Size = new System.Drawing.Size(42, 22);
            this.btnAngle.Text = "Angle";
            // 
            // btnFrequency
            // 
            this.btnFrequency.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnFrequency.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnFrequency.Name = "btnFrequency";
            this.btnFrequency.Size = new System.Drawing.Size(66, 22);
            this.btnFrequency.Text = "Frequency";
            this.btnFrequency.Click += new System.EventHandler(this.DisplayDropDown_Click);
            // 
            // lblHighlightDiffs
            // 
            this.lblHighlightDiffs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.lblHighlightDiffs.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.breakerStateToolStripMenuItem,
            this.switchStateToolStripMenuItem,
            this.mWToolStripMenuItem,
            this.mVARToolStripMenuItem,
            this.mVAToolStripMenuItem,
            this.gMWToolStripMenuItem,
            this.gMVToolStripMenuItem,
            this.toolStripMenuItem2,
            this.analogThresholdToolStripMenuItem,
            this.txtAnalogThreshold,
            this.tsIgnoreMissing});
            this.lblHighlightDiffs.Image = global::MacomberMapClient.Properties.Resources.Image;
            this.lblHighlightDiffs.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.lblHighlightDiffs.Name = "lblHighlightDiffs";
            this.lblHighlightDiffs.Size = new System.Drawing.Size(139, 22);
            this.lblHighlightDiffs.Text = "SCADA-RT Differences";
            // 
            // breakerStateToolStripMenuItem
            // 
            this.breakerStateToolStripMenuItem.Name = "breakerStateToolStripMenuItem";
            this.breakerStateToolStripMenuItem.Size = new System.Drawing.Size(228, 22);
            this.breakerStateToolStripMenuItem.Text = "Breaker state";
            this.breakerStateToolStripMenuItem.Click += new System.EventHandler(this.DisplayDropDown_Click);
            // 
            // switchStateToolStripMenuItem
            // 
            this.switchStateToolStripMenuItem.Name = "switchStateToolStripMenuItem";
            this.switchStateToolStripMenuItem.Size = new System.Drawing.Size(228, 22);
            this.switchStateToolStripMenuItem.Text = "Switch State";
            this.switchStateToolStripMenuItem.Click += new System.EventHandler(this.DisplayDropDown_Click);
            // 
            // mWToolStripMenuItem
            // 
            this.mWToolStripMenuItem.Name = "mWToolStripMenuItem";
            this.mWToolStripMenuItem.Size = new System.Drawing.Size(228, 22);
            this.mWToolStripMenuItem.Text = "MW";
            this.mWToolStripMenuItem.Click += new System.EventHandler(this.DisplayDropDown_Click);
            // 
            // mVARToolStripMenuItem
            // 
            this.mVARToolStripMenuItem.Name = "mVARToolStripMenuItem";
            this.mVARToolStripMenuItem.Size = new System.Drawing.Size(228, 22);
            this.mVARToolStripMenuItem.Text = "MVAR";
            this.mVARToolStripMenuItem.Click += new System.EventHandler(this.DisplayDropDown_Click);
            // 
            // mVAToolStripMenuItem
            // 
            this.mVAToolStripMenuItem.Name = "mVAToolStripMenuItem";
            this.mVAToolStripMenuItem.Size = new System.Drawing.Size(228, 22);
            this.mVAToolStripMenuItem.Text = "MVA";
            this.mVAToolStripMenuItem.Click += new System.EventHandler(this.DisplayDropDown_Click);
            // 
            // gMWToolStripMenuItem
            // 
            this.gMWToolStripMenuItem.Name = "gMWToolStripMenuItem";
            this.gMWToolStripMenuItem.Size = new System.Drawing.Size(228, 22);
            this.gMWToolStripMenuItem.Text = "GMW";
            this.gMWToolStripMenuItem.Click += new System.EventHandler(this.DisplayDropDown_Click);
            // 
            // gMVToolStripMenuItem
            // 
            this.gMVToolStripMenuItem.Name = "gMVToolStripMenuItem";
            this.gMVToolStripMenuItem.Size = new System.Drawing.Size(228, 22);
            this.gMVToolStripMenuItem.Text = "GMV";
            this.gMVToolStripMenuItem.Click += new System.EventHandler(this.DisplayDropDown_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(225, 6);
            // 
            // analogThresholdToolStripMenuItem
            // 
            this.analogThresholdToolStripMenuItem.Name = "analogThresholdToolStripMenuItem";
            this.analogThresholdToolStripMenuItem.Size = new System.Drawing.Size(228, 22);
            this.analogThresholdToolStripMenuItem.Text = "Analog threshold (%):";
            // 
            // txtAnalogThreshold
            // 
            this.txtAnalogThreshold.Name = "txtAnalogThreshold";
            this.txtAnalogThreshold.Size = new System.Drawing.Size(168, 23);
            this.txtAnalogThreshold.Text = "10";
            this.txtAnalogThreshold.TextChanged += new System.EventHandler(this.txtAnalogThreshold_TextChanged);
            // 
            // tsIgnoreMissing
            // 
            this.tsIgnoreMissing.Checked = true;
            this.tsIgnoreMissing.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tsIgnoreMissing.Name = "tsIgnoreMissing";
            this.tsIgnoreMissing.Size = new System.Drawing.Size(228, 22);
            this.tsIgnoreMissing.Text = "Ignore missing values";
            this.tsIgnoreMissing.Click += new System.EventHandler(this.DisplayDropDown_Click);
            // 
            // btnSwapDetails
            // 
            this.btnSwapDetails.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnSwapDetails.Image = global::MacomberMapClient.Properties.Resources.Image;
            this.btnSwapDetails.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSwapDetails.Name = "btnSwapDetails";
            this.btnSwapDetails.Size = new System.Drawing.Size(117, 22);
            this.btnSwapDetails.Text = "Swap detail location";
            this.btnSwapDetails.Click += new System.EventHandler(this.btnSwapDetails_Click);
            // 
            // btnDisplay
            // 
            this.btnDisplay.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnDisplay.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.breakersToolStripMenuItem,
            this.capacitorsToolStripMenuItem,
            this.endcapsToolStripMenuItem,
            this.mnuLines,
            this.loadsToolStripMenuItem,
            this.mnuNodes,
            this.mnuNodeToElementLines,
            this.pricingVectorsToolStripMenuItem,
            this.reactorsToolStripMenuItem,
            this.switchesToolStripMenuItem,
            this.transformersToolStripMenuItem,
            this.unitsToolStripMenuItem,
            this.toolStripMenuItem1,
            this.shapesToolStripMenuItem,
            this.arrowsToolStripMenuItem});
            this.btnDisplay.Image = global::MacomberMapClient.Properties.Resources.Image;
            this.btnDisplay.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnDisplay.Name = "btnDisplay";
            this.btnDisplay.Size = new System.Drawing.Size(58, 22);
            this.btnDisplay.Text = "Display";
            // 
            // breakersToolStripMenuItem
            // 
            this.breakersToolStripMenuItem.Checked = true;
            this.breakersToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.breakersToolStripMenuItem.Name = "breakersToolStripMenuItem";
            this.breakersToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.breakersToolStripMenuItem.Text = "Breakers";
            this.breakersToolStripMenuItem.Click += new System.EventHandler(this.DisplayDropDown_Click);
            // 
            // capacitorsToolStripMenuItem
            // 
            this.capacitorsToolStripMenuItem.Checked = true;
            this.capacitorsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.capacitorsToolStripMenuItem.Name = "capacitorsToolStripMenuItem";
            this.capacitorsToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.capacitorsToolStripMenuItem.Text = "Capacitors";
            this.capacitorsToolStripMenuItem.Click += new System.EventHandler(this.DisplayDropDown_Click);
            // 
            // endcapsToolStripMenuItem
            // 
            this.endcapsToolStripMenuItem.Checked = true;
            this.endcapsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.endcapsToolStripMenuItem.Name = "endcapsToolStripMenuItem";
            this.endcapsToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.endcapsToolStripMenuItem.Text = "End Caps";
            this.endcapsToolStripMenuItem.Click += new System.EventHandler(this.DisplayDropDown_Click);
            // 
            // mnuLines
            // 
            this.mnuLines.Checked = true;
            this.mnuLines.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mnuLines.Name = "mnuLines";
            this.mnuLines.Size = new System.Drawing.Size(193, 22);
            this.mnuLines.Text = "Lines";
            this.mnuLines.Click += new System.EventHandler(this.DisplayDropDown_Click);
            // 
            // loadsToolStripMenuItem
            // 
            this.loadsToolStripMenuItem.Checked = true;
            this.loadsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.loadsToolStripMenuItem.Name = "loadsToolStripMenuItem";
            this.loadsToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.loadsToolStripMenuItem.Text = "Loads";
            this.loadsToolStripMenuItem.Click += new System.EventHandler(this.DisplayDropDown_Click);
            // 
            // mnuNodes
            // 
            this.mnuNodes.Checked = true;
            this.mnuNodes.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mnuNodes.Name = "mnuNodes";
            this.mnuNodes.Size = new System.Drawing.Size(193, 22);
            this.mnuNodes.Text = "Nodes";
            this.mnuNodes.Click += new System.EventHandler(this.DisplayDropDown_Click);
            // 
            // mnuNodeToElementLines
            // 
            this.mnuNodeToElementLines.Checked = true;
            this.mnuNodeToElementLines.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mnuNodeToElementLines.Name = "mnuNodeToElementLines";
            this.mnuNodeToElementLines.Size = new System.Drawing.Size(193, 22);
            this.mnuNodeToElementLines.Text = "Node to Element Lines";
            this.mnuNodeToElementLines.Click += new System.EventHandler(this.DisplayDropDown_Click);
            // 
            // pricingVectorsToolStripMenuItem
            // 
            this.pricingVectorsToolStripMenuItem.Checked = true;
            this.pricingVectorsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.pricingVectorsToolStripMenuItem.Name = "pricingVectorsToolStripMenuItem";
            this.pricingVectorsToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.pricingVectorsToolStripMenuItem.Text = "Pricing Vectors";
            this.pricingVectorsToolStripMenuItem.Click += new System.EventHandler(this.DisplayDropDown_Click);
            // 
            // reactorsToolStripMenuItem
            // 
            this.reactorsToolStripMenuItem.Checked = true;
            this.reactorsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.reactorsToolStripMenuItem.Name = "reactorsToolStripMenuItem";
            this.reactorsToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.reactorsToolStripMenuItem.Text = "Reactors";
            this.reactorsToolStripMenuItem.Click += new System.EventHandler(this.DisplayDropDown_Click);
            // 
            // switchesToolStripMenuItem
            // 
            this.switchesToolStripMenuItem.Checked = true;
            this.switchesToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.switchesToolStripMenuItem.Name = "switchesToolStripMenuItem";
            this.switchesToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.switchesToolStripMenuItem.Text = "Switches";
            this.switchesToolStripMenuItem.Click += new System.EventHandler(this.DisplayDropDown_Click);
            // 
            // transformersToolStripMenuItem
            // 
            this.transformersToolStripMenuItem.Checked = true;
            this.transformersToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.transformersToolStripMenuItem.Name = "transformersToolStripMenuItem";
            this.transformersToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.transformersToolStripMenuItem.Text = "Transformers";
            this.transformersToolStripMenuItem.Click += new System.EventHandler(this.DisplayDropDown_Click);
            // 
            // unitsToolStripMenuItem
            // 
            this.unitsToolStripMenuItem.Checked = true;
            this.unitsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.unitsToolStripMenuItem.Name = "unitsToolStripMenuItem";
            this.unitsToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.unitsToolStripMenuItem.Text = "Units";
            this.unitsToolStripMenuItem.Click += new System.EventHandler(this.DisplayDropDown_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(190, 6);
            // 
            // shapesToolStripMenuItem
            // 
            this.shapesToolStripMenuItem.Checked = true;
            this.shapesToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.shapesToolStripMenuItem.Name = "shapesToolStripMenuItem";
            this.shapesToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.shapesToolStripMenuItem.Text = "Shapes";
            this.shapesToolStripMenuItem.Click += new System.EventHandler(this.DisplayDropDown_Click);
            // 
            // arrowsToolStripMenuItem
            // 
            this.arrowsToolStripMenuItem.Checked = true;
            this.arrowsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.arrowsToolStripMenuItem.Name = "arrowsToolStripMenuItem";
            this.arrowsToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.arrowsToolStripMenuItem.Text = "Arrows";
            this.arrowsToolStripMenuItem.Click += new System.EventHandler(this.DisplayDropDown_Click);
            // 
            // pnlElements
            // 
            this.pnlElements.AutoScroll = true;
            this.pnlElements.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlElements.Font = new System.Drawing.Font("Times New Roman", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pnlElements.Location = new System.Drawing.Point(0, 25);
            this.pnlElements.Name = "pnlElements";
            this.pnlElements.Size = new System.Drawing.Size(1474, 228);
            this.pnlElements.TabIndex = 0;
            this.pnlElements.Paint += new System.Windows.Forms.PaintEventHandler(this.PanelPaint);
            this.pnlElements.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.pnlElements_MouseDoubleClick);
            this.pnlElements.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnlElements_MouseDown);
            this.pnlElements.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pnlElements_MouseMove);
            this.pnlElements.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pnlElements_MouseUp);
            // 
            // tmrUpdate
            // 
            this.tmrUpdate.Interval = 1000;
            this.tmrUpdate.Tick += new System.EventHandler(this.tmrUpdate_Tick);
            // 
            // MM_OneLine_Viewer
            // 
            this.Controls.Add(this.pnlElements);
            this.Controls.Add(this.tsMain);
            this.DoubleBuffered = true;
            this.Name = "MM_OneLine_Viewer";
            this.Size = new System.Drawing.Size(1474, 253);
            this.tsMain.ResumeLayout(false);
            this.tsMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
                      #endregion

        internal DoubleBufferedPanel pnlElements;
        internal ToolStripDropDownButton btnDataSource;
        internal ToolStripButton btnNames;
        internal ToolStripButton btnMW;
        internal ToolStripButton btnMVAR;
        internal ToolStripButton btnMVA;
        internal ToolStripButton btnPercent;
        internal ToolStripButton btnOtherLine;
        internal ToolStripButton btnVoltage;
        private ToolStripButton btnSwapDetails;
        internal ToolStripMenuItem loadsToolStripMenuItem;
        internal ToolStripMenuItem pricingVectorsToolStripMenuItem;
        internal ToolStripMenuItem transformersToolStripMenuItem;
        internal ToolStripMenuItem unitsToolStripMenuItem;
        internal ToolStripMenuItem breakersToolStripMenuItem;
        internal ToolStripMenuItem capacitorsToolStripMenuItem;
        internal ToolStripMenuItem mnuLines;
        internal ToolStripMenuItem mnuNodes;
        internal ToolStripMenuItem mnuNodeToElementLines;
        internal ToolStripMenuItem reactorsToolStripMenuItem;
        internal ToolStripMenuItem switchesToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripDropDownButton btnDisplay;
        internal ToolStripMenuItem shapesToolStripMenuItem;
        internal ToolStripMenuItem endcapsToolStripMenuItem;
        internal ToolStripMenuItem arrowsToolStripMenuItem;
        private ToolStripDropDownButton lblHighlightDiffs;
        private ToolStripMenuItem breakerStateToolStripMenuItem;
        private ToolStripMenuItem switchStateToolStripMenuItem;
        private ToolStripMenuItem mWToolStripMenuItem;
        private ToolStripMenuItem mVARToolStripMenuItem;
        private ToolStripMenuItem mVAToolStripMenuItem;
        private ToolStripMenuItem gMWToolStripMenuItem;
        private ToolStripMenuItem tsIgnoreMissing;
        private ToolStripSeparator toolStripMenuItem2;
        private ToolStripMenuItem analogThresholdToolStripMenuItem;
        private ToolStripTextBox txtAnalogThreshold;
        private ToolStripMenuItem gMVToolStripMenuItem;
        internal ToolStripButton btnFollowMap;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripLabel lblZoom;
        internal ToolStripButton btnFrequency;
        private Timer tmrUpdate;
        private ToolStripButton btnFitToPage;
        private ToolStripButton btnZoomUp;
        private ToolStripButton btnZoomDown;
        private ToolStripTextBox txtZoomLevel;
        internal ToolStripButton btnAngle;
    }
}
