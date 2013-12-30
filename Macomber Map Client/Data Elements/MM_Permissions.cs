using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Reflection;
using Macomber_Map.Data_Connections;
using System.Windows.Forms;

namespace Macomber_Map.Data_Elements
{
    /// <summary>
    /// This class holds the permissions restrictions on the Macomber Map.
    /// </summary>
    public class MM_Permissions
    {
        #region Variable declaration
        /// <summary>Whether the user can execute commands</summary>
        public bool ShowCommands = true;

        /// <summary>Whether ownership information should be shown</summary>
        public bool ShowOwnership = true;

        /// <summary>Whether DUNS information should be shown</summary>
        public bool ShowDUNS = true;

        /// <summary>Whether the user can switch different models</summary>
        public bool AllowModelSwitching = true;

        /// <summary>Whether telephone information should be shown</summary>
        public bool ShowTelephone = true;

        /// <summary>The connection threshold in seconds for a timeout event to be considered</summary>
        public int ConnectionTimeoutThreshold = 120;

        /// <summary>The voltage limits for alarms to flag (when auto-generated in simulation mode</summary>
        public int LineVoltageLimit = 120;

        /// <summary>The lower limit for bus voltage (when auto-generated in simulation mode</summary>
        public int BusLowLimit = 80;

        /// <summary>The upper limit for bus voltage (when auto-generated in simulation mode</summary>
        public int BusHighLimit = 120;

        /// <summary>OTS vs RT net Flags</summary>
        public bool OTS = false;
      
        /// <summary>Whether the user is public, and thus limited information should be shown</summary>        
        public bool PublicUser = false;

        /// <summary>Dev options such as second disconnect statement or other options to allow for alternate run modes if required/// </summary>
        public bool runDevOptions = false;

        /// <summary>For future implementation of no-violations start up mode/// </summary>
        public bool ShowViolations = false; 

        ///<summary>Temp Option to turn things off that are not working quite right</summary>
        public bool showRTHist = false;

        /// <summary>Whether Processes should be shown</summary>
        public bool ShowProcesses= true;
        
        /// <summary>Whether views should be shown</summary>
        public bool ShowViews = true;

        /// <summary>Whether the user can show history</summary>
        public bool ShowHistory = true;

        /// <summary>Whether breaker-to-breakers should be shown</summary>
        public bool ShowBreakerToBreakers = true;

        /// <summary>Whether RAP/SPSs should be shown</summary>
        public bool ShowRASs = true;

        /// <summary>Whether units are shown (right-click menu or search)</summary>
        public bool ShowUnits = true;

        /// <summary>Whether loads are shown (right-click menu or search)</summary>
        public bool ShowLoads = true;

        /// <summary>Whether shunt compensators are shown (right-click menu or search)</summary>
        public bool ShowShuntCompensators = true;

        /// <summary>Whether transformers are shown (right-click menu or search)</summary>
        public bool ShowTransformers= true;       

        /// <summary>Whether buses are shown (right-click menu or search)</summary>
        public bool ShowBuses = true;

        /// <summary>Whether estimated MVAs are displayed (on network map, or right-click menu)</summary>
        public bool ShowEstMVAs = true;

        /// <summary>Whether estimated voltages and pUs are displayed (on network map, or right-click menu)</summary>
        public bool ShowEstVs = true;

        /// <summary>Whether telemetered voltages and pUs are displayed (on network map, or right-click menu)</summary>
        public bool ShowTelVs = true;

        /// <summary>Whether telemetered MVAs are displayed (on network map, or right-click menu)</summary>
        public bool ShowTelMVAs = true;

        /// <summary>Whether estimated MVARs are displayed (on network map, or right-click menu)</summary>
        public bool ShowEstMVARs = true;

        /// <summary>Whether telemetered MVARs are displayed (on network map, or right-click menu)</summary>
        public bool ShowTelMVARs = true;

        /// <summary>Whether estimated MWs are displayed (on network map, or right-click menu)</summary>
        public bool ShowEstMWs = true;

        /// <summary>Whether telemetered MWs are displayed (on network map, or right-click menu)</summary>
        public bool ShowTelMWs = true;

        /// <summary>Whether limits are displayed (on network map, or right-click menu)</summary>
        public bool ShowLimits = true;

        /// <summary>Whether loading percentages (MVA/Normal limit) are displayed (on network map, or right-click menu)</summary>
        public bool ShowPercentageLoading = true;

        /// <summary>Whether the user  call up one-lines</summary>
        public bool ShowOneLines = true;

