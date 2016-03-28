namespace MacomberMapClient.User_Interfaces.Blackstart
{
    partial class MM_LoadGen_Tracking_Operator
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lblOperator = new System.Windows.Forms.Label();
            this.lblBase = new System.Windows.Forms.Label();
            this.lblDelta = new System.Windows.Forms.Label();
            this.zgGraph = new ZedGraph.ZedGraphControl();
            this.lblBaseValue = new System.Windows.Forms.Label();
            this.lblDeltaValue = new System.Windows.Forms.Label();
            this.splMain = new System.Windows.Forms.SplitContainer();
            this.tmrUpdate = new System.Windows.Forms.Timer(this.components);
            this.btnReset = new System.Windows.Forms.Button();
            this.nudMax = new System.Windows.Forms.NumericUpDown();
            this.nudMin = new System.Windows.Forms.NumericUpDown();
            this.chkMax = new System.Windows.Forms.CheckBox();
            this.chkMin = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.splMain)).BeginInit();
            this.splMain.Panel1.SuspendLayout();
            this.splMain.Panel2.SuspendLayout();
            this.splMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMax)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMin)).BeginInit();
            this.SuspendLayout();
            // 
            // lblOperator
            // 
            this.lblOperator.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOperator.Location = new System.Drawing.Point(3, 0);
            this.lblOperator.Name = "lblOperator";
            this.lblOperator.Size = new System.Drawing.Size(145, 43);
            this.lblOperator.TabIndex = 0;
            this.lblOperator.Text = "Operator";
            this.lblOperator.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.lblOperator.DoubleClick += new System.EventHandler(this.MM_LoadGen_Tracking_Operator_MouseDoubleClick);
            this.lblOperator.MouseClick += new System.Windows.Forms.MouseEventHandler(this.MM_LoadGen_Tracking_Operator_MouseClick);
            // 
            // lblBase
            // 
            this.lblBase.Location = new System.Drawing.Point(2, 43);
            this.lblBase.Name = "lblBase";
            this.lblBase.Size = new System.Drawing.Size(39, 23);
            this.lblBase.TabIndex = 1;
            this.lblBase.Text = "Base";
            this.lblBase.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblBase.DoubleClick += new System.EventHandler(this.MM_LoadGen_Tracking_Operator_MouseDoubleClick);
            this.lblBase.MouseClick += new System.Windows.Forms.MouseEventHandler(this.MM_LoadGen_Tracking_Operator_MouseClick);
            // 
            // lblDelta
            // 
            this.lblDelta.Location = new System.Drawing.Point(2, 77);
            this.lblDelta.Name = "lblDelta";
            this.lblDelta.Size = new System.Drawing.Size(39, 23);
            this.lblDelta.TabIndex = 2;
            this.lblDelta.Text = "Delta";
            this.lblDelta.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblDelta.DoubleClick += new System.EventHandler(this.MM_LoadGen_Tracking_Operator_MouseDoubleClick);
            this.lblDelta.MouseClick += new System.Windows.Forms.MouseEventHandler(this.MM_LoadGen_Tracking_Operator_MouseClick);
            // 
            // zgGraph
            // 
            this.zgGraph.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zgGraph.IsShowPointValues = true;
            this.zgGraph.Location = new System.Drawing.Point(0, 0);
            this.zgGraph.Name = "zgGraph";
            this.zgGraph.ScrollGrace = 0D;
            this.zgGraph.ScrollMaxX = 0D;
            this.zgGraph.ScrollMaxY = 0D;
            this.zgGraph.ScrollMaxY2 = 0D;
            this.zgGraph.ScrollMinX = 0D;
            this.zgGraph.ScrollMinY = 0D;
            this.zgGraph.ScrollMinY2 = 0D;
            this.zgGraph.Size = new System.Drawing.Size(320, 106);
            this.zgGraph.TabIndex = 3;
            // 
            // lblBaseValue
            // 
            this.lblBaseValue.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBaseValue.Location = new System.Drawing.Point(38, 43);
            this.lblBaseValue.Name = "lblBaseValue";
            this.lblBaseValue.Size = new System.Drawing.Size(109, 23);
            this.lblBaseValue.TabIndex = 4;
            this.lblBaseValue.Text = "Base";
            this.lblBaseValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblBaseValue.DoubleClick += new System.EventHandler(this.MM_LoadGen_Tracking_Operator_MouseDoubleClick);
            this.lblBaseValue.MouseClick += new System.Windows.Forms.MouseEventHandler(this.MM_LoadGen_Tracking_Operator_MouseClick);
            // 
            // lblDeltaValue
            // 
            this.lblDeltaValue.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDeltaValue.Location = new System.Drawing.Point(38, 76);
            this.lblDeltaValue.Name = "lblDeltaValue";
            this.lblDeltaValue.Size = new System.Drawing.Size(109, 23);
            this.lblDeltaValue.TabIndex = 5;
            this.lblDeltaValue.Text = "Delta";
            this.lblDeltaValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblDeltaValue.DoubleClick += new System.EventHandler(this.MM_LoadGen_Tracking_Operator_MouseDoubleClick);
            this.lblDeltaValue.MouseClick += new System.Windows.Forms.MouseEventHandler(this.MM_LoadGen_Tracking_Operator_MouseClick);
            // 
            // splMain
            // 
            this.splMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splMain.IsSplitterFixed = true;
            this.splMain.Location = new System.Drawing.Point(0, 0);
            this.splMain.Name = "splMain";
            // 
            // splMain.Panel1
            // 
            this.splMain.Panel1.Controls.Add(this.chkMin);
            this.splMain.Panel1.Controls.Add(this.chkMax);
            this.splMain.Panel1.Controls.Add(this.nudMin);
            this.splMain.Panel1.Controls.Add(this.nudMax);
            this.splMain.Panel1.Controls.Add(this.btnReset);
            this.splMain.Panel1.Controls.Add(this.lblOperator);
            this.splMain.Panel1.Controls.Add(this.lblDeltaValue);
            this.splMain.Panel1.Controls.Add(this.lblBase);
            this.splMain.Panel1.Controls.Add(this.lblBaseValue);
            this.splMain.Panel1.Controls.Add(this.lblDelta);
            this.splMain.Panel1.DoubleClick += new System.EventHandler(this.MM_LoadGen_Tracking_Operator_MouseDoubleClick);
            this.splMain.Panel1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.MM_LoadGen_Tracking_Operator_MouseClick);
            // 
            // splMain.Panel2
            // 
            this.splMain.Panel2.Controls.Add(this.zgGraph);
            this.splMain.Size = new System.Drawing.Size(474, 106);
            this.splMain.SplitterDistance = 150;
            this.splMain.TabIndex = 6;
            // 
            // tmrUpdate
            // 
            this.tmrUpdate.Enabled = true;
            this.tmrUpdate.Interval = 1000;
            this.tmrUpdate.Tick += new System.EventHandler(this.tmrUpdate_Tick);
            // 
            // btnReset
            // 
            this.btnReset.ForeColor = System.Drawing.Color.Black;
            this.btnReset.Location = new System.Drawing.Point(34, 113);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(75, 23);
            this.btnReset.TabIndex = 6;
            this.btnReset.Text = "&Reset";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // nudMax
            // 
            this.nudMax.Location = new System.Drawing.Point(61, 157);
            this.nudMax.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.nudMax.Minimum = new decimal(new int[] {
            1000000000,
            0,
            0,
            -2147483648});
            this.nudMax.Name = "nudMax";
            this.nudMax.Size = new System.Drawing.Size(83, 20);
            this.nudMax.TabIndex = 9;
            this.nudMax.ValueChanged += new System.EventHandler(this.UpdateAxis);
            // 
            // nudMin
            // 
            this.nudMin.Location = new System.Drawing.Point(61, 192);
            this.nudMin.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.nudMin.Minimum = new decimal(new int[] {
            1000000000,
            0,
            0,
            -2147483648});
            this.nudMin.Name = "nudMin";
            this.nudMin.Size = new System.Drawing.Size(83, 20);
            this.nudMin.TabIndex = 10;
            this.nudMin.ValueChanged += new System.EventHandler(this.UpdateAxis);
            // 
            // chkMax
            // 
            this.chkMax.AutoSize = true;
            this.chkMax.Location = new System.Drawing.Point(6, 158);
            this.chkMax.Name = "chkMax";
            this.chkMax.Size = new System.Drawing.Size(49, 17);
            this.chkMax.TabIndex = 11;
            this.chkMax.Text = "Max:";
            this.chkMax.UseVisualStyleBackColor = true;
            this.chkMax.CheckedChanged += new System.EventHandler(this.UpdateAxis);
            // 
            // chkMin
            // 
            this.chkMin.AutoSize = true;
            this.chkMin.Location = new System.Drawing.Point(6, 193);
            this.chkMin.Name = "chkMin";
            this.chkMin.Size = new System.Drawing.Size(46, 17);
            this.chkMin.TabIndex = 12;
            this.chkMin.Text = "Min:";
            this.chkMin.UseVisualStyleBackColor = true;
            this.chkMin.CheckedChanged += new System.EventHandler(this.UpdateAxis);
            // 
            // MM_LoadGen_Tracking_Operator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Controls.Add(this.splMain);
            this.ForeColor = System.Drawing.Color.White;
            this.Name = "MM_LoadGen_Tracking_Operator";
            this.Size = new System.Drawing.Size(474, 106);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.MM_LoadGen_Tracking_Operator_MouseClick);
            this.splMain.Panel1.ResumeLayout(false);
            this.splMain.Panel1.PerformLayout();
            this.splMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splMain)).EndInit();
            this.splMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nudMax)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMin)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblOperator;
        private System.Windows.Forms.Label lblBase;
        private System.Windows.Forms.Label lblDelta;
        private ZedGraph.ZedGraphControl zgGraph;
        private System.Windows.Forms.Label lblBaseValue;
        private System.Windows.Forms.Label lblDeltaValue;
        private System.Windows.Forms.SplitContainer splMain;
        private System.Windows.Forms.Timer tmrUpdate;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.CheckBox chkMin;
        private System.Windows.Forms.CheckBox chkMax;
        private System.Windows.Forms.NumericUpDown nudMin;
        private System.Windows.Forms.NumericUpDown nudMax;
    }
}
