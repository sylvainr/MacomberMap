using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Net;
using System.IO;
using System.Drawing.Imaging;

namespace Macomber_Map.Data_Elements.Display
{
    /// <summary>
    /// (C) 2013, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class holds a map tile that will be displayed on the main map.
    /// </summary>
    public class MM_MapTile: MM_Serializable, IDisposable
    {
        #region Variable declarations
        /// <summary>Our actual tile image</summary>
        public Bitmap Tile=null;

        /// <summary>The coordinates of our map</summary>
        public Coordinates MapCoordinates;

        /// <summary>The top-left point of our tile (in Lat/Long)</summary>
        public PointF TopLeft;

        /// <summary>The bottom-right point of our tile (in Lat/Long)</summary>
        public PointF BottomRight;

        /// <summary>The top-left point of our tile (in pixels)</summary>
        public PointF TopLeftXY;

        /// <summary>The bottom-right point of our tile (in pixels)</summary>
        public PointF BottomRightXY;

       
        /// <summary>The bitmap is saving</summary>
        public bool Saving = false;

        /// <summary>The bitmap has failed to download</summary>
        public bool Failed = false;

        
        /// <summary>The current server number to be used for download</summary>
        private static int ServerNumber;

        /// <summary>Our web client for downloading all requested tiles</summary>
        public static WebClient[] DownloadClient = null;

        #endregion

        #region Enumerations
        /// <summary>This class holds the enumerations for the map tile types that can be retrieved for the appliation</summary>
        public enum enumMapType : byte
        {
            /// <summary>Do not display map tiles</summary>
            None,

            /// <summary>MapQuest/OSM</summary>
            MapQuest_OSM,

            /// <summary>MapQuest/Satellite</summary>
            MapQuest_Sat,

            /// <summary>Open street map</summary>
            OpenStreetMap
        }
        #endregion

        #region Initiaiization
        /// <summary>
        /// Load in a new tile
        /// </summary>
        /// <param name="MapCoordinates"></param>
        public MM_MapTile(Coordinates MapCoordinates)
        {
            this.MapCoordinates = MapCoordinates;
        
            
            //Determine our tile's positioning
            this.TopLeft = MM_Coordinates.TileTopLeft(MapCoordinates.TileXY, MapCoordinates.ZoomLevel);
            this.BottomRight = MM_Coordinates.TileBottomRight(MapCoordinates.TileXY, MapCoordinates.ZoomLevel);
            this.TopLeftXY = new Point(MapCoordinates.TileXY.X * MM_Repository.OverallDisplay.MapTileSize.Width, MapCoordinates.TileXY.Y * MM_Repository.OverallDisplay.MapTileSize.Height);
            this.BottomRightXY = new Point(((MapCoordinates.TileXY.X + 1) * MM_Repository.OverallDisplay.MapTileSize.Width) - 1, ((MapCoordinates.TileXY.Y + 1) * MM_Repository.OverallDisplay.MapTileSize.Height) - 1);
            this.Saving = false;
            this.Failed = false;
        }
        #endregion

        #region Tile downloading
        /// <summary>
        /// Get the download image URL
        /// </summary>
        /// <returns></returns>
        private String GetImageURL()
        {
            //Thanks to http://developer.mapquest.com/web/products/open/map for map tile URL and development information
            ServerNumber = (ServerNumber + 1) % 4;
            if (MapCoordinates.MapType == enumMapType.MapQuest_OSM)
                return String.Format("http://otile{0}.mqcdn.com/tiles/1.0.0/map/{1}/{2}/{3}.jpg", ServerNumber + 1, MapCoordinates.ZoomLevel, MapCoordinates.TileXY.X, MapCoordinates.TileXY.Y);
            else if (MapCoordinates.MapType == enumMapType.MapQuest_Sat)
                return String.Format("http://otile{0}.mqcdn.com/tiles/1.0.0/sat/{1}/{2}/{3}.jpg", ServerNumber + 1, MapCoordinates.ZoomLevel, MapCoordinates.TileXY.X, MapCoordinates.TileXY.Y);
            else if (MapCoordinates.MapType == enumMapType.OpenStreetMap)
                return String.Format("http://{0}.tile.openstreetmap.org/{1}/{2}/{3}.png", "abca"[ServerNumber], MapCoordinates.ZoomLevel, MapCoordinates.TileXY.X, MapCoordinates.TileXY.Y);
            else
                throw new InvalidOperationException("Unknown map type source: " + MapCoordinates.MapType.ToString());
        }

