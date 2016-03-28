using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Integration;

namespace MacomberMapClient.User_Interfaces.OneLines
{
    /// <summary>
    /// This class provides a generator control panel
    /// </summary>
    public partial class MM_Generator_ControlPanel : UserControl
    {
        #region Variable declarations
        /// <summary>Our unit that we're associated with</summary>
        public MM_Unit BaseUnit;

        /// <summary>The list of breakers associated with our display</summary>
        public SortedDictionary<String, MM_Breaker_Switch> Breakers = new SortedDictionary<string, MM_Breaker_Switch>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>Our collection of base transformers</summary>
        public SortedDictionary<String, MM_OneLine_Element> BaseTransformers = new SortedDictionary<string, MM_OneLine_Element>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>Our one-line element associated with the control</summary>
        public MM_OneLine_Element OneLineElement;

        /// <summary>Our high side transformer</summary>
        public MM_OneLine_Element HighsideTransformer;

        /// <summary>Our associated one-line viewer</summary>
        public MM_OneLine_Viewer OneLineViewer;

        /// <summary>Our winding KV</summary>
        public MM_KVLevel WindingKV;

        /// <summary>The other winding KV</summary>
        public MM_KVLevel OtherWindingKV;

        /// <summary>The ratio of our transformer</summary>
        public double TransformerRatio = 0;

        /// <summary>Our system voltage value</summary>
        private double KVolts = 0;

        /// <summary>Our RPM acceleration factor</summary>
        private double RPMAcceleration = 0;

        /// <summary>Our system voltage level</summary>
        private float VoltageLevel = 0;

        /// <summary>Whether this control panel is the local owner</summary>
        private bool LocalOwner = false;

        /// <summary>Our pseudo-bus</summary>
        public MM_Bus GeneratorBus = new MM_Bus();

        /// <summary>Our pseudo-island</summary>
        public MM_Island GeneratorIsland = new MM_Island();
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new generator control panel
        /// </summary>
        public MM_Generator_ControlPanel()
        {
            InitializeComponent();
            typeof(Panel).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(pnlMVARCapability, true);
            typeof(Panel).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(pnlWaveform, true);
            pnlXF.GetType().GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pnlXF, true);
            mM_FrqGauge.CurrentChanged += FreqChanged;
            mM_LSVoltageGauge.CurrentChanged += VoltageChanged;
            mM_HSVoltageGauge.CurrentChanged += VoltageChanged;
        }

