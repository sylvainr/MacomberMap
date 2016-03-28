using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Threading;
using System.Threading.Tasks;
using MacomberMapClient.Data_Elements.Display;
using MacomberMapClient.Data_Elements.Geographic;
using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.NetworkMap.DX;
using MacomberMapClient.User_Interfaces.NetworkMap.Proxies;
using MacomberMapCommunications.Extensions;
using SharpDX.Direct2D1;
using Point = System.Drawing.Point;

namespace MacomberMapClient.User_Interfaces.NetworkMap.Layers
{
    public class WebClientDownloadState
    {
        public WebClientDownloadState(object data, WebClient client)
        {
            Data = data;
            Client = client;
        }

        public object Data { get; private set; }
        public WebClient Client { get; private set; }
    }

    public class BaseMapLayer : RenderLayer
    {
        private int zoomLevel = 0;

        private Dictionary<TileCoordinates, BaseMapTileProxy> _tiles = new Dictionary<TileCoordinates, BaseMapTileProxy>();
        private ConcurrentQueue<BaseMapTileProxy> _loadQueue = new ConcurrentQueue<BaseMapTileProxy>();
        public WebClient[] webClients = null;
        //private Queue<string> _downloadQueue = new Queue<string>();

        private Thread tileLoadThread;
        

        private static int ServerNumber = 0;
        private MM_MapTile.enumMapType _baseMapTile;

        private static string GetImageURL(TileCoordinates coords)
        {
            var dtNow = DateTime.Now.ToString();
            // notice ESRI and mapquest X/Y are reversed.
            var esriUrl = "http://services.arcgisonline.com/arcgis/rest/services/{0}/MapServer/tile/{1}/{3}/{2}.jpg";

            var mesonetUrl = "http://mesonet.agron.iastate.edu/cache/tile.py/1.0.0/nexrad-n0q-900913/";

            //Thanks to http://developer.mapquest.com/web/products/open/map for map tile URL and development information

            ServerNumber = (ServerNumber + 1) % 4;
            switch (coords.MapType)
            {
                case MM_MapTile.enumMapType.None:
                    return string.Empty;
                case MM_MapTile.enumMapType.World_Dark_Gray_Base:
                    return String.Format(esriUrl, "Canvas/World_Dark_Gray_Base", coords.ZoomLevel, coords.TileXY.X, coords.TileXY.Y);
                case MM_MapTile.enumMapType.Weather_Radar:
                    return String.Format(mesonetUrl + "/" + coords.ZoomLevel + "/" + coords.TileXY.X + "/" + coords.TileXY.Y + ".png?" + dtNow);
                case MM_MapTile.enumMapType.MapQuest_OSM:
                    return String.Format("http://otile{0}.mqcdn.com/tiles/1.0.0/map/{1}/{2}/{3}.jpg", ServerNumber + 1, coords.ZoomLevel, coords.TileXY.X, coords.TileXY.Y);
                case MM_MapTile.enumMapType.MapQuest_Sat:
                    return String.Format("http://otile{0}.mqcdn.com/tiles/1.0.0/sat/{1}/{2}/{3}.jpg", ServerNumber + 1, coords.ZoomLevel, coords.TileXY.X, coords.TileXY.Y);
                case MM_MapTile.enumMapType.OpenStreetMap:
                    return String.Format("http://{0}.tile.openstreetmap.org/{1}/{2}/{3}.png", "abca"[ServerNumber], coords.ZoomLevel, coords.TileXY.X, coords.TileXY.Y);
                default:
                    return String.Format(esriUrl, coords.MapType.ToString(), coords.ZoomLevel, coords.TileXY.X, coords.TileXY.Y);
            }
        }


        /// <summary>
        /// Create a new layer
        /// </summary>
        /// <param name="surface">The <see cref="Direct3DSurface"/> this layer belongs to.</param>
        /// <param name="name">The name of the layer.</param>
        /// <param name="order">The order of the layer</param>
        public BaseMapLayer(Direct3DSurface surface) : base(surface, "BaseMap", 1)
        {
            tileLoadThread = new Thread(ProcessLoadQueueThread);
            tileLoadThread.Start();
        }

