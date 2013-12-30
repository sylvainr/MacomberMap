using System;
using System.Collections.Generic;
using System.Text;
using Macomber_Map.Data_Elements;
using System.Data.SqlClient;
using System.IO;
using System.Xml;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Macomber_Map.Data_Elements.Weather;
using System.Data;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using Macomber_Map.User_Interface_Components;
using System.Data.Common;
using System.Speech.Synthesis;
using Macomber_Map.Data_Connections.Generic;
using System.Reflection;
using Macomber_Map.User_Interface_Components.GDIPlus;
using Macomber_Map.User_Interface_Components.NetworkMap;
using Macomber_Map.Data_Elements.Training;

namespace Macomber_Map.Data_Connections
{
    /// <summary>
    /// This class is the data integration layer, pulling information from a variety of sources and integrating them for the UI.
    /// </summary>
    public static class Data_Integration
    {
        #region Variable declarations
        /// <summary>Whether to use speech for violations</summary>
        public static bool UseSpeech = false;

        /// <summary>If a savecase is being used, when it was last done</summary>
        public static DateTime SaveTime = DateTime.FromOADate(0);

        /// <summary>Our random TEIDs.</summary>
        private static Random TEIDRandomizer = new Random();

        /// <summary>The load forecast table</summary>
        public static DataTable LoadForecast;

        /// <summary>The load forecast dates table</summary>
        public static DataTable LoadForecastDates;

        /// <summary>Whether the end dates should be used from LF</summary>
        public static bool LoadForecastUseEndDates = true;

        /// <summary>The date when the load forecast was last refreshed</summary>
        public static DateTime LFDate;

        /// <summary>The date when the RTGen data was last refreshed</summary>
        public static DateTime RTGenDate;

        /// <summary>Our log file to capture value changes</summary>
        public static StreamWriter ValueChangeLog = null;

        /// <summary>The collection of currently-active data sets, for serialization purposes</summary>
        public static Dictionary<String, DataSet> ActiveData = new Dictionary<string, DataSet>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>The collection of runnign threads</summary>
        public static List<Thread> RunningThreads
        {
            get { return _RunningThreads; }
        }
        private static List<Thread> _RunningThreads = new List<Thread>();

        /// <summary>The collection of running forms</summary>
        public static List<MM_Form> RunningForms
        {
            get { return _RunningForms; }
        }
        private static List<MM_Form> _RunningForms = new List<MM_Form>();

        private static string _userName = null;

        /// <summary>The current user name</summary>
        public static string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }

        /// <summary>The path for locally-cached one-lines</summary>
        public static String OneLineDirectory;


        /// <summary>
        /// An delegate for changes in violations, either addition or removal
        /// </summary>        
        /// <param name="Violation">The added / removed violation</param>
        public delegate void ViolationChangeDelegate(MM_AlarmViolation Violation);

        /// <summary>A violation has been added</summary>
        public static event ViolationChangeDelegate ViolationAdded;

        /// <summary>A violation has been added</summary>
        public static event ViolationChangeDelegate ViolationAcknowledged;

        /// <summary>A violation has been removed</summary>
        public static event ViolationChangeDelegate ViolationRemoved;

        /// <summary>A violation has been modified</summary>
        public static event ViolationChangeDelegate ViolationModified;

        /* /// <summary>The collection of notes</summary>
         public static Dictionary<Int32, MM_Note> Notes
         {
             get { return _Notes; }
         }
         private static Dictionary<Int32, MM_Note> _Notes = new Dictionary<Int32, MM_Note>();
         */
        /// <summary>
        /// A delegate for changes in notes, either addition, removal, or modification
        /// </summary>
        /// <param name="Note">The added/changed/removed note</param>
        public delegate void NoteChangeDelegate(MM_Note Note);

        /// <summary>The network source has changed</summary>
        public static event EventHandler NetworkSourceChanged;

        /// <summary>A note has been added</summary>
        public static event NoteChangeDelegate NoteAdded;

        /// <summary>A note has been modified</summary>
        public static event NoteChangeDelegate NoteModified;

        /// <summary>A note has been removed</summary>
        public static event NoteChangeDelegate NoteRemoved;

        /// <summary>The weather station loader and maintenance system</summary>
        public static MM_WeatherStationCollection Weather = new MM_WeatherStationCollection();

        /// <summary>The data connections available within the Macomber Map system</summary>
        public static Dictionary<String, MM_DataConnector> DataConnections
        {
            get { return _DataConnections; }
        }
        private static Dictionary<String, MM_DataConnector> _DataConnections = new Dictionary<string, MM_DataConnector>(8, StringComparer.CurrentCultureIgnoreCase);


        ///<summary>The time of last execution for a process</summary>
        public static Dictionary<String, MM_Process> Processes
        {
            get { return _Processes; }
        }
        private static Dictionary<String, MM_Process> _Processes = new Dictionary<string, MM_Process>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>Our collection of communications linkages</summary>
        public static SortedDictionary<String, MM_Communication_Linkage> Communications
        {
            get { return _Communications; }
        }
        private static SortedDictionary<String, MM_Communication_Linkage> _Communications = new SortedDictionary<string, MM_Communication_Linkage>();

        /// <summary>Whether to use estimates or telemetered values</summary>
        private static bool _UseEstimates = true;

        /// <summary>Whether to show post-contignency values</summary>
        private static bool _ShowPostCtgValues = false;

        /// <summary>The network source for the data</summary>
        private static MM_Data_Source _NetworkSource = null;

        /// <summary>Whether all communications points have yet been loaded</summary>
        public static bool CommLoaded = false;

        /// <summary>Whether to show Alarms</summary>
        private static bool _DisplayAlarms = false;

        /// <summary>The currently activated view</summary>
        public static String ActiveView
        {
            get { return _ActiveView; }
            set
            {
                foreach (MM_Display_View View in MM_Repository.Views.Values)
                    if (View.FullName == value)
                        //{
                        //  View.Activate();
                        _ActiveView = value;
                //}
            }
        }
        private static string _ActiveView;

        /// <summary>The current time, according to CIM</summary>
        public static DateTime CurrentTime = DateTime.FromOADate(0);

