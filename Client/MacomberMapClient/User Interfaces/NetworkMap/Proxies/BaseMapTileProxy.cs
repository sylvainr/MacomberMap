using MacomberMapClient.Data_Elements.Display;
using SharpDX.Mathematics.Interop;

namespace MacomberMapClient.User_Interfaces.NetworkMap.Proxies
{
    public class BaseMapTileProxy
    {
        //public RawVector2 TopLeft;
        //public RawVector2 BottomRight;
        //public RawVector2 TopLeftXY;
        //public RawVector2 BottomRightXY;
        public BaseMapTileProxy(TileCoordinates coords)
        {
            TileCoordinates = coords;
        }

        public RawVector2 TileXY;
        public int ZoomLevel;
        public string MapType;
        public string URL;

        public TileCoordinates TileCoordinates;
        public virtual SharpDX.Direct2D1.Bitmap Bitmap { get; set; }

        public TileState State = TileState.Empty;

        public bool IsReady
        {
            get { return State == TileState.Loaded && Bitmap != null && !Bitmap.IsDisposed; }
        }

        public string GetFileName(string extension = ".jpg")
        {
            if (TileCoordinates != null)
                return TileCoordinates.GetFileName(extension);
            else
                return null;
        }
        
        public enum TileState
        {
            Empty,
            Loading,
            Loaded,
            Failed,
            NotFound,
            Disposed
        }
    }
}