        /// <summary>
        /// Initialize a new generator control panel
        /// </summary>
        /// <param name="BaseElement"></param>
        /// <param name="OneLine"></param>
        public MM_Generator_ControlPanel(MM_OneLine_Element BaseElement, MM_OneLine_Viewer OneLine) : this()
        {
            this.OneLineElement = BaseElement;
            this.BaseUnit = (MM_Unit)BaseElement.BaseElement;
            this.OneLineViewer = OneLine;

            //If we're taking ownership of our unit, do so now.
            BaseUnit.UnitStatus.TEID = BaseUnit.TEID;
            if (String.IsNullOrEmpty(BaseUnit.UnitStatus.UnitController) && !BaseUnit.UnitStatus.IsOwner)
            {
                BaseUnit.UnitStatus.UnitController = MM_Server_Interface.UserName + "@" + Environment.MachineName;
                BaseUnit.UnitStatus.OwnershipStart = DateTime.Now;
                BaseUnit.UnitStatus.IsOwner = LocalOwner = true;
            }
            if (BaseUnit.Unit_Status == "Online" || BaseUnit.Unit_Status == "Synchronized")
            {
                if (!BaseUnit.UnitStatus.Online)
                    BaseUnit.UnitStatus.Online = true;
                if (BaseUnit.UnitStatus.BaseRPM <= (BaseUnit.NominalRPM / 60.0) * BaseUnit.Frequency)
                    BaseUnit.UnitStatus.BaseRPM = (BaseUnit.NominalRPM / 60.0) * BaseUnit.Frequency;
            }
            else if (BaseUnit.Unit_Status == "Offline")
            {
                if (BaseUnit.UnitStatus.Online)
                    BaseUnit.UnitStatus.Online = false;
                if (BaseUnit.UnitStatus.CheckedFrequencyControl)
                    BaseUnit.UnitStatus.CheckedFrequencyControl = false;
                if (BaseUnit.UnitStatus.BaseRPM != 0)
                    BaseUnit.UnitStatus.BaseRPM = 0;
            }
            MM_Server_Interface.PushUnitStatus(BaseUnit.UnitStatus);
            BaseUnit.ValuesChanged += BaseUnit_ValuesChanged;

            //First, start by adding all of the transformers in our substation that have a winding at our level
            foreach (MM_OneLine_Element Elem in OneLine.ElementsByTEID.Values)
                if (Elem.ElemType == MM_OneLine_Element.enumElemTypes.Transformer)
                {
                    MM_OneLine_Element NewElemParent = new MM_OneLine_Element(Elem);
                    NewElemParent.Orientation = MM_OneLine_Element.enumOrientations.Right;
                    MM_OneLine_Element NewElem = new MM_OneLine_Element(Elem.Descriptor);
                    NewElemParent.ParentElement = NewElemParent;
                    if (NewElemParent.Windings[0].Orientation == MM_OneLine_Element.enumOrientations.Down)
                    {
                        NewElemParent.Windings[0].Orientation = MM_OneLine_Element.enumOrientations.Left;
                        NewElemParent.Windings[1].Orientation = MM_OneLine_Element.enumOrientations.Right;
                    }
                    else if (NewElemParent.Windings[0].Orientation == MM_OneLine_Element.enumOrientations.Up)
                    {
                        NewElemParent.Windings[0].Orientation = MM_OneLine_Element.enumOrientations.Right;
                        NewElemParent.Windings[1].Orientation = MM_OneLine_Element.enumOrientations.Left;
                    }
                    NewElem.Orientation = MM_OneLine_Element.enumOrientations.Horizontal;
                    BaseTransformers.Add((Elem.BaseElement as MM_Transformer).PrimaryWinding.Name, NewElem);
                }

            //Look within all of our breaker-to-breaker traces, and see if we can find breakers within our contingencies
            List<MM_Transformer> FoundTransformers = new List<MM_Transformer>();
            List<MM_Contingency> FoundContingencies = new List<MM_Contingency>();
            double breakerVoltage = 0;
            foreach (MM_Element TestElement in MM_Repository.TEIDs.Values)
                if (TestElement.Contingencies != null && TestElement.Contingencies.Contains(BaseUnit.Contingencies[0]))

                    // if (TestElement.ElemType.Name == "Breaker")
                    if (TestElement.ElemType.Name == "Breaker" && BaseUnit.SubName == TestElement.SubName)
                    {
                        Breakers.Add(TestElement.Name, (MM_Breaker_Switch)TestElement);
                        comboCB.Items.Add(TestElement.Name);
                        foreach (MM_Contingency Ctg in TestElement.Contingencies)
                            if (Ctg != BaseUnit.Contingencies[0])
                                FoundContingencies.Add(Ctg);
                        breakerVoltage = (!double.IsNaN(TestElement.FarVoltage) && TestElement.FarVoltage > 1) ? TestElement.FarVoltage : TestElement.NearVoltage;
                    }
                    else if (TestElement.ElemType.Name == "Transformer")
                        FoundTransformers.Add((MM_Transformer)TestElement);

            //If we haven't found any transformers, look on the other side of our B2B path.
            if (FoundTransformers.Count == 0 && FoundContingencies.Count > 0)
                foreach (MM_Element TestElement in MM_Repository.TEIDs.Values)
                    if (TestElement.Contingencies != null && TestElement.Contingencies.Contains(FoundContingencies[0]) && TestElement.ElemType.Name == "Transformer")
                        FoundTransformers.Add((MM_Transformer)TestElement);

            //If we still haven't found any transformers, keep looking on the other side of our path.
            if (FoundTransformers.Count == 0 && FoundContingencies.Count > 0)
                foreach (MM_Element TestElement in MM_Repository.TEIDs.Values)
                    if (TestElement.Contingencies != null && TestElement.Contingencies.Contains(FoundContingencies[0]) && TestElement.ElemType.Name == "Breaker")
                        foreach (MM_Element TestElem in MM_Repository.TEIDs.Values)
                            if (TestElem.Contingencies != null && TestElem.Contingencies.Contains(TestElement.Contingencies[0]) && TestElem.ElemType.Name == "Transformer")
                                FoundTransformers.Add((MM_Transformer)TestElem);

            //If we still haven't found any transformers, set our default voltage to prevent crashes.
            if (FoundTransformers.Count == 0)
            {
                comboTransformer.Items.AddRange(BaseTransformers.Keys.ToArray());

                if (Breakers.Count > 0)
                {
                    VoltageLevel = BaseUnit.KVLevel.Nominal;
                    KVolts = (!double.IsNaN(breakerVoltage)) ? breakerVoltage : VoltageLevel;
                }
            }

            List<float> FoundLevels = new List<float>();
            List<MM_KVLevel> WindingKVList = new List<MM_KVLevel>();
            List<MM_OneLine_Element> HighsideTransformerList = new List<MM_OneLine_Element>();
            float UnitKV = Single.Parse(BaseElement.xConfig.Attributes["BaseElement.KVLevel"].Value.Replace(" KV", ""));
            WindingKV = BaseElement.KVLevel;
            foreach (MM_Transformer XF in FoundTransformers)
                foreach (MM_OneLine_TransformerWinding XFW in OneLine.ElementsByTEID[XF.TEID].Windings)
                {
                    float WindingKV_Value = Convert.ToSingle(XFW.xConfig.Attributes["BaseElement.KVLevel"].Value.Replace(" KV", ""));
                    if (WindingKV_Value != 1 && WindingKV_Value != UnitKV)
                    {
                        if (FoundTransformers.Count == 1)
                        {
                            VoltageLevel = WindingKV_Value;
                            OtherWindingKV = XFW.BaseElement.KVLevel;
                            HighsideTransformer = new MM_OneLine_Element(OneLine.Descriptors[XF]) { Orientation = MM_OneLine_Element.enumOrientations.Right };
                            comboTransformer.Items.Add(XF.PrimaryWinding.Name);
                            comboTransformer.SelectedItem = XF.PrimaryWinding.Name;
                            KVolts = (!double.IsNaN(HighsideTransformer.BaseElement.FarVoltage)) ? HighsideTransformer.BaseElement.FarVoltage : (!double.IsNaN(HighsideTransformer.BaseElement.NearVoltage) && (!double.IsNaN(breakerVoltage)) && HighsideTransformer.BaseElement.NearVoltage > breakerVoltage) ? HighsideTransformer.BaseElement.NearVoltage : (!double.IsNaN(breakerVoltage)) ? breakerVoltage : KVolts;
                        }
                        else if (FoundTransformers.Count > 1)
                        {
                            FoundLevels.Add(WindingKV_Value);
                            WindingKVList.Add(XFW.BaseElement.KVLevel);
                            HighsideTransformerList.Add(new MM_OneLine_Element(OneLine.Descriptors[XF]) { Orientation = MM_OneLine_Element.enumOrientations.Right });
                            comboTransformer.Items.Add(XF.PrimaryWinding.Name);
                        }
                    }
                }
            if (HighsideTransformerList.Count > 0)
            {
                int idx = 0;
                foreach (MM_OneLine_Element TestElement in HighsideTransformerList)
                {
                    if (TestElement.BaseElement != null && TestElement.BaseElement.ElemType.Name == "Transformer")
                    {
                        VoltageLevel = FoundLevels[idx];
                        OtherWindingKV = WindingKVList[idx];
                        HighsideTransformer = TestElement;
                        comboTransformer.SelectedItem = (HighsideTransformer.BaseElement as MM_Transformer).PrimaryWinding.Name;
                        KVolts = (!double.IsNaN(HighsideTransformer.BaseElement.NearVoltage)) ? HighsideTransformer.BaseElement.NearVoltage : (!double.IsNaN(HighsideTransformer.BaseElement.FarVoltage)) ? HighsideTransformer.BaseElement.FarVoltage : (!double.IsNaN(breakerVoltage)) ? breakerVoltage : KVolts;
                        break;
                    }
                    idx++;
                }
            }
            this.TransformerRatio = VoltageLevel > 0 ? VoltageLevel / UnitKV : 1;

            if (comboCB.Items.Count != 0)
            {
                comboCB.SelectedIndex = 0;
                BaseUnit.OpenBreaker = (BaseUnit.Unit_Status == "Synchronized") ? false : true;
            }

            if (LocalOwner && !BaseUnit.RemovedStatus)
            {
                if (!BaseUnit.UnitStatus.Online && BaseUnit.Unit_Status == "Offline")
                {
                    btnStart.Text = "Start";
                    lblOnline.Text = "OFFLINE";
                    chkUnit.Visible = false;
                    chkIsoch.Visible = false;
                    btnStartIsoch.Enabled = false;
                    btnStartIsoch.Visible = false;
                    txtIsoch.Enabled = false;
                    txtIsoch.Visible = false;
                    txtTolIsoch.Enabled = false;
                    txtTolIsoch.Visible = false;
                    lblIsoch.Visible = false;
                    lblTolerance.Visible = false;
                    chkManualVoltageTarget.Visible = false;
                    comboPLC.Visible = false;
                    comboUnit.Visible = false;
                    btnDownRPM.Visible = false;
                    btnUpRPM.Visible = false;
                    btnDownVoltage.Visible = false;
                    btnUpVoltage.Visible = false;
                    btnToggleBreaker.Visible = false;
                    btnRamp.Visible = false;
                    lblPLC.Visible = false;
                    textKV.Visible = false;
                    textMW.Visible = false;
                    lblMW.Visible = false;
                    lblRampRate.Visible = false;
                    BaseUnit.UnitStatus.BaseVoltage = 0;
                }
                else if (BaseUnit.UnitStatus.Online && (BaseUnit.Unit_Status == "Online" || BaseUnit.Unit_Status == "Synchronized"))
                {
                    btnStart.Text = "Stop";
                    lblOnline.Text = BaseUnit.Unit_Status == "Online" ? "STARTED" : "ONLINE";
                    chkIsoch.Visible = true;
                    if (chkIsoch.Checked)
                    {
                        txtIsoch.Enabled = true;
                        txtIsoch.Visible = true;
                        txtTolIsoch.Visible = true;
                        lblIsoch.Visible = true;
                        lblTolerance.Visible = true;
                    }
                    chkManualVoltageTarget.Visible = true;
                    comboPLC.Visible = true;
                    if (comboPLC.Text.Equals("Local"))
                        comboUnit.Visible = true;
                    btnDownRPM.Visible = true;
                    btnUpRPM.Visible = true;
                    btnDownVoltage.Visible = true;
                    btnUpVoltage.Visible = true;
                    btnToggleBreaker.Visible = true;
                    lblPLC.Visible = true;
                    textKV.Visible = true;
                    textMW.Visible = true;
                    lblMW.Visible = true;
                    lblRampRate.Visible = true;
                    BaseUnit.UnitStatus.BaseVoltage = BaseUnit.Unit_Status == "Online" ? ((UnitKV * .8) <= BaseUnit.UnitStatus.BaseVoltage && BaseUnit.UnitStatus.BaseVoltage <= (UnitKV * 1.2)) ? BaseUnit.UnitStatus.BaseVoltage : (UnitKV * .8) : ((UnitKV * .8) <= (KVolts / TransformerRatio) && BaseUnit.UnitStatus.BaseVoltage <= (KVolts / TransformerRatio)) ? (KVolts / TransformerRatio) : BaseUnit.UnitStatus.BaseVoltage >= (KVolts / TransformerRatio) ? BaseUnit.UnitStatus.BaseVoltage : (UnitKV * .8);
                }
            }
            else
            {
                lblOnline.Text = (!LocalOwner) ? "IN USE" : "UNAVAILABLE";
                if (!LocalOwner)
                    tTip.SetToolTip(lblOnline, "Unit controlled by " + BaseUnit.UnitStatus.UnitController + " as of " + BaseUnit.UnitStatus.OwnershipStart);
                else if (BaseUnit.RemovedStatus)
                {
                    tTip.SetToolTip(lblOnline, "Removed unit status is set.\n Please double click on text to reset flag before you can open display");
                    BaseUnit.UnitStatus.UnitStatus = MacomberMapCommunications.Messages.EMS.MM_Unit_Control_Status.enumUnitStatus.Unavailable;
                }
                chkIsoch.Visible = false;
                chkUnit.Visible = false;
                btnStartIsoch.Visible = false;
                txtIsoch.Visible = false;
                txtTolIsoch.Visible = false;
                lblIsoch.Visible = false;
                lblTolerance.Visible = false;
                chkManualVoltageTarget.Visible = false;
                comboPLC.Visible = false;
                comboUnit.Visible = false;
                btnDownRPM.Visible = false;
                btnUpRPM.Visible = false;
                btnDownVoltage.Visible = false;
                btnUpVoltage.Visible = false;
                btnToggleBreaker.Visible = false;
                btnRamp.Visible = false;
                btnStart.Visible = false;
                lblStart.Visible = false;
                textKV.Visible = false;
                textMW.Visible = false;
                lblMW.Visible = false;
                lblRampRate.Visible = false;
            }

            mM_RPMGauge.NumberFormat = "0.0";
            mM_RPMGauge.AssignGauge(BaseUnit.UnitStatus.BaseRPM, 0, 4320);
            mM_FrqGauge.AssignGauge((mM_RPMGauge.Current / BaseUnit.NominalRPM) * 60.0, 58, 62);
            mM_FrqGauge.ErrorRangeMaximum = 62;
            mM_FrqGauge.ErrorRangeMinimum = 58;
            mM_LSVoltageGauge.AssignGauge(BaseUnit.UnitStatus.BaseVoltage, (float)(UnitKV * .8), (float)(UnitKV * 1.2));
            mM_LSVoltageGauge.ErrorRangeMaximum = mM_LSVoltageGauge.Maximum;
            mM_LSVoltageGauge.ErrorRangeMinimum = mM_LSVoltageGauge.Minimum;
            mM_LSVoltageGauge.WarningRangeMaximum = UnitKV * 1.05;
            mM_LSVoltageGauge.WarningRangeMinimum = UnitKV * .95;
            mM_LSVoltageGauge.GoodRangeMaximum = UnitKV * 1.03;
            mM_LSVoltageGauge.GoodRangeMinimum = UnitKV * .97;
            mM_LSVoltageGauge.NumberFormat = "0.00";
            mM_HSVoltageGauge.AssignGauge(mM_LSVoltageGauge.Current * TransformerRatio, (float)(VoltageLevel * .8), (float)(VoltageLevel * 1.2));
            mM_HSVoltageGauge.ErrorRangeMinimum = mM_HSVoltageGauge.Minimum;
            mM_HSVoltageGauge.ErrorRangeMaximum = mM_HSVoltageGauge.Maximum;
            mM_HSVoltageGauge.WarningRangeMaximum = VoltageLevel * 1.05;
            mM_HSVoltageGauge.WarningRangeMinimum = VoltageLevel * .95;
            mM_HSVoltageGauge.GoodRangeMaximum = VoltageLevel * 1.03;
            mM_HSVoltageGauge.GoodRangeMinimum = VoltageLevel * .97;
            mM_HSVoltageGauge.NumberFormat = "0.0";

            textKV.Text = (BaseUnit.OpenBreaker) ? mM_LSVoltageGauge.Current.ToString(mM_LSVoltageGauge.NumberFormat) : BaseUnit.VoltageTarget.ToString("0.00");
            textKV.BackColor = BaseUnit.ManVoltageTarg ? Color.Black : Color.White;
            textKV.ForeColor = BaseUnit.ManVoltageTarg ? Color.White : Color.Black;

            textMW.Text = BaseUnit.Desired_MW.ToString("0.0");
            txtIsoch.Text = BaseUnit.FreqTarget.ToString("0.00");
            txtTolIsoch.Text = BaseUnit.FreqToler.ToString("0.00");
            lblIsoch.Text = "Frequency : " + BaseUnit.FreqTarget.ToString("0.00");
            lblTolerance.Text += " : " + BaseUnit.FreqToler.ToString("0.00");
            lblRampRate.Text = "Target : " + BaseUnit.Desired_MW.ToString("0.0") + "  Rate : " + BaseUnit.RampRate.ToString("0.0");
            lblPLC.Text = "System : " + (BaseUnit.isAGC ? "AGC" : "Local") + " " + (BaseUnit.isAGC ? "" : BaseUnit.isLocal ? "Local" : "Fixed");

            comboPLC.Items.Add("Local");
            comboPLC.Items.Add("AGC");

            comboUnit.Items.Add("Local");
            comboUnit.Items.Add("Fixed");

            if (LocalOwner)
            {
                chkUnit.CheckedChanged -= chkUnit_CheckedChanged;
                chkUnit.Checked = BaseUnit.FrequencyControl;
                chkUnit.CheckedChanged += chkUnit_CheckedChanged;

                if (BaseUnit.NearIsland != null && BaseUnit.NearIsland.FrequencyControl && BaseUnit.FrequencyControl)
                {
                    chkIsoch.Checked = true;
                    btnStartIsoch.Text = "Stop";
                }
                else
                {
                    btnStartIsoch.Enabled = false;
                    btnStartIsoch.Visible = false;
                    txtIsoch.Visible = false;
                    txtTolIsoch.Visible = false;
                    lblIsoch.Visible = false;
                    lblTolerance.Visible = false;
                }

                if (BaseUnit.UnitStatus.Online || BaseUnit.Unit_Status != "Offline")
                {
                    chkManualVoltageTarget.Checked = BaseUnit.ManVoltageTarg;
                    chkManualVoltageTarget.Text += " : " + BaseUnit.VoltageTarget.ToString("0.00");

                    comboPLC.SelectedIndex = BaseUnit.isAGC ? 1 : 0;
                    comboUnit.SelectedIndex = BaseUnit.isLocal ? 0 : 1;
                }
            }

            MM_Server_Interface.PushUnitStatus(BaseUnit.UnitStatus);
        }

