using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using Macomber_Map.Data_Connections;
using System.Reflection;
using System.Drawing;
using Macomber_Map.Data_Connections.Generic;
using Macomber_Map.Data_Elements.Training;
using Macomber_Map.Data_Elements.Display;


namespace Macomber_Map.Data_Elements
{
    /// <summary>
    /// The CIM Repository stores all elements retrieved from the CIM model for rapid lookup. This provides the base for the network model and most lookups.
    /// </summary>
    public static class MM_Repository
    {
        #region Variable Declarations
        /// <summary>The set of cursors used for panning</summary>
        public static Cursor[,] PanCursors = new Cursor[,] { { Cursors.PanNW, Cursors.PanNorth, Cursors.PanNE }, { Cursors.PanWest, Cursors.NoMove2D, Cursors.PanEast }, { Cursors.PanSW, Cursors.PanSouth, Cursors.PanSE } };

        /// <summary>The collection of CIM elements based on TEID</summary>
        public static Dictionary<Int32, MM_Element> TEIDs = new Dictionary<Int32, MM_Element>();

        /// <summary>The collection of counties based on name</summary>
        public static Dictionary<String, MM_Boundary> Counties = new Dictionary<string, MM_Boundary>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>The collection of KV levels used within the region</summary>
        public static Dictionary<String, MM_KVLevel> KVLevels = new Dictionary<String, MM_KVLevel>(4, StringComparer.CurrentCultureIgnoreCase);
        
        /// <summary>The collection of generation types used within the network</summary>
        public static Dictionary<String, MM_Generation_Type> GenerationTypes = new Dictionary<string, MM_Generation_Type>(10, StringComparer.CurrentCultureIgnoreCase);

        /// <summary>The collection of contingency definitions</summary>
        public static Dictionary<String, MM_Contingency> Contingencies = new Dictionary<string, MM_Contingency>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>The collection of map tiles that are displayed on the screen</summary>
        public static Dictionary<MM_MapTile.Coordinates, MM_MapTile> MapTiles = new Dictionary<MM_MapTile.Coordinates, MM_MapTile>();

        /// <summary>Our collection of basecase violations</summary>
        public static Dictionary<MM_Element, MM_AlarmViolation> BasecaseViolations = new Dictionary<MM_Element, MM_AlarmViolation>();

        /// <summary>The collection of remedial action schemes</summary>
        public static Dictionary<String, MM_RemedialActionScheme> RASs = new Dictionary<string, MM_RemedialActionScheme>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>The collection of companies (TDSPs and QSEs)</summary>
        public static Dictionary<String, MM_Company> Companies = new Dictionary<string, MM_Company>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>The collection of substations</summary>
        public static Dictionary<String, MM_Substation> Substations = new Dictionary<string, MM_Substation>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>Our interface to our trainer</summary>
        public static MM_Training Training = null;

        /// <summary>The collection of violation types</summary>
        public static Dictionary<String, MM_AlarmViolation_Type> ViolationTypes = new Dictionary<string, MM_AlarmViolation_Type>(10, StringComparer.CurrentCultureIgnoreCase);        

        ///<summary>The collection of lines</summary>
        public static Dictionary<String, MM_Line> Lines = new Dictionary<string, MM_Line>(StringComparer.CurrentCultureIgnoreCase);

        ///<summary>The collection of blackstart corridors</summary>
        public static Dictionary<String, MM_Blackstart_Corridor> BlackstartCorridors = new Dictionary<string, MM_Blackstart_Corridor>(StringComparer.CurrentCultureIgnoreCase);
        
        /// <summary>The collection of multiple line segments</summary>
        public static Dictionary<Int32, MM_Line[]> MultipleLineSegments = new Dictionary<Int32, MM_Line[]>();

        ///<summary>The collection of electrical buses</summary>
        public static SortedDictionary<String, MM_Electrical_Bus> EBs = new SortedDictionary<string, MM_Electrical_Bus>(StringComparer.CurrentCultureIgnoreCase);

