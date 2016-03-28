using MacomberMapClient.Data_Elements.Physical;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;

namespace MacomberMapClient.User_Interfaces.NetworkMap.Proxies
{
    public class SubstationProxy
    {
        public MM_Substation Substation;
        public string DetailsText;
        public TextLayout DetailsTextLayout;
        public string NameText;
        public TextLayout NameTextLayout;
        public bool Visible;
        public RawColor4 Color;
        public RawVector2 Coordinates;
        public bool ConnectorVisible { get; set; }
        public bool BlackStartDim;
        public bool BlackStartHidden;
        public float PieRadius;
        public float PieValue;
        public PathGeometry PieGeometry;

        public bool HitTest(RawVector2 screenCoordinates, float radius)
        {
            if (float.IsNaN(Coordinates.X)) return false;

            if (Substation.HasUnits)
                radius *= 2f;

            if ((screenCoordinates.X < Coordinates.X - radius) || 
                (screenCoordinates.X > Coordinates.X + radius) || 
                (screenCoordinates.Y < Coordinates.Y - radius) || 
                (screenCoordinates.Y > Coordinates.Y + radius))
                return false;
            else
                return true;
        }
    }
}