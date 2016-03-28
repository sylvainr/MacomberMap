using MacomberMapClient.Data_Elements.Blackstart;
using MacomberMapClient.Data_Elements.Display;
using MacomberMapClient.Data_Elements.Geographic;
using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Data_Elements.SystemInformation;
using MacomberMapClient.Data_Elements.Violations;
using MacomberMapClient.Data_Elements.Weather;
using MacomberMapClient.User_Interfaces.Blackstart;
using MacomberMapClient.User_Interfaces.Communications;
using MacomberMapClient.User_Interfaces.Generic;
using MacomberMapClient.User_Interfaces.Startup;
using MacomberMapClient.User_Interfaces.Summary;
using MacomberMapCommunications.Extensions;
using MacomberMapCommunications.Messages;
using MacomberMapCommunications.Messages.EMS;
using MacomberMapCommunications.WCF;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace MacomberMapClient.Integration
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class is the data integration layer, pulling information from a variety of sources and integrating them for the UI.
    /// </summary>
    public static class Data_Integration
    {
        #region Variable declarations
        /// <summary>Our list of EMS commands</summary>
        public static List<MM_EMS_Command> EMSCommands = new List<MM_EMS_Command>();


        /// <summary>Our event handler for events</summary>
        public static event EventHandler<MM_EMS_Command_Display.EMSCommandEventArgs> EMSCommandAdded;

        /// <summary>Our event handler for event clearing</summary>
        public static event EventHandler EMSCommandsCleared;

        /// <summary>The time when the model load was completed</summary>
        public static DateTime ModelLoadCompletion = DateTime.Now;

        /// <summary>Our list of user interactions</summary>
        private static SortedDictionary<String, MM_UserInteraction_Viewer> _userInteractions = new SortedDictionary<string, MM_UserInteraction_Viewer>();
        /// <summary>Our list of user interactions</summary>
        public static SortedDictionary<string, MM_UserInteraction_Viewer> UserInteractions
        {
            get { return _userInteractions; }
            set { _userInteractions = value; }
        }

        /// <summary>Our list of operatorship TEIDs, to track what we have access to</summary>
        public static int[] UserOperatorships = new int[] { 999999 };

        /// <summary>Whether to use speech for violations</summary>
        public static bool UseSpeech = false;

        /// <summary>If a savecase is being used, when it was last done</summary>
        public static DateTime SaveTime = DateTime.FromOADate(0);

        /// <summary>Our random TEIDs.</summary>
        private static Random TEIDRandomizer = new Random();

        /// <summary>The load forecast table</summary>
        public static MM_Load_Forecast_Data[] LoadForecastData = new MM_Load_Forecast_Data[0];

        /// <summary>The date when the RTGen data was last refreshed</summary>
        public static DateTime RTGenDate;

        /// <summary>The collection of currently-active data sets, for serialization purposes</summary>
        public static Dictionary<String, DataSet> ActiveData = new Dictionary<string, DataSet>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>The collection of running threads</summary>
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

        /// <summary>The network source</summary>
        public static MM_Data_Source NetworkSource
        {
            get { return _NetworkSource; }
            set
            {
                _NetworkSource = value;
                if (value != null)
                    _UseEstimates = value.Estimates;
                   
                //Reset all points
                lock (MM_Repository.TEIDs)
                    foreach (MM_Element Elem in MM_Repository.TEIDs.Values)
                        ClearPoint(Elem);

                List<MM_AlarmViolation> ToRemove = new List<MM_AlarmViolation>();
                foreach (MM_AlarmViolation Viol in MM_Repository.Violations.Keys)
                    ToRemove.Add(Viol);

                foreach (MM_AlarmViolation RemoveMe in ToRemove)
                    Data_Integration.RemoveViolation(RemoveMe);
                MM_Repository.ArchivedViolations.Clear();


                if (NetworkSourceChanged != null)
                    NetworkSourceChanged(typeof(Data_Integration), EventArgs.Empty);
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

        /// <summary>Whether the system is in test mode (no data connectors)</summary>
        public static bool TestMode = false;

        /// <summary>Whether all main components and UI loading are complete.</summary>
        public static bool InitializationComplete = false;

        /// <summary>The access-level permissions for the Macomber Map</summary>
        public static MM_Permissions Permissions = new MM_Permissions(null);

        /// <summary>Our reserve status</summary>
        public static Dictionary<String, MM_Reserve> Reserves
        { get { return _Reserves; } }
        private static Dictionary<String, MM_Reserve> _Reserves = new Dictionary<string, MM_Reserve>(20, StringComparer.CurrentCultureIgnoreCase);

        /// <summary>Our collection of CSC/IROL limits</summary>
        public static Dictionary<string, MM_Limit> CSCIROLLimits
        { get { return _CSCIROLLimits; } }
        private static Dictionary<string, MM_Limit> _CSCIROLLimits = new Dictionary<string, MM_Limit>(10, StringComparer.CurrentCultureIgnoreCase);

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

        /// <summary>
        /// Our delegate for island changing
        /// </summary>
        /// <param name="Island"></param>
        public delegate void IslandChangeDelegate(MM_Island Island);

        /// <summary>Our event for an island addition</summary>
        public static event IslandChangeDelegate IslandAdded;

        /// <summary>Our event for an island removal</summary>
        public static event IslandChangeDelegate IslandRemoved;
        #endregion


        //TODO: Restore user interactions
        /*
        /// <summary>
        /// Process a new user interaction
        /// </summary>
        /// <param name="IncomingMessage"></param>
        public static void ProcessUserInteraction(TCP_Message IncomingMessage)
        {
            
            if (IncomingMessage.IncomingData.Length < 8)
                return;
            MM_UserInteraction_Viewer UserInteraction;
            if (!UserInteractions.TryGetValue(IncomingMessage.GetString(1), out UserInteraction))
                UserInteractions.Add(IncomingMessage.GetString(1), UserInteraction = new MM_UserInteraction_Viewer(IncomingMessage.GetString(1), IncomingMessage.GetDateTime(0)));
            UserInteraction.AddEvent(IncomingMessage.GetDateTime(0), IncomingMessage.GetString(3), IncomingMessage.GetString(4), IncomingMessage.GetString(5), IncomingMessage.GetString(2), IncomingMessage.GetString(6), IncomingMessage.GetString(7));
        }

        /// <summary>
        /// Process new user interactions
        /// </summary>
        /// <param name="MPName"></param>
        /// <param name="IncomingMessages"></param>
        public static void ProcessUserInteractions(String MPName, List<TCP_Message> IncomingMessages)
        {
            MM_UserInteraction_Viewer UserInteraction;
            TCP_Message LastMessage = IncomingMessages[IncomingMessages.Count - 1];
            if (!UserInteractions.TryGetValue(MPName, out UserInteraction))
                UserInteractions.Add(MPName, UserInteraction = new MM_UserInteraction_Viewer(MPName, LastMessage.GetDateTime(0)));

            //Add all items after the last one in.
            for (int a = 0; a < IncomingMessages.Count - 1; a++)
                UserInteraction.AddEventWithoutNotification(IncomingMessages[a].GetDateTime(0), IncomingMessages[a].GetString(3), IncomingMessages[a].GetString(4), IncomingMessages[a].GetString(5), IncomingMessages[a].GetString(2), IncomingMessages[a].GetString(6), IncomingMessages[a].GetString(7));

            //Add our last event, which will update the map
            UserInteraction.AddEvent(LastMessage.GetDateTime(0), LastMessage.GetString(3), LastMessage.GetString(4), LastMessage.GetString(5), LastMessage.GetString(2), LastMessage.GetString(6), LastMessage.GetString(7));
        }
        */

        /// <summary>
        /// Our delegate for starting up the model
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="Startup"></param>
        /// <param name="WeatherData"></param>
        /// <returns></returns>
        public delegate bool StartupModelDelegate(IMM_WCF_Interface Client, MM_Startup_Form Startup, bool WeatherData);

        /// <summary>
        /// Start up our network model
        /// </summary>
        /// <param name="Client">Our target MM client</param>
        /// <param name="Startup">Our startup form</param>
        /// <param name="WeatherData">Whether to retrieve weather data</param>
        /// <returns></returns>        
        public static bool StartupModel(IMM_WCF_Interface Client, MM_Startup_Form Startup, bool WeatherData)
        {

            NetworkSource = new MM_Data_Source() { Estimates = true, Telemetry = true};
            Dictionary<String, LoadEventDelegate> StartupSteps = new Dictionary<string, LoadEventDelegate>();
            StartupSteps.Add("Loading Macomber Map configuration", MM_Server_Interface.LoadMacomberMapConfiguration);
            StartupSteps.Add("Fix Coordinates if Null", Startup.FixCoordinatesNull);
            StartupSteps.Add("Loading network model data", MM_Server_Interface.LoadNetworkModel);
            StartupSteps.Add("Loading operatorship updates", MM_Server_Interface.LoadOperatorshipUpdateData);
            StartupSteps.Add("Registering for value updates", MM_Server_Interface.RegisterForUpdates);
            StartupSteps.Add("Loading line, ZBR and Tie data", MM_Server_Interface.LoadLineData);
            StartupSteps.Add("Loading breaker and switch data", MM_Server_Interface.LoadBreakerSwitchData);
            StartupSteps.Add("Loading load data", MM_Server_Interface.LoadLoadData);
            StartupSteps.Add("Loading load forecast data", MM_Server_Interface.LoadLoadForecastData);
            StartupSteps.Add("Loading generation chart data", MM_Server_Interface.LoadChartingData);
            StartupSteps.Add("Loading island data", MM_Server_Interface.LoadIslandData);
            StartupSteps.Add("Loading transformer data", MM_Server_Interface.LoadTransformerData);
            StartupSteps.Add("Loading unit data", MM_Server_Interface.LoadUnitData);
            StartupSteps.Add("Loading capacitor and reactor data", MM_Server_Interface.LoadShuntCompensatorData);
            StartupSteps.Add("Loading system-wide data", MM_Server_Interface.LoadSystemWideData);
            StartupSteps.Add("Loading SPS and ICCP data", MM_Server_Interface.LoadSCADAData);
            StartupSteps.Add("Loading violation data", MM_Server_Interface.LoadViolationData);
            StartupSteps.Add("Loading training data", MM_Server_Interface.LoadTrainingData);
            StartupSteps.Add("Loading notes", MM_Server_Interface.LoadNotes);
            StartupSteps.Add("Loading bus data", MM_Server_Interface.LoadBusData);
            StartupSteps.Add("Loading flowgate data", MM_Server_Interface.LoadFlowgateData);
            if (MM_Server_Interface.ClientAreas.ContainsKey("ERCOT"))
                StartupSteps.Add("Loading command history", MM_Server_Interface.LoadOperatorCommands);

            //StartupSteps.Add("Loading equipment analog data", MM_Server_Interface.LoadAnalogData);

            //If requested, load our weather data
            if (WeatherData)
                StartupSteps.Add("WX: Starting weather updates", LoadWeatherStations);

            StartupSteps.Add("MM: Starting up Map", Startup.CreateMap);
            StartupSteps.Add("MM: Waiting for Map to complete loading", Startup.WaitForMap);

            //Now, go through all steps             
            foreach (KeyValuePair<String, LoadEventDelegate> Step in StartupSteps)
            {
                if (!TryLoadStep(Step.Key, Step.Value, Startup))
                    return false;
            }
            //Run our garbage collection
           // GC.Collect();
            //Thread.Sleep(7000);
           // MM_Server_Interface.LoadLineData();
            Program.MM.ResetNetworkMap();

            //Return successful completion
            return true;
        }

        /// <summary>
        /// Restart our network model, pulling in all of our data
        /// </summary>
        /// <param name="Client"></param>
        /// <returns></returns>
        public static bool RestartModel(IMM_WCF_Interface Client)
        {
            Dictionary<String, LoadEventDelegate> StartupSteps = new Dictionary<string, LoadEventDelegate>();
            StartupSteps.Add("Loading operatorship updates", MM_Server_Interface.LoadOperatorshipUpdateData);
            StartupSteps.Add("Registering for value updates", MM_Server_Interface.RegisterForUpdates);
            StartupSteps.Add("Loading line, ZBR and Tie data", MM_Server_Interface.LoadLineData);
            StartupSteps.Add("Loading bus data", MM_Server_Interface.LoadBusData);
            StartupSteps.Add("Loading load data", MM_Server_Interface.LoadLoadData);
            StartupSteps.Add("Loading load forecast data", MM_Server_Interface.LoadLoadForecastData);
            StartupSteps.Add("Loading generation chart data", MM_Server_Interface.LoadChartingData);
            StartupSteps.Add("Loading island data", MM_Server_Interface.LoadIslandData);
            StartupSteps.Add("Loading transformer data", MM_Server_Interface.LoadTransformerData);
            StartupSteps.Add("Loading unit data", MM_Server_Interface.LoadUnitData);
            StartupSteps.Add("Loading capacitor and reactor data", MM_Server_Interface.LoadShuntCompensatorData);
            StartupSteps.Add("Loading system-wide data", MM_Server_Interface.LoadSystemWideData);
            StartupSteps.Add("Loading SPS and ICCP data", MM_Server_Interface.LoadSCADAData);
            StartupSteps.Add("Loading breaker and switch data", MM_Server_Interface.LoadBreakerSwitchData);
            StartupSteps.Add("Loading violation data", MM_Server_Interface.LoadViolationData);
            StartupSteps.Add("Loading training data", MM_Server_Interface.LoadTrainingData);
            StartupSteps.Add("Loading notes", MM_Server_Interface.LoadNotes);
            StartupSteps.Add("Loading flowgate data", MM_Server_Interface.LoadFlowgateData);
            if (MM_Server_Interface.ClientAreas.ContainsKey("ERCOT"))
                StartupSteps.Add("Loading command history", MM_Server_Interface.LoadOperatorCommands);

            //StartupSteps.Add("Loading equipment analog data", MM_Server_Interface.LoadAnalogData);


            //Now, go through all steps             
            foreach (KeyValuePair<String, LoadEventDelegate> Step in StartupSteps)
                if (!TryLoadStep(Step.Key, Step.Value, null))
                    return false;
            //Run our garbage collection
           // GC.Collect();

            MM_Server_Interface.LoadLineData();

            //Return successful completion
            return true;
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
                                AggPt.X += Line.Substation1.LngLat.X;
                                AggPt.Y += Line.Substation1.LngLat.Y;
                                TallyPt++;
                            }
                            else if (Line.Substation2 != Sub && !float.IsNaN(Line.Substation2.Latitude))
                            {
                                AggPt.X += Line.Substation2.LngLat.X;
                                AggPt.Y += Line.Substation2.LngLat.Y;
                                TallyPt++;
                            }
                        //Now, add in a little bit of randomness
                        if (TallyPt > 0)
                            Sub.LngLat = new PointF(((AggPt.X / TallyPt) + (((float)rnd.NextDouble() - 0.5f) / 4f)), ((AggPt.Y / TallyPt) + (((float)rnd.NextDouble() - 0.5f) / 4f)));
                        else if (a == 1)
                        {
                            Sub.LngLat = TopLeft;
                            TopLeft.X += .05f;
                            if (TopLeft.X > MM_Repository.Counties["STATE"].Max_X)
                                TopLeft = new PointF(StateBound.X, TopLeft.Y + .05f);
                        }
                        if (!float.IsNaN(Sub.LngLat.X))
                            MM_System_Interfaces.LogError("CIM: Estimated substation {0} positioning at {1},{2}", Sub.Name, Sub.Latitude, Sub.Longitude);
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
            //TODO: Create interfaces to historic database
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
        private static bool TryLoadStep(String EventText, LoadEventDelegate EventToRun, MM_Startup_Form Startup)
        {
            bool TryAgain = true;
            while (TryAgain)
                try
                {
                    MM_System_Interfaces.LogError(EventText);
                    EventToRun(); //MN//This can crash??? Why are we generating pop up below. error also describes this document line number 1261... however this is the catch it is finding - dest array not large enough check array size???
                    Application.DoEvents();
                    TryAgain = false;
                    return true;
                }
                catch (Exception ex)
                {
                    DialogResult Resp = MM_System_Interfaces.MessageBox("Error during step \"" + EventText + "\":\n\n " + ex.ToString(), Application.ProductName, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
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
        public static void StartupSavecase(String NetworkModelPath, MM_Startup_Form Startup, String OneLineDirectory)
        {
            //Load our network model
            MM_System_Interfaces.LogError("MM: Loading MM Savecase");
            XmlDocument xNetworkModel = new XmlDocument();
            xNetworkModel.Load(NetworkModelPath);

            //Loading map data
            MM_System_Interfaces.LogError("MM: Loading XML Map data");
            var weakRefXDoc = new WeakReference<XmlDocument>(xNetworkModel, true);
            LoadXMLData(weakRefXDoc, MM_Startup_Form.Coordinates, OneLineDirectory);
            TestMode = false;
            xNetworkModel.LoadXml("<root></root>");

            //Assign our pseudo-data source


            //Create the map
            MM_System_Interfaces.LogError("MM: Starting up Map");
            Startup.CreateMap();

            MM_System_Interfaces.LogError("MM: Waiting for Map to complete loading");
            Startup.WaitForMap();
        }


        /// <summary>
        /// Initialize the data integration model
        /// </summary>
        /// <param name="xNetworkModel"></param>
        /// <param name="Startup"></param>
        /// <param name="RunTraining"></param>
        public static void StartupSavecase(XmlDocument xNetworkModel, MM_Startup_Form Startup, bool RunTraining)
        {
            //Load our network model
            MM_System_Interfaces.LogError("MM: Loading MM Savecase");

            //Loading map data
            MM_System_Interfaces.LogError("MM: Loading XML Map data");
            var weakRefXDoc = new WeakReference<XmlDocument>(xNetworkModel, true);
            LoadXMLData(weakRefXDoc, MM_Startup_Form.Coordinates, null);
            TestMode = false;
            xNetworkModel.LoadXml("<root></root>");
            //Assign our pseudo-data source


            //Create the map
            MM_System_Interfaces.LogError("MM: Starting up Map");
            Startup.CreateMap();

            MM_System_Interfaces.LogError("MM: Waiting for Map to complete loading");
            Startup.WaitForMap();
        }


        /// <summary>
        /// Save the full network model, as requested by the user.
        /// </summary>
        /// <param name="FileName">The file to output</param>
        /// <param name="Coordinates">The network map coordinates</param>
        public static void SaveXMLData(String FileName, MM_Coordinates Coordinates, bool saveViolations = true)
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
            if (NetworkSource != null)
                NetworkSource.WriteXML(xW);


            //Write out our configuration XML            
            XmlElement DisplayParams = MM_Repository.xConfiguration["Configuration"];
            xW.WriteStartElement("Configuration");
            DisplayParams.WriteContentTo(xW);
            xW.WriteEndElement();

            //Write our network map coordinates
            if (Coordinates != null)
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
            // contingencies have to be before lines and transformers
            foreach (MM_Contingency contingency in MM_Repository.Contingencies.Values)
            {
                contingency.WriteXML(xW);
            }
            //Now, write out substations
            foreach (MM_Substation Substation in MM_Repository.Substations.Values)
                Substation.WriteXML(xW);

            foreach (MM_Bus bus in MM_Repository.Busbars.Values)
                bus.WriteXML(xW);

            // we need to make sure transformers before windings.
            foreach (MM_Element Elem in MM_Repository.TEIDs.Values)
                if (Elem is MM_Transformer)
                {
                    Elem.WriteXML(xW);
                }

            //Now, go through the rest of our TEIDs, and add them in.
            foreach (MM_Element Elem in MM_Repository.TEIDs.Values)
                if (Elem is MM_Company == false && !(Elem is MM_Transformer) && !(Elem is MM_Substation) && !(Elem is MM_Bus))
                {
                    if (Elem is MM_Tie)
                        (Elem as MM_Tie).AssociatedLine.WriteXML(xW);
                    Elem.WriteXML(xW);
                }


            //foreach (MM_Substation Substation in MM_Repository.Substations.Values)
            //Substation.WriteXML(xW);

            /* foreach (MM_Contingency contingency in MM_Repository.Contingencies.Values)
             {
                 contingency.WriteXML(xW);
             } */
            //Write out our generation types
            foreach (MM_Unit_Type GenType in MM_Repository.GenerationTypes.Values)
                GenType.WriteXML(xW);
            if (saveViolations)
            {
            //Now, write out all of our violations
            foreach (MM_AlarmViolation Viol in MM_Repository.Violations.Keys)
                Viol.WriteXML(xW);
            }
            else
            {
                foreach (MM_AlarmViolation Viol in MM_Repository.Violations.Keys)
                    if (Viol.Type.Name == "PlannedOutage")
                        Viol.WriteXML(xW);
            }
            //Write out our notes
            foreach (MM_Note Note in MM_Repository.Notes.Values)
                Note.WriteXML(xW);

            //Now, write out our data sets
            DataSet OverallData = new DataSet();
            OverallData.Tables.Add(GenerateTable<MM_Chart_Data>(ChartData));
            OverallData.Tables.Add(GenerateTable<MM_Load_Forecast_Data>(LoadForecastData));

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
                    MM_System_Interfaces.WriteConsoleLine("Unable to save thread: " + ex.Message);
                }

            //Now close our document
            xW.WriteEndDocument();
            xW.Close();


            MM_System_Interfaces.MessageBox("Model save complete!", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        /// <summary>
        /// Generate a data table based on an array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="InValues"></param>
        /// <returns></returns>
        public static DataTable GenerateTable<T>(T[] InValues)
        {
            DataTable OutTable = new DataTable();
            PropertyInfo[] Properties = typeof(T).GetProperties();
            foreach (PropertyInfo pI in Properties)
                OutTable.Columns.Add(pI.Name, pI.PropertyType);
            foreach (T ThisValue in InValues)
            {
                DataRow NewRow = OutTable.NewRow();
                foreach (PropertyInfo pI in Properties)
                    NewRow[pI.Name] = pI.GetValue(ThisValue);
                OutTable.Rows.Add(NewRow);
            }
            return OutTable;
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
                if (FoundElement.GetType().Equals(DestinationType) || DestinationType == typeof(MM_Element))
                    return FoundElement;
                else if (FoundElement.GetType() == typeof(MM_Unit) && (DestinationType == typeof(MM_Unit_Logical) || DestinationType == typeof(MM_Unit_CombinedCycle)))
                    return FoundElement;
                else if (DestinationType == typeof(MM_Electrical_Bus) && FoundElement.GetType() == typeof(MM_Node))
                    return FoundElement;
                else if (FoundElement.GetType() == typeof(MM_Element) && FoundElement.Name == null)
                {
                    MM_Element OutElement = System.Activator.CreateInstance(DestinationType) as MM_Element;
                    OutElement.TEID = TEID;
                    return OutElement;
                }
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
                else if (FoundElem.GetType() == typeof(MM_Unit) && DestinationType == typeof(MM_Unit_Logical))
                {
                    MM_Unit_Logical OutElement = System.Activator.CreateInstance(DestinationType) as MM_Unit_Logical;
                    OutElement.ReadXML(xElem, AddIfNew);

                    //Update references to this element
                    if (OutElement.PhysicalUnit != null)
                        OutElement.PhysicalUnit.LogicalUnits[Array.IndexOf(OutElement.PhysicalUnit.LogicalUnits, FoundElem)] = OutElement;
                    if (OutElement.Substation != null)
                        OutElement.Substation.Units[OutElement.Substation.Units.IndexOf(FoundElem as MM_Unit)] = OutElement;
                    MM_Repository.TEIDs[OutElement.TEID] = OutElement;
                    return OutElement;
                }

                else if (FoundElem.GetType() == typeof(MM_Unit) && DestinationType == typeof(MM_Unit_CombinedCycle))
                {
                    MM_Unit_CombinedCycle OutElement = System.Activator.CreateInstance(DestinationType) as MM_Unit_CombinedCycle;
                    OutElement.ReadXML(xElem, AddIfNew);

                    //Update references to this element                    
                    if (OutElement.Substation != null)
                        OutElement.Substation.Units[OutElement.Substation.Units.IndexOf(FoundElem as MM_Unit)] = OutElement;
                    MM_Repository.TEIDs[OutElement.TEID] = OutElement;
                    return OutElement;
                }
                else if (FoundElem.Name == null)
                {
                    MM_Repository.TEIDs.Remove(TEID); // didn't get the full object last time, so lets try again.
                    MM_Element OutElement = System.Activator.CreateInstance(DestinationType) as MM_Element;
                    OutElement.TEID = TEID;
                    OutElement.ReadXML(xElem, AddIfNew);
                    if (AddIfNew)
                        MM_Repository.TEIDs.Add(TEID, OutElement);
                    return OutElement;

                }
                else
                {
                    MM_System_Interfaces.LogError("Couldn't find the right type for TEID ID: " + TEID.ToString() + " looking for type: " + DestinationType.Name);
                    //string error = "Searching for TEID " + TEID.ToString() + ", found type " + MM_Repository.TEIDs[TEID].GetType().Name + ", while looking for type " + DestinationType.Name;
                    //throw new InvalidDataException(error);
                    return null;
                }
            else
            {
                MM_Element OutElement = System.Activator.CreateInstance(DestinationType) as MM_Element;
                OutElement.TEID = TEID;
                OutElement.ReadXML(xElem, AddIfNew);
                if (AddIfNew && TEID > 0)
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
        public static void LoadXMLData(WeakReference<XmlDocument> xDocRef, MM_Coordinates Coordinates, String OneLineDirectory)
        {
            Data_Integration.UserName = Environment.UserName;
            Data_Integration.OneLineDirectory = OneLineDirectory;
            if (!MM_Repository.ViolationTypes.ContainsKey("ThermalWarning"))
            {
                MM_Repository.ViolationTypes.Add("ThermalWarning", new MM_AlarmViolation_Type(MM_Repository.ViolationTypes.Count, "ThermalWarning", "TW", true, MM_Repository.OverallDisplay.WarningColor, 1.5f, true));
                MM_Repository.ViolationTypes.Add("ThermalAlert", new MM_AlarmViolation_Type(MM_Repository.ViolationTypes.Count, "ThermalAlert", "TA", true, MM_Repository.OverallDisplay.ErrorColor, 1.5f, true));
                MM_Repository.ViolationTypes.Add("VoltageWarning", new MM_AlarmViolation_Type(MM_Repository.ViolationTypes.Count, "VoltageWarning", "VW", true, MM_Repository.OverallDisplay.WarningColor, 1.5f, true));
                MM_Repository.ViolationTypes.Add("VoltageAlert", new MM_AlarmViolation_Type(MM_Repository.ViolationTypes.Count, "VoltageAlert", "VA", true, MM_Repository.OverallDisplay.ErrorColor, 1.5f, true));
            }
            XmlDocument xDoc = null;
            //Because we may not have the IDs for violations, make some up
            xDocRef.TryGetTarget(out xDoc);
            //Assign our data integration layer characteristics            
            foreach (XmlAttribute attrib in xDoc.DocumentElement.Attributes)
            {
                XmlAttribute xAttr = (XmlAttribute)attrib.Clone();
                if (typeof(Data_Integration).GetField(xAttr.Name) != null && xAttr.Name != "CIMServer")
                    MM_Serializable.AssignValue(xAttr.Name, xAttr.Value, typeof(Data_Integration), true);
                else if (xAttr.Name == "View")
                    foreach (MM_Display_View View in MM_Repository.Views.Values)
                        if (View.FullName == xAttr.Value)
                            MM_Repository.ActiveView = View;
            }
            Data_Integration.InitializationComplete = false;

            //Now go node by node
            foreach (XmlNode xElemNode in xDoc.DocumentElement.ChildNodes)
            {
                if (xElemNode is XmlElement)
                {
                    XmlElement xElem = (XmlElement)xElemNode.Clone(); // we need this clone to allow the XMLDocument to always get garbage collected.
                    switch (xElem.Name)
                    {
                        case "MM_Data_Source":
                            Data_Integration.NetworkSource = new MM_Data_Source(xElem, true);
                            break;
                        case "State":
                        case "MM_Boundary":
                        case "County":
                            lock (MM_Repository.Counties)
                            {
                                try
                                {
                                    MM_Boundary Boundary = new MM_Boundary(xElem, true);
                                    if (Boundary.TEID == 0)
                                        MM_Repository.TEIDs.Add(Boundary.TEID = Data_Integration.GetTEID(), Boundary);
                                    if (!MM_Repository.Counties.ContainsKey(xElem.Attributes["Name"].Value))
                                        MM_Repository.Counties.Add(xElem.Attributes["Name"].Value, Boundary);
                                }
                                catch (Exception ex)
                                {
                                    MM_System_Interfaces.LogError(ex);
                                }
                            }
                            break;
                        case "District":
                            MM_Boundary District = new MM_Boundary(xElem, true);
                            if (District.TEID == 0)
                                MM_Repository.TEIDs.Add(District.TEID = Data_Integration.GetTEID(), District);
                            MM_Repository.Districts.Add(xElem.Attributes["Name"].Value, District);
                            break;
                        case "Company":
                            MM_Company NewCompany = LocateElement(xElem, typeof(MM_Company), true) as MM_Company;
                            if (!MM_Repository.Companies.ContainsKey(NewCompany.Alias))
                                MM_Repository.Companies.Add(NewCompany.Alias, NewCompany);
                            break;
                        case "MM_Substation":
                        case "Substation":
                            lock (MM_Repository.Substations)
                            {
                                MM_Substation NewSub = LocateElement(xElem, typeof(MM_Substation), true) as MM_Substation;
                                if (NewSub != null && !MM_Repository.Substations.ContainsKey(NewSub.Name))
                                {
                                    MM_Repository.Substations.Add(NewSub.Name, NewSub);
                                }
                            }
                            break;
                        case "Line":
                            lock (MM_Repository.Lines)
                            {
                                MM_Line NewLine = LocateElement(xElem, typeof(MM_Line), true) as MM_Line;
                                if (NewLine == null)
                                    break;
                                if (MM_Repository.Lines.ContainsKey(NewLine.Name))
                                {
                                    if (NewLine is MM_Tie)
                                    {
                                        MM_Repository.Lines.Remove(NewLine.Name);
                                        MM_Repository.Lines.Add(NewLine.Name, NewLine);
                                    }
                                }
                                else
                                    MM_Repository.Lines.Add(NewLine.Name, NewLine);

                                if (xElem.HasAttribute("Contingencies"))
                                {
                                    String[] splStr = xElem.Attributes["Contingencies"].Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                    foreach (string str in splStr)
                                    {
                                        MM_Contingency con = null;
                                        MM_Repository.Contingencies.TryGetValue(str, out con);
                                        if (NewLine.Contingencies == null || NewLine.Contingencies.Count == 0)
                                            NewLine.Contingencies = new List<MM_Contingency>();
                                        else
                                            NewLine.Contingencies = NewLine.Contingencies.ToList();
                                        if (con != null && !NewLine.Contingencies.Any(c => c.Name == con.Name))
                                            NewLine.Contingencies.Add(con);
                                    }
                                }

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
                            }
                            break;
                        case "CombinedCycleConfiguration":
                        case "MM_Unit_CombinedCycle":
                            LocateElement(xElem, typeof(MM_Unit_CombinedCycle), true);
                            break;
                        case "MM_Unit_Logical":
                        case "LogicalUnit":
                            LocateElement(xElem, typeof(MM_Unit_Logical), true);
                            break;
                        case "Unit":
                            MM_Unit Unit = (MM_Unit)LocateElement(xElem, typeof(MM_Unit), true);
                            if (Unit.UnitTEID != 0)     
                                    MM_Repository.TEIDs.Add(Unit.UnitTEID, Unit);
                            if (!MM_Repository.Units.ContainsKey(Unit.Substation.Name + "." + Unit.Name))
                                MM_Repository.Units.Add(Unit.Substation.Name + "." + Unit.Name, Unit);
                            Unit.LogicalUnits = new MM_Unit_Logical[xElem.ChildNodes.Count];
                            for (int a = 0; a < xElem.ChildNodes.Count; a++)
                                Unit.LogicalUnits[a] = (MM_Unit_Logical)LocateElement((XmlElement)xElem.ChildNodes[a], typeof(MM_Unit_Logical), true);
                            break;
                        case "SPS":
                        case "RAP":
                            break;
                        case "Load":
                        case "LAAR":
                        case "LaaR":
                            MM_Load NewLoad = LocateElement(xElem, typeof(MM_Load), true) as MM_Load;
                            if (!MM_Repository.Loads.ContainsKey(NewLoad.Substation.Name + "." + NewLoad.Name))
                                MM_Repository.Loads.Add(NewLoad.Substation.Name + "." + NewLoad.Name, NewLoad);
                            break;
                        case "MM_Generation_Type":
                        case "MM_Unit_Type":

                            if (MM_Repository.GenerationTypes.ContainsKey(xElem.Attributes["Name"].Value))
                                MM_Repository.GenerationTypes[xElem.Attributes["Name"].Value].ReadXML(xElem, true);
                            else
                            {
                                MM_Unit_Type NewGen = new MM_Unit_Type(xElem, true);
                                if (!MM_Repository.GenerationTypes.ContainsKey(NewGen.Name))
                                    MM_Repository.GenerationTypes.Add(NewGen.Name, NewGen);
                                if (!String.IsNullOrEmpty(NewGen.EMSName) && !MM_Repository.GenerationTypes.ContainsKey(NewGen.EMSName))
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
                            try
                            {
                                MM_Bus Bus = LocateElement(xElem, typeof (MM_Bus), true) as MM_Bus;
                                string name = "Bus";
                                if (Bus == null || Bus.Substation == null)
                                    continue;
                                if (!name.Contains(Bus.Substation.Name))
                                    name = Bus.Substation.Name + "." + Bus.Name;
                                if (!MM_Repository.Busbars.ContainsKey(name))
                                    MM_Repository.Busbars.Add(name, Bus);
                                
                            }
                            catch (Exception)
                            {

                            }
                            break;
                        case "Transformer":
                            MM_Transformer NewTransformer = LocateElement(xElem, typeof(MM_Transformer), true) as MM_Transformer;
                            NewTransformer.Winding1.TEID = Convert.ToInt32(xElem.Attributes["Windings"].Value.Split(',')[0]);
                            NewTransformer.Winding2.TEID = Convert.ToInt32(xElem.Attributes["Windings"].Value.Split(',')[1]);

                            if (xElem.HasAttribute("Contingencies"))
                            {
                                String[] splStr = xElem.Attributes["Contingencies"].Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (string str in splStr)
                                {
                                    MM_Contingency con = null;
                                    MM_Repository.Contingencies.TryGetValue(str, out con);
                                    if (NewTransformer.Contingencies == null || NewTransformer.Contingencies.Count == 0)
                                        NewTransformer.Contingencies = new List<MM_Contingency>();
                                    else
                                        NewTransformer.Contingencies = NewTransformer.Contingencies.ToList();
                                    if (con != null && !NewTransformer.Contingencies.Any(c => c.Name == con.Name))
                                        NewTransformer.Contingencies.Add(con);
                                }
                            }


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
                            if (!MM_Repository.ShuntCompensators.ContainsKey(NewSc.Substation.Name + "." + NewSc.Name))
                                MM_Repository.ShuntCompensators.Add(NewSc.Substation.Name + "." + NewSc.Name, NewSc);
                            break;
                        case "StaticVarCompensator":
                            MM_StaticVarCompensator NewSVC = LocateElement(xElem, typeof(MM_StaticVarCompensator), true) as MM_StaticVarCompensator;
                            NewSVC.ElemType = MM_Repository.FindElementType(xElem.Name);
                            break;
                        case "Contingency":
                            MM_Contingency NewContingency = new MM_Contingency(xElem, true);
                            if (!MM_Repository.Contingencies.ContainsKey(NewContingency.Name))
                                MM_Repository.Contingencies.Add(NewContingency.Name, NewContingency);
                            if (!MM_Repository.TEIDs.ContainsKey(NewContingency.TEID))
                                MM_Repository.TEIDs.Add(NewContingency.TEID, NewContingency);
                            break;
                        case "Flowgate":
                            MM_Flowgate fg = new MM_Flowgate(xElem, true);
                            if (!MM_Repository.Contingencies.ContainsKey(fg.Name))
                                MM_Repository.Contingencies.Add(fg.Name, fg);
                            if (!MM_Repository.TEIDs.ContainsKey(fg.TEID))
                                MM_Repository.TEIDs.Add(fg.TEID, fg);
                            break;
                        case "Violation":
                            MM_AlarmViolation NewViol = new MM_AlarmViolation(xElem, true);
                            NewViol.New = false; // if we are loading it from XML, it's not new anymore.
                            if (NewViol.ViolatedElement.TEID != 0)
                                CheckAddViolation(NewViol);

                            break;
                        case "Breaker":
                        case "Switch":
                            LocateElement(xElem, typeof(MM_Breaker_Switch), true);
                            break;
                        case "Element":
                            MM_Element NewElement = new MM_Element(xElem, true);
                            if (!MM_Repository.TEIDs.ContainsKey(NewElement.TEID))
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
                        case "Tie":
                        case "DCTie":
                            MM_Tie tie = LocateElement(xElem, typeof(MM_Tie), true) as MM_Tie;
                            if (tie.AssociatedLine != null)
                                tie.Name = tie.AssociatedLine.Name;
                            if (String.IsNullOrEmpty(tie.TieDescriptor))
                                tie.TieDescriptor = tie.Name;
                            if (!MM_Repository.Ties.ContainsKey(tie.TieDescriptor))
                                MM_Repository.Ties.Add(tie.TieDescriptor, tie);
                            if (!MM_Repository.Lines.ContainsKey(tie.Name))
                                MM_Repository.Lines.Add(tie.Name, tie);
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
                            CSCIROLLimits.Add(NewLimit.Name, NewLimit);
                            break;
                        case "MM_Reserve":
                        case "Reserve":
                            Reserves.Add(xElem.Attributes["Category"].Value + "-" + xElem.Attributes["Name"].Value, new MM_Reserve(xElem, true));
                            break;
                        case "Island":
                            MM_Island NewIsland = new MM_Island(xElem, true);
                            if (!MM_Repository.Islands.ContainsKey(NewIsland.ID))
                                MM_Repository.Islands.Add(NewIsland.ID, NewIsland);
                            break;
                        case "RemedialActionScheme":
                            MM_RemedialActionScheme NewRas = new MM_RemedialActionScheme(xElem, true);
                            MM_Repository.RASs.Add(NewRas.Name, NewRas);

                            break;
                        case "Blackstart_Corridor":
                            MM_Blackstart_Corridor NewCorridor = new MM_Blackstart_Corridor(xElem as XmlElement);
                            NewCorridor.TEID = Data_Integration.GetTEID();
                            MM_Repository.TEIDs.Add(NewCorridor.TEID, NewCorridor);
                            MM_Repository.BlackstartCorridors.Add(NewCorridor.Name, NewCorridor);
                            break;
                        default:
                            MM_System_Interfaces.LogError("Unknown type handled: " + xElem.Name);
                            break;
                    }
                }

            }
            //Run through our elements a second time, picking up downstream-linked elements
            foreach (XmlElement xElemfe in xDoc.DocumentElement.ChildNodes)
            {
                XmlElement xElem = (XmlElement)xElemfe;
                switch (xElem.Name)
                {
                    case "SPS":
                    case "RAP":
                        MM_RemedialActionScheme RAS = new MM_RemedialActionScheme(xElem, true); // LocateElement(xElem, typeof(MM_RemedialActionScheme)) as MM_RemedialActionScheme;
                        break;
                    case "Flowgate":
                        MM_Flowgate fg = new MM_Flowgate(xElem, true);
                        if (!MM_Repository.Contingencies.ContainsKey(fg.Name))
                            MM_Repository.Contingencies.Add(fg.Name, fg);
                        if (!MM_Repository.TEIDs.ContainsKey(fg.TEID))
                            MM_Repository.TEIDs.Add(fg.TEID, fg);
                        break;
                }
                xElem.RemoveAll();
                xElemfe.RemoveAll();
            }
            RollUpElementsToSubstation();
            CommLoaded = true;

            //Add our violations
             MM_KVLevel.MonitoringChanged += MM_KVLevel_MonitoringChanged;

            ModelLoadCompletion = DateTime.Now;
            //MM_Repository.Counties["STATE"].Coordinates = BetterStateBoundary();
        }


        private static Dictionary<MM_Element, MM_AlarmViolation> ElementAlerts = new Dictionary<MM_Element, MM_AlarmViolation>();

        /// <summary>
        /// When our voltage monitoring flags change, recompute everything
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void MM_KVLevel_MonitoringChanged(object sender, EventArgs e)
        {
            foreach (MM_Line Line in MM_Repository.Lines.Values.ToArray())
                if (Line.KVLevel != null && Line.KVLevel.MonitorEquipment && Line.LinePercentage >= Line.KVLevel.ThermalAlert && !(Line is MM_Tie))
                    AddUpdateMonitoringAlert(Line, MM_Repository.ViolationTypes["ThermalAlert"], Line.LinePercentageText);
                else if (Line.KVLevel != null && Line.KVLevel.MonitorEquipment && Line.LinePercentage >= Line.KVLevel.ThermalWarning && !(Line is MM_Tie))
                    AddUpdateMonitoringAlert(Line, MM_Repository.ViolationTypes["ThermalWarning"], Line.LinePercentageText);
                else
                    RemoveMonitoringAlert(Line);

            foreach (MM_Bus Bus in MM_Repository.BusNumbers.Values.ToArray())
            {
                if (Bus.BusNumber == 1585)
                { }
                if (Bus.KVLevel != null && Bus.KVLevel.MonitorEquipment && !Bus.Dead && !float.IsNaN(Bus.Estimated_kV) && Bus.Estimated_kV != 0 && (Bus.PerUnitVoltage >= Bus.KVLevel.HighVoltageAlert || Bus.PerUnitVoltage <= Bus.KVLevel.LowVoltageAlert))
                    AddUpdateMonitoringAlert(Bus, MM_Repository.ViolationTypes["VoltageAlert"], Bus.VoltageText);
                else if (Bus.KVLevel != null && Bus.KVLevel.MonitorEquipment && !Bus.Dead && (Bus.PerUnitVoltage >= Bus.KVLevel.HighVoltageWarning || Bus.PerUnitVoltage <= Bus.KVLevel.LowVoltageWarning))
                    AddUpdateMonitoringAlert(Bus, MM_Repository.ViolationTypes["VoltageWarning"], Bus.VoltageText);
                else
                    RemoveMonitoringAlert(Bus);
            }
        }
        static internal DateTime? TimeStarted = null;
        /// <summary>
        /// Add or update an alarm
        /// </summary>
        /// <param name="Elem"></param>
        /// <param name="AlarmType"></param>
        /// <param name="Text"></param>
        public static void AddUpdateMonitoringAlert(MM_Element Elem, MM_AlarmViolation_Type AlarmType, String Text)
        {
            MM_AlarmViolation Viol;
            bool AddViolation = true;
            if (ElementAlerts.TryGetValue(Elem, out Viol))
            {
                if (AlarmType != Viol.Type)
                {
                    ElementAlerts.Remove(Elem);
                    RemoveViolation(Viol);
                }
                else
                {
                    if (Viol.Text != Text)
                    {
                        Viol.EventTime = DateTime.Now;
                        Viol.Text = Text;
                        //UpdateViolation(Viol);
                    }
                    AddViolation = false;
                }
            }

            if (TimeStarted == null)
                TimeStarted = DateTime.Now;
            lock (ElementAlerts)
            {
            if (AddViolation && !ElementAlerts.ContainsKey(Elem))
            {
                ElementAlerts.Add(Elem, Viol = new MM_AlarmViolation());
                Viol.Type = AlarmType;
                Viol.ViolatedElement = Elem;
                Viol.EventTime = DateTime.Now;
                Viol.Text = Text;
                CheckAddViolation(Viol);

                // violations that occur after the inital load of MM should be flagged as new
                if ((Viol.EventTime - TimeStarted.Value).TotalMinutes > 6)
                    Viol.New = true;
            }
            }
        }

        /// <summary>
        /// Remove a monitoring alert
        /// </summary>
        /// <param name="Elem"></param>
        public static void RemoveMonitoringAlert(MM_Element Elem)
        {
            MM_AlarmViolation Viol;
            lock (ElementAlerts)
            {
            if (ElementAlerts.TryGetValue(Elem, out Viol))
            {
                RemoveViolation(Viol);
                ElementAlerts.Remove(Elem);
                }
            }
        }

        /// <summary>
        /// Go through all units and loads, and assign to their respective substations
        /// </summary>
        public static void RollUpElementsToSubstation()
        {
            bool IntegrateKV = true;

            //First, go through all of our elements, and pull them in.
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
                //MM_System_Interfaces.LogError(ex.ToString());  //nataros - why are soo many null ones ending up here??? what is going on ????? O_o //commented out to speed things up - mn - 6-4-13
            }

            foreach (MM_Element Elem in new List<MM_Element>(MM_Repository.TEIDs.Values))
                try
                {
                    if (Elem.Name != null)
                    {
                        if (Elem is MM_Substation)
                        {
                            MM_Substation Sub = (MM_Substation)Elem;

                            //Make sure our county and districts are represented
                            if (!Sub.County.Substations.Contains(Sub))
                                Sub.County.Substations.Add(Sub);


                        }
                        else if (Elem is MM_Blackstart_Corridor_Element)
                            CorridorElemsToRemove.Add(Elem.TEID);
                    }
                }
                catch (Exception)
                {
                    //MM_System_Interfaces.LogError("Error in rollup for " + Elem.GetType().Name + " " + Elem.TEID.ToString("#,##0") + ": " + ex.ToString()); //commented out to speed up start up - we know we have an issue here however it is not a real issue so we should be fine, just ignore for now and fix later - mn - 6-4-13
                }

            //Run our thread to map districts
            ThreadPool.QueueUserWorkItem(new WaitCallback(LoadDistricts));

            //Swap out our DC Ties for their equivalent lines - 30120607 - mn/ml
            foreach (MM_Tie Tie in MM_Repository.Ties.Values)
            {
                MM_Repository.Lines[Tie.AssociatedLine.Name] = Tie;
                MM_Repository.TEIDs[Tie.AssociatedLine.TEID] = Tie;
                //MM_Repository.Lines.Remove(Tie.AssociatedLine.Name);
                //MM_Repository.TEIDs.Remove(Tie.AssociatedLine.TEID);
                Tie.Counties = Tie.AssociatedLine.Counties;
                Tie.Coordinates = Tie.AssociatedLine.Coordinates;
                //Tie.TieDescriptor = Tie.Name;
                Tie.IntegrateFromLine(Tie.AssociatedLine);
                //Tie.Coordinates = Tie.AssociatedLine.Coordinates;

                //since we dont recompute the lines anymore, we need this here to get 'new' coords after making line go outside state border to fix model 
                //for easier viewing for our users. - //MN//20130607
                if (Tie.Coordinates.Count < 1)
                {
                    Tie.Coordinates = new List<PointF> { Tie.AssociatedLine.ConnectedStations[0].LngLat, Tie.AssociatedLine.ConnectedStations[1].LngLat };
                    Tie.AssociatedLine.Coordinates = Tie.Coordinates;
                }


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
            Dictionary<MM_Substation, List<MM_Bus>> Busbars = new Dictionary<MM_Substation, List<MM_Bus>>();
            Dictionary<MM_Substation, List<MM_ShuntCompensator>> ShuntCompensators = new Dictionary<MM_Substation, List<MM_ShuntCompensator>>();
            Dictionary<MM_Substation, List<MM_Breaker_Switch>> BreakerSwitches = new Dictionary<MM_Substation, List<MM_Breaker_Switch>>();

            var teids = MM_Repository.TEIDs.Values.ToList();
            //Go through all TEIDs
            foreach (MM_Element Elem in teids)
                if (Elem.TEID > 0)
                    try
                    {

                        if (IntegrateKV && Elem.Substation != null && Elem.KVLevel != null && !Elem.Substation.KVLevels.Contains(Elem.KVLevel))
                            Elem.Substation.KVLevels.Add(Elem.KVLevel);
                        else if (IntegrateKV && Elem is MM_Line)
                        {

                            foreach (MM_Substation LineSub in (Elem as MM_Line).ConnectedStations)
                            {
                                if (LineSub.KVLevels == null)
                                    LineSub.KVLevels = new List<MM_KVLevel>(5);
                                if (!LineSub.KVLevels.Contains(Elem.KVLevel))
                                    LineSub.KVLevels.Add(Elem.KVLevel);
                            }

                        }
                        if (Elem is MM_Unit)
                        {
                            if (Elem.Substation == null)
                            { }
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
                        else if (Elem is MM_Line)
                        {
                            if (((MM_Line)Elem).Substation1 == null)
                                ((MM_Line)Elem).Substation1 = Elem.Substation;
                            if (((MM_Line)Elem).Substation2 == null)
                                ((MM_Line)Elem).Substation2 = Elem.Substation;
                            if (((MM_Line)Elem).Substation1 == null)
                                MM_System_Interfaces.LogError(new Exception("Substation is not initialized on " + Elem.Name));
                        }
                        else if (Elem is MM_Contingency)
                        {
                            if (((MM_Contingency)Elem).Substations != null)
                            {
                                foreach (var sub in ((MM_Contingency)Elem).Substations)
                                    if (!sub.Contingencies.Any(cg => Elem.Name == cg.Name))
                                        sub.Contingencies.Add(((MM_Contingency)Elem));
                            }
                            else if (Elem.Substation != null)
                            {
                                ((MM_Contingency)Elem).Substations = new MM_Substation[1];
                                ((MM_Contingency)Elem).Substations[0] = Elem.Substation;
                            }
                            if (((MM_Contingency)Elem).Substations != null)
                            {
                                try
                                {
                                    var subs = ((MM_Contingency)Elem).Substations.ToList();
                                    // remove duplicate substations from this list.
                                    for (int i = 0; i < subs.Count; i++)
                                    {
                                        for (int j = 0; j < i; j++)
                                        {
                                            if (subs[j].Name == subs[i].Name)
                                                subs.RemoveAt(i--);
                                        }
                                    }
                                    ((MM_Contingency)Elem).Substations = subs.ToArray();
                                }
                                catch (Exception ex)
                                {
                                    MM_System_Interfaces.LogError(ex);
                                }
                            }
                        }
                        else if (Elem is MM_Transformer)
                        {

                            if (Elem.Substation == null && Elem.SubName != null && MM_Repository.Substations.ContainsKey(Elem.SubName))
                            {
                                MM_Substation sub = null;
                                MM_Repository.Substations.TryGetValue(Elem.SubName, out sub);
                                Elem.Substation = sub;
                            }
                            if (!Elem.Substation.Transformers.Contains((MM_Transformer)Elem))
                                Elem.Substation.Transformers.Add((MM_Transformer)Elem);
                            if (!Transformers.ContainsKey(Elem.Substation))
                                Transformers.Add(Elem.Substation, new List<MM_Transformer>());
                            Transformers[Elem.Substation].Add(Elem as MM_Transformer);

                            if (((MM_Transformer)Elem).Winding1 == null && ((MM_Transformer)Elem).Winding1.Name != null)
                            {
                                foreach (MM_Element elem in MM_Repository.TEIDs.Values)
                                {
                                    if (elem is MM_TransformerWinding && elem.Name == ((MM_Transformer)Elem).Winding1.Name)
                                    {
                                        ((MM_Transformer)Elem).Winding1 = (MM_TransformerWinding)elem;

                                        break;
                                    }
                                }
                            }
                            if (((MM_Transformer)Elem).Winding1 == null || ((MM_Transformer)Elem).Winding1.Name == null || ((MM_Transformer)Elem).Winding1.Name.Length < 3)
                            {
                                foreach (MM_Element elem in MM_Repository.TEIDs.Values)
                                {
                                    if (elem is MM_TransformerWinding && elem.Substation == Elem.Substation)
                                    {
                                        ((MM_Transformer)Elem).Winding1 = (MM_TransformerWinding)elem;
                                        break;
                                    }
                                }
                            }

                            if (((MM_Transformer)Elem).Winding1 != null && ((MM_Transformer)Elem).Winding1.Substation == null)
                                ((MM_Transformer)Elem).Winding1.Substation = Elem.Substation;
                            if (((MM_Transformer)Elem).Winding2 == null && ((MM_Transformer)Elem).Winding2.Name != null)
                            {
                                foreach (MM_Element elem in MM_Repository.TEIDs.Values)
                                {
                                    if (elem is MM_TransformerWinding && elem.Name == ((MM_Transformer)Elem).Winding2.Name)
                                    {
                                        ((MM_Transformer)Elem).Winding2 = (MM_TransformerWinding)elem;
                                        break;
                                    }
                                }
                            }
                            if (((MM_Transformer)Elem).Winding2 == null || ((MM_Transformer)Elem).Winding2.Name == null || ((MM_Transformer)Elem).Winding2.Name.Length < 3)
                            {
                                foreach (MM_Element elem in MM_Repository.TEIDs.Values)
                                {
                                    if (elem is MM_TransformerWinding && elem.Substation == Elem.Substation && elem != ((MM_Transformer)Elem).Winding1)
                                    {
                                        ((MM_Transformer)Elem).Winding2 = (MM_TransformerWinding)elem;
                                        break;
                                    }
                                }
                            }

                            if (((MM_Transformer)Elem).Winding2 != null && ((MM_Transformer)Elem).Winding2.Substation == null)
                                ((MM_Transformer)Elem).Winding2.Substation = Elem.Substation;
                        }
                        else if (Elem is MM_Bus && Elem.Substation != null)
                        {
                            MM_Bus Bus = (MM_Bus)Elem;
                            if (!Busbars.ContainsKey(Elem.Substation))
                                Busbars.Add(Elem.Substation, new List<MM_Bus>());
                            Busbars[Elem.Substation].Add(Bus);

                            if (Bus.BusNumber != -1 && !MM_Repository.BusNumbers.ContainsKey(Bus.BusNumber))
                                MM_Repository.BusNumbers.Add(Bus.BusNumber, Bus);
                        }
                        else if (Elem is MM_ShuntCompensator)
                        {
                            if (!ShuntCompensators.ContainsKey(Elem.Substation))
                                ShuntCompensators.Add(Elem.Substation, new List<MM_ShuntCompensator>());
                            ShuntCompensators[Elem.Substation].Add(Elem as MM_ShuntCompensator);
                        }
                        else if (Elem is MM_Breaker_Switch && Elem != null && Elem.Substation != null)
                        {
                            if (!BreakerSwitches.ContainsKey(Elem.Substation))
                                BreakerSwitches.Add(Elem.Substation, new List<MM_Breaker_Switch>());
                            BreakerSwitches[Elem.Substation].Add((MM_Breaker_Switch)Elem);
                        }

                        for (int i = 0; i < Elem.Contingencies.Count; i++)
                        {
                            MM_Contingency con = Elem.Contingencies[i];
                            if (con.Substations == null)
                                con.Substations = new MM_Substation[0];
                            if (con.Substations != null && Elem.Substation != null && Elem.Substation.Name != null && !Array.Exists(con.Substations, x => x.Substation != null && x.Substation.Name == Elem.Substation.Name))
                                con.Substations = con.Substations.Add(Elem.Substation);
                            //remove extras
                            for (int j = 0; j < i; j++)
                                if (Elem.Contingencies[j].Name == Elem.Contingencies[i].Name)
                                {
                                    Elem.Contingencies.RemoveAt(i--);
                                    break;
                                }
                        }

                    }
                    catch (Exception ex)
                    {
                        MM_System_Interfaces.LogError(ex);
                    }

            //Now pull out our lists
            foreach (KeyValuePair<MM_Substation, List<MM_Unit>> UnitCollection in Units)
                UnitCollection.Key.Units = UnitCollection.Value;
            foreach (KeyValuePair<MM_Substation, List<MM_Load>> LoadCollection in Loads)
                LoadCollection.Key.Loads = LoadCollection.Value;
            foreach (KeyValuePair<MM_Substation, List<MM_Transformer>> TransformerCollection in Transformers)
                TransformerCollection.Key.Transformers = TransformerCollection.Value;
            foreach (KeyValuePair<MM_Substation, List<MM_Bus>> BusCollection in Busbars)
                BusCollection.Key.BusbarSections = BusCollection.Value;
            foreach (KeyValuePair<MM_Substation, List<MM_ShuntCompensator>> SCCollection in ShuntCompensators)
                SCCollection.Key.ShuntCompensators = SCCollection.Value;
            foreach (KeyValuePair<MM_Substation, List<MM_Breaker_Switch>> BSCollection in BreakerSwitches)
                BSCollection.Key.BreakerSwitches = BSCollection.Value;
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

            //Determine our substation operatorships
            /*  MM_Company[] Companies;
              foreach (MM_Substation Sub in MM_Repository.Substations.Values)
                  Companies = Sub.Operators;*/
        }

        private static void LoadDistricts(object state)
        {
            foreach (MM_Substation Sub in MM_Repository.Substations.Values)
                foreach (MM_Boundary District in MM_Repository.Districts.Values)
                    if (District.HitTest(Sub.LngLat))
                    {
                        Sub.District = District;
                        District.Substations.Add(Sub);
                        break;
                    }
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
            if (Viol == null || Data_Integration.Permissions.PublicUser == true || Viol.ViolatedElement == null)
                return;
            try
            {
                MM_AlarmViolation FoundViol;

                if (Viol.ViolatedElement.Substation == null && !(Viol.ViolatedElement is MM_Line))
                    return;

                // don't care about external warnings.
                if (!(Viol.ViolatedElement is MM_Line) && Viol.ViolatedElement.Substation != null && !Viol.ViolatedElement.Substation.IsInternal && (Viol.Type.Name.IndexOf("Warning", StringComparison.InvariantCultureIgnoreCase) >= 0 || Viol.Type.Name.IndexOf("Branch", StringComparison.InvariantCultureIgnoreCase) >= 0))
                    return;

                //If our violation exists in our archived folder, then it's been taken care of and should be ignored.
                if (MM_Repository.ArchivedViolations.ContainsKey(Viol) || MM_Repository.ArchivedViolations.Values.ToList().Any(v => v.ViolatedElement.Name == Viol.ViolatedElement.Name && Viol.Type == v.Type))
                {
                    return;

                    //do nothing for now, to add time and test idea
                }
                //return; // test - nataros
                //MM_Repository.ViolationTypes["ForcedOutage"]
                if ((Viol.Type == MM_Repository.ViolationTypes["PlannedOutage"] || Viol.Type == MM_Repository.ViolationTypes["ForcedOutage"]) && Viol.ReportedEnd < DateTime.Now)
                    return;


                if (Viol.Type == MM_Repository.ViolationTypes["PlannedOutage"] && Viol.ViolatedElement.Violations.Count > 0) {

                    var forcedOutages = Viol.ViolatedElement.Violations.Values.ToList().Where(v => v.Type == MM_Repository.ViolationTypes["ForcedOutage"]).ToList();

                    for (int i = 0;forcedOutages != null && i < forcedOutages.Count;i++)
                        RemoveViolation(forcedOutages[i]);
                }

                if (MM_Repository.Violations.TryGetValue(Viol, out FoundViol) || (FoundViol = MM_Repository.Violations.Values.ToList().FirstOrDefault(v => v.ViolatedElement.Name == Viol.ViolatedElement.Name && Viol.Type == v.Type)) != null)
                {
                    if (!String.Equals(Viol.Text, FoundViol.Text) || Viol.EventTime != FoundViol.EventTime || Viol.New != FoundViol.New)
                    {
                        FoundViol.Text = Viol.Text;
                        FoundViol.EventTime = Viol.EventTime;
                        //FoundViol.New = Viol.New; (we might have acknowledged it already).
                        FoundViol.PostCtgValue = Viol.PostCtgValue;
                        FoundViol.PreCtgValue = Viol.PreCtgValue;
                        FoundViol.ViolatedElement.TriggerValueChange("Violation");
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
                    MM_System_Interfaces.LogError("Unable to add {0} {1} to violation by element type collection, because the element type collection didn't include {0}s", Viol.ViolatedElement.ElemType.Name, Viol.ViolatedElement.Name);
                else if (!Viol.Type.ViolationsByElementType[Viol.ViolatedElement.ElemType].ContainsKey(Viol))
                    Viol.Type.ViolationsByElementType[Viol.ViolatedElement.ElemType].Add(Viol, Viol);
                */
                //Add this violation to the violated element's collection of violations
                if (!Viol.ViolatedElement.Violations.ContainsKey(Viol) && Viol.ViolatedElement.Violations.Count < 25)
                    Viol.ViolatedElement.Violations.Add(Viol, Viol);

                //If we have a breaker-to-breaker path, update accordingly
                if (Viol.ViolatedElement.Contingencies != null)
                    foreach (MM_Contingency Ctg in Viol.ViolatedElement.Contingencies)
                        if (!Ctg.Violations.ContainsKey(Viol))
                            Ctg.Violations.Add(Viol, Viol);

                //If we have an associated substation, add this violation to the list
                if (Viol.ViolatedElement.Substation != null && !(Viol.ViolatedElement is MM_Line) && !Viol.ViolatedElement.Substation.Violations.ContainsKey(Viol) && Viol.ViolatedElement.Violations.Count < 25)
                    Viol.ViolatedElement.Substation.Violations.Add(Viol, Viol);


                //If we have a contingency definition, add this violation to the list
                if (Viol.Contingency != null)
                {
                    if (!Viol.Contingency.Violations.ContainsKey(Viol) && Viol.Contingency.Violations.Count < 25)
                        Viol.Contingency.Violations.Add(Viol, Viol);
                }


                if (!MM_Repository.Violations.ContainsKey(Viol))
                {
                    //Now add it to the master list
                    MM_Repository.Violations.Add(Viol, Viol);
                    if (ViolationAdded != null)
                        ViolationAdded(Viol);

                    Viol.ViolatedElement.TriggerValueChange("Violation");

                    if (Viol.Type.SpeakViolation && UseSpeech)
                        ThreadPool.QueueUserWorkItem(new WaitCallback(SpeakText), Viol);
                }
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
            MM_System_Interfaces.LogError("Speech: " + TextToSpeak);
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
            if (Viol == null)
                return;
            Viol.EventTime = ViolDate;
            Viol.Text = ViolText;
            if (ViolationModified != null)
                ViolationModified(Viol);
        }

        /// <summary>
        /// Update an existing violation
        /// </summary>
        /// <param name="Viol"></param>
        public static void UpdateViolation(MM_AlarmViolation Viol)
        {
            MM_AlarmViolation FoundViol;
            if (Viol == null || Viol.ViolatedElement == null)
                return;
            if (!MM_Repository.Violations.TryGetValue(Viol, out FoundViol))
                CheckAddViolation(Viol);
            else
            {
                FoundViol.EventTime = Viol.EventTime;
                FoundViol.Text = Viol.Text;
                FoundViol.ReportedStart = Viol.ReportedStart;
                FoundViol.ReportedEnd = Viol.ReportedEnd;
                FoundViol.PreCtgValue = Viol.PreCtgValue;
                FoundViol.PostCtgValue = Viol.PostCtgValue;
                FoundViol.New = Viol.New;
                if (ViolationModified != null)
                    ViolationModified(Viol);
            }
        }

        /// <summary>
        /// Remove a violation from the collection
        /// </summary>
        /// <param name="Violation">The violation to be removed</param>
        public static void RemoveViolation(MM_AlarmViolation Violation)
        {
            try
            {
                if (Violation == null || Violation.ViolatedElement == null)
                    return;

                if (!MM_Repository.ArchivedViolations.ContainsKey(Violation))
                    MM_Repository.ArchivedViolations.Add(Violation, Violation);
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
                if (Violation.ViolatedElement != null)
                    Violation.ViolatedElement.Violations.Remove(Violation);

                //Remove the violation from the substation collection
                if (Violation.ViolatedElement != null && Violation.ViolatedElement.Substation != null)
                    Violation.ViolatedElement.Substation.Violations.Remove(Violation);

                //If the violated element is in a trace, remove it
                if (Violation.ViolatedElement != null && Violation.ViolatedElement.Contingencies != null)
                    foreach (MM_Contingency Ctg in Violation.ViolatedElement.Contingencies)
                        Ctg.Violations.Remove(Violation);

                //Remove the violation from the contingency
                if (Violation.Contingency != null)
                    Violation.Contingency.Violations.Remove(Violation);

                if (ViolationRemoved != null)
                    ViolationRemoved(Violation);
                if (Violation.ViolatedElement != null)
                    Violation.ViolatedElement.TriggerValueChange("Violation");

                /*This following archived violations should only function for non-ots as removed violations never return.  
                 *This should be addressed in the future, however it is as was in 'old' mmap application. - 20130611 - mn/ym */
                if (Permissions.OTS == false)
                    MM_Repository.ArchivedViolations.Add(Violation, Violation);
                //MM_System_Interfaces.LogError("Violation archived: " + Violation.ToString()); //commented out to speed up alarms - mn 6-4-13
            }
            catch { }
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

        /// <summary>The current time of our system</summary>
        public static DateTime CurrentTime
        {
            get
            {
                if (TimeOverride)
                    return _CurrentTime;
                else
                    return DateTime.Now;
            }
            set
            {
                if (value == DateTime.MinValue)
                    return;
                _CurrentTime = value;
                TimeOverride = true;
            }
        }

        /// <summary>Our overridden current time, if needed</summary>
        private static DateTime _CurrentTime = DateTime.Now;

        /// <summary>Whether our current time is defined externally</summary>
        public static bool TimeOverride = false;

        /// <summary>Our collection of STNET cases</summary>
        public static SortedDictionary<String, String[]> Savecases;

        /// <summary>The chart data for our frequency, ACE, etc. charts</summary>
        public static MM_Chart_Data[] ChartData;

        /// <summary>The time stamp when our charting data were last received</summary>
        public static DateTime ChartDate;

        /// <summary>The color that ACE should be assigned</summary>
        public static Color ACEColor = Color.White;

        /// <summary>The time stamp of when our load forecast was last received</summary>
        public static DateTime LFDate;

        /// <summary>The current simulator status</summary>
        public static MM_Simulation_Time.enumSimulationStatus SimulatorStatus = MM_Simulation_Time.enumSimulationStatus.Unknown;

        /// <summary>
        /// Report the color associated with our simulator
        /// </summary>
        public static Color SimulatorStatusColor
        {
            get
            {
                if (Data_Integration.SimulatorStatus == MM_Simulation_Time.enumSimulationStatus.Running)
                    return Color.Green;
                //else if (SimulatorStatus == MM_Simulation_Time.enumSimulationStatus.Paused || SimulatorStatus == MM_Simulation_Time.enumSimulationStatus.Recovery || SimulatorStatus == MM_Simulation_Time.enumSimulationStatus.Starting || SimulatorStatus == MM_Simulation_Time.enumSimulationStatus.Pausing)
                //    return Color.Yellow;
                else
                    return Color.DarkOrange;
            }
        }



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
        /// Report system-level data - something so important it should be spoken
        /// </summary>
        /// <param name="Data">The data for the string</param>
        /// <param name="args">The arguments for the string</param>
        public static void ReportSystemLevelData(String Data, params string[] args)
        {
            String TextToSpeak = String.Format(Data, args);
            MM_System_Interfaces.LogError(TextToSpeak);
            if (UseSpeech)
            {
                Speech.Rate = 3;
                System.Media.SystemSounds.Exclamation.Play();
                Speech.SpeakAsync(TextToSpeak);
            }
        }



        /// <summary>
        /// Shut down all connections, threads, etc.
        /// </summary>
        public static void ShutDown()
        {
            //TODO: Write shutdown code

        }

        /// <summary>
        /// Add/update a note entry from XML
        /// </summary>
        /// <param name="InNote"></param>
        /// <returns></returns>
        public static MM_Note HandleNoteEntry(MM_Note InNote)
        {
            MM_Element AssociatedElement;
            if (MM_Repository.TEIDs.TryGetValue(InNote.AssociatedElement, out AssociatedElement) && AssociatedElement != null)
                AssociatedElement.Notes.Add(InNote.ID, InNote);
            MM_Repository.Notes.Add(InNote.ID, InNote);
            if (NoteAdded != null)
                NoteAdded(InNote);
            return InNote;
        }

        /// <summary>
        /// Handle a note modification
        /// </summary>
        /// <param name="InNote"></param>
        public static void HandleNoteModification(MM_Note InNote)
        {
            if (MM_Repository.Notes.ContainsKey(InNote.ID) && NoteModified != null)
                NoteModified(InNote);
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
                MM_Element AssociatedElement = MM_Repository.TEIDs[TEID];
                FoundNote = new MM_Note() { ID = ID, CreatedOn = CreatedOn, Author = Author, AssociatedElement = TEID, Acknowledged = true, Note = Note };
                MM_Repository.Notes.Add(ID, FoundNote);
                if (AssociatedElement != null)
                    AssociatedElement.Notes.Add(ID, FoundNote);
                if (NoteAdded != null)
                    NoteAdded(FoundNote);
            }
            else if (FoundNote.CreatedOn != CreatedOn)
            {
                FoundNote.CreatedOn = CreatedOn;
                FoundNote.AssociatedElement = TEID;
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
                MM_Element AssociatedElement = MM_Repository.TEIDs[NoteToRemove.AssociatedElement];
                if (AssociatedElement != null && AssociatedElement.Notes.ContainsKey(ID))
                    AssociatedElement.Notes.Remove(ID);
                if (AssociatedElement != null && AssociatedElement.Substation != null && AssociatedElement.Substation.Notes.ContainsKey(ID))
                    AssociatedElement.Substation.Notes.Remove(ID);
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
            //MM_System_Interfaces.LogError("Violation " + Viol.TEID + " (" + Viol.EventTime.ToString() + " " + Viol.Text + ") acknowledged: Element " + Viol.ViolatedElement.TEID + " (" + (Viol.ViolatedElement.Substation != null ? Viol.ViolatedElement.Substation + " " : "") + Viol.ViolatedElement.ElemType + " " + Viol.ViolatedElement.Name + (Viol.ViolatedElement.KVLevel != null ? " (" + Viol.ViolatedElement.KVLevel + ")" : "")); //commented out to speed up alarms - mn - 6-4-13
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

                MM_System_Interfaces.MessageBox("Form closed unexpectedly: " + ex.Message + "\n" + ConvertError(ex), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (FormToDisplay is MM_Form_Builder && (FormToDisplay as MM_Form_Builder).viewOneLine != null)
                    (FormToDisplay as MM_Form_Builder).viewOneLine.ShutDownQuery();
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

        /// <summary>
        /// Handle our commands
        /// </summary>
        /// <param name="Command"></param>
        public static void HandleEMSCommands(MM_EMS_Command[] Command)
        {
            EMSCommands.AddRange(Command);
            if (EMSCommandAdded != null)
                EMSCommandAdded(null, new MM_EMS_Command_Display.EMSCommandEventArgs(Command));
        }

        /// <summary>
        /// Handle our EMS events clearing
        /// </summary>
        public static void HandleEMSCommandClear()
        {
            if (EMSCommands == null)
                EMSCommands = new List<MM_EMS_Command>();
            EMSCommands.Clear();
            if (EMSCommandsCleared != null)
                EMSCommandsCleared(null, EventArgs.Empty);
        }


        /// <summary>
        /// Handle an island removal
        /// </summary>
        /// <param name="Island"></param>
        public static void HandleIslandRemoval(MM_Island Island)
        {
            if (IslandRemoved != null)
                IslandRemoved(Island);
        }

        /// <summary>
        /// HAndle an island addition
        /// </summary>
        /// <param name="Island"></param>
        public static void HandleIslandAdd(MM_Island Island)
        {
            if (IslandAdded != null)
                IslandAdded(Island);
        }




    }
}