        ///<summary>The collection of electrical buses</summary>
        public static SortedDictionary<String, MM_BusbarSection> Busbars = new SortedDictionary<string, MM_BusbarSection>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>The collection of electrical buses by number</summary>
        public static Dictionary<int, MM_BusbarSection> BusNumbers = new Dictionary<int, MM_BusbarSection>();

        /// <summary>The collection of units</summary>
        public static Dictionary<String, MM_Unit> Units = new Dictionary<string, MM_Unit>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>The collection of islands</summary>
        public static Dictionary<int, MM_Island> Islands = new Dictionary<int, MM_Island>();

        /// <summary>The collection of DCTies</summary>
        public static Dictionary<String, MM_DCTie> DCTies = new Dictionary<string, MM_DCTie>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>The collection of loads</summary>
        public static Dictionary<String, MM_Load> Loads = new Dictionary<string, MM_Load>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>The collection of shunt compensators</summary>
        public static Dictionary<String, MM_ShuntCompensator> ShuntCompensators = new Dictionary<string, MM_ShuntCompensator>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>The collection of element types</summary>
        public static Dictionary<String, MM_Element_Type> ElemTypes = new Dictionary<string, MM_Element_Type>(StringComparer.CurrentCultureIgnoreCase);        

        /// <summary>The display options for substations</summary>
        public static MM_Substation_Display SubstationDisplay;

        /// <summary>The overall display parameters for the Macomber Map</summary>
        public static MM_Display OverallDisplay;
     
        /// <summary>The XML configuration document, responsible for handling updates to the system</summary>
        public static XmlDocument xConfiguration;       

        /// <summary>The delegate for a view change</summary>        
        /// <param name="ActiveView">The active view</param>
        public delegate void ViewChangedDelegate(MM_Display_View ActiveView);

        /// <summary>Alert other classes to a view change</summary>
        public static event ViewChangedDelegate ViewChanged;

        /// <summary>
        /// Our collection of network map rendering modes
        /// </summary>
        public enum enumRenderMode 
        {
            /// <summary>Use a DirectX network map for rendering</summary>
            DirectX, 
            ///<summary>Use a GDI+ network map for rendering</summary>
            GDIPlus,
            ///<summary>Use a Windows Presentation Framework (.NET 3.5) network map for rendering</summary>
            WPF
        }

        /// <summary>Our current rendering mode</summary>
        public static enumRenderMode RenderMode = enumRenderMode.GDIPlus;

        /// <summary>Whether to use adaptive rendering</summary>
        public static bool AdaptiveRendering = false;

        /// <summary>The collection of violation images</summary>
        public static ImageList ViolationImages;

        /// <summary>The collection of display views</summary>
        public static Dictionary<XmlElement, MM_Display_View> Views = new Dictionary<XmlElement, MM_Display_View>();

        /// <summary>The currently active view</summary>
        public static MM_Display_View ActiveView;

        /// <summary>The collection of load and weather zones</summary>
        public static Dictionary<String, MM_Zone> Zones = new Dictionary<string, MM_Zone>(20);

        /// <summary>The collection of operator notes, linked to TEIDs.</summary>
        public static Dictionary<Int32, MM_Note> Notes = new Dictionary<Int32, MM_Note>(100);

        /// <summary>Our collection of violations that are archived and shouldn't reappear</summary>
        public static Dictionary<MM_AlarmViolation, MM_AlarmViolation> ArchivedViolations = new Dictionary<MM_AlarmViolation, MM_AlarmViolation>();

        /// <summary>Our collection of violations</summary>
        public static Dictionary<MM_AlarmViolation, MM_AlarmViolation> Violations = new Dictionary<MM_AlarmViolation, MM_AlarmViolation>();

        /// <summary>Our system CPU usage</summary>
        public static float SystemCPUUsage=0;

