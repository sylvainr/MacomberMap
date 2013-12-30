namespace Macomber_Map.User_Interface_Components
{
    partial class MM_Model_Selector
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
            this.calModel = new System.Windows.Forms.MonthCalendar();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.cmbModel = new System.Windows.Forms.ComboBox();
            this.lblModel = new System.Windows.Forms.Label();
            this.cmbModelCategory = new System.Windows.Forms.ComboBox();
            this.lblModelCategory = new System.Windows.Forms.Label();
            this.cmbModelClass = new System.Windows.Forms.ComboBox();
            this.lblClass = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // calModel
            // 
            this.calModel.Location = new System.Drawing.Point(9, 12);
            this.calModel.MaxSelectionCount = 1;
            this.calModel.Name = "calModel";
            this.calModel.ShowWeekNumbers = true;
            this.calModel.TabIndex = 19;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(460, 145);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 18;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(314, 145);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 17;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // cmbModel
            // 
            this.cmbModel.FormattingEnabled = true;
            this.cmbModel.Location = new System.Drawing.Point(354, 106);
            this.cmbModel.Name = "cmbModel";
            this.cmbModel.Size = new System.Drawing.Size(181, 21);
            this.cmbModel.TabIndex = 16;
            this.cmbModel.SelectedIndexChanged += new System.EventHandler(this.cmbModel_SelectedIndexChanged);
            // 
            // lblModel
            // 
            this.lblModel.AutoSize = true;
            this.lblModel.Location = new System.Drawing.Point(270, 109);
            this.lblModel.Name = "lblModel";
            this.lblModel.Size = new System.Drawing.Size(39, 13);
            this.lblModel.TabIndex = 15;
            this.lblModel.Text = "Model:";
            // 
            // cmbModelCategory
            // 
            this.cmbModelCategory.FormattingEnabled = true;
            this.cmbModelCategory.Location = new System.Drawing.Point(354, 28);
            this.cmbModelCategory.Name = "cmbModelCategory";
            this.cmbModelCategory.Size = new System.Drawing.Size(181, 21);
            this.cmbModelCategory.TabIndex = 14;
            this.cmbModelCategory.SelectedIndexChanged += new System.EventHandler(this.cmbModelCategory_SelectedIndexChanged);
            // 
            // lblModelCategory
            // 
            this.lblModelCategory.AutoSize = true;
            this.lblModelCategory.Location = new System.Drawing.Point(270, 31);
            this.lblModelCategory.Name = "lblModelCategory";
            this.lblModelCategory.Size = new System.Drawing.Size(84, 13);
            this.lblModelCategory.TabIndex = 13;
            this.lblModelCategory.Text = "Model Category:";
            // 
            // cmbModelClass
            // 
            this.cmbModelClass.FormattingEnabled = true;
            this.cmbModelClass.Location = new System.Drawing.Point(354, 66);
            this.cmbModelClass.Name = "cmbModelClass";
            this.cmbModelClass.Size = new System.Drawing.Size(183, 21);
            this.cmbModelClass.TabIndex = 12;
            this.cmbModelClass.SelectedIndexChanged += new System.EventHandler(this.cmbModelClass_SelectedIndexChanged);
            // 
            // lblClass
            // 
            this.lblClass.AutoSize = true;
            this.lblClass.Location = new System.Drawing.Point(270, 69);
            this.lblClass.Name = "lblClass";
            this.lblClass.Size = new System.Drawing.Size(67, 13);
            this.lblClass.TabIndex = 11;
            this.lblClass.Text = "Model Class:";
            // 
            // MM_Model_Selector
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(551, 192);
            this.Controls.Add(this.calModel);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.cmbModel);
            this.Controls.Add(this.lblModel);
            this.Controls.Add(this.cmbModelCategory);
            this.Controls.Add(this.lblModelCategory);
            this.Controls.Add(this.cmbModelClass);
            this.Controls.Add(this.lblClass);
            this.Name = "MM_Model_Selector";
            this.Text = "Model Selection - Macomber Map®";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MonthCalendar calModel;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.ComboBox cmbModel;
        private System.Windows.Forms.Label lblModel;
        private System.Windows.Forms.ComboBox cmbModelCategory;
        private System.Windows.Forms.Label lblModelCategory;
        private System.Windows.Forms.ComboBox cmbModelClass;
        private System.Windows.Forms.Label lblClass;
    }
}