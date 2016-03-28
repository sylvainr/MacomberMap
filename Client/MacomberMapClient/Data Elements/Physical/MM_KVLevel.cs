using MacomberMapClient.Data_Elements.Display;
using MacomberMapClient.Integration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MacomberMapClient.User_Interfaces.Generic;
using MacomberMapCommunications.Extensions;

namespace MacomberMapClient.Data_Elements.Physical
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// Store display information on a particular KV level
    /// </summary>
    public class MM_KVLevel : MM_Element //MM_Serializable, IComparable
    {
        #region Variable declarations

        /// <summary>The display parameters for energized state</summary>        
        public MM_DisplayParameter Energized;

        /// <summary>The display parameters for energized state</summary>
        public MM_DisplayParameter PartiallyEnergized;

        /// <summary>The display parameters for an unknown state</summary>
        public MM_DisplayParameter Unknown;

        /// <summary>The display parameters for deenergized state</summary>
        public MM_DisplayParameter DeEnergized;

        /// <summary>Our handler for KV-level monitoring</summary>
        public static event EventHandler MonitoringChanged;

        /// <summary>The integer value of the KV level (used by the SQL-based versions of the CIM server)</summary>
        public int Index;

        private bool _MonitorEquipment = true;
        private bool ComputedNominal = false;
        private bool _ShowMVA = true;
        private float _Nominal = 0f;
        private float _ThermalWarning = .95f;
        private float _ThermalAlert = .99f;
        private float _LowVoltageAlert = .9f;
        private float _LowVoltageWarning = .95f;
        private float _HighVoltageWarning = 1.05f;
        private float _HighVoltageAlert = 1.1f;
        private float _MVASize = 1.5f;

        /// <summary>
        /// The zoom levels at which equipment of this type is visible
        /// </summary>
        [Category(" Visibility"), Description("The zoom levels at which equipment of this type is visible"), DefaultValue(true)]
        public int VisibilityByZoom { get; set; }


        /// <summary>
        /// Whether buses of this KV level are shown on the network map
        /// </summary>
        [Category(" Visibility"), Description("The zoom level at which MW levels (e.g., load, gen) are shown"), DefaultValue(true)]
        public int StationMW { get; set; }

        /// <summary>
        /// Whether buses of this KV level are shown on the network map
        /// </summary>
        [Category(" Visibility"), Description("The zoom level at which station names are shown"), DefaultValue(true)]
        public int StationNames { get; set; }

        /// <summary>Our nominal voltage level</summary>
        public float Nominal
        {
            get
            {
                if (!ComputedNominal)
                {
                    ComputedNominal = true;
                    float.TryParse(Name.Trim().Split(' ')[0].Trim(), out _Nominal);
                }
                return _Nominal;
            }
        }

        /// <summary>
        /// Whether buses of this KV level are shown on the network map
        /// </summary>
        [Category(" Visibility"), Description("Whether buses of this KV level are shown on the network map"), DefaultValue(true)]
        public bool ShowOnMap { get { return _ShowOnMap; } set { _ShowOnMap = value; } }
        private bool _ShowOnMap = true;

        /// <summary>
        /// Whether buses of this KV level are shown in menus
        /// </summary>
        [Category(" Visibility"), Description("Whether buses of this KV level are shown in menus"), DefaultValue(true)]
        public bool ShowInMenu { get; set; }

        /// <summary>
        /// Show the full routing of transmission lines
        /// </summary>
        [Category(" Visibility"), Description("Show the full routing of transmission lines"), DefaultValue(true)]
        public bool ShowLineRouting { get; set; }

        /// <summary>
        /// Show energized lines in the network map
        /// </summary>
        [Category(" Visibility"), Description("Show energized lines in the network map"), DefaultValue(true)]
        public bool ShowEnergized { get; set; }

        /// <summary>
        /// Show unknown-state lines in the network map
        /// </summary>
        [Category(" Visibility"), Description("Show unknown-state lines in the network map"), DefaultValue(true)]
        public bool ShowUnknown { get; set; }

        /// <summary>
        /// Show partially energized lines in the network map
        /// </summary>
        [Category(" Visibility"), Description("Show partially energized lines in the network map"), DefaultValue(true)]
        public bool ShowPartiallyEnergized { get; set; }

        /// <summary>
        /// Show normally opened lines in the network map
        /// </summary>
        [Category(" Visibility"), Description("Show normally opened lines the network map"), DefaultValue(true)]
        public bool ShowNormalOpened { get; set; }

        /// <summary>
        /// Show de-energized lines in the network map
        /// </summary>
        [Category(" Visibility"), Description("Show de-energized lines in the network map"), DefaultValue(true)]
        public bool ShowDeEnergized { get; set; }

        /// <summary>
        /// Show the overall visibility of this KV level
        /// </summary>
        [Category(" Visibility"), Description("Show the overall visibility of this KV level"), DefaultValue(true), ReadOnly(true)]
        public bool SimpleVisibility
        {
            get { return ShowEnergized | ShowDeEnergized | ShowPartiallyEnergized | ShowUnknown; }
            set { ShowEnergized = ShowDeEnergized = ShowPartiallyEnergized = ShowUnknown = value; }
        }

        /// <summary>
        /// Show MVA flow in the network map
        /// </summary>
        [Category("MVA Animation"), Description("Show MVA flow in the network map"), DefaultValue(false)]
        public bool ShowMVA { get { return _ShowMVA; } set { _ShowMVA = value; } }

        /// <summary>
        /// The threshold (in percent) at or over which the MVA flow should be shown
        /// </summary>
        [Category("MVA Animation"), Description("The threshold (in percent) at or over which the MVA flow should be shown"), DefaultValue(0f)]
        public float MVAThreshold { get; set; }

        /// <summary>
        /// The threshold (in percent) at or over which the MVA flow should be shown
        /// </summary>
        [Category("Visibility"), Description("The threshold (in percent) at or over which the line should be shown"), DefaultValue(0f)]
        public float VisibilityThreshold { get; set; }

        /// <summary>
        /// The base size of an MVA flow line at 100% of normal limit
        /// </summary>
        [Category("MVA Animation"), Description("The base size of an MVA flow line at 100% of normal limit")]
        public float MVASize
        {
            get { return _MVASize; }
            set {_MVASize = value;}
        }

        /// <summary>
        /// Whether the MW flow and direction should be displayed above the line
        /// </summary>
        [Category("Status text"), Description("Whether the MW flow and direction should be displayed above the line"), DefaultValue(false)]
        public bool ShowMWText { get; set; }

        /// <summary>
        /// Whether the line's name should be displayed
        /// </summary>
        [Category("Status text"), Description("Whether the line's name should be displayed"), DefaultValue(false)]
        public bool ShowLineName { get; set; }

        /// <summary>
        /// Whether the MVAR flow and direction should be displayed above the line
        /// </summary>
        [Category("Status text"), Description("Whether the MVAR flow and direction should be displayed above the line"), DefaultValue(false)]
        public bool ShowMVARText { get; set; }

        /// <summary>
        /// Whether the MVA flow and direction should be displayed above the line
        /// </summary>
        [Category("Status text"), Description("Whether the MVA flow and direction should be displayed above the line"), DefaultValue(false)]
        public bool ShowMVAText { get; set; }

        /// <summary>
        /// Whether the Percentage flow and direction should be displayed above the line
        /// </summary>
        [Category("Status text"), Description("Whether the percentage of MVA flow and direction should be displayed above the line"), DefaultValue(false)]
        public bool ShowPercentageText { get; set; }

        /// <summary>The threshold for a thermal warning</summary>
        [Category("Monitoring"), Description("Thermal monitoring warning threshold")]
        public float ThermalWarning
        {
            get { return _ThermalWarning; }
            set
            {
                _ThermalWarning = value;
                if (MonitoringChanged != null)
                    MonitoringChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>The threshold for an alarm warning</summary>
        [Category("Monitoring"), Description("Thermal monitoring alert threshold")]
        public float ThermalAlert
        {
            get { return _ThermalAlert; }
            set
            {
                _ThermalAlert = value; if (MonitoringChanged != null)
                    MonitoringChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>The threshold for a low voltage alarm</summary>
        [Category("Monitoring"), Description("Low voltage alarm threshold")]
        public float LowVoltageAlert
        {
            get { return _LowVoltageAlert; }
            set
            {
                _LowVoltageAlert = value; if (MonitoringChanged != null)
                    MonitoringChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>The threshold for a low voltage alarm</summary>
        [Category("Monitoring"), Description("Low voltage warning threshold")]
        public float LowVoltageWarning
        {
            get { return _LowVoltageWarning; }
            set
            {
                _LowVoltageWarning = value; if (MonitoringChanged != null)
                    MonitoringChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>The threshold for a low voltage alarm</summary>
        [Category("Monitoring"), Description("High voltage warning threshold")]
        public float HighVoltageWarning
        {
            get { return _HighVoltageWarning; }
            set
            {
                _HighVoltageWarning = value; if (MonitoringChanged != null)
                    MonitoringChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>The threshold for a low voltage alarm</summary>
        [Category("Monitoring"), Description("High voltage alarm threshold")]
        public float HighVoltageAlert
        {
            get { return _HighVoltageAlert; }
            set
            {
                _HighVoltageAlert = value; if (MonitoringChanged != null)
                    MonitoringChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>Monitor equipment</summary>
        [Category("Monitoring"), Description("Monitor equipment")]
        public bool MonitorEquipment
        {
            get { return _MonitorEquipment; }
            set
            {
                _MonitorEquipment = value;
                if (MonitoringChanged != null)
                    MonitoringChanged(this, EventArgs.Empty);
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new KV Level
        /// </summary>
        /// <param name="ElementSource">Our element source</param>
        /// <param name="AddIfNew"></param>
        public MM_KVLevel(XmlElement ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
            this.Energized = new MM_DisplayParameter(ElementSource["Energized"], AddIfNew);
            this.PartiallyEnergized = new MM_DisplayParameter(ElementSource["PartiallyEnergized"], AddIfNew);
            if (ElementSource["Unknown"] != null)
                this.Unknown = new MM_DisplayParameter(ElementSource["Unknown"], AddIfNew);
            else
                this.Unknown = new MM_DisplayParameter(ElementSource["PartiallyEnergized"], AddIfNew);
            this.DeEnergized = new MM_DisplayParameter(ElementSource["DeEnergized"], AddIfNew);
        }

        /// <summary>
        /// Intiaizlize a new KV level
        /// </summary>
        public MM_KVLevel()
        {
            this.Energized = new MM_DisplayParameter();
            this.DeEnergized = new MM_DisplayParameter();
            this.PartiallyEnergized = new MM_DisplayParameter();
            this.DeEnergized.ForeColor = this.PartiallyEnergized.ForeColor = Color.Gray;
            this.Energized.Width = this.DeEnergized.Width = this.PartiallyEnergized.Width = 1;
        }

        /// <summary>
        /// Initialize a new KV level
        /// </summary>
        /// <param name="dRd"></param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_KVLevel(DbDataReader dRd, bool AddIfNew)
            : base(dRd, AddIfNew)
        {

        }

        public void Reset()
        {
            MM_KVLevel resetDisplay = new MM_KVLevel();
            resetDisplay.CopyTo(this);
        }

        /// <summary>
        /// Initialize a new KV level based on its name and foreground color
        /// </summary>
        /// <param name="Name">The name of the KV level</param>
        /// <param name="ForeColor">The foreground color </param>
        public MM_KVLevel(String Name, String ForeColor)
        {
            this.Name = Name;
            this.Energized = new MM_DisplayParameter(ColorTranslator.FromHtml(ForeColor), 1, false);
            this.DeEnergized = new MM_DisplayParameter(Color.DarkGray, 1, false);
            this.PartiallyEnergized = new MM_DisplayParameter(Color.DarkGray, 1, false);
            this.PartiallyEnergized = new MM_DisplayParameter(Color.DarkGray, 1, false);
            this.Index = MM_Repository.KVLevels.Count;
        }
        /// <summary>
        /// Update the KV level, and its underlying components
        /// </summary>
        /// <param name="ElementSource"></param>
        /// <param name="AddIfNew"></param>
        public new void ReadXML(XmlElement ElementSource, bool AddIfNew)
        {
            try
            {
                this.Energized.ReadXML(ElementSource["Energized"], AddIfNew);
                this.PartiallyEnergized.ReadXML(ElementSource["PartiallyEnergized"], AddIfNew);
                this.DeEnergized.ReadXML(ElementSource["DeEnergized"], AddIfNew);

                if (ElementSource["Unknown"] != null)
                    this.Unknown.ReadXML(ElementSource["Unknown"], AddIfNew);
                else
                    this.Unknown.ReadXML(ElementSource["PartiallyEnergized"], AddIfNew);

                base.ReadXML(ElementSource, AddIfNew);
            } catch (Exception ex)
            {
                MM_System_Interfaces.LogError(ex);
            }
        }

        /*/// <summary>
        /// Initialize a new KV Level display
        /// </summary>
        /// <param name="xE"></param>
        public MM_KVLevel(XmlElement xE)
        {
            this.Name = xE.Attributes["Name"].Value;
            

            ReinitializeDisplay(xE);
        }

        /// <summary>
        /// Reinitialize the display elements
        /// </summary>
        /// <param name="xE"></param>
        public void ReinitializeDisplay(XmlElement xE)
        {
            this.Energized.ReinitializeDisplay(xE["Energized"]);
            this.PartiallyEnergized.ReinitializeDisplay(xE["PartiallyEnergized"]);
            this.DeEnergized.ReinitializeDisplay(xE["DeEnergized"]);

            if (xE.HasAttribute("ShowPartiallyEnergized"))
                this.ShowPartiallyEnergized = XmlConvert.ToBoolean(xE.Attributes["ShowPartiallyEnergized"].Value);
            if (xE.HasAttribute("ShowEnergized"))
                this.ShowEnergized = XmlConvert.ToBoolean(xE.Attributes["ShowEnergized"].Value);
            if (xE.HasAttribute("Permitted"))
                this.Permitted = XmlConvert.ToBoolean(xE.Attributes["Permitted"].Value);            
            if (xE.HasAttribute("ShowDeEnergized"))
                this.ShowDeEnergized = XmlConvert.ToBoolean(xE.Attributes["ShowDeEnergized"].Value);            
            if (xE.HasAttribute("ShowMW"))
                this.ShowMVA= XmlConvert.ToBoolean(xE.Attributes["ShowMW"].Value);
            if (xE.HasAttribute("ShowMVA"))
                this.ShowMVA = XmlConvert.ToBoolean(xE.Attributes["ShowMVA"].Value);
            if (xE.HasAttribute("MVAThreshold"))
                this.MVAThreshold = MM_Converter.ToSingle(xE.Attributes["MVAThreshold"].Value);
            if (xE.HasAttribute("MWThreshold"))
                this.MVAThreshold = MM_Converter.ToSingle(xE.Attributes["MWThreshold"].Value);
            if (xE.HasAttribute("MVASize"))
                this.MVASize = MM_Converter.ToSingle(xE.Attributes["MVASize"].Value);
            
        }*/
        #endregion

        #region IComparable Members
        /// <summary>
        /// Handle the comparison between KV levels to allow for sorting
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public new int CompareTo(object obj)
        {
            int rt = Nominal.CompareTo(((MM_KVLevel)obj).Nominal);

            if (rt == 0 && obj.ToString() != ToString())
                return ToString().CompareTo(obj.ToString());

            return rt;
        }

        #endregion


        /// <summary>
        /// Display the name of the KV level
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Name;
        }
    }
}