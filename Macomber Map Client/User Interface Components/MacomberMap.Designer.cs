using Macomber_Map.Data_Connections;
namespace Macomber_Map
{
    partial class MacomberMap
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MacomberMap));
            this.splVertical = new System.Windows.Forms.SplitContainer();
            this.ctlKeyIndicators = new Macomber_Map.User_Interface_Components.Key_Indicators();
            this.splHorizontal = new System.Windows.Forms.SplitContainer();
            this.ctlViolationViewer = new Macomber_Map.User_Interface_Components.Violation_Viewer();
            this.ctlMiniMap = new Macomber_Map.User_Interface_Components.Mini_Map();
            this.splVertical.Panel1.SuspendLayout();
            this.splVertical.Panel2.SuspendLayout();
            this.splVertical.SuspendLayout();
            this.splHorizontal.Panel1.SuspendLayout();
            this.splHorizontal.SuspendLayout();
            this.SuspendLayout();
            // 
            // splVertical
            // 
            this.splVertical.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splVertical.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splVertical.IsSplitterFixed = true;
            this.splVertical.Location = new System.Drawing.Point(8, 8);
            this.splVertical.Name = "splVertical";
            this.splVertical.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splVertical.Panel1
            // 
            this.splVertical.Panel1.Controls.Add(this.ctlKeyIndicators);
            // 
            // splVertical.Panel2
            // 
            this.splVertical.Panel2.Controls.Add(this.splHorizontal);
            this.splVertical.Size = new System.Drawing.Size(1260, 605);
            this.splVertical.SplitterDistance = 73;
            this.splVertical.TabIndex = 0;
            // 
            // ctlKeyIndicators
            // 
            this.ctlKeyIndicators.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ctlKeyIndicators.ForeColor = System.Drawing.Color.DarkGray;
            this.ctlKeyIndicators.Location = new System.Drawing.Point(0, 0);
            this.ctlKeyIndicators.MinimumSize = new System.Drawing.Size(4, 73);
            this.ctlKeyIndicators.Name = "ctlKeyIndicators";
            this.ctlKeyIndicators.ShowMinMaxIndicators = true;
            this.ctlKeyIndicators.Size = new System.Drawing.Size(1260, 73);
            this.ctlKeyIndicators.TabIndex = 0;
            // 
            // splHorizontal
            // 
            this.splHorizontal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splHorizontal.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splHorizontal.Location = new System.Drawing.Point(0, 0);
            this.splHorizontal.Name = "splHorizontal";
            // 
            // splHorizontal.Panel1
            // 
            this.splHorizontal.Panel1.Controls.Add(this.ctlViolationViewer);
            this.splHorizontal.Panel1.Controls.Add(this.ctlMiniMap);
            this.splHorizontal.Panel1.Resize += new System.EventHandler(this.ResizeLeftSlider);
            this.splHorizontal.Size = new System.Drawing.Size(1260, 528);
            this.splHorizontal.SplitterDistance = 181;
            this.splHorizontal.TabIndex = 0;
            // 
            // ctlViolationViewer
            // 
            this.ctlViolationViewer.BackColor = System.Drawing.Color.Black;
            this.ctlViolationViewer.Location = new System.Drawing.Point(9, 29);
            this.ctlViolationViewer.Name = "ctlViolationViewer";
            this.ctlViolationViewer.Size = new System.Drawing.Size(150, 385);
            this.ctlViolationViewer.TabIndex = 2;
            // 
            // ctlMiniMap
            // 
            this.ctlMiniMap.BackColor = System.Drawing.Color.Black;
            this.ctlMiniMap.Location = new System.Drawing.Point(12, 420);
            this.ctlMiniMap.Name = "ctlMiniMap";
            this.ctlMiniMap.Size = new System.Drawing.Size(125, 121);
            this.ctlMiniMap.TabIndex = 1;
            // 
            // MacomberMap
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(1276, 621);
            this.Controls.Add(this.splVertical);
            this.ForeColor = System.Drawing.Color.White;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "MacomberMap";
            this.Padding = new System.Windows.Forms.Padding(8);
            this.Text = "Macomber Map®";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MacomberMap_FormClosing);
            this.Resize += new System.EventHandler(this.MacomberMap_Resize);
            this.splVertical.Panel1.ResumeLayout(false);
            this.splVertical.Panel2.ResumeLayout(false);
            this.splVertical.ResumeLayout(false);
            this.splHorizontal.Panel1.ResumeLayout(false);
            this.splHorizontal.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splVertical;
        private System.Windows.Forms.SplitContainer splHorizontal;

        /// <summary>Our network map</summary>
        public Macomber_Map.User_Interface_Components.NetworkMap.Network_Map ctlNetworkMap;
        /// <summary>Our mini-map</summary>
        public Macomber_Map.User_Interface_Components.Mini_Map ctlMiniMap;
        /// <summary>Our violation viewer</summary>
        public Macomber_Map.User_Interface_Components.Violation_Viewer ctlViolationViewer;
        /// <summary>Our key indicators</summary>
        public Macomber_Map.User_Interface_Components.Key_Indicators ctlKeyIndicators;
    }
}

