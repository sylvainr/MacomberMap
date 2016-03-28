namespace MacomberMapClient.User_Interfaces.Blackstart
{
    partial class MM_Video_Recorder
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
            this.lblCaptureFrameEvery = new System.Windows.Forms.Label();
            this.txtCaptureEvery = new System.Windows.Forms.NumericUpDown();
            this.btnRecord = new System.Windows.Forms.Button();
            this.lblFramesCaptured = new System.Windows.Forms.Label();
            this.tmrCaptureScreenshot = new System.Windows.Forms.Timer(this.components);
            this.lblFPS = new System.Windows.Forms.Label();
            this.txtFPS = new System.Windows.Forms.NumericUpDown();
            this.btnScreenLock = new System.Windows.Forms.Button();
            this.btnPositionScreen = new System.Windows.Forms.Button();
            this.btnProduceVideo = new System.Windows.Forms.Button();
            this.chkCapturePaused = new System.Windows.Forms.CheckBox();
            this.cmbVideoRecorder = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.txtCaptureEvery)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtFPS)).BeginInit();
            this.SuspendLayout();
            // 
            // lblCaptureFrameEvery
            // 
            this.lblCaptureFrameEvery.AutoSize = true;
            this.lblCaptureFrameEvery.Location = new System.Drawing.Point(12, 44);
            this.lblCaptureFrameEvery.Name = "lblCaptureFrameEvery";
            this.lblCaptureFrameEvery.Size = new System.Drawing.Size(189, 13);
            this.lblCaptureFrameEvery.TabIndex = 2;
            this.lblCaptureFrameEvery.Text = "Every second of video corresponds to:";
            // 
            // txtCaptureEvery
            // 
            this.txtCaptureEvery.Location = new System.Drawing.Point(214, 42);
            this.txtCaptureEvery.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.txtCaptureEvery.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.txtCaptureEvery.Name = "txtCaptureEvery";
            this.txtCaptureEvery.Size = new System.Drawing.Size(87, 20);
            this.txtCaptureEvery.TabIndex = 4;
            this.txtCaptureEvery.Value = new decimal(new int[] {
            15,
            0,
            0,
            0});
            // 
            // btnRecord
            // 
            this.btnRecord.Location = new System.Drawing.Point(215, 101);
            this.btnRecord.Name = "btnRecord";
            this.btnRecord.Size = new System.Drawing.Size(94, 47);
            this.btnRecord.TabIndex = 5;
            this.btnRecord.Text = "RECORD";
            this.btnRecord.UseVisualStyleBackColor = true;
            this.btnRecord.Click += new System.EventHandler(this.btnRecord_Click);
            // 
            // lblFramesCaptured
            // 
            this.lblFramesCaptured.AutoSize = true;
            this.lblFramesCaptured.Location = new System.Drawing.Point(12, 164);
            this.lblFramesCaptured.Name = "lblFramesCaptured";
            this.lblFramesCaptured.Size = new System.Drawing.Size(89, 13);
            this.lblFramesCaptured.TabIndex = 6;
            this.lblFramesCaptured.Text = "Frames captured:";
            // 
            // tmrCaptureScreenshot
            // 
            this.tmrCaptureScreenshot.Tick += new System.EventHandler(this.tmrCaptureScreenshot_Tick);
            // 
            // lblFPS
            // 
            this.lblFPS.AutoSize = true;
            this.lblFPS.Location = new System.Drawing.Point(12, 9);
            this.lblFPS.Name = "lblFPS";
            this.lblFPS.Size = new System.Drawing.Size(130, 13);
            this.lblFPS.TabIndex = 7;
            this.lblFPS.Text = "Video Frames per second:";
            // 
            // txtFPS
            // 
            this.txtFPS.Location = new System.Drawing.Point(214, 7);
            this.txtFPS.Name = "txtFPS";
            this.txtFPS.Size = new System.Drawing.Size(87, 20);
            this.txtFPS.TabIndex = 8;
            this.txtFPS.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // btnScreenLock
            // 
            this.btnScreenLock.Location = new System.Drawing.Point(15, 101);
            this.btnScreenLock.Name = "btnScreenLock";
            this.btnScreenLock.Size = new System.Drawing.Size(94, 47);
            this.btnScreenLock.TabIndex = 9;
            this.btnScreenLock.Text = "Screen Lock:\r\nOFF";
            this.btnScreenLock.UseVisualStyleBackColor = true;
            this.btnScreenLock.Click += new System.EventHandler(this.btnScreenLock_Click);
            // 
            // btnPositionScreen
            // 
            this.btnPositionScreen.Location = new System.Drawing.Point(115, 101);
            this.btnPositionScreen.Name = "btnPositionScreen";
            this.btnPositionScreen.Size = new System.Drawing.Size(94, 47);
            this.btnPositionScreen.TabIndex = 10;
            this.btnPositionScreen.Text = "Position Screen";
            this.btnPositionScreen.UseVisualStyleBackColor = true;
            this.btnPositionScreen.Click += new System.EventHandler(this.btnPositionScreen_Click);
            // 
            // btnProduceVideo
            // 
            this.btnProduceVideo.Location = new System.Drawing.Point(319, 101);
            this.btnProduceVideo.Name = "btnProduceVideo";
            this.btnProduceVideo.Size = new System.Drawing.Size(94, 47);
            this.btnProduceVideo.TabIndex = 11;
            this.btnProduceVideo.Text = "Produce Video";
            this.btnProduceVideo.UseVisualStyleBackColor = true;
            this.btnProduceVideo.Click += new System.EventHandler(this.btnProduceVideo_Click);
            // 
            // chkCapturePaused
            // 
            this.chkCapturePaused.AutoSize = true;
            this.chkCapturePaused.Location = new System.Drawing.Point(12, 69);
            this.chkCapturePaused.Name = "chkCapturePaused";
            this.chkCapturePaused.Size = new System.Drawing.Size(136, 17);
            this.chkCapturePaused.TabIndex = 12;
            this.chkCapturePaused.Text = "Capture Paused frames";
            this.chkCapturePaused.UseVisualStyleBackColor = true;
            // 
            // cmbVideoRecorder
            // 
            this.cmbVideoRecorder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbVideoRecorder.FormattingEnabled = true;
            this.cmbVideoRecorder.Items.AddRange(new object[] {
            "minutes",
            "hours",
            "seconds"});
            this.cmbVideoRecorder.Location = new System.Drawing.Point(307, 41);
            this.cmbVideoRecorder.Name = "cmbVideoRecorder";
            this.cmbVideoRecorder.Size = new System.Drawing.Size(93, 21);
            this.cmbVideoRecorder.TabIndex = 13;
            // 
            // MM_Video_Recorder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(421, 197);
            this.Controls.Add(this.cmbVideoRecorder);
            this.Controls.Add(this.chkCapturePaused);
            this.Controls.Add(this.btnProduceVideo);
            this.Controls.Add(this.btnPositionScreen);
            this.Controls.Add(this.btnScreenLock);
            this.Controls.Add(this.txtFPS);
            this.Controls.Add(this.lblFPS);
            this.Controls.Add(this.lblFramesCaptured);
            this.Controls.Add(this.btnRecord);
            this.Controls.Add(this.txtCaptureEvery);
            this.Controls.Add(this.lblCaptureFrameEvery);
            this.Icon = Properties.Resources.CompanyIcon;
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "MM_Video_Recorder";
            this.Text = "Macomber Map Video Recorder";
            ((System.ComponentModel.ISupportInitialize)(this.txtCaptureEvery)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtFPS)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblCaptureFrameEvery;
        private System.Windows.Forms.NumericUpDown txtCaptureEvery;
        private System.Windows.Forms.Button btnRecord;
        private System.Windows.Forms.Label lblFramesCaptured;
        private System.Windows.Forms.Timer tmrCaptureScreenshot;
        private System.Windows.Forms.Label lblFPS;
        private System.Windows.Forms.NumericUpDown txtFPS;
        private System.Windows.Forms.Button btnScreenLock;
        private System.Windows.Forms.Button btnPositionScreen;
        private System.Windows.Forms.Button btnProduceVideo;
        private System.Windows.Forms.CheckBox chkCapturePaused;
        private System.Windows.Forms.ComboBox cmbVideoRecorder;
    }
}