        /// <summary>Whether the user can move the map around elements</summary>
        public bool ShowMoveMap = true;

        /// <summary>Whether the user  Show notes</summary>
        public bool ShowNotes = true;

        /// <summary>Whether the user can call up the property page</summary>
        public bool ShowPropertyPage = true;

        /// <summary>Whether the user  Show violations</summary>
        public bool ProcessViolations = true;

        /// <summary>Whether the user can suggest coordinate updates</summary>
        public bool SuggestCoordinateUpdates = true;

        /// <summary>Whether the user can play the situation awareness game</summary>
        public bool PlayGame = true;

        /// <summary>Whether the user  Show the list of elements within a substation</summary>
        public bool ShowElementTypes = true;

        /// <summary>Whether the user  Show the list of KV Levels within a substation</summary>
        public bool ShowKVLevels = true;

        /// <summary>Whether the user can see a line's distance</summary>
        public bool ShowDistance = true;
        
        /// <summary>Whether counties are shown on substation displays</summary>
        public bool ShowCounties = true;

        /// <summary>Whether the website option is shown on substation displays</summary>
        public bool ShowWebsite = false;

        /// <summary>Whether TEIDs are shown</summary>
        public bool ShowTEIDs = true;

        /// <summary>Whether the user  create notes</summary>
        public bool CreateNotes = true;

        /// <summary>Whether the search feature is available to the user</summary>
        public bool SearchAvailable = true;

        /// <summary>Whether the user  draw a standard lasso</summary>
        public bool LassoStandard = true;

        /// <summary>Whether the user  create an lasso including invisible elements</summary>
        public bool LassoInvisible = true;

        /// <summary>Whether the user  Show EMS ICCP/ISD links</summary>
        public bool DisplayICCPISDLinks = true;

        /// <summary>Whether the user  Show the communications status between MM and connected systems</summary>
        public bool DisplayMMCommLinks = true;

        /// <summary>Whether the user  Show energized lines</summary>
        public bool ShowEnergizedLines = true;

        /// <summary>Whether the user  Show de-energized lines</summary>
        public bool ShowDeenergizedLines = true;

        /// <summary>Whether the user  Show partially energized lines</summary>
        public bool ShowPartiallyEnergizedLines = true;

        /// <summary>How far back the user  go in history</summary>
        public TimeSpan HistoricalTime = TimeSpan.FromSeconds(0);

        /// <summary>How far into the future the user  go</summary>        
        public TimeSpan FutureTime = TimeSpan.FromSeconds(0);

        /// <summary>The maximum timespan for a query</summary>
        public TimeSpan MaxSpan = TimeSpan.FromDays(365 * 5);

        /// <summary>Whether the user can see mouseover details on a historical view</summary>
        public bool HistoricalSelection = true;

        /// <summary>Whether boilerplate text needs to be added</summary>
        public string BoilerplateText = "";

        /// <summary>Whether the user's activity should be logged</summary>
        public bool LogActivity = false;

        /// <summary>Whether the unit's generation type is shown</summary>
        public bool ShowUnitGenerationType = true;

        /// <summary>Whether the default connectors should be used</summary>
        public bool DefaultConnectors = false;

        /// <summary>Whether the user can create a savecase</summary>
        public bool CreateSavecase = true;

        /// <summary>Whether the user can access the display properties page</summary>
        public bool DisplayProperties = true;

        /// <summary>The maximum zoom level for the network map</summary>
        public int MaxZoom = 30;

        /// <summary>The miniumum zoom level for the network map.</summary>
        public int MinZoom = 5;

       
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize the MM permissions
        /// </summary>
        /// <param name="xConfig">The configuration element for this permissions.</param>
        public MM_Permissions(XmlElement xConfig)
        {
            if (xConfig != null)
                foreach (XmlAttribute xAttr in xConfig.Attributes)
                    try
                    {
                        FieldInfo fI = this.GetType().GetField(xAttr.Name);
                        if (fI.FieldType == typeof(bool))
                            fI.SetValue(this, XmlConvert.ToBoolean(xAttr.Value));
                        else if (fI.FieldType == typeof(String))
                            fI.SetValue(this, xAttr.Value);
                        else if (fI.FieldType == typeof(int))
                            fI.SetValue(this, XmlConvert.ToInt32(xAttr.Value));
                    }
                    catch (Exception ex)
                    {
                        Program.ErrorLog.WriteLine("Error in retriving permission {0}={1}: {2}", xAttr.Name, xAttr.Value, ex.Message);
                    }

            BoilerplateText = xConfig.InnerText;
                

        }
        #endregion

    }
}
