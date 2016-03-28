namespace MacomberMapClient.User_Interfaces.Blackstart
{
    partial class MM_EMS_Command_Display
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MM_EMS_Command_Display));
            this.tsMain = new System.Windows.Forms.ToolStrip();
            this.btnExportExcel = new System.Windows.Forms.ToolStripButton();
            this.btnSynchFile = new System.Windows.Forms.ToolStripButton();
            this.btnShowOneLine = new System.Windows.Forms.ToolStripButton();
            this.splMain = new System.Windows.Forms.SplitContainer();
            this.olView = new MacomberMapClient.User_Interfaces.OneLines.MM_OneLine_Viewer();
            this.tsMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splMain)).BeginInit();
            this.splMain.Panel1.SuspendLayout();
            this.splMain.Panel2.SuspendLayout();
            this.splMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCommands)).BeginInit();
            this.SuspendLayout();
            // 
            // tsMain
            // 
            this.tsMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnExportExcel, this.btnSynchFile, this.btnShowOneLine});
            this.tsMain.Location = new System.Drawing.Point(0, 0);
            this.tsMain.Name = "tsMain";
            this.tsMain.Size = new System.Drawing.Size(666, 25);
            this.tsMain.TabIndex = 1;
            this.tsMain.Text = "toolStrip1";
            // 
            // btnExportExcel
            // 
            this.btnExportExcel.ForeColor = System.Drawing.Color.Black;
            this.btnExportExcel.Image = global::MacomberMapClient.Properties.Resources.Excel;
            this.btnExportExcel.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnExportExcel.Name = "btnExportExcel";
            this.btnExportExcel.Size = new System.Drawing.Size(103, 22);
            this.btnExportExcel.Text = "Export to Excel";
            this.btnExportExcel.Click += new System.EventHandler(this.btnExportExcel_Click);
            // 
            // btnSynchFile
            // 
            this.btnSynchFile.ForeColor = System.Drawing.Color.Black;
            this.btnSynchFile.Image = global::MacomberMapClient.Properties.Resources.NoteHS;
            this.btnSynchFile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSynchFile.Name = "btnSynchFile";
            this.btnSynchFile.Size = new System.Drawing.Size(103, 22);
            this.btnSynchFile.Text = "Synch with EMS log";
            this.btnSynchFile.Click += new System.EventHandler(this.btnSynch_Click);
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
            // splMain
            // 
            this.splMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splMain.Location = new System.Drawing.Point(0, 25);
            this.splMain.Name = "splMain";
            this.splMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.splMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splMain.Panel2Collapsed = true;
            // 
            // splMain.Panel1
            // 
            this.splMain.Panel1.Controls.Add(this.dgvCommands);
            // 
            // splMain.Panel2
            // 
            this.splMain.Panel2.Controls.Add(this.olView);
            this.splMain.Size = new System.Drawing.Size(666, 236);
            this.splMain.SplitterDistance = 353;
            this.splMain.TabIndex = 2;
            // 
            // olView
            // 
            this.olView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olView.Location = new System.Drawing.Point(0, 0);
            this.olView.Name = "olView";
            this.olView.Size = new System.Drawing.Size(309, 236);
            this.olView.TabIndex = 0;
            this.olView.ForeColor = System.Drawing.Color.Black;
            // 
            // dgvCommands
            // 
            this.dgvCommands.AllowUserToAddRows = false;
            this.dgvCommands.AllowUserToDeleteRows = false;
            this.dgvCommands.AllowUserToOrderColumns = true;
            this.dgvCommands.BackgroundColor = System.Drawing.Color.Black;
            this.dgvCommands.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.Black;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvCommands.DefaultCellStyle = dataGridViewCellStyle1;
            this.dgvCommands.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvCommands.Location = new System.Drawing.Point(0, 0);
            this.dgvCommands.Name = "dgvCommands";
            this.dgvCommands.ReadOnly = true;
            this.dgvCommands.RowHeadersVisible = false;
            this.dgvCommands.Size = new System.Drawing.Size(353, 236);
            this.dgvCommands.TabIndex = 0;
            this.dgvCommands.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvCommands_CellMouseClick);
            this.dgvCommands.SelectionChanged += new System.EventHandler(this.dgvCommands_SelectionChanged);
            // 
            // MM_EMS_Command_Display
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(666, 261);
            this.Controls.Add(this.splMain);
            this.Controls.Add(this.tsMain);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = Properties.Resources.CompanyIcon;
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "MM_EMS_Command_Display";
            this.Text = "Macomber Map Command History";
            this.tsMain.ResumeLayout(false);
            this.tsMain.PerformLayout();
            this.splMain.Panel1.ResumeLayout(false);
            this.splMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splMain)).EndInit();
            this.splMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvCommands)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip tsMain;
        private System.Windows.Forms.ToolStripButton btnExportExcel;
        private System.Windows.Forms.ToolStripButton btnSynchFile;
        private System.Windows.Forms.ToolStripButton btnShowOneLine;
        private System.Windows.Forms.SplitContainer splMain;
        private OneLines.MM_OneLine_Viewer olView;
    }
}