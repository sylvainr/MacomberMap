
using Macomber_Map.Data_Connections;
namespace Macomber_Map.User_Interface_Components
{
    partial class Violation_Viewer
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

            //Disconnect the integration viewer
            Data_Integration.ViolationAdded -= _ViolationAdded;
            Data_Integration.ViolationModified -= _ViolationModified;
            Data_Integration.ViolationRemoved -= _ViolationRemoved;
            Data_Integration.ViolationAcknowledged -= _ViolationAcknowledged;
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
            this.lvViolations = new System.Windows.Forms.ListView();
            this.tsSummary = new System.Windows.Forms.ToolStrip();
            this.tmrUpdate = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // lvViolations
            // 
            this.lvViolations.BackColor = System.Drawing.Color.Black;
            this.lvViolations.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvViolations.ForeColor = System.Drawing.Color.White;
            this.lvViolations.Location = new System.Drawing.Point(0, 0);
            this.lvViolations.Name = "lvViolations";
            this.lvViolations.ShowItemToolTips = true;
            this.lvViolations.Size = new System.Drawing.Size(146, 146);
            this.lvViolations.TabIndex = 0;
            this.lvViolations.TileSize = new System.Drawing.Size(168, 20);
            this.lvViolations.UseCompatibleStateImageBehavior = false;
            this.lvViolations.View = System.Windows.Forms.View.Details;
            this.lvViolations.VirtualMode = true;
            this.lvViolations.SelectedIndexChanged += new System.EventHandler(this.HandleNewSelection);
            this.lvViolations.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvViolations_ColumnClick);
            this.lvViolations.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.lvViolations_RetrieveVirtualItem);
            // 
            // tsSummary
            // 
            this.tsSummary.BackColor = System.Drawing.Color.Black;
            this.tsSummary.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsSummary.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.tsSummary.Location = new System.Drawing.Point(0, 0);
            this.tsSummary.Name = "tsSummary";
            this.tsSummary.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.tsSummary.Size = new System.Drawing.Size(146, 0);
            this.tsSummary.TabIndex = 1;
            this.tsSummary.MouseClick += new System.Windows.Forms.MouseEventHandler(this.tsSummary_MouseClick);
            // 
            // tmrUpdate
            // 
            this.tmrUpdate.Enabled = true;
            this.tmrUpdate.Interval = 1000;
            this.tmrUpdate.Tick += new System.EventHandler(this.tmrUpdate_Tick);
            // 
            // Violation_Viewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(this.lvViolations);
            this.Controls.Add(this.tsSummary);
            this.Name = "Violation_Viewer";
            this.Size = new System.Drawing.Size(146, 146);
            this.Resize += new System.EventHandler(this.ViolationViewer_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        /// <summary>Our list of violations</summary>
        public System.Windows.Forms.ListView lvViolations;
        private System.Windows.Forms.ToolStrip tsSummary;
        private System.Windows.Forms.Timer tmrUpdate;
    }
}
