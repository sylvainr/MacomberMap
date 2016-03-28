using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Integration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using MacomberMapCommunications.Extensions;
using SharpDX;
using SharpDX.Mathematics.Interop;
using Color = System.Drawing.Color;

namespace MacomberMapClient.Data_Elements.Display
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// The parameters for the overall display
    /// </summary>
    public class MM_Display : MM_Serializable
    {
        #region Varaibles
        private int _StationNames, _LineFlows, _LineText, _StationMW, _TextContrast;
        private int _FlowInterval = 400;
        private int _MultipleLineWidthAddition = 2;
        private int _BlackstartDimmingFactor = 20;
        private int _StationZoomLevel = 12;
        private int _SystemValueCount = 20;
        private float _ContourThreshold = 5f;
        private float _ContourBrightness = 400f;
        private float _EnergizationThreshold = 0;
        private float _MapTransparency = 0.5f;
        private bool _DisplayCounties = true;
        private bool _DisplayStateBorder = true;
        private bool _FPS = false;
        private bool _UseLongNames = true;
        private bool _SplitBusesByVoltage = true;
        private bool _GroupLinesByVoltage = true;
        private bool _DisplayDistricts = false;
        private bool _MoveLines = false;
        private string _MapTileCacheLocation = Path.Combine(Application.StartupPath, "MapTileCache");
        private enumBlackstartMode _BlackstartMode = enumBlackstartMode.DimExternalElements;
        private MM_Contour_Enum _Contour = MM_Contour_Enum.None;
        private SmoothingMode _SmoothingMode = SmoothingMode.None;
        private TextRenderingHint _TextRenderingHint = TextRenderingHint.SystemDefault;
        private PixelOffsetMode _PixelOffsetMode = PixelOffsetMode.None;
        private InterpolationMode _InterpolationMode = InterpolationMode.Default;
        private CompositingMode _CompositingMode = CompositingMode.SourceOver;
        private CompositingQuality _CompositingQuality = CompositingQuality.Default;
        private MM_MapTile.enumMapType _MapTiles = MM_MapTile.enumMapType.None;
        private Font _NetworkMapFont = new Font("Arial", 10);
        private Font _KeyIndicatorSimpleFont = new Font("Arial", 14F, FontStyle.Bold, GraphicsUnit.Point, (Byte)0);
        private Font _KeyIndicatorLabelFont = new Font("Arial", 14F, FontStyle.Bold, GraphicsUnit.Point, (Byte)0);
        private Font _KeyIndicatorValueFont = new Font("Arial", 14F, FontStyle.Regular, GraphicsUnit.Point, (Byte)0);
        private Color _BackgroundColor = Color.Black;
        private Color _ForegroundColor = Color.White;
        private Color _WarningColor = Color.Yellow;
        private Color _ErrorColor = Color.Red;
        private Color _ErrorBackground = Color.Black;
        private Color _WarningBackground = Color.Black;
        private Color _HighValue = Color.Red;
        private Color _LowValue = Color.Blue;
        private Color _NormalColor = Color.LightGreen;
        private Color _NormalBackground = Color.Black;
        private Size _MapTileSize = new Size(256, 256);
        private MM_Company _DisplayCompany = null;

        /// <summary>
        /// Our event handler for the map transparency changing
        /// </summary>
        public event EventHandler<EventArgs> MapTransparencyChanged;

        /// <summary>Our collection of blackstart modes</summary>
        public enum enumBlackstartMode
        {
            /// <summary>Show all elements</summary>
            ShowAll,
            /// <summary>Dim non-blackstar elements</summary>
            DimNonBlackstartElements,
            /// <summary>Hide non-blackstart elements</summary>
            HideNonBlackstartElements,
            /// <summary>Dim non-blackstar elements</summary>
            DimNonOperatorElements,
            /// <summary>Hide non-blackstart elements</summary>
            HideNonOperatorElements,
            /// <summary>Hide external elements</summary>
            DimExternalElements,
            /// <summary>Hide external elements</summary>
            HideExternalElements
        }

        /// <summary>
        /// The collection of contours available
        /// </summary>
        public enum MM_Contour_Enum
        {
            /// <summary>Show bus voltages</summary>
            BusVoltage,
            /// <summary>Show standard deviation of bus angle</summary>
            BusAngle,
            /// <summary>Show maximum locational market prices</summary>
            LMPMaximum,
            /// <summary>Show average locational market prices</summary>
            LMPAverage,
            /// <summary>Show standard deviation of locational market prices</summary>
            LMPStdDev,
            /// <summary>Show minimium locational market prices</summary>
            LMPMinimum,
            /// <summary>The weather zones</summary>
            WeatherZones,
            /// <summary>The load zones</summary>
            LoadZones,
            /// <summary>No contours</summary>
            None
        }

        /// <summary>
        /// Our collection of contour types
        /// </summary>
        public enum ContourOverlayDataTypes
        {
            /// <summary>No contour</summary>
            None,
            /// <summary>Voltage Contour</summary>
            KVAverage,
            /// <summary>LMP Contour</summary>
            LMPAverage,
            /// <summary>Line loading contour</summary>
            LineLoadingPercentage
        }

        /// <summary>Data to show in the contour overlay.</summary>
        [Category("Contour Overlay"), Description("Data to show in the contour overlay."), DefaultValue(2)]
        public ContourOverlayDataTypes ContourData { get; set; } 

        /// <summary>Data to show in the contour overlay.</summary>
        [Category("Contour Overlay"), Description("Contour automatic refresh time in seconds."), DefaultValue(2)]
        public int ContourRefreshTime { get; set; }

        /// <summary>
        /// Whether lines should be grouped voltage level, and then in/out
        /// </summary>
        [Category("Lines"), Description("The extra padding over a multiple line"), DefaultValue(2)]
        public int MultipleLineWidthAddition
        {
            get { return _MultipleLineWidthAddition; }
            set { _MultipleLineWidthAddition = value; }
        }

        /// <summary>
        /// Whether lines should be grouped voltage level, and then in/out
        /// </summary>
        [Category("Display"), Description("Whether lines should be grouped voltage level, and then in/out"), DefaultValue(true)]
        public bool GroupLinesByVoltage
        {
            get { return _GroupLinesByVoltage; }
            set { _GroupLinesByVoltage = value; }
        }

        /// <summary>
        /// Whether we can suggest new coordinates for lines
        /// </summary>
        [Category("Display"), Description("Whether we can suggest new coordinates for lines"), DefaultValue(false)]
        public bool MoveLines
        {
            get { return _MoveLines; }
            set { _MoveLines = value; }
        }
        /// The zoom level one wants to be at when zeroing in on a substation
        /// </summary>
        [Category("Display"), Description("The zoom level one wants to be at when zeroing in on a substation"), DefaultValue(4)]
        public int StationZoomLevel
        {
            get { return _StationZoomLevel; }
            set { _StationZoomLevel = value; }
        }

        /// <summary>
        /// Should Lines be Rendered.
        /// </summary>
        [Category("Display"), Description("Should Lines be Rendered."), DefaultValue(true)]
        public bool ShowLines { get; set; } 

        [Category("Display"), Description("Add Tie label for each Tie line"), DefaultValue(true)]
        public bool LabelTieLines { get; set;}

        [Category("Display"), Description("Target FPS. This limits rendering speed to prevent overusing system resources. 60 or 30 are good values. Values <= 0 are limited to 240 FPS."), DefaultValue(true)]
        public int TargetFPS { get; set; }

        [Category("Contour Overlay"), Description("Clip Contour to footprint"), DefaultValue(true)]
        public bool ClipContours { get; set; } 

       

        /// <summary>
        /// Whether buses should be split by voltage level
        /// </summary>
        [Category("Display"), Description("Whether buses should be split by voltage level"), DefaultValue(true)]
        public bool SplitBusesByVoltage
        {
            get { return _SplitBusesByVoltage; }
            set { _SplitBusesByVoltage = value; }
        }

        /// <summary>
        /// The number of points to be displayed on the graph
        /// </summary>
        [Category("Display"), Description("The number of points to be displayed on the graph")]
        public int SystemValueCount
        {
            get { return _SystemValueCount; }
            set { _SystemValueCount = value; }
        }

        /// <summary>
        /// The font for text on the network map
        /// </summary>
        [Category("Display"), Description("The font for text on the network map")]
        public Font NetworkMapFont
        {
            get { return _NetworkMapFont; }
            set { _NetworkMapFont = value; }
        }

        /// <summary>
        /// The font for the label on a key indicator
        /// </summary>
        [Category("Display"), Description("The font for the label on a key indicator")]
        public Font KeyIndicatorLabelFont
        {
            get { return _KeyIndicatorLabelFont; }
            set { _KeyIndicatorLabelFont = value; }
        }

        /// <summary>
        /// The font for the value on a key indicator
        /// </summary>
        [Category("Display"), Description("The font for the value on a key indicator")]
        public Font KeyIndicatorValueFont
        {
            get { return _KeyIndicatorValueFont; }
            set { _KeyIndicatorValueFont = value; }
        }

        /// <summary>
        /// The simple font for a key indicator (used for measurements)
        /// </summary>
        [Category("Display"), Description("The simple font for a key indicator (used for measurements)")]
        public Font KeyIndicatorSimpleFont
        {
            get { return _KeyIndicatorSimpleFont; }
            set { _KeyIndicatorSimpleFont = value; }
        }

        /// <summary>
        /// The smoothing mode
        /// </summary>
        [Category("Display"), Description("The smoothing mode"), DefaultValue(SmoothingMode.None)]
        public SmoothingMode SmoothingMode
        {
            get { return _SmoothingMode; }
            set { _SmoothingMode = value; }
        }

        /// <summary>
        /// The compositing quality
        /// </summary>
        [Category("Display"), Description("The compositing quality"), DefaultValue(CompositingQuality.Default)]
        public CompositingQuality CompositingQuality
        {
            get { return _CompositingQuality; }
            set { _CompositingQuality = value; }
        }

        /// <summary>
        /// The text rendering hint
        /// </summary>
        [Category("Display"), Description("The text rendering hint"), DefaultValue(TextRenderingHint.SystemDefault)]
        public TextRenderingHint TextRenderingHint
        {
            get { return _TextRenderingHint; }
            set { _TextRenderingHint = value; }
        }

        /// <summary>
        /// The text contrast
        /// </summary>
        [Category("Display"), Description("The text contrast")]
        public int TextContrast
        {
            get { return _TextContrast; }
            set { _TextContrast = value; }
        }

        /// <summary>
        /// The pixel offset mode
        /// </summary>
        [Category("Display"), Description("The pixel offset mode"), DefaultValue(PixelOffsetMode.None)]
        public PixelOffsetMode PixelOffsetMode
        {
            get { return _PixelOffsetMode; }
            set { _PixelOffsetMode = value; }
        }

        /// <summary>
        /// The interpolation mode
        /// </summary>
        [Category("Display"), Description("The interpolation mode"), DefaultValue(InterpolationMode.Default)]
        public InterpolationMode InterpolationMode
        {
            get { return _InterpolationMode; }
            set { _InterpolationMode = value; }
        }

        /// <summary>
        /// The compositing mode
        /// </summary>
        [Category("Display"), Description("The compositing mode"), DefaultValue(CompositingMode.SourceOver)]
        public CompositingMode CompositingMode
        {
            get { return _CompositingMode; }
            set { _CompositingMode = value; }
        }

        /// <summary>
        /// The threshold (above/below mean) needed for the contour to be shown
        /// </summary>
        [Category("Contour"), Description("The threshold (above/below mean) needed for the contour to be shown"), DefaultValue(5f)]
        public float ContourThreshold
        {
            get { return _ContourThreshold; }
            set { _ContourThreshold = value; }
        }

        /// <summary>
        /// The brightness for the contour
        /// </summary>
        [Category("Contour"), Description("The brightness for the contour"), DefaultValue(400f)]

        public float ContourBrightness
        {
            get { return _ContourBrightness; }
            set { _ContourBrightness = value; }
        }

        /// <summary>
        /// The contour (by county) to be shown
        /// </summary>
        [Category("Contour"), Description("The contour (by county) to be shown"), DefaultValue(MM_Display.MM_Contour_Enum.None)]
        public MM_Contour_Enum Contour
        {
            get { return _Contour; }
            set { _Contour = value; }
        }

        /// <summary>
        /// The interval at which the network flow arrows
        /// </summary>
        [Category("Display Options"), Description("The interval at which the network flow arrows"), DefaultValue(400)]
        public int FlowInterval
        {
            get { return _FlowInterval; }
            set { _FlowInterval = value; }
        }

        /// <summary>Show Line Flow Arrows on Map</summary>
        [Category("Lines"), Description("Show Line Flow Arrows on Map")]
        public bool ShowLineFlows { get; set; } 

        /// <summary>Show legends</summary>
        [Category("Display Options"), Description("Show legends"), DefaultValue(false)]
        public bool ShowLegend { get; set; } 
        /// <summary>
        /// Whether the long names should be used for substations (e.g., West Batesville vs. w_batesv)
        /// </summary>
        [Category("Display Options"), Description("Display outage scheduler name or EMS descrip. for substations"), DefaultValue(true)]
        public bool UseLongNames
        {
            get { return _UseLongNames; }
            set { _UseLongNames = value; }
        }

        /// <summary>
        /// Return the name of the element
        /// </summary>
        public string Name
        {
            get { return "Overall"; }
            set { }
        }

        /// <summary>
        /// Whether the number of frames per second should be displayed in the network map
        /// </summary>
        [Category("Display Options"), Description("Whether the number of frames per second should be displayed in the network map"), DefaultValue(false)]
        public bool FPS
        {
            get { return _FPS; }
            set { _FPS = value; }
        }

        /// <summary>
        /// Whether counties should be displayed in the network map
        /// </summary>
        [Category("Display Options"), Description("Whether counties should be displayed in the network map"), DefaultValue(true)]
        public bool DisplayCounties
        {
            get { return _DisplayCounties; }
            set { _DisplayCounties = value; }
        }

        /// <summary>
        /// Whether counties should be displayed in the network map
        /// </summary>
        [Category("Display Options"), Description("Whether districts should be displayed in the network map"), DefaultValue(false)]
        public bool DisplayDistricts
        {
            get { return _DisplayDistricts; }
            set { _DisplayDistricts = value; }
        }

        /// <summary>
        /// Whether counties should be displayed in the network map
        /// </summary>
        [Category("Display Options"), Description("Whether the state border should be displayed in the network map"), DefaultValue(true)]
        public bool DisplayStateBorder
        {
            get { return _DisplayStateBorder; }
            set { _DisplayStateBorder = value; }
        }

        /// <summary>
        /// The zoom level at which the substation names become visible
        /// </summary>
        [Category("Zoom Levels"), Description("The zoom level at which the substation names become visible")]
        public int StationNames
        {
            get { return _StationNames; }
            set
            {
                _StationNames = value;
            }
        }

        /// <summary>
        /// The zoom level at which the substation MWs (generation and/or load) become visible
        /// </summary>
        [Category("Zoom Levels"), Description("The zoom level at which the substation MWs (generation and/or load) become visible")]
        public int StationMW
        {
            get { return _StationMW; }
            set
            {
                _StationMW = value;
            }
        }

        /// <summary>
        /// The zoom level at which the line flow graphics become visible
        /// </summary>
        [Category("Zoom Levels"), Description("The zoom level at which the line flow graphics become visible")]
        public int LineFlows
        {
            get { return _LineFlows; }
            set { _LineFlows = value; }
        }
        /// <summary>
        /// Var flow arrows visible
        /// </summary>
        [Category("Lines"), Description("Var flow arrows")]
        public bool VarFlows { get; set; } 

        [Category("Tie Lines"), Description("Enhanced visiblity on tie flows.")]
        public bool TieFlowDirectionVisible { get; set; } 

        [Category("Tie Lines"), Description("Enhanced color for importing tie lines.")]
        public System.Drawing.Color TieFlowImportColor { get; set; }

        [Category("Tie Lines"), Description("Enhanced color for exporting tie lines.")]
        public System.Drawing.Color TieFlowExportColor { get; set; }

        /// <summary>
        /// The zoom level at which the line flow text becomes visible
        /// </summary>
        [Category("Zoom Levels"), Description("The zoom level at which the line flow text becomes visible")]
        public int LineText
        {
            get { return _LineText; }
            set { _LineText = value; }
        }

        /// <summary>
        /// The MVA value at which an element is seen as energized
        /// </summary>
        [Category("Display"), Description("The MVA value at which an element is seen as energized")]
        public float EnergizationThreshold
        {
            get { return _EnergizationThreshold; }
            set { _EnergizationThreshold = value; }
        }

        /// <summary>
        /// The blackstart mode of the main map
        /// </summary>
        [Category("Display"), Description("The blackstart mode of the main map")]
        public enumBlackstartMode BlackstartMode
        {
            get { return _BlackstartMode; }
            set { _BlackstartMode = value; }
        }

        /// <summary>
        /// The percentage (0-100) of full color a color is dimmed for non-blackstart elements.
        /// </summary>
        [Category("Display"), Description("The percentage (0-100) of full color a color is dimmed for non-blackstart elements."), DefaultValue(20)]
        public int BlackstartDimmingFactor
        {
            get { return _BlackstartDimmingFactor; }
            set { _BlackstartDimmingFactor = value; }
        }

        /// <summary>
        /// Return a blackstart dimming color
        /// </summary>
        /// <param name="InColor"></param>
        /// <returns></returns>
        public Color BlackstartDim(Color InColor)
        {
            //return Color.FromArgb(BlackstartDimmingFactor , InColor);
            return Color.FromArgb((InColor.R * BlackstartDimmingFactor) / 100, (InColor.G * BlackstartDimmingFactor) / 100, (InColor.B * BlackstartDimmingFactor) / 100);
        }

        /// <summary>
        /// Return a blackstart dimming color
        /// </summary>
        /// <param name="InColor"></param>
        /// <returns></returns>
        public RawColor4 BlackstartDim(RawColor4 InColor)
        {
            //return Color.FromArgb(BlackstartDimmingFactor , InColor);
            return new RawColor4((InColor.R * BlackstartDimmingFactor) / 100, (InColor.G * BlackstartDimmingFactor) / 100, (InColor.B * BlackstartDimmingFactor) / 100, InColor.A);
        }

        /// <summary>
        /// The background color for the map
        /// </summary>
        [Category("Display"), Description("The background color for the map")]
        public Color BackgroundColor
        {
            get { return _BackgroundColor; }
            set { _BackgroundColor = value; }
        }

        /// <summary>
        /// The foreground color for the map
        /// </summary>
        [Category("Display"), Description("The foreground color for the map")]
        public Color ForegroundColor
        {
            get { return _ForegroundColor; }
            set { _ForegroundColor = value; }
        }

        /// <summary>
        /// The Warning color for the map
        /// </summary>
        [Category("Display"), Description("The Warning color for the map")]
        public Color WarningColor
        {
            get { return _WarningColor; }
            set { _WarningColor = value; }
        }

        /// <summary>
        /// The Warning background color for the map
        /// </summary>
        [Category("Display"), Description("The Warning background color for the map")]
        public Color WarningBackground
        {
            get { return _WarningBackground; }
            set { _WarningBackground = value; }
        }

        /// <summary>
        /// The Error color for the map
        /// </summary>
        [Category("Display"), Description("The Error color for the map")]
        public Color ErrorColor
        {
            get { return _ErrorColor; }
            set { _ErrorColor = value; }
        }

        /// <summary>
        /// The Error background color for the map
        /// </summary>
        [Category("Display"), Description("The Error background color for the map")]
        public Color ErrorBackground
        {
            get { return _ErrorBackground; }
            set { _ErrorBackground = value; }
        }

        /// <summary>
        /// Whether to show node violations on other one-line equipment
        /// </summary>
        [Category("Display"), Description("Whether to show node violations on other one-line equipment")]
        public bool NodeViolationsOnEquipment { get; set; }

        /// <summary>
        /// Whether to show violations in a one-line, on the lines between element and node
        /// </summary>
        [Category("Display"), Description("Whether to show violations in a one-line, on the lines between element and node")]
        public bool NodeToElementViolations { get; set; }

        /// <summary>
        /// The Normal color for the map
        /// </summary>
        [Category("Display"), Description("The Normal color for the map")]
        public Color NormalColor
        {
            get { return _NormalColor; }
            set { _NormalColor = value; }
        }

        /// <summary>
        /// The Normal background color for the map
        /// </summary>
        [Category("Display"), Description("The Normal background color for the map")]
        public Color NormalBackground
        {
            get { return _NormalBackground; }
            set { _NormalBackground = value; }
        }

        /// <summary>
        /// The color to be used for above-average values
        /// </summary>
        [Category("Display"), Description("The color to be used for above-average values")]
        public Color HighValue
        {
            get { return _HighValue; }
            set { _HighValue = value; }
        }

        /// <summary>
        /// Our map transparency level
        /// </summary>
        [Category("Pan Speed"), Description("The speed at which st holding middle mouse and dragging pans the map.")]
        public float PanSpeed { get; set; } 

        /// <summary>
        /// Our map transparency level
        /// </summary>
        [Category("Map Overlay"), Description("The transparency level of the map")]
        public float MapTransparency
        {
            get { return _MapTransparency; }
            set
            {
                _MapTransparency = value;
                if (MapTransparencyChanged != null)
                    MapTransparencyChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>The size of our map tile</summary>
        public Size MapTileSize
        {
            get { return _MapTileSize; }
            set { _MapTileSize = value; }
        }

        /// <summary>
        /// The color to be used for below-average values
        /// </summary>
        [Category("Display"), Description("The color to be used for below-average values")]
        public Color LowValue
        {
            get { return _LowValue; }
            set { _LowValue = value; }
        }

        /// <summary>Whether map tiles should be displayed</summary>
        [Category("Map Overlay"), Description("Whether map tiles should be displayed")]
        public MM_MapTile.enumMapType MapTiles
        {
            get { return _MapTiles; }
            set { _MapTiles = value; }
        }

        /// <summary>The path where map tiles should be cached (leave blank for no cacheing)</summary>
        [Category("Map Overlay"), Description("The path where map tiles should be cached (leave blank for no cacheing, e.g. Google TOS)")]
        public string MapTileCache
        {
            get
            {
                if (_MapTileCacheLocation == null)
                    _MapTileCacheLocation = Path.Combine(Application.StartupPath, "MapTileCache");
                return _MapTileCacheLocation;
            }
            set
            {
                try
                {
                    if (Directory.Exists(value))
                        _MapTileCacheLocation = value;
                }
                catch
                {
                    _MapTileCacheLocation = Path.Combine(Application.StartupPath, "MapTileCache");
                }
            }
        }

        /// <summary>The company to display</summary>
        [Category("Display"), Description("The company to display on the main map")]
        public MM_Company DisplayCompany
        {
            get { return _DisplayCompany; }
            set { _DisplayCompany = value; }
        }
        /// <summary>Show Loaded Flowgates on Lines</summary>
        [Category("Lines"), Description("Show Loaded Flowgates on Lines")]
        public bool ShowFlowgates { get; set; }

        [Category("Map Overlay"), Description("Show Weather Layer")]
        public bool ShowWeather { get; set; }

        #endregion

        public void Reset()
        {
            MM_Display resetDisplay = new MM_Display();
            resetDisplay.CopyTo(this);
        }

        public MM_Display()
        {
            ContourData= ContourOverlayDataTypes.None;
            ContourRefreshTime = 30;
            TargetFPS=60;
            ClipContours=true;
            TieFlowImportColor=Color.DarkGreen;
            TieFlowExportColor=Color.Blue;
            TieFlowDirectionVisible=true;
            VarFlows=true;
            PanSpeed=0.3f;
            ShowFlowgates=true;
            ShowLineFlows=true;
            ShowLegend=false;
            ShowLines = true;
        }

        private static XmlElement GetElement(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return doc.DocumentElement;
        }
        #region Initialization
        /// <summary>
        /// Load the overall display parameters for the Macomber Map
        /// </summary>
        /// <param name="ElementSource">The XML source for our elements</param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Display(XmlElement ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
            ContourData = ContourOverlayDataTypes.None;
            ContourRefreshTime = 30;
            TargetFPS = 60;
            ClipContours = true;
            TieFlowImportColor = Color.DarkGreen;
            TieFlowExportColor = Color.Blue;
            TieFlowDirectionVisible = true;
            VarFlows = true;
            PanSpeed = 0.3f;
            ShowFlowgates = true;
            ShowLineFlows = true;
            ShowLegend = false;
            ShowLines = true;

        }

        /*        /// <summary>
                /// Reinitialize the overall display parameters from the Macomber Map
                /// </summary>
                /// <param name="ElementSource">The XML source for our elements</param>
                /// <param name="IntegrationLayer">Our data integration Layer</param>        
                public void ReinitializeDisplay(XmlElement ElementSource)
                {
                    base.ReadXML(ElementSource);            
                }
                */
        #endregion
    }
}
