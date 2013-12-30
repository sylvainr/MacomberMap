namespace Macomber_Map.User_Interface_Components
{
    partial class Historical_Viewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Historical_Viewer));
            this.tmrUpdate = new System.Windows.Forms.Timer(this.components);
            this.tcMain = new System.Windows.Forms.TabControl();
            this.tpRealTime = new System.Windows.Forms.TabPage();
            this.zgRealTime = new ZedGraph.ZedGraphControl();
            this.tpHistoric = new System.Windows.Forms.TabPage();
            this.splHistoric = new System.Windows.Forms.SplitContainer();
            this.splHistoricLookup = new System.Windows.Forms.SplitContainer();
            this.tvTags = new System.Windows.Forms.TreeView();
            this.zgHistoricDetails = new ZedGraph.ZedGraphControl();
            this.zgHistoricOverview = new ZedGraph.ZedGraphControl();
            this.tsMain = new System.Windows.Forms.ToolStrip();
            this.lblHistRange = new System.Windows.Forms.ToolStripLabel();
            this.cmbHistoricRange = new System.Windows.Forms.ToolStripComboBox();
            this.btnRefresh = new System.Windows.Forms.ToolStripButton();
            this.btnLog = new System.Windows.Forms.ToolStripButton();
            this.btnDetailedView = new System.Windows.Forms.ToolStripButton();
            this.tcMain.SuspendLayout();
            this.tpRealTime.SuspendLayout();
            this.tpHistoric.SuspendLayout();
            this.splHistoric.Panel1.SuspendLayout();
            this.splHistoric.Panel2.SuspendLayout();
            this.splHistoric.SuspendLayout();
            this.splHistoricLookup.Panel1.SuspendLayout();
            this.splHistoricLookup.Panel2.SuspendLayout();
            this.splHistoricLookup.SuspendLayout();
            this.tsMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // tmrUpdate
            // 
            this.tmrUpdate.Enabled = true;
            this.tmrUpdate.Interval = 1000;
            this.tmrUpdate.Tick += new System.EventHandler(this.tmrUpdate_Tick);
            // 
            // tcMain
            // 
            this.tcMain.Controls.Add(this.tpRealTime);
            this.tcMain.Controls.Add(this.tpHistoric);
            this.tcMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcMain.Location = new System.Drawing.Point(0, 0);
            this.tcMain.Name = "tcMain";
            this.tcMain.SelectedIndex = 0;
            this.tcMain.Size = new System.Drawing.Size(835, 460);
            this.tcMain.TabIndex = 9;
            // 
            // tpRealTime
            // 
            this.tpRealTime.BackColor = System.Drawing.Color.Black;
            this.tpRealTime.Controls.Add(this.zgRealTime);
            this.tpRealTime.Location = new System.Drawing.Point(4, 22);
            this.tpRealTime.Name = "tpRealTime";
            this.tpRealTime.Padding = new System.Windows.Forms.Padding(3);
            this.tpRealTime.Size = new System.Drawing.Size(827, 434);
            this.tpRealTime.TabIndex = 0;
            this.tpRealTime.Text = "Real-Time";
            // 
            // zgRealTime
            // 
            this.zgRealTime.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zgRealTime.EditButtons = System.Windows.Forms.MouseButtons.None;
            this.zgRealTime.EditModifierKeys = System.Windows.Forms.Keys.None;
            this.zgRealTime.IsAntiAlias = true;
            this.zgRealTime.IsShowPointValues = true;
            this.zgRealTime.Location = new System.Drawing.Point(3, 3);
            this.zgRealTime.Name = "zgRealTime";
            this.zgRealTime.ScrollGrace = 0;
            this.zgRealTime.ScrollMaxX = 0;
            this.zgRealTime.ScrollMaxY = 0;
            this.zgRealTime.ScrollMaxY2 = 0;
            this.zgRealTime.ScrollMinX = 0;
            this.zgRealTime.ScrollMinY = 0;
            this.zgRealTime.ScrollMinY2 = 0;
            this.zgRealTime.Size = new System.Drawing.Size(821, 428);
            this.zgRealTime.TabIndex = 0;
            this.zgRealTime.PointValueEvent += new ZedGraph.ZedGraphControl.PointValueHandler(this.zg_PointValueEvent);
            // 
            // tpHistoric
            // 
            this.tpHistoric.BackColor = System.Drawing.Color.Black;
            this.tpHistoric.Controls.Add(this.splHistoric);
            this.tpHistoric.Location = new System.Drawing.Point(4, 22);
            this.tpHistoric.Name = "tpHistoric";
            this.tpHistoric.Padding = new System.Windows.Forms.Padding(3);
            this.tpHistoric.Size = new System.Drawing.Size(827, 434);
            this.tpHistoric.TabIndex = 1;
            this.tpHistoric.Text = "Historic";
            // 
            // splHistoric
            // 
            this.splHistoric.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splHistoric.Location = new System.Drawing.Point(3, 3);
            this.splHistoric.Name = "splHistoric";
            this.splHistoric.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splHistoric.Panel1
            // 
            this.splHistoric.Panel1.Controls.Add(this.splHistoricLookup);
            // 
            // splHistoric.Panel2
            // 
            this.splHistoric.Panel2.Controls.Add(this.zgHistoricOverview);
            this.splHistoric.Panel2.Controls.Add(this.tsMain);
            this.splHistoric.Size = new System.Drawing.Size(821, 428);
            this.splHistoric.SplitterDistance = 245;
            this.splHistoric.TabIndex = 0;
            // 
            // splHistoricLookup
            // 
            this.splHistoricLookup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splHistoricLookup.Location = new System.Drawing.Point(0, 0);
            this.splHistoricLookup.Name = "splHistoricLookup";
            // 
            // splHistoricLookup.Panel1
            // 
            this.splHistoricLookup.Panel1.Controls.Add(this.tvTags);
            // 
            // splHistoricLookup.Panel2
            // 
            this.splHistoricLookup.Panel2.Controls.Add(this.zgHistoricDetails);
            this.splHistoricLookup.Size = new System.Drawing.Size(821, 245);
            this.splHistoricLookup.SplitterDistance = 272;
            this.splHistoricLookup.TabIndex = 0;
            // 
            // tvTags
            // 
            this.tvTags.BackColor = System.Drawing.Color.Black;
            this.tvTags.CheckBoxes = true;
            this.tvTags.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvTags.ForeColor = System.Drawing.Color.White;
            this.tvTags.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.tvTags.Location = new System.Drawing.Point(0, 0);
            this.tvTags.Name = "tvTags";
            this.tvTags.ShowNodeToolTips = true;
            this.tvTags.Size = new System.Drawing.Size(272, 245);
            this.tvTags.TabIndex = 9;
            this.tvTags.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.tvTags_AfterCheck);
            // 
            // zgHistoricDetails
            // 
            this.zgHistoricDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zgHistoricDetails.IsAntiAlias = true;
            this.zgHistoricDetails.IsShowPointValues = true;
            this.zgHistoricDetails.Location = new System.Drawing.Point(0, 0);
            this.zgHistoricDetails.Name = "zgHistoricDetails";
            this.zgHistoricDetails.ScrollGrace = 0;
            this.zgHistoricDetails.ScrollMaxX = 0;
            this.zgHistoricDetails.ScrollMaxY = 0;
            this.zgHistoricDetails.ScrollMaxY2 = 0;
            this.zgHistoricDetails.ScrollMinX = 0;
            this.zgHistoricDetails.ScrollMinY = 0;
            this.zgHistoricDetails.ScrollMinY2 = 0;
            this.zgHistoricDetails.Size = new System.Drawing.Size(545, 245);
            this.zgHistoricDetails.TabIndex = 0;
            this.zgHistoricDetails.PointValueEvent += new ZedGraph.ZedGraphControl.PointValueHandler(this.zg_PointValueEvent);
            // 
            // zgHistoricOverview
            // 
            this.zgHistoricOverview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zgHistoricOverview.IsShowPointValues = true;
            this.zgHistoricOverview.Location = new System.Drawing.Point(0, 25);
            this.zgHistoricOverview.Name = "zgHistoricOverview";
            this.zgHistoricOverview.ScrollGrace = 0;
            this.zgHistoricOverview.ScrollMaxX = 0;
            this.zgHistoricOverview.ScrollMaxY = 0;
            this.zgHistoricOverview.ScrollMaxY2 = 0;
            this.zgHistoricOverview.ScrollMinX = 0;
            this.zgHistoricOverview.ScrollMinY = 0;
            this.zgHistoricOverview.ScrollMinY2 = 0;
            this.zgHistoricOverview.Size = new System.Drawing.Size(821, 154);
            this.zgHistoricOverview.TabIndex = 1;
            this.zgHistoricOverview.PointValueEvent += new ZedGraph.ZedGraphControl.PointValueHandler(this.zg_PointValueEvent);
            this.zgHistoricOverview.MouseMoveEvent += new ZedGraph.ZedGraphControl.ZedMouseEventHandler(this.zgHistoricOverview_MouseMoveEvent);
            // 
            // tsMain
            // 
            this.tsMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblHistRange,
            this.cmbHistoricRange,
            this.btnRefresh,
            this.btnLog,
            this.btnDetailedView});
            this.tsMain.Location = new System.Drawing.Point(0, 0);
            this.tsMain.Name = "tsMain";
            this.tsMain.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.tsMain.Size = new System.Drawing.Size(821, 25);
            this.tsMain.TabIndex = 0;
            this.tsMain.Text = "toolStrip1";
            // 
            // lblHistRange
            // 
            this.lblHistRange.BackColor = System.Drawing.Color.Black;
            this.lblHistRange.ForeColor = System.Drawing.Color.White;
            this.lblHistRange.Name = "lblHistRange";
            this.lblHistRange.Size = new System.Drawing.Size(43, 22);
            this.lblHistRange.Text = "Range:";
            // 
            // cmbHistoricRange
            // 
            this.cmbHistoricRange.BackColor = System.Drawing.Color.Black;
            this.cmbHistoricRange.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbHistoricRange.ForeColor = System.Drawing.Color.White;
            this.cmbHistoricRange.Name = "cmbHistoricRange";
            this.cmbHistoricRange.Size = new System.Drawing.Size(121, 25);
            this.cmbHistoricRange.SelectedIndexChanged += new System.EventHandler(this.cmbHistoricRange_SelectedIndexChanged);
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackColor = System.Drawing.Color.Black;
            this.btnRefresh.ForeColor = System.Drawing.Color.White;
            this.btnRefresh.Image = global::Macomber_Map.Properties.Resources.RefreshDocViewHS;
            this.btnRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(66, 22);
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnLog
            // 
            this.btnLog.BackColor = System.Drawing.Color.Black;
            this.btnLog.ForeColor = System.Drawing.Color.White;
            this.btnLog.Image = ((System.Drawing.Image)(resources.GetObject("btnLog.Image")));
            this.btnLog.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnLog.Name = "btnLog";
            this.btnLog.Size = new System.Drawing.Size(76, 22);
            this.btnLog.Text = "Log scale";
            this.btnLog.Click += new System.EventHandler(this.btnLog_Click);
            // 
            // btnDetailedView
            // 
            this.btnDetailedView.ForeColor = System.Drawing.Color.White;
            this.btnDetailedView.Image = global::Macomber_Map.Properties.Resources.PropertiesHS;
            this.btnDetailedView.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnDetailedView.Name = "btnDetailedView";
            this.btnDetailedView.Size = new System.Drawing.Size(98, 22);
            this.btnDetailedView.Text = "Detailed View";
            this.btnDetailedView.Click += new System.EventHandler(this.btnDetailedView_Click);
            // 
            // Historical_Viewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(this.tcMain);
            this.Name = "Historical_Viewer";
            this.Size = new System.Drawing.Size(835, 460);
            this.tcMain.ResumeLayout(false);
            this.tpRealTime.ResumeLayout(false);
            this.tpHistoric.ResumeLayout(false);
            this.splHistoric.Panel1.ResumeLayout(false);
            this.splHistoric.Panel2.ResumeLayout(false);
            this.splHistoric.Panel2.PerformLayout();
            this.splHistoric.ResumeLayout(false);
            this.splHistoricLookup.Panel1.ResumeLayout(false);
            this.splHistoricLookup.Panel2.ResumeLayout(false);
            this.splHistoricLookup.ResumeLayout(false);
            this.tsMain.ResumeLayout(false);
            this.tsMain.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer tmrUpdate;
        private System.Windows.Forms.TabControl tcMain;
        private System.Windows.Forms.TabPage tpRealTime;
        private System.Windows.Forms.TabPage tpHistoric;
        private System.Windows.Forms.SplitContainer splHistoric;
        private System.Windows.Forms.SplitContainer splHistoricLookup;
        private System.Windows.Forms.TreeView tvTags;
        private ZedGraph.ZedGraphControl zgHistoricOverview;
        private System.Windows.Forms.ToolStrip tsMain;
        private System.Windows.Forms.ToolStripLabel lblHistRange;
        private System.Windows.Forms.ToolStripComboBox cmbHistoricRange;
        private System.Windows.Forms.ToolStripButton btnRefresh;
        private System.Windows.Forms.ToolStripButton btnLog;
        private ZedGraph.ZedGraphControl zgHistoricDetails;
        private ZedGraph.ZedGraphControl zgRealTime;
        private System.Windows.Forms.ToolStripButton btnDetailedView;

    }
}
