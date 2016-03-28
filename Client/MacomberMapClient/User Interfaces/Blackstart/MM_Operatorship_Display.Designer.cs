using MacomberMapClient.User_Interfaces.Generic;
namespace MacomberMapClient.User_Interfaces.Blackstart
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MM_Operatorship_Display));
            this.splTitle = new System.Windows.Forms.SplitContainer();
            this.grpOwnedEquipment = new System.Windows.Forms.GroupBox();
            this.tcMain = new System.Windows.Forms.TabControl();
            this.tpSubstations = new System.Windows.Forms.TabPage();
            this.tpLines = new System.Windows.Forms.TabPage();
            this.tpUnits = new System.Windows.Forms.TabPage();
            this.tsRefresh = new System.Windows.Forms.ToolStrip();
            this.btnRefresh = new System.Windows.Forms.ToolStripButton();
            this.btnShowOneLine = new System.Windows.Forms.ToolStripButton();
            this.olView = new MacomberMapClient.User_Interfaces.OneLines.MM_OneLine_Viewer();
            this.tmrUpdate = new System.Windows.Forms.Timer();
            ((System.ComponentModel.ISupportInitialize)(this.splTitle)).BeginInit();
            this.splTitle.Panel1.SuspendLayout();
            this.splTitle.Panel2.SuspendLayout();
            this.splTitle.SuspendLayout();
            this.grpOwnedEquipment.SuspendLayout();
            this.tcMain.SuspendLayout();
            this.tpSubstations.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSubstations)).BeginInit();
            this.tpLines.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLines)).BeginInit();
            this.tpUnits.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUnits)).BeginInit();
            this.tsRefresh.SuspendLayout();
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
            this.splTitle.Panel1.Controls.Add(this.grpOwnedEquipment);
            this.splTitle.Panel1.Controls.Add(this.tsRefresh);
            // 
            // splTitle.Panel2
            // 
            this.splTitle.Panel2.Controls.Add(this.olView);
            this.splTitle.Size = new System.Drawing.Size(998, 524);
            this.splTitle.SplitterDistance = 236;
            this.splTitle.TabIndex = 2;
            // 
            // grpOwnedEquipment
            // 
            this.grpOwnedEquipment.Controls.Add(this.tcMain);
            this.grpOwnedEquipment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpOwnedEquipment.Location = new System.Drawing.Point(0, 25);
            this.grpOwnedEquipment.Name = "grpOwnedEquipment";
            this.grpOwnedEquipment.Size = new System.Drawing.Size(236, 499);
            this.grpOwnedEquipment.TabIndex = 1;
            this.grpOwnedEquipment.TabStop = false;
            this.grpOwnedEquipment.Text = "Owned Equipment";
            // 
            // tcMain
            // 
            this.tcMain.Controls.Add(this.tpSubstations);
            this.tcMain.Controls.Add(this.tpLines);
            this.tcMain.Controls.Add(this.tpUnits);
            this.tcMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcMain.Location = new System.Drawing.Point(3, 16);
            this.tcMain.Name = "tcMain";
            this.tcMain.SelectedIndex = 0;
            this.tcMain.Size = new System.Drawing.Size(230, 480);
            this.tcMain.TabIndex = 1;
            // 
            // tpSubstations
            // 
            this.tpSubstations.Controls.Add(this.dgvSubstations);
            this.tpSubstations.Location = new System.Drawing.Point(4, 22);
            this.tpSubstations.Name = "tpSubstations";
            this.tpSubstations.Padding = new System.Windows.Forms.Padding(3);
            this.tpSubstations.Size = new System.Drawing.Size(222, 454);
            this.tpSubstations.TabIndex = 0;
            this.tpSubstations.Text = "Substations";
            this.tpSubstations.UseVisualStyleBackColor = true;
            // 
            // dgvSubstations
            // 
            this.dgvSubstations.AllowUserToAddRows = false;
            this.dgvSubstations.AllowUserToDeleteRows = false;
            this.dgvSubstations.AllowUserToOrderColumns = true;
            this.dgvSubstations.BackgroundColor = System.Drawing.Color.Black;
            this.dgvSubstations.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.Black;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvSubstations.DefaultCellStyle = dataGridViewCellStyle1;
            this.dgvSubstations.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvSubstations.Location = new System.Drawing.Point(3, 3);
            this.dgvSubstations.Name = "dgvSubstations";
            this.dgvSubstations.ReadOnly = true;
            this.dgvSubstations.ShowCellErrors = false;
            this.dgvSubstations.ShowCellToolTips = false;
            this.dgvSubstations.ShowEditingIcon = false;
            this.dgvSubstations.ShowRowErrors = false;
            this.dgvSubstations.Size = new System.Drawing.Size(216, 448);
            this.dgvSubstations.TabIndex = 1;
            // 
            // tpLines
            // 
            this.tpLines.Controls.Add(this.dgvLines);
            this.tpLines.Location = new System.Drawing.Point(4, 22);
            this.tpLines.Name = "tpLines";
            this.tpLines.Padding = new System.Windows.Forms.Padding(3);
            this.tpLines.Size = new System.Drawing.Size(222, 454);
            this.tpLines.TabIndex = 1;
            this.tpLines.Text = "Lines";
            this.tpLines.UseVisualStyleBackColor = true;
            // 
            // dgvLines
            // 
            this.dgvLines.AllowUserToAddRows = false;
            this.dgvLines.AllowUserToDeleteRows = false;
            this.dgvLines.AllowUserToOrderColumns = true;
            this.dgvLines.BackgroundColor = System.Drawing.Color.Black;
            this.dgvLines.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.Black;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvLines.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvLines.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvLines.Location = new System.Drawing.Point(3, 3);
            this.dgvLines.Name = "dgvLines";
            this.dgvLines.ReadOnly = true;
            this.dgvLines.ShowCellErrors = false;
            this.dgvLines.ShowCellToolTips = false;
            this.dgvLines.ShowEditingIcon = false;
            this.dgvLines.ShowRowErrors = false;
            this.dgvLines.Size = new System.Drawing.Size(216, 448);
            this.dgvLines.TabIndex = 0;
            // 
            // tpUnits
            // 
            this.tpUnits.Controls.Add(this.dgvUnits);
            this.tpUnits.Location = new System.Drawing.Point(4, 22);
            this.tpUnits.Name = "tpUnits";
            this.tpUnits.Size = new System.Drawing.Size(222, 454);
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
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.Black;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvUnits.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvUnits.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvUnits.Location = new System.Drawing.Point(0, 0);
            this.dgvUnits.Name = "dgvUnits";
            this.dgvUnits.ReadOnly = true;
            this.dgvUnits.Size = new System.Drawing.Size(222, 454);
            this.dgvUnits.TabIndex = 1;
            // 
            // tsRefresh
            // 
            this.tsRefresh.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnRefresh, this.btnShowOneLine});
            this.tsRefresh.Location = new System.Drawing.Point(0, 0);
            this.tsRefresh.Name = "tsRefresh";
            this.tsRefresh.Size = new System.Drawing.Size(236, 25);
            this.tsRefresh.TabIndex = 2;
            this.tsRefresh.Text = "toolStrip1";
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
            // btnShowOneLine
            //
            this.btnShowOneLine.ForeColor = System.Drawing.Color.Black;
            this.btnShowOneLine.Image = global::MacomberMapClient.Properties.Resources.Collapse_large;
            this.btnShowOneLine.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnShowOneLine.Name = "btnShowOneLine";
            this.btnShowOneLine.Size = new System.Drawing.Size(103, 22);
            this.btnShowOneLine.Text = "Show One-Line";
            this.btnShowOneLine.Click += new System.EventHandler(this.btnShowOneLine_Click);
         
            // 
            // olView
            // 
            this.olView.BackColor = System.Drawing.Color.Black;
            this.olView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olView.Location = new System.Drawing.Point(0, 0);
            this.olView.Name = "olView";
            this.olView.Size = new System.Drawing.Size(758, 524);
            this.olView.TabIndex = 1;
            // 
            // tmrUpdate
            // 
            this.tmrUpdate.Enabled = true;
            this.tmrUpdate.Interval = 1000;
            this.tmrUpdate.Tick += new System.EventHandler(this.tmrUpdate_Tick);
            // 
            // MM_Operatorship_Display
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(998, 524);
            this.Controls.Add(this.splTitle);
            this.Icon = Properties.Resources.CompanyIcon;
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "MM_Operatorship_Display";
            this.Text = "Operated Substations";
            this.splTitle.Panel1.ResumeLayout(false);
            this.splTitle.Panel1.PerformLayout();
            this.splTitle.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splTitle)).EndInit();
            this.splTitle.ResumeLayout(false);
            this.grpOwnedEquipment.ResumeLayout(false);
            this.tcMain.ResumeLayout(false);
            this.tpSubstations.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSubstations)).EndInit();
            this.tpLines.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvLines)).EndInit();
            this.tpUnits.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvUnits)).EndInit();
            this.tsRefresh.ResumeLayout(false);
            this.tsRefresh.PerformLayout();
            this.ResumeLayout(false);

        }


        #endregion

        private System.Windows.Forms.SplitContainer splTitle;
        private System.Windows.Forms.GroupBox grpOwnedEquipment;
        private OneLines.MM_OneLine_Viewer olView;
        private System.Windows.Forms.ToolStrip tsRefresh;
        private System.Windows.Forms.ToolStripButton btnRefresh;
        private System.Windows.Forms.ToolStripButton btnShowOneLine;
        private System.Windows.Forms.TabControl tcMain;
        private System.Windows.Forms.TabPage tpSubstations;
        private System.Windows.Forms.TabPage tpLines;
        private System.Windows.Forms.TabPage tpUnits;
        private System.Windows.Forms.Timer tmrUpdate;
    }
}