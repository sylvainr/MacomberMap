using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.ComponentModel;
using Macomber_Map.Data_Connections;
using System.Data.Common;
using System.Drawing;

namespace Macomber_Map.Data_Elements
{
    /// <summary>
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


        private bool _ShowEnergized, _ShowPartiallyEnergized, _ShowDeEnergized, _ShowUnknown = false, _ShowMVA, _ShowLineName, _ShowLineRouting = true;
        private bool _ShowMWText, _ShowMVARText, _ShowMVAText, _ShowPercentageText, _ShowInMenu = true, _ShowOnMap = true;
        private float _MVAThreshold, _MVASize = 7f, _VisibilityThreshold = 0f;
        private int _StationMW, _StationNames, _VisibilityByZoom;


        /// <summary>
        /// The zoom levels at which equipment of this type is visible
        /// </summary>
        [Category(" Visibility"), Description("The zoom levels at which equipment of this type is visible"), DefaultValue(true)]
        public int VisibilityByZoom
        {
            get { return _VisibilityByZoom; }
            set { _VisibilityByZoom = value; }
        }


        /// <summary>
        /// Whether buses of this KV level are shown on the network map
        /// </summary>
        [Category(" Visibility"), Description("The zoom level at which MW levels (e.g., load, gen) are shown"), DefaultValue(true)]
        public int StationMW
        {
            get { return _StationMW; }
            set { _StationMW = value; }
        }

        /// <summary>
        /// Whether buses of this KV level are shown on the network map
        /// </summary>
        [Category(" Visibility"), Description("The zoom level at which station names are shown"), DefaultValue(true)]
        public int StationNames
        {
            get { return _StationNames; }
            set { _StationNames = value; }
        }


        /// <summary>Our nominal voltage level</summary>
        public float Nominal
        {
            get
            {
                if (!ComputedNominal)
                {
                    ComputedNominal = true;
                    float.TryParse(Name.Split(' ')[0], out _Nominal);
                }
                return _Nominal;
            }
        }

        private bool ComputedNominal = false;
        private float _Nominal = 0f;

        /// <summary>
        /// Whether buses of this KV level are shown on the network map
        /// </summary>
        [Category(" Visibility"), Description("Whether buses of this KV level are shown on the network map"), DefaultValue(true)]
        public bool ShowOnMap
        {
            get { return _ShowOnMap; }
            set { _ShowOnMap = value; }
        }

        /// <summary>
        /// Whether buses of this KV level are shown in menus
        /// </summary>
        [Category(" Visibility"), Description("Whether buses of this KV level are shown in menus"), DefaultValue(true)]
        public bool ShowInMenu
        {
            get { return _ShowInMenu; }
            set { _ShowInMenu = value; }
        }


        /// <summary>
        /// Show the full routing of transmission lines
        /// </summary>
        [Category(" Visibility"), Description("Show the full routing of transmission lines"), DefaultValue(true)]
        public bool ShowLineRouting
        {
            get { return _ShowLineRouting; }
            set { _ShowLineRouting = value; }
        }

        /// <summary>
        /// Show energized lines in the network map
        /// </summary>
        [Category(" Visibility"), Description("Show energized lines in the network map"), DefaultValue(true)]
        public bool ShowEnergized
        {
            get { return _ShowEnergized; }
            set { _ShowEnergized = value; }
        }

        /// <summary>
        /// Show unknown-state lines in the network map
        /// </summary>
        [Category(" Visibility"), Description("Show unknown-state lines in the network map"), DefaultValue(false)]
        public bool ShowUnknown
        {
            get { return _ShowUnknown; }
            set { _ShowUnknown = value; }
        }

        /// <summary>
        /// Show partially energized lines in the network map
        /// </summary>
        [Category(" Visibility"), Description("Show partially energized lines in the network map"), DefaultValue(true)]
        public bool ShowPartiallyEnergized
        {
            get { return _ShowPartiallyEnergized; }
            set { _ShowPartiallyEnergized = value; }
        }

        /// <summary>
        /// Show de-energized lines in the network map
        /// </summary>
        [Category(" Visibility"), Description("Show de-energized lines in the network map"), DefaultValue(true)]
        public bool ShowDeEnergized
        {
            get { return _ShowDeEnergized; }
            set { _ShowDeEnergized = value; }
        }


