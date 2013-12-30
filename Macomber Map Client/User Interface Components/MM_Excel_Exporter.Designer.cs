namespace Macomber_Map.User_Interface_Components
{
    partial class MM_Excel_Exporter
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MM_Excel_Exporter));
            this.pbToExcel = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // pbToExcel
            // 
            this.pbToExcel.Location = new System.Drawing.Point(12, 12);
            this.pbToExcel.Name = "pbToExcel";
            this.pbToExcel.Size = new System.Drawing.Size(268, 23);
            this.pbToExcel.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.pbToExcel.TabIndex = 0;
            // 
            // MM_Excel_Exporter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 48);
            this.Controls.Add(this.pbToExcel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "MM_Excel_Exporter";
            this.Text = "Exporting to Excel...";
            this.ResumeLayout(false);

        }

        #endregion

        /// <summary>
        /// The progress bar showing Excel loading
        /// </summary>
        public System.Windows.Forms.ProgressBar pbToExcel;

    }
}