using MacomberMapCommunications.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapCommunications.Messages.Display
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class holds information on the display configuration for a user
    /// </summary>
    [RetrievalCommand("LoadDisplayConfiguration")]
    public class MM_Display_Configuration
    {
        #region Enumerations
        /// <summary>The collection of contours available</summary>
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


        #region Variable declarations
        /// <summary>The extra padding over a multiple line</summary>
        [Category("Lines"), Description("The extra padding over a multiple line"), DefaultValue(2)]
        public int MultipleLineWidthAddition { get; set; }

           /// <summary>Whether lines should be grouped voltage level, and then in/out</summary>
        [Category("Display"), Description("Whether lines should be grouped voltage level, and then in/out"), DefaultValue(true)]
        public bool GroupLinesByVoltage { get; set; }

         /// <summary>The zoom level one wants to be at when zeroing in on a substation</summary>
        [Category("Display"), Description("The zoom level one wants to be at when zeroing in on a substation"), DefaultValue(12)]
        public int StationZoomLevel { get; set; }

        /// <summary>Whether buses should be split by voltage level on menu display</summary>
        [Category("Display"), Description("Whether buses should be split by voltage level"), DefaultValue(true)]
        public bool SplitBusesByVoltage { get; set; }

         /// <summary>The number of points to be displayed on the graph of system-level values</summary>
        [Category("Display"), Description("The number of points to be displayed on the graph of system-level values"), DefaultValue(20)]
        public int SystemValueCount { get; set; }

      /*  /// <summary>The font for text on the network map</summary>
        [Category("Display"), Description("The font for text on the network map"), DefaultValue(new Font("Arial", 8))]
        public Font NetworkMapFont { get; set; }

        /// <summary>The font for the label on a key indicator</summary>
        [Category("Display"), Description("The font for the label on a key indicator"), DefaultValue(new Font("Arial", 16F, FontStyle.Bold, GraphicsUnit.Point, (Byte)0))]
        public Font KeyIndicatorLabelFont { get; set; }

        /// <summary>The font for the value on a key indicator</summary>
        [Category("Display"), Description("The font for the value on a key indicator"), DefaultValue(new Font("Arial", 16F, FontStyle.Regular, GraphicsUnit.Point, (Byte)0))]
        public Font KeyIndicatorValueFont { get; set;}
        
        /// <summary>The simple font for a key indicator (used for measurements)</summary>
        [Category("Display"), Description("The simple font for a key indicator (used for measurements)"), DefaultValue(new Font("Arial", 16F, FontStyle.Bold, GraphicsUnit.Point, (Byte)0))]
        public Font KeyIndicatorSimpleFont { get; set; }
       */ 
        /// <summary>The smoothing mode</summary>
        [Category("Display"), Description("The smoothing mode"), DefaultValue(SmoothingMode.None)]
        public SmoothingMode SmoothingMode { get; set;}
        
        /// <summary>The compositing quality</summary>
        [Category("Display"), Description("The compositing quality"), DefaultValue(CompositingQuality.Default)]
        public CompositingQuality CompositingQuality { get; set;}
        
        /// <summary>The text rendering hint</summary>
        [Category("Display"), Description("The text rendering hint"), DefaultValue(TextRenderingHint.SystemDefault)]
        public TextRenderingHint TextRenderingHint { get; set;}
        
        /// <summary>The text contrast</summary>
        [Category("Display"), Description("The text contrast"), DefaultValue(0)]
        public int TextContrast { get; set;}
        
        /// <summary>The pixel offset mode</summary>
        [Category("Display"), Description("The pixel offset mode"), DefaultValue(PixelOffsetMode.None)]
        public PixelOffsetMode PixelOffsetMode { get; set;}
        
        /// <summary>The interpolation mode</summary>
        [Category("Display"), Description("The interpolation mode"), DefaultValue(InterpolationMode.Default)]
        public InterpolationMode InterpolationMode { get; set;}
        
        /// <summary>The compositing mode</summary>
        [Category("Display"), Description("The compositing mode"), DefaultValue(CompositingMode.SourceOver)]
        public CompositingMode CompositingMode { get; set; }
        
        /// <summary>The threshold (above/below mean) needed for the contour to be shown</summary>
        [Category("Contour"), Description("The threshold (above/below mean) needed for the contour to be shown"), DefaultValue(5f)]
        public float ContourThreshold { get; set;}
        
        /// <summary>The brightness for the contour</summary>
        [Category("Contour"), Description("The brightness for the contour"), DefaultValue(400f)]
        public float ContourBrightness { get; set; }
        
        /// <summary>The contour (by county) to be shown</summary>
        [Category("Contour"), Description("The contour (by county) to be shown"), DefaultValue(MM_Contour_Enum.None)]
        public MM_Contour_Enum Contour { get; set;}        

        /// <summary>The interval at which the network flow arrows</summary>
        [Category("Display Options"), Description("The interval at which the network flow arrows"), DefaultValue(400)]
        public int FlowInterval { get; set;}
        
        /// <summary>Whether the long names should be used for substations (e.g., West Batesville vs. w_batesv)</summary>
        [Category("Display Options"), Description("Whether the number of frames per second should be displayed in the network map"), DefaultValue(true)]
        public bool UseLongNames { get; set;}
        
        /// <summary>Return the name of the element</summary>
        public string Name { get; set;}
        
        /// <summary>Whether the number of frames per second should be displayed in the network map</summary>
        [Category("Display Options"), Description("Whether the number of frames per second should be displayed in the network map"), DefaultValue(false)]
        public bool FPS { get; set;}
        
        /// <summary>Whether counties should be displayed in the network map</summary>
        [Category("Display Options"), Description("Whether counties should be displayed in the network map"), DefaultValue(true)]
        public bool DisplayCounties { get; set;}
        
        /// <summary>Whether counties should be displayed in the network map</summary>
        [Category("Display Options"), Description("Whether the state border should be displayed in the network map"), DefaultValue(true)]
        public bool DisplayStateBorder { get; set; }
        
        /// <summary>The zoom level at which the substation names become visible</summary>
        [Category("Zoom Levels"), Description("The zoom level at which the substation names become visible"), DefaultValue(10)]
        public int StationNames { get; set; }
        
        /// <summary>The zoom level at which the substation MWs (generation and/or load) become visible</summary>
        [Category("Zoom Levels"), Description("The zoom level at which the substation MWs (generation and/or load) become visible"), DefaultValue(12)]
        public int StationMW { get; set;}
        
        /// <summary>The zoom level at which the line flow graphics become visible</summary>
        [Category("Zoom Levels"), Description("The zoom level at which the line flow graphics become visible"), DefaultValue(8)]
        public int LineFlows { get; set; }
        
        /// <summary>The zoom level at which the line flow text becomes visible</summary>
        [Category("Zoom Levels"), Description("The zoom level at which the line flow text becomes visible"), DefaultValue(13)]
        public int LineText { get; set; }
        
        /// <summary>The MVA value at which an element is seen as energized</summary>
        [Category("Display"), Description("The MVA value at which an element is seen as energized"), DefaultValue(0)]
        public float EnergizationThreshold { get; set; }
        
        /// <summary>The blackstart mode of the main map</summary>
        [Category("Display"), Description("The blackstart mode of the main map"), DefaultValue(enumBlackstartMode.ShowAll)]
        public enumBlackstartMode BlackstartMode { get; set; }
        
        /// <summary>The percentage (0-100) of full color a color is dimmed for non-blackstart elements.</summary>
        [Category("Display"), Description("The blackstart mode of the main map"), DefaultValue(20)]
        public int BlackstartDimmingFactor { get; set;}
  /*      
        /// <summary>The background color for the map</summary>
        [Category("Display"), Description("The background color for the map"), DefaultValue(Color.Black)]
        public Color BackgroundColor { get; set; }
        
        /// <summary>The foreground color for the map</summary>
        [Category("Display"), Description("The foreground color for the map"), DefaultValue(Color.White)]
        public Color ForegroundColor { get; set; }
        
        /// <summary>The Warning color for the map</summary>
        [Category("Display"), Description("The Warning color for the map"), DefaultValue(Color.Yellow)]
        public Color WarningColor { get; set;}
        
        /// <summary>The Warning background color for the map</summary>
        [Category("Display"), Description("The Warning background color for the map"), DefaultValue(Color.Black)]
        public Color WarningBackground { get; set;}
        
        /// <summary>The Error color for the map</summary>
        [Category("Display"), Description("The Error color for the map"), DefaultValue(Color.Red)]
        public Color ErrorColor { get; set; }
        
        /// <summary>The Error background color for the map</summary>
        [Category("Display"), Description("The Error background color for the map"), DefaultValue(Color.Black)]
        public Color ErrorBackground { get; set;}
        
        /// <summary>The Normal color for the map</summary>
        [Category("Display"), Description("The Normal color for the map"), DefaultValue(Color.LightGreen)]
        public Color NormalColor { get; set;}
        
        /// <summary>The Normal background color for the map</summary>
        [Category("Display"), Description("The Normal background color for the map"), DefaultValue(Color.Black)]
        public Color NormalBackground {get; set;}
        
        /// <summary>The color to be used for above-average values</summary>
        [Category("Display"), Description("The color to be used for above-average values"), DefaultValue(Color.Red)]
        public Color HighValue { get; set;}
        
        /// <summary>Our map transparency level</summary>
        [Category("Map Overlay"), Description("The transparency level of the map"), DefaultValue(0.5f)]
        public float MapTransparency { get; set;}
               
        /// <summary>The size of our map tile</summary>
        [Category("Map Overlay"), Description("The map tile size"), DefaultValue(new Size(256, 256))]
        public Size MapTileSize { get; set;}        

        /// <summary>The color to be used for below-average values</summary>
        [Category("Display"), Description("The color to be used for below-average values"), DefaultValue(Color.Blue)]
        public Color LowValue { get; set;}

      //  [Category("Map Overlay"), Description("Whether map tiles should be displayed"), DefaultValue(MM_MapTile.enumMapType.None)]
      //  public MM_MapTile.enumMapType MapTiles { get; set;}
    */    
        /// <summary>The path where map tiles should be cached (leave blank for no cacheing)</summary>
        [Category("Map Overlay"), Description("The path where map tiles should be cached (leave blank for no cacheing, e.g. Google TOS)"), DefaultValue(".\\MapTileCache")]
        public string MapTileCache { get; set;}        

        // <summary>The company to display</summary>
       //[Category("Display"), Description("The company to display on the main map"), DefaultValue(null)]
      //  public MM_Company DisplayCompany { get; set;}
        #endregion

        /// <summary>
        /// Return a blackstart dimming color
        /// </summary>
        /// <param name="InColor"></param>
        /// <returns></returns>
        public Color BlackstartDim(Color InColor)
        {
            return Color.FromArgb((InColor.R * BlackstartDimmingFactor) / 100, (InColor.G * BlackstartDimmingFactor) / 100, (InColor.B * BlackstartDimmingFactor) / 100);
        }
    }
}
