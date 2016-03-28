namespace MacomberMap.UI.DgvFilterPopup {
    partial class DgvCheckedListBoxColumn {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.checkedListBoxValue = new System.Windows.Forms.CheckedListBox();
            this.SuspendLayout();
            // 
            // checkedListBoxValue
            // 
            this.checkedListBoxValue.CheckOnClick = true;
            this.checkedListBoxValue.FormattingEnabled = true;
            this.checkedListBoxValue.Location = new System.Drawing.Point(3, 3);
            this.checkedListBoxValue.Name = "checkedListBoxValue";
            this.checkedListBoxValue.Size = new System.Drawing.Size(270, 94);
            this.checkedListBoxValue.TabIndex = 2;
            // 
            // DgvCheckedListBoxColumn
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.checkedListBoxValue);
            this.Name = "DgvCheckedListBoxColumn";
            this.Size = new System.Drawing.Size(276, 101);
            this.ResumeLayout(false);

        }



        #endregion

        private System.Windows.Forms.CheckedListBox checkedListBoxValue;
    }
}
