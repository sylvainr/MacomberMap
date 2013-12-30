namespace Macomber_Map.User_Interface_Components
{
    partial class UserInteraction_Viewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserInteraction_Viewer));
            this.lvHistory = new System.Windows.Forms.ListView();
            this.colTime = new System.Windows.Forms.ColumnHeader();
            this.colStation = new System.Windows.Forms.ColumnHeader();
            this.colElemType = new System.Windows.Forms.ColumnHeader();
            this.colName = new System.Windows.Forms.ColumnHeader();
            this.colAction = new System.Windows.Forms.ColumnHeader();
            this.colOld = new System.Windows.Forms.ColumnHeader();
            this.colNew = new System.Windows.Forms.ColumnHeader();
            this.SuspendLayout();
            // 
            // lvHistory
            // 
            this.lvHistory.BackColor = System.Drawing.Color.Black;
            this.lvHistory.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colTime,
            this.colStation,
            this.colElemType,
            this.colName,
            this.colAction,
            this.colOld,
            this.colNew});
            this.lvHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvHistory.ForeColor = System.Drawing.Color.White;
            this.lvHistory.Location = new System.Drawing.Point(0, 0);
            this.lvHistory.Name = "lvHistory";
            this.lvHistory.Size = new System.Drawing.Size(338, 262);
            this.lvHistory.TabIndex = 1;
            this.lvHistory.TileSize = new System.Drawing.Size(168, 60);
            this.lvHistory.UseCompatibleStateImageBehavior = false;
            this.lvHistory.View = System.Windows.Forms.View.Details;
            this.lvHistory.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lvHistory_MouseClick);
            this.lvHistory.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(lvHistory_MouseDoubleClick);
            // 
            // colTime
            // 
            this.colTime.Text = "Time";
            // 
            // colStation
            // 
            this.colStation.Text = "Station";
            // 
            // colElemType
            // 
            this.colElemType.Text = "Type";
            // 
            // colName
            // 
            this.colName.Text = "Name";
            // 
            // colAction
            // 
            this.colAction.Text = "Action";
            // 
            // colOld
            // 
            this.colOld.Text = "Old";
            // 
            // colNew
            // 
            this.colNew.Text = "New";
            // 
            // UserInteraction_Viewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(338, 262);
            this.Controls.Add(this.lvHistory);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "UserInteraction_Viewer";
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.ListView lvHistory;
        private System.Windows.Forms.ColumnHeader colTime;
        private System.Windows.Forms.ColumnHeader colStation;
        private System.Windows.Forms.ColumnHeader colElemType;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colAction;
        private System.Windows.Forms.ColumnHeader colOld;
        private System.Windows.Forms.ColumnHeader colNew;
    }
}