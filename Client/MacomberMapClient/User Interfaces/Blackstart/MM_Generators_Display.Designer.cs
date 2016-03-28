using MacomberMapClient.User_Interfaces.Generic;
namespace MacomberMapClient.User_Interfaces.Blackstart
{
    partial class MM_Generators_Display
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MM_Generators_Display));
            this.splTitle = new System.Windows.Forms.SplitContainer();
            this.grpGenSummary = new System.Windows.Forms.GroupBox();
            this.tcMain = new System.Windows.Forms.TabControl();
            this.tpUnits = new System.Windows.Forms.TabPage();
            this.btnRefresh = new System.Windows.Forms.ToolStripButton();
            this.olView = new MacomberMapClient.User_Interfaces.OneLines.MM_OneLine_Viewer();
            this.tmrUpdate = new System.Windows.Forms.Timer();
            ((System.ComponentModel.ISupportInitialize)(this.splTitle)).BeginInit();
            this.splTitle.Panel1.SuspendLayout();
            this.splTitle.SuspendLayout();
            this.grpGenSummary.SuspendLayout();
            this.tcMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUnits)).BeginInit();
            this.SuspendLayout();
            // 
            // splTitle
            // 
            this.splTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splTitle.Location = new System.Drawing.Point(0, 0);
            this.splTitle.Name = "splTitle";
            this.splTitle.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splTitle.Panel2Collapsed = true;
            // 
            // splTitle.Panel1
            // 
            this.splTitle.Panel1.Controls.Add(this.grpGenSummary);
            // 
            // grpGenSummary
            // 
            this.grpGenSummary.Controls.Add(this.tcMain);
            this.grpGenSummary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpGenSummary.Location = new System.Drawing.Point(0, 25);
            this.grpGenSummary.Name = "grpGenSummary";
            this.grpGenSummary.Size = new System.Drawing.Size(1065, 899);
            this.grpGenSummary.TabIndex = 1;
            this.grpGenSummary.TabStop = false;
            this.grpGenSummary.Text = "Generators Summary";
            // 
            // tcMain
            // 
            this.tcMain.Controls.Add(this.tpUnits);
            this.tcMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcMain.Location = new System.Drawing.Point(3, 16);
            this.tcMain.Name = "tcMain";
            this.tcMain.SelectedIndex = 0;
            this.tcMain.Size = new System.Drawing.Size(1065, 880);
            this.tcMain.TabIndex = 1;
            // 
            // tpUnits
            // 
            this.tpUnits.Controls.Add(this.dgvUnits);
            this.tpUnits.Location = new System.Drawing.Point(4, 22);
            this.tpUnits.Name = "tpUnits";
            this.tpUnits.Size = new System.Drawing.Size(1065, 854);
            this.tpUnits.TabIndex = 3;
            this.tpUnits.Text = "Units";
            this.tpUnits.UseVisualStyleBackColor = true;
            // 
            // dgvUnits
            // 
            this.dgvUnits.AllowUserToAddRows = false;
            this.dgvUnits.AllowUserToDeleteRows = false;
            this.dgvUnits.AllowUserToOrderColumns = true;
            this.dgvUnits.BackgroundColor = System.Drawing.Color.Black;
            this.dgvUnits.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle.BackColor = System.Drawing.Color.Black;
            dataGridViewCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvUnits.DefaultCellStyle = dataGridViewCellStyle;
            this.dgvUnits.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvUnits.Location = new System.Drawing.Point(0, 0);
            this.dgvUnits.Name = "dgvUnits";
            this.dgvUnits.ReadOnly = true;
            this.dgvUnits.Size = new System.Drawing.Size(1068, 924);
            this.dgvUnits.TabIndex = 1;
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
            // tmrUpdate
            // 
            this.tmrUpdate.Enabled = true;
            this.tmrUpdate.Interval = 1000;
            this.tmrUpdate.Tick += new System.EventHandler(this.tmrUpdate_Tick);
            // 
            // MM_Generators_Display
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1068, 924);
            this.Controls.Add(this.splTitle);
            this.Icon = Properties.Resources.CompanyIcon;
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "MM_Generators_Display";
            this.Text = "Generators Summary";
            this.splTitle.Panel1.ResumeLayout(false);
            this.splTitle.Panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splTitle)).EndInit();
            this.splTitle.ResumeLayout(false);
            this.grpGenSummary.ResumeLayout(false);
            this.tcMain.ResumeLayout(false);
            this.tpUnits.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvUnits)).EndInit();
            this.ResumeLayout(false);
        }
        #endregion

        private System.Windows.Forms.SplitContainer splTitle;
        private System.Windows.Forms.GroupBox grpGenSummary;
        private OneLines.MM_OneLine_Viewer olView;
        private System.Windows.Forms.ToolStripButton btnRefresh;
        private System.Windows.Forms.TabControl tcMain;
        private System.Windows.Forms.TabPage tpUnits;
        private System.Windows.Forms.Timer tmrUpdate;
    }
}