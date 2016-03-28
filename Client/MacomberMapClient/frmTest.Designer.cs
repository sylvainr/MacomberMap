namespace MacomberMapClient
{
    partial class frmTest
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.lblTestServer = new System.Windows.Forms.ToolStripLabel();
            this.txtServer = new System.Windows.Forms.ToolStripTextBox();
            this.btnOpen = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnQueryApplicationVersion = new System.Windows.Forms.ToolStripButton();
            this.btnFillData = new System.Windows.Forms.ToolStripDropDownButton();
            this.btnTestPI = new System.Windows.Forms.ToolStripButton();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 25);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(804, 237);
            this.dataGridView1.TabIndex = 1;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblTestServer,
            this.txtServer,
            this.btnOpen,
            this.toolStripSeparator1,
            this.btnQueryApplicationVersion,
            this.btnFillData,
            this.btnTestPI});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(804, 25);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // lblTestServer
            // 
            this.lblTestServer.Name = "lblTestServer";
            this.lblTestServer.Size = new System.Drawing.Size(42, 22);
            this.lblTestServer.Text = "Server:";
            // 
            // txtServer
            // 
            this.txtServer.Name = "txtServer";
            this.txtServer.Size = new System.Drawing.Size(315, 25);
            this.txtServer.Text = "net.tcp://localhost:8774/MacomberMapWCFService.svc";
            // 
            // btnOpen
            // 
            this.btnOpen.Image = global::MacomberMapClient.Properties.Resources.Network_ConnectTo;
            this.btnOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(56, 22);
            this.btnOpen.Text = "Open";
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // btnQueryApplicationVersion
            // 
            this.btnQueryApplicationVersion.Image = global::MacomberMapClient.Properties.Resources.DataSource;
            this.btnQueryApplicationVersion.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnQueryApplicationVersion.Name = "btnQueryApplicationVersion";
            this.btnQueryApplicationVersion.Size = new System.Drawing.Size(162, 22);
            this.btnQueryApplicationVersion.Text = "Query application version";
            this.btnQueryApplicationVersion.Click += new System.EventHandler(this.QueryApplicationVersion_Click);
            // 
            // btnFillData
            // 
            this.btnFillData.Image = global::MacomberMapClient.Properties.Resources.DataSource;
            this.btnFillData.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnFillData.Name = "btnFillData";
            this.btnFillData.Size = new System.Drawing.Size(127, 22);
            this.btnFillData.Text = "Fill grid with data";
            this.btnFillData.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.btnFillData_DropDownItemClicked);
            // 
            // btnTestPI
            // 
            this.btnTestPI.Image = global::MacomberMapClient.Properties.Resources.DataSource;
            this.btnTestPI.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnTestPI.Name = "btnTestPI";
            this.btnTestPI.Size = new System.Drawing.Size(62, 22);
            this.btnTestPI.Text = "Test PI";
            this.btnTestPI.Click += new System.EventHandler(this.btnTestPI_Click);
            // 
            // frmTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(804, 262);
            this.Controls.Add(this.dataGridView1);
            this.Icon = Properties.Resources.CompanyIcon;
            this.Controls.Add(this.toolStrip1);
            this.Name = "frmTest";
            this.Text = "MM Client <-> Server test";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnQueryApplicationVersion;
        private System.Windows.Forms.ToolStripDropDownButton btnFillData;
        private System.Windows.Forms.ToolStripButton btnTestPI;
        private System.Windows.Forms.ToolStripLabel lblTestServer;
        private System.Windows.Forms.ToolStripTextBox txtServer;
        private System.Windows.Forms.ToolStripButton btnOpen;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;

    }
}