        /// <summary>
        /// Clean up our units from list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Generator_ControlPanel_Close(object sender, EventArgs e)
        {
            if (LocalOwner)
            {
                if (BaseUnit.UnitStatus.Online && BaseUnit.Unit_Status == "Online")
                    SendUnitValues("HZ", true);
                BaseUnit.UnitStatus.IsOwner = LocalOwner = false;
                BaseUnit.UnitStatus.UnitController = "";
                MM_Server_Interface.PushUnitStatus(BaseUnit.UnitStatus);
            }
        }

        /// <summary>
        /// When our unit value changes, refresh as needed
        /// </summary>
        /// <param name="Element"></param>
        /// <param name="Property"></param>
        private void BaseUnit_ValuesChanged(MM_Element Element, string Property)
        {
            pnlWaveform.Invalidate();
            pnlMVARCapability.Invalidate();
            pnlXF.Invalidate();
        }
        #endregion

        /// <summary>
        /// Allow an all operators, if they double-click, to free up a unit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblOnline_DoubleClick(object sender, EventArgs e)
        {
            DialogResult result;

            if (lblOnline.Text == "BREAKER")
            {
                result = MessageBox.Show("Breaker(s) are closed.\nPlease open breaker(s) before opening this display", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    this.ParentForm.Close();
                }
            }
            if (lblOnline.Text == "IN USE")
            {
                result = MessageBox.Show("Are you sure you want to free this unit so that you can operate it?", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    BaseUnit.UnitStatus.IsOwner = LocalOwner = false;
                    BaseUnit.UnitStatus.UnitController = "";
                    MM_Server_Interface.PushUnitStatus(BaseUnit.UnitStatus);
                    this.ParentForm.Close();
                }
            }
            else if (lblOnline.Text == "UNAVAILABLE")
            {
                result = MessageBox.Show("Are you sure you want to reset this unit so that you can operate it?", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    if (BaseUnit.OpenBreaker)
                    {
                        if (BaseUnit.RemovedStatus)
                            SendUnitValues("RM", true);
                        this.ParentForm.Close();
                    }
                    else
                    {
                        result = MessageBox.Show("Breaker(s) are closed.\nPlease open breaker(s) before trying to perform this action.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        if (result == System.Windows.Forms.DialogResult.OK)
                            this.ParentForm.Close();
                    }
                }
            }
        }

        /// <summary>
        /// When our timer triggers, invalidate the wave form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmrUpdate_Tick(object sender, EventArgs e)
        {
            pnlWaveform.Invalidate();
            pnlXF.Invalidate();
            BaseUnit.OpenBreaker = (BaseUnit.Unit_Status == "Synchronized") ? false : true;
            if (BaseUnit.OpenBreaker)
            {
                btnStartIsoch.Visible = false;
                if (BaseUnit.NearIsland != null && !BaseUnit.NearIsland.FrequencyControl)
                {
                    BaseUnit.UnitStatus.StartFrequencyControl = false;
                    btnStartIsoch.Text = "Start";
                    MM_Server_Interface.PushUnitStatus(BaseUnit.UnitStatus);
                }
                lblIsochFlag.Text = BaseUnit.FrequencyControl ? "FRQCTRL" : "";
                lblIsochFlag.Text += BaseUnit.NearIsland != null && BaseUnit.NearIsland.FrequencyControl ? " ISLFRQCTRL" : "";
                btnRamp.Visible = false;
                if (BaseUnit.UnitStatus.StartRamping)
                {
                    BaseUnit.UnitStatus.StartRamping = false;
                    btnRamp.Text = "Ramp";
                    MM_Server_Interface.PushUnitStatus(BaseUnit.UnitStatus);
                }
                if (BaseUnit.UnitStatus.Online && lblOnline.Text == "ONLINE")
                    lblOnline.Text = "STARTED";
                SendUnitValues("HZ", false);
            }
            else if (BaseUnit.UnitStatus.Online && lblOnline.Text == "STARTED")
                lblOnline.Text = "ONLINE";
            else if (BaseUnit.UnitStatus.StartRamping)
            {
                if (BaseUnit.Estimated_MW <= 1.01 * BaseUnit.Desired_MW && BaseUnit.Estimated_MW >= .99 * BaseUnit.Desired_MW)
                {
                    BaseUnit.UnitStatus.StartRamping = false;
                    btnRamp.Text = "Ramp";
                    MM_Server_Interface.PushUnitStatus(BaseUnit.UnitStatus);
                }
            }
            else
            {
                lblIsochFlag.Text = BaseUnit.FrequencyControl ? "FRQCTRL" : "";
                lblIsochFlag.Text += BaseUnit.NearIsland != null && BaseUnit.NearIsland.FrequencyControl ? " ISLFRQCTRL" : "";
                mM_HSVoltageGauge.Current = KVolts;
                if (chkManualVoltageTarget.Checked != BaseUnit.ManVoltageTarg)
                    chkManualVoltageTarget.Checked = BaseUnit.ManVoltageTarg;
            }

            if (LocalOwner)
            {
                if (BaseUnit.UnitStatus.Online)
                {
                    BaseUnit.UnitStatus.BaseVoltage = (mM_LSVoltageGauge.Minimum <= mM_LSVoltageGauge.Current) ? mM_LSVoltageGauge.Current : mM_LSVoltageGauge.Minimum;
                    MM_Server_Interface.PushUnitStatus(BaseUnit.UnitStatus);
                }
                else if (!BaseUnit.UnitStatus.Online && lblOnline.Text == "OFFLINE")
                {
                    BaseUnit.UnitStatus.BaseVoltage = 0;
                    MM_Server_Interface.PushUnitStatus(BaseUnit.UnitStatus);
                }
            }
            else
            {
                mM_LSVoltageGauge.Current = BaseUnit.UnitStatus.BaseVoltage;
                mM_HSVoltageGauge.Current = mM_LSVoltageGauge.Current * TransformerRatio;
                if (!BaseUnit.UnitStatus.Online)
                {
                    mM_RPMGauge.Current = BaseUnit.UnitStatus.BaseRPM;
                    mM_FrqGauge.Current = BaseUnit.UnitStatus.BaseRPM;
                }
            }

            if (BaseUnit.FrequencyControl && BaseUnit.NearIsland != null && BaseUnit.NearIsland.FrequencyControl)
            {
                btnStartIsoch.Text = "Stop";
            }
            else
            {
                if (btnStartIsoch.Visible)
                    btnStartIsoch.Text = "Start";
            }
        }

        #region Painting
        /// <summary>
        /// Paint our waveform
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pnlWaveform_Paint(object sender, PaintEventArgs e)
        {
            synchBreaker.DrawWaveform(e.Graphics, pnlWaveform.DisplayRectangle);
        }

        /// <summary>
        /// When we paint our MVAR capability curve, draw the curve
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pnlMVARCapability_Paint(object sender, PaintEventArgs e)
        {
            float Cap, Reac;

            BaseUnit.DetermineMVARCapabilities(out Cap, out Reac);
            BaseUnit.DrawUnitMVARCapabilityCurve(e.Graphics, pnlMVARCapability.DisplayRectangle, true);

            lblMWMin.Text = "MW Min     0";
            lblMWMax.Text = "MW Max     " + BaseUnit.MaxCapacity;
            lblMVARMin.Text = "MVAR Min  " + Cap.ToString("0");
            lblMVARMax.Text = "MVAR Max  " + Reac.ToString("0");
        }

        /// <summary>
        /// Draw our transformer icon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pnlXF_Paint(object sender, PaintEventArgs e)
        {
            float sysFreq = BaseUnit.Frequency;

            if (OtherWindingKV != null)
                try
                {
                    Point Center = new Point(pnlXF.DisplayRectangle.Width / 2, 20);
                    using (Pen DrawPen = new Pen(WindingKV.Energized.ForeColor))
                        e.Graphics.DrawLine(DrawPen, 0, Center.Y, Center.X - 8, Center.Y);
                    using (Pen DrawPen = new Pen(OtherWindingKV.Energized.ForeColor))
                        e.Graphics.DrawLine(DrawPen, Center.X + 8, Center.Y, DisplayRectangle.Right, Center.Y);
                    MM_OneLine_Element.DrawTransformerWinding(e.Graphics, new Rectangle(Center.X - 14, Center.Y - 12, 14, 15), Brushes.Black, MM_OneLine_Element.enumOrientations.Right, WindingKV.Energized.ForeColor, true, false);
                    MM_OneLine_Element.DrawTransformerWinding(e.Graphics, new Rectangle(Center.X + 2, Center.Y - 12, 14, 15), Brushes.Black, MM_OneLine_Element.enumOrientations.Left, OtherWindingKV.Energized.ForeColor, true, false);
                    using (StringFormat sF = new StringFormat() { Alignment = StringAlignment.Center })
                        e.Graphics.DrawString(HighsideTransformer.DescriptorText(OneLineViewer), this.Font, Brushes.White, Center.X, Center.Y + 15, sF);
                    KVolts = (!double.IsNaN(HighsideTransformer.BaseElement.NearVoltage) && (!double.IsNaN(HighsideTransformer.BaseElement.FarVoltage)) && HighsideTransformer.BaseElement.NearVoltage > HighsideTransformer.BaseElement.FarVoltage) ? HighsideTransformer.BaseElement.NearVoltage : (!double.IsNaN(HighsideTransformer.BaseElement.FarVoltage)) ? HighsideTransformer.BaseElement.FarVoltage : KVolts;
                    sysFreq = (!double.IsNaN(HighsideTransformer.BaseElement.NearFrequency) && (!double.IsNaN(HighsideTransformer.BaseElement.FarFrequency)) && HighsideTransformer.BaseElement.NearFrequency > HighsideTransformer.BaseElement.FarFrequency) ? HighsideTransformer.BaseElement.NearFrequency : (!double.IsNaN(HighsideTransformer.BaseElement.FarFrequency)) ? HighsideTransformer.BaseElement.FarFrequency : (comboCB.Items.Count != 0) ? !double.IsNaN(synchBreaker.BreakerSwitch.FarFrequency) ? synchBreaker.BreakerSwitch.FarFrequency : !double.IsNaN(synchBreaker.BreakerSwitch.NearFrequency) ? synchBreaker.BreakerSwitch.NearFrequency : 60 : 60;
                    lblSysVolt.ForeColor = OtherWindingKV.Energized.ForeColor;
                }
                catch (Exception ex)
                { }
            else
                sysFreq = (comboCB.Items.Count != 0) ? !double.IsNaN(synchBreaker.BreakerSwitch.FarFrequency) ? synchBreaker.BreakerSwitch.FarFrequency : !double.IsNaN(synchBreaker.BreakerSwitch.NearFrequency) ? synchBreaker.BreakerSwitch.NearFrequency : 60 : 60;
            lblSysVolt.Text = "System Voltage : " + KVolts.ToString("0.00") + " kV";
            lblSysMW.Text = "Actual MW :  " + BaseUnit.Estimated_MW.ToString("0.0") + "  MVAR :  " + BaseUnit.Estimated_MVAR.ToString("0.0");
            lblSysFreq.Text = "System Freq : " + sysFreq.ToString("0.000") + " Hz";
            lblRampRate.Text = "Target : " + BaseUnit.Desired_MW.ToString("0.0") + "  Rate : " + BaseUnit.RampRate.ToString("0.0");
            lblPLC.Text = "System : " + (BaseUnit.isAGC ? "AGC" : "Local") + " " + (BaseUnit.isAGC ? "" : BaseUnit.isLocal ? "Local" : "Fixed");
            lblIsoch.Text = "Frequency : " + BaseUnit.FreqTarget.ToString("0.00");
            lblTolerance.Text = "Tolerance : " + BaseUnit.FreqToler.ToString("0.00");
            chkManualVoltageTarget.Text = "Manual Voltage Target : " + BaseUnit.VoltageTarget.ToString("0.00");
        }
        #endregion

        #region ComboBoxes
        /// <summary>
        /// When our transformer index changes, set our new high side transformer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboTransformer_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                HighsideTransformer = BaseTransformers[comboTransformer.SelectedItem.ToString()];

                float NomKv = Convert.ToSingle(OneLineElement.xConfig.Attributes["BaseElement.KVLevel"].Value.Replace(" KV", ""));
                float W1Kv = Convert.ToSingle(HighsideTransformer.ParentElement.Windings[0].xConfig.Attributes["BaseElement.KVLevel"].Value.Replace(" KV", ""));
                float W2Kv = Convert.ToSingle(HighsideTransformer.ParentElement.Windings[1].xConfig.Attributes["BaseElement.KVLevel"].Value.Replace(" KV", ""));
                MM_OneLine_TransformerWinding XFw = HighsideTransformer.ParentElement.Windings[W2Kv > W1Kv ? 1 : 0];
                OtherWindingKV = XFw.KVLevel;
                VoltageLevel = Math.Max(W2Kv, W1Kv);
                TransformerRatio = VoltageLevel > 0 ? VoltageLevel / NomKv : 1;
                mM_HSVoltageGauge.AssignGauge(mM_LSVoltageGauge.Current * TransformerRatio, (float)(VoltageLevel * .8), (float)(VoltageLevel * 1.2));
                mM_HSVoltageGauge.ErrorRangeMinimum = mM_HSVoltageGauge.Minimum;
                mM_HSVoltageGauge.ErrorRangeMaximum = mM_HSVoltageGauge.Maximum;
                mM_HSVoltageGauge.WarningRangeMaximum = VoltageLevel * 1.05;
                mM_HSVoltageGauge.WarningRangeMinimum = VoltageLevel * .95;
                mM_HSVoltageGauge.GoodRangeMaximum = VoltageLevel * 1.03;
                mM_HSVoltageGauge.GoodRangeMinimum = VoltageLevel * .97;
                mM_HSVoltageGauge.NumberFormat = "0.0";
                pnlXF.Invalidate();
            }
            catch { }
        }

        private void comboPLC_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboPLC.Text.Equals("Local"))
            {
                double val;
                comboUnit.Visible = true;
                if (comboUnit.Text.Equals("Local"))
                {
                    chkIsoch.Visible = true;
                    chkUnit.Visible = true;
                    if (chkIsoch.Checked)
                    {
                        txtIsoch.Enabled = true;
                        txtIsoch.Visible = true;
                        txtTolIsoch.Visible = true;
                        lblIsoch.Visible = true;
                        lblTolerance.Visible = true;
                        txtIsoch.Text = BaseUnit.FreqTarget.ToString("0.00");
                        txtTolIsoch.Text = BaseUnit.FreqToler.ToString("0.00");
                        if (!String.IsNullOrEmpty(txtIsoch.Text) && !String.IsNullOrWhiteSpace(txtIsoch.Text) && double.TryParse(txtIsoch.Text, out val))
                        {
                            if (float.Parse(txtIsoch.Text) >= mM_FrqGauge.Minimum && float.Parse(txtIsoch.Text) < mM_FrqGauge.Maximum)
                            {
                                if (!String.IsNullOrEmpty(txtTolIsoch.Text) && !String.IsNullOrWhiteSpace(txtTolIsoch.Text) && double.TryParse(txtTolIsoch.Text, out val))
                                {
                                    if (float.Parse(txtTolIsoch.Text) > 0 && float.Parse(txtTolIsoch.Text) <= 1)
                                    {
                                        if (!BaseUnit.OpenBreaker)
                                        {
                                            btnStartIsoch.Visible = true;
                                            btnStartIsoch.Enabled = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    btnDownRPM.Visible = true;
                    btnUpRPM.Visible = true;
                }
                btnDownVoltage.Visible = true;
                btnUpVoltage.Visible = true;
                chkManualVoltageTarget.Visible = true;
                textKV.Visible = true;
                textMW.Visible = true;
                textMW.Text = BaseUnit.Desired_MW.ToString("0.0");
                lblMW.Visible = true;
                lblRampRate.Visible = true;
                if (!String.IsNullOrEmpty(textMW.Text) && !String.IsNullOrWhiteSpace(textMW.Text) && double.TryParse(textMW.Text, out val))
                {
                    if (!BaseUnit.OpenBreaker)
                    {
                        btnRamp.Visible = (float.Parse(textMW.Text) <= 0 || float.Parse(textMW.Text) > BaseUnit.MaxCapacity) ? false : true;
                    }
                }
                if (BaseUnit.isAGC)
                    SendUnitValues("LO", true);
                BaseUnit.UnitStatus.InAGC = false;
                MM_Server_Interface.PushUnitStatus(BaseUnit.UnitStatus);
            }
            else if (comboPLC.Text.Equals("AGC"))
            {
                comboUnit.Visible = false;
                if (comboUnit.Text.Equals("Local"))
                {
                    chkIsoch.Visible = false;
                    chkUnit.Visible = false;
                    btnStartIsoch.Visible = false;
                    txtIsoch.Visible = false;
                    txtTolIsoch.Visible = false;
                    lblIsoch.Visible = false;
                    lblTolerance.Visible = false;
                    btnDownRPM.Visible = false;
                    btnUpRPM.Visible = false;
                }
                btnDownVoltage.Visible = false;
                btnUpVoltage.Visible = false;
                btnRamp.Visible = false;
                textMW.Visible = false;
                lblMW.Visible = false;
                lblRampRate.Visible = false;
                if (!BaseUnit.isAGC)
                    SendUnitValues("LO", true);
                BaseUnit.UnitStatus.InAGC = true;
                MM_Server_Interface.PushUnitStatus(BaseUnit.UnitStatus);
            }
        }

        private void comboUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboUnit.Text.Equals("Local"))
            {
                double val;
                chkIsoch.Visible = true;
                chkUnit.Visible = true;
                if (chkIsoch.Checked)
                {
                    txtIsoch.Enabled = true;
                    txtIsoch.Visible = true;
                    txtTolIsoch.Visible = true;
                    lblIsoch.Visible = true;
                    lblTolerance.Visible = true;
                    txtIsoch.Text = BaseUnit.FreqTarget.ToString("0.00");
                    txtTolIsoch.Text = BaseUnit.FreqToler.ToString("0.00");
                    if (!String.IsNullOrEmpty(txtIsoch.Text) && !String.IsNullOrWhiteSpace(txtIsoch.Text) && double.TryParse(txtIsoch.Text, out val))
                    {
                        if (float.Parse(txtIsoch.Text) >= mM_FrqGauge.Minimum && float.Parse(txtIsoch.Text) < mM_FrqGauge.Maximum)
                        {
                            if (!String.IsNullOrEmpty(txtTolIsoch.Text) && !String.IsNullOrWhiteSpace(txtTolIsoch.Text) && double.TryParse(txtTolIsoch.Text, out val))
                            {
                                if (float.Parse(txtTolIsoch.Text) > 0 && float.Parse(txtTolIsoch.Text) <= 1)
                                {
                                    if (!BaseUnit.OpenBreaker)
                                    {
                                        btnStartIsoch.Visible = true;
                                        btnStartIsoch.Enabled = true;
                                    }
                                }
                            }
                        }
                    }
                }
                textMW.Text = BaseUnit.Desired_MW.ToString("0.0");
                if (!String.IsNullOrEmpty(textMW.Text) && !String.IsNullOrWhiteSpace(textMW.Text) && double.TryParse(textMW.Text, out val))
                {
                    if (!BaseUnit.OpenBreaker)
                    {
                        btnRamp.Visible = (float.Parse(textMW.Text) <= 0 || float.Parse(textMW.Text) > BaseUnit.MaxCapacity) ? false : true;
                    }
                }
                btnDownRPM.Visible = true;
                btnUpRPM.Visible = true;
                if (!BaseUnit.isLocal)
                    SendUnitValues("LL", true);
            }
            else if (comboUnit.Text.Equals("Fixed"))
            {
                chkIsoch.Visible = false;
                chkUnit.Visible = false;
                btnStartIsoch.Visible = false;
                txtIsoch.Visible = false;
                txtTolIsoch.Visible = false;
                lblIsoch.Visible = false;
                lblTolerance.Visible = false;
                btnDownRPM.Visible = false;
                btnUpRPM.Visible = false;
                btnRamp.Visible = false;
                if (BaseUnit.isLocal)
                    SendUnitValues("LL", true);
            }
        }
        #endregion

        #region Synchroscope
        /// <summary>
        /// Handle a breaker selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            MM_Breaker_Switch Breaker = Breakers[comboCB.SelectedItem.ToString()];
            MM_Element[] BreakerNodes = OneLineViewer.ElementNodes(OneLineViewer.DisplayElements[Breaker]);
            MM_Element[] UnitNodes = OneLineViewer.ElementNodes(OneLineElement);

            if (UnitNodes.Length == 0 || UnitNodes == null)
            {
                synchBreaker.AssignSynchroscope(Breaker, (MM_Node)BreakerNodes[0], (MM_Node)BreakerNodes[1]);
                if (LocalOwner)
                    synchBreaker.AssignSynchroscope(GeneratorBus, GeneratorIsland);
                else
                    synchBreaker.AssignSynchroscope(GeneratorBus, GeneratorIsland, BaseUnit.UnitStatus.OpenTime);
            }
            else if (BreakerNodes[0] == UnitNodes[0])
            {
                synchBreaker.AssignSynchroscope(Breaker, (MM_Node)BreakerNodes[0], (MM_Node)BreakerNodes[1]);
                if (LocalOwner)
                    synchBreaker.AssignSynchroscope(GeneratorBus, GeneratorIsland);
                else
                    synchBreaker.AssignSynchroscope(GeneratorBus, GeneratorIsland, BaseUnit.UnitStatus.OpenTime);
            }
            else
            {
                synchBreaker.AssignSynchroscope(Breaker, (MM_Node)BreakerNodes[1], (MM_Node)BreakerNodes[0]);
                if (LocalOwner)
                    synchBreaker.AssignSynchroscope(GeneratorBus, GeneratorIsland);
                else
                    synchBreaker.AssignSynchroscope(GeneratorBus, GeneratorIsland, BaseUnit.UnitStatus.OpenTime);
            }
            VoltageChanged(null, EventArgs.Empty);
        }

        /// <summary>
        /// Toggle our breaker
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnToggleBreaker_Click(object sender, EventArgs e)
        {
            if (comboCB.Items.Count == 0)
                return;
            else if (synchBreaker.PhaseDifferential[0] > synchBreaker.BreakerSwitch.MaxSynch - 10 || synchBreaker.PhaseDifferential[0] < synchBreaker.BreakerSwitch.MinSynch + 10)
            {
                Unit_Crashed("your phase angle difference was outside of 10°");
                return;
            }
            else if (KVolts > 0 && (mM_HSVoltageGauge.Current - KVolts > .05 * KVolts || KVolts - mM_HSVoltageGauge.Current > .05 * KVolts))
            {
                Unit_Crashed("your voltage was outside of 5% of the system voltage");
                return;
            }

            if (synchBreaker.BreakerSwitch.BreakerState != MM_Breaker_Switch.BreakerStateEnum.Unknown)
            {
                if (synchBreaker.BreakerSwitch.BreakerState == MM_Breaker_Switch.BreakerStateEnum.Open)
                {
                    BaseUnit.OpenBreaker = false;
                    SendUnitValues("HZ", true);
                    SendUnitValues("KV", true);
                    MM_Server_Interface.SendCommand("Open " + synchBreaker.BreakerSwitch.TEID.ToString() + "=" + (synchBreaker.BreakerSwitch.BreakerState == MM_Breaker_Switch.BreakerStateEnum.Open ? "F" : "T"), synchBreaker.BreakerSwitch.BreakerState == MM_Breaker_Switch.BreakerStateEnum.Open ? "T" : "F");
                    BaseUnit.UnitStatus.UnitStatus = MacomberMapCommunications.Messages.EMS.MM_Unit_Control_Status.enumUnitStatus.Online;
                }
                else if (synchBreaker.BreakerSwitch.BreakerState == MM_Breaker_Switch.BreakerStateEnum.Closed)
                {
                    DialogResult result = MessageBox.Show("Are you sure you want to open this Unit breaker?", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == System.Windows.Forms.DialogResult.Yes)
                    {
                        btnStartIsoch.Visible = false;
                        if (BaseUnit.NearIsland != null && BaseUnit.NearIsland.FrequencyControl)
                        {
                            BaseUnit.UnitStatus.StartFrequencyControl = false;
                            btnStartIsoch.Text = "Start";
                        }
                        btnRamp.Visible = false;
                        if (BaseUnit.UnitStatus.StartRamping)
                        {
                            BaseUnit.UnitStatus.StartRamping = false;
                            btnRamp.Text = "Ramp";
                        }
                        MM_Server_Interface.PushUnitStatus(BaseUnit.UnitStatus);
                        MM_Server_Interface.SendCommand("Set OPEN " + synchBreaker.BreakerSwitch.TEID.ToString() + "=" + (synchBreaker.BreakerSwitch.BreakerState == MM_Breaker_Switch.BreakerStateEnum.Open ? "F" : "T"), synchBreaker.BreakerSwitch.BreakerState == MM_Breaker_Switch.BreakerStateEnum.Open ? "T" : "F");
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// Send our unit values
        /// </summary>
        /// <param name="ValType"></param>
        /// <param name="SendCommand"></param>
        private void SendUnitValues(String ValType, bool SendCommand)
        {
            //TODO: Handle our request to send unit values
            
        }

        /// <summary>
        /// When our frequency changes, update accordingly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FreqChanged(object sender, EventArgs e)
        {
            GeneratorIsland.Frequency = (float)mM_FrqGauge.Current;
        }

        /// <summary>
        /// Ramp up our unit
        /// </summary>
        /// <param name="state"></param>
        private void RampUpUnit(object state)
        {
            //Ramp up our unit over 90 seconds
            int RestMS = 50;
            double TargetRPM = (58.2 / 60.0) * BaseUnit.NominalRPM;
            double RampStep = TargetRPM / (90.0 * 1000.0 / RestMS);
            while (mM_RPMGauge.Current < TargetRPM)
            {
                SetRPM(BaseUnit.UnitStatus.BaseRPM + RampStep, TargetRPM);
                if (BaseUnit.UnitStatus.BaseRPM >= TargetRPM || !BaseUnit.UnitStatus.Online)
                    break;
                Thread.Sleep(RestMS);
            }
            if (BaseUnit.UnitStatus.BaseRPM < TargetRPM)
                SetRPM(TargetRPM, TargetRPM);
        }

        private delegate void SafeSetRPM(double RPM, double TargetRPM);

        /// <summary>
        /// Set our RPM value
        /// </summary>
        /// <param name="RPM"></param>
        /// <param name="TargetRPM"></param>
        private void SetRPM(double RPM, double TargetRPM)
        {
            if (IsDisposed)
                return;
            else if (InvokeRequired)
                Invoke(new SafeSetRPM(SetRPM), RPM, TargetRPM);
            else
            {
                BaseUnit.UnitStatus.BaseRPM = RPM;
                if (BaseUnit.UnitStatus.BaseRPM >= TargetRPM)
                {
                    if (BaseUnit.UnitStatus.Online && lblOnline.Text == "STARTING")
                    {
                        lblOnline.Text = "STARTED";
                        chkUnit.Visible = true;
                        chkIsoch.Visible = true;
                        if (chkIsoch.Checked)
                        {
                            txtIsoch.Enabled = true;
                            txtIsoch.Visible = true;
                            txtTolIsoch.Visible = true;
                            lblIsoch.Visible = true;
                            lblTolerance.Visible = true;
                        }
                        chkManualVoltageTarget.Visible = true;
                        comboPLC.Visible = true;
                        if (comboPLC.Text.Equals("Local"))
                            comboUnit.Visible = true;
                        comboPLC.SelectedIndex = BaseUnit.isAGC ? 1 : 0;
                        comboUnit.SelectedIndex = BaseUnit.isLocal ? 0 : 1;
                        btnDownRPM.Visible = true;
                        btnUpRPM.Visible = true;
                        btnDownVoltage.Visible = true;
                        btnUpVoltage.Visible = true;
                        btnToggleBreaker.Visible = true;
                        lblPLC.Visible = true;
                        textKV.Visible = true;
                        textMW.Visible = true;
                        lblMW.Visible = true;
                        lblRampRate.Visible = true;
                        lblSysMW.Visible = true;
                        SendUnitValues("HZ", true);
                    }
                }
            }
        }

        /// <summary>
        /// Handle our update to our current RPM
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmrRPM_Tick(object sender, EventArgs e)
        {
            //First, pull a little bit of acceleration off
            if (BaseUnit.UnitStatus.Online)
            {
                RPMAcceleration += Math.Sign(RPMAcceleration) * -Math.Min(.20, Math.Abs(RPMAcceleration));
                BaseUnit.UnitStatus.BaseRPM += RPMAcceleration;
                double Pos = ((DateTime.Now - BaseUnit.UnitStatus.OpenTime).TotalMilliseconds / 50.0) % 360.0;
                mM_RPMGauge.Current = BaseUnit.UnitStatus.BaseRPM + (1 + RPMAcceleration) * Math.Sin(Pos * Math.PI / 180.0);
                mM_FrqGauge.Current = (BaseUnit.Unit_Status == "Synchronized") ? BaseUnit.Frequency : (mM_RPMGauge.Current / BaseUnit.NominalRPM) * 60.0;
                if (mM_FrqGauge.Current >= mM_FrqGauge.Maximum)
                    Unit_Crashed("your frequency was too high and exceeded the maximum");
            }
        }

        /// <summary>
        /// When our unit crashes, refresh values as needed
        /// </summary>
        private void Unit_Crashed(String reason)
        {
            if (BaseUnit.UnitStatus.Online && LocalOwner)
            {
                if (comboCB.Items.Count != 0)
                    comboCB.SelectedIndex = 0;
                comboPLC.SelectedIndex = 0;
                comboUnit.SelectedIndex = 0;
                RPMAcceleration = 0;
                BaseUnit.UnitStatus.BaseRPM = 0;
                mM_RPMGauge.Current = 0;
                mM_FrqGauge.Current = mM_RPMGauge.Current;
                mM_LSVoltageGauge.Current = 0;
                mM_HSVoltageGauge.Current = mM_LSVoltageGauge.Current;
                textKV.Text = mM_LSVoltageGauge.Current.ToString(mM_LSVoltageGauge.NumberFormat);
                BaseUnit.UnitStatus.Online = false;
                BaseUnit.UnitStatus.UnitStatus = MacomberMapCommunications.Messages.EMS.MM_Unit_Control_Status.enumUnitStatus.Tripped;
                btnStart.Text = "Start";
                lblOnline.Text = "TRIPPED";
                SendUnitValues("ST", true);
                chkIsoch.Visible = false;
                chkIsoch.Checked = false;
                chkUnit.Visible = false;
                btnStartIsoch.Visible = false;
                txtIsoch.Visible = false;
                txtTolIsoch.Visible = false;
                lblIsoch.Visible = false;
                lblTolerance.Visible = false;
                chkManualVoltageTarget.Visible = false;
                comboPLC.Visible = false;
                comboUnit.Visible = false;
                btnDownRPM.Visible = false;
                btnUpRPM.Visible = false;
                btnDownVoltage.Visible = false;
                btnUpVoltage.Visible = false;
                btnToggleBreaker.Visible = false;
                btnRamp.Visible = false;
                lblPLC.Visible = false;
                textKV.Visible = false;
                textMW.Visible = false;
                lblMW.Visible = false;
                lblRampRate.Visible = false;
                lblSysMW.Visible = false;
                tmrButton.Enabled = false;
                DialogResult result = MessageBox.Show("Oops! You have tripped this Unit. The Unit tripped because " + reason, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    result = MessageBox.Show("Unit tripped. Please wait while we take Unit OFFLINE for repairs.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        lblOnline.Text = "IN SERVICE";
                        result = MessageBox.Show("Starting repairs. Please wait until we finish repairs. We will inform you when repairs have completed. Do NOT close any windows.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        if (result == System.Windows.Forms.DialogResult.OK)
                            result = MessageBox.Show("Almost done with repairs. Please be patient.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        if (result == System.Windows.Forms.DialogResult.OK)
                            result = MessageBox.Show("All repairs have been made. You can now restart Unit.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        if (result == System.Windows.Forms.DialogResult.OK)
                        {
                            if (!BaseUnit.OpenBreaker)
                            {
                                btnStart.Visible = false;
                                lblStart.Visible = false;
                                lblOnline.Text = "BREAKER";
                            }
                            else
                            {
                                btnStart.Visible = true;
                                lblStart.Visible = true;
                                lblOnline.Text = "OFFLINE";
                            }
                        }
                    }
                }
                MM_Server_Interface.PushUnitStatus(BaseUnit.UnitStatus);
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            BaseUnit.UnitStatus.BaseRPM = 0;
            RPMAcceleration = 0;
            mM_RPMGauge.Current = 0;
            mM_FrqGauge.Current = 0;
            if (BaseUnit.UnitStatus.Online)
            {
                if (comboCB.Items.Count != 0)
                    comboCB.SelectedIndex = 0;
                comboPLC.SelectedIndex = 0;
                comboUnit.SelectedIndex = 0;
                mM_LSVoltageGauge.Current = 0;
                mM_HSVoltageGauge.Current = mM_LSVoltageGauge.Current;
                textKV.Text = mM_LSVoltageGauge.Current.ToString(mM_LSVoltageGauge.NumberFormat);
                BaseUnit.UnitStatus.Online = false;
                BaseUnit.UnitStatus.UnitStatus = MacomberMapCommunications.Messages.EMS.MM_Unit_Control_Status.enumUnitStatus.Offline;
                SendUnitValues("ST", true);
                btnStart.Text = "Start";
                if (!BaseUnit.OpenBreaker)
                {
                    btnStart.Visible = false;
                    lblStart.Visible = false;
                    lblOnline.Text = "BREAKER";
                }
                else
                {
                    btnStart.Visible = true;
                    lblStart.Visible = true;
                    lblOnline.Text = "OFFLINE";
                }
                chkUnit.Visible = false;
                chkIsoch.Visible = false;
                chkIsoch.Checked = false;
                btnStartIsoch.Visible = false;
                txtIsoch.Visible = false;
                txtTolIsoch.Visible = false;
                lblIsoch.Visible = false;
                lblTolerance.Visible = false;
                chkManualVoltageTarget.Visible = false;
                comboPLC.Visible = false;
                comboUnit.Visible = false;
                btnDownRPM.Visible = false;
                btnUpRPM.Visible = false;
                btnDownVoltage.Visible = false;
                btnUpVoltage.Visible = false;
                btnToggleBreaker.Visible = false;
                btnRamp.Visible = false;
                lblPLC.Visible = false;
                textKV.Visible = false;
                textMW.Visible = false;
                lblMW.Visible = false;
                lblRampRate.Visible = false;
                lblSysMW.Visible = false;
                MM_Server_Interface.PushUnitStatus(BaseUnit.UnitStatus);
            }
            else
            {
                mM_LSVoltageGauge.Current = mM_LSVoltageGauge.Minimum;
                mM_HSVoltageGauge.Current = mM_LSVoltageGauge.Current * TransformerRatio;
                textKV.Text = mM_LSVoltageGauge.Current.ToString(mM_LSVoltageGauge.NumberFormat);
                BaseUnit.UnitStatus.Online = true;
                BaseUnit.UnitStatus.UnitStatus = MacomberMapCommunications.Messages.EMS.MM_Unit_Control_Status.enumUnitStatus.Started;
                btnStart.Text = "Stop";
                lblOnline.Text = "STARTING";
                ThreadPool.QueueUserWorkItem(new WaitCallback(RampUpUnit));
            }
            MM_Server_Interface.PushUnitStatus(BaseUnit.UnitStatus);
        }

        #region Up/down button controls
        /// <summary>
        /// Handle the button pressing down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Control_MouseDown(object sender, MouseEventArgs e)
        {
            RPMAcceleration = 0;
            //Set up our timer, and trigger.
            tmrButton.Tag = sender;
            tmrButton.Interval = 50;
            tmrButton_Tick(sender, e);
            tmrButton.Enabled = true;
        }

        /// <summary>
        /// Handle our button up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Control_MouseUp(object sender, MouseEventArgs e)
        {
            tmrButton.Enabled = false;
        }

        /// <summary>
        /// Handle our timer tick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmrButton_Tick(object sender, EventArgs e)
        {
            if (tmrButton.Tag == btnDownRPM && mM_RPMGauge.Current > mM_RPMGauge.Minimum)
                RPMAcceleration = Math.Min(RPMAcceleration - .15, -.5);
            else if (tmrButton.Tag == btnUpRPM && mM_RPMGauge.Current < mM_RPMGauge.Maximum)
                RPMAcceleration = Math.Max(RPMAcceleration + .15, .5);
            else if (tmrButton.Tag == btnDownVoltage && mM_LSVoltageGauge.Current > mM_LSVoltageGauge.Minimum)
            {
                mM_LSVoltageGauge.Current -= 0.01;
                mM_HSVoltageGauge.Current = (BaseUnit.OpenBreaker) ? mM_LSVoltageGauge.Current * TransformerRatio : KVolts;
                textKV.Text = (BaseUnit.OpenBreaker) ? mM_LSVoltageGauge.Current.ToString() : BaseUnit.VoltageTarget.ToString("0.00");
            }
            else if (tmrButton.Tag == btnUpVoltage && mM_LSVoltageGauge.Current < mM_LSVoltageGauge.Maximum)
            {
                mM_LSVoltageGauge.Current += 0.01;
                mM_HSVoltageGauge.Current = (BaseUnit.OpenBreaker) ? mM_LSVoltageGauge.Current * TransformerRatio : KVolts;
                textKV.Text = (BaseUnit.OpenBreaker) ? mM_LSVoltageGauge.Current.ToString() : BaseUnit.VoltageTarget.ToString("0.00");
            }
        }
        #endregion

        #region Voltage
        /// <summary>
        /// When our voltage changes, update accordingly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VoltageChanged(object sender, EventArgs e)
        {
            if (OneLineElement != null && synchBreaker.BreakerSwitch != null)
            {
                if (OneLineElement.KVLevel == synchBreaker.BreakerSwitch.KVLevel)
                    GeneratorBus.Estimated_kV = (float)mM_LSVoltageGauge.Current;
                else if (OneLineElement.KVLevel != synchBreaker.BreakerSwitch.KVLevel)
                    GeneratorBus.Estimated_kV = (float)mM_HSVoltageGauge.Current;
            }
        }

        private void textKV_TextChanged(object sender, KeyPressEventArgs e)
        {
            double val;

            if (e.KeyChar == '\r' || e.KeyChar == '\n')
            {
                if (!String.IsNullOrEmpty(textKV.Text) && !String.IsNullOrWhiteSpace(textKV.Text) && double.TryParse(textKV.Text, out val))
                {
                    if (float.Parse(textKV.Text) >= mM_LSVoltageGauge.Minimum && float.Parse(textKV.Text) <= mM_LSVoltageGauge.Maximum)
                    {
                        mM_LSVoltageGauge.Current = (BaseUnit.OpenBreaker) ? float.Parse(textKV.Text) : mM_LSVoltageGauge.Current;
                        mM_HSVoltageGauge.Current = (BaseUnit.OpenBreaker) ? mM_LSVoltageGauge.Current * TransformerRatio : KVolts;
                        if (!BaseUnit.OpenBreaker)
                            SendUnitValues("MAKV", true);
                    }
                }
                else
                {
                    textKV.Text = (BaseUnit.OpenBreaker) ? mM_LSVoltageGauge.Current.ToString() : BaseUnit.VoltageTarget.ToString("0.00");
                }
            }
        }

        private void chkManualVoltageTarget_CheckedChanged(object sender, EventArgs e)
        {
            if (chkManualVoltageTarget.Checked)
            {
                textKV.BackColor = Color.White;
                textKV.ForeColor = Color.Black;
                textKV.Enabled = true;
            }
            else
            {
                textKV.BackColor = Color.Black;
                textKV.ForeColor = Color.White;
                textKV.Enabled = false;
            }
            if (chkManualVoltageTarget.Checked != BaseUnit.ManVoltageTarg)
                SendUnitValues("AUKV", true);
        }
        #endregion

        #region MW/MVR
        private void btnRamp_Click(object sender, EventArgs e)
        {
            if (BaseUnit.UnitStatus.StartRamping)
            {
                BaseUnit.UnitStatus.StartRamping = false;
                btnRamp.Text = "Ramp";
            }
            else
            {
                double val;
                if (!String.IsNullOrEmpty(BaseUnit.Desired_MW.ToString("0.0")) && !String.IsNullOrWhiteSpace(BaseUnit.Desired_MW.ToString("0.0")) && double.TryParse(BaseUnit.Desired_MW.ToString("0.0"), out val))
                {
                    if (BaseUnit.Desired_MW > 0 && BaseUnit.Desired_MW <= BaseUnit.MaxCapacity)
                    {
                        BaseUnit.UnitStatus.StartRamping = true;
                        btnRamp.Text = "Ramping";
                        SendUnitValues("RAMP", true);
                    }
                }
            }
            MM_Server_Interface.PushUnitStatus(BaseUnit.UnitStatus);
        }

        private void textMW_TextChanged(object sender, KeyPressEventArgs e)
        {
            double val;

            if (e.KeyChar == '\r' || e.KeyChar == '\n')
            {
                if (!String.IsNullOrEmpty(textMW.Text) && !String.IsNullOrWhiteSpace(textMW.Text) && double.TryParse(textMW.Text, out val))
                {
                    if (!BaseUnit.OpenBreaker && comboPLC.Text.Equals("Local") && comboUnit.Text.Equals("Local"))
                    {
                        btnRamp.Visible = (float.Parse(textMW.Text) <= 0 || float.Parse(textMW.Text) > BaseUnit.MaxCapacity) ? false : true;
                        SendUnitValues("MW", true);
                    }
                }
                else
                {
                    textMW.Text = BaseUnit.Desired_MW.ToString("0.0");
                }
            }
        }
        #endregion

        #region Isochronous
        private void textIsoch_TextChanged(object sender, KeyPressEventArgs e)
        {
            double val;

            if (e.KeyChar == '\r' || e.KeyChar == '\n')
            {
                if (!String.IsNullOrEmpty(txtIsoch.Text) && !String.IsNullOrWhiteSpace(txtIsoch.Text) && double.TryParse(txtIsoch.Text, out val))
                {
                    if (float.Parse(txtIsoch.Text) >= mM_FrqGauge.Minimum && float.Parse(txtIsoch.Text) < mM_FrqGauge.Maximum)
                    {
                        txtTolIsoch.Enabled = true;
                        if (!String.IsNullOrEmpty(txtTolIsoch.Text) && !String.IsNullOrWhiteSpace(txtTolIsoch.Text) && double.TryParse(txtTolIsoch.Text, out val))
                        {
                            if (float.Parse(txtTolIsoch.Text) > 0 && float.Parse(txtTolIsoch.Text) <= 1)
                            {
                                if (!BaseUnit.OpenBreaker)
                                {
                                    btnStartIsoch.Visible = true;
                                    btnStartIsoch.Enabled = true;
                                    SendUnitValues("ISCH", true);
                                }
                            }
                        }
                    }
                    else
                    {
                        txtIsoch.Text = BaseUnit.FreqTarget.ToString("0.00");
                    }
                }
                else
                {
                    txtIsoch.Text = BaseUnit.FreqTarget.ToString("0.00");
                }
            }
        }

        private void txtTolIsoch_TextChanged(object sender, KeyPressEventArgs e)
        {
            double val;

            if (e.KeyChar == '\r' || e.KeyChar == '\n')
            {
                if (!String.IsNullOrEmpty(txtTolIsoch.Text) && !String.IsNullOrWhiteSpace(txtTolIsoch.Text) && double.TryParse(txtTolIsoch.Text, out val))
                {
                    if (float.Parse(txtTolIsoch.Text) > 0 && float.Parse(txtTolIsoch.Text) <= 1)
                    {
                        if (float.Parse(txtIsoch.Text) >= mM_FrqGauge.Minimum && float.Parse(txtIsoch.Text) < mM_FrqGauge.Maximum)
                        {
                            if (!BaseUnit.OpenBreaker)
                            {
                                btnStartIsoch.Visible = true;
                                btnStartIsoch.Enabled = true;
                                SendUnitValues("ISCH", true);
                            }
                        }
                    }
                    else
                    {
                        txtTolIsoch.Text = BaseUnit.FreqToler.ToString("0.00");
                    }
                }
                else
                {
                    txtTolIsoch.Text = BaseUnit.FreqToler.ToString("0.00");
                }
            }
        }

        private void btnStartIsoch_Click(object sender, EventArgs e)
        {
            if (BaseUnit.NearIsland != null && BaseUnit.NearIsland.FrequencyControl)
            {
                BaseUnit.UnitStatus.StartFrequencyControl = false;
                btnStartIsoch.Text = "Start";
                SendUnitValues("ISOC", true);
            }
            else
            {
                double val;
                if (!String.IsNullOrEmpty(BaseUnit.FreqTarget.ToString("0.0")) && !String.IsNullOrWhiteSpace(BaseUnit.FreqTarget.ToString("0.0")) && double.TryParse(BaseUnit.FreqTarget.ToString("0.0"), out val))
                {
                    if (!String.IsNullOrEmpty(BaseUnit.FreqToler.ToString("0.00")) && !String.IsNullOrWhiteSpace(BaseUnit.FreqToler.ToString("0.00")) && double.TryParse(BaseUnit.FreqToler.ToString("0.00"), out val))
                    {
                        if (BaseUnit.FreqTarget >= mM_FrqGauge.Minimum && BaseUnit.FreqTarget < mM_FrqGauge.Maximum)
                        {
                            if (BaseUnit.FreqToler > 0 && BaseUnit.FreqToler <= 1)
                            {
                                BaseUnit.UnitStatus.StartFrequencyControl = true;
                                btnStartIsoch.Text = "Stop";
                                SendUnitValues("ISOC", true);
                            }
                        }
                    }
                }
            }
            MM_Server_Interface.PushUnitStatus(BaseUnit.UnitStatus);
        }

        private void chkIsoch_CheckedChanged(object sender, EventArgs e)
        {
            if (!chkIsoch.Checked)
            {
                btnStartIsoch.Enabled = false;
                btnStartIsoch.Visible = false;
                txtIsoch.Enabled = false;
                txtIsoch.Visible = false;
                txtTolIsoch.Enabled = false;
                txtTolIsoch.Visible = false;
                lblIsoch.Visible = false;
                lblTolerance.Visible = false;
            }
            else
            {
                double val;
                txtIsoch.Enabled = true;
                txtIsoch.Visible = true;
                txtTolIsoch.Visible = true;
                lblIsoch.Visible = true;
                lblTolerance.Visible = true;
                txtIsoch.Text = BaseUnit.FreqTarget.ToString("0.00");
                txtTolIsoch.Text = BaseUnit.FreqToler.ToString("0.00");
                if (!String.IsNullOrEmpty(txtIsoch.Text) && !String.IsNullOrWhiteSpace(txtIsoch.Text) && double.TryParse(txtIsoch.Text, out val))
                {
                    if (float.Parse(txtIsoch.Text) >= mM_FrqGauge.Minimum && float.Parse(txtIsoch.Text) < mM_FrqGauge.Maximum)
                    {
                        if (!String.IsNullOrEmpty(txtTolIsoch.Text) && !String.IsNullOrWhiteSpace(txtTolIsoch.Text) && double.TryParse(txtTolIsoch.Text, out val))
                        {
                            txtTolIsoch.Enabled = true;
                            if (float.Parse(txtTolIsoch.Text) > 0 && float.Parse(txtTolIsoch.Text) <= 1)
                            {
                                if (!BaseUnit.OpenBreaker)
                                {
                                    btnStartIsoch.Visible = true;
                                    btnStartIsoch.Enabled = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void chkUnit_CheckedChanged(object sender, EventArgs e)
        {
            BaseUnit.UnitStatus.CheckedFrequencyControl = chkUnit.Checked;
            SendUnitValues("UNFR", true);
            MM_Server_Interface.PushUnitStatus(BaseUnit.UnitStatus);
        }
        #endregion
    }
}
