namespace MacomberMapClient.User_Interfaces.NetworkMap
{
    partial class MM_Operatorship_Display
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
            this.splTitle = new System.Windows.Forms.SplitContainer();
            this.grpOwnedEquipment = new System.Windows.Forms.GroupBox();
            this.lvEquipment = new System.Windows.Forms.ListView();
            this.colSubstation = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnRefresh = new System.Windows.Forms.ToolStripButton();
            this.olView = new MacomberMapClient.User_Interfaces.OneLines.MM_OneLine_Viewer();
            ((System.ComponentModel.ISupportInitialize)(this.splTitle)).BeginInit();
            this.splTitle.Panel1.SuspendLayout();
            this.splTitle.Panel2.SuspendLayout();
            this.splTitle.SuspendLayout();
            this.grpOwnedEquipment.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splTitle
            // 
            this.splTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splTitle.Location = new System.Drawing.Point(0, 0);
            this.splTitle.Name = "splTitle";
            // 
            // splTitle.Panel1
            // 
            this.splTitle.Panel1.Controls.Add(this.grpOwnedEquipment);
            this.splTitle.Panel1.Controls.Add(this.toolStrip1);
            // 
            // splTitle.Panel2
            // 
            this.splTitle.Panel2.Controls.Add(this.olView);
            this.splTitle.Size = new System.Drawing.Size(743, 397);
            this.splTitle.SplitterDistance = 176;
            this.splTitle.TabIndex = 2;
            // 
            // grpOwnedEquipment
            // 
            this.grpOwnedEquipment.Controls.Add(this.lvEquipment);
            this.grpOwnedEquipment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpOwnedEquipment.Location = new System.Drawing.Point(0, 25);
            this.grpOwnedEquipment.Name = "grpOwnedEquipment";
            this.grpOwnedEquipment.Size = new System.Drawing.Size(176, 372);
            this.grpOwnedEquipment.TabIndex = 1;
            this.grpOwnedEquipment.TabStop = false;
            this.grpOwnedEquipment.Text = "Owned Equipment";
            // 
            // lvEquipment
            // 
            this.lvEquipment.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colSubstation});
            this.lvEquipment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvEquipment.Location = new System.Drawing.Point(3, 16);
            this.lvEquipment.Name = "lvEquipment";
            this.lvEquipment.Size = new System.Drawing.Size(170, 353);
            this.lvEquipment.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvEquipment.TabIndex = 0;
            this.lvEquipment.UseCompatibleStateImageBehavior = false;
            this.lvEquipment.View = System.Windows.Forms.View.Details;
            this.lvEquipment.SelectedIndexChanged += new System.EventHandler(this.lvEquipment_SelectedIndexChanged);
            // 
            // colSubstation
            // 
            this.colSubstation.Text = "Substation";
            this.colSubstation.Width = 64;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnRefresh});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(176, 25);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnRefresh
            // 
            this.btnRefresh.Image = global::MacomberMapClient.Properties.Resources.RefreshDocViewHS;
            this.btnRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(66, 22);
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // olView
            // 
            this.olView.BackColor = System.Drawing.Color.Black;
            this.olView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olView.Location = new System.Drawing.Point(0, 0);
            this.olView.MaximumSize = new System.Drawing.Size(1280, 984);
            this.olView.Name = "olView";
            this.olView.Size = new System.Drawing.Size(563, 397);
            this.olView.TabIndex = 1;
            // 
            // MM_Operatorship_Display
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(743, 397);
            this.Controls.Add(this.splTitle);
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "MM_Operatorship_Display";
            this.Text = "Operated Substations";
            this.splTitle.Panel1.ResumeLayout(false);
            this.splTitle.Panel1.PerformLayout();
            this.splTitle.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splTitle)).EndInit();
            this.splTitle.ResumeLayout(false);
            this.grpOwnedEquipment.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);

        }


        #endregion

        private System.Windows.Forms.SplitContainer splTitle;
        private System.Windows.Forms.GroupBox grpOwnedEquipment;
        private OneLines.MM_OneLine_Viewer olView;
        private System.Windows.Forms.ListView lvEquipment;
        private System.Windows.Forms.ColumnHeader colSubstation;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnRefresh;
    }
}