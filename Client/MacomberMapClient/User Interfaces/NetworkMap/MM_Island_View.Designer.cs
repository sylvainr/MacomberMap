namespace MacomberMapClient.User_Interfaces.NetworkMap
{
    partial class MM_Island_View
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MM_Island_View));
            ((System.ComponentModel.ISupportInitialize)(this.dgvIslandView)).BeginInit();
            this.tmrUpdate = new System.Windows.Forms.Timer();
            this.SuspendLayout();
            // 
            // dgvIslandView
            // 
            this.dgvIslandView.AllowUserToAddRows = false;
            this.dgvIslandView.AllowUserToDeleteRows = false;
            this.dgvIslandView.AllowUserToOrderColumns = true;
            this.dgvIslandView.BackgroundColor = System.Drawing.Color.Black;
            this.dgvIslandView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.Black;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvIslandView.DefaultCellStyle = dataGridViewCellStyle1;
            this.dgvIslandView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvIslandView.Location = new System.Drawing.Point(0, 0);
            this.dgvIslandView.Name = "dgvIslandView";
            this.dgvIslandView.ReadOnly = true;
            this.dgvIslandView.RowHeadersVisible = false;
            this.dgvIslandView.AutoSize = true;
            this.dgvIslandView.TabIndex = 0;
            // 
            // tmrUpdate
            // 
            this.tmrUpdate.Enabled = true;
            this.tmrUpdate.Interval = 1000;
            this.tmrUpdate.Tick += new System.EventHandler(this.tmrUpdate_Tick);
            // 
            // MM_Island_View
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.dgvIslandView);
            this.Icon = Properties.Resources.CompanyIcon;
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "MM_Island_View";
            this.Text = "MM_Island_View";
            ((System.ComponentModel.ISupportInitialize)(this.dgvIslandView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        
        private System.Windows.Forms.Timer tmrUpdate;
    }
}