        /// <summary>The network source</summary>
        public static MM_Data_Source NetworkSource
        {
            get { return _NetworkSource; }
            set
            {
                //User_Interface_Components.GDIPlus.Network_Map_GDI.LockForDataUpdate = true;
                _NetworkSource = value;
                if (value != null)
                    _UseEstimates = value.Estimates;

                //Reset all points
                if (DataConnections.Count > 0 || Data_Integration.MMServer != null)
                {
                    lock (MM_Repository.TEIDs)
                    {
                        try //MN//20130607
                        {
                            foreach (MM_Element Elem in MM_Repository.TEIDs.Values)
                                ClearPoint(Elem);
                        }
                        catch
                        {

                        }
                    }
                    List<MM_AlarmViolation> ToRemove = new List<MM_AlarmViolation>();

                    foreach (MM_AlarmViolation Viol in MM_Repository.Violations.Keys)
                    {

                        // if (Viol.Type.Name == "ForcedOutage" || Viol.Type.Name == "UndocumentedOutage" || Viol.Type.Name == "ForcedExtension" || Viol.Type.Name == "UnavoidableExtension")
                        ToRemove.Add(Viol);
                    }

                    foreach (MM_AlarmViolation RemoveMe in ToRemove)
                        Data_Integration.RemoveViolation(RemoveMe);
                    MM_Repository.ArchivedViolations.Clear();
                    
                }

                //Link to our study server
                if (Data_Integration.MMServer != null && Data_Integration.InitializationComplete)
                    if (Data_Integration.MMServer.StudyMode = (value.Application == "StudySystem"))
                    {
                        if (MMServer.StudiesEnabled)
                            Data_Integration.MMServer.RetrieveStudyData();
                        else
                            Program.MessageBox("No Access to Study Network", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    }
                    else
                        Data_Integration.MMServer.RetrieveRealTimeData();


                /*
                foreach (MM_DataConnector Conn in DataConnections.Values)
                    if (value.ConnectorType == Conn.GetType())
                        Conn.SetNetworkSource(value);
                    else
                        Conn.SetNetworkSource(null);*/

                if (NetworkSourceChanged != null)
                    NetworkSourceChanged(typeof(Data_Integration), EventArgs.Empty);
                //User_Interface_Components.GDIPlus.Network_Map_GDI
                //Macomber_Map.User_Interface_Components.
                //ResetNetworkMap();
               // Macomber_Map.User_Interface_Components.NetworkMap.GDIPlus.re   .ResetMap();
                //ctlNetworkMap.SetControls(ctlViolationViewer, ctlMiniMap, 0);
                //GC.Collect();
                //Program.MM.ResetNetworkMap(); //MN//20130607
                //User_Interface_Components.GDIPlus.Network_Map_GDI.UpdateGraphicsPaths(false, true);
                //User_Interface_Components.GDIPlus.Network_Map_GDI.LockForDataUpdate = false;
                
            }
        }

        /// <summary>
        /// Whether components with contingeny violations should show post-Ctg values
        /// </summary>
        public static bool ShowPostCtgValues
        {
            get { return _ShowPostCtgValues; }
            set
            {
                _ShowPostCtgValues = value;
                if (NetworkSourceChanged != null)
                    NetworkSourceChanged(typeof(Data_Integration), EventArgs.Empty);
            }
        }

        /// <summary>Whether the named pipe communications should be used</summary>
        public static bool UsePipe = false;

        /// <summary>
        /// Whether estimates should be shown
        /// </summary>
        public static bool UseEstimates
        {
            get { return _UseEstimates; }
            set
            {
                _UseEstimates = value;
                if (NetworkSourceChanged != null)
                    NetworkSourceChanged(typeof(Data_Integration), EventArgs.Empty);
            }
        }

        /// <summary>
        /// Whether alarms should be shown
        /// </summary>
        public static bool DisplayAlarms
        {
            get { return _DisplayAlarms; }
            set
            {
                _DisplayAlarms = value;
            }
        }

        /// <summary>Whether the system is in test mode (no data connectors)</summary>
        public static bool TestMode = false;

        /// <summary>Whether all main components and UI loading are complete.</summary>
        public static bool InitializationComplete = false;

        /// <summary>The access-level permissions for the Macomber Map</summary>
        public static MM_Permissions Permissions;

        /// <summary>Our reserve status</summary>
        public static Dictionary<String, MM_Reserve> Reserves
        { get { return _Reserves; } }
        private static Dictionary<String, MM_Reserve> _Reserves = new Dictionary<string, MM_Reserve>(20, StringComparer.CurrentCultureIgnoreCase);

        /// <summary>Our collection of CSC/IROL limits</summary>
        public static Dictionary<int, MM_Limit> CSCIROLLimits
        { get { return _CSCIROLLimits; } }
        private static Dictionary<int, MM_Limit> _CSCIROLLimits = new Dictionary<int, MM_Limit>(10);

        /// <summary>The name of the MM server to which we're connected</summary>
        public static string MMServerName;

        /// <summary>The name of the MMS server to which we're connected</summary>
        public static string MMSServerName;

        /// <summary>The name of the Historic server to which we're connected</summary>
        public static string HistoryServerName;

        /// <summary>Whether the state estimator import has completed.</summary>
        public static bool ImportCompleted = false;

        /// <summary>The speech synthesizer</summary>
        public static SpeechSynthesizer Speech = new SpeechSynthesizer();

        /// <summary>The repository of commands that MM can execute</summary>
        public static List<MM_Command> Commands
        {
            get { return _Commands; }
        }
        /// <summary>The repository of commands that MM can execute</summary>
        private static List<MM_Command> _Commands = new List<MM_Command>();
        

        /// <summary>The full list of available data sources</summary>
        public static DataTable DataSources;

        /// <summary>The name of the network model</summary>
        public static String ModelName = "Unknown";

        /// <summary>The date of the network model</summary>
        public static DateTime ModelDate = DateTime.Now;

        /// <summary>The connection to the CIM server (can be Microsoft SQL, Oracle data connector, MySQL connector, Microsoft Access connector)</summary>
        public static MM_Server_Connector MMServer;

        #endregion


        /// <summary>
        /// Initialize the data integration layer 
        /// </summary>
        /// <param name="MMServer">Our MM Server</param>
        /// <param name="MMServerName">The name of the MM Server</param>
        /// <param name="MMS_Server_Name">The name of the MMS Server</param>
        /// <param name="History_Server_Name">The name of the historic server</param>
        /// <param name="WeatherData">Whether weather data should be downloaded</param>
        /// <param name="Startup">The startup form</param>
        /// <param name="NetworkSource">The network source to be used</param>
        /// <param name="UserName">The user's name</param>
        /// <param name="Password">The user's password</param>
        /// <param name="WriteValueUpdates">Whether to write out value updates</param>
        /// <returns>True on full success</returns>                        
        public static bool StartupModel(MM_Server_Connector MMServer, String MMServerName, String MMS_Server_Name, String History_Server_Name, bool WeatherData, Startup_Form Startup, MM_Data_Source NetworkSource, String UserName, String Password, bool WriteValueUpdates)
        {
            Data_Integration.MMServer = MMServer;

            Data_Integration.UserName = UserName;
            Data_Integration.NetworkSource = NetworkSource;

            Data_Integration.MMServerName = MMServerName;
            Data_Integration.MMSServerName = MMS_Server_Name;
            Data_Integration.HistoryServerName = History_Server_Name;

            //Now, build our collection of steps to carry out
            Dictionary<String, LoadEventDelegate> StartupSteps = new Dictionary<string, LoadEventDelegate>();
            //StartupSteps.Add("Requesting record locators", MMServer.RequestRecordLocators);
            //StartupSteps.Add("Requesting one-line mapping data", MMServer.LoadOneLineMappings);            
            //StartupSteps.Add("Loading the CIM model from the MM Server", MMServer.ValidateCIMModel);
            //StartupSteps.Add("Requesting key data from the MM Server", MMServer.RetrieveData);
            //StartupSteps.Add("Loading reserve information", MMServer.LoadReserves);
            //StartupSteps.Add("CIM: Applying system permissions", ApplyPermissions);
            //StartupSteps.Add("CIM: Matching units, loads, transformers and capacitors to substations.", RollUpElementsToSubstation);
            //StartupSteps.Add("CIM: Completing startup process", MMServer.StartupCompleted);
            //if (WriteValueUpdates)
            //    StartupSteps.Add("MM: Creating value updates log file", Data_Integration.CreateValueUpdateLogFile);
            //StartupSteps.Add("MM: Starting up Map", Startup.CreateMap);
            //StartupSteps.Add("MM: Waiting for Map to complete loading", Startup.WaitForMap);
            //StartupSteps.Add("Requesting data again from the MM Server", MMServer.RetrieveData); //yma fix for violations data missing in 4-4-13
            
            //Assign our test mode
            TestMode = String.IsNullOrEmpty(MMServerName);




            //Load our MMS system
            if (!String.IsNullOrEmpty(MMS_Server_Name))
            {
                StartupSteps.Add("MMS: Connecting to " + (MM_Repository.xConfiguration.SelectSingleNode("Configuration/Databases/Database[@Type='MMS_Connector' and @Name='" + MMSServerName + "']") as XmlElement).Attributes["Name"].Value, LoadMMS);
                StartupSteps.Add("MMS: Building MMS Name <-> CIM TEID mappings", BuildMMSTableMappings);
            }

            //Load our historic system
            if (!String.IsNullOrEmpty(History_Server_Name))
                StartupSteps.Add("Hist: Connecting to " + (MM_Repository.xConfiguration.SelectSingleNode("Configuration/Databases/Database[@Type='Historic_Connector' and @Name='" + History_Server_Name + "']") as XmlElement).Attributes["Name"].Value, LoadHistoricDatabase);


            //If requested, load our weather data
            if (WeatherData)
                StartupSteps.Add("WX: Starting weather updates", LoadWeatherStations);

            //Now, go through all steps             
            foreach (KeyValuePair<String, LoadEventDelegate> Step in StartupSteps)
                if (!TryLoadStep(Step.Key, Step.Value, Startup))
                {
                    Application.Exit();
                    return false;
                }

            //Run our garbage collection
            GC.Collect();

            //Return successful completion
            return true;
        }

        /// <summary>
        /// Create a log file for value updates
        /// </summary>
        private static void CreateValueUpdateLogFile()
        {
            Data_Integration.ValueChangeLog = new StreamWriter(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "MacomberMap_ValueChangeLog_" + Environment.UserName + "_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".xml"), false, new UTF8Encoding(false));
            Data_Integration.ValueChangeLog.WriteLine("<?xml version=\"1.0\"?>");
            Data_Integration.ValueChangeLog.WriteLine("<MM_ValueUpdate_Log CreatedOn=\"" + XmlConvert.ToString(DateTime.Now, XmlDateTimeSerializationMode.Unspecified) + ">");
            Data_Integration.ValueChangeLog.AutoFlush = true;
        }


        /// <summary>
        /// Go through all substations, and find ones with no latitude/longitude. When we find them, estimate their positions
        /// </summary>
        private static void EstimateSubstationPositioning()
        {
            PointF StateBound = new PointF(MM_Repository.Counties["STATE"].Min_X, MM_Repository.Counties["STATE"].Max_Y);
            PointF TopLeft = StateBound;
            Random rnd = new Random();
            for (int a = 0; a < 2; a++)
                foreach (MM_Substation Sub in MM_Repository.Substations.Values)
                    if (float.IsNaN(Sub.Latitude))
                    {
                        //Build up a list of all lines connected to this substation
                        List<MM_Line> ConnectedLines = new List<MM_Line>();
                        foreach (MM_Line Line in MM_Repository.Lines.Values)
                            if (Line.Substation1 == Sub ^ Line.Substation2 == Sub)
                                ConnectedLines.Add(Line);

                        //If we have one or more, estimate its position
                        PointF AggPt = Point.Empty;
                        float TallyPt = 0;
                        foreach (MM_Line Line in ConnectedLines)
                            if (Line.Substation1 != Sub && !float.IsNaN(Line.Substation1.Latitude))
                            {
                                AggPt.X += Line.Substation1.LatLong.X;
                                AggPt.Y += Line.Substation1.LatLong.Y;
                                TallyPt++;
                            }
                            else if (Line.Substation2 != Sub && !float.IsNaN(Line.Substation2.Latitude))
                            {
                                AggPt.X += Line.Substation2.LatLong.X;
                                AggPt.Y += Line.Substation2.LatLong.Y;
                                TallyPt++;
                            }
                        //Now, add in a little bit of randomness
                        if (TallyPt > 0)
                            Sub.LatLong = new PointF(((AggPt.X / TallyPt) + (((float)rnd.NextDouble() - 0.5f) / 4f)), ((AggPt.Y / TallyPt) + (((float)rnd.NextDouble() - 0.5f) / 4f)));
                        else if (a == 1)
                        {
                            Sub.LatLong = TopLeft;
                            TopLeft.X += .05f;
                            if (TopLeft.X > MM_Repository.Counties["STATE"].Max_X)
                                TopLeft = new PointF(StateBound.X, TopLeft.Y + .05f);
                        }
                        if (!float.IsNaN(Sub.LatLong.X))
                            Program.LogError("CIM: Estimated substation {0} positioning at {1},{2}", Sub.Name, Sub.Latitude, Sub.Longitude);
                    }
        }

        /// <summary>
        /// Load our weather stations
        /// </summary>
        private static void LoadWeatherStations()
        {
            Weather.LoadWeatherStations();
        }


        /// <summary>
        /// Connect to the MMS database
        /// </summary>
        private static void LoadMMS()
        {
            //TODO: Create interfaces to market database
        }

        /// <summary>
        /// Connect to the historic database
        /// </summary>
        private static void LoadHistoricDatabase()
        {
            //Find our XML element corresponding to our historic name
            XmlElement xHist = MM_Repository.xConfiguration.SelectSingleNode("/Configuration/Databases/Database[@Name='" + HistoryServerName + "' and @Type='MM_Historic_Connector']") as XmlElement;
            DataConnections.Add(xHist.Attributes["Name"].Value, new Historic.MM_Historic_Connector(xHist));
        }


        /// <summary>
        /// Build our MMS table mappings
        /// </summary>
        private static void BuildMMSTableMappings()
        {
            //TODO: Create market database table mappings, if needed
        }

        private delegate void LoadEventDelegate();
        private delegate void LoadEventDelegateWithBoolean(bool BooleanTSystemWidess);
        /// <summary>
        /// Try a startup load step
        /// </summary>
        /// <param name="EventText">The text of the event</param>
        /// <param name="EventToRun">The delegate to attempt to run</param>
        /// <param name="Startup">Our startup form</param>
        /// <returns>The success state of the step</returns>
        private static bool TryLoadStep(String EventText, LoadEventDelegate EventToRun, Startup_Form Startup)
        {
            bool TryAgain = true;
            while (TryAgain)
                try
                {
                    Program.LogError(EventText);
                    EventToRun(); //MN//This can crash??? Why are we generating pop up below. error also describes this document line number 1261... however this is the catch it is finding - dest array not large enough check array size???
                    Application.DoEvents();
                    TryAgain = false;
                    return true;
                }
                catch (Exception ex)
                {
                    DialogResult Resp = Program.MessageBox("Error during step \"" + EventText + "\":\n\n " + ex.ToString(), Application.ProductName, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
                    if (Resp == DialogResult.Retry)
                        TryAgain = true;
                    else if (Resp == DialogResult.Ignore)
                        return true;
                    else
                        return false;
                }
            return false;
        }



        /// <summary>
        /// Go through all elements, and remove those for which we do not have permission
        /// </summary>
        private static void ApplyPermissions()
        {
            //TODO: Apply all premissions as appropriate          
        }

        /// <summary>
        /// Initialize the data integration model
        /// </summary>
        /// <param name="NetworkModelPath">The path to the model to be used</param>
        /// <param name="Startup">The startup form</param>
        /// <param name="OneLineDirectory"></param>                
        /// <param name="HistoryConnector">The connection to our historic server</param>
        public static void StartupSavecase(String NetworkModelPath, Startup_Form Startup, String OneLineDirectory, String HistoryConnector)
        {
            //Load our network model
            Program.LogError("MM: Loading MM Savecase");
            XmlDocument xNetworkModel = new XmlDocument();
            xNetworkModel.Load(NetworkModelPath);

            //Loading map data
            Program.LogError("MM: Loading XML Map data");
            LoadXMLData(xNetworkModel, Startup.Coordinates, OneLineDirectory);
            TestMode = false;

            //Connect to our history server
            Data_Integration.HistoryServerName = HistoryConnector;
            Data_Integration.LoadHistoricDatabase();

            //Create the map
            Program.LogError("MM: Starting up Map");
            Startup.CreateMap();

            Program.LogError("MM: Waiting for Map to complete loading");
            Startup.WaitForMap();
        }





        /// <summary>
        /// Save the full network model, as requested by the user.
        /// </summary>
        /// <param name="FileName">The file to output</param>
        /// <param name="Coordinates">The network map coordinates</param>
        public static void SaveXMLData(String FileName, MM_Coordinates Coordinates)
        {



            //Create our XML text writer
            XmlTextWriter xW = new XmlTextWriter(FileName, Encoding.UTF8);
            xW.Formatting = Formatting.Indented;
            xW.WriteStartDocument(true);

            //Write our network model header
            xW.WriteStartElement("NetworkModel");
            foreach (String Attr in Enum.GetNames(typeof(OverallIndicatorEnum)))
                xW.WriteAttributeString(Attr, XmlConvert.ToString(OverallIndicators[(int)Enum.Parse(typeof(OverallIndicatorEnum), Attr)]));

            foreach (MemberInfo mI in typeof(Data_Integration).GetMembers())
                if (mI.Name == "SaveTime")
                    MM_Serializable.WriteXMLData("SaveTime", DateTime.Now, xW);
                else if (mI is PropertyInfo && (mI as PropertyInfo).CanWrite)
                    MM_Serializable.WriteXMLData(mI.Name, (mI as PropertyInfo).GetValue(typeof(Data_Integration), null), xW);
                else if (mI is FieldInfo)
                    MM_Serializable.WriteXMLData(mI.Name, (mI as FieldInfo).GetValue(typeof(Data_Integration)), xW);


            //xW.WriteAttributeString("SaveTime", XmlConvert.ToString(DateTime.Now, XmlDateTimeSerializationMode.Unspecified));
            if (MM_Repository.ActiveView != null)
                xW.WriteAttributeString("View", MM_Repository.ActiveView.FullName);


            //Now, write out our data connectors
            foreach (KeyValuePair<String, MM_DataConnector> DataConn in DataConnections)
                xW.WriteAttributeString(DataConn.Key, DataConn.Value.ConnectorName);
            if (MMServer.BaseElement != null)
                xW.WriteAttributeString("MMServer", MMServer.BaseElement.Attributes["Name"].Value);
            NetworkSource.WriteXML(xW);


            //Write out our configuration XML            
            XmlElement DisplayParams = MM_Repository.xConfiguration["Configuration"];
            xW.WriteStartElement("Configuration");
            DisplayParams.WriteContentTo(xW);
            xW.WriteEndElement();

            //Write our network map coordinates
            Coordinates.WriteXML(xW);

            //Write out our weather information
            foreach (MM_WeatherStation WeatherStation in Weather.WeatherStations.Values)
                WeatherStation.WriteXML(xW);

            //Now, write out our boundaries
            foreach (MM_Boundary Bound in MM_Repository.Counties.Values)
                Bound.WriteXML(xW);

            //Now, write out our companies
            foreach (MM_Company Company in MM_Repository.Companies.Values)
                Company.WriteXML(xW);

            //Now, write out substations
            foreach (MM_Substation Substation in MM_Repository.Substations.Values)
                Substation.WriteXML(xW);

            //Now, go through the rest of our TEIDs, and add them in.
            foreach (MM_Element Elem in MM_Repository.TEIDs.Values)
                if (Elem is MM_Company == false && Elem is MM_Substation == false)
                {
                    if (Elem is MM_DCTie)
                        (Elem as MM_DCTie).AssociatedLine.WriteXML(xW);
                    Elem.WriteXML(xW);
                }

            //Write out our generation types
            foreach (MM_Generation_Type GenType in MM_Repository.GenerationTypes.Values)
                GenType.WriteXML(xW);

            //Now, write out all of our violations
            foreach (MM_AlarmViolation Viol in MM_Repository.Violations.Keys)
                Viol.WriteXML(xW);

            //Write out our notes
            foreach (MM_Note Note in MM_Repository.Notes.Values)
                Note.WriteXML(xW);

            //Now, write out our data sets
            DataSet OverallData = new DataSet();
            if (SystemValues != null)
                OverallData.Tables.Add(SystemValues);
            if (LoadForecast != null)
                OverallData.Tables.Add(LoadForecast);
            if (LoadForecastDates != null)
                OverallData.Tables.Add(LoadForecastDates);


            //Now, write out our processes
            foreach (MM_Process Proc in Processes.Values)
                Proc.WriteXML(xW);

            //Now, write our communications status
            foreach (MM_Communication_Linkage Comm in Communications.Values)
                Comm.WriteXML(xW);

            //Write our CSC/IROL liimts
            foreach (MM_Limit CSCIROL in CSCIROLLimits.Values)
                CSCIROL.WriteXML(xW);

            //Write out our reserves
            foreach (MM_Reserve Reserve in Reserves.Values)
                Reserve.WriteXML(xW);


            //Write out our list of forms
            foreach (MM_Form FormToWrite in RunningForms)
            {
                xW.WriteStartElement("Form");
                xW.WriteAttributeString("Type", FormToWrite.GetType().Name);
                xW.WriteAttributeString("Coordinates", FormToWrite.Left.ToString() + "," + FormToWrite.Top.ToString() + "," + FormToWrite.Width.ToString() + "," + FormToWrite.Height.ToString());
                xW.WriteAttributeString("Visible", FormToWrite.Visible.ToString());
                xW.WriteEndElement();
            }

            //Write a snapshot of threads
            foreach (Thread ThreadToWrite in RunningThreads)
                try
                {
                    xW.WriteStartElement("Thread");
                    xW.WriteAttributeString("Name", ThreadToWrite.Name);
                    xW.WriteAttributeString("Priority", ThreadToWrite.Priority.ToString());
                    xW.WriteAttributeString("State", ThreadToWrite.ThreadState.ToString());
                    xW.WriteEndElement();
                }
                catch (Exception ex)
                {
                    Program.WriteConsoleLine("Unable to save thread: " + ex.Message);
                }

            //Now close our document
            xW.WriteEndDocument();
            xW.Close();


            Program.MessageBox("Model save complete!", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);




        }

        /// <summary>
        /// This process is handled when the user requests a network model/savecase save.
        /// </summary>
        /// <param name="Coordinates">The coordinates to be exported</param>
        public static void SaveXMLDataRequest(MM_Coordinates Coordinates)
        {
            Thread SaveThread = new Thread(new ParameterizedThreadStart(SaveXMLUI));
            SaveThread.SetApartmentState(ApartmentState.STA);
            SaveThread.Name = "Model save";
            RunningThreads.Add(SaveThread);
            SaveThread.Start(Coordinates);
        }

        /// <summary>
        /// This provides the user with the save options, and runs accordingly.
        /// </summary>
        public static void SaveXMLUI(Object Coordinates)
        {
            SaveFileDialog s = new SaveFileDialog();
            s.Title = "Save a Macomber Map model savecase";
            s.Filter = "Macomber Map Network Model (*.MM_Model)|*.MM_Model";
            s.RestoreDirectory = true;
            if (s.ShowDialog() == DialogResult.OK)
                SaveXMLData(s.FileName, Coordinates as MM_Coordinates);
        }

        /// <summary>
        /// Locate an element. If it's not found, crete it, and queue it for later
        /// </summary>
        /// <param name="TEID">The TEID of the element</param>        
        /// <param name="AddIfNew"></param>
        /// <returns></returns>
        public static MM_Element LocateElement(Int32 TEID, bool AddIfNew)
        {
            if (MM_Repository.TEIDs.ContainsKey(TEID))
                return MM_Repository.TEIDs[TEID];
            else
            {
                MM_Element OutElement = new MM_Element();
                OutElement.TEID = TEID;
                if (AddIfNew)
                    MM_Repository.TEIDs.Add(TEID, OutElement);
                return OutElement;
            }
        }

        /// <summary>
        /// Locate an element of the requested type. If it's not found, crete it, and queue it for later
        /// </summary>
        /// <param name="TEID">The TEID of the element</param>
        /// <param name="DestinationType">The target type of the element</param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        /// <returns></returns>        
        public static MM_Element LocateElement(Int32 TEID, Type DestinationType, bool AddIfNew)
        {
            MM_Element FoundElement;
            if (TEID != 0 && MM_Repository.TEIDs.TryGetValue(TEID, out FoundElement))
                if (FoundElement.GetType().Equals(DestinationType) || DestinationType==typeof(MM_Element))
                    return FoundElement;
                else if (FoundElement.GetType() == typeof(MM_Unit) && (DestinationType == typeof(MM_LogicalUnit) || DestinationType == typeof(MM_CombinedCycleConfiguration)))                
                    return FoundElement;                                               
                else                    
                    throw new InvalidDataException("Searching for TEID " + TEID.ToString() + ", found type " + MM_Repository.TEIDs[TEID].GetType().Name + ", while looking for type " + DestinationType.Name);
            else
            {
                MM_Element OutElement = System.Activator.CreateInstance(DestinationType) as MM_Element;
                OutElement.TEID = TEID;
                if (TEID != 0 && AddIfNew)
                    MM_Repository.TEIDs.Add(TEID, OutElement);
                return OutElement;
            }
        }

        /// <summary>
        /// Return a new TEID
        /// </summary>
        /// <returns>A new unique TEID</returns>
        public static Int32 GetTEID()
        {
            Int32 InResult;
            do
                InResult = (Int32)TEIDRandomizer.Next(0, int.MaxValue);
            while (MM_Repository.TEIDs.ContainsKey(InResult));
            return InResult;
        }

        /// <summary>
        /// Locate an element of the requested type. If it's not found, crete it, and queue it for later
        /// </summary>
        /// <param name="xElem">The XML element to read</param>
        /// <param name="DestinationType">The target type of the element</param>
        /// <param name="AddIfNew"></param>
        /// <returns></returns>
        public static MM_Element LocateElement(XmlElement xElem, Type DestinationType, bool AddIfNew)
        {
            Int32 TEID = Convert.ToInt32(xElem.Attributes["TEID"].Value);
            MM_Element FoundElem;

            if (MM_Repository.TEIDs.TryGetValue(TEID, out FoundElem))
                if (FoundElem.GetType().Equals(DestinationType))
                {
                    FoundElem.ReadXML(xElem, AddIfNew);
                    return FoundElem;
                }
                else if (FoundElem.GetType() == typeof(MM_Unit) && DestinationType == typeof(MM_LogicalUnit))
                {
                    MM_LogicalUnit OutElement = System.Activator.CreateInstance(DestinationType) as MM_LogicalUnit;
                    OutElement.ReadXML(xElem, AddIfNew);

                    //Update references to this element
                    if (OutElement.PhysicalUnit != null)
                        OutElement.PhysicalUnit.LogicalUnits[Array.IndexOf(OutElement.PhysicalUnit.LogicalUnits, FoundElem)] = OutElement;
                    if (OutElement.Substation != null)
                        OutElement.Substation.Units[Array.IndexOf(OutElement.Substation.Units, FoundElem)] = OutElement;
                    MM_Repository.TEIDs[OutElement.TEID] = OutElement;
                    return OutElement;
                }
                else if (FoundElem.GetType() == typeof(MM_Unit) && DestinationType == typeof(MM_CombinedCycleConfiguration))
                {
                    MM_CombinedCycleConfiguration OutElement = System.Activator.CreateInstance(DestinationType) as MM_CombinedCycleConfiguration;
                    OutElement.ReadXML(xElem, AddIfNew);
                    
                    //Update references to this element                    
                    if (OutElement.Substation != null)
                        OutElement.Substation.Units[Array.IndexOf(OutElement.Substation.Units, FoundElem)] = OutElement;
                    MM_Repository.TEIDs[OutElement.TEID] = OutElement;
                    return OutElement;
                }
                else
                    throw new InvalidDataException("Searching for TEID " + TEID.ToString() + ", found type " + MM_Repository.TEIDs[TEID].GetType().Name + ", while looking for type " + DestinationType.Name);
            else
            {
                MM_Element OutElement = System.Activator.CreateInstance(DestinationType) as MM_Element;
                OutElement.TEID = TEID;
                OutElement.ReadXML(xElem, AddIfNew);
                if (AddIfNew)
                    MM_Repository.TEIDs.Add(TEID, OutElement);
                return OutElement;
            }
        }

        /// <summary>
        /// Load an XML configuration file from a stream, and parse it
        /// </summary>
        /// <param name="xDoc">The XML Document containing the model/state information</param>
        /// <param name="Coordinates">The coordinate information</param>
        /// <param name="OneLineDirectory"></param>
        public static void LoadXMLData(XmlDocument xDoc, MM_Coordinates Coordinates, String OneLineDirectory)
        {
            Data_Integration.UserName = Environment.UserName;
            Data_Integration.OneLineDirectory = OneLineDirectory;
            //Because we may not have the IDs for violations, make some up
            Random r = new Random();

            //Assign our data integration layer characteristics            
            foreach (XmlAttribute xAttr in xDoc.DocumentElement.Attributes)
                if (typeof(Data_Integration).GetField(xAttr.Name) != null && xAttr.Name != "CIMServer")
                    MM_Serializable.AssignValue(xAttr.Name, xAttr.Value, typeof(Data_Integration), true);
                else if (xAttr.Name == "View")
                    foreach (MM_Display_View View in MM_Repository.Views.Values)
                        if (View.FullName == xAttr.Value)
                            MM_Repository.ActiveView = View;

            Data_Integration.InitializationComplete = false;

            //Now go node by node
            foreach (XmlNode xElemNode in xDoc.DocumentElement.ChildNodes)
                if (xElemNode is XmlElement)
                {
                    XmlElement xElem = (XmlElement)xElemNode;
                    switch (xElem.Name)
                    {
                        case "MM_Data_Source":
                            Data_Integration.NetworkSource = new MM_Data_Source(xElem, true);
                            break;
                        case "State":                            
                        case "MM_Boundary":
                        case "County":
                            MM_Boundary Boundary = new MM_Boundary(xElem, true);
                            if (Boundary.TEID == 0)                                                            
                                MM_Repository.TEIDs.Add(Boundary.TEID = Data_Integration.GetTEID(), Boundary);
                            MM_Repository.Counties.Add(xElem.Attributes["Name"].Value, Boundary);                            
                            break;
                        case "Company":
                            MM_Company NewCompany = LocateElement(xElem, typeof(MM_Company), true) as MM_Company;
                            MM_Repository.Companies.Add(NewCompany.Alias, NewCompany);
                            break;
                        case "MM_Substation":
                        case "Substation":
                            MM_Substation NewSub = LocateElement(xElem, typeof(MM_Substation), true) as MM_Substation;
                            if (MM_Repository.Substations.ContainsKey(NewSub.Name))
                                Program.LogError("Duplicate substation name detected: " + NewSub.Name);
                            else
                                MM_Repository.Substations.Add(NewSub.Name, NewSub);
                            break;                       
                        case "Line":
                            MM_Line NewLine = LocateElement(xElem, typeof(MM_Line), true) as MM_Line;
                            if (MM_Repository.Lines.ContainsKey(NewLine.Name))
                                Program.LogError("Duplicate line name detected: " + NewLine.Name);
                            else
                                MM_Repository.Lines.Add(NewLine.Name, NewLine);

                            //Update our multiple segment list
                            if (NewLine.IsMultipleSegment)
                            {
                                if (MM_Repository.MultipleLineSegments.ContainsKey(NewLine.TEID))
                                {
                                    List<MM_Line> Lines = new List<MM_Line>(MM_Repository.MultipleLineSegments[NewLine.TEID]);
                                    Lines.Add(NewLine);
                                    MM_Repository.MultipleLineSegments[NewLine.TEID] = Lines.ToArray();
                                }
                                else
                                    MM_Repository.MultipleLineSegments.Add(NewLine.TEID, new MM_Line[] { NewLine });
                            }
                            break;
                        case "CombinedCycleConfiguration":
                            LocateElement(xElem, typeof(MM_CombinedCycleConfiguration), true);
                            break;
                        case "LogicalUnit":
                            LocateElement(xElem, typeof(MM_LogicalUnit), true);                           
                            break;
                        case "Unit":
                            LocateElement(xElem, typeof(MM_Unit), true);
                            break;
                        case "SPS":
                        case "RAP":
                            break;
                        case "Load":
                        case "LAAR":
                        case "LaaR":
                            MM_Load NewLoad = LocateElement(xElem, typeof(MM_Load), true) as MM_Load;
                            MM_Repository.Loads.Add(NewLoad.Substation.Name + "." + NewLoad.Name, NewLoad);
                            break;
                        case "MM_Generation_Type":

                            if (MM_Repository.GenerationTypes.ContainsKey(xElem.Attributes["Name"].Value))
                                MM_Repository.GenerationTypes[xElem.Attributes["Name"].Value].ReadXML(xElem, true);
                            else
                            {
                                MM_Generation_Type NewGen = new MM_Generation_Type(xElem, true);
                                MM_Repository.GenerationTypes.Add(NewGen.Name, NewGen);
                                if (!String.IsNullOrEmpty(NewGen.EMSName))
                                    MM_Repository.GenerationTypes.Add(NewGen.EMSName, NewGen);
                            }
                            break;
                        case "MM_Node":
                        case "Node":
                            MM_Node NewNode = LocateElement(xElem, typeof(MM_Node), true) as MM_Node;
                            break;
                        case "ElectricalBus":
                            MM_Electrical_Bus NewElectrical_Bus = LocateElement(xElem, typeof(MM_Electrical_Bus), true) as MM_Electrical_Bus;
                            String EBName = NewElectrical_Bus.Substation.Name + "." + NewElectrical_Bus.Name;
                            if (!MM_Repository.EBs.ContainsKey(EBName))
                                MM_Repository.EBs.Add(EBName, NewElectrical_Bus);
                            break;
                        case "BusbarSection":
                            MM_BusbarSection Bus = LocateElement(xElem, typeof(MM_BusbarSection), true) as MM_BusbarSection;
                            if (!MM_Repository.Busbars.ContainsKey(Bus.Substation.Name + "." + Bus.Name))
                                MM_Repository.Busbars.Add(Bus.Substation.Name + "." + Bus.Name, Bus);
                            break;
                        case "Transformer":
                            MM_Transformer NewTransformer = LocateElement(xElem, typeof(MM_Transformer), true) as MM_Transformer;
                            NewTransformer.Winding1.TEID = Convert.ToInt32(xElem.Attributes["Windings"].Value.Split(',')[0]);
                            NewTransformer.Winding2.TEID = Convert.ToInt32(xElem.Attributes["Windings"].Value.Split(',')[1]);
                            break;
                        case "TransformerWinding":
                            if (xElem.HasAttribute("Transformer"))
                            {
                                MM_Transformer XF = MM_Repository.TEIDs[Convert.ToInt32(xElem.Attributes["Transformer"].Value)] as MM_Transformer;
                                foreach (MM_TransformerWinding Winding in XF.Windings)
                                    if (Winding.TEID == Convert.ToInt32(xElem.Attributes["TEID"].Value))
                                    {
                                        Winding.ReadXML(xElem, true);
                                        Winding.Transformer = XF;
                                        Winding.Substation = XF.Substation;
                                    }
                            }
                            break;

                        case "Capacitor":
                        case "Reactor":
                            MM_ShuntCompensator NewSc = LocateElement(xElem, typeof(MM_ShuntCompensator), true) as MM_ShuntCompensator;
                            NewSc.ElemType = MM_Repository.FindElementType(xElem.Name);
                            break;
                        case "Contingency":
                            MM_Contingency NewContingency = new MM_Contingency(xElem, true);
                            MM_Repository.Contingencies.Add(NewContingency.Name, NewContingency);
                            break;
                        case "Violation":
                            MM_AlarmViolation NewViol = new MM_AlarmViolation(xElem, true);
                            if (NewViol.ViolatedElement.TEID != 0)
                                CheckAddViolation(NewViol);

                            break;
                        case "Breaker":
                        case "Switch":
                            LocateElement(xElem, typeof(MM_Breaker_Switch), true);
                            break;
                        case "Element":
                            MM_Element NewElement = new MM_Element(xElem, true);
                            MM_Repository.TEIDs.Add(NewElement.TEID, NewElement);
                            break;
                        case "DataSet":

                            /*DataSet NewSet = new DataSet(xElem.Attributes["Name"].Value);
                            using (MemoryStream mS = new MemoryStream(Encoding.UTF8.GetBytes(xElem.InnerXml), false))
                            {
                                mS.Seek(0, SeekOrigin.Begin);
                                NewSet.ReadXml(mS, XmlReadMode.ReadSchema);
                            }
                            if (xElem.Attributes["Name"].Value == "Overall")
                            {
                                SystemValues = NewSet.Tables["SystemValues"];
                                LoadForecast = NewSet.Tables["LF_LFMOM_H.EMS"];
                                LoadForecastDates = NewSet.Tables["LF_LFMOM_H-Dates.EMS"];
                            }
                            else
                                ActiveData.Add(xElem.Attributes["Name"].Value, NewSet);*/
                            break;
                        case "DCTie":
                            MM_DCTie DCTie = LocateElement(xElem, typeof(MM_DCTie), true) as MM_DCTie;
                            if (!MM_Repository.DCTies.ContainsKey(DCTie.Name))
                                MM_Repository.DCTies.Add(DCTie.Name, DCTie);
                            if (!MM_Repository.Lines.ContainsKey(DCTie.Name))
                                MM_Repository.Lines.Add(DCTie.Name, DCTie);
                            break;
                        case "MM_Note":
                        case "Note":
                            MM_Note ThisNote = new MM_Note(xElem);
                            HandleNoteEntry(ThisNote);
                            break;
                        case "Configuration":
                            break;
                        case "MM_Coordinates":
                            Coordinates.ReadXML(xElem, true);
                            break;
                        case "MM_Process":
                            if (xElem.HasAttribute("Name"))
                                Processes.Add(xElem.Attributes["Name"].Value, new MM_Process(xElem));
                            break;
                        case "Form":
                        case "Thread":
                            break;
                        case "MM_Communication":
                            MM_Communication_Linkage NewComm = new MM_Communication_Linkage(xElem, true);
                            Communications.Add(NewComm.SCADAName, NewComm);
                            break;
                        case "MM_Limit":
                            MM_Limit NewLimit = new MM_Limit(xElem, true);
                            CSCIROLLimits.Add(CSCIROLLimits.Count, NewLimit);
                            break;
                        case "MM_Reserve":
                        case "Reserve":
                            Reserves.Add(xElem.Attributes["Category"].Value + "-" + xElem.Attributes["Name"].Value, new MM_Reserve(xElem, true));
                            break;
                        case "Island":
                            MM_Island NewIsland = new MM_Island(xElem, true);
                            MM_Repository.Islands.Add(NewIsland.ID, NewIsland);
                            break;
                        case "RemedialActionScheme":
                            MM_RemedialActionScheme NewRas = new MM_RemedialActionScheme(xElem, true);
                            MM_Repository.RASs.Add(NewRas.Name, NewRas);
                            
                            break;
                        case "Blackstart_Corridor":
                            MM_Blackstart_Corridor NewCorridor = new MM_Blackstart_Corridor(xElem);                            
                            NewCorridor.TEID = Data_Integration.GetTEID();
                            MM_Repository.TEIDs.Add(NewCorridor.TEID, NewCorridor);                        
                            MM_Repository.BlackstartCorridors.Add(NewCorridor.Name, NewCorridor);
                            break;
                        default:
                            Program.LogError("Unknown type handled: " + xElem.Name);
                            break;
                    }
                }


            //Run through our elements a second time, picking up downstream-linked elements
            foreach (XmlElement xElem in xDoc.DocumentElement.ChildNodes)
                switch (xElem.Name)
                {

                    case "SPS":
                    case "RAP":
                        MM_RemedialActionScheme RAS = new MM_RemedialActionScheme(xElem, true);// LocateElement(xElem, typeof(MM_RemedialActionScheme)) as MM_RemedialActionScheme;
                        break;
                }
            RollUpElementsToSubstation();
            CommLoaded = true;
            //MM_Repository.Counties["STATE"].Coordinates = BetterStateBoundary();
        }



        /// <summary>
        /// Go through all units and loads, and assign to their respective substations
        /// </summary>

        private static void RollUpElementsToSubstation()
        {
            bool IntegrateKV = true;

            //First, go through all of our elements, and pull them in.
            MM_Repository.Counties.Clear();
            //MM_Repository.Contingencies.Clear();
            MM_Repository.Companies.Clear();
            MM_Repository.BlackstartCorridors.Clear();
            MM_Repository.DCTies.Clear();
            MM_Repository.EBs.Clear();
            MM_Repository.Lines.Clear();
            MM_Repository.Loads.Clear();
            MM_Repository.RASs.Clear();
            MM_Repository.ShuntCompensators.Clear();
            MM_Repository.Units.Clear();
            MM_Repository.Substations.Clear();
            List<Int32> CorridorElemsToRemove = new List<int>();

            try
            {
                //Update our violation by voltage collection
                foreach (MM_AlarmViolation_Type AlarmViolType in MM_Repository.ViolationTypes.Values)
                {
                    foreach (MM_Element_Type ElemType in MM_Repository.ElemTypes.Values)
                        if (!AlarmViolType.ViolationsByElementType.ContainsKey(ElemType))
                            AlarmViolType.ViolationsByElementType.Add(ElemType, new Dictionary<MM_AlarmViolation, MM_AlarmViolation>());

                    foreach (MM_KVLevel KVLevel in MM_Repository.KVLevels.Values)
                        if (!AlarmViolType.ViolationsByVoltage.ContainsKey(KVLevel))
                            AlarmViolType.ViolationsByVoltage.Add(KVLevel, new Dictionary<MM_AlarmViolation, MM_AlarmViolation>());
                }
            }
            catch (Exception)
            {
                //Program.LogError(ex.ToString());  //nataros - why are soo many null ones ending up here??? what is going on ????? O_o //commented out to speed things up - mn - 6-4-13
            }

            foreach (MM_Element Elem in new List<MM_Element>(MM_Repository.TEIDs.Values))
                try
                {
                    if (Elem.Name != null)
                    {
                        if (Elem is MM_Boundary)
                            MM_Repository.Counties.Add(Elem.Name, Elem as MM_Boundary);
                        //else if (Elem is MM_Island)
                        //    MM_Repository.Islands.Add((Elem as MM_Island).ID, Elem as MM_Island);
                        else if (Elem is MM_Substation)
                        {
                            //   if (Elem.TEID == 284598)
                            //     Console.WriteLine();                            
                            MM_Repository.Substations.Add(Elem.Name, Elem as MM_Substation);
                            if (!(Elem as MM_Substation).County.Substations.Contains(Elem as MM_Substation))
                                (Elem as MM_Substation).County.Substations.Add(Elem as MM_Substation);
                            MM_Company[] LoadOperators = (Elem as MM_Substation).Operators;
                        }
                        else if (Elem is MM_Contingency)
                            MM_Repository.Contingencies.Add(Elem.Name, Elem as MM_Contingency);
                        else if (Elem is MM_Company)
                        {
                            //Add in our company, and if it matches our permissions area, set it up
                            MM_Repository.Companies.Add(Elem.Name, Elem as MM_Company);
                            if (Data_Integration.MMServer != null && Array.IndexOf(Data_Integration.MMServer.UserOperatorships, Elem.TEID) != -1)
                            {
                                MM_Repository.OverallDisplay.DisplayCompany = Elem as MM_Company;
                                MMServer.OperatorshipCompanies.Add(Elem as MM_Company);
                            }
                        }
                        else if (Elem is MM_Blackstart_Corridor)
                            MM_Repository.BlackstartCorridors.Add(Elem.Name, Elem as MM_Blackstart_Corridor);
                        else if (Elem is MM_Blackstart_Corridor_Element)
                            CorridorElemsToRemove.Add(Elem.TEID);
                        else if (Elem is MM_DCTie)
                            MM_Repository.DCTies.Add(Elem.Name, Elem as MM_DCTie);
                        else if (Elem is MM_Electrical_Bus)
                            MM_Repository.EBs.Add(Elem.Substation.Name + "." + Elem.Name, Elem as MM_Electrical_Bus);
                        else if (Elem is MM_Line)
                            MM_Repository.Lines.Add(Elem.Name, Elem as MM_Line);
                        else if (Elem is MM_Load)
                            MM_Repository.Loads.Add(Elem.Substation.Name + "." + Elem.Name, Elem as MM_Load);
                        else if (Elem is MM_RemedialActionScheme)
                            MM_Repository.RASs.Add(Elem.Name, Elem as MM_RemedialActionScheme);
                        else if (Elem is MM_ShuntCompensator)
                            MM_Repository.ShuntCompensators.Add(Elem.Substation.Name + "." + Elem.Name, Elem as MM_ShuntCompensator);
                        else if (Elem is MM_Unit && Elem.Substation != null)
                            MM_Repository.Units.Add(Elem.Substation.Name + "." + Elem.Name, Elem as MM_Unit);
                        else if (Elem is MM_KVLevel)
                        {
                        }
                        else if (Elem is MM_BusbarSection)
                            if (MM_Repository.Busbars.ContainsKey(Elem.Substation.Name + "." + Elem.Name))
                            {} //Program.LogError("Duplicate busbar section detected by name: " + Elem.Substation.Name + "." + Elem.Name);
                            else
                                MM_Repository.Busbars.Add(Elem.Substation.Name + "." + Elem.Name, Elem as MM_BusbarSection);
                        else if (Elem is MM_Node)
                            if ((Elem as MM_Node).BusbarSection != null)
                            {
                                (Elem as MM_Node).BusbarSection.BusNumber = Elem.BusNumber;
                                if (!MM_Repository.BusNumbers.ContainsKey((Elem as MM_Node).BusbarSection.BusNumber))
                                    MM_Repository.BusNumbers.Add(Elem.BusNumber, (Elem as MM_Node).BusbarSection);
                                else
                                    MM_Repository.BusNumbers[Elem.BusNumber] = (Elem as MM_Node).BusbarSection;
                            }
                            else if (Elem.BusNumber != -1)
                            {
                                MM_BusbarSection NewBus = new MM_BusbarSection();
                                NewBus.Substation = Elem.Substation;
                                NewBus.Name = Elem.Name;
                                NewBus.Owner = Elem.Owner;
                                NewBus.Operator = Elem.Operator;
                                NewBus.BusNumber = Elem.BusNumber;
                                NewBus.TEID = Data_Integration.GetTEID();
                                NewBus.KVLevel = Elem.KVLevel;
                                MM_Repository.TEIDs.Add(NewBus.TEID, NewBus);
                                (Elem as MM_Node).BusbarSection = NewBus;
                                NewBus.Node = Elem as MM_Node;
                                MM_Repository.BusNumbers.Add(Elem.BusNumber, NewBus);
                            }
                            else if (Elem is MM_Blackstart_Corridor_Element || Elem is MM_Transformer || Elem is MM_Breaker_Switch || Elem is MM_TransformerWinding || Elem is MM_Node || Elem.ElemType == null || Elem.ElemType.Name == "Unknown")
                            { }
                            else
                                Console.WriteLine("Unknown type: " + Elem.GetType().Name + " " + Elem.TEID.ToString("#,##0"));

                    }
                }
                catch (Exception)
                {
                    //Program.LogError("Error in rollup for " + Elem.GetType().Name + " " + Elem.TEID.ToString("#,##0") + ": " + ex.ToString()); //commented out to speed up start up - we know we have an issue here however it is not a real issue so we should be fine, just ignore for now and fix later - mn - 6-4-13
                }

            //Swap out our DC Ties for their equivalent lines - 30120607 - mn/ml
            foreach (MM_DCTie Tie in MM_Repository.DCTies.Values)
            {
                MM_Repository.Lines[Tie.AssociatedLine.Name] = Tie;
                MM_Repository.TEIDs[Tie.AssociatedLine.TEID] = Tie;
                //MM_Repository.Lines.Remove(Tie.AssociatedLine.Name);
                //MM_Repository.TEIDs.Remove(Tie.AssociatedLine.TEID);
                Tie.Counties = Tie.AssociatedLine.Counties;
                Tie.Coordinates = Tie.AssociatedLine.Coordinates;
                Tie.TieDescriptor = Tie.Name;
                Tie.IntegrateFromLine(Tie.AssociatedLine);
                //Tie.Coordinates = Tie.AssociatedLine.Coordinates;

                //since we dont recompute the lines anymore, we need this here to get 'new' coords after making line go outside state border to fix model 
                //for easier viewing for our users. - //MN//20130607
                Tie.Coordinates = new PointF[] { Tie.AssociatedLine.ConnectedStations[0].LatLong, Tie.AssociatedLine.ConnectedStations[1].LatLong };
                Tie.AssociatedLine.Coordinates = Tie.Coordinates;
                
 
                //Tie.AssociatedLine.KVLevel = Tie.KVLevel;
            }

            

            //Remove our blackstart corridor elements
            foreach (int ToRemove in CorridorElemsToRemove)
                MM_Repository.TEIDs.Remove(ToRemove);

            //Now trim excess memory
            foreach (MM_Boundary Bound in MM_Repository.Counties.Values)
                Bound.Substations.TrimExcess();

            //Build our dictionaries
            Dictionary<MM_Substation, List<MM_Unit>> Units = new Dictionary<MM_Substation, List<MM_Unit>>();
            Dictionary<MM_Substation, List<MM_Load>> Loads = new Dictionary<MM_Substation, List<MM_Load>>();
            Dictionary<MM_Substation, List<MM_Transformer>> Transformers = new Dictionary<MM_Substation, List<MM_Transformer>>();
            Dictionary<MM_Substation, List<MM_BusbarSection>> Busbars = new Dictionary<MM_Substation, List<MM_BusbarSection>>();
            Dictionary<MM_Substation, List<MM_ShuntCompensator>> ShuntCompensators = new Dictionary<MM_Substation, List<MM_ShuntCompensator>>();

            //Go through all TEIDs
            foreach (MM_Element Elem in MM_Repository.TEIDs.Values)
                if (Elem.TEID > 0)
                    try
                    {
                        if (IntegrateKV && Elem.Substation != null && Elem.KVLevel != null && !Elem.Substation.KVLevels.Contains(Elem.KVLevel))
                            Elem.Substation.KVLevels.Add(Elem.KVLevel);
                        else if (IntegrateKV && Elem is MM_Line)
                        {
                            foreach (MM_Substation LineSub in (Elem as MM_Line).ConnectedStations)
                                if (!LineSub.KVLevels.Contains(Elem.KVLevel))
                                    LineSub.KVLevels.Add(Elem.KVLevel);
                        }
                        if (Elem is MM_Unit)
                        {
                            if (!Units.ContainsKey(Elem.Substation))
                                Units.Add(Elem.Substation, new List<MM_Unit>());
                            if (!Units[Elem.Substation].Contains(Elem as MM_Unit))
                                Units[Elem.Substation].Add(Elem as MM_Unit);
                        }
                        else if (Elem is MM_Load)
                        {
                            if (!Loads.ContainsKey(Elem.Substation))
                                Loads.Add(Elem.Substation, new List<MM_Load>());
                            Loads[Elem.Substation].Add(Elem as MM_Load);
                        }
                        else if (Elem is MM_Transformer)
                        {
                            if (!Transformers.ContainsKey(Elem.Substation))
                                Transformers.Add(Elem.Substation, new List<MM_Transformer>());
                            Transformers[Elem.Substation].Add(Elem as MM_Transformer);
                        }
                        else if (Elem is MM_BusbarSection && Elem.Substation != null)
                        {
                            if (!Busbars.ContainsKey(Elem.Substation))
                                Busbars.Add(Elem.Substation, new List<MM_BusbarSection>());
                            Busbars[Elem.Substation].Add(Elem as MM_BusbarSection);
                        }
                        else if (Elem is MM_ShuntCompensator)
                        {
                            if (!ShuntCompensators.ContainsKey(Elem.Substation))
                                ShuntCompensators.Add(Elem.Substation, new List<MM_ShuntCompensator>());
                            ShuntCompensators[Elem.Substation].Add(Elem as MM_ShuntCompensator);
                        }
                    }
                    catch (Exception ex)
                    {
                        Program.LogError(ex);
                    }

            //Now pull out our lists
            foreach (KeyValuePair<MM_Substation, List<MM_Unit>> UnitCollection in Units)
                UnitCollection.Key.Units = UnitCollection.Value.ToArray();
            foreach (KeyValuePair<MM_Substation, List<MM_Load>> LoadCollection in Loads)
                LoadCollection.Key.Loads = LoadCollection.Value.ToArray();
            foreach (KeyValuePair<MM_Substation, List<MM_Transformer>> TransformerCollection in Transformers)
                TransformerCollection.Key.Transformers = TransformerCollection.Value.ToArray();
            foreach (KeyValuePair<MM_Substation, List<MM_BusbarSection>> BusCollection in Busbars)
                BusCollection.Key.BusbarSections = BusCollection.Value.ToArray();
            foreach (KeyValuePair<MM_Substation, List<MM_ShuntCompensator>> SCCollection in ShuntCompensators)
                SCCollection.Key.ShuntCompensators = SCCollection.Value.ToArray();

            //Now, recompute our components
            foreach (MM_Substation Sub in MM_Repository.Substations.Values)
                Sub.ElemTypes.Clear();

            foreach (MM_Element Elem in MM_Repository.TEIDs.Values)
                if (Elem.Substation != null && !Elem.Substation.ElemTypes.Contains(Elem.ElemType))
                    Elem.Substation.ElemTypes.Add(Elem.ElemType);

            //Roll up our blackstart elements
            foreach (MM_Blackstart_Corridor bsc in MM_Repository.BlackstartCorridors.Values)
                foreach (MM_Blackstart_Corridor_Target bsct in bsc.Blackstart_Targets)
                    foreach (MM_Blackstart_Corridor_Element Elem in bsct.Elements)
                    {
                        if (Elem.Substation != null)
                            Elem.Substation.IsBlackstart = true;
                        if (Elem.AssociatedElement is MM_Line)
                            (Elem.AssociatedElement as MM_Line).IsBlackstart = true;
                    }
            //Make sure that a boundary doesn't have duplicates of substations
            /* Dictionary<MM_Substation, int> Subs = new Dictionary<MM_Substation, int>();
             foreach (MM_Boundary Bound in MM_Repository.Counties.Values)
             {
                 Subs.Clear();
                 foreach (MM_Substation Sub in Bound.Substations)
                     if (!Subs.ContainsKey(Sub))
                         Subs.Add(Sub, 0);
                 Bound.Substations.Clear();
                 Bound.Substations.AddRange(Subs.Keys);
             }
             */

            //Roll up our contingencies
            foreach (MM_Contingency Ctg in MM_Repository.Contingencies.Values)
                Ctg.RollUpCountiesAndVoltages();
        }

        private static void CheckElement(XmlElement Element, String AttributeName, ref float OutValue)
        {
            if (Element.HasAttribute(AttributeName))
                OutValue = MM_Converter.ToSingle(Element.Attributes[AttributeName].Value);

        }



        #region Violation collection modification



        /// <summary>
        /// Check a violation, and add it if it's not in the model yet
        /// </summary>
        /// <param name="Viol"></param>
        public static void CheckAddViolation(MM_AlarmViolation Viol)
        {
            if (Data_Integration.Permissions.PublicUser== true)
                return;
            try
            {
                MM_AlarmViolation FoundViol;


                //If our violation exists in our archived folder, then it's been taken care of and should be ignored.
                if (MM_Repository.ArchivedViolations.ContainsKey(Viol))
                {
                    return;
                    
                    //do nothing for now, to add time and test idea
                }
                //return; // test - nataros


                if (MM_Repository.Violations.TryGetValue(Viol, out FoundViol))
                {
                    if (!String.Equals(Viol.Text, FoundViol.Text) || Viol.EventTime != FoundViol.EventTime || Viol.New != FoundViol.New)
                    {
                        FoundViol.Text = Viol.Text;
                        FoundViol.EventTime = Viol.EventTime;
                        FoundViol.New = Viol.New;
                        FoundViol.PostCtgValue = Viol.PostCtgValue;
                        FoundViol.PreCtgValue = Viol.PreCtgValue;
                        FoundViol.ViolatedElement.TriggerValueChange();
                        if (ViolationModified != null)
                            ViolationModified(FoundViol);
                    }
                    return;
                }
                if (!Viol.Type.Violations.ContainsKey(Viol))
                    Viol.Type.Violations.Add(Viol, Viol);


                Dictionary<MM_AlarmViolation, MM_AlarmViolation> FoundViols;
                if (Viol.ViolatedElement.KVLevel != null)
                {
                    if (!Viol.Type.ViolationsByVoltage.TryGetValue(Viol.ViolatedElement.KVLevel, out FoundViols))
                        Viol.Type.ViolationsByVoltage.Add(Viol.ViolatedElement.KVLevel, FoundViols = new Dictionary<MM_AlarmViolation, MM_AlarmViolation>());
                    if (!FoundViols.ContainsKey(Viol))
                        FoundViols.Add(Viol, Viol);
                }
                if (Viol.ViolatedElement.ElemType != null)
                {
                    if (!Viol.Type.ViolationsByElementType.TryGetValue(Viol.ViolatedElement.ElemType, out FoundViols))
                        Viol.Type.ViolationsByElementType.Add(Viol.ViolatedElement.ElemType, FoundViols = new Dictionary<MM_AlarmViolation, MM_AlarmViolation>());

                    if (!FoundViols.ContainsKey(Viol))
                        FoundViols.Add(Viol, Viol);
                }

                /*if ((!Viol.Type.ViolationsByElementType.ContainsKey(Viol.ViolatedElement.ElemType)))
                    Program.LogError("Unable to add {0} {1} to violation by element type collection, because the element type collection didn't include {0}s", Viol.ViolatedElement.ElemType.Name, Viol.ViolatedElement.Name);
                else if (!Viol.Type.ViolationsByElementType[Viol.ViolatedElement.ElemType].ContainsKey(Viol))
                    Viol.Type.ViolationsByElementType[Viol.ViolatedElement.ElemType].Add(Viol, Viol);
                */
                //Add this violation to the violated element's collection of violations
                if (!Viol.ViolatedElement.Violations.ContainsKey(Viol))
                    Viol.ViolatedElement.Violations.Add(Viol, Viol);

                //If we have a breaker-to-breaker path, update accordingly
                if (Viol.ViolatedElement.Contingencies != null)
                    foreach (MM_Contingency Ctg in Viol.ViolatedElement.Contingencies)
                        if (!Ctg.Violations.ContainsKey(Viol))
                            Ctg.Violations.Add(Viol, Viol);

                //If we have an associated substation, add this violation to the list
                if (Viol.ViolatedElement.Substation != null && !Viol.ViolatedElement.Substation.Violations.ContainsKey(Viol))
                    Viol.ViolatedElement.Substation.Violations.Add(Viol, Viol);


                //If we have a contingency definition, add this violation to the list
                if (Viol.Contingency != null)
                {
                    if (!Viol.Contingency.Violations.ContainsKey(Viol))
                        Viol.Contingency.Violations.Add(Viol, Viol);
                }



                //Now add it to the master list
                MM_Repository.Violations.Add(Viol, Viol);
                if (ViolationAdded != null)
                    ViolationAdded(Viol);

                Viol.ViolatedElement.TriggerValueChange();

                if (Viol.Type.SpeakViolation && UseSpeech)
                    ThreadPool.QueueUserWorkItem(new WaitCallback(SpeakText), Viol);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }


        }

        /// <summary>
        /// Speak text in a new thread
        /// </summary>
        /// <param name="state">The violation to be spoken</param>
        private static void SpeakText(Object state)
        {
            //Pull in our violation
            MM_AlarmViolation Viol = state as MM_AlarmViolation;

            //Create our outgoing text
            if (Viol.ViolatedElement.Substation != null && Viol.ViolatedElement.KVLevel != null)
                SpeakText(PrepareSpeech(Viol.Type.Name + " violation for " + Viol.ViolatedElement.KVLevel.Name + " " + Viol.ViolatedElement.ElemType.Name + "  in substation " + Viol.ViolatedElement.Substation.LongName));
            else if (Viol.ViolatedElement is MM_Line)
                SpeakText(PrepareSpeech(Viol.Type.Name + " violation for " + Viol.ViolatedElement.KVLevel.Name + " line between " + (Viol.ViolatedElement as MM_Line).ConnectedStations[0].LongName + " and " + (Viol.ViolatedElement as MM_Line).ConnectedStations[1].LongName));
            else if (Viol.ViolatedElement is MM_Substation)
                SpeakText(PrepareSpeech(Viol.Type.Name + " violation for substation " + (Viol.ViolatedElement as MM_Substation).LongName));



            //SpeakText(PrepareSpeech(state as string));           
        }

        /// <summary>
        /// Speak text
        /// </summary>
        /// <param name="TextToSpeak"></param>
        private static void SpeakText(String TextToSpeak)
        {
            Program.LogError("Speech: " + TextToSpeak);
            Speech.Rate = 3;
            System.Media.SystemSounds.Exclamation.Play();
            Speech.SpeakAsync(TextToSpeak);
        }

        /// <summary>
        /// Cancel the queued text to be spoken.
        /// </summary>
        public static void HandleSpeechRequest()
        {
            Speech.SpeakAsyncCancelAll();
        }


        /// <summary>
        /// Prepare text to be readable as a string
        /// </summary>
        /// <param name="InString">The incoming string</param>
        /// <returns></returns>
        private static string PrepareSpeech(String InString)
        {
            StringBuilder sB = new StringBuilder();
            for (int a = 0; a < InString.Length; a++)
                if (Char.IsNumber(InString[a]))
                    sB.Append(InString[a].ToString() + " ");
                else if (Char.IsUpper(InString[a]) && a > 0 && Char.IsLetter(InString[a - 1]))
                    sB.Append(" " + InString[a].ToString());
                else if (a == 'v' && InString.IndexOf("ville", a) == a)
                    sB.Append("-v");
                else
                    sB.Append(InString[a]);
            return sB.ToString();
        }


        /// <summary>
        /// Update an existing violation (text/date)
        /// </summary>
        /// <param name="Viol">The violation to be updated</param>
        /// <param name="ViolText">The text of the violation</param>
        /// <param name="ViolDate">The date/time of the violation</param>
        public static void UpdateViolation(MM_AlarmViolation Viol, string ViolText, DateTime ViolDate)
        {
            Viol.EventTime = ViolDate;
            Viol.Text = ViolText;
            if (ViolationModified != null)
                ViolationModified(Viol);
        }

        /// <summary>
        /// Remove a violation from the collection
        /// </summary>
        /// <param name="Violation">The violation to be removed</param>
        public static void RemoveViolation(MM_AlarmViolation Violation)
        {
            //Remove the violation from the list
            MM_Repository.Violations.Remove(Violation);


            //Remove the violation from the collection by type            
            Violation.Type.Violations.Remove(Violation);

            //Remove the violation from the collection by voltage
            Dictionary<MM_AlarmViolation, MM_AlarmViolation> FoundDictionary;
            if (Violation.KVLevel != null && Violation.Type.ViolationsByVoltage.TryGetValue(Violation.KVLevel, out FoundDictionary))
                FoundDictionary.Remove(Violation);

            //Remove the violation from the collection by element type
            if (Violation.Type.ViolationsByElementType.TryGetValue(Violation.ViolatedElement.ElemType, out FoundDictionary))
                FoundDictionary.Remove(Violation);

            //Remove the violation from the particular element
            Violation.ViolatedElement.Violations.Remove(Violation);

            //Remove the violation from the substation collection
            if (Violation.ViolatedElement.Substation != null)
                Violation.ViolatedElement.Substation.Violations.Remove(Violation);

            //If the violated element is in a trace, remove it
            if (Violation.ViolatedElement.Contingencies != null)
                foreach (MM_Contingency Ctg in Violation.ViolatedElement.Contingencies)
                    Ctg.Violations.Remove(Violation);

            //Remove the violation from the contingency
            if (Violation.Contingency != null)
                Violation.Contingency.Violations.Remove(Violation);

            if (ViolationRemoved != null)
                ViolationRemoved(Violation);
            Violation.ViolatedElement.TriggerValueChange();
            
            /*This following archived violations should only function for non-ots as removed violations never return.  
             *This should be addressed in the future, however it is as was in 'old' mmap application. - 20130611 - mn/ym */
            if(Permissions.OTS==false)
                MM_Repository.ArchivedViolations.Add(Violation, Violation); 
            //Program.LogError("Violation archived: " + Violation.ToString()); //commented out to speed up alarms - mn 6-4-13
        }
        #endregion

        #region Overall information
        /// <summary>
        /// The overall indicators stored and updated by the MM system
        /// </summary>
        public enum OverallIndicatorEnum
        {
            /// <summary>ACE</summary>
            ACE,
            /// <summary>Frequency</summary>
            Frequency,
            /// <summary>Total Generation</summary>
            Gen,
            /// <summary>Total Load</summary>
            Load,
            /// <summary>Total Wind Generation</summary>
            Wind,
            /// <summary>Load Res</summary>
            LoadRes,
            /// <summary>DCTie incoming MW</summary>
            DCTieIn,
            /// <summary>DCTie outgoing MW</summary>
            DCTieOut,
            /// <summary>The average LMP</summary>
            AverageLMP,
            /// <summary>The maximimum absolute difference between the average and its maxima or minima</summary>
            LMPSpread,
            /// <summary>The highest LMP</summary>
            MaximumLMP,
            /// <summary>The lowest LMP</summary>
            MinimumLMP,
            /// <summary>Physical responsive</summary>
            PRC,
            /// <summary>The remaining capacity for generation (to HASL)</summary>
            GenCapacity,
            /// <summary>The remaining emergency capacity for generation (to HSL)</summary>
            GenEmrgCapacity,
            /// <summary>The number of islands on the system</summary>
            IslandCount
        }

        /// <summary>Overall indicators on the state of the network</summary>
        public static float[] OverallIndicators = new float[] { float.NaN, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN };

        /// <summary>Our collection of StudySystem cases</summary>
        public static SortedDictionary<String, String[]> Savecases;

        /// <summary>The collection of system values</summary>
        public static DataTable SystemValues = null;

        /// <summary>The color that ACE should be assigned</summary>
        public static Color ACEColor = Color.White;
        #endregion

        /// <summary>
        /// Determine whether a column exists in a table. If not, create it.
        /// </summary>
        /// <param name="InTable">The data table</param>
        /// <param name="ColumnName">The column name</param>
        /// <param name="ColumnType">The column type</param>
        private static void ValidateColumn(DataTable InTable, String ColumnName, Type ColumnType)
        {
            if (!InTable.Columns.Contains(ColumnName))
                InTable.Columns.Add(ColumnName, ColumnType);
        }

        /// <summary>
        /// Add a series of system-level values to our collection
        /// </summary>
        /// <param name="RTGenTime">The time stamp for RTGEN</param>
        /// <param name="Wind">Current wind value</param>
        /// <param name="TotalGen">Total generation</param>
        public static void AddSystemValues(float Wind, float TotalGen, DateTime RTGenTime)
        {
            //nataros - we never addSystemValues, this is likley the root issue, at least a good chance? To look at more.
            //very old mmap stand along code which is missing in this mmap client is somehting like the following:
            //    Data_Integration.AddSystemValues(dr, Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.Wind], Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.Gen]);
            // //Go through and update our frequency deviation to show frequency
            // foreach (DataRow dr in HistoricData.Rows)
            // {
            //     dr["FHZ"] = Convert.ToSingle(dr["FHZ"]) + 60f;
            //     Data_Integration.AddSystemValues(dr, float.NaN, float.NaN);
            // }
            
            //First, create our system values table if needed
            if (SystemValues == null)
                SystemValues = new DataTable("SystemValues");

            //Now, validate all columns
            foreach (String oInd in Enum.GetNames(typeof(Data_Integration.OverallIndicatorEnum)))
                ValidateColumn(SystemValues, oInd, typeof(Single));
            ValidateColumn(SystemValues, "Wind", typeof(Single));
            ValidateColumn(SystemValues, "WindPercent", typeof(Single));
            ValidateColumn(SystemValues, "TAGCLAST", typeof(DateTime));
            //Now, update our tables
            lock (SystemValues)
            {
                if (SystemValues.Rows.Count == MM_Repository.OverallDisplay.SystemValueCount)
                    SystemValues.Rows.RemoveAt(0);
                if (SystemValues.Rows.Count == 0 || Convert.ToDateTime(SystemValues.Rows[SystemValues.Rows.Count - 1]["TAGCLAST"]) != RTGenTime)
                {
                    DataRow NewRow = SystemValues.Rows.Add();
                    foreach (Data_Integration.OverallIndicatorEnum enumIndicator in Enum.GetValues(typeof(Data_Integration.OverallIndicatorEnum)))
                        NewRow[Enum.GetName(typeof(Data_Integration.OverallIndicatorEnum),enumIndicator)] = OverallIndicators[(int)enumIndicator];                  
                    NewRow["Wind"] = Wind;
                    NewRow["WindPercent"] = 100f * Wind / TotalGen;
                    NewRow["TAGCLAST"] = RTGenTime;
                }

                RTGenDate = RTGenTime;
            }
        }



        /// <summary>
        /// Report system-level data - something so important it should be spoken
        /// </summary>
        /// <param name="Data">The data for the string</param>
        /// <param name="args">The arguments for the string</param>
        public static void ReportSystemLevelData(String Data, params string[] args)
        {
            String TextToSpeak = String.Format(Data, args);
            Program.LogError(TextToSpeak);
            if (UseSpeech)
            {
                Speech.Rate = 3;
                System.Media.SystemSounds.Exclamation.Play();
                Speech.SpeakAsync(TextToSpeak);
            }
        }

        /// <summary>
        /// Initate a new query, and return its GUID.
        /// </summary>
        /// <param name="Configuration">The configuration for the query</param>
        /// <param name="DataSource">The data source for the query</param>
        /// <returns></returns>
        public static Guid Query(MM_Query_Configuration Configuration, MM_Data_Source DataSource)
        {
            if (ActiveData.ContainsKey(Configuration.QueryName))
                ActiveData[Configuration.QueryName] = Configuration.TargetData;
            else
                ActiveData.Add(Configuration.QueryName, Configuration.TargetData);
            Application.DoEvents();
            return Configuration.Connector.Query(Configuration, DataSource);
        }


        /// <summary>
        /// Shut down all connections, threads, etc.
        /// </summary>
        public static void ShutDown()
        {
            foreach (MM_DataConnector dConn in DataConnections.Values)
                dConn.Disconnect();
            if (Data_Integration.ValueChangeLog != null)
            {
                ValueChangeLog.WriteLine("\t<Shutdown Timestamp=\"" + XmlConvert.ToString(DateTime.Now, XmlDateTimeSerializationMode.Unspecified) + "\"/>");
                ValueChangeLog.WriteLine("</MM_ValueUpdate_Log>");
                ValueChangeLog.Close();
                ValueChangeLog.Dispose();
            }

        }

        /// <summary>
        /// Add/update a note entry from XML
        /// </summary>
        /// <param name="InNote"></param>
        /// <returns></returns>
        public static MM_Note HandleNoteEntry(MM_Note InNote)
        {
            if (InNote.AssociatedElement != null)
                InNote.AssociatedElement.Notes.Add(InNote.ID, InNote);
            MM_Repository.Notes.Add(InNote.ID, InNote);
            if (NoteAdded != null)
                NoteAdded(InNote);
            return InNote;
        }

        /// <summary>
        /// Add/update a note entry
        /// </summary>
        /// <param name="ID">The unique ID of the note</param>
        /// <param name="CreatedOn">When the note was created or updated</param>
        /// <param name="Author">The author of the note</param>
        /// <param name="Note">The text of the note</param>
        /// <param name="TEID">The TEID of the note</param>
        /// <returns>The note passed to the integration layer</returns>
        public static MM_Note HandleNoteEntry(Int32 ID, DateTime CreatedOn, String Author, String Note, Int32 TEID)
        {
            MM_Note FoundNote = null;
            if (!MM_Repository.Notes.TryGetValue(ID, out FoundNote))
            {
                FoundNote = new MM_Note(ID, CreatedOn, Author, Note, TEID, true);
                MM_Repository.Notes.Add(ID, FoundNote);
                if (FoundNote.AssociatedElement != null)
                    FoundNote.AssociatedElement.Notes.Add(ID, FoundNote);
                //if (FoundNote.AssociatedElement != null && FoundNote.AssociatedElement.Substation != null)
                //    FoundNote.AssociatedElement.Substation.Notes.Add(ID, FoundNote);
                if (NoteAdded != null)
                    NoteAdded(FoundNote);
            }
            else if (FoundNote.CreatedOn != CreatedOn)
            {
                FoundNote.CreatedOn = CreatedOn;
                FoundNote.AssociatedElement = MM_Repository.TEIDs[TEID];
                FoundNote.Note = Note;
                if (NoteModified != null)
                    NoteModified(FoundNote);
            }
            return FoundNote;
        }

        /// <summary>
        /// Remove the referenced note
        /// </summary>
        /// <param name="ID">The ID of the note to be removed</param>
        public static void RemoveNoteEntry(Int32 ID)
        {
            MM_Note NoteToRemove;
            if (MM_Repository.Notes.TryGetValue(ID, out NoteToRemove))
            {
                MM_Repository.Notes.Remove(ID);
                if (NoteToRemove.AssociatedElement != null && NoteToRemove.AssociatedElement.Notes.ContainsKey(ID))
                    NoteToRemove.AssociatedElement.Notes.Remove(ID);
                if (NoteToRemove.AssociatedElement != null && NoteToRemove.AssociatedElement.Substation != null && NoteToRemove.AssociatedElement.Substation.Notes.ContainsKey(ID))
                    NoteToRemove.AssociatedElement.Substation.Notes.Remove(ID);
                if (NoteRemoved != null)
                    NoteRemoved(NoteToRemove);
            }
        }









        /// <summary>
        /// Acknowledge a violation, and let all elements know it's been done
        /// </summary>
        /// <param name="Viol"></param>
        public static void AcknowledgeViolation(MM_AlarmViolation Viol)
        {
            if (!Viol.New)
                return;
            Viol.New = false;
            //Program.LogError("Violation " + Viol.TEID + " (" + Viol.EventTime.ToString() + " " + Viol.Text + ") acknowledged: Element " + Viol.ViolatedElement.TEID + " (" + (Viol.ViolatedElement.Substation != null ? Viol.ViolatedElement.Substation + " " : "") + Viol.ViolatedElement.ElemType + " " + Viol.ViolatedElement.Name + (Viol.ViolatedElement.KVLevel != null ? " (" + Viol.ViolatedElement.KVLevel + ")" : "")); //commented out to speed up alarms - mn - 6-4-13
            if (ViolationAcknowledged != null)
                ViolationAcknowledged(Viol);
        }




        /// <summary>
        /// Clear all float data on a point
        /// </summary>
        /// <param name="Elem"></param>
        public static void ClearPoint(MM_Element Elem)
        {
            foreach (FieldInfo fI in Elem.GetType().GetFields())
                if (fI.FieldType == typeof(float))
                    fI.SetValue(Elem, float.NaN);
                else if (fI.FieldType == typeof(float[]))
                {
                    float[] inVal = (float[])fI.GetValue(Elem);
                    for (int a = 0; a < inVal.Length; a++)
                        inVal[a] = float.NaN;
                }
        }

        /// <summary>
        /// Display a form and store it accordingly
        /// </summary>
        /// <param name="FormToDisplay"></param>
        /// <param name="ThreadToHold"></param>
        public static void DisplayForm(MM_Form FormToDisplay, Thread ThreadToHold)
        {
            //Add it to the thread and forms collections in the integration layer
            Data_Integration.RunningForms.Add(FormToDisplay);
            Data_Integration.RunningThreads.Add(ThreadToHold);
            FormToDisplay.Shown += new EventHandler(FormToDisplay_Shown);
            try
            {
                Application.Run(FormToDisplay);
                if (!FormToDisplay.IsDisposed)
                    FormToDisplay.Dispose();
            }
            catch (Exception ex)
            {

                Program.MessageBox("Form closed unexpectedly: " + ex.Message + "\n" + ConvertError(ex), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (FormToDisplay is Form_Builder && (FormToDisplay as Form_Builder).viewOneLine != null)
                    (FormToDisplay as Form_Builder).viewOneLine.ShutDownQuery();
                return;
            }
            //Now that the form is done, shut it down.
            Data_Integration.RunningForms.Remove(FormToDisplay);
            Data_Integration.RunningThreads.Remove(ThreadToHold);
        }

        /// <summary>
        /// When our new form is shown, bring it to the forefront.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void FormToDisplay_Shown(object sender, EventArgs e)
        {
            (sender as Form).Activate();
        }

        #region Error reporting
        /// <summary>
        /// Build a string export of an error
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static String ConvertError(Exception ex)
        {
            StringBuilder sB = new StringBuilder();
            int Tab = 0;
            String InLine;
            while (ex != null)
            {
                sB.AppendLine();
                sB.AppendLine(new String('\t', Tab) + "---------------------------");
                sB.AppendLine(new string('\t', Tab) + ex.Message);
                if (!String.IsNullOrEmpty(ex.StackTrace))
                    using (StringReader sRd = new StringReader(ex.StackTrace))
                        while (!String.IsNullOrEmpty(InLine = sRd.ReadLine()))
                            sB.AppendLine(new string('\t', Tab) + InLine);
                sB.AppendLine();
                ex = ex.InnerException;
                Tab++;
            }
            return sB.ToString();
        }
        #endregion
    }
}