        /// <summary>Our CPU performance counter</summary>
        private static System.Diagnostics.PerformanceCounter CPUCounter = new System.Diagnostics.PerformanceCounter();
        #endregion


        #region Initialization
        /// <summary>
        /// Initialize the MM Repository
        /// </summary>
        public static void Initialize()        
        {            
            //Initialize our image list for violation/alarm/event types
            ViolationImages = new ImageList();
            ViolationImages.ImageSize = new System.Drawing.Size(16, 16);            

            //Open up the XML configuration file, and load the settings into memory
            LoadXmlConfiguration();
        }

        /// <summary>
        /// Initialize and start our CPU monitor
        /// </summary>
        public static void StartCPUMonitor()
        {
            CPUCounter.CategoryName = "Processor";
            CPUCounter.CounterName = "% Processor Time";
            CPUCounter.InstanceName = "_Total";
            SystemCPUUsage = CPUCounter.NextValue();
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(CPUMonitor));
        }

        /// <summary>
        /// Run the CPU monitoring thread
        /// </summary>
        /// <param name="state"></param>
        private static void CPUMonitor(object state)
        {
            while (true)
            {
                SystemCPUUsage = CPUCounter.NextValue();
                System.Threading.Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Go through all display parameters, and reinitialize them
        /// </summary>
        public static void ReinitializeDisplayParameters()
        {
            foreach (XmlNode xNode in xConfiguration["Configuration"]["DisplayParameters"].ChildNodes)
                if (xNode is XmlElement)
                {
                    XmlElement xE = xNode as XmlElement;
                    if (xE.Name == "Substation")
                        SubstationDisplay.ReadXML(xE,true);
                    else if (xE.Name == "Overall")
                        OverallDisplay.ReadXML(xE,true);
                    else if (xE.Name == "VoltageLevel")
                        KVLevels[xE.Attributes["Name"].Value].ReadXML(xE,true);
                    else if (xE.Name == "Violation")
                        ViolationTypes[xE.Attributes["Name"].Value].ReadXML(xE,true);
                }
        }

        


        /// <summary>
        /// Load the XML Configuration file for the Macomber Map system
        /// </summary>
        private static void LoadXmlConfiguration()
        {
            //Create a blank violation entry for 'none'
            //ViolationImages.Images.Add("None", new Bitmap(16, 16));

            xConfiguration = new XmlDocument();
            xConfiguration.Load(Application.StartupPath + "\\MacomberMapConfiguration.xml");            
            Data_Integration.Permissions = new MM_Permissions(xConfiguration["Configuration"]["Permissions"]);
            foreach (XmlNode xNode in xConfiguration["Configuration"]["DisplayParameters"].ChildNodes)
                if (xNode is XmlElement)
                {
                    XmlElement xE = xNode as XmlElement;
                    if (xE.Name == "VoltageLevel")
                        KVLevels.Add(xE.Attributes["Name"].Value, new MM_KVLevel(xE, true));
                    else if (xE.Name == "ElementType")
                        ElemTypes.Add(xE.Attributes["Name"].Value, new MM_Element_Type(xE, true));
                    else if (xE.Name == "GenerationType")
                    {
                        MM_Generation_Type NewGen = new MM_Generation_Type(xE, true);
                        GenerationTypes.Add(NewGen.Name, NewGen);
                        if (!GenerationTypes.ContainsKey(NewGen.EMSName))
                            GenerationTypes.Add(NewGen.EMSName, NewGen);
                    }
                    else if (xE.Name == "Violation")
                        ViolationTypes.Add(xE.Attributes["Name"].Value, new MM_AlarmViolation_Type(xE, true));
                    else if (xE.Name == "Substation")
                        SubstationDisplay = new MM_Substation_Display(xE, true);
                    else if (xE.Name == "Overall")
                        OverallDisplay = new MM_Display(xE, true);
                    else if (xE.Name == "WeatherStationDetail")
                        Weather.MM_WeatherStation.QueryPrepend = xE.Attributes["URL"].Value;
                    else if (xE.Name == "WeatherStations")
                        Weather.MM_WeatherStationCollection.QueryPrepend = xE.Attributes["URL"].Value;
                    else if (xE.Name == "WeatherStationAlerts")
                        Weather.MM_WeatherStationCollection.AlertsPrepend = xE.Attributes["URL"].Value;
                    else if (xE.Name == "WeatherStationForecast")
                        Weather.MM_WeatherStationCollection.ForecastPrepend = xE.Attributes["URL"].Value;
                    else if (xE.Name == "StateBorder")
                        MM_Boundary.StateDisplay = new MM_DisplayParameter(xE, true);
                    else if (xE.Name == "CountyBorder")
                        MM_Boundary.CountyDisplay = new MM_DisplayParameter(xE, true);
                    else if (xE.Name == "Displays" || xE.Name == "Views")
                    { }
                    else if (xE.Name == "Levels")
                        MM_Repository.Training = new MM_Training(xE);                        
                    else
                        Program.WriteConsoleLine("Unknown element from XML configuration: " + xE.Name);
                }
            
            //Now pull in our views                         
            Views.Add(xConfiguration["Configuration"]["DisplayParameters"], new MM_Display_View(xConfiguration["Configuration"]["DisplayParameters"].Attributes["DefaultName"].Value));
            foreach (XmlElement xElem in xConfiguration["Configuration"]["DisplayParameters"]["Views"].SelectNodes("//View"))
                Views.Add(xElem, new MM_Display_View(xElem));

            //Pull in our commands
            foreach (XmlElement xElem in xConfiguration.SelectNodes("/Configuration/Commands/Command"))
                Data_Integration.Commands.Add(new MM_Command(xElem, true));

           


            //Now add in our icons for actions to our image collection
            ViolationImages.Images.Add("Note", Macomber_Map.Properties.Resources.NoteHS);
            ViolationImages.Images.Add("OneLine", Macomber_Map.Properties.Resources.OneLine);
            ViolationImages.Images.Add("MoveMap", Macomber_Map.Properties.Resources.MoveMap);
            ViolationImages.Images.Add("Web", Macomber_Map.Properties.Resources.SearchWebHS);
            ViolationImages.Images.Add("Acknowledge", Macomber_Map.Properties.Resources.Acknowledge);
            ViolationImages.Images.Add("Archive", Macomber_Map.Properties.Resources.Archive);
            ViolationImages.Images.Add("Properties", Macomber_Map.Properties.Resources.PropertiesHS);
        }
        #endregion

        /// <summary>
        /// Retrieve the current value from a property
        /// </summary>        
        /// <param name="BaseObject">The object to modify</param>
        /// <param name="PropertyName">The name of the object's property to be modified</param>
        /// <param name="NewValue">The new value</param>
        public static void SetProperty(Object BaseObject, String PropertyName, String NewValue)
        {
            PropertyInfo ToSet = BaseObject.GetType().GetProperty(PropertyName);
            if (ToSet.PropertyType.IsEnum)
                ToSet.SetValue(BaseObject, Enum.Parse(ToSet.PropertyType, NewValue), null);
            else
                switch (ToSet.PropertyType.Name)
                {
                    case "Font": ToSet.SetValue(BaseObject, System.ComponentModel.TypeDescriptor.GetConverter(ToSet.PropertyType).ConvertFromString(NewValue),null); return;                        
                    case "Color": ToSet.SetValue(BaseObject, ColorTranslator.FromHtml(NewValue), null); return;
                    case "Boolean": ToSet.SetValue(BaseObject, bool.Parse(NewValue), null); return;
                    case "Single": ToSet.SetValue(BaseObject, Single.Parse(NewValue), null); return;
                    case "Double": ToSet.SetValue(BaseObject, Double.Parse(NewValue), null); return;
                    case "Int32": ToSet.SetValue(BaseObject, Int32.Parse(NewValue), null); return;
                    case "String": ToSet.SetValue(BaseObject, (String)NewValue, null); return;
                    case "DisplayModeEnum": ToSet.SetValue(BaseObject, (MM_AlarmViolation_Type.DisplayModeEnum)Enum.Parse(typeof(MM_AlarmViolation_Type.DisplayModeEnum), (String)NewValue, true), null); return;
                    case "SubstationViewEnum": ToSet.SetValue(BaseObject, (Macomber_Map.Data_Elements.MM_Substation_Display.SubstationViewEnum)Enum.Parse(typeof(Macomber_Map.Data_Elements.MM_Substation_Display.SubstationViewEnum), (String)NewValue, true), null); return;
                    case "MM_Contour_Enum": ToSet.SetValue(BaseObject, (Macomber_Map.Data_Elements.MM_Display.MM_Contour_Enum)Enum.Parse(typeof(Macomber_Map.Data_Elements.MM_Display.MM_Contour_Enum), (String)NewValue, true), null); return;
                    default:
                        throw new Exception("Unknown type to parse: " + ToSet.PropertyType.Name);
                }               
        }

        /// <summary>
        /// Locate or create an element type
        /// </summary>
        /// <param name="TypeName"></param>
        /// <returns></returns>
        public static MM_Element_Type FindElementType(String TypeName)
        {
            MM_Element_Type OutType;
            if (!MM_Repository.ElemTypes.TryGetValue(TypeName, out OutType))
                MM_Repository.ElemTypes.Add(TypeName, OutType = new MM_Element_Type(TypeName, TypeName.Substring(0, 3), ""));
            return OutType;
        }

        /// <summary>
        /// Locate an KV Level. If one is not found, add it in.
        /// </summary>
        /// <param name="KVLevel">The KV level to search for</param>
        /// <returns></returns>
        public static MM_KVLevel FindKVLevel(string KVLevel)
        {

            MM_KVLevel OutLevel;
            if (KVLevels.TryGetValue(KVLevel, out OutLevel))
                return OutLevel;

            //Try making sure the KV tag is appropriately added            
            else if (KVLevels.TryGetValue(KVLevel.Replace("KV", "").Trim() + " KV", out OutLevel))
                return OutLevel;

            //Return the "Other KV" if we have it
            else if (KVLevels.TryGetValue("Other KV", out OutLevel))
                return OutLevel;
            else
                KVLevels.Add(KVLevel, OutLevel = new MM_KVLevel(KVLevel, "Green"));
            return OutLevel;
        }

        /// <summary>
        /// Locate a generation type by the specified name
        /// </summary>
        /// <param name="GenType">The type to search for</param>
        /// <returns></returns>
        public static MM_Generation_Type FindGenerationType(string GenType)
        {
            if (GenerationTypes.ContainsKey(GenType))
                return GenerationTypes[GenType];
            else
            {
                MM_Generation_Type Newtype = new MM_Generation_Type(GenType);
                GenerationTypes.Add(GenType, Newtype);
                return Newtype;
            }
        }

        /// <summary>
        /// Assign a view as being active
        /// </summary>
        /// <param name="ActiveView">The active view</param>
        public static void SetActiveView(MM_Display_View ActiveView)
        {
            Data_Integration.ActiveView = ActiveView.FullName;
            if (ViewChanged != null)
                ViewChanged(ActiveView);
        }

        /// <summary>
        /// Attempt to locate an element by its TEID
        /// </summary>
        /// <param name="TEID"></param>
        /// <returns></returns>
        public static MM_Element LocateElement(object TEID)
        {
            if (TEID is DBNull)
                return null;
            MM_Element FoundElem = null;
            TEIDs.TryGetValue(Convert.ToInt32(TEID), out FoundElem);
            return FoundElem;
        }

        /// <summary>
        /// Locate an element, based on its substation name, element name, element type and KV level.
        /// </summary>
        /// <param name="SubstationName">The name of the substation</param>
        /// <param name="ElementName">The name of the element</param>
        /// <param name="ElementType">The type of the element</param>
        /// <param name="ElementKVLevel">The KV level of the element</param>
        /// <returns>The appropriate element</returns>
        public static MM_Element LocateElement(String SubstationName, String ElementName, String ElementType, String ElementKVLevel)
        {
            //First, if we have a line, let's try and find that line
            if ((ElementType == "LN" || ElementType == "Line") && (Lines.ContainsKey(ElementName)))
                return Lines[ElementName];

            //First, find the substation in question.
            MM_Substation BaseStation = Substations[SubstationName];

            //Now, based on the element type, see if we already have the element
            MM_Element_Type OutElemType = null;
            if (ElementType == "DSC")
                ElementType = "SW";
            else if (ElementType == "SHUNT")
                ElementType = "CP";
            else if (ElementType == "TRANSF")
                ElementType = "XF";
            foreach (MM_Element_Type ElemType in ElemTypes.Values)
                if (String.Equals(ElemType.Name, ElementType, StringComparison.CurrentCultureIgnoreCase) || String.Equals(ElemType.Acronym, ElementType, StringComparison.CurrentCultureIgnoreCase))
                {
                    OutElemType = ElemType;
                    break;
                }
            
            //Now, if we have a unit, load or transformer, run a quick check to see if it's already there
            if (OutElemType.Name == "Unit")
                foreach (MM_Unit Unit in BaseStation.Units)                    
                    if (String.Equals(Unit.Name, ElementName, StringComparison.CurrentCultureIgnoreCase))
                        return Unit;
                    else if (OutElemType.Name == "Load")
                foreach (MM_Load Load in BaseStation.Loads)
                    if (String.Equals(Load.Name, ElementName, StringComparison.CurrentCultureIgnoreCase))
                        return Load;
                    else if (OutElemType.Name == "Transformer")
                foreach (MM_Transformer Transformer in BaseStation.Transformers)
                    if (String.Equals(Transformer.Name, ElementName, StringComparison.CurrentCultureIgnoreCase))
                        return Transformer;

            //If not, send out our request to the CIM server            
            MM_Element FoundElement = null;
            if (Data_Integration.MMServer != null)
                FoundElement = Data_Integration.MMServer.LoadElement(SubstationName, ElementName, OutElemType);
            
            
            //If we can't find it, let's create a new one.
            if (FoundElement == null)
            {
                FoundElement = new MM_Element();
                FoundElement.ElemType = OutElemType;
                FoundElement.Name = ElementName;
                FoundElement.Substation = BaseStation;
                FoundElement.KVLevel = GetVoltageLevel(ElementKVLevel);
            }
            return FoundElement;
        }

        /// <summary>
        /// Return the voltage level of an element
        /// </summary>
        /// <param name="VoltageLevel">The incoming voltage level</param>
        /// <returns></returns>
        public static MM_KVLevel GetVoltageLevel(String VoltageLevel)
        {
            foreach (MM_KVLevel KVLevel in KVLevels.Values)
                if (KVLevel.Name.Split(' ')[0] == VoltageLevel.Split(' ')[0])
                    return KVLevel;
            return KVLevels["Other KV"];
        }

        /// <summary>
        /// Convert text so that the first letter of each word is capitalized
        /// </summary>
        /// <param name="InString"></param>
        /// <returns></returns>
        public static string TitleCase(string InString)
        {
            char[] inArray = InString.ToCharArray();
            bool FirstLetter = true;
            for (int a = 0; a < inArray.Length; a++)
                if (char.IsSeparator(inArray[a]) || char.IsPunctuation(inArray[a]))
                    FirstLetter = true;
                else if (FirstLetter)
                {
                    inArray[a] = Char.ToUpper(inArray[a]);
                    FirstLetter = false;
                }
                else
                    inArray[a] = Char.ToLower(inArray[a]);
            return new string(inArray);
        }
    }
}
