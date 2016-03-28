using MacomberMapClient.Properties;
namespace MacomberMapClient.User_Interfaces.Summary
{
    partial class MM_Search_Results
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
            this.splSearch = new System.Windows.Forms.SplitContainer();
            this.picAbort = new System.Windows.Forms.PictureBox();
            this.picSearch = new System.Windows.Forms.PictureBox();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSearch = new System.Windows.Forms.Label();
            this.lvSearch = new System.Windows.Forms.ListView();
            ((System.ComponentModel.ISupportInitialize)(this.splSearch)).BeginInit();
            this.splSearch.Panel1.SuspendLayout();
            this.splSearch.Panel2.SuspendLayout();
            this.splSearch.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picAbort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picSearch)).BeginInit();
            this.SuspendLayout();
            // 
            // splSearch
            // 
            this.splSearch.BackColor = System.Drawing.Color.Black;
            this.splSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splSearch.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splSearch.Location = new System.Drawing.Point(0, 0);
            this.splSearch.Name = "splSearch";
            this.splSearch.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splSearch.Panel1
            // 
            this.splSearch.Panel1.Controls.Add(this.picAbort);
            this.splSearch.Panel1.Controls.Add(this.picSearch);
            this.splSearch.Panel1.Controls.Add(this.txtSearch);
            this.splSearch.Panel1.Controls.Add(this.lblSearch);
            // 
            // splSearch.Panel2
            // 
            this.splSearch.Panel2.Controls.Add(this.lvSearch);
            this.splSearch.Size = new System.Drawing.Size(664, 183);
            this.splSearch.SplitterDistance = 34;
            this.splSearch.TabIndex = 0;
            // 
            // picAbort
            // 
            this.picAbort.BackColor = System.Drawing.Color.Transparent;
            this.picAbort.Image = global::MacomberMapClient.Properties.Resources.Acknowledge;
            this.picAbort.Location = new System.Drawing.Point(316, 6);
            this.picAbort.Name = "picAbort";
            this.picAbort.Size = new System.Drawing.Size(22, 22);
            this.picAbort.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picAbort.TabIndex = 40;
            this.picAbort.TabStop = false;
            this.picAbort.Visible = false;
            this.picAbort.Click += new System.EventHandler(this.picAbort_Click);
            // 
            // picSearch
            // 
            this.picSearch.BackColor = System.Drawing.Color.Transparent;
            this.picSearch.Image = global::MacomberMapClient.Properties.Resources.Search;
            this.picSearch.Location = new System.Drawing.Point(288, 6);
            this.picSearch.Name = "picSearch";
            this.picSearch.Size = new System.Drawing.Size(22, 22);
            this.picSearch.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picSearch.TabIndex = 39;
            this.picSearch.TabStop = false;
            this.picSearch.Click += new System.EventHandler(this.PerformSearch);
            // 
            // txtSearch
            // 
            this.txtSearch.AutoCompleteCustomSource.AddRange(new string[] {
            "Substation",
            "Line",
            "Company",
            "County"});
            this.txtSearch.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.txtSearch.BackColor = System.Drawing.Color.Black;
            this.txtSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSearch.ForeColor = System.Drawing.Color.LightGray;
            this.txtSearch.Location = new System.Drawing.Point(75, 6);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(207, 22);
            this.txtSearch.TabIndex = 38;
            this.txtSearch.TextChanged += new System.EventHandler(this.UpdateText);
            this.txtSearch.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtSearch_KeyPress);
            // 
            // lblSearch
            // 
            this.lblSearch.AutoSize = true;
            this.lblSearch.Font = new System.Drawing.Font("Arial Black", 10F);
            this.lblSearch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.lblSearch.Location = new System.Drawing.Point(3, 9);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(66, 19);
            this.lblSearch.TabIndex = 37;
            this.lblSearch.Text = "Search:";
            // 
            // lvSearch
            // 
            this.lvSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvSearch.FullRowSelect = true;
            this.lvSearch.Location = new System.Drawing.Point(0, 0);
            this.lvSearch.Name = "lvSearch";
            this.lvSearch.Size = new System.Drawing.Size(664, 145);
            this.lvSearch.TabIndex = 0;
            this.lvSearch.UseCompatibleStateImageBehavior = false;
            this.lvSearch.View = System.Windows.Forms.View.Details;
            this.lvSearch.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lvSearch_MouseClick);
            // 
            // MM_Search_Results
            // 
            this.Controls.Add(this.splSearch);
            this.Name = "MM_Search_Results";
            this.Size = new System.Drawing.Size(664, 183);
            this.splSearch.Panel1.ResumeLayout(false);
            this.splSearch.Panel1.PerformLayout();
            this.splSearch.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splSearch)).EndInit();
            this.splSearch.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picAbort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picSearch)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

    }
}