        /// <summary>
        /// Converts tile XY coordinates into a QuadKey at a specified level of detail.
        /// (from http://msdn.microsoft.com/en-us/library/bb259689.aspx)
        /// </summary>
        /// <param name="tileX">Tile X coordinate.</param>
        /// <param name="tileY">Tile Y coordinate.</param>
        /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
        /// to 23 (highest detail).</param>
        /// <returns>A string containing the QuadKey.</returns>
        private string TileXYToQuadKey(int tileX, int tileY, int levelOfDetail)
        {
            StringBuilder quadKey = new StringBuilder();
            for (int i = levelOfDetail; i > 0; i--)
            {
                char digit = '0';
                int mask = 1 << (i - 1);
                if ((tileX & mask) != 0)
                    digit++;
                if ((tileY & mask) != 0)
                    digit += (char)2;
                quadKey.Append(digit);
            }
            return quadKey.ToString();
        }

        /// <summary>
        /// Download our tile
        /// </summary>
        public void DownloadTile()
        {
            //First, make sure our downloader is set up
            if (DownloadClient == null)
            {
                DownloadClient = new WebClient[5];
                for (int a = 0; a < DownloadClient.Length; a++)
                    (DownloadClient[a] = new WebClient()).DownloadDataCompleted += new DownloadDataCompletedEventHandler(MM_MapTile_DownloadDataCompleted);
            }

            //Now, check to see whether our tile is cached
            if (!String.IsNullOrEmpty(MM_Repository.OverallDisplay.MapTileCache))
            {
                if (!Directory.Exists(MM_Repository.OverallDisplay.MapTileCache))
                    Directory.CreateDirectory(MM_Repository.OverallDisplay.MapTileCache);
                if (File.Exists(Path.Combine(MM_Repository.OverallDisplay.MapTileCache, this.MapCoordinates.ToString() + ".jpg")))
                    this.Tile = (Bitmap)Bitmap.FromFile(Path.Combine(MM_Repository.OverallDisplay.MapTileCache, this.MapCoordinates.ToString() + ".jpg"));
            }

            if (this.Tile == null)
            foreach (WebClient wClient in DownloadClient)
                if (wClient.IsBusy == false)
                {
                    Saving = true;
                    wClient.DownloadDataAsync(new Uri(GetImageURL()), this);
                    return;
                }
        }

        /// <summary>
        /// Handle the completion of our map tile download
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MM_MapTile_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            try
            {
                MM_MapTile Sender = (MM_MapTile)e.UserState;
                if (e.Cancelled || e.Error != null)
                    Sender.Failed = true;
                else
                {
                    byte[] InBytes = e.Result;
                    using (MemoryStream mS = new MemoryStream(InBytes))
                        Sender.Tile = (Bitmap)Bitmap.FromStream(mS);

                    //If we're cacheing tiles, save it
                    if (!String.IsNullOrEmpty(MM_Repository.OverallDisplay.MapTileCache))
                        File.WriteAllBytes(Path.Combine(MM_Repository.OverallDisplay.MapTileCache, Sender.MapCoordinates.ToString() + ".jpg"), InBytes);
                }
                Sender.Saving = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving map tile: " + ex.ToString());
            }
        }
        #endregion


        /// <summary>
        /// If we have a tile, dispose of it.
        /// </summary>
        public void Dispose()
        {
            if (Tile != null)
                Tile.Dispose();
        }

        /// <summary>
        /// Report our tile name
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return MapCoordinates.ToString();
        }


        #region Coordinates handling
        /// <summary>
        /// This class holds our coordinates
        /// </summary>
        public class Coordinates : IEquatable<Coordinates>
        {
            #region Variable declarations
            /// <summary>The X/Y coordinates of our tile</summary>
            public Point TileXY;

            /// <summary>The zoom level of our map tile</summary>
            public int ZoomLevel;

            /// <summary>The map type of the tile</summary>
            public enumMapType MapType;
            #endregion

            #region Initialization
            /// <summary>
            /// Initialize a new set of coordinates
            /// </summary>
            /// <param name="MapType"></param>
            /// <param name="TileXY"></param>
            /// <param name="ZoomLevel"></param>
            public Coordinates(enumMapType MapType, Point TileXY, int ZoomLevel)
            {
                this.MapType = MapType;
                this.TileXY = TileXY;
                this.ZoomLevel = ZoomLevel;
            }
            #endregion

            #region Comparisons
            /// <summary>Determine whether two coordinates are equal</summary>
            /// <param name="other"></param>
            /// <returns></returns>
            public bool Equals(Coordinates other)
            {
                return MapType == other.MapType && TileXY.X == other.TileXY.X && TileXY.Y == other.TileXY.Y && ZoomLevel == other.ZoomLevel;
            }

            /// <summary>
            /// Return a hash code for our map tile
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                return MapType.GetHashCode() + TileXY.GetHashCode() + ZoomLevel.GetHashCode();
            }
            #endregion

            /// <summary>            
            /// Report an easy to read string for our object
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return MapType.ToString() + "," + ZoomLevel.ToString() + "," + TileXY.X + "," + TileXY.Y;
            }
        }

        #endregion

    }
}
