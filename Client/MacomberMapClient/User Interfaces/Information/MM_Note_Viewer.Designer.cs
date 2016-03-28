namespace MacomberMapClient.User_Interfaces.Information
{
    partial class MM_Note_Viewer
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MM_Note_Viewer));
            this.lvNotes = new System.Windows.Forms.ListView();
            this.colBlank = new System.Windows.Forms.ColumnHeader();
            this.colDate = new System.Windows.Forms.ColumnHeader();
            this.colElement = new System.Windows.Forms.ColumnHeader();
            this.colElemType = new System.Windows.Forms.ColumnHeader();
            this.colAuthor = new System.Windows.Forms.ColumnHeader();
            this.colNote = new System.Windows.Forms.ColumnHeader();
            this.tmrUpdate = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // lvNotes
            // 
            this.lvNotes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colBlank,
            this.colDate,
            this.colElement,
            this.colElemType,
            this.colAuthor,
            this.colNote});
            this.lvNotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvNotes.FullRowSelect = true;
            this.lvNotes.Location = new System.Drawing.Point(0, 0);
            this.lvNotes.Name = "lvNotes";
            this.lvNotes.Size = new System.Drawing.Size(386, 266);
            this.lvNotes.TabIndex = 0;
            this.lvNotes.UseCompatibleStateImageBehavior = false;
            this.lvNotes.View = System.Windows.Forms.View.Details;
            this.lvNotes.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lvNotes_MouseDoubleClick);
            this.lvNotes.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lvNotes_MouseClick);
            this.lvNotes.SelectedIndexChanged += new System.EventHandler(this.lvNotes_SelectedIndexChanged);
            // 
            // colBlank
            // 
            this.colBlank.Text = " ";
            this.colBlank.Width = 22;
            // 
            // colDate
            // 
            this.colDate.Text = "Date";
            // 
            // colElement
            // 
            this.colElement.Text = "Element";
            // 
            // colElemType
            // 
            this.colElemType.Text = "Type";
            // 
            // colAuthor
            // 
            this.colAuthor.Text = "Author";
            // 
            // colNote
            // 
            this.colNote.Text = "Note";
            // 
            // tmrUpdate
            // 
            this.tmrUpdate.Enabled = true;
            this.tmrUpdate.Interval = 20000;
            this.tmrUpdate.Tick += new System.EventHandler(this.tmrUpdate_Tick);
            // 
            // Note_Viewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(386, 266);
            this.Controls.Add(this.lvNotes);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = Properties.Resources.CompanyIcon;
            this.Name = "Note_Viewer";
            this.Text = "Notes";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Note_Viewer_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lvNotes;
        private System.Windows.Forms.ColumnHeader colDate;
        private System.Windows.Forms.ColumnHeader colElement;
        private System.Windows.Forms.ColumnHeader colElemType;
        private System.Windows.Forms.ColumnHeader colAuthor;
        private System.Windows.Forms.ColumnHeader colNote;
        private System.Windows.Forms.ColumnHeader colBlank;
        private System.Windows.Forms.Timer tmrUpdate;
    }
}