        private bool TryGetTile(TileCoordinates key, out BaseMapTileProxy proxy)
        {
            // queue new load
            if (!_tiles.TryGetValue(key, out proxy))
            {
                var tile = new BaseMapTileProxy(key) { State = BaseMapTileProxy.TileState.Loading, ZoomLevel = zoomLevel };
                _tiles.Add(key, tile);
                _loadQueue.Enqueue(tile);
                return false;
            }

            // requeue failed
            if (proxy.State == BaseMapTileProxy.TileState.Failed || proxy.State == BaseMapTileProxy.TileState.Empty)
            {
                _loadQueue.Enqueue(proxy);
            }

            return true;
        }

        private volatile int concurrentDownloads = 0;
        private void ProcessLoadQueueThread()
        {
            while (Surface != null && !Surface.Disposing && !Surface.IsDisposed)
            {
                ParallelUtils.While(() => concurrentDownloads < 6 &&_loadQueue.Count > 0 && (Surface != null && !Surface.Disposing && !Surface.IsDisposed), ProcessItem);

                Thread.Sleep(50);
            }
        }

        private void ProcessLoadingQueue()
        {
            if (_loadQueue.Count > 0)
            // Task.Run(() =>
            {
                ParallelUtils.While(() => _loadQueue.Count > 0 && (Surface != null && !Surface.Disposing && !Surface.IsDisposed), ProcessItem);
            }//);
        }

        private void ProcessItem()
        {
            BaseMapTileProxy tile;
            if (_loadQueue.TryDequeue(out tile))
            {
                if (tile.TileCoordinates.ZoomLevel != zoomLevel || tile.TileCoordinates.MapType != MM_Repository.OverallDisplay.MapTiles)
                    return;

                bool loadedCache = false;
                if (!string.IsNullOrEmpty(MM_Repository.OverallDisplay.MapTileCache) && tile.TileCoordinates.MapType != MM_MapTile.enumMapType.Weather_Radar)
                {
                    string filename = tile.TileCoordinates.GetFileName();

                    string filePath = Path.GetDirectoryName(filename);
                    if (!Directory.Exists(filePath))
                        Directory.CreateDirectory(filePath);
                    if (File.Exists(filename))
                    {
                        loadedCache = true;
                        tile.State = BaseMapTileProxy.TileState.Loading;
                        tile.Bitmap = Surface.RenderTarget2D.LoadBitmapFromFile(filename);
                        tile.State = tile.Bitmap != null ? BaseMapTileProxy.TileState.Loaded : BaseMapTileProxy.TileState.Failed;
                    }
                }

                if (!loadedCache)
                {
                    concurrentDownloads++;
                    var uri = new Uri(GetImageURL(tile.TileCoordinates));
                    var webClient = new WebClient
                    {
                        UseDefaultCredentials = false,
                        Credentials = CredentialCache.DefaultCredentials,
                        CachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache),
                        Proxy = new WebProxy(WebRequest.DefaultWebProxy.GetProxy(uri))
                        {
                            Credentials = CredentialCache.DefaultCredentials
                        }
                    };
                    webClient.DownloadDataCompleted += TileDownloadDataCompleted;

                    tile.State = BaseMapTileProxy.TileState.Loading;
                    webClient.DownloadDataAsync(uri, new WebClientDownloadState(tile,webClient));
                }
            }
        }

