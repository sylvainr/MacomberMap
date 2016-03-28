using System;
using MacomberMapClient.Data_Elements.Display;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;

namespace MacomberMapClient.User_Interfaces.NetworkMap.Proxies
{
    public class WeatherMapTileProxy : BaseMapTileProxy
    {
        public DateTime Timestamp;

        private SharpDX.Direct2D1.Bitmap[] _bitmaps;

        public WeatherMapTileProxy(TileCoordinates coords) : base(coords)
        {
            // var mesonetUrl = "http://mesonet.agron.iastate.edu/cache/tile.py/1.0.0/nexrad-n0q-900913/";
        }

        public int Frames;

        public Bitmap this[int frame]
        {
            get { return _bitmaps[frame]; }
            set { _bitmaps[frame] = value; }
        }

        public override Bitmap Bitmap { get; set; }
    }
}