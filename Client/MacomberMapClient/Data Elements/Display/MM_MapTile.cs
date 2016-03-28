using MacomberMapClient.Data_Elements.Geographic;
using MacomberMapClient.Integration;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MacomberMapClient.User_Interfaces.NetworkMap.DX;
using SharpDX.Direct2D1;
using Bitmap = System.Drawing.Bitmap;

namespace MacomberMapClient.Data_Elements.Display
{
    /// <summary>
    /// (C) 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class holds a map tile that will be displayed on the main map.
    /// </summary>
    public class MM_MapTile : MM_Serializable, IDisposable
    {
        #region Variable declarations
        /// <summary>Our actual tile image</summary>
        public Bitmap Tile = null;

        /// <summary>The coordinates of our map</summary>
        public TileCoordinates TileCoordinates;

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

            /// <summary>ESRI Dark Gray</summary>
            World_Dark_Gray_Base,

            /// <summary>ESRI Imagery</summary>
            World_Imagery,

            /// <summary>ESRI Street Map</summary>
            World_Street_Map,

            /// <summary>ESRI Topo Map</summary>
            World_Topo_Map,

            /// <summary>ESRI Shaded Relief</summary>
            World_Shaded_Relief,

            /// <summary>MapQuest/Satellite</summary>
            MapQuest_Sat,

            /// <summary>MapQuest/OSM</summary>
            MapQuest_OSM,

            /// <summary>Open street map</summary>
            OpenStreetMap,

            /// <summary>Weather TMS</summary>
            Weather_Radar

        }
        #endregion

