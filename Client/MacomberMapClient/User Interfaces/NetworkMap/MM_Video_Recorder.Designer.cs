namespace MacomberMapClient.User_Interfaces.NetworkMap
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MM_Video_Recorder));
            this.lblCaptureFrameEvery = new System.Windows.Forms.Label();
            this.lblTimeConversion = new System.Windows.Forms.Label();
            this.txtCaptureEvery = new System.Windows.Forms.NumericUpDown();
            this.btnRecord = new System.Windows.Forms.Button();
            this.lblFramesCaptured = new System.Windows.Forms.Label();
            this.tmrCaptureScreenshot = new System.Windows.Forms.Timer(this.components);
            this.lblFPS = new System.Windows.Forms.Label();
            this.txtFPS = new System.Windows.Forms.NumericUpDown();
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
            // lblTimeConversion
            // 
            this.lblTimeConversion.AutoSize = true;
            this.lblTimeConversion.Location = new System.Drawing.Point(316, 44);
            this.lblTimeConversion.Name = "lblTimeConversion";
            this.lblTimeConversion.Size = new System.Drawing.Size(43, 13);
            this.lblTimeConversion.TabIndex = 3;
            this.lblTimeConversion.Text = "minutes";
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
            this.btnRecord.Location = new System.Drawing.Point(149, 77);
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
            this.lblFramesCaptured.Location = new System.Drawing.Point(12, 138);
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
            // MM_Video_Recorder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(370, 197);
            this.Controls.Add(this.txtFPS);
            this.Controls.Add(this.lblFPS);
            this.Controls.Add(this.lblFramesCaptured);
            this.Controls.Add(this.btnRecord);
            this.Controls.Add(this.txtCaptureEvery);
            this.Controls.Add(this.lblTimeConversion);
            this.Controls.Add(this.lblCaptureFrameEvery);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
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
        private System.Windows.Forms.Label lblTimeConversion;
        private System.Windows.Forms.NumericUpDown txtCaptureEvery;
        private System.Windows.Forms.Button btnRecord;
        private System.Windows.Forms.Label lblFramesCaptured;
        private System.Windows.Forms.Timer tmrCaptureScreenshot;
        private System.Windows.Forms.Label lblFPS;
        private System.Windows.Forms.NumericUpDown txtFPS;
    }
}