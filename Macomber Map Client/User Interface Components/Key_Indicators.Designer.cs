
namespace Macomber_Map.User_Interface_Components
{
    partial class Key_Indicators
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
            this.components = new System.ComponentModel.Container();
            this.lblViews = new System.Windows.Forms.Label();
            this.mnuViews = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cmbNetwork = new System.Windows.Forms.ComboBox();
            this.lblNetModel = new System.Windows.Forms.Label();
            this.btnMax = new System.Windows.Forms.Button();
            this.btnMin = new System.Windows.Forms.Button();
            this.mnuMain = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tmrMain = new System.Windows.Forms.Timer(this.components);
            this.lblProcesses = new System.Windows.Forms.Label();
            this.chkUseEstimates = new System.Windows.Forms.CheckBox();
            this.tTip = new System.Windows.Forms.ToolTip(this.components);
            this.chkPostCtgs = new System.Windows.Forms.CheckBox();
            this.cmbSavecase = new System.Windows.Forms.ComboBox();
            this.chkDisplayAlarms = new System.Windows.Forms.CheckBox();
            this.btnData = new System.Windows.Forms.Button();
            this.picERCOT = new System.Windows.Forms.PictureBox();
            this.txtSearch = new Macomber_Map.User_Interface_Components.Overrides.MM_SearchBox();
            this.lblNotes = new System.Windows.Forms.Label();
            this.lblComm = new Macomber_Map.User_Interface_Components.Communication_Status();
            ((System.ComponentModel.ISupportInitialize)(this.picERCOT)).BeginInit();
            this.SuspendLayout();
            // 
            // lblViews
            // 
            this.lblViews.AutoSize = true;
            this.lblViews.BackColor = System.Drawing.Color.Transparent;
            this.lblViews.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblViews.Font = new System.Drawing.Font("Arial Black", 10F);
            this.lblViews.Location = new System.Drawing.Point(805, 13);
            this.lblViews.Name = "lblViews";
            this.lblViews.Size = new System.Drawing.Size(58, 21);
            this.lblViews.TabIndex = 9;
            this.lblViews.Text = "Views";
            this.lblViews.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lblViews_MouseClick);
            // 
            // mnuViews
            // 
            this.mnuViews.Name = "mnuViews";
            this.mnuViews.Size = new System.Drawing.Size(61, 4);
            this.mnuViews.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.mnuViews_ItemClicked);
            // 
            // cmbNetwork
            // 
            this.cmbNetwork.BackColor = System.Drawing.Color.Black;
            this.cmbNetwork.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbNetwork.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbNetwork.ForeColor = System.Drawing.Color.LightGray;
            this.cmbNetwork.FormattingEnabled = true;
            this.cmbNetwork.Location = new System.Drawing.Point(190, 11);
            this.cmbNetwork.Name = "cmbNetwork";
            this.cmbNetwork.Size = new System.Drawing.Size(127, 24);
            this.cmbNetwork.TabIndex = 3;
            // 
            // lblNetModel
            // 
            this.lblNetModel.AutoSize = true;
            this.lblNetModel.BackColor = System.Drawing.Color.Transparent;
            this.lblNetModel.Font = new System.Drawing.Font("Arial Black", 10F);
            this.lblNetModel.Location = new System.Drawing.Point(110, 13);
            this.lblNetModel.Name = "lblNetModel";
            this.lblNetModel.Size = new System.Drawing.Size(78, 19);
            this.lblNetModel.TabIndex = 2;
            this.lblNetModel.Text = "Network:";
            // 
            // btnMax
            // 
            this.btnMax.Font = new System.Drawing.Font("Marlett", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.btnMax.ForeColor = System.Drawing.Color.Black;
            this.btnMax.Location = new System.Drawing.Point(1183, 10);
            this.btnMax.Name = "btnMax";
            this.btnMax.Size = new System.Drawing.Size(23, 22);
            this.btnMax.TabIndex = 14;
            this.btnMax.Text = "2";
            this.btnMax.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnMax.UseVisualStyleBackColor = true;
            this.btnMax.Click += new System.EventHandler(this.btnMax_Click);
            // 
            // btnMin
            // 
            this.btnMin.BackColor = System.Drawing.Color.Transparent;
            this.btnMin.Font = new System.Drawing.Font("Marlett", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.btnMin.ForeColor = System.Drawing.Color.Black;
            this.btnMin.Location = new System.Drawing.Point(1160, 10);
            this.btnMin.Name = "btnMin";
            this.btnMin.Size = new System.Drawing.Size(23, 22);
            this.btnMin.TabIndex = 13;
            this.btnMin.Text = "0";
            this.btnMin.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnMin.UseVisualStyleBackColor = false;
            this.btnMin.Click += new System.EventHandler(this.btnMin_Click);
            // 
            // mnuMain
            // 
            this.mnuMain.Name = "mnuMain";
            this.mnuMain.Size = new System.Drawing.Size(61, 4);
            this.mnuMain.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.mnuMain_ItemClicked);
            // 
            // tmrMain
            // 
            this.tmrMain.Enabled = true;
            this.tmrMain.Interval = 1000;
            this.tmrMain.Tick += new System.EventHandler(this.UpdateSystemTime);
            // 
            // lblProcesses
            // 
            this.lblProcesses.AutoSize = true;
            this.lblProcesses.BackColor = System.Drawing.Color.Transparent;
            this.lblProcesses.Font = new System.Drawing.Font("Arial Black", 10F);
            this.lblProcesses.Location = new System.Drawing.Point(444, 11);
            this.lblProcesses.Name = "lblProcesses";
            this.lblProcesses.Size = new System.Drawing.Size(43, 19);
            this.lblProcesses.TabIndex = 8;
            this.lblProcesses.Text = "Proc";
            this.lblProcesses.Visible = false;
            this.lblProcesses.Click += new System.EventHandler(this.ChangeVisibility);
            // 
            // chkUseEstimates
            // 
            this.chkUseEstimates.AutoSize = true;
            this.chkUseEstimates.BackColor = System.Drawing.Color.Transparent;
            this.chkUseEstimates.Location = new System.Drawing.Point(604, 16);
            this.chkUseEstimates.Name = "chkUseEstimates";
            this.chkUseEstimates.Size = new System.Drawing.Size(44, 17);
            this.chkUseEstimates.TabIndex = 5;
            this.chkUseEstimates.Text = "Est.";
            this.chkUseEstimates.UseVisualStyleBackColor = false;
            this.chkUseEstimates.CheckedChanged += new System.EventHandler(this.chkUseEstimates_CheckedChanged);
            // 
            // chkPostCtgs
            // 
            this.chkPostCtgs.AutoSize = true;
            this.chkPostCtgs.BackColor = System.Drawing.Color.Transparent;
            this.chkPostCtgs.Location = new System.Drawing.Point(655, 16);
            this.chkPostCtgs.Name = "chkPostCtgs";
            this.chkPostCtgs.Size = new System.Drawing.Size(66, 17);
            this.chkPostCtgs.TabIndex = 6;
            this.chkPostCtgs.Text = "Post-Ctg";
            this.chkPostCtgs.UseVisualStyleBackColor = false;
            this.chkPostCtgs.CheckedChanged += new System.EventHandler(this.chkPostCtgs_CheckedChanged);
            // 
            // cmbSavecase
            // 
            this.cmbSavecase.BackColor = System.Drawing.Color.Black;
            this.cmbSavecase.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbSavecase.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbSavecase.ForeColor = System.Drawing.Color.LightGray;
            this.cmbSavecase.FormattingEnabled = true;
            this.cmbSavecase.Location = new System.Drawing.Point(385, 11);
            this.cmbSavecase.Name = "cmbSavecase";
            this.cmbSavecase.Size = new System.Drawing.Size(53, 24);
            this.cmbSavecase.TabIndex = 37;
            // 
            // chkDisplayAlarms
            // 
            this.chkDisplayAlarms.AutoSize = true;
            this.chkDisplayAlarms.BackColor = System.Drawing.Color.Transparent;
            this.chkDisplayAlarms.Location = new System.Drawing.Point(541, 16);
            this.chkDisplayAlarms.Name = "chkDisplayAlarms";
            this.chkDisplayAlarms.Size = new System.Drawing.Size(57, 17);
            this.chkDisplayAlarms.TabIndex = 38;
            this.chkDisplayAlarms.Text = "Alarms";
            this.chkDisplayAlarms.UseVisualStyleBackColor = false;
            this.chkDisplayAlarms.Visible = false;
            this.chkDisplayAlarms.CheckedChanged += new System.EventHandler(this.chkDisplayalarms_CheckedChanged);
            // 
            // btnData
            // 
            this.btnData.AutoSize = true;
            this.btnData.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnData.ForeColor = System.Drawing.Color.Black;
            this.btnData.Image = global::Macomber_Map.Properties.Resources.PauseHS;
            this.btnData.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnData.Location = new System.Drawing.Point(320, 10);
            this.btnData.Name = "btnData";
            this.btnData.Size = new System.Drawing.Size(59, 23);
            this.btnData.TabIndex = 4;
            this.btnData.Text = "    Pause";
            this.btnData.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnData.UseVisualStyleBackColor = true;
            this.btnData.Click += new System.EventHandler(this.btnData_Click);
            // 
            // picERCOT
            // 
            this.picERCOT.BackColor = System.Drawing.Color.Transparent;
            this.picERCOT.ContextMenuStrip = this.mnuMain;
            this.picERCOT.Image = global::Macomber_Map.Properties.Resources.ERCOT_Logo;
            this.picERCOT.Location = new System.Drawing.Point(9, 6);
            this.picERCOT.Name = "picERCOT";
            this.picERCOT.Size = new System.Drawing.Size(87, 31);
            this.picERCOT.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picERCOT.TabIndex = 27;
            this.picERCOT.TabStop = false;
            this.picERCOT.MouseClick += new System.Windows.Forms.MouseEventHandler(this.picERCOT_MouseClick);
            // 
            // txtSearch
            // 
            this.txtSearch.BackColor = System.Drawing.Color.Black;
            this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSearch.ForeColor = System.Drawing.Color.LightGray;
            this.txtSearch.Location = new System.Drawing.Point(972, 12);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(154, 22);
            this.txtSearch.TabIndex = 12;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            this.txtSearch.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtSearch_KeyPress);
            this.txtSearch.SearchClicked += new System.EventHandler(this.txtSearch_SearchClicked);
            // 
            // lblNotes
            // 
            this.lblNotes.AutoSize = true;
            this.lblNotes.BackColor = System.Drawing.Color.Transparent;
            this.lblNotes.Font = new System.Drawing.Font("Arial Black", 10F);
            this.lblNotes.Image = global::Macomber_Map.Properties.Resources.NoteHS;
            this.lblNotes.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblNotes.Location = new System.Drawing.Point(868, 13);
            this.lblNotes.Name = "lblNotes";
            this.lblNotes.Size = new System.Drawing.Size(33, 19);
            this.lblNotes.TabIndex = 10;
            this.lblNotes.Text = "   0";
            this.lblNotes.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblNotes.Click += new System.EventHandler(this.ChangeVisibility);
            // 
            // lblComm
            // 
            this.lblComm.BackColor = System.Drawing.Color.Transparent;
            this.lblComm.Font = new System.Drawing.Font("Arial Black", 10F);
            this.lblComm.Location = new System.Drawing.Point(726, 13);
            this.lblComm.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.lblComm.Name = "lblComm";
            this.lblComm.Size = new System.Drawing.Size(72, 21);
            this.lblComm.TabIndex = 7;
            this.lblComm.Click += new System.EventHandler(this.ChangeVisibility);
            // 
            // Key_Indicators
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chkDisplayAlarms);
            this.Controls.Add(this.cmbSavecase);
            this.Controls.Add(this.lblNetModel);
            this.Controls.Add(this.btnData);
            this.Controls.Add(this.chkUseEstimates);
            this.Controls.Add(this.picERCOT);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.cmbNetwork);
            this.Controls.Add(this.chkPostCtgs);
            this.Controls.Add(this.lblNotes);
            this.Controls.Add(this.btnMax);
            this.Controls.Add(this.lblViews);
            this.Controls.Add(this.lblProcesses);
            this.Controls.Add(this.lblComm);
            this.Controls.Add(this.btnMin);
            this.Name = "Key_Indicators";
            this.Size = new System.Drawing.Size(1207, 44);
            this.Resize += new System.EventHandler(this.KeyIndicators_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.picERCOT)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblViews;
        private System.Windows.Forms.ComboBox cmbNetwork;
        private System.Windows.Forms.Label lblNetModel;
        private Macomber_Map.User_Interface_Components.Overrides.MM_SearchBox txtSearch;
        private System.Windows.Forms.Button btnMax;
        private System.Windows.Forms.Button btnMin;
        private System.Windows.Forms.PictureBox picERCOT;
        private System.Windows.Forms.Timer tmrMain;
        private System.Windows.Forms.Label lblProcesses;
        private System.Windows.Forms.ContextMenuStrip mnuViews;
        private System.Windows.Forms.ContextMenuStrip mnuMain;
        private System.Windows.Forms.Label lblNotes;
        private System.Windows.Forms.Button btnData;
        private System.Windows.Forms.CheckBox chkUseEstimates;
        private Communication_Status lblComm;
        private System.Windows.Forms.ToolTip tTip;
        private System.Windows.Forms.CheckBox chkPostCtgs;
        private System.Windows.Forms.ComboBox cmbSavecase;
        private System.Windows.Forms.CheckBox chkDisplayAlarms;

    }
}
