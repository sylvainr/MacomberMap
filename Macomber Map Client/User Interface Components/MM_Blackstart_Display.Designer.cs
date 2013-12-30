namespace Macomber_Map.User_Interface_Components
{
    partial class MM_Blackstart_Display
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MM_Blackstart_Display));
            this.splMain = new System.Windows.Forms.SplitContainer();
            this.lvItems = new System.Windows.Forms.ListView();
            this.colNumber = new System.Windows.Forms.ColumnHeader();
            this.colCommand = new System.Windows.Forms.ColumnHeader();
            this.colSubstation = new System.Windows.Forms.ColumnHeader();
            this.colElement = new System.Windows.Forms.ColumnHeader();
            this.ilElements = new System.Windows.Forms.ImageList(this.components);
            this.tsEditMenu = new System.Windows.Forms.ToolStrip();
            this.lblCorridor = new System.Windows.Forms.ToolStripLabel();
            this.cmbCorridor = new System.Windows.Forms.ToolStripComboBox();
            this.lblCorridorTarget = new System.Windows.Forms.ToolStripLabel();
            this.cmbCorridorTarget = new System.Windows.Forms.ToolStripComboBox();
            this.lblPrimarySecondary = new System.Windows.Forms.ToolStripLabel();
            this.cmbPrimarySecondary = new System.Windows.Forms.ToolStripComboBox();
            this.tmrUpdate = new System.Windows.Forms.Timer(this.components);
            this.olView = new Macomber_Map.User_Interface_Components.OneLines.OneLine_Viewer(true);
            this.splTitle = new System.Windows.Forms.SplitContainer();
            this.grpSummary = new System.Windows.Forms.GroupBox();
            this.tvSummary = new System.Windows.Forms.TreeView();
            this.splMain.Panel1.SuspendLayout();
            this.splMain.Panel2.SuspendLayout();
            this.splMain.SuspendLayout();
            this.tsEditMenu.SuspendLayout();
            this.splTitle.Panel1.SuspendLayout();
            this.splTitle.Panel2.SuspendLayout();
            this.splTitle.SuspendLayout();
            this.grpSummary.SuspendLayout();
            this.SuspendLayout();
            // 
            // splMain
            // 
            this.splMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splMain.Location = new System.Drawing.Point(0, 25);
            this.splMain.Name = "splMain";
            // 
            // splMain.Panel1
            // 
            this.splMain.Panel1.Controls.Add(this.lvItems);
            // 
            // splMain.Panel2
            // 
            this.splMain.Panel2.Controls.Add(this.olView);
            this.splMain.Size = new System.Drawing.Size(563, 372);
            this.splMain.SplitterDistance = 186;
            this.splMain.TabIndex = 0;
            // 
            // lvItems
            // 
            this.lvItems.CheckBoxes = true;
            this.lvItems.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colNumber,
            this.colCommand,
            this.colSubstation,
            this.colElement});
            this.lvItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvItems.FullRowSelect = true;
            this.lvItems.HideSelection = false;
            this.lvItems.Location = new System.Drawing.Point(0, 0);
            this.lvItems.Name = "lvItems";
            this.lvItems.Size = new System.Drawing.Size(186, 372);
            this.lvItems.SmallImageList = this.ilElements;
            this.lvItems.TabIndex = 3;
            this.lvItems.UseCompatibleStateImageBehavior = false;
            this.lvItems.View = System.Windows.Forms.View.Details;
            this.lvItems.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lvItems_MouseClick);
            this.lvItems.SelectedIndexChanged += new System.EventHandler(this.lvItems_SelectedIndexChanged);
            // 
            // colNumber
            // 
            this.colNumber.Text = "#";
            this.colNumber.Width = 30;
            // 
            // colCommand
            // 
            this.colCommand.Text = "Command";
            // 
            // colSubstation
            // 
            this.colSubstation.Text = "Substation";
            // 
            // colElement
            // 
            this.colElement.Text = "Element";
            // 
            // ilElements
            // 
            this.ilElements.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilElements.ImageStream")));
            this.ilElements.TransparentColor = System.Drawing.Color.Magenta;
            this.ilElements.Images.SetKeyName(0, "Unknown");
            this.ilElements.Images.SetKeyName(1, "Good");
            this.ilElements.Images.SetKeyName(2, "Bad");
            // 
            // olView
            // 
            this.olView.BackColor = System.Drawing.Color.Black;
            this.olView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olView.Location = new System.Drawing.Point(0, 0);
            this.olView.MaximumSize = new System.Drawing.Size(1280, 984);
            this.olView.Name = "olView";
            this.olView.Size = new System.Drawing.Size(373, 372);
            this.olView.TabIndex = 0;
            // 
            // tsEditMenu
            // 
            this.tsEditMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblCorridor,
            this.cmbCorridor,
            this.lblCorridorTarget,
            this.cmbCorridorTarget,
            this.lblPrimarySecondary,
            this.cmbPrimarySecondary});
            this.tsEditMenu.Location = new System.Drawing.Point(0, 0);
            this.tsEditMenu.Name = "tsEditMenu";
            this.tsEditMenu.Size = new System.Drawing.Size(563, 25);
            this.tsEditMenu.TabIndex = 1;
            this.tsEditMenu.Text = "toolStrip1";
            // 
            // lblCorridor
            // 
            this.lblCorridor.Name = "lblCorridor";
            this.lblCorridor.Size = new System.Drawing.Size(54, 22);
            this.lblCorridor.Text = "Corridor:";
            // 
            // cmbCorridor
            // 
            this.cmbCorridor.Name = "cmbCorridor";
            this.cmbCorridor.Size = new System.Drawing.Size(121, 25);
            this.cmbCorridor.SelectedIndexChanged += new System.EventHandler(this.cmbCorridor_SelectedIndexChanged);
            // 
            // lblCorridorTarget
            // 
            this.lblCorridorTarget.Name = "lblCorridorTarget";
            this.lblCorridorTarget.Size = new System.Drawing.Size(44, 22);
            this.lblCorridorTarget.Text = "Target:";
            // 
            // cmbCorridorTarget
            // 
            this.cmbCorridorTarget.Name = "cmbCorridorTarget";
            this.cmbCorridorTarget.Size = new System.Drawing.Size(121, 25);
            this.cmbCorridorTarget.SelectedIndexChanged += new System.EventHandler(this.cmbCorridorTarget_SelectedIndexChanged);
            // 
            // lblPrimarySecondary
            // 
            this.lblPrimarySecondary.Name = "lblPrimarySecondary";
            this.lblPrimarySecondary.Size = new System.Drawing.Size(47, 22);
            this.lblPrimarySecondary.Text = "Pri/Sec:";
            // 
            // cmbPrimarySecondary
            // 
            this.cmbPrimarySecondary.AutoSize = false;
            this.cmbPrimarySecondary.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPrimarySecondary.Items.AddRange(new object[] {
            "Primary",
            "Secondary"});
            this.cmbPrimarySecondary.Name = "cmbPrimarySecondary";
            this.cmbPrimarySecondary.Size = new System.Drawing.Size(80, 23);
            this.cmbPrimarySecondary.SelectedIndexChanged += new System.EventHandler(this.cmbPrimarySecondary_SelectedIndexChanged);
            // 
            // tmrUpdate
            // 
            this.tmrUpdate.Enabled = true;
            this.tmrUpdate.Interval = 4000;
            this.tmrUpdate.Tick += new System.EventHandler(this.tmrUpdate_Tick);
            // 
            // splTitle
            // 
            this.splTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splTitle.Location = new System.Drawing.Point(0, 0);
            this.splTitle.Name = "splTitle";
            // 
            // splTitle.Panel1
            // 
            this.splTitle.Panel1.Controls.Add(this.grpSummary);
            // 
            // splTitle.Panel2
            // 
            this.splTitle.Panel2.Controls.Add(this.splMain);
            this.splTitle.Panel2.Controls.Add(this.tsEditMenu);
            this.splTitle.Size = new System.Drawing.Size(743, 397);
            this.splTitle.SplitterDistance = 176;
            this.splTitle.TabIndex = 2;
            // 
            // grpSummary
            // 
            this.grpSummary.Controls.Add(this.tvSummary);
            this.grpSummary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpSummary.Location = new System.Drawing.Point(0, 0);
            this.grpSummary.Name = "grpSummary";
            this.grpSummary.Size = new System.Drawing.Size(176, 397);
            this.grpSummary.TabIndex = 1;
            this.grpSummary.TabStop = false;
            this.grpSummary.Text = "Corridor Summary";
            // 
            // tvSummary
            // 
            this.tvSummary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvSummary.ImageIndex = 0;
            this.tvSummary.ImageList = this.ilElements;
            this.tvSummary.Location = new System.Drawing.Point(3, 16);
            this.tvSummary.Name = "tvSummary";
            this.tvSummary.SelectedImageIndex = 0;
            this.tvSummary.Size = new System.Drawing.Size(170, 378);
            this.tvSummary.TabIndex = 0;
            this.tvSummary.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvSummary_AfterSelect);
            // 
            // MM_Blackstart_Display
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(743, 397);
            this.Controls.Add(this.splTitle);
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "MM_Blackstart_Display";
            this.Text = "Blackstart Corridor Information";
            this.splMain.Panel1.ResumeLayout(false);
            this.splMain.Panel2.ResumeLayout(false);
            this.splMain.ResumeLayout(false);
            this.tsEditMenu.ResumeLayout(false);
            this.tsEditMenu.PerformLayout();
            this.splTitle.Panel1.ResumeLayout(false);
            this.splTitle.Panel2.ResumeLayout(false);
            this.splTitle.Panel2.PerformLayout();
            this.splTitle.ResumeLayout(false);
            this.grpSummary.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splMain;
        private System.Windows.Forms.ListView lvItems;
        private System.Windows.Forms.ColumnHeader colNumber;
        private System.Windows.Forms.ColumnHeader colCommand;
        private System.Windows.Forms.ColumnHeader colSubstation;
        private System.Windows.Forms.ColumnHeader colElement;
        private System.Windows.Forms.ToolStrip tsEditMenu;
        private System.Windows.Forms.ToolStripComboBox cmbCorridor;
        private System.Windows.Forms.ToolStripComboBox cmbCorridorTarget;
        private System.Windows.Forms.ToolStripComboBox cmbPrimarySecondary;
        private System.Windows.Forms.ToolStripLabel lblCorridor;
        private System.Windows.Forms.ToolStripLabel lblCorridorTarget;
        private System.Windows.Forms.ToolStripLabel lblPrimarySecondary;
        private Macomber_Map.User_Interface_Components.OneLines.OneLine_Viewer olView;
        private System.Windows.Forms.ImageList ilElements;
        private System.Windows.Forms.Timer tmrUpdate;
        private System.Windows.Forms.SplitContainer splTitle;
        private System.Windows.Forms.TreeView tvSummary;
        private System.Windows.Forms.GroupBox grpSummary;
    }
}