        #region Initiaiization
        /// <summary>
        /// Load in a new tile
        /// </summary>
        /// <param name="tileCoordinates"></param>
        public MM_MapTile(TileCoordinates tileCoordinates)
        {
            this.TileCoordinates = tileCoordinates;


            //Determine our tile's positioning
            this.TopLeft = MM_Coordinates.TileTopLeft(tileCoordinates.TileXY, tileCoordinates.ZoomLevel);
            this.BottomRight = MM_Coordinates.TileBottomRight(tileCoordinates.TileXY, tileCoordinates.ZoomLevel);
            this.TopLeftXY = new Point(tileCoordinates.TileXY.X * MM_Repository.OverallDisplay.MapTileSize.Width, tileCoordinates.TileXY.Y * MM_Repository.OverallDisplay.MapTileSize.Height);
            this.BottomRightXY = new Point(((tileCoordinates.TileXY.X + 1) * MM_Repository.OverallDisplay.MapTileSize.Width) - 1, ((tileCoordinates.TileXY.Y + 1) * MM_Repository.OverallDisplay.MapTileSize.Height) - 1);
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
            var dtNow = DateTime.Now.ToString();
            // notice ESRI and mapquest X/Y are reversed.
            var esriUrl = "http://services.arcgisonline.com/arcgis/rest/services/{0}/MapServer/tile/{1}/{3}/{2}.jpg";

            var mesonetUrl = "http://mesonet.agron.iastate.edu/cache/tile.py/1.0.0/nexrad-n0q-900913/";

            //Thanks to http://developer.mapquest.com/web/products/open/map for map tile URL and development information

            ServerNumber = (ServerNumber + 1) % 4;
            switch (TileCoordinates.MapType)
            {
                case enumMapType.None:
                    return string.Empty;
                case enumMapType.World_Dark_Gray_Base:
                    return String.Format(esriUrl, "Canvas/World_Dark_Gray_Base", TileCoordinates.ZoomLevel, TileCoordinates.TileXY.X, TileCoordinates.TileXY.Y);
                case enumMapType.Weather_Radar:
                    return String.Format(mesonetUrl+"/"+ TileCoordinates.ZoomLevel + "/" + TileCoordinates.TileXY.X +"/" + TileCoordinates.TileXY.Y + ".png?" + dtNow);
                case enumMapType.MapQuest_OSM:
                    return String.Format("http://otile{0}.mqcdn.com/tiles/1.0.0/map/{1}/{2}/{3}.jpg", ServerNumber + 1, TileCoordinates.ZoomLevel, TileCoordinates.TileXY.X, TileCoordinates.TileXY.Y);
                case enumMapType.MapQuest_Sat:
                    return String.Format("http://otile{0}.mqcdn.com/tiles/1.0.0/sat/{1}/{2}/{3}.jpg", ServerNumber + 1, TileCoordinates.ZoomLevel, TileCoordinates.TileXY.X, TileCoordinates.TileXY.Y);
                case enumMapType.OpenStreetMap:
                    return String.Format("http://{0}.tile.openstreetmap.org/{1}/{2}/{3}.png", "abca"[ServerNumber], TileCoordinates.ZoomLevel, TileCoordinates.TileXY.X, TileCoordinates.TileXY.Y);
                default:
                    return String.Format(esriUrl, TileCoordinates.MapType.ToString(), TileCoordinates.ZoomLevel, TileCoordinates.TileXY.X, TileCoordinates.TileXY.Y);
            }
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
        public Task DownloadTile()
        {
            if (DownloadClient == null)
            {
                DownloadClient = new WebClient[5];
                for (int a = 0; a < DownloadClient.Length; a++)
                {
                    DownloadClient[a] = new WebClient();
                    DownloadClient[a].DownloadDataCompleted += new DownloadDataCompletedEventHandler(MM_MapTile_DownloadDataCompleted);
                    DownloadClient[a].UseDefaultCredentials = false;
                    DownloadClient[a].Proxy = new WebProxy(WebRequest.DefaultWebProxy.GetProxy(new Uri(GetImageURL())));
                    DownloadClient[a].Proxy.Credentials = CredentialCache.DefaultCredentials;
                    DownloadClient[a].Credentials = CredentialCache.DefaultCredentials;
                }
            }

            //Now, check to see whether our tile is cached
            if (!String.IsNullOrEmpty(MM_Repository.OverallDisplay.MapTileCache))
            {

                if (!Directory.Exists(MM_Repository.OverallDisplay.MapTileCache))
                    Directory.CreateDirectory(MM_Repository.OverallDisplay.MapTileCache);
                string filename = GetFileName();
                string filePath = Path.GetDirectoryName(filename);
                if (!Directory.Exists(filePath))
                    Directory.CreateDirectory(filePath);
                if (File.Exists(filename))
                    this.Tile = ((Bitmap)Bitmap.FromFile(filename));
            }

            return Task.Run(() =>
            {
                //First, make sure our downloader is set up


                if (this.Tile == null)
                    foreach (WebClient wClient in DownloadClient)
                        if (wClient.IsBusy == false)
                        {
                            Saving = true;
                            wClient.DownloadDataAsync(new Uri(GetImageURL()), this);
                            return;
                        }
            });
        }

        private SharpDX.Direct2D1.Bitmap TileDx;
        public SharpDX.Direct2D1.Bitmap GetTileDx(RenderTarget renderTarget)
        {
            if (TileDx == null)
            {
                TileDx = this.Tile.ToDxBitmap(renderTarget);
                this.Tile.Dispose();
            }

            return TileDx;
        }

        public string GetFileName()
        {
            return Path.Combine(MM_Repository.OverallDisplay.MapTileCache,
                this.TileCoordinates.MapType.ToString(),
                this.TileCoordinates.ZoomLevel.ToString(),
                this.TileCoordinates.TileXY.Y.ToString(),
                this.TileCoordinates.TileXY.X.ToString() + ".jpg");
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
                        File.WriteAllBytes(Sender.GetFileName(), InBytes);
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

            if (TileDx != null)
                TileDx.Dispose();
        }

        /// <summary>
        /// Report our tile name
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return TileCoordinates.ToString();
        }


        #region Coordinates handling

        #endregion

    }

    /// <summary>
    /// This class holds our coordinates
    /// </summary>
    public class TileCoordinates : IEquatable<TileCoordinates>
    {
        #region Variable declarations
        /// <summary>The X/Y coordinates of our tile</summary>
        public Point TileXY;

        /// <summary>The zoom level of our map tile</summary>
        public int ZoomLevel;

        /// <summary>The map type of the tile</summary>
        public MM_MapTile.enumMapType MapType;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new set of coordinates
        /// </summary>
        /// <param name="MapType"></param>
        /// <param name="TileXY"></param>
        /// <param name="ZoomLevel"></param>
        public TileCoordinates(MM_MapTile.enumMapType MapType, Point TileXY, int ZoomLevel)
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
        public bool Equals(TileCoordinates other)
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

        public string GetFileName(string extension = ".jpg")
        {
            return Path.Combine(MM_Repository.OverallDisplay.MapTileCache,
                                MapType.ToString(),
                                ZoomLevel.ToString(),
                                TileXY.Y.ToString(),
                                TileXY.X.ToString() + extension);
        }
    }
}
