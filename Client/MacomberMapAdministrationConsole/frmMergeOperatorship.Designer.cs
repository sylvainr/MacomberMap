namespace MacomberMapAdministrationConsole
{
    partial class frmMergeOperatorship
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMergeOperatorship));
            this.lblDropPermission = new System.Windows.Forms.Label();
            this.lblDropArea = new System.Windows.Forms.Label();
            this.lblDropUserName = new System.Windows.Forms.Label();
            this.lblDropOldOperatorships = new System.Windows.Forms.Label();
            this.lvOutputs = new System.Windows.Forms.ListView();
            this.colUser = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colUserType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colNewUser = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colAdditions = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colRemovals = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnExportOperatorship = new System.Windows.Forms.Button();
            this.lblCountUserNames = new System.Windows.Forms.Label();
            this.lblCountAreas = new System.Windows.Forms.Label();
            this.splTop = new System.Windows.Forms.SplitContainer();
            this.lblCountUserTypeToModeLinkages = new System.Windows.Forms.Label();
            this.lblDropUserTypeToModeLinkages = new System.Windows.Forms.Label();
            this.lblCountOldOperatorships = new System.Windows.Forms.Label();
            this.lblCountPermissions = new System.Windows.Forms.Label();
            this.lblCountModes = new System.Windows.Forms.Label();
            this.lblDropMode = new System.Windows.Forms.Label();
            this.lblCountUserType = new System.Windows.Forms.Label();
            this.lblDropUserTypeFile = new System.Windows.Forms.Label();
            this.splBottom = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.splTop)).BeginInit();
            this.splTop.Panel1.SuspendLayout();
            this.splTop.Panel2.SuspendLayout();
            this.splTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splBottom)).BeginInit();
            this.splBottom.Panel1.SuspendLayout();
            this.splBottom.Panel2.SuspendLayout();
            this.splBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblDropPermission
            // 
            this.lblDropPermission.AllowDrop = true;
            this.lblDropPermission.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblDropPermission.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDropPermission.Location = new System.Drawing.Point(652, 9);
            this.lblDropPermission.Name = "lblDropPermission";
            this.lblDropPermission.Size = new System.Drawing.Size(108, 61);
            this.lblDropPermission.TabIndex = 0;
            this.lblDropPermission.Text = "Drop EXEC PERMISSION file here";
            this.lblDropPermission.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblDropPermission.DragDrop += new System.Windows.Forms.DragEventHandler(this.lbl_DragDrop);
            this.lblDropPermission.DragOver += new System.Windows.Forms.DragEventHandler(this.lbl_DragOver);
            // 
            // lblDropArea
            // 
            this.lblDropArea.AllowDrop = true;
            this.lblDropArea.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblDropArea.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDropArea.Location = new System.Drawing.Point(527, 9);
            this.lblDropArea.Name = "lblDropArea";
            this.lblDropArea.Size = new System.Drawing.Size(108, 61);
            this.lblDropArea.TabIndex = 1;
            this.lblDropArea.Text = "Drop AREA file here";
            this.lblDropArea.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblDropArea.DragDrop += new System.Windows.Forms.DragEventHandler(this.lbl_DragDrop);
            this.lblDropArea.DragOver += new System.Windows.Forms.DragEventHandler(this.lbl_DragOver);
            // 
            // lblDropUserName
            // 
            this.lblDropUserName.AllowDrop = true;
            this.lblDropUserName.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblDropUserName.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDropUserName.Location = new System.Drawing.Point(12, 9);
            this.lblDropUserName.Name = "lblDropUserName";
            this.lblDropUserName.Size = new System.Drawing.Size(108, 61);
            this.lblDropUserName.TabIndex = 2;
            this.lblDropUserName.Text = "Drop USERNAME file here";
            this.lblDropUserName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblDropUserName.DragDrop += new System.Windows.Forms.DragEventHandler(this.lbl_DragDrop);
            this.lblDropUserName.DragOver += new System.Windows.Forms.DragEventHandler(this.lbl_DragOver);
            // 
            // lblDropOldOperatorships
            // 
            this.lblDropOldOperatorships.AllowDrop = true;
            this.lblDropOldOperatorships.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblDropOldOperatorships.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDropOldOperatorships.ForeColor = System.Drawing.Color.Gray;
            this.lblDropOldOperatorships.Location = new System.Drawing.Point(778, 9);
            this.lblDropOldOperatorships.Name = "lblDropOldOperatorships";
            this.lblDropOldOperatorships.Size = new System.Drawing.Size(108, 61);
            this.lblDropOldOperatorships.TabIndex = 3;
            this.lblDropOldOperatorships.Text = "(Optional) Drop current operatorships here";
            this.lblDropOldOperatorships.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblDropOldOperatorships.DragDrop += new System.Windows.Forms.DragEventHandler(this.lbl_DragDrop);
            this.lblDropOldOperatorships.DragOver += new System.Windows.Forms.DragEventHandler(this.lbl_DragOver);
            // 
            // lvOutputs
            // 
            this.lvOutputs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colUser,
            this.colUserType,
            this.colNewUser,
            this.colAdditions,
            this.colRemovals});
            this.lvOutputs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvOutputs.Location = new System.Drawing.Point(0, 0);
            this.lvOutputs.Name = "lvOutputs";
            this.lvOutputs.Size = new System.Drawing.Size(908, 162);
            this.lvOutputs.TabIndex = 4;
            this.lvOutputs.UseCompatibleStateImageBehavior = false;
            this.lvOutputs.View = System.Windows.Forms.View.Details;
            // 
            // colUser
            // 
            this.colUser.Text = "User";
            // 
            // colUserType
            // 
            this.colUserType.Text = "User Type";
            // 
            // colNewUser
            // 
            this.colNewUser.Text = "New?";
            // 
            // colAdditions
            // 
            this.colAdditions.Text = "Additions";
            // 
            // colRemovals
            // 
            this.colRemovals.Text = "Removals";
            // 
            // btnExportOperatorship
            // 
            this.btnExportOperatorship.AutoSize = true;
            this.btnExportOperatorship.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnExportOperatorship.Image = global::MacomberMapAdministrationConsole.Properties.Resources.SaveAllHS;
            this.btnExportOperatorship.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnExportOperatorship.Location = new System.Drawing.Point(0, 0);
            this.btnExportOperatorship.Name = "btnExportOperatorship";
            this.btnExportOperatorship.Size = new System.Drawing.Size(908, 34);
            this.btnExportOperatorship.TabIndex = 5;
            this.btnExportOperatorship.Text = "Export operatorship file";
            this.btnExportOperatorship.UseVisualStyleBackColor = true;
            this.btnExportOperatorship.Click += new System.EventHandler(this.btnExportOperatorship_Click);
            // 
            // lblCountUserNames
            // 
            this.lblCountUserNames.Location = new System.Drawing.Point(9, 70);
            this.lblCountUserNames.Name = "lblCountUserNames";
            this.lblCountUserNames.Size = new System.Drawing.Size(108, 19);
            this.lblCountUserNames.TabIndex = 8;
            this.lblCountUserNames.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblCountAreas
            // 
            this.lblCountAreas.Location = new System.Drawing.Point(527, 73);
            this.lblCountAreas.Name = "lblCountAreas";
            this.lblCountAreas.Size = new System.Drawing.Size(108, 19);
            this.lblCountAreas.TabIndex = 9;
            this.lblCountAreas.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // splTop
            // 
            this.splTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splTop.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splTop.Location = new System.Drawing.Point(0, 0);
            this.splTop.Name = "splTop";
            this.splTop.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splTop.Panel1
            // 
            this.splTop.Panel1.Controls.Add(this.lblCountUserTypeToModeLinkages);
            this.splTop.Panel1.Controls.Add(this.lblDropUserTypeToModeLinkages);
            this.splTop.Panel1.Controls.Add(this.lblCountOldOperatorships);
            this.splTop.Panel1.Controls.Add(this.lblCountPermissions);
            this.splTop.Panel1.Controls.Add(this.lblCountModes);
            this.splTop.Panel1.Controls.Add(this.lblDropMode);
            this.splTop.Panel1.Controls.Add(this.lblDropArea);
            this.splTop.Panel1.Controls.Add(this.lblDropPermission);
            this.splTop.Panel1.Controls.Add(this.lblDropOldOperatorships);
            this.splTop.Panel1.Controls.Add(this.lblCountUserType);
            this.splTop.Panel1.Controls.Add(this.lblDropUserTypeFile);
            this.splTop.Panel1.Controls.Add(this.lblCountAreas);
            this.splTop.Panel1.Controls.Add(this.lblCountUserNames);
            this.splTop.Panel1.Controls.Add(this.lblDropUserName);
            // 
            // splTop.Panel2
            // 
            this.splTop.Panel2.Controls.Add(this.splBottom);
            this.splTop.Size = new System.Drawing.Size(908, 303);
            this.splTop.SplitterDistance = 99;
            this.splTop.TabIndex = 10;
            // 
            // lblCountUserTypeToModeLinkages
            // 
            this.lblCountUserTypeToModeLinkages.Location = new System.Drawing.Point(393, 73);
            this.lblCountUserTypeToModeLinkages.Name = "lblCountUserTypeToModeLinkages";
            this.lblCountUserTypeToModeLinkages.Size = new System.Drawing.Size(108, 19);
            this.lblCountUserTypeToModeLinkages.TabIndex = 17;
            this.lblCountUserTypeToModeLinkages.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblDropUserTypeToModeLinkages
            // 
            this.lblDropUserTypeToModeLinkages.AllowDrop = true;
            this.lblDropUserTypeToModeLinkages.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblDropUserTypeToModeLinkages.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDropUserTypeToModeLinkages.Location = new System.Drawing.Point(393, 9);
            this.lblDropUserTypeToModeLinkages.Name = "lblDropUserTypeToModeLinkages";
            this.lblDropUserTypeToModeLinkages.Size = new System.Drawing.Size(108, 61);
            this.lblDropUserTypeToModeLinkages.TabIndex = 16;
            this.lblDropUserTypeToModeLinkages.Text = "Drop USERTYP to MODE linkages here";
            this.lblDropUserTypeToModeLinkages.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblDropUserTypeToModeLinkages.DragDrop += new System.Windows.Forms.DragEventHandler(this.lbl_DragDrop);
            this.lblDropUserTypeToModeLinkages.DragOver += new System.Windows.Forms.DragEventHandler(this.lbl_DragOver);
            // 
            // lblCountOldOperatorships
            // 
            this.lblCountOldOperatorships.Location = new System.Drawing.Point(778, 73);
            this.lblCountOldOperatorships.Name = "lblCountOldOperatorships";
            this.lblCountOldOperatorships.Size = new System.Drawing.Size(108, 19);
            this.lblCountOldOperatorships.TabIndex = 15;
            this.lblCountOldOperatorships.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblCountPermissions
            // 
            this.lblCountPermissions.Location = new System.Drawing.Point(652, 73);
            this.lblCountPermissions.Name = "lblCountPermissions";
            this.lblCountPermissions.Size = new System.Drawing.Size(108, 19);
            this.lblCountPermissions.TabIndex = 14;
            this.lblCountPermissions.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblCountModes
            // 
            this.lblCountModes.Location = new System.Drawing.Point(262, 73);
            this.lblCountModes.Name = "lblCountModes";
            this.lblCountModes.Size = new System.Drawing.Size(108, 19);
            this.lblCountModes.TabIndex = 13;
            this.lblCountModes.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblDropMode
            // 
            this.lblDropMode.AllowDrop = true;
            this.lblDropMode.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblDropMode.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDropMode.Location = new System.Drawing.Point(262, 9);
            this.lblDropMode.Name = "lblDropMode";
            this.lblDropMode.Size = new System.Drawing.Size(108, 61);
            this.lblDropMode.TabIndex = 12;
            this.lblDropMode.Text = "Drop MODE file here";
            this.lblDropMode.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblDropMode.DragDrop += new System.Windows.Forms.DragEventHandler(this.lbl_DragDrop);
            this.lblDropMode.DragOver += new System.Windows.Forms.DragEventHandler(this.lbl_DragOver);
            // 
            // lblCountUserType
            // 
            this.lblCountUserType.Location = new System.Drawing.Point(135, 70);
            this.lblCountUserType.Name = "lblCountUserType";
            this.lblCountUserType.Size = new System.Drawing.Size(108, 19);
            this.lblCountUserType.TabIndex = 11;
            this.lblCountUserType.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblDropUserTypeFile
            // 
            this.lblDropUserTypeFile.AllowDrop = true;
            this.lblDropUserTypeFile.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblDropUserTypeFile.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDropUserTypeFile.Location = new System.Drawing.Point(137, 9);
            this.lblDropUserTypeFile.Name = "lblDropUserTypeFile";
            this.lblDropUserTypeFile.Size = new System.Drawing.Size(108, 61);
            this.lblDropUserTypeFile.TabIndex = 10;
            this.lblDropUserTypeFile.Text = "Drop USERTYPE file here";
            this.lblDropUserTypeFile.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblDropUserTypeFile.DragDrop += new System.Windows.Forms.DragEventHandler(this.lbl_DragDrop);
            this.lblDropUserTypeFile.DragOver += new System.Windows.Forms.DragEventHandler(this.lbl_DragOver);
            // 
            // splBottom
            // 
            this.splBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splBottom.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splBottom.Location = new System.Drawing.Point(0, 0);
            this.splBottom.Name = "splBottom";
            this.splBottom.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splBottom.Panel1
            // 
            this.splBottom.Panel1.Controls.Add(this.lvOutputs);
            // 
            // splBottom.Panel2
            // 
            this.splBottom.Panel2.Controls.Add(this.btnExportOperatorship);
            this.splBottom.Size = new System.Drawing.Size(908, 200);
            this.splBottom.SplitterDistance = 162;
            this.splBottom.TabIndex = 11;
            // 
            // frmMergeOperatorship
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(908, 303);
            this.Controls.Add(this.splTop);
            this.Icon = Properties.Resources.CompanyIcon;
            this.Name = "frmMergeOperatorship";
            this.Text = "Merge Operatorships";
            this.splTop.Panel1.ResumeLayout(false);
            this.splTop.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splTop)).EndInit();
            this.splTop.ResumeLayout(false);
            this.splBottom.Panel1.ResumeLayout(false);
            this.splBottom.Panel2.ResumeLayout(false);
            this.splBottom.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splBottom)).EndInit();
            this.splBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblDropPermission;
        private System.Windows.Forms.Label lblDropArea;
        private System.Windows.Forms.Label lblDropUserName;
        private System.Windows.Forms.Label lblDropOldOperatorships;
        private System.Windows.Forms.ListView lvOutputs;
        private System.Windows.Forms.ColumnHeader colUser;
        private System.Windows.Forms.ColumnHeader colNewUser;
        private System.Windows.Forms.ColumnHeader colAdditions;
        private System.Windows.Forms.ColumnHeader colRemovals;
        private System.Windows.Forms.Button btnExportOperatorship;
        private System.Windows.Forms.Label lblCountUserNames;
        private System.Windows.Forms.Label lblCountAreas;
        private System.Windows.Forms.SplitContainer splTop;
        private System.Windows.Forms.SplitContainer splBottom;
        private System.Windows.Forms.Label lblCountOldOperatorships;
        private System.Windows.Forms.Label lblCountPermissions;
        private System.Windows.Forms.Label lblCountModes;
        private System.Windows.Forms.Label lblDropMode;
        private System.Windows.Forms.Label lblCountUserType;
        private System.Windows.Forms.Label lblDropUserTypeFile;
        private System.Windows.Forms.Label lblCountUserTypeToModeLinkages;
        private System.Windows.Forms.Label lblDropUserTypeToModeLinkages;
        private System.Windows.Forms.ColumnHeader colUserType;
    }
}