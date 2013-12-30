namespace Macomber_Map.User_Interface_Components.OneLines
{
    partial class MM_OneLine_Synchroscope
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
            this.LabelPhaseDiff = new System.Windows.Forms.Label();
            this.LabelCB = new System.Windows.Forms.Label();
            this.LabelZND = new System.Windows.Forms.Label();
            this.LabelIND = new System.Windows.Forms.Label();
            this.LabelSync = new System.Windows.Forms.Label();
            this.LabelKV = new System.Windows.Forms.Label();
            this.LabelKVI = new System.Windows.Forms.Label();
            this.LabelKVZ = new System.Windows.Forms.Label();
            this.LabelSyncOn = new System.Windows.Forms.Label();
            this.LabelFreqZ = new System.Windows.Forms.Label();
            this.LabelFreqI = new System.Windows.Forms.Label();
            this.LabelAngleDiff = new System.Windows.Forms.Label();
            this.LabelFrequency_ = new System.Windows.Forms.Label();
            this.LabelCBID = new System.Windows.Forms.Label();
            this.LabelCBID_ = new System.Windows.Forms.Label();
            this.LabelStation = new System.Windows.Forms.Label();
            this.LabelStation_ = new System.Windows.Forms.Label();
            this.Synchroscope = new System.Windows.Forms.PictureBox();
            this.picLineUnderName = new System.Windows.Forms.PictureBox();
            this.picLineUnderScope = new System.Windows.Forms.PictureBox();
            this.tmrUpdate = new System.Windows.Forms.Timer(this.components);
            this.lblDataSource = new System.Windows.Forms.Label();
            this.lblApplication = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.Synchroscope)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picLineUnderName)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picLineUnderScope)).BeginInit();
            this.SuspendLayout();
            // 
            // LabelPhaseDiff
            // 
            this.LabelPhaseDiff.BackColor = System.Drawing.Color.Black;
            this.LabelPhaseDiff.Cursor = System.Windows.Forms.Cursors.Default;
            this.LabelPhaseDiff.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelPhaseDiff.ForeColor = System.Drawing.Color.White;
            this.LabelPhaseDiff.Location = new System.Drawing.Point(166, 364);
            this.LabelPhaseDiff.Name = "LabelPhaseDiff";
            this.LabelPhaseDiff.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.LabelPhaseDiff.Size = new System.Drawing.Size(33, 17);
            this.LabelPhaseDiff.TabIndex = 18;
            this.LabelPhaseDiff.Text = "0.000";
            this.LabelPhaseDiff.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // LabelCB
            // 
            this.LabelCB.BackColor = System.Drawing.Color.Black;
            this.LabelCB.Cursor = System.Windows.Forms.Cursors.Default;
            this.LabelCB.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelCB.ForeColor = System.Drawing.Color.White;
            this.LabelCB.Location = new System.Drawing.Point(25, 287);
            this.LabelCB.Name = "LabelCB";
            this.LabelCB.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.LabelCB.Size = new System.Drawing.Size(172, 46);
            this.LabelCB.TabIndex = 8;
            this.LabelCB.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.LabelCB.Paint += new System.Windows.Forms.PaintEventHandler(this.LabelCB_Paint);
            // 
            // LabelZND
            // 
            this.LabelZND.BackColor = System.Drawing.Color.Transparent;
            this.LabelZND.Cursor = System.Windows.Forms.Cursors.Default;
            this.LabelZND.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelZND.ForeColor = System.Drawing.Color.White;
            this.LabelZND.Location = new System.Drawing.Point(142, 300);
            this.LabelZND.Name = "LabelZND";
            this.LabelZND.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.LabelZND.Size = new System.Drawing.Size(57, 25);
            this.LabelZND.TabIndex = 10;
            this.LabelZND.Text = "000";
            this.LabelZND.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // LabelIND
            // 
            this.LabelIND.BackColor = System.Drawing.Color.Transparent;
            this.LabelIND.Cursor = System.Windows.Forms.Cursors.Default;
            this.LabelIND.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelIND.ForeColor = System.Drawing.Color.White;
            this.LabelIND.Location = new System.Drawing.Point(22, 300);
            this.LabelIND.Name = "LabelIND";
            this.LabelIND.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.LabelIND.Size = new System.Drawing.Size(57, 17);
            this.LabelIND.TabIndex = 9;
            this.LabelIND.Text = "000";
            // 
            // LabelSync
            // 
            this.LabelSync.BackColor = System.Drawing.Color.Black;
            this.LabelSync.Cursor = System.Windows.Forms.Cursors.Default;
            this.LabelSync.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelSync.ForeColor = System.Drawing.Color.White;
            this.LabelSync.Location = new System.Drawing.Point(22, 76);
            this.LabelSync.Name = "LabelSync";
            this.LabelSync.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.LabelSync.Size = new System.Drawing.Size(177, 17);
            this.LabelSync.TabIndex = 6;
            this.LabelSync.Text = "Synchroscope";
            this.LabelSync.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // LabelKV
            // 
            this.LabelKV.BackColor = System.Drawing.Color.Black;
            this.LabelKV.Cursor = System.Windows.Forms.Cursors.Default;
            this.LabelKV.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelKV.ForeColor = System.Drawing.Color.White;
            this.LabelKV.Location = new System.Drawing.Point(62, 348);
            this.LabelKV.Name = "LabelKV";
            this.LabelKV.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.LabelKV.Size = new System.Drawing.Size(97, 17);
            this.LabelKV.TabIndex = 15;
            this.LabelKV.Text = "Voltage (kV):";
            this.LabelKV.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // LabelKVI
            // 
            this.LabelKVI.BackColor = System.Drawing.Color.Black;
            this.LabelKVI.Cursor = System.Windows.Forms.Cursors.Default;
            this.LabelKVI.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelKVI.ForeColor = System.Drawing.Color.White;
            this.LabelKVI.Location = new System.Drawing.Point(22, 348);
            this.LabelKVI.Name = "LabelKVI";
            this.LabelKVI.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.LabelKVI.Size = new System.Drawing.Size(49, 17);
            this.LabelKVI.TabIndex = 14;
            this.LabelKVI.Text = "0.0000";
            // 
            // LabelKVZ
            // 
            this.LabelKVZ.BackColor = System.Drawing.Color.Black;
            this.LabelKVZ.Cursor = System.Windows.Forms.Cursors.Default;
            this.LabelKVZ.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelKVZ.ForeColor = System.Drawing.Color.White;
            this.LabelKVZ.Location = new System.Drawing.Point(150, 348);
            this.LabelKVZ.Name = "LabelKVZ";
            this.LabelKVZ.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.LabelKVZ.Size = new System.Drawing.Size(49, 17);
            this.LabelKVZ.TabIndex = 16;
            this.LabelKVZ.Text = "0.0000";
            this.LabelKVZ.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // LabelSyncOn
            // 
            this.LabelSyncOn.BackColor = System.Drawing.Color.Black;
            this.LabelSyncOn.Cursor = System.Windows.Forms.Cursors.Default;
            this.LabelSyncOn.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelSyncOn.ForeColor = System.Drawing.Color.White;
            this.LabelSyncOn.Location = new System.Drawing.Point(22, 268);
            this.LabelSyncOn.Name = "LabelSyncOn";
            this.LabelSyncOn.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.LabelSyncOn.Size = new System.Drawing.Size(41, 17);
            this.LabelSyncOn.TabIndex = 7;
            this.LabelSyncOn.Text = "Sync On";
            // 
            // LabelFreqZ
            // 
            this.LabelFreqZ.BackColor = System.Drawing.Color.Black;
            this.LabelFreqZ.Cursor = System.Windows.Forms.Cursors.Default;
            this.LabelFreqZ.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelFreqZ.ForeColor = System.Drawing.Color.White;
            this.LabelFreqZ.Location = new System.Drawing.Point(150, 332);
            this.LabelFreqZ.Name = "LabelFreqZ";
            this.LabelFreqZ.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.LabelFreqZ.Size = new System.Drawing.Size(49, 17);
            this.LabelFreqZ.TabIndex = 13;
            this.LabelFreqZ.Text = "60.0";
            this.LabelFreqZ.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // LabelFreqI
            // 
            this.LabelFreqI.BackColor = System.Drawing.Color.Black;
            this.LabelFreqI.Cursor = System.Windows.Forms.Cursors.Default;
            this.LabelFreqI.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelFreqI.ForeColor = System.Drawing.Color.White;
            this.LabelFreqI.Location = new System.Drawing.Point(22, 332);
            this.LabelFreqI.Name = "LabelFreqI";
            this.LabelFreqI.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.LabelFreqI.Size = new System.Drawing.Size(49, 17);
            this.LabelFreqI.TabIndex = 11;
            this.LabelFreqI.Text = "60.0";
            // 
            // LabelAngleDiff
            // 
            this.LabelAngleDiff.BackColor = System.Drawing.Color.Black;
            this.LabelAngleDiff.Cursor = System.Windows.Forms.Cursors.Default;
            this.LabelAngleDiff.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelAngleDiff.ForeColor = System.Drawing.Color.White;
            this.LabelAngleDiff.Location = new System.Drawing.Point(22, 364);
            this.LabelAngleDiff.Name = "LabelAngleDiff";
            this.LabelAngleDiff.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.LabelAngleDiff.Size = new System.Drawing.Size(137, 17);
            this.LabelAngleDiff.TabIndex = 17;
            this.LabelAngleDiff.Text = "Phase Angle Diff.:";
            // 
            // LabelFrequency_
            // 
            this.LabelFrequency_.BackColor = System.Drawing.Color.Black;
            this.LabelFrequency_.Cursor = System.Windows.Forms.Cursors.Default;
            this.LabelFrequency_.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelFrequency_.ForeColor = System.Drawing.Color.White;
            this.LabelFrequency_.Location = new System.Drawing.Point(62, 332);
            this.LabelFrequency_.Name = "LabelFrequency_";
            this.LabelFrequency_.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.LabelFrequency_.Size = new System.Drawing.Size(97, 17);
            this.LabelFrequency_.TabIndex = 12;
            this.LabelFrequency_.Text = "Frequency:";
            this.LabelFrequency_.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // LabelCBID
            // 
            this.LabelCBID.BackColor = System.Drawing.Color.Black;
            this.LabelCBID.Cursor = System.Windows.Forms.Cursors.Default;
            this.LabelCBID.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelCBID.ForeColor = System.Drawing.Color.White;
            this.LabelCBID.Location = new System.Drawing.Point(84, 44);
            this.LabelCBID.Name = "LabelCBID";
            this.LabelCBID.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.LabelCBID.Size = new System.Drawing.Size(115, 14);
            this.LabelCBID.TabIndex = 5;
            this.LabelCBID.Text = "XXXXXXX";
            this.LabelCBID.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // LabelCBID_
            // 
            this.LabelCBID_.AutoSize = true;
            this.LabelCBID_.BackColor = System.Drawing.Color.Black;
            this.LabelCBID_.Cursor = System.Windows.Forms.Cursors.Default;
            this.LabelCBID_.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelCBID_.ForeColor = System.Drawing.Color.White;
            this.LabelCBID_.Location = new System.Drawing.Point(22, 44);
            this.LabelCBID_.Name = "LabelCBID_";
            this.LabelCBID_.Size = new System.Drawing.Size(106, 14);
            this.LabelCBID_.TabIndex = 4;
            this.LabelCBID_.Text = "Circuit Breaker ID:";
            // 
            // LabelStation
            // 
            this.LabelStation.BackColor = System.Drawing.Color.Black;
            this.LabelStation.Cursor = System.Windows.Forms.Cursors.Default;
            this.LabelStation.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelStation.ForeColor = System.Drawing.Color.White;
            this.LabelStation.Location = new System.Drawing.Point(70, 28);
            this.LabelStation.Name = "LabelStation";
            this.LabelStation.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.LabelStation.Size = new System.Drawing.Size(129, 17);
            this.LabelStation.TabIndex = 3;
            this.LabelStation.Text = "XXXXXXX";
            this.LabelStation.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // LabelStation_
            // 
            this.LabelStation_.AutoSize = true;
            this.LabelStation_.BackColor = System.Drawing.Color.Black;
            this.LabelStation_.Cursor = System.Windows.Forms.Cursors.Default;
            this.LabelStation_.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelStation_.ForeColor = System.Drawing.Color.White;
            this.LabelStation_.Location = new System.Drawing.Point(22, 28);
            this.LabelStation_.Name = "LabelStation_";
            this.LabelStation_.Size = new System.Drawing.Size(48, 14);
            this.LabelStation_.TabIndex = 2;
            this.LabelStation_.Text = "Station:";
            // 
            // Synchroscope
            // 
            this.Synchroscope.BackColor = System.Drawing.Color.Black;
            this.Synchroscope.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.Synchroscope.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Synchroscope.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Synchroscope.Location = new System.Drawing.Point(20, 92);
            this.Synchroscope.Name = "Synchroscope";
            this.Synchroscope.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Synchroscope.Size = new System.Drawing.Size(177, 177);
            this.Synchroscope.TabIndex = 39;
            this.Synchroscope.TabStop = false;
            // 
            // picLineUnderName
            // 
            this.picLineUnderName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picLineUnderName.Location = new System.Drawing.Point(20, 68);
            this.picLineUnderName.Name = "picLineUnderName";
            this.picLineUnderName.Size = new System.Drawing.Size(177, 1);
            this.picLineUnderName.TabIndex = 40;
            this.picLineUnderName.TabStop = false;
            // 
            // picLineUnderScope
            // 
            this.picLineUnderScope.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picLineUnderScope.Location = new System.Drawing.Point(20, 283);
            this.picLineUnderScope.Name = "picLineUnderScope";
            this.picLineUnderScope.Size = new System.Drawing.Size(177, 1);
            this.picLineUnderScope.TabIndex = 41;
            this.picLineUnderScope.TabStop = false;
            // 
            // tmrUpdate
            // 
            this.tmrUpdate.Interval = 20;
            this.tmrUpdate.Tick += new System.EventHandler(this.tmrUpdate_Tick);
            // 
            // lblDataSource
            // 
            this.lblDataSource.AutoSize = true;
            this.lblDataSource.BackColor = System.Drawing.Color.Black;
            this.lblDataSource.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblDataSource.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDataSource.ForeColor = System.Drawing.Color.White;
            this.lblDataSource.Location = new System.Drawing.Point(22, 9);
            this.lblDataSource.Name = "lblDataSource";
            this.lblDataSource.Size = new System.Drawing.Size(75, 14);
            this.lblDataSource.TabIndex = 0;
            this.lblDataSource.Text = "Data Source:";
             // 
            // lblApplication
            // 
            this.lblApplication.BackColor = System.Drawing.Color.Black;
            this.lblApplication.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblApplication.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblApplication.ForeColor = System.Drawing.Color.White;
            this.lblApplication.Location = new System.Drawing.Point(70, 8);
            this.lblApplication.Name = "lblApplication";
            this.lblApplication.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblApplication.Size = new System.Drawing.Size(129, 17);
            this.lblApplication.TabIndex = 42;
            this.lblApplication.Text = "XXXXXXX";
            this.lblApplication.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // MM_OneLine_Synchroscope
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(217, 380);
            this.Controls.Add(this.lblDataSource);
            this.Controls.Add(this.LabelCB);
            this.Controls.Add(this.picLineUnderScope);
            this.Controls.Add(this.picLineUnderName);
            this.Controls.Add(this.LabelCBID_);
            this.Controls.Add(this.Synchroscope);
            this.Controls.Add(this.LabelPhaseDiff);
            this.Controls.Add(this.LabelZND);
            this.Controls.Add(this.LabelIND);
            this.Controls.Add(this.LabelSync);
            this.Controls.Add(this.LabelKV);
            this.Controls.Add(this.LabelKVI);
            this.Controls.Add(this.LabelKVZ);
            this.Controls.Add(this.LabelSyncOn);
            this.Controls.Add(this.LabelFreqZ);
            this.Controls.Add(this.LabelFreqI);
            this.Controls.Add(this.LabelAngleDiff);
            this.Controls.Add(this.LabelFrequency_);
            this.Controls.Add(this.LabelCBID);
            this.Controls.Add(this.LabelStation);
            this.Controls.Add(this.LabelStation_);
            this.Controls.Add(this.lblApplication);
            this.ForeColor = System.Drawing.Color.White;
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "MM_OneLine_Synchroscope";
            this.Text = "Synchroscope";
            ((System.ComponentModel.ISupportInitialize)(this.Synchroscope)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picLineUnderName)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picLineUnderScope)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.Label LabelPhaseDiff;
        internal System.Windows.Forms.Label LabelCB;
        internal System.Windows.Forms.Label LabelZND;
        internal System.Windows.Forms.Label LabelIND;
        internal System.Windows.Forms.Label LabelSync;
        internal System.Windows.Forms.Label LabelKV;
        internal System.Windows.Forms.Label LabelKVI;
        internal System.Windows.Forms.Label LabelKVZ;
        internal System.Windows.Forms.Label LabelSyncOn;
        internal System.Windows.Forms.Label LabelFreqZ;
        internal System.Windows.Forms.Label LabelFreqI;
        internal System.Windows.Forms.Label LabelAngleDiff;
        internal System.Windows.Forms.Label LabelFrequency_;
        internal System.Windows.Forms.Label LabelCBID;
        internal System.Windows.Forms.Label LabelCBID_;
        internal System.Windows.Forms.Label LabelStation;
        internal System.Windows.Forms.Label LabelStation_;
        internal System.Windows.Forms.PictureBox Synchroscope;
        private System.Windows.Forms.PictureBox picLineUnderName;
        private System.Windows.Forms.PictureBox picLineUnderScope;
        private System.Windows.Forms.Timer tmrUpdate;
        internal System.Windows.Forms.Label lblDataSource;
        internal System.Windows.Forms.Label lblApplication;
    }
}