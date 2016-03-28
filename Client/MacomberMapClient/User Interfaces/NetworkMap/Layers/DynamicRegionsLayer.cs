using MacomberMapClient.User_Interfaces.NetworkMap.DX;

namespace MacomberMapClient.User_Interfaces.NetworkMap.Layers
{
    public class DynamicRegionsLayer : RenderLayer
    {
        /// <summary>
        /// Create a new layer
        /// </summary>
        /// <param name="surface">The <see cref="Direct3DSurface"/> this layer belongs to.</param>
        /// <param name="name">The name of the layer.</param>
        /// <param name="order">The order of the layer</param>
        public DynamicRegionsLayer(Direct3DSurface surface) : base(surface, "DynamicRegions", 3) { }
    }
}