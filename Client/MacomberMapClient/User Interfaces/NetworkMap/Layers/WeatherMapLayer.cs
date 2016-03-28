using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.NetworkMap.DX;

namespace MacomberMapClient.User_Interfaces.NetworkMap.Layers
{
    public class WeatherMapLayer : RenderLayer
    {
        /// <summary>
        /// Create a new layer
        /// </summary>
        /// <param name="surface">The <see cref="Direct3DSurface"/> this layer belongs to.</param>
        /// <param name="name">The name of the layer.</param>
        /// <param name="order">The order of the layer</param>
        public WeatherMapLayer(Direct3DSurface surface) : base(surface, "Weather", 20) {}

        /// <summary>
        /// Perform calculations and update data.
        /// </summary>
        /// <param name="updateTime">Time since last update.</param>
        /// <remarks>include base.Update(renderTime); in overloads to preserve updating UpdateTime field.</remarks>
        public override void Update(RenderTime updateTime)
        {
            base.Update(updateTime);

            if (!MM_Repository.OverallDisplay.ShowWeather) return;
        }

        /// <summary>
        /// Draw the layer. 
        /// </summary>
        /// <param name="renderTime">Time since last render.</param>
        /// <remarks>include base.Render(renderTime); in overloads to preserve updating RenderTime field.</remarks>
        public override void Render(RenderTime renderTime)
        {
            base.Render(renderTime);

            if (!MM_Repository.OverallDisplay.ShowWeather) return;
        }
    }
}
