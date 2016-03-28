using MacomberMapClient.Data_Elements.Display;
using MacomberMapClient.Integration;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MacomberMapClient.User_Interfaces.NetworkMap.DX;
using SharpDX;
using SharpDX.Mathematics.Interop;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

namespace MacomberMapClient.Data_Elements.Geographic
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// Store the coordinate system used by a map, corresponding to the Google Maps coordinate system
    /// </summary>
    public class MM_Coordinates : MM_Serializable
    {
        public const double RadiansToDegreesRatio = Math.PI / 180d;
        public const double DegreesToRadiansRatio = 180d / Math.PI;
        public const double PiOver2 = Math.PI / 2.0;

        #region Variable declarations
        /// <summary>Our collection of constants to improve map rendering</summary>
        private static List<double[]> _gConstants = null;

        /// <summary>Our collection of constants to improve map rendering</summary>
        public static List<double[]> GConstants
        {
            get
            {
                if (_gConstants == null)
                {
                    _gConstants = new List<double[]>(Integration.Data_Integration.Permissions.MaxZoom);
                    double halfTileHeight = (double)MM_Repository.OverallDisplay.MapTileSize.Height;
                    double halfTileWidth = (double)MM_Repository.OverallDisplay.MapTileSize.Width;
                    for (int a = 0; a <= Integration.Data_Integration.Permissions.MaxZoom; a++)
                    {
                        _gConstants.Add(new double[]
                        {
                            halfTileHeight / 360.0,
                            halfTileWidth / (2.0 * Math.PI),
                            halfTileHeight / 2.0,
                            halfTileWidth
                        });
                        halfTileHeight *= 2;
                        halfTileWidth *= 2;
                    }
                }
                return _gConstants;
            }
        }

        /// <summary>
        /// A delegate for handling changes in coordates
        /// </summary>
        /// <param name="oldCenter">The old center of the coordinates</param>
        /// <param name="newCenter">The new center of the coordinates</param>
        public delegate void PanEvent(PointF oldCenter, PointF newCenter);

        /// <summary>
        /// A delegate for handling zoom events
        /// </summary>
        /// <param name="oldZoom">The previous zoom level</param>
        /// <param name="newZoom"></param>
        public delegate void ZoomEvent(float oldZoom, float newZoom);

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
        public PointF TopLeft { get; private set; }

        /// <summary>Our top-left Pixel</summary>
        public Point TopLeftXY { get; private set; }

        /// <summary>Our bottom-right Latitude/Longitude</summary>
        public PointF BottomRight { get; private set; }

        /// <summary>Our bottom-right pixel</summary>
        public Point BottomRightXY { get; private set; }

        /// <summary>
        /// Report our zoom level
        /// </summary>
        public int ZoomLevel { get; private set; }


        public System.Drawing.RectangleF GetViewLngLat()
        {
            return GetViewLngLat(TopLeft, BottomRight);
        }

        public static System.Drawing.RectangleF GetViewLngLat(PointF topLeft, PointF bottomRight)
        {
            return new System.Drawing.RectangleF(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
        }

        public RawRectangleF GetRawViewLngLat()
        {
            return GetRawViewLngLat(TopLeft, BottomRight);
        }

        public static RawRectangleF GetRawViewLngLat(PointF topLeft, PointF bottomRight)
        {
            return new RawRectangleF(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y );
        }

        public System.Drawing.Rectangle GetViewXY()
        {
            return new System.Drawing.Rectangle(TopLeftXY.X, TopLeftXY.Y, BottomRightXY.X - TopLeftXY.X, BottomRightXY.Y - TopLeftXY.Y);
        }

        /// <summary>
        /// Convert Lng/Lat points to Screen Coordinates
        /// </summary>
        /// <param name="lngLats">Lng/Lat Points</param>
        /// <returns></returns>
        public IEnumerable<Vector2> ConvertPoints(IEnumerable<PointF> lngLats)
        {
            return ConvertPoints(lngLats, ZoomLevel);
        }


        /// <summary>
        /// Convert Lng/Lat points to Screen Coordinates
        /// </summary>
        /// <param name="lngLats">Lng/Lat Points</param>
        /// <param name="zoomLevel">ZoomLevel</param>
        /// <returns></returns>
        public static IEnumerable<Vector2> ConvertPoints(IEnumerable<PointF> lngLats, int zoomLevel)
        {
            foreach (var lngLat in lngLats)
            {
                yield return MM_Coordinates.LngLatToScreenVector2(lngLat, zoomLevel);
            }
        }

        //public Rectangle GetViewXY()

        /*/// <summary>Our current zoom level</summary>
        public int ZoomLevel
        {
            get { return _ZoomLevel; }
            set
            {
                int OldZoom = _ZoomLevel;

                //First, determine the X/Y of our image center
                Point ImageCenter = LngLatToXY(this.Center, value);

                //Now, extrapolate the rest of our coordinates based on our display rectangle.
                this.TopLeftXY = new Point(ImageCenter.X - (DisplayRectangle.Width / 2), ImageCenter.Y - (DisplayRectangle.Height / 2));
                this.TopLeft = XYToLngLat(this.TopLeftXY, value);

                this.BottomRightXY = new Point(ImageCenter.X + (DisplayRectangle.Width / 2), ImageCenter.Y + (DisplayRectangle.Height / 2));
                this.BottomRight = XYToLngLat(this.BottomRightXY, value);
                _ZoomLevel = value;
                if (Zoomed != null)
                    Zoomed(OldZoom, value);
            }
        }
        */

        public RawRectangleF GetBounds()
        {
            return new SharpDX.RectangleF(TopLeftXY.X,
                                          TopLeftXY.Y,
                                          BottomRightXY.X - TopLeftXY.X,
                                          BottomRightXY.Y - TopLeftXY.Y);
        }

        /// <summary>The starting point for a lasso</summary>
        private PointF _lassoStart = PointF.Empty;

        /// <summary>The ending for a lasso</summary>
        private PointF _lassoEnd;

        /// <summary>The collection of points to draw a lasso if it's not in rectangular mode</summary>
        private List<PointF> _lassoPoints = new List<PointF>();

        /// <summary>Whether the user is holding down the control key (indicating the lasso should be include all elements, not just visible ones.</summary>
        private bool _controlDown;

        /// <summary>The display size of the network map</summary>
        public Rectangle DisplayRectangle;

        /// <summary>The rotation of the main map</summary>
        public float Rotation = 0f;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize the coordinate system
        /// </summary>
        /// <param name="topLeft">The top/left latitude/longitude</param>
        /// <param name="bottomRight">The bottom/right latitude/longitude</param>
        /// <param name="zoomLevel">The zoom level</param>
        public MM_Coordinates(PointF topLeft, PointF bottomRight, float zoomLevel)
        {
            this.TopLeft = topLeft;
            this.ZoomLevel = (int)zoomLevel;
            UpdateFromLngLat(topLeft);
        }

        /// <summary>
        /// Initialize the coordinate system
        /// </summary>
        /// <param name="longitudeMin">The minimim longitude</param>
        /// <param name="latitudeMin">The minimum latitude</param>
        /// <param name="longitudeMax">The maximum longitude</param>
        /// <param name="latitudeMax">The maximum latitude</param>
        /// <param name="zoomLevel">The zoom level</param>
        public MM_Coordinates(float longitudeMin, float latitudeMin, float longitudeMax, float latitudeMax, float zoomLevel) :
            this(new PointF(longitudeMin, latitudeMin), new PointF(longitudeMax, latitudeMax), zoomLevel)
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
            get { return _controlDown; }
            set
            {
                _controlDown = value;
                if (LassoDrawing != null)
                    LassoDrawing();
            }
        }


        /// <summary>
        /// The end point on the lasso
        /// </summary>
        public PointF LassoEnd
        {
            get { return _lassoEnd; }
            set
            {
                _lassoEnd = value;
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
            get { return _lassoPoints; }
        }

        /// <summary>
        /// Add a point to the lasso point collection
        /// </summary>
        /// <param name="lngLat"></param>
        public void AddLassoPoint(PointF lngLat)
        {
            _lassoPoints.Add(lngLat);
            if (LassoDrawing != null)
                LassoDrawing();
        }

        /// <summary>
        /// The starting point on the lasso
        /// </summary>
        public PointF LassoStart
        {
            get { return _lassoStart; }
            set
            {
                _lassoStart = value;
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
        /// <param name="lngLat">The latitude/longitude</param>
        /// <param name="zoomLevel">Zoom level</param>
        /// <returns></returns>
        public static Point LngLatToXY(PointF lngLat, int zoomLevel)
        {
            Point ret = Point.Empty;
            if (zoomLevel > Data_Integration.Permissions.MaxZoom || zoomLevel < 1)
                return ret;

            double[] zoomParams = GConstants[zoomLevel];
            ret.X = (int)Math.Round(zoomParams[2] + (lngLat.X * zoomParams[0]), MidpointRounding.AwayFromZero);

            double f = Math.Min(Math.Max(Math.Sin(lngLat.Y * RadiansToDegreesRatio), -0.9999), 0.9999);
            ret.Y = (int)Math.Round(zoomParams[2] + (0.5 * Math.Log((1 + f) / (1 - f)) * -zoomParams[1]), MidpointRounding.AwayFromZero);

            return ret;
        }

        /// <summary>
        /// Converts a set of GPS coordinates to screen coordinates.
        /// </summary>
        /// <param name="lngLat">GPS Coordinates.</param>
        /// <returns>Screen Coordinates</returns>
        public Vector2 LngLatToScreenVector2(PointF lngLat)
        {
            return LngLatToScreenVector2(lngLat, this.ZoomLevel);
        }

        /// <summary>
        /// Converts a set of GPS coordinates to screen coordinates.
        /// </summary>
        /// <param name="lngLat">GPS Coordinates.</param>
        /// <returns>Screen Coordinates</returns>
        public Vector2 LngLatToScreenVector2(RawVector2 lngLat)
        {
            return LngLatToScreenVector2(lngLat, this.ZoomLevel);
        }

        /// <summary>
        /// Converts a set of GPS coordinates to screen coordinates.
        /// </summary>
        /// <param name="lngLat">GPS Coordinates.</param>
        /// <param name="zoomLevel">Zoom Level.</param>
        /// <returns>Screen Coordinates</returns>
        public static RawVector2 LngLatToScreenVector2(PointF lngLat, int zoomLevel)
        {
            return LngLatToScreenVector2(lngLat.ToRawVector2(), zoomLevel);
        }

        /// <summary>
        /// Converts a set of GPS coordinates to screen coordinates.
        /// </summary>
        /// <param name="lngLat">GPS Coordinates.</param>
        /// <param name="zoomLevel">Zoom Level.</param>
        /// <returns>Screen Coordinates</returns>
        public static RawVector2 LngLatToScreenVector2(RawVector2 lngLat, int zoomLevel)
        {
            var screenCoords = new RawVector2 { X = 0, Y = 0 };
            if (zoomLevel > Data_Integration.Permissions.MaxZoom || zoomLevel < 1)
                return screenCoords;

            double[] zoomParams = GConstants[zoomLevel];
            screenCoords.X = (float)(zoomParams[2] + (lngLat.X * zoomParams[0]));

            double f = Math.Min(Math.Max(Math.Sin(lngLat.Y * RadiansToDegreesRatio), -0.9999), 0.9999);
            screenCoords.Y = (float)(zoomParams[2] + (0.5 * Math.Log((1 + f) / (1 - f)) * -zoomParams[1]));

            return screenCoords;
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
                Point centerXY = LngLatToXY(value, ZoomLevel);
                UpdateFromXY(new Point(centerXY.X - (DisplayRectangle.Width / 2), centerXY.Y - (DisplayRectangle.Height / 2)));
            }
        }

        /// <summary>
        /// Set our top left and bottom right coordinates
        /// </summary>
        /// <param name="topLeft"></param>
        /// <param name="bottomRight"></param>
        public void SetTopLeftAndBottomRight(PointF topLeft, PointF bottomRight)
        {
            //Determine the zoom levels that fit our need
            PointF center = new PointF((topLeft.X + bottomRight.X) / 2f, (topLeft.Y + bottomRight.Y) / 2f);
            PointF topLeftLngLat = PointF.Empty;
            int zoomLevel = Integration.Data_Integration.Permissions.MaxZoom - 1;
            while (zoomLevel > 0)
            {
                Point centerXY = LngLatToXY(center, zoomLevel);
                Point topLeftXY = new Point(centerXY.X - (DisplayRectangle.Width / 2), centerXY.Y - (DisplayRectangle.Height / 2));
                Point bottomRightXY = new Point(centerXY.X + (DisplayRectangle.Width / 2), centerXY.Y + (DisplayRectangle.Height / 2));
                topLeftLngLat = XYToLngLat(topLeftXY, zoomLevel);
                PointF bottomRightLngLat = XYToLngLat(bottomRightXY, zoomLevel);
                if (topLeftLngLat.X <= topLeft.X && topLeftLngLat.Y >= bottomRight.Y && bottomRightLngLat.X >= bottomRight.X && bottomRightLngLat.Y <= topLeft.Y)
                    break;
                zoomLevel--;
            }

            this.UpdateFromLngLatAndZoom(topLeftLngLat, zoomLevel);
        }

        /// <summary>
        /// Compute an inverted point
        /// </summary>
        /// <param name="lngLat"></param>
        /// <returns></returns>
        public PointF ConvertInvertedPoint(PointF lngLat)
        {

            return lngLat;
        }


        /// <summary>
        /// Update our coordinates to reflect a new top-left pixel
        /// </summary>
        /// <param name="topLeftXY">The top-left pixel</param>
        public void UpdateFromXY(Point topLeftXY)
        {
            var max = Point.Subtract(LngLatToXY(new PointF(180, -75), ZoomLevel), DisplayRectangle.Size);
            var min = LngLatToXY(new PointF(-180, 82), ZoomLevel);
            topLeftXY.X = Math.Max(Math.Min(max.X, topLeftXY.X), min.X);
            topLeftXY.Y = Math.Max(Math.Min(max.Y, topLeftXY.Y), min.Y);

            //Point.Subtract(TopLeftXY, DisplayRectangle.Size)

            PointF oldCenter = Center;

            //Update our top-left coordinates
            this.TopLeftXY = topLeftXY;
            this.TopLeft = XYToLngLat(topLeftXY, ZoomLevel);

            //Determine our bottom-right pixel
            this.BottomRightXY = Point.Add(topLeftXY, DisplayRectangle.Size);
            this.BottomRight = XYToLngLat(this.BottomRightXY, ZoomLevel);

            if (this.Panned != null)
                this.Panned(oldCenter, Center);
        }

        /// <summary>
        /// Zoom the coordinates so that our center is maintained at the present zoom level
        /// </summary>
        /// <param name="zoomLevel">The current zoom level</param>
        public void UpdateZoom(int zoomLevel)
        {
            //Clear out our tiles
            List<MM_MapTile> tilesToClear = new List<MM_MapTile>(MM_Repository.MapTiles.Values);
            MM_Repository.MapTiles.Clear();
            foreach (MM_MapTile tile in tilesToClear)
                tile.Dispose();

            //First, determine the X/Y of our image center
            int oldZoom = this.ZoomLevel;
            this.ZoomLevel = zoomLevel;


            //if (oldZoom != zoomLevel)
            // {
            //     Point imageCenter = LngLatToXY(this.Center, zoomLevel);
            //
            //     //Now, extrapolate the rest of our coordinates based on our display rectangle.
            //     this.TopLeftXY = new Point(imageCenter.X - (DisplayRectangle.Width / 2), imageCenter.Y - (DisplayRectangle.Height / 2));
            //     this.TopLeft = XYToLngLat(this.TopLeftXY, zoomLevel);
            //
            //     this.BottomRightXY = new Point(imageCenter.X + (DisplayRectangle.Width / 2), imageCenter.Y + (DisplayRectangle.Height / 2));
            //     this.BottomRight = XYToLngLat(this.BottomRightXY, zoomLevel);
            // }


            Point adjustedCursorPosition = new Point(TopLeftXY.X, TopLeftXY.Y);
            PointF oldZoomPosition = XYToLngLat(adjustedCursorPosition, oldZoom);
            Point newZoomPosition = LngLatToXY(oldZoomPosition, zoomLevel);
            TopLeftXY = new Point(newZoomPosition.X, newZoomPosition.Y);
            UpdateFromXY(TopLeftXY);
            if (Zoomed != null)
                Zoomed(oldZoom, this.ZoomLevel);
        }

        /// <summary>
        /// Zoom our coordinates so a particular X/Y coordinate maintains the same Lat/Long in different zoom levels.
        /// </summary>
        /// <param name="zoomLevel"></param>
        /// <param name="oldZoomLevel"></param>
        /// <param name="cursorPosition"></param>
        public void UpdateZoom(int zoomLevel, int oldZoomLevel, Point cursorPosition)
        {
             if (zoomLevel > 20) return;
            if (zoomLevel < 2) return;
            //If we're in training mode, abort if no zoom allowed
            if (MM_Repository.Training != null && MM_Repository.Training.TrainingMode != Training.MM_Training.enumTrainingMode.NotTraning && MM_Repository.Training.CurrentLevel != null && !MM_Repository.Training.CurrentLevel.AllowZoom)
                return;
            //Clear out our tiles
            List<MM_MapTile> tilesToClear = new List<MM_MapTile>(MM_Repository.MapTiles.Values);
            MM_Repository.MapTiles.Clear();
            foreach (MM_MapTile tile in tilesToClear)
                tile.Dispose();

            //First, determine the X/Y of our image center
            int oldZoom = this.ZoomLevel;
            this.ZoomLevel = zoomLevel;

            Point adjustedCursorPosition = new Point(cursorPosition.X + TopLeftXY.X, cursorPosition.Y + TopLeftXY.Y);
            PointF oldZoomPosition = XYToLngLat(adjustedCursorPosition, oldZoomLevel);
            Point newZoomPosition = LngLatToXY(oldZoomPosition, zoomLevel);
            TopLeftXY = new Point(newZoomPosition.X - cursorPosition.X, newZoomPosition.Y - cursorPosition.Y);
            UpdateFromXY(TopLeftXY);
            //BottomRightXY = new Point(TopLeftXY.X + DisplayRectangle.Width, TopLeftXY.Y + DisplayRectangle.Height);
            //this.TopLeft = XYToLngLat(this.TopLeftXY, ZoomLevel);
            //this.BottomRight = XYToLngLat(this.BottomRightXY, ZoomLevel);


            if (Zoomed != null)
                Zoomed(oldZoom, this.ZoomLevel);
        }

        /// <summary>
        /// Update our coordinates to reflect a new top/left lat/long
        /// </summary>
        /// <param name="topLeft">The top-left latitude and longitude</param>
        public void UpdateFromLngLat(PointF topLeft)
        {
            //Determine our top-left coordinates
            this.TopLeft = topLeft;
            this.TopLeftXY = LngLatToXY(topLeft, ZoomLevel);

            //Determine our bottom-right pixel
            this.BottomRightXY = Point.Add(TopLeftXY, DisplayRectangle.Size);
            this.BottomRight = XYToLngLat(this.BottomRightXY, ZoomLevel);
        }

        /// <summary>
        /// Update our coordinates to reflect a new top/left lat/long and zoom level
        /// </summary>
        /// <param name="topLeft"></param>
        /// <param name="zoomLevel"></param>
        public void UpdateFromLngLatAndZoom(PointF topLeft, int zoomLevel)
        {
            //Determine our top-left coordinates
            this.TopLeft = topLeft;
            this.TopLeftXY = LngLatToXY(topLeft, this.ZoomLevel = zoomLevel);

            //Determine our bottom-right pixel
            this.BottomRightXY = Point.Add(TopLeftXY, DisplayRectangle.Size);
            this.BottomRight = XYToLngLat(this.BottomRightXY, zoomLevel);
        }

        /// <summary>
        /// Return a rectangle of our coordinates
        /// </summary>
        /// <param name="topLeft"></param>
        /// <param name="bottomRight"></param>
        /// <returns></returns>
        public Rectangle ConvertRectangle(PointF topLeft, PointF bottomRight)
        {
            Point topLeftXY = ConvertPoint(topLeft);
            Point bottomRightXY = ConvertPoint(bottomRight);
            return Rectangle.FromLTRB(Math.Min(topLeftXY.X, bottomRightXY.X), Math.Min(topLeftXY.Y, bottomRightXY.Y), Math.Max(topLeftXY.X, bottomRightXY.X), Math.Max(topLeftXY.Y, bottomRightXY.Y));
        }


        /// <summary>
        /// Convert a latitude/longitude to screen point
        /// </summary>
        /// <param name="inPoint">The point to convert</param>
        /// <returns></returns>
        public Point ConvertPoint(PointF inPoint)
        {
            Point xyPoint = MM_Coordinates.LngLatToXY(inPoint, ZoomLevel);
            return new Point(xyPoint.X + TopLeftXY.X, xyPoint.Y + TopLeftXY.Y);
        }

        /// <summary>
        /// Convert a latitude/longitude to screen point
        /// </summary>
        /// <param name="inPoint">The point to convert</param>
        /// <param name="bottomRight"></param>
        /// <param name="topLeft"></param>
        /// <returns></returns>
        public Point ConvertPoint(PointF inPoint, PointF topLeft, PointF bottomRight)
        {
            return ConvertPoint(inPoint);
        }
        #endregion


        #region Tile handling
        /// <summary>
        /// Convert a pixel to tile coordinate
        /// </summary>
        /// <param name="xy"></param>
        /// <returns></returns>
        public static Point XYToTile(Point xy)
        {
            return new Point(xy.X / MM_Repository.OverallDisplay.MapTileSize.Width, xy.Y / MM_Repository.OverallDisplay.MapTileSize.Height);
        }


        /// <summary>
        /// Determine the top/left Lat/Long of a map tile at a particular zoom level
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="zoomLevel"></param>
        /// <returns></returns>
        public static PointF TileTopLeft(Point tile, int zoomLevel)
        {
            return XYToLngLat(new Point(tile.X * MM_Repository.OverallDisplay.MapTileSize.Width, tile.Y * MM_Repository.OverallDisplay.MapTileSize.Height), zoomLevel);
        }

        /// <summary>
        /// Determine the bottom-right point of a tile at the requested zoom level
        /// </summary>
        /// <param name="tile">The tile to be measured</param>
        /// <param name="zoomLevel"></param>
        /// <returns></returns>
        public static PointF TileBottomRight(Point tile, int zoomLevel)
        {
            return XYToLngLat(new Point(((tile.X + 1) * MM_Repository.OverallDisplay.MapTileSize.Width) - 1, ((tile.Y + 1) * MM_Repository.OverallDisplay.MapTileSize.Height) - 1), zoomLevel);
        }

        /// <summary>
        /// Convert a pixel to latitude/longitude based on the current zoom level
        /// </summary>
        /// <param name="xy">The current pixel</param>
        /// <param name="zoomLevel">The current zoom level</param>
        /// <returns></returns>
        public static PointF XYToLngLat(Point xy, int zoomLevel)
        {
            return XYToLngLat(new PointF(xy.X, xy.Y), zoomLevel);
        }

        /// <summary>
        /// Convert a pixel to latitude/longitude based on the current zoom level
        /// </summary>
        /// <param name="xy">The current pixel</param>
        /// <param name="zoomLevel">The current zoom level</param>
        /// <returns></returns>
        public static PointF XYToLngLat(PointF xy, int zoomLevel)
        {
            PointF outPoint = PointF.Empty;
            if (zoomLevel > Data_Integration.Permissions.MaxZoom || zoomLevel < 1)
                return outPoint;
            double[] zoomParams = GConstants[zoomLevel];
            outPoint.X = (float)((xy.X - zoomParams[2]) / zoomParams[0]);
            outPoint.Y = (float)((2.0 * Math.Atan(Math.Exp((xy.Y - zoomParams[2]) / -zoomParams[1])) - PiOver2) * DegreesToRadiansRatio);
            return outPoint;
        }

        /// <summary>
        /// Convert a screen point to lat/long
        /// </summary>
        /// <param name="xy"></param>
        /// <returns></returns>
        public PointF XYToLngLat(Point xy)
        {
            return XYToLngLat(new Point(xy.X + TopLeftXY.X, xy.Y + TopLeftXY.Y), ZoomLevel);
        }

        /// <summary>
        /// Determine if a tile is valid based on its coordinates
        /// </summary>
        /// <param name="tileCoordinates">The coordinates of the tile</param>
        /// <returns></returns>
        private bool IsValidTile(Point tileCoordinates)
        {
            return tileCoordinates.X / 256 < Math.Pow(2, ZoomLevel) && tileCoordinates.Y / 256 < Math.Pow(2, ZoomLevel);
        }

        #endregion
    }
}