        /// <summary>
        /// Handle the completion of our map tile download
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TileDownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            concurrentDownloads--;
            WebClientDownloadState state = null;
            try
            {
                state = (WebClientDownloadState)e.UserState;
                BaseMapTileProxy tile = (BaseMapTileProxy)state.Data;
                if (e.Cancelled || e.Error != null)
                {
                    var webException = e.Error as WebException;
                    if (webException != null && webException.Response is System.Net.HttpWebResponse)
                    {
                        if ((((System.Net.HttpWebResponse)webException.Response).StatusCode == HttpStatusCode.NotFound))
                        {
                            tile.State = BaseMapTileProxy.TileState.NotFound;
                        }
                    }
                    else
                    {
                        tile.State = BaseMapTileProxy.TileState.Failed;
                    }
                }
                else
                {
                    byte[] InBytes = e.Result;
                    if (!string.IsNullOrEmpty(MM_Repository.OverallDisplay.MapTileCache))
                    {
                        string filename = tile.GetFileName();
                        if (!Directory.Exists(Path.GetDirectoryName(filename)))
                            Directory.CreateDirectory(Path.GetDirectoryName(filename));
                        File.WriteAllBytes(filename, InBytes);
                        tile.Bitmap = Surface.RenderTarget2D.LoadBitmapFromFile(filename);
                        tile.State = BaseMapTileProxy.TileState.Loaded;
                    }
                    else
                    {
                        using (MemoryStream mS = new MemoryStream(InBytes))
                        {
                            var bmp = ((System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(mS));
                            tile.Bitmap = bmp.ToDxBitmap(this.Surface.RenderTarget2D);
                            bmp.Dispose();
                            tile.State = BaseMapTileProxy.TileState.Loaded;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving map tile: " + ex.Message);
            }
            finally
            {
                if (state != null && state.Client != null)
                    state.Client.Dispose();
            }
        }


        /// <summary>
        /// Unload content and cleanup unmanaged resources.
        /// </summary>
        public override void UnloadContent()
        {
            base.UnloadContent();

            CleanupTiles();
        }

        private void CleanupTiles()
        {
            var tiles = _tiles.Values.ToList();
            _tiles.Clear();
            foreach (var tile in tiles)
            {
                tile.State = BaseMapTileProxy.TileState.Disposed;
                var bmp = tile.Bitmap;
                tile.Bitmap = null;
                if (bmp != null)
                    bmp.Dispose();
                bmp = null;
            }
            tiles = null;
        }

        /// <summary>
        /// Perform calculations and update data.
        /// </summary>
        /// <param name="updateTime">Time since last update.</param>
        /// <remarks>include base.Update(renderTime); in overloads to preserve updating UpdateTime field.</remarks>
        public override void Update(RenderTime updateTime)
        {
            base.Update(updateTime);

            bool zoomChanged = zoomLevel != Surface.Coordinates.ZoomLevel;
            bool baseMapChanged = _baseMapTile != MM_Repository.OverallDisplay.MapTiles;
            zoomLevel = Surface.Coordinates.ZoomLevel;
            _baseMapTile = MM_Repository.OverallDisplay.MapTiles;


            if (baseMapChanged || zoomChanged)
            {
                CleanupTiles();
            }

            if (baseMapChanged)
            {
                Surface.AddOnscreenMessage("Base map layer changed: "+_baseMapTile.ToString(), _baseMapTile == MM_MapTile.enumMapType.None ? SharpDX.Color.Red : SharpDX.Color.Aqua);
            }
            //ProcessLoadingQueue();
        }

        /// <summary>
        /// Draw the layer. 
        /// </summary>
        /// <param name="renderTime">Time since last render.</param>
        /// <remarks>include base.Render(renderTime); in overloads to preserve updating RenderTime field.</remarks>
        public override void Render(RenderTime renderTime)
        {
            base.Render(renderTime);
            if (MM_Repository.OverallDisplay.MapTiles == MM_MapTile.enumMapType.None) return;

            var tx = Surface.RenderTarget2D.Transform;
            Surface.RenderTarget2D.Transform = SharpDX.Matrix3x2.Identity;
            BaseMapTileProxy backgroundTile;
            Point topLeftTile = MM_Coordinates.XYToTile(Surface.Coordinates.TopLeftXY);
            Point bottomRightTile = MM_Coordinates.XYToTile(Surface.Coordinates.BottomRightXY);
            Point topLeftTileShift = new Point(Surface.Coordinates.TopLeftXY.X % MM_Repository.OverallDisplay.MapTileSize.Width, Surface.Coordinates.TopLeftXY.Y % MM_Repository.OverallDisplay.MapTileSize.Height);
            int extraRadius = 4;
            SharpDX.RectangleF tileSourceRect = new SharpDX.RectangleF(0, 0, MM_Repository.OverallDisplay.MapTileSize.Width, MM_Repository.OverallDisplay.MapTileSize.Height);

            if (MM_Repository.OverallDisplay.MapTiles != MM_MapTile.enumMapType.None)
            {
                int startX = topLeftTile.X - extraRadius;
                int endX = bottomRightTile.X + extraRadius;
                int startY = topLeftTile.Y - extraRadius;
                int endY = bottomRightTile.Y + extraRadius;


                foreach (var thisTile in Spiral(topLeftTile, bottomRightTile, extraRadius))
                    {
                        TileCoordinates tileCoord = new TileCoordinates(MM_Repository.OverallDisplay.MapTiles, thisTile, (int)Surface.Coordinates.ZoomLevel);
                        if (TryGetTile(tileCoord, out backgroundTile))
                        {
                            if (!backgroundTile.IsReady) continue;

                            var targetRect = new SharpDX.RectangleF(((thisTile.X - topLeftTile.X) * MM_Repository.OverallDisplay.MapTileSize.Width) - topLeftTileShift.X, ((thisTile.Y - topLeftTile.Y) * MM_Repository.OverallDisplay.MapTileSize.Height) - topLeftTileShift.Y, MM_Repository.OverallDisplay.MapTileSize.Width, MM_Repository.OverallDisplay.MapTileSize.Height);
                            Surface.RenderTarget2D.DrawBitmap(backgroundTile.Bitmap, targetRect, MM_Repository.OverallDisplay.MapTransparency, BitmapInterpolationMode.NearestNeighbor, tileSourceRect);
                        }
                    }

                //for (Point thisTile = new Point(topLeftTile.X - extraRadius, topLeftTile.Y - extraRadius); thisTile.X <= bottomRightTile.X + extraRadius; thisTile.X++, thisTile.Y = topLeftTile.Y)
                //{
                //    for (; thisTile.Y <= bottomRightTile.Y + extraRadius; thisTile.Y++)
                //    {
                //        TileCoordinates tileCoord = new TileCoordinates(MM_Repository.OverallDisplay.MapTiles, thisTile, (int)Surface.Coordinates.ZoomLevel);
                //        if (TryGetTile(tileCoord, out backgroundTile))
                //        {
                //            if (!backgroundTile.IsReady) continue;
                //
                //            var targetRect = new SharpDX.RectangleF(((thisTile.X - topLeftTile.X) * MM_Repository.OverallDisplay.MapTileSize.Width) - topLeftTileShift.X, ((thisTile.Y - topLeftTile.Y) * MM_Repository.OverallDisplay.MapTileSize.Height) - topLeftTileShift.Y, MM_Repository.OverallDisplay.MapTileSize.Width, MM_Repository.OverallDisplay.MapTileSize.Height);
                //            Surface.RenderTarget2D.DrawBitmap(backgroundTile.Bitmap, targetRect, MM_Repository.OverallDisplay.MapTransparency, BitmapInterpolationMode.NearestNeighbor, tileSourceRect);
                //        }
                //    }
                //}
            }

            Surface.RenderTarget2D.Transform = tx;
        }

        public static IEnumerable<Point> Spiral(Point topLeft, Point bottomRight, int extend = 4)
        {
            int X = bottomRight.X - topLeft.X + (2 * extend);
            int Y = bottomRight.Y - topLeft.X + (2 * extend);

            int offsetX = (bottomRight.X + topLeft.X) / 2;
            int offsetY = (bottomRight.Y + topLeft.Y) / 2;

            int x, y, dx, dy;
            x = y = dx = 0;
            dy = -1;
            int t = Math.Max(X, Y);
            int maxI = t * t;
            for (int i = 0; i < maxI; i++)
            {
                if ((-X / 2 <= x) && (x <= X / 2) && (-Y / 2 <= y) && (y <= Y / 2))
                {
                    yield return new Point(x + offsetX, y + offsetY);
                }
                if ((x == y) || ((x < 0) && (x == -y)) || ((x > 0) && (x == 1 - y)))
                {
                    t = dx;
                    dx = -dy;
                    dy = t;
                }
                x += dx;
                y += dy;
            }
        }
    }
}