        /// <summary>
        /// Show the overall visibility of this KV level
        /// </summary>
        [Category(" Visibility"), Description("Show the overall visibility of this KV level"), DefaultValue(true), ReadOnly(true)]
        public bool SimpleVisibility
        {
            get { return _ShowEnergized | _ShowDeEnergized | _ShowPartiallyEnergized | _ShowUnknown; }
            set { _ShowEnergized = _ShowDeEnergized = _ShowPartiallyEnergized = _ShowUnknown = value; }
        }

        /// <summary>
        /// Show MVA flow in the network map
        /// </summary>
        [Category("MVA Animation"), Description("Show MVA flow in the network map"), DefaultValue(false)]
        public bool ShowMVA
        {
            get { return _ShowMVA; }
            set { _ShowMVA = value; }
        }



        /// <summary>
        /// The threshold (in percent) at or over which the MVA flow should be shown
        /// </summary>
        [Category("MVA Animation"), Description("The threshold (in percent) at or over which the MVA flow should be shown"), DefaultValue(0f)]
        public float MVAThreshold
        {
            get { return _MVAThreshold; }
            set { _MVAThreshold = value; }
        }


        /// <summary>
        /// The threshold (in percent) at or over which the MVA flow should be shown
        /// </summary>
        [Category("Visibility"), Description("The threshold (in percent) at or over which the line should be shown"), DefaultValue(0f)]
        public float VisibilityThreshold
        {
            get { return _VisibilityThreshold; }
            set { _VisibilityThreshold = value; }
        }

        /// <summary>
        /// The base size of an MVA flow line at 100% of normal limit
        /// </summary>
        [Category("MVA Animation"), Description("The base size of an MVA flow line at 100% of normal limit")]
        public float MVASize
        {
            get { return _MVASize; }
            set { _MVASize = value; }
        }



        /// <summary>
        /// Whether the MW flow and direction should be displayed above the line
        /// </summary>
        [Category("Status text"), Description("Whether the MW flow and direction should be displayed above the line"), DefaultValue(false)]
        public bool ShowMWText
        {
            get { return _ShowMWText; }
            set { _ShowMWText = value; }
        }

        /// <summary>
        /// Whether the line's name should be displayed
        /// </summary>
        [Category("Status text"), Description("Whether the line's name should be displayed"), DefaultValue(false)]
        public bool ShowLineName
        {
            get { return _ShowLineName; }
            set { _ShowLineName = value; }
        }

        /// <summary>
        /// Whether the MVAR flow and direction should be displayed above the line
        /// </summary>
        [Category("Status text"), Description("Whether the MVAR flow and direction should be displayed above the line"), DefaultValue(false)]
        public bool ShowMVARText
        {
            get { return _ShowMVARText; }
            set { _ShowMVARText = value; }
        }

        /// <summary>
        /// Whether the MVA flow and direction should be displayed above the line
        /// </summary>
        [Category("Status text"), Description("Whether the MVA flow and direction should be displayed above the line"), DefaultValue(false)]
        public bool ShowMVAText
        {
            get { return _ShowMVAText; }
            set { _ShowMVAText = value; }
        }

        /// <summary>
        /// Whether the Percentage flow and direction should be displayed above the line
        /// </summary>
        [Category("Status text"), Description("Whether the percentage of MVA flow and direction should be displayed above the line"), DefaultValue(false)]
        public bool ShowPercentageText
        {
            get { return _ShowPercentageText; }
            set { _ShowPercentageText = value; }
        }

        /// <summary>
        /// The integer value of the KV level (used by the SQL-based versions of the CIM server)
        /// </summary>
        public int Index;
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
            this.Energized.ReadXML(ElementSource["Energized"], AddIfNew);
            this.PartiallyEnergized.ReadXML(ElementSource["PartiallyEnergized"], AddIfNew);
            this.DeEnergized.ReadXML(ElementSource["DeEnergized"], AddIfNew);

            if (ElementSource["Unknown"] != null)
                this.Unknown.ReadXML(ElementSource["Unknown"], AddIfNew);
            else
                this.Unknown.ReadXML(ElementSource["PartiallyEnergized"], AddIfNew);

            base.ReadXML(ElementSource, AddIfNew);
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
            int CurVal = 0, OtherVal = 0;
            int.TryParse(Name.Split(' ')[0], out CurVal);
            int.TryParse((obj as MM_KVLevel).Name.Split(' ')[0], out OtherVal);
            return OtherVal.CompareTo(CurVal);
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
