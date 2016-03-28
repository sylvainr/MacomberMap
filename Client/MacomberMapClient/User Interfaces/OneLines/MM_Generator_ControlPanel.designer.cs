namespace MacomberMapClient.User_Interfaces.OneLines
{
    partial class MM_Generator_ControlPanel
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
            this.grpFreq = new System.Windows.Forms.GroupBox();
            this.lblSysFreq = new System.Windows.Forms.Label();
            this.grpRPM = new System.Windows.Forms.GroupBox();
            this.btnDownRPM = new System.Windows.Forms.Button();
            this.btnUpRPM = new System.Windows.Forms.Button();
            this.grpVoltage = new System.Windows.Forms.GroupBox();
            this.comboTransformer = new System.Windows.Forms.ComboBox();
            this.btnDownVoltage = new System.Windows.Forms.Button();
            this.btnUpVoltage = new System.Windows.Forms.Button();
            this.textKV = new System.Windows.Forms.TextBox();
            this.chkManualVoltageTarget = new System.Windows.Forms.CheckBox();
            this.pnlXF = new System.Windows.Forms.Panel();
            this.lblSysVolt = new System.Windows.Forms.Label();
            this.grpSynchroscope = new System.Windows.Forms.GroupBox();
            this.lblCB = new System.Windows.Forms.Label();
            this.btnToggleBreaker = new System.Windows.Forms.Button();
            this.comboCB = new System.Windows.Forms.ComboBox();
            this.grpGeneration = new System.Windows.Forms.GroupBox();
            this.lblRampRate = new System.Windows.Forms.Label();
            this.chkUnit = new System.Windows.Forms.CheckBox();
            this.lblSysMW = new System.Windows.Forms.Label();
            this.comboUnit = new System.Windows.Forms.ComboBox();
            this.lblTolerance = new System.Windows.Forms.Label();
            this.txtTolIsoch = new System.Windows.Forms.TextBox();
            this.btnRamp = new System.Windows.Forms.Button();
            this.chkIsoch = new System.Windows.Forms.CheckBox();
            this.lblMW = new System.Windows.Forms.Label();
            this.lblIsoch = new System.Windows.Forms.Label();
            this.textMW = new System.Windows.Forms.TextBox();
            this.txtIsoch = new System.Windows.Forms.TextBox();
            this.btnStartIsoch = new System.Windows.Forms.Button();
            this.comboPLC = new System.Windows.Forms.ComboBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.lblPLC = new System.Windows.Forms.Label();
            this.lblStart = new System.Windows.Forms.Label();
            this.lblIsochFlag = new System.Windows.Forms.Label();
            this.lblMVARMax = new System.Windows.Forms.Label();
            this.lblMVARMin = new System.Windows.Forms.Label();
            this.grpWave = new System.Windows.Forms.GroupBox();
            this.pnlWaveform = new System.Windows.Forms.Panel();
            this.lblOnline = new System.Windows.Forms.Label();
            this.grpMVAR = new System.Windows.Forms.GroupBox();
            this.lblMWMax = new System.Windows.Forms.Label();
            this.lblMWMin = new System.Windows.Forms.Label();
            this.pnlMVARCapability = new System.Windows.Forms.Panel();
            this.tmrUpdate = new System.Windows.Forms.Timer(this.components);
            this.tmrButton = new System.Windows.Forms.Timer(this.components);
            this.tmrRPM = new System.Windows.Forms.Timer(this.components);
            this.tTip = new System.Windows.Forms.ToolTip(this.components);
            this.synchBreaker = new MacomberMapClient.User_Interfaces.OneLines.MM_OneLine_Synchroscope();
            this.mM_HSVoltageGauge = new MacomberMapClient.User_Interfaces.Generic.MM_Gauge();
            this.mM_LSVoltageGauge = new MacomberMapClient.User_Interfaces.Generic.MM_Gauge();
            this.mM_RPMGauge = new MacomberMapClient.User_Interfaces.Generic.MM_Gauge();
            this.mM_FrqGauge = new MacomberMapClient.User_Interfaces.Generic.MM_Gauge();
            this.grpFreq.SuspendLayout();
            this.grpRPM.SuspendLayout();
            this.grpVoltage.SuspendLayout();
            this.grpSynchroscope.SuspendLayout();
            this.grpGeneration.SuspendLayout();
            this.grpWave.SuspendLayout();
            this.grpMVAR.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpFreq
            // 
            this.grpFreq.Controls.Add(this.lblSysFreq);
            this.grpFreq.Controls.Add(this.mM_FrqGauge);
            this.grpFreq.ForeColor = System.Drawing.Color.White;
            this.grpFreq.Location = new System.Drawing.Point(394, 14);
            this.grpFreq.Name = "grpFreq";
            this.grpFreq.Size = new System.Drawing.Size(243, 252);
            this.grpFreq.TabIndex = 4;
            this.grpFreq.TabStop = false;
            this.grpFreq.Text = "Frequency";
            // 
            // lblSysFreq
            // 
            this.lblSysFreq.AutoSize = true;
            this.lblSysFreq.BackColor = System.Drawing.Color.Black;
            this.lblSysFreq.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSysFreq.ForeColor = System.Drawing.Color.LightGreen;
            this.lblSysFreq.Location = new System.Drawing.Point(38, 231);
            this.lblSysFreq.Name = "lblSysFreq";
            this.lblSysFreq.Size = new System.Drawing.Size(115, 15);
            this.lblSysFreq.TabIndex = 16;
            this.lblSysFreq.Text = "System Freq : Hz";
            // 
            // grpRPM
            // 
            this.grpRPM.Controls.Add(this.btnDownRPM);
            this.grpRPM.Controls.Add(this.btnUpRPM);
            this.grpRPM.Controls.Add(this.mM_RPMGauge);
            this.grpRPM.ForeColor = System.Drawing.Color.White;
            this.grpRPM.Location = new System.Drawing.Point(225, 68);
            this.grpRPM.Name = "grpRPM";
            this.grpRPM.Size = new System.Drawing.Size(163, 198);
            this.grpRPM.TabIndex = 5;
            this.grpRPM.TabStop = false;
            this.grpRPM.Text = "RPM";
            // 
            // btnDownRPM
            // 
            this.btnDownRPM.BackColor = System.Drawing.Color.Black;
            this.btnDownRPM.ForeColor = System.Drawing.Color.Red;
            this.btnDownRPM.Location = new System.Drawing.Point(18, 169);
            this.btnDownRPM.Name = "btnDownRPM";
            this.btnDownRPM.Size = new System.Drawing.Size(30, 23);
            this.btnDownRPM.TabIndex = 12;
            this.btnDownRPM.Text = "<";
            this.btnDownRPM.UseVisualStyleBackColor = false;
            this.btnDownRPM.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Control_MouseDown);
            this.btnDownRPM.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Control_MouseUp);
            // 
            // btnUpRPM
            // 
            this.btnUpRPM.BackColor = System.Drawing.Color.Black;
            this.btnUpRPM.ForeColor = System.Drawing.Color.Red;
            this.btnUpRPM.Location = new System.Drawing.Point(118, 169);
            this.btnUpRPM.Name = "btnUpRPM";
            this.btnUpRPM.Size = new System.Drawing.Size(30, 23);
            this.btnUpRPM.TabIndex = 11;
            this.btnUpRPM.Text = ">";
            this.btnUpRPM.UseVisualStyleBackColor = false;
            this.btnUpRPM.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Control_MouseDown);
            this.btnUpRPM.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Control_MouseUp);
            // 
            // grpVoltage
            // 
            this.grpVoltage.Controls.Add(this.comboTransformer);
            this.grpVoltage.Controls.Add(this.btnDownVoltage);
            this.grpVoltage.Controls.Add(this.btnUpVoltage);
            this.grpVoltage.Controls.Add(this.mM_HSVoltageGauge);
            this.grpVoltage.Controls.Add(this.mM_LSVoltageGauge);
            this.grpVoltage.Controls.Add(this.textKV);
            this.grpVoltage.Controls.Add(this.chkManualVoltageTarget);
            this.grpVoltage.Controls.Add(this.pnlXF);
            this.grpVoltage.ForeColor = System.Drawing.Color.White;
            this.grpVoltage.Location = new System.Drawing.Point(225, 272);
            this.grpVoltage.Name = "grpVoltage";
            this.grpVoltage.Size = new System.Drawing.Size(412, 218);
            this.grpVoltage.TabIndex = 6;
            this.grpVoltage.TabStop = false;
            this.grpVoltage.Text = "Voltage";
            // 
            // comboTransformer
            // 
            this.comboTransformer.FormattingEnabled = true;
            this.comboTransformer.Location = new System.Drawing.Point(149, 19);
            this.comboTransformer.Name = "comboTransformer";
            this.comboTransformer.Size = new System.Drawing.Size(88, 21);
            this.comboTransformer.TabIndex = 18;
            this.comboTransformer.SelectedIndexChanged += new System.EventHandler(this.comboTransformer_SelectedIndexChanged);
            // 
            // btnDownVoltage
            // 
            this.btnDownVoltage.BackColor = System.Drawing.Color.Black;
            this.btnDownVoltage.ForeColor = System.Drawing.Color.Red;
            this.btnDownVoltage.Location = new System.Drawing.Point(18, 164);
            this.btnDownVoltage.Name = "btnDownVoltage";
            this.btnDownVoltage.Size = new System.Drawing.Size(30, 23);
            this.btnDownVoltage.TabIndex = 13;
            this.btnDownVoltage.Text = "<";
            this.btnDownVoltage.UseVisualStyleBackColor = false;
            this.btnDownVoltage.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Control_MouseDown);
            this.btnDownVoltage.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Control_MouseUp);
            // 
            // btnUpVoltage
            // 
            this.btnUpVoltage.BackColor = System.Drawing.Color.Black;
            this.btnUpVoltage.ForeColor = System.Drawing.Color.Red;
            this.btnUpVoltage.Location = new System.Drawing.Point(118, 164);
            this.btnUpVoltage.Name = "btnUpVoltage";
            this.btnUpVoltage.Size = new System.Drawing.Size(30, 23);
            this.btnUpVoltage.TabIndex = 12;
            this.btnUpVoltage.Text = ">";
            this.btnUpVoltage.UseVisualStyleBackColor = false;
            this.btnUpVoltage.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Control_MouseDown);
            this.btnUpVoltage.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Control_MouseUp);
            // 
            // textKV
            // 
            this.textKV.Enabled = false;
            this.textKV.Location = new System.Drawing.Point(269, 192);
            this.textKV.Name = "textKV";
            this.textKV.Size = new System.Drawing.Size(42, 20);
            this.textKV.TabIndex = 4;
            this.textKV.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textKV_TextChanged);
            // 
            // chkManualVoltageTarget
            // 
            this.chkManualVoltageTarget.AutoSize = true;
            this.chkManualVoltageTarget.Location = new System.Drawing.Point(99, 193);
            this.chkManualVoltageTarget.Name = "chkManualVoltageTarget";
            this.chkManualVoltageTarget.Size = new System.Drawing.Size(134, 17);
            this.chkManualVoltageTarget.TabIndex = 3;
            this.chkManualVoltageTarget.Text = "Manual Voltage Target";
            this.chkManualVoltageTarget.UseVisualStyleBackColor = true;
            this.chkManualVoltageTarget.CheckedChanged += new System.EventHandler(this.chkManualVoltageTarget_CheckedChanged);
            // 
            // pnlXF
            // 
            this.pnlXF.BackColor = System.Drawing.Color.Black;
            this.pnlXF.ForeColor = System.Drawing.Color.White;
            this.pnlXF.Location = new System.Drawing.Point(149, 71);
            this.pnlXF.Name = "pnlXF";
            this.pnlXF.Size = new System.Drawing.Size(88, 87);
            this.pnlXF.TabIndex = 14;
            this.pnlXF.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlXF_Paint);
            // 
            // lblSysVolt
            // 
            this.lblSysVolt.AutoSize = true;
            this.lblSysVolt.BackColor = System.Drawing.Color.Black;
            this.lblSysVolt.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSysVolt.ForeColor = System.Drawing.Color.White;
            this.lblSysVolt.Location = new System.Drawing.Point(432, 436);
            this.lblSysVolt.Name = "lblSysVolt";
            this.lblSysVolt.Size = new System.Drawing.Size(134, 15);
            this.lblSysVolt.TabIndex = 15;
            this.lblSysVolt.Text = "System Voltage : KV";
            // 
            // grpSynchroscope
            // 
            this.grpSynchroscope.Controls.Add(this.lblCB);
            this.grpSynchroscope.Controls.Add(this.btnToggleBreaker);
            this.grpSynchroscope.Controls.Add(this.comboCB);
            this.grpSynchroscope.Controls.Add(this.synchBreaker);
            this.grpSynchroscope.ForeColor = System.Drawing.Color.White;
            this.grpSynchroscope.Location = new System.Drawing.Point(643, 14);
            this.grpSynchroscope.Name = "grpSynchroscope";
            this.grpSynchroscope.Size = new System.Drawing.Size(229, 476);
            this.grpSynchroscope.TabIndex = 8;
            this.grpSynchroscope.TabStop = false;
            this.grpSynchroscope.Text = "Breaker / Control";
            // 
            // lblCB
            // 
            this.lblCB.AutoSize = true;
            this.lblCB.Location = new System.Drawing.Point(23, 30);
            this.lblCB.Name = "lblCB";
            this.lblCB.Size = new System.Drawing.Size(44, 13);
            this.lblCB.TabIndex = 17;
            this.lblCB.Text = "Breaker";
            // 
            // btnToggleBreaker
            // 
            this.btnToggleBreaker.BackColor = System.Drawing.Color.Black;
            this.btnToggleBreaker.ForeColor = System.Drawing.Color.White;
            this.btnToggleBreaker.Location = new System.Drawing.Point(57, 450);
            this.btnToggleBreaker.Name = "btnToggleBreaker";
            this.btnToggleBreaker.Size = new System.Drawing.Size(108, 23);
            this.btnToggleBreaker.TabIndex = 17;
            this.btnToggleBreaker.Text = "Toggle Breaker";
            this.btnToggleBreaker.UseVisualStyleBackColor = false;
            this.btnToggleBreaker.Click += new System.EventHandler(this.btnToggleBreaker_Click);
            // 
            // comboCB
            // 
            this.comboCB.FormattingEnabled = true;
            this.comboCB.Location = new System.Drawing.Point(90, 27);
            this.comboCB.Name = "comboCB";
            this.comboCB.Size = new System.Drawing.Size(121, 21);
            this.comboCB.TabIndex = 18;
            this.comboCB.SelectedIndexChanged += new System.EventHandler(this.comboCB_SelectedIndexChanged);
            // 
            // grpGeneration
            // 
            this.grpGeneration.Controls.Add(this.lblRampRate);
            this.grpGeneration.Controls.Add(this.chkUnit);
            this.grpGeneration.Controls.Add(this.lblSysMW);
            this.grpGeneration.Controls.Add(this.comboUnit);
            this.grpGeneration.Controls.Add(this.lblTolerance);
            this.grpGeneration.Controls.Add(this.txtTolIsoch);
            this.grpGeneration.Controls.Add(this.btnRamp);
            this.grpGeneration.Controls.Add(this.chkIsoch);
            this.grpGeneration.Controls.Add(this.lblMW);
            this.grpGeneration.Controls.Add(this.lblIsoch);
            this.grpGeneration.Controls.Add(this.textMW);
            this.grpGeneration.Controls.Add(this.txtIsoch);
            this.grpGeneration.Controls.Add(this.btnStartIsoch);
            this.grpGeneration.Controls.Add(this.comboPLC);
            this.grpGeneration.Controls.Add(this.btnStart);
            this.grpGeneration.Controls.Add(this.lblPLC);
            this.grpGeneration.Controls.Add(this.lblStart);
            this.grpGeneration.ForeColor = System.Drawing.Color.White;
            this.grpGeneration.Location = new System.Drawing.Point(19, 14);
            this.grpGeneration.Name = "grpGeneration";
            this.grpGeneration.Size = new System.Drawing.Size(200, 476);
            this.grpGeneration.TabIndex = 9;
            this.grpGeneration.TabStop = false;
            this.grpGeneration.Text = "Main Panel";
            // 
            // lblRampRate
            // 
            this.lblRampRate.AutoSize = true;
            this.lblRampRate.Location = new System.Drawing.Point(12, 246);
            this.lblRampRate.Name = "lblRampRate";
            this.lblRampRate.Size = new System.Drawing.Size(61, 13);
            this.lblRampRate.TabIndex = 34;
            this.lblRampRate.Text = "Ramp Rate";
            // 
            // chkUnit
            // 
            this.chkUnit.AutoSize = true;
            this.chkUnit.Location = new System.Drawing.Point(20, 329);
            this.chkUnit.Name = "chkUnit";
            this.chkUnit.Size = new System.Drawing.Size(134, 17);
            this.chkUnit.TabIndex = 32;
            this.chkUnit.Text = "Unit Frequency Control";
            this.chkUnit.UseVisualStyleBackColor = true;
            this.chkUnit.CheckedChanged += new System.EventHandler(this.chkUnit_CheckedChanged);
            // 
            // lblSysMW
            // 
            this.lblSysMW.AutoSize = true;
            this.lblSysMW.BackColor = System.Drawing.Color.Black;
            this.lblSysMW.ForeColor = System.Drawing.Color.White;
            this.lblSysMW.Location = new System.Drawing.Point(12, 182);
            this.lblSysMW.Name = "lblSysMW";
            this.lblSysMW.Size = new System.Drawing.Size(89, 13);
            this.lblSysMW.TabIndex = 30;
            this.lblSysMW.Text = "Actual MW : MW";
            // 
            // comboUnit
            // 
            this.comboUnit.FormattingEnabled = true;
            this.comboUnit.Location = new System.Drawing.Point(120, 102);
            this.comboUnit.Name = "comboUnit";
            this.comboUnit.Size = new System.Drawing.Size(63, 21);
            this.comboUnit.TabIndex = 29;
            this.comboUnit.SelectedIndexChanged += new System.EventHandler(this.comboUnit_SelectedIndexChanged);
            // 
            // lblTolerance
            // 
            this.lblTolerance.AutoSize = true;
            this.lblTolerance.Location = new System.Drawing.Point(17, 422);
            this.lblTolerance.Name = "lblTolerance";
            this.lblTolerance.Size = new System.Drawing.Size(55, 13);
            this.lblTolerance.TabIndex = 28;
            this.lblTolerance.Text = "Tolerance";
            // 
            // txtTolIsoch
            // 
            this.txtTolIsoch.Location = new System.Drawing.Point(122, 419);
            this.txtTolIsoch.Name = "txtTolIsoch";
            this.txtTolIsoch.Size = new System.Drawing.Size(46, 20);
            this.txtTolIsoch.TabIndex = 27;
            this.txtTolIsoch.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtTolIsoch_TextChanged);
            // 
            // btnRamp
            // 
            this.btnRamp.BackColor = System.Drawing.Color.Black;
            this.btnRamp.ForeColor = System.Drawing.Color.White;
            this.btnRamp.Location = new System.Drawing.Point(131, 209);
            this.btnRamp.Name = "btnRamp";
            this.btnRamp.Size = new System.Drawing.Size(62, 23);
            this.btnRamp.TabIndex = 17;
            this.btnRamp.Text = "Ramp";
            this.btnRamp.UseVisualStyleBackColor = false;
            this.btnRamp.Click += new System.EventHandler(this.btnRamp_Click);
            // 
            // chkIsoch
            // 
            this.chkIsoch.AutoSize = true;
            this.chkIsoch.Location = new System.Drawing.Point(20, 358);
            this.chkIsoch.Name = "chkIsoch";
            this.chkIsoch.Size = new System.Drawing.Size(143, 17);
            this.chkIsoch.TabIndex = 25;
            this.chkIsoch.Text = "Island Frequency Control";
            this.chkIsoch.UseVisualStyleBackColor = true;
            this.chkIsoch.CheckedChanged += new System.EventHandler(this.chkIsoch_CheckedChanged);
            // 
            // lblMW
            // 
            this.lblMW.AutoSize = true;
            this.lblMW.Location = new System.Drawing.Point(12, 218);
            this.lblMW.Name = "lblMW";
            this.lblMW.Size = new System.Drawing.Size(61, 13);
            this.lblMW.TabIndex = 6;
            this.lblMW.Text = "Target MW";
            // 
            // lblIsoch
            // 
            this.lblIsoch.AutoSize = true;
            this.lblIsoch.Location = new System.Drawing.Point(17, 393);
            this.lblIsoch.Name = "lblIsoch";
            this.lblIsoch.Size = new System.Drawing.Size(91, 13);
            this.lblIsoch.TabIndex = 20;
            this.lblIsoch.Text = "Frequency Target";
            // 
            // textMW
            // 
            this.textMW.Location = new System.Drawing.Point(79, 211);
            this.textMW.Name = "textMW";
            this.textMW.Size = new System.Drawing.Size(46, 20);
            this.textMW.TabIndex = 5;
            this.textMW.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textMW_TextChanged);
            // 
            // txtIsoch
            // 
            this.txtIsoch.Location = new System.Drawing.Point(122, 386);
            this.txtIsoch.Name = "txtIsoch";
            this.txtIsoch.Size = new System.Drawing.Size(46, 20);
            this.txtIsoch.TabIndex = 19;
            this.txtIsoch.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textIsoch_TextChanged);
            // 
            // btnStartIsoch
            // 
            this.btnStartIsoch.BackColor = System.Drawing.Color.Black;
            this.btnStartIsoch.ForeColor = System.Drawing.Color.White;
            this.btnStartIsoch.Location = new System.Drawing.Point(20, 450);
            this.btnStartIsoch.Name = "btnStartIsoch";
            this.btnStartIsoch.Size = new System.Drawing.Size(46, 23);
            this.btnStartIsoch.TabIndex = 18;
            this.btnStartIsoch.Text = "Start";
            this.btnStartIsoch.UseVisualStyleBackColor = false;
            this.btnStartIsoch.Click += new System.EventHandler(this.btnStartIsoch_Click);
            // 
            // comboPLC
            // 
            this.comboPLC.FormattingEnabled = true;
            this.comboPLC.Location = new System.Drawing.Point(120, 75);
            this.comboPLC.Name = "comboPLC";
            this.comboPLC.Size = new System.Drawing.Size(63, 21);
            this.comboPLC.TabIndex = 16;
            this.comboPLC.SelectedIndexChanged += new System.EventHandler(this.comboPLC_SelectedIndexChanged);
            // 
            // btnStart
            // 
            this.btnStart.BackColor = System.Drawing.Color.Black;
            this.btnStart.ForeColor = System.Drawing.Color.Red;
            this.btnStart.Location = new System.Drawing.Point(79, 19);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(104, 34);
            this.btnStart.TabIndex = 16;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = false;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // lblPLC
            // 
            this.lblPLC.AutoSize = true;
            this.lblPLC.Location = new System.Drawing.Point(9, 83);
            this.lblPLC.Name = "lblPLC";
            this.lblPLC.Size = new System.Drawing.Size(105, 13);
            this.lblPLC.TabIndex = 4;
            this.lblPLC.Text = "System : Local Local";
            // 
            // lblStart
            // 
            this.lblStart.AutoSize = true;
            this.lblStart.Location = new System.Drawing.Point(17, 28);
            this.lblStart.Name = "lblStart";
            this.lblStart.Size = new System.Drawing.Size(37, 13);
            this.lblStart.TabIndex = 0;
            this.lblStart.Text = "Power";
            // 
            // lblIsochFlag
            // 
            this.lblIsochFlag.AutoSize = true;
            this.lblIsochFlag.BackColor = System.Drawing.Color.Black;
            this.lblIsochFlag.ForeColor = System.Drawing.Color.Red;
            this.lblIsochFlag.Location = new System.Drawing.Point(91, 470);
            this.lblIsochFlag.Name = "lblIsochFlag";
            this.lblIsochFlag.Size = new System.Drawing.Size(105, 13);
            this.lblIsochFlag.TabIndex = 31;
            this.lblIsochFlag.Text = "In Frequency Control";
            // 
            // lblMVARMax
            // 
            this.lblMVARMax.AutoSize = true;
            this.lblMVARMax.BackColor = System.Drawing.Color.Black;
            this.lblMVARMax.ForeColor = System.Drawing.Color.White;
            this.lblMVARMax.Location = new System.Drawing.Point(125, 206);
            this.lblMVARMax.Name = "lblMVARMax";
            this.lblMVARMax.Size = new System.Drawing.Size(61, 13);
            this.lblMVARMax.TabIndex = 24;
            this.lblMVARMax.Text = "MVAR Max";
            // 
            // lblMVARMin
            // 
            this.lblMVARMin.AutoSize = true;
            this.lblMVARMin.BackColor = System.Drawing.Color.Black;
            this.lblMVARMin.ForeColor = System.Drawing.Color.White;
            this.lblMVARMin.Location = new System.Drawing.Point(6, 206);
            this.lblMVARMin.Name = "lblMVARMin";
            this.lblMVARMin.Size = new System.Drawing.Size(58, 13);
            this.lblMVARMin.TabIndex = 22;
            this.lblMVARMin.Text = "MVAR Min";
            // 
            // grpWave
            // 
            this.grpWave.Controls.Add(this.pnlWaveform);
            this.grpWave.ForeColor = System.Drawing.Color.White;
            this.grpWave.Location = new System.Drawing.Point(882, 36);
            this.grpWave.Name = "grpWave";
            this.grpWave.Size = new System.Drawing.Size(268, 218);
            this.grpWave.TabIndex = 12;
            this.grpWave.TabStop = false;
            this.grpWave.Text = "Wave Form";
            // 
            // pnlWaveform
            // 
            this.pnlWaveform.BackColor = System.Drawing.Color.Black;
            this.pnlWaveform.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlWaveform.ForeColor = System.Drawing.Color.White;
            this.pnlWaveform.Location = new System.Drawing.Point(3, 16);
            this.pnlWaveform.Name = "pnlWaveform";
            this.pnlWaveform.Size = new System.Drawing.Size(262, 199);
            this.pnlWaveform.TabIndex = 0;
            this.pnlWaveform.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlWaveform_Paint);
            // 
            // lblOnline
            // 
            this.lblOnline.AutoSize = true;
            this.lblOnline.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOnline.ForeColor = System.Drawing.Color.Red;
            this.lblOnline.Location = new System.Drawing.Point(242, 35);
            this.lblOnline.Name = "lblOnline";
            this.lblOnline.Size = new System.Drawing.Size(131, 25);
            this.lblOnline.TabIndex = 13;
            this.lblOnline.Text = "ON/OFFLINE";
            this.lblOnline.DoubleClick += new System.EventHandler(this.lblOnline_DoubleClick);
            // 
            // grpMVAR
            // 
            this.grpMVAR.Controls.Add(this.lblMWMax);
            this.grpMVAR.Controls.Add(this.lblMWMin);
            this.grpMVAR.Controls.Add(this.pnlMVARCapability);
            this.grpMVAR.Controls.Add(this.lblMVARMin);
            this.grpMVAR.Controls.Add(this.lblMVARMax);
            this.grpMVAR.ForeColor = System.Drawing.Color.White;
            this.grpMVAR.Location = new System.Drawing.Point(882, 260);
            this.grpMVAR.Name = "grpMVAR";
            this.grpMVAR.Size = new System.Drawing.Size(268, 230);
            this.grpMVAR.TabIndex = 15;
            this.grpMVAR.TabStop = false;
            this.grpMVAR.Text = "MW/MVAR";
            // 
            // lblMWMax
            // 
            this.lblMWMax.AutoSize = true;
            this.lblMWMax.BackColor = System.Drawing.Color.Black;
            this.lblMWMax.ForeColor = System.Drawing.Color.White;
            this.lblMWMax.Location = new System.Drawing.Point(125, 188);
            this.lblMWMax.Name = "lblMWMax";
            this.lblMWMax.Size = new System.Drawing.Size(50, 13);
            this.lblMWMax.TabIndex = 26;
            this.lblMWMax.Text = "MW Max";
            // 
            // lblMWMin
            // 
            this.lblMWMin.AutoSize = true;
            this.lblMWMin.BackColor = System.Drawing.Color.Black;
            this.lblMWMin.ForeColor = System.Drawing.Color.White;
            this.lblMWMin.Location = new System.Drawing.Point(6, 188);
            this.lblMWMin.Name = "lblMWMin";
            this.lblMWMin.Size = new System.Drawing.Size(47, 13);
            this.lblMWMin.TabIndex = 25;
            this.lblMWMin.Text = "MW Min";
            // 
            // pnlMVARCapability
            // 
            this.pnlMVARCapability.Location = new System.Drawing.Point(6, 19);
            this.pnlMVARCapability.Name = "pnlMVARCapability";
            this.pnlMVARCapability.Size = new System.Drawing.Size(257, 166);
            this.pnlMVARCapability.TabIndex = 18;
            this.pnlMVARCapability.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlMVARCapability_Paint);
            // 
            // tmrUpdate
            // 
            this.tmrUpdate.Enabled = true;
            this.tmrUpdate.Tick += new System.EventHandler(this.tmrUpdate_Tick);
            // 
            // tmrButton
            // 
            this.tmrButton.Interval = 200;
            this.tmrButton.Tick += new System.EventHandler(this.tmrButton_Tick);
            // 
            // tmrRPM
            // 
            this.tmrRPM.Enabled = true;
            this.tmrRPM.Interval = 200;
            this.tmrRPM.Tick += new System.EventHandler(this.tmrRPM_Tick);
            // 
            // synchBreaker
            // 
            this.synchBreaker.BackColor = System.Drawing.Color.Black;
            this.synchBreaker.ForeColor = System.Drawing.Color.White;
            this.synchBreaker.Location = new System.Drawing.Point(6, 54);
            this.synchBreaker.Name = "synchBreaker";
            this.synchBreaker.Size = new System.Drawing.Size(217, 380);
            this.synchBreaker.TabIndex = 0;
            // 
            // mM_HSVoltageGauge
            // 
            this.mM_HSVoltageGauge.BackColor = System.Drawing.Color.Black;
            this.mM_HSVoltageGauge.Current = 0D;
            this.mM_HSVoltageGauge.ErrorRangeMaximum = null;
            this.mM_HSVoltageGauge.ErrorRangeMinimum = null;
            this.mM_HSVoltageGauge.GoodRangeMaximum = null;
            this.mM_HSVoltageGauge.GoodRangeMinimum = null;
            this.mM_HSVoltageGauge.Location = new System.Drawing.Point(233, 28);
            this.mM_HSVoltageGauge.Maximum = 400D;
            this.mM_HSVoltageGauge.MaximumAngle = 150;
            this.mM_HSVoltageGauge.Minimum = 0D;
            this.mM_HSVoltageGauge.MinimumAngle = -150;
            this.mM_HSVoltageGauge.Name = "mM_HSVoltageGauge";
            this.mM_HSVoltageGauge.NumberFormat = null;
            this.mM_HSVoltageGauge.Size = new System.Drawing.Size(130, 130);
            this.mM_HSVoltageGauge.TabIndex = 10;
            this.mM_HSVoltageGauge.Text = "mM_HSVoltageGauge";
            this.mM_HSVoltageGauge.WarningRangeMaximum = null;
            this.mM_HSVoltageGauge.WarningRangeMinimum = null;
            // 
            // mM_LSVoltageGauge
            // 
            this.mM_LSVoltageGauge.BackColor = System.Drawing.Color.Black;
            this.mM_LSVoltageGauge.Current = 0D;
            this.mM_LSVoltageGauge.ErrorRangeMaximum = 17D;
            this.mM_LSVoltageGauge.ErrorRangeMinimum = 0D;
            this.mM_LSVoltageGauge.GoodRangeMaximum = 14.3D;
            this.mM_LSVoltageGauge.GoodRangeMinimum = 13.3D;
            this.mM_LSVoltageGauge.Location = new System.Drawing.Point(18, 28);
            this.mM_LSVoltageGauge.Maximum = 17D;
            this.mM_LSVoltageGauge.MaximumAngle = 150;
            this.mM_LSVoltageGauge.Minimum = 0D;
            this.mM_LSVoltageGauge.MinimumAngle = -150;
            this.mM_LSVoltageGauge.Name = "mM_LSVoltageGauge";
            this.mM_LSVoltageGauge.NumberFormat = "0.0";
            this.mM_LSVoltageGauge.Size = new System.Drawing.Size(130, 130);
            this.mM_LSVoltageGauge.TabIndex = 10;
            this.mM_LSVoltageGauge.Text = "mM_LSVoltageGauge";
            this.mM_LSVoltageGauge.WarningRangeMaximum = 16D;
            this.mM_LSVoltageGauge.WarningRangeMinimum = 11D;
            // 
            // mM_RPMGauge
            // 
            this.mM_RPMGauge.BackColor = System.Drawing.Color.Black;
            this.mM_RPMGauge.Current = 0D;
            this.mM_RPMGauge.ErrorRangeMaximum = 4320D;
            this.mM_RPMGauge.ErrorRangeMinimum = 0D;
            this.mM_RPMGauge.GoodRangeMaximum = 1900D;
            this.mM_RPMGauge.GoodRangeMinimum = 1700D;
            this.mM_RPMGauge.Location = new System.Drawing.Point(18, 21);
            this.mM_RPMGauge.Maximum = 4320D;
            this.mM_RPMGauge.MaximumAngle = 150;
            this.mM_RPMGauge.Minimum = 0D;
            this.mM_RPMGauge.MinimumAngle = -150;
            this.mM_RPMGauge.Name = "mM_RPMGauge";
            this.mM_RPMGauge.NumberFormat = null;
            this.mM_RPMGauge.Size = new System.Drawing.Size(130, 130);
            this.mM_RPMGauge.TabIndex = 10;
            this.mM_RPMGauge.Text = "mM_RPMGauge";
            this.mM_RPMGauge.WarningRangeMaximum = 2100D;
            this.mM_RPMGauge.WarningRangeMinimum = 1500D;
            // 
            // mM_FrqGauge
            // 
            this.mM_FrqGauge.BackColor = System.Drawing.Color.Black;
            this.mM_FrqGauge.Current = 60D;
            this.mM_FrqGauge.ErrorRangeMaximum = 62D;
            this.mM_FrqGauge.ErrorRangeMinimum = 57D;
            this.mM_FrqGauge.GoodRangeMaximum = 60.1D;
            this.mM_FrqGauge.GoodRangeMinimum = 59.8D;
            this.mM_FrqGauge.Location = new System.Drawing.Point(24, 19);
            this.mM_FrqGauge.Maximum = 62D;
            this.mM_FrqGauge.MaximumAngle = 150;
            this.mM_FrqGauge.Minimum = 57D;
            this.mM_FrqGauge.MinimumAngle = -150;
            this.mM_FrqGauge.Name = "mM_FrqGauge";
            this.mM_FrqGauge.NumberFormat = "0.00";
            this.mM_FrqGauge.Size = new System.Drawing.Size(196, 203);
            this.mM_FrqGauge.TabIndex = 10;
            this.mM_FrqGauge.Text = "mM_FreqGauge";
            this.mM_FrqGauge.WarningRangeMaximum = 60.3D;
            this.mM_FrqGauge.WarningRangeMinimum = 59.5D;
            // 
            // MM_Generator_ControlPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(this.lblSysVolt);
            this.Controls.Add(this.grpMVAR);
            this.Controls.Add(this.lblOnline);
            this.Controls.Add(this.lblIsochFlag);
            this.Controls.Add(this.grpWave);
            this.Controls.Add(this.grpGeneration);
            this.Controls.Add(this.grpSynchroscope);
            this.Controls.Add(this.grpVoltage);
            this.Controls.Add(this.grpRPM);
            this.Controls.Add(this.grpFreq);
            this.ForeColor = System.Drawing.Color.White;
            this.Name = "MM_Generator_ControlPanel";
            this.Size = new System.Drawing.Size(1172, 505);
            this.grpFreq.ResumeLayout(false);
            this.grpFreq.PerformLayout();
            this.grpRPM.ResumeLayout(false);
            this.grpVoltage.ResumeLayout(false);
            this.grpVoltage.PerformLayout();
            this.grpSynchroscope.ResumeLayout(false);
            this.grpSynchroscope.PerformLayout();
            this.grpGeneration.ResumeLayout(false);
            this.grpGeneration.PerformLayout();
            this.grpWave.ResumeLayout(false);
            this.grpMVAR.ResumeLayout(false);
            this.grpMVAR.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.GroupBox grpFreq;
        private System.Windows.Forms.GroupBox grpRPM;
        private System.Windows.Forms.GroupBox grpVoltage;
        private System.Windows.Forms.TextBox textKV;
        private System.Windows.Forms.CheckBox chkManualVoltageTarget;
        private System.Windows.Forms.GroupBox grpSynchroscope;
        private System.Windows.Forms.GroupBox grpGeneration;
        private System.Windows.Forms.Label lblStart;
        private System.Windows.Forms.GroupBox grpWave;
        private System.Windows.Forms.Label lblOnline;
        private Generic.MM_Gauge mM_FrqGauge;
        private Generic.MM_Gauge mM_RPMGauge;
        private Generic.MM_Gauge mM_HSVoltageGauge;
        private Generic.MM_Gauge mM_LSVoltageGauge;
        private MM_OneLine_Synchroscope synchBreaker;
        private System.Windows.Forms.GroupBox grpMVAR;
        private System.Windows.Forms.ComboBox comboPLC;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Label lblPLC;
        private System.Windows.Forms.Button btnRamp;
        private System.Windows.Forms.Label lblMW;
        private System.Windows.Forms.TextBox textMW;
        private System.Windows.Forms.ComboBox comboTransformer;
        private System.Windows.Forms.ComboBox comboCB;
        private System.Windows.Forms.Label lblCB;
        private System.Windows.Forms.Panel pnlMVARCapability;
        private System.Windows.Forms.Button btnToggleBreaker;
        private System.Windows.Forms.Button btnDownRPM;
        private System.Windows.Forms.Button btnUpRPM;
        private System.Windows.Forms.Button btnDownVoltage;
        private System.Windows.Forms.Button btnUpVoltage;
        private System.Windows.Forms.Timer tmrUpdate;
        private System.Windows.Forms.Timer tmrRPM;
        private System.Windows.Forms.Panel pnlWaveform;
        private System.Windows.Forms.Label lblIsoch;
        private System.Windows.Forms.TextBox txtIsoch;
        private System.Windows.Forms.Button btnStartIsoch;
        private System.Windows.Forms.Label lblMVARMax;
        private System.Windows.Forms.Label lblMVARMin;
        private System.Windows.Forms.CheckBox chkIsoch;
        private System.Windows.Forms.Label lblTolerance;
        private System.Windows.Forms.TextBox txtTolIsoch;
        private System.Windows.Forms.Timer tmrButton;
        private System.Windows.Forms.Panel pnlXF;
        private System.Windows.Forms.ComboBox comboUnit;
        private System.Windows.Forms.Label lblMWMax;
        private System.Windows.Forms.Label lblMWMin;
        private System.Windows.Forms.Label lblSysVolt;
        private System.Windows.Forms.Label lblSysMW;
        private System.Windows.Forms.Label lblIsochFlag;
        private System.Windows.Forms.ToolTip tTip;
        private System.Windows.Forms.CheckBox chkUnit;
        private System.Windows.Forms.Label lblSysFreq;
        private System.Windows.Forms.Label lblRampRate;
    }
}
