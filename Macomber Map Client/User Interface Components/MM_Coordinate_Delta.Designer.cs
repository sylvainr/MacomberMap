namespace Macomber_Map.User_Interface_Components
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MM_Coordinate_Delta));
            this.splMain = new System.Windows.Forms.SplitContainer();
            this.grpCurrent = new System.Windows.Forms.GroupBox();
            this.txtSubLatitude = new System.Windows.Forms.TextBox();
            this.txtSubLongitude = new System.Windows.Forms.TextBox();
            this.lblCoordinates = new System.Windows.Forms.Label();
            this.lblElementValue = new System.Windows.Forms.Label();
            this.lblElement = new System.Windows.Forms.Label();
            this.dgvLineLatLong = new System.Windows.Forms.DataGridView();
            this.grpChanges = new System.Windows.Forms.GroupBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnEmail = new System.Windows.Forms.Button();
            this.dgvChanges = new System.Windows.Forms.DataGridView();
            this.splMain.Panel1.SuspendLayout();
            this.splMain.Panel2.SuspendLayout();
            this.splMain.SuspendLayout();
            this.grpCurrent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLineLatLong)).BeginInit();
            this.grpChanges.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvChanges)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splMain.Location = new System.Drawing.Point(0, 0);
            this.splMain.Name = "splitContainer1";
            this.splMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splMain.Panel1.Controls.Add(this.grpCurrent);
            // 
            // splitContainer1.Panel2
            // 
            this.splMain.Panel2.Controls.Add(this.grpChanges);
            this.splMain.Size = new System.Drawing.Size(488, 432);
            this.splMain.SplitterDistance = 162;
            this.splMain.TabIndex = 0;
            // 
            // grpCurrent
            // 
            this.grpCurrent.Controls.Add(this.txtSubLatitude);
            this.grpCurrent.Controls.Add(this.txtSubLongitude);
            this.grpCurrent.Controls.Add(this.lblCoordinates);
            this.grpCurrent.Controls.Add(this.lblElementValue);
            this.grpCurrent.Controls.Add(this.lblElement);
            this.grpCurrent.Controls.Add(this.dgvLineLatLong);
            this.grpCurrent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpCurrent.Location = new System.Drawing.Point(0, 0);
            this.grpCurrent.Name = "grpCurrent";
            this.grpCurrent.Size = new System.Drawing.Size(488, 162);
            this.grpCurrent.TabIndex = 0;
            this.grpCurrent.TabStop = false;
            this.grpCurrent.Text = "Current Item";
            // 
            // txtSubLatitude
            // 
            this.txtSubLatitude.Location = new System.Drawing.Point(233, 57);
            this.txtSubLatitude.Name = "txtSubLatitude";
            this.txtSubLatitude.Size = new System.Drawing.Size(100, 22);
            this.txtSubLatitude.TabIndex = 4;
            this.txtSubLatitude.Visible = false;
            // 
            // txtSubLongitude
            // 
            this.txtSubLongitude.Location = new System.Drawing.Point(116, 57);
            this.txtSubLongitude.Name = "txtSubLongitude";
            this.txtSubLongitude.Size = new System.Drawing.Size(100, 22);
            this.txtSubLongitude.TabIndex = 3;
            this.txtSubLongitude.Visible = false;
            // 
            // lblCoordinates
            // 
            this.lblCoordinates.AutoSize = true;
            this.lblCoordinates.Location = new System.Drawing.Point(12, 60);
            this.lblCoordinates.Name = "lblCoordinates";
            this.lblCoordinates.Size = new System.Drawing.Size(98, 17);
            this.lblCoordinates.TabIndex = 2;
            this.lblCoordinates.Text = "Coordinate(s):";
            // 
            // lblElementValue
            // 
            this.lblElementValue.AutoSize = true;
            this.lblElementValue.Location = new System.Drawing.Point(81, 32);
            this.lblElementValue.Name = "lblElementValue";
            this.lblElementValue.Size = new System.Drawing.Size(0, 17);
            this.lblElementValue.TabIndex = 1;
            // 
            // lblElement
            // 
            this.lblElement.AutoSize = true;
            this.lblElement.Location = new System.Drawing.Point(12, 32);
            this.lblElement.Name = "lblElement";
            this.lblElement.Size = new System.Drawing.Size(63, 17);
            this.lblElement.TabIndex = 0;
            this.lblElement.Text = "Element:";
            // 
            // dgvLineLatLong
            // 
            this.dgvLineLatLong.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvLineLatLong.Location = new System.Drawing.Point(116, 57);
            this.dgvLineLatLong.Name = "dgvLineLatLong";
            this.dgvLineLatLong.RowTemplate.Height = 24;
            this.dgvLineLatLong.Size = new System.Drawing.Size(360, 99);
            this.dgvLineLatLong.TabIndex = 5;
            this.dgvLineLatLong.Visible = false;
            // 
            // grpChanges
            // 
            this.grpChanges.Controls.Add(this.btnCancel);
            this.grpChanges.Controls.Add(this.btnClose);
            this.grpChanges.Controls.Add(this.btnEmail);
            this.grpChanges.Controls.Add(this.dgvChanges);
            this.grpChanges.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpChanges.Location = new System.Drawing.Point(0, 0);
            this.grpChanges.Name = "grpChanges";
            this.grpChanges.Size = new System.Drawing.Size(488, 266);
            this.grpChanges.TabIndex = 0;
            this.grpChanges.TabStop = false;
            this.grpChanges.Text = "Changes";
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(344, 231);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(132, 32);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(208, 231);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(132, 32);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnEmail
            // 
            this.btnEmail.Location = new System.Drawing.Point(70, 231);
            this.btnEmail.Name = "btnEmail";
            this.btnEmail.Size = new System.Drawing.Size(132, 32);
            this.btnEmail.TabIndex = 1;
            this.btnEmail.Text = "Email suggestions";
            this.btnEmail.UseVisualStyleBackColor = true;
            this.btnEmail.Click += new System.EventHandler(this.btnEmail_Click);
            // 
            // dgvChanges
            // 
            this.dgvChanges.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvChanges.Location = new System.Drawing.Point(15, 21);
            this.dgvChanges.Name = "dgvChanges";
            this.dgvChanges.RowTemplate.Height = 24;
            this.dgvChanges.Size = new System.Drawing.Size(461, 200);
            this.dgvChanges.TabIndex = 0;
            // 
            // MM_Coordinate_Delta
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(488, 432);
            this.Controls.Add(this.splMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "MM_Coordinate_Delta";
            this.Text = "Coordinate changer";
            this.splMain.Panel1.ResumeLayout(false);
            this.splMain.Panel2.ResumeLayout(false);
            this.splMain.ResumeLayout(false);
            this.grpCurrent.ResumeLayout(false);
            this.grpCurrent.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLineLatLong)).EndInit();
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
        private System.Windows.Forms.DataGridView dgvLineLatLong;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnEmail;
        private System.Windows.Forms.DataGridView dgvChanges;
    }
}