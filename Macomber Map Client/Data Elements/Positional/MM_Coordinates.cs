using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Macomber_Map.Data_Connections;

namespace Macomber_Map.Data_Elements
{
    /// <summary>
    /// Store the coordinate system used by a map, corresponding to the Google Maps coordinate system
    /// </summary>
    public class MM_Coordinates: MM_Serializable
    {
        #region Variable declarations
        /// <summary>Our collection of constants to improve map rendering</summary>
        private static List<double[]> _GConstants=null;

        /// <summary>Our collection of constants to improve map rendering</summary>
        private static List<double[]> GConstants
        {
            get
            {
                if (_GConstants == null)
                {
                    _GConstants = new List<double[]>(Data_Integration.Permissions.MaxZoom);
                    double HalfTileHeight = (double)MM_Repository.OverallDisplay.MapTileSize.Height;
                    double HalfTileWidth = (double)MM_Repository.OverallDisplay.MapTileSize.Width;
                    for (int a = 0; a <= Data_Integration.Permissions.MaxZoom; a++)
                    {
                        _GConstants.Add(new double[] { HalfTileHeight / 360.0, HalfTileWidth / (2.0 * Math.PI), HalfTileHeight / 2.0, HalfTileWidth });
                        HalfTileHeight *= 2;
                        HalfTileWidth *= 2;
                    }
                }
                return _GConstants;
            }
        }

        /// <summary>
        /// A delegate for handling changes in coordates
        /// </summary>
        /// <param name="OldCenter">The old center of the coordinates</param>
        /// <param name="NewCenter">The new center of the coordinates</param>
        public delegate void PanEvent(PointF OldCenter, PointF NewCenter);
        
        /// <summary>
        /// A delegate for handling zoom events
        /// </summary>
        /// <param name="OldZoom">The previous zoom level</param>
        /// <param name="NewZoom"></param>
        public delegate void ZoomEvent(float OldZoom, float NewZoom);

        /// <summary>
        /// A delegate to fire when a lasso is being drawn
        /// </summary>
        public delegate void LassoDrawingEvent();

        /// <summary>This event is fired when our lasso is being drawn, and its end is moved.</summary>
        public event LassoDrawingEvent LassoDrawing;

        /// <summary>This event is fired when our coordinates are panned.</summary>
        public event PanEvent Panned;

        /// <summary>
        /// This event is fired when our coordinates are zoomed.
        /// </summary>
        public event ZoomEvent Zoomed;

        /// <summary>Our top-left Latitude/Longitude</summary>
        public PointF TopLeft
        {
            get { return _TopLeft; }
            private set { _TopLeft = value; }
        }

        /// <summary>Our top-left Pixel</summary>
        public Point TopLeftXY { get; private set; }

        /// <summary>Our bottom-right Latitude/Longitude</summary>
        public PointF BottomRight { get; private set; }

        /// <summary>Our bottom-right pixel</summary>
        public Point BottomRightXY { get; private set; }

        /// <summary>
        /// Report our zoom level
        /// </summary>
        public int ZoomLevel
        {
            get { return _ZoomLevel; }
            private set { _ZoomLevel = value; }
        }

        /*/// <summary>Our current zoom level</summary>
        public int ZoomLevel
        {
            get { return _ZoomLevel; }
            set
            {
                int OldZoom = _ZoomLevel;

                //First, determine the X/Y of our image center
                Point ImageCenter = LatLngToXY(this.Center, value);

                //Now, extrapolate the rest of our coordinates based on our display rectangle.
                this.TopLeftXY = new Point(ImageCenter.X - (DisplayRectangle.Width / 2), ImageCenter.Y - (DisplayRectangle.Height / 2));
                this.TopLeft = XYToLatLng(this.TopLeftXY, value);

                this.BottomRightXY = new Point(ImageCenter.X + (DisplayRectangle.Width / 2), ImageCenter.Y + (DisplayRectangle.Height / 2));
                this.BottomRight = XYToLatLng(this.BottomRightXY, value);
                _ZoomLevel = value;
                if (Zoomed != null)
                    Zoomed(OldZoom, value);
            }
        }
        */

        /// <summary>The starting point for a lasso</summary>
        private PointF _LassoStart = PointF.Empty;

        /// <summary>The ending for a lasso</summary>
        private PointF _LassoEnd;

        /// <summary>The current zoom level</summary>
        private int _ZoomLevel;

        /// <summary>The top/left coordinates</summary>
        private PointF _TopLeft;

        /// <summary>The collection of points to draw a lasso if it's not in rectangular mode</summary>
        private List<PointF> _LassoPoints = new List<PointF>();

        /// <summary>Whether the user is holding down the control key (indicating the lasso should be include all elements, not just visible ones.</summary>
        private bool _ControlDown;
       
        /// <summary>The display size of the network map</summary>
        public Rectangle DisplayRectangle;

        /// <summary>The rotation of the main map</summary>
        public float Rotation = 0f;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize the coordinate system
        /// </summary>
        /// <param name="TopLeft">The top/left latitude/longitude</param>
        /// <param name="BottomRight">The bottom/right latitude/longitude</param>
        /// <param name="ZoomLevel">The zoom level</param>
        public MM_Coordinates(PointF TopLeft, PointF BottomRight, float ZoomLevel)
        {
            this.TopLeft = TopLeft;
            this.ZoomLevel = (int)ZoomLevel;
            UpdateFromLatLng(TopLeft);
        }

        /// <summary>
        /// Initialize the coordinate system
        /// </summary>
        /// <param name="Longitude_Min">The minimim longitude</param>
        /// <param name="Latitude_Min">The minimum latitude</param>
        /// <param name="Longitude_Max">The maximum longitude</param>
        /// <param name="Latitude_Max">The maximum latitude</param>
        /// <param name="ZoomLevel">The zoom level</param>
        public MM_Coordinates(float Longitude_Min, float Latitude_Min, float Longitude_Max, float Latitude_Max, float ZoomLevel):
            this(new PointF(Longitude_Min, Latitude_Min), new PointF(Longitude_Max, Latitude_Max), ZoomLevel)
        {
        }

        /// <summary>
        /// Assign the appropriate controls to the coordinate structure
        /// </summary>      
        public void SetControls()
        {
            ZoomLevel = ZoomLevel;
        }

        #endregion

        #region Data interfaces

        /// <summary>Whether the user is holding down the control key (indicating the lasso should be include all elements, not just visible ones.</summary>        
        public bool ControlDown
        {
            get { return _ControlDown; }
            set
            {
                _ControlDown = value;
                if (LassoDrawing != null)
                    LassoDrawing();
            }
        }


        /// <summary>
        /// The end point on the lasso
        /// </summary>
        public PointF LassoEnd
        {
            get { return _LassoEnd; }
            set
            {
                _LassoEnd = value;
                if (LassoDrawing != null)
                    LassoDrawing();
            }
        }

        /// <summary>
        /// Get the lasso color
        /// </summary>
        public Color LassoColor
        {
            get { return ControlDown ? Color.DarkKhaki : Color.DarkSlateBlue; }
        }

        /// <summary>
        /// Get the lasso color
        /// </summary>
        public Brush LassoBrush
        {
            get { return ControlDown ? Brushes.DarkKhaki : Brushes.DarkSlateBlue; }
        }

        /// <summary>
        /// Get the lasso pen
        /// </summary>
        public Pen LassoPen
        {
            get { return ControlDown ? Pens.DarkKhaki : Pens.DarkSlateBlue; }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<PointF> LassoPoints
        {
            get { return _LassoPoints; }
        }

        /// <summary>
        /// Add a point to the lasso point collection
        /// </summary>
        /// <param name="LatLng"></param>
        public void AddLassoPoint(PointF LatLng)
        {
            _LassoPoints.Add(LatLng);
            if (LassoDrawing != null)
                LassoDrawing();
        }

        /// <summary>
        /// The starting point on the lasso
        /// </summary>
        public PointF LassoStart
        {
            get { return _LassoStart; }
            set
            {
                _LassoStart = value;
                LassoPoints.Clear();
            }
        }

        /// <summary>
        /// Determine whether the lasso contains multiple points.
        /// </summary>
        public bool LassoMultiplePoints
        {
            get { return LassoPoints.Count > 1; }
        }
        #endregion

        #region Data manipulation

        /// <summary>
        /// Convert a latitude/longitude to pixel based on the current zoom level
        /// </summary>
        /// <param name="LatLng">The latitude/longitude</param>
        /// <param name="ZoomLevel">Zoom level</param>
        /// <returns></returns>
        public static Point LatLngToXY(PointF LatLng, int ZoomLevel)
        {
            Point ret = Point.Empty;
            if (ZoomLevel > Data_Integration.Permissions.MaxZoom || ZoomLevel < 1)
                return ret;

            double[] ZoomParams = GConstants[ZoomLevel];
            ret.X = (int)Math.Round(ZoomParams[2] + (LatLng.X * ZoomParams[0]), MidpointRounding.AwayFromZero);

            double f = Math.Min(Math.Max(Math.Sin(LatLng.Y * (Math.PI / 180.0)), -0.9999), 0.9999);
            ret.Y = (int)Math.Round(ZoomParams[2] + (0.5 * Math.Log((1 + f) / (1 - f)) * (-ZoomParams[1])), MidpointRounding.AwayFromZero);

            return ret;
        }


        #endregion

        #region Point conversions
        /// <summary>
        /// Determine the center of our display
        /// </summary>
        public PointF Center
        {
            get { return new PointF((TopLeft.X + BottomRight.X) / 2f, (TopLeft.Y + BottomRight.Y) / 2f); }
            set
            {
                Point CenterXY = LatLngToXY(value, ZoomLevel);
                UpdateFromXY(new Point(CenterXY.X - (DisplayRectangle.Width / 2), CenterXY.Y - (DisplayRectangle.Height / 2)));
            }
        }

        /// <summary>
        /// Set our top left and bottom right coordinates
        /// </summary>
        /// <param name="TopLeft"></param>
        /// <param name="BottomRight"></param>
        public void SetTopLeftAndBottomRight(PointF TopLeft, PointF BottomRight)
        {
            //Determine the zoom levels that fit our need
            PointF Center = new PointF((TopLeft.X + BottomRight.X) / 2f, (TopLeft.Y + BottomRight.Y) / 2f);
            PointF TopLeftLatLng = PointF.Empty;
            int ZoomLevel = Data_Integration.Permissions.MaxZoom-1;
            while (ZoomLevel > 0)
            {
                Point CenterXY = LatLngToXY(Center, ZoomLevel);
                Point TopLeftXY = new Point(CenterXY.X - (DisplayRectangle.Width / 2), CenterXY.Y - (DisplayRectangle.Height / 2));
                Point BottomRightXY = new Point(CenterXY.X + (DisplayRectangle.Width / 2), CenterXY.Y + (DisplayRectangle.Height / 2));
                TopLeftLatLng = XYToLatLng(TopLeftXY, ZoomLevel);
                PointF BottomRightLatLng = XYToLatLng(BottomRightXY, ZoomLevel);
                if (TopLeftLatLng.X <= TopLeft.X && TopLeftLatLng.Y >= BottomRight.Y && BottomRightLatLng.X >= BottomRight.X && BottomRightLatLng.Y <= TopLeft.Y)
                    break;
                ZoomLevel--;
            }

            this.UpdateFromLatLngAndZoom(TopLeftLatLng, ZoomLevel);
        }

        /// <summary>
        /// Compute an inverted point
        /// </summary>
        /// <param name="LatLng"></param>
        /// <returns></returns>
        public PointF ConvertInvertedPoint(PointF LatLng)
        {

            return LatLng;
        }


        /// <summary>
        /// Update our coordinates to reflect a new top-left pixel
        /// </summary>
        /// <param name="TopLeftXY">The top-left pixel</param>
        public void UpdateFromXY(Point TopLeftXY)
        {
            PointF OldCenter = Center;

            //Update our top-left coordinates
            this.TopLeftXY = TopLeftXY;
            this.TopLeft = XYToLatLng(TopLeftXY, ZoomLevel);

            //Determine our bottom-right pixel
            this.BottomRightXY = Point.Add(TopLeftXY, DisplayRectangle.Size);
            this.BottomRight = XYToLatLng(this.BottomRightXY, ZoomLevel);

            if (this.Panned != null)
                this.Panned(OldCenter, Center);
        }

        /// <summary>
        /// Zoom the coordinates so that our center is maintained at the present zoom level
        /// </summary>
        /// <param name="ZoomLevel">The current zoom level</param>
        public void UpdateZoom(int ZoomLevel)
        {
            //Clear out our tiles
            List<Macomber_Map.Data_Elements.Display.MM_MapTile> TilesToClear = new List<Macomber_Map.Data_Elements.Display.MM_MapTile>(MM_Repository.MapTiles.Values);
            MM_Repository.MapTiles.Clear();
            foreach (Macomber_Map.Data_Elements.Display.MM_MapTile Tile in TilesToClear)
                Tile.Dispose();

            //First, determine the X/Y of our image center
            int OldZoom = _ZoomLevel;
            this._ZoomLevel = ZoomLevel;
            Point ImageCenter = LatLngToXY(this.Center, ZoomLevel);

            //Now, extrapolate the rest of our coordinates based on our display rectangle.
            this.TopLeftXY = new Point(ImageCenter.X - (DisplayRectangle.Width / 2), ImageCenter.Y - (DisplayRectangle.Height / 2));
            this.TopLeft = XYToLatLng(this.TopLeftXY, ZoomLevel);

            this.BottomRightXY = new Point(ImageCenter.X + (DisplayRectangle.Width / 2), ImageCenter.Y + (DisplayRectangle.Height / 2));
            this.BottomRight = XYToLatLng(this.BottomRightXY, ZoomLevel);

            if (Zoomed != null)
                Zoomed(OldZoom, _ZoomLevel);
        }

        /// <summary>
        /// Zoom our coordinates so a particular X/Y coordinate maintains the same Lat/Long in different zoom levels.
        /// </summary>
        /// <param name="ZoomLevel"></param>
        /// <param name="OldZoomLevel"></param>
        /// <param name="CursorPosition"></param>
        public void UpdateZoom(int ZoomLevel, int OldZoomLevel, Point CursorPosition)
        {
            //Clear out our tiles
            List<Macomber_Map.Data_Elements.Display.MM_MapTile> TilesToClear = new List<Macomber_Map.Data_Elements.Display.MM_MapTile>(MM_Repository.MapTiles.Values);
            MM_Repository.MapTiles.Clear();
            foreach (Macomber_Map.Data_Elements.Display.MM_MapTile Tile in TilesToClear)
                Tile.Dispose();

            //First, determine the X/Y of our image center
            int OldZoom = _ZoomLevel;
            this._ZoomLevel = ZoomLevel;

            Point AdjustedCursorPosition = new Point(CursorPosition.X + TopLeftXY.X, CursorPosition.Y + TopLeftXY.Y);
            PointF OldZoomPosition = XYToLatLng(AdjustedCursorPosition, OldZoomLevel);
            Point NewZoomPosition = LatLngToXY(OldZoomPosition, ZoomLevel);
            TopLeftXY = new Point(NewZoomPosition.X - CursorPosition.X, NewZoomPosition.Y - CursorPosition.Y);
            BottomRightXY = new Point(TopLeftXY.X + DisplayRectangle.Width, TopLeftXY.Y+ DisplayRectangle.Height);
            this.TopLeft = XYToLatLng(this.TopLeftXY, ZoomLevel);
            this.BottomRight = XYToLatLng(this.BottomRightXY, ZoomLevel);


            if (Zoomed != null)
                Zoomed(OldZoom, _ZoomLevel);
        }

        /// <summary>
        /// Update our coordinates to reflect a new top/left lat/long
        /// </summary>
        /// <param name="TopLeft">The top-left latitude and longitude</param>
        public void UpdateFromLatLng(PointF TopLeft)
        {
            //Determine our top-left coordinates
            this.TopLeft = TopLeft;
            this.TopLeftXY = LatLngToXY(TopLeft, ZoomLevel);

            //Determine our bottom-right pixel
            this.BottomRightXY = Point.Add(TopLeftXY, DisplayRectangle.Size);
            this.BottomRight = XYToLatLng(this.BottomRightXY, ZoomLevel);
        }

        /// <summary>
        /// Update our coordinates to reflect a new top/left lat/long and zoom level
        /// </summary>
        /// <param name="TopLeft"></param>
        /// <param name="ZoomLevel"></param>
        public void UpdateFromLatLngAndZoom(PointF TopLeft, int ZoomLevel)
        {
            //Determine our top-left coordinates
            this.TopLeft = TopLeft;
            this.TopLeftXY = LatLngToXY(TopLeft, this.ZoomLevel = ZoomLevel);

            //Determine our bottom-right pixel
            this.BottomRightXY = Point.Add(TopLeftXY, DisplayRectangle.Size);
            this.BottomRight = XYToLatLng(this.BottomRightXY, ZoomLevel);
        }

        /// <summary>
        /// Return a rectangle of our coordinates
        /// </summary>
        /// <param name="TopLeft"></param>
        /// <param name="BottomRight"></param>
        /// <returns></returns>
        public Rectangle ConvertRectangle(PointF TopLeft, PointF BottomRight)
        {
            Point TopLeftXY = ConvertPoint(TopLeft);
            Point BottomRightXY = ConvertPoint(BottomRight);
            return Rectangle.FromLTRB(Math.Min(TopLeftXY.X, BottomRightXY.X), Math.Min(TopLeftXY.Y, BottomRightXY.Y), Math.Max(TopLeftXY.X, BottomRightXY.X), Math.Max(TopLeftXY.Y, BottomRightXY.Y));
        }


        /// <summary>
        /// Convert a latitude/longitude to screen point
        /// </summary>
        /// <param name="inPoint">The point to convert</param>
        /// <returns></returns>
        public Point ConvertPoint(PointF inPoint)
        {
            Point XYPoint = MM_Coordinates.LatLngToXY(inPoint, _ZoomLevel);
            return new Point(XYPoint.X + TopLeftXY.X, XYPoint.Y + TopLeftXY.Y);
        }

         /// <summary>
        /// Convert a latitude/longitude to screen point
        /// </summary>
        /// <param name="inPoint">The point to convert</param>
        /// <param name="BottomRight"></param>
        /// <param name="TopLeft"></param>
        /// <returns></returns>
        public Point ConvertPoint(PointF inPoint, PointF TopLeft, PointF BottomRight)
        {
            return ConvertPoint(inPoint);
        }
        #endregion


        #region Tile handling
        /// <summary>
        /// Convert a pixel to tile coordinate
        /// </summary>
        /// <param name="XY"></param>
        /// <returns></returns>
        public static Point XYToTile(Point XY)
        {
            return new Point(XY.X / MM_Repository.OverallDisplay.MapTileSize.Width, XY.Y / MM_Repository.OverallDisplay.MapTileSize.Height);
        }


        /// <summary>
        /// Determine the top/left Lat/Long of a map tile at a particular zoom level
        /// </summary>
        /// <param name="Tile"></param>
        /// <param name="ZoomLevel"></param>
        /// <returns></returns>
        public static PointF TileTopLeft(Point Tile, int ZoomLevel)
        {
            return XYToLatLng(new Point(Tile.X * MM_Repository.OverallDisplay.MapTileSize.Width, Tile.Y * MM_Repository.OverallDisplay.MapTileSize.Height), ZoomLevel);
        }

        /// <summary>
        /// Determine the bottom-right point of a tile at the requested zoom level
        /// </summary>
        /// <param name="Tile">The tile to be measured</param>
        /// <param name="ZoomLevel"></param>
        /// <returns></returns>
        public static PointF TileBottomRight(Point Tile, int ZoomLevel)
        {
            return XYToLatLng(new Point(((Tile.X + 1) * MM_Repository.OverallDisplay.MapTileSize.Width) - 1, ((Tile.Y + 1) * MM_Repository.OverallDisplay.MapTileSize.Height) - 1), ZoomLevel);
        }

        /// <summary>
        /// Convert a pixel to latitude/longitude based on the current zoom level
        /// </summary>
        /// <param name="XY">The current pixel</param>
        /// <param name="ZoomLevel">The current zoom level</param>
        /// <returns></returns>
        public static PointF XYToLatLng(Point XY, int ZoomLevel)
        {
            PointF OutPoint = PointF.Empty;
            if (ZoomLevel > Data_Integration.Permissions.MaxZoom || ZoomLevel < 1)
                return OutPoint;
            double[] ZoomParams = GConstants[ZoomLevel];
            OutPoint.X = (float)(((double)XY.X - ZoomParams[2]) / ZoomParams[0]);
            OutPoint.Y = (float)((2.0 * Math.Atan(Math.Exp(((double)XY.Y - ZoomParams[2]) / (-ZoomParams[1]))) - (Math.PI / 2.0)) / (Math.PI / 180.0));
            return OutPoint;
        }

        /// <summary>
        /// Convert a screen point to lat/long
        /// </summary>
        /// <param name="XY"></param>
        /// <returns></returns>
        public PointF UnconvertPoint(Point XY)
        {            
            return MM_Coordinates.XYToLatLng(new Point(XY.X + TopLeftXY.X, XY.Y + TopLeftXY.Y), _ZoomLevel);
        }

        /// <summary>
        /// Determine if a tile is valid based on its coordinates
        /// </summary>
        /// <param name="TileCoordinates">The coordinates of the tile</param>
        /// <returns></returns>
        private bool IsValidTile(Point TileCoordinates)
        {
            return TileCoordinates.X / 256 < Math.Pow(2, ZoomLevel) && TileCoordinates.Y / 256 < Math.Pow(2, ZoomLevel);
        }

        #endregion
    }
}
