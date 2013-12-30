using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Xml;
using Macomber_Map.Data_Connections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using Macomber_Map.Data_Elements.Display;

namespace Macomber_Map.Data_Elements
{
    /// <summary>
    /// The parameters for the overall display
    /// </summary>
    public class MM_Display : MM_Serializable
    {

        #region Varaibles
        private int _StationNames, _LineFlows, _LineText, _StationMW, _FlowInterval = 400, _MultipleLineWidthAddition = 2, _BlackstartDimmingFactor = 20, _StationZoomLevel = 12;
        private float _ContourThreshold = 5f, _ContourBrightness = 400f, _EnergizationThreshold = 0;
        private bool _DisplayCounties = true, _DisplayStateBorder = true, _FPS = false, _UseLongNames = true, _SplitBusesByVoltage = true, _GroupLinesByVoltage = true;
        private MM_Contour_Enum _Contour = MM_Contour_Enum.None;
        private SmoothingMode _SmoothingMode = SmoothingMode.None;
        private TextRenderingHint _TextRenderingHint = TextRenderingHint.SystemDefault;
        private int _TextContrast;
        private PixelOffsetMode _PixelOffsetMode = PixelOffsetMode.None;
        private InterpolationMode _InterpolationMode = InterpolationMode.Default;
        private CompositingMode _CompositingMode = CompositingMode.SourceOver;
        private CompositingQuality _CompositingQuality = CompositingQuality.Default;
        private Font _NetworkMapFont = new Font("Arial", 8);
        private Font _KeyIndicatorSimpleFont = new Font("Arial", 16F, FontStyle.Bold, GraphicsUnit.Point, (Byte)0);
        private Font _KeyIndicatorLabelFont = new Font("Arial", 16F, FontStyle.Bold, GraphicsUnit.Point, (Byte)0);
        private Font _KeyIndicatorValueFont = new Font("Arial", 16F, FontStyle.Regular, GraphicsUnit.Point, (Byte)0);
        private int _SystemValueCount = 20;


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
        /// The zoom level one wants to be at when zeroing in on a substation
        /// </summary>
        [Category("Display"), Description("The zoom level one wants to be at when zeroing in on a substation"), DefaultValue(true)]
        
        public int StationZoomLevel
        {
            get { return _StationZoomLevel; }
            set { _StationZoomLevel = value; }
        }

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

        /// <summary>
        /// Whether the long names should be used for substations (e.g., West Batesville vs. w_batesv)
        /// </summary>
        [Category("Display Options"), Description("Whether the number of frames per second should be displayed in the network map"), DefaultValue(true)]
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
                foreach (MM_KVLevel Voltage in MM_Repository.KVLevels.Values)
                    Voltage.StationNames = value;
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
                foreach (MM_KVLevel Voltage in MM_Repository.KVLevels.Values)
                    Voltage.StationMW = value;
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
        [Category("Display"), Description("The blackstart mode of the main map"), DefaultValue(20)]
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

        /// <summary>
        /// Our event handler for the map transparency changing
        /// </summary>
        public event EventHandler<EventArgs> MapTransparencyChanged;

        private float _MapTransparency = 0.5f;


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
        private Color _BackgroundColor = Color.Black, _ForegroundColor = Color.White, _WarningColor = Color.Yellow, _ErrorColor = Color.Red, _ErrorBackground = Color.Black, _WarningBackground = Color.Black, _HighValue = Color.Red, _LowValue = Color.Blue,_NormalColor = Color.LightGreen, _NormalBackground = Color.Black;

        private Size _MapTileSize = new Size(256, 256);

        private enumBlackstartMode _BlackstartMode = enumBlackstartMode.ShowAll;

        /// <summary>The company to display, if any</summary>
        private MM_Company _DisplayCompany = null;

        private MM_MapTile.enumMapType _MapTiles = MM_MapTile.enumMapType.None;

        /// <summary>Whether map tiles should be displayed</summary>
        [Category("Map Overlay"), Description("Whether map tiles should be displayed")]
        public MM_MapTile.enumMapType MapTiles
        {
            get { return _MapTiles; }
            set { _MapTiles = value; }
        }

        private string _MapTileCache = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath,"MapTileCache");

        /// <summary>The path where map tiles should be cached (leave blank for no cacheing)</summary>
        [Category("Map Overlay"), Description("The path where map tiles should be cached (leave blank for no cacheing, e.g. Google TOS)")]
        public string MapTileCache
        {
            get { return _MapTileCache; }
            set { _MapTileCache = value; }
        }



        /// <summary>The company to display</summary>
        [Category("Display"), Description("The company to display on the main map")]
        public MM_Company DisplayCompany
        {
            get { return _DisplayCompany; }
            set { _DisplayCompany = value; }
        }

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
            HideNonOperatorElements
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Load the overall display parameters for the Macomber Map
        /// </summary>
        /// <param name="ElementSource">The XML source for our elements</param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Display(XmlElement ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {

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
