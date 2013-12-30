using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Macomber_Map.Data_Connections;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;

namespace Macomber_Map.Data_Elements
{
    /// <summary>
    /// This class holds the display parameters for a substation
    /// </summary>
    public class MM_Substation_Display: MM_DisplayParameter
    {
        /// <summary>
        /// Create a new substation display parameter
        /// </summary>
        /// <param name="ElementSource">The XML Configuration element for the substation</param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Substation_Display(XmlElement ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {            
        }

        private bool _ShowSubstationVoltages = false, _ShowSubsOnLines = true, _ShowTotalGeneration, _ShowTotalHSL, _ShowRemainingCapacity, _ShowTotalLoad = true, _ShowAutoTransformersOut = false, _ShowFrequencyControl = true, _HighlightMultipleLines = true, _ShowSynchroscopes=false;
        private SubstationViewEnum _ShowSubstations = SubstationViewEnum.All;
        private Color _GenerationColor, _LoadColor;
        
        /// <summary>
        /// The collection of possible options for viewing the substation
        /// </summary>
        public enum SubstationViewEnum { 
            /// <summary>Show all substations</summary>
            All,
            /// <summary>Show only substations with units</summary>
            Units,
            /// <summary>Show only substations with loads</summary>
            Loads,
            /// <summary>Show only substations with LAARs</summary>
            LAARs,
            /// <summary>Show only substations with transformers</summary>
            Transformers,
            /// <summary>Show only substations with capacitors</summary>
            Capacitors,
            /// <summary>Show only substations with capacitors that are online</summary>
            CapacitorsOnline,
            /// <summary>Show only substations with capacitors that are available</summary>
            CapacitorsAvailable,
            /// <summary>Show only substations with reactors</summary>
            Reactors,
            /// <summary>Show only substations with reactors that are online</summary>
            ReactorsOnline,
            /// <summary>Show only substations with reactors that are available</summary>
            ReactorsAvailable,
            /// <summary>Show only substations with static var compensators</summary>
            StaticVarCompensators,
            /// <summary>Show no substations</summary>
            None 
        };

        /// <summary>
        /// Whether substations should be shown if they're connected to a visible line
        /// </summary>
        [Category("Display"), Description("Whether substations should be shown if they're connected to a visible line"), DefaultValue(true)]
        public bool ShowSubsOnLines
        {
            get { return _ShowSubsOnLines; }
            set { _ShowSubsOnLines = value; }
        }

        /// <summary>
        /// Whether substations should be drawn to show voltages
        /// </summary>
        [Category("Display"), Description("Whether substations should be drawn to show voltages"), DefaultValue(true)]
        public bool ShowSubstationVoltages
        {
            get { return _ShowSubstationVoltages; }
            set { _ShowSubstationVoltages = value; }
        }

        /// <summary>
        /// Whether substations should be shown on the network map
        /// </summary>
        [Category("Display"), Description("Whether substations should be shown on the network map"), DefaultValue(Macomber_Map.Data_Elements.MM_Substation_Display.SubstationViewEnum.All)]
        public SubstationViewEnum ShowSubstations
        {
            get { return _ShowSubstations; }
            set { _ShowSubstations = value; }
        }


        /// <summary>
        /// The color for a substation with generation
        /// </summary>
        [Category("Generation"), Description("The color for a substation with generation")]
        public Color GenerationColor
        {
            get { return _GenerationColor; }
            set { _GenerationColor = value; }
        }


        /// <summary>
        /// The color for a substation with Load
        /// </summary>
        [Category("Load"), Description("The color for a substation with Load")]
        public Color LoadColor
        {
            get { return _LoadColor; }
            set { _LoadColor = value; }
        }

        /// <summary>
        /// Whether the total generation should be displayed under a substation with generation
        /// </summary>
        [Category("Generation"), Description("Whether the total generation should be displayed under a substation with generation"), DefaultValue(false)]
        public bool ShowTotalGeneration
        {
            get { return _ShowTotalGeneration; }
            set { _ShowTotalGeneration = value; }
        }

        /// <summary>
        /// Whether the total HSL should be displayed under a substation with HSL
        /// </summary>
        [Category("Generation"), Description("Whether the total HSL should be displayed under a substation with HSL"), DefaultValue(false)]
        public bool ShowTotalHSL
        {
            get { return _ShowTotalHSL; }
            set { _ShowTotalHSL = value; }
        }

        /// <summary>
        /// Whether the remaining capacity should be displayed under a substation with generation
        /// </summary>
        [Category("Generation"), Description("Whether the remaining capacity should be displayed under a substation with generation"), DefaultValue(false)]
        public bool ShowRemainingCapacity
        {
            get { return _ShowRemainingCapacity; }
            set { _ShowRemainingCapacity = value; }
        }

        /// <summary>
        /// Whether the total load should be displayed under a substation with loads
        /// </summary>
        [Category("Load"), Description("Whether the total load should be displayed under a substation with loads"), DefaultValue(false)]
        public bool ShowTotalLoad
        {
            get { return _ShowTotalLoad; }
            set { _ShowTotalLoad = value; }
        }

        /// <summary>
        /// Return the name of the variable
        /// </summary>
        public new string Name
        {
            get { return "Substations"; }
            set { }
        }

        /// <summary>
        /// Show auto-transformers that are out of service on the map
        /// </summary>
        [Category("Display"),Description("Show auto-transformers that are out of service on the map"), DefaultValue(false)]
        public bool ShowAutoTransformersOut
        {
            get { return _ShowAutoTransformersOut; }
            set { _ShowAutoTransformersOut = value; }
        }

        /// <summary>
        /// Show substations with frequency-controlled units
        /// </summary>
        [Category("Display"), Description("Show substations with frequency-controlled units"), DefaultValue(false)]
        public bool ShowFrequencyControl
        {
            get { return _ShowFrequencyControl; }
            set { _ShowFrequencyControl = value; }
        }

        /// <summary>
        /// Show substations with synchroscopes
        /// </summary>
        [Category("Display"), Description("Show substations with synchroscopes"), DefaultValue(false)]        
        public bool ShowSynchroscopes
        {
            get { return _ShowSynchroscopes; }
            set { _ShowSynchroscopes = value; }
        }

        /// <summary>
        /// Highlight multiple lines (two or more lines connecting the same two substations)
        /// </summary>
        [Category("Display"), Description("Highlight multiple lines (two or more lines connecting the same two substations)"), DefaultValue(true)]
        public bool HighlightMultipleLines
        {
            get { return _HighlightMultipleLines; }
            set { _HighlightMultipleLines = value; }
        }
    }
}
