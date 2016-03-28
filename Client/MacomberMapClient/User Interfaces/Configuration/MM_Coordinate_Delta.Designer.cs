namespace MacomberMapClient.User_Interfaces.Configuration
{
    partial class MM_Coordinate_Delta
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
            this.splMain = new System.Windows.Forms.SplitContainer();
            this.grpCurrent = new System.Windows.Forms.GroupBox();
            this.txtSubLatitude = new System.Windows.Forms.TextBox();
            this.txtSubLongitude = new System.Windows.Forms.TextBox();
            this.lblCoordinates = new System.Windows.Forms.Label();
            this.lblElementValue = new System.Windows.Forms.Label();
            this.lblElement = new System.Windows.Forms.Label();
            this.dgvLineLngLat = new System.Windows.Forms.DataGridView();
            this.grpChanges = new System.Windows.Forms.GroupBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.dgvChanges = new System.Windows.Forms.DataGridView();
            this.btnEmail = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splMain)).BeginInit();
            this.splMain.Panel1.SuspendLayout();
            this.splMain.Panel2.SuspendLayout();
            this.splMain.SuspendLayout();
            this.grpCurrent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLineLngLat)).BeginInit();
            this.grpChanges.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvChanges)).BeginInit();
            this.SuspendLayout();
            // 
            // splMain
            // 
            this.splMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splMain.Location = new System.Drawing.Point(0, 0);
            this.splMain.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.splMain.Name = "splMain";
            this.splMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splMain.Panel1
            // 
            this.splMain.Panel1.Controls.Add(this.grpCurrent);
            // 
            // splMain.Panel2
            // 
            this.splMain.Panel2.Controls.Add(this.grpChanges);
            this.splMain.Size = new System.Drawing.Size(366, 351);
            this.splMain.SplitterDistance = 131;
            this.splMain.SplitterWidth = 3;
            this.splMain.TabIndex = 0;
            // 
            // grpCurrent
            // 
            this.grpCurrent.Controls.Add(this.txtSubLatitude);
            this.grpCurrent.Controls.Add(this.txtSubLongitude);
            this.grpCurrent.Controls.Add(this.lblCoordinates);
            this.grpCurrent.Controls.Add(this.lblElementValue);
            this.grpCurrent.Controls.Add(this.lblElement);
            this.grpCurrent.Controls.Add(this.dgvLineLngLat);
            this.grpCurrent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpCurrent.Location = new System.Drawing.Point(0, 0);
            this.grpCurrent.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.grpCurrent.Name = "grpCurrent";
            this.grpCurrent.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.grpCurrent.Size = new System.Drawing.Size(366, 131);
            this.grpCurrent.TabIndex = 0;
            this.grpCurrent.TabStop = false;
            this.grpCurrent.Text = "Current Item";
            // 
            // txtSubLatitude
            // 
            this.txtSubLatitude.Location = new System.Drawing.Point(175, 46);
            this.txtSubLatitude.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtSubLatitude.Name = "txtSubLatitude";
            this.txtSubLatitude.Size = new System.Drawing.Size(76, 20);
            this.txtSubLatitude.TabIndex = 4;
            this.txtSubLatitude.Visible = false;
            // 
            // txtSubLongitude
            // 
            this.txtSubLongitude.Location = new System.Drawing.Point(87, 46);
            this.txtSubLongitude.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtSubLongitude.Name = "txtSubLongitude";
            this.txtSubLongitude.Size = new System.Drawing.Size(76, 20);
            this.txtSubLongitude.TabIndex = 3;
            this.txtSubLongitude.Visible = false;
            // 
            // lblCoordinates
            // 
            this.lblCoordinates.AutoSize = true;
            this.lblCoordinates.Location = new System.Drawing.Point(9, 49);
            this.lblCoordinates.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblCoordinates.Name = "lblCoordinates";
            this.lblCoordinates.Size = new System.Drawing.Size(72, 13);
            this.lblCoordinates.TabIndex = 2;
            this.lblCoordinates.Text = "Coordinate(s):";
            // 
            // lblElementValue
            // 
            this.lblElementValue.AutoSize = true;
            this.lblElementValue.Location = new System.Drawing.Point(61, 26);
            this.lblElementValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblElementValue.Name = "lblElementValue";
            this.lblElementValue.Size = new System.Drawing.Size(0, 13);
            this.lblElementValue.TabIndex = 1;
            // 
            // lblElement
            // 
            this.lblElement.AutoSize = true;
            this.lblElement.Location = new System.Drawing.Point(9, 26);
            this.lblElement.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblElement.Name = "lblElement";
            this.lblElement.Size = new System.Drawing.Size(48, 13);
            this.lblElement.TabIndex = 0;
            this.lblElement.Text = "Element:";
            // 
            // dgvLineLngLat
            // 
            this.dgvLineLngLat.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvLineLngLat.Location = new System.Drawing.Point(87, 46);
            this.dgvLineLngLat.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dgvLineLngLat.Name = "dgvLineLngLat";
            this.dgvLineLngLat.RowTemplate.Height = 24;
            this.dgvLineLngLat.Size = new System.Drawing.Size(270, 80);
            this.dgvLineLngLat.TabIndex = 5;
            this.dgvLineLngLat.Visible = false;
            // 
            // grpChanges
            // 
            this.grpChanges.Controls.Add(this.btnCancel);
            this.grpChanges.Controls.Add(this.btnClose);
            this.grpChanges.Controls.Add(this.btnEmail);
            this.grpChanges.Controls.Add(this.dgvChanges);
            this.grpChanges.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpChanges.Location = new System.Drawing.Point(0, 0);
            this.grpChanges.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.grpChanges.Name = "grpChanges";
            this.grpChanges.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.grpChanges.Size = new System.Drawing.Size(366, 217);
            this.grpChanges.TabIndex = 0;
            this.grpChanges.TabStop = false;
            this.grpChanges.Text = "Changes";
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(258, 188);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(99, 26);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Revert/Cancel all";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // dgvChanges
            // 
            this.dgvChanges.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvChanges.Location = new System.Drawing.Point(11, 17);
            this.dgvChanges.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dgvChanges.Name = "dgvChanges";
            this.dgvChanges.RowTemplate.Height = 24;
            this.dgvChanges.Size = new System.Drawing.Size(346, 162);
            this.dgvChanges.TabIndex = 0;
            // 
            // btnEmail
            // 
            this.btnEmail.Location = new System.Drawing.Point(52, 188);
            this.btnEmail.Margin = new System.Windows.Forms.Padding(2);
            this.btnEmail.Name = "btnEmail";
            this.btnEmail.Size = new System.Drawing.Size(99, 26);
            this.btnEmail.TabIndex = 1;
            this.btnEmail.Text = "Send suggestions";
            this.btnEmail.UseVisualStyleBackColor = true;
            this.btnEmail.Click += new System.EventHandler(this.btnEmail_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(156, 188);
            this.btnClose.Margin = new System.Windows.Forms.Padding(2);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(99, 26);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // MM_Coordinate_Delta
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(366, 351);
            this.Controls.Add(this.splMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = Properties.Resources.CompanyIcon;
            this.Location = new System.Drawing.Point(0, 0);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "MM_Coordinate_Delta";
            this.Text = "Coordinate changer";
            this.splMain.Panel1.ResumeLayout(false);
            this.splMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splMain)).EndInit();
            this.splMain.ResumeLayout(false);
            this.grpCurrent.ResumeLayout(false);
            this.grpCurrent.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLineLngLat)).EndInit();
            this.grpChanges.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvChanges)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splMain;
        private System.Windows.Forms.GroupBox grpCurrent;
        private System.Windows.Forms.GroupBox grpChanges;
        private System.Windows.Forms.TextBox txtSubLatitude;
        private System.Windows.Forms.TextBox txtSubLongitude;
        private System.Windows.Forms.Label lblCoordinates;
        private System.Windows.Forms.Label lblElementValue;
        private System.Windows.Forms.Label lblElement;
        private System.Windows.Forms.DataGridView dgvLineLngLat;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.DataGridView dgvChanges;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnEmail;
    }
}