using System.Windows.Forms;
namespace MacomberMapClient.User_Interfaces.Summary
{
    partial class MM_Limit_Reserve_Viewer
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

        private ListView lv;
        private ColumnHeader colName;
        private ColumnHeader colValue;
        private Timer tmrUpdate;
        private TabControl tcMain;
        private TabPage tpCurrent;
        private TabPage tpHistoric;
        private bool HasResized = false;

        /// <summary>
        /// Initialize a new limit reserve viewer
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("CSC Limits", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("IROL Limits", System.Windows.Forms.HorizontalAlignment.Left);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MM_Limit_Reserve_Viewer));
            this.lv = new System.Windows.Forms.ListView();
            this.colName = new System.Windows.Forms.ColumnHeader();
            this.colValue = new System.Windows.Forms.ColumnHeader();
            this.tmrUpdate = new System.Windows.Forms.Timer(this.components);
            this.tcMain = new System.Windows.Forms.TabControl();
            this.tpCurrent = new System.Windows.Forms.TabPage();
            this.tpHistoric = new System.Windows.Forms.TabPage();
            this.tcMain.SuspendLayout();
            this.tpCurrent.SuspendLayout();
            this.SuspendLayout();
            // 
            // lv
            // 
            this.lv.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName,
            this.colValue});
            this.lv.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewGroup1.Header = "CSC Limits";
            listViewGroup1.Name = "CSC Limits";
            listViewGroup2.Header = "IROL Limits";
            listViewGroup2.Name = "IROL Limits";
            this.lv.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2});
            this.lv.Location = new System.Drawing.Point(3, 3);
            this.lv.Name = "lv";
            this.lv.Size = new System.Drawing.Size(805, 394);
            this.lv.TabIndex = 0;
            this.lv.UseCompatibleStateImageBehavior = false;
            this.lv.View = System.Windows.Forms.View.Details;
            // 
            // colName
            // 
            this.colName.Text = "Name";
            this.colName.Width = 250;
            // 
            // colValue
            // 
            this.colValue.Text = "Value";
            this.colValue.Width = 216;
            // 
            // tmrUpdate
            // 
            this.tmrUpdate.Enabled = true;
            this.tmrUpdate.Interval = 1000;
            this.tmrUpdate.Tick += new System.EventHandler(this.tmrUpdate_Tick);
            // 
            // tcMain
            // 
            this.tcMain.Controls.Add(this.tpCurrent);
            this.tcMain.Controls.Add(this.tpHistoric);
            this.tcMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcMain.Location = new System.Drawing.Point(0, 0);
            this.tcMain.Name = "tcMain";
            this.tcMain.SelectedIndex = 0;
            this.tcMain.Size = new System.Drawing.Size(819, 426);
            this.tcMain.TabIndex = 1;
            // 
            // tpCurrent
            // 
            this.tpCurrent.Controls.Add(this.lv);
            this.tpCurrent.Location = new System.Drawing.Point(4, 22);
            this.tpCurrent.Name = "tpCurrent";
            this.tpCurrent.Padding = new System.Windows.Forms.Padding(3);
            this.tpCurrent.Size = new System.Drawing.Size(811, 400);
            this.tpCurrent.TabIndex = 0;
            this.tpCurrent.Text = "Current";
            this.tpCurrent.UseVisualStyleBackColor = true;
            // 
            // tpHistoric
            // 
            this.tpHistoric.Location = new System.Drawing.Point(4, 22);
            this.tpHistoric.Name = "tpHistoric";
            this.tpHistoric.Padding = new System.Windows.Forms.Padding(3);
            this.tpHistoric.Size = new System.Drawing.Size(239, 296);
            this.tpHistoric.TabIndex = 1;
            this.tpHistoric.Text = "Historic";
            this.tpHistoric.UseVisualStyleBackColor = true;
            // 
            // Limit_Reserve_Viewer
            // 
            this.ClientSize = new System.Drawing.Size(819, 426);
            this.Controls.Add(this.tcMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = Properties.Resources.CompanyIcon;
            this.Name = "Limit_Reserve_Viewer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultBounds;
            this.Text = "Limits and Reserves";
            this.tcMain.ResumeLayout(false);
            this.tpCurrent.ResumeLayout(false);
            this.ResumeLayout(false);

        }
    }
}