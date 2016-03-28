using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MacomberMapClient.Data_Elements.Blackstart;
using MacomberMapClient.Data_Elements.Geographic;
using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.NetworkMap.DX;
using MacomberMapClient.User_Interfaces.NetworkMap.Proxies;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;

namespace MacomberMapClient.User_Interfaces.NetworkMap.Layers
{

    public class StateRegionsLayer : RenderLayer
    {
        private BrushContainer brushes;
        public Dictionary<MM_Boundary, RegionProxy> regions = new Dictionary<MM_Boundary, RegionProxy>(10);
        private RegionProxy state;
        private int _lastZoom;

        public RegionProxy State { get { return state; } }

        /// <summary>
        /// Create a new layer
        /// </summary>
        /// <param name="surface">The <see cref="Direct3DSurface"/> this layer belongs to.</param>
        /// <param name="name">The name of the layer.</param>
        /// <param name="order">The order of the layer</param>
        public StateRegionsLayer(Direct3DSurface surface) : base(surface, "RegionBorders", 2)
        {

        }

        /// <summary>
        /// Returns a list of objects that are in the requested space.
        /// </summary>
        /// <param name="screenCoordinates">The test coordinates</param>
        /// <param name="radius">The radius to test.</param>
        /// <param name="onlyVisible">Only include elements visible on the screen.</param>
        /// <returns></returns>
        public override IEnumerable<MM_Element> HitTest(RawVector2 screenCoordinates, float radius, bool onlyVisible = true)
        {
            foreach (var proxy in regions)
            {
                if (proxy.Value.Geometry.FillContainsPoint(screenCoordinates))
                    yield return proxy.Key;
            }

            // if (MM_Repository.OverallDisplay.DisplayCounties)
            //     foreach (KeyValuePair<MM_Boundary, RegionProxy> County in Counties.ToList())
            //         if (County.Value.Visible && County.Value.Geometry.FillContainsPoint(PixelLocation))
            //             Elements.Add(County.Key);
            // if (MM_Repository.OverallDisplay.DisplayDistricts)
            //     foreach (KeyValuePair<MM_Boundary, RegionProxy> District in Districts)
            //         Elements.Add(District.Key);
        }

        /// <summary>
        /// Load Content and initialize unmanaged resources.
        /// </summary>
        public override void LoadContent()
        {
            base.LoadContent();
            brushes = new BrushContainer(Surface);
        }


        /// <summary>
        /// Unload content and cleanup unmanaged resources.
        /// </summary>
        public override void UnloadContent()
        {
            base.UnloadContent();
            brushes.Cleanup();
            brushes = null;

            var regionProxies = regions.Values.ToList();
            regions.Clear();
            
            foreach (var r in regionProxies)
            {
                if (!r.IsDisposed)
                    r.Dispose();
            }

            if (state != null && !state.IsDisposed)
            {
                state.Dispose();
                state = null;
            }
        }

        /// <summary>
        /// Perform calculations and update data.
        /// </summary>
        /// <param name="updateTime">Time since last update.</param>
        /// <remarks>include base.Update(renderTime); in overloads to preserve updating UpdateTime field.</remarks>
        public override void Update(RenderTime updateTime)
        {
            base.Update(updateTime);

            if (!Surface.IsDirty && _lastZoom == Surface.Coordinates.ZoomLevel)
                return;

            _lastZoom = Surface.Coordinates.ZoomLevel;

            foreach (var value in regions.Values)
            {
                value.Geometry.Dispose();
            }
            regions.Clear();

            foreach (var boundary in MM_Repository.Counties.Values.ToList())
            {
                RawRectangleF bounds;
                var geometry = Surface.CreateRegionPathGeometry(MM_Coordinates.ConvertPoints(boundary.Coordinates, Surface.Coordinates.ZoomLevel).ToList(), out bounds, filled: true, simplify: true);

                var region = new RegionProxy(geometry, boundary.DisplayParameter.ForeColor, boundary.DisplayParameter.Width, bounds);
                region.Boundary = boundary;
                if (boundary.Name == "STATE")
                    state = region;
                else
                    regions.Add(boundary, region);
            }

            foreach (MM_Boundary boundary in MM_Repository.Districts.Values.ToList())
            {
                RawRectangleF bounds;
                var geometry = Surface.CreateRegionPathGeometry(MM_Coordinates.ConvertPoints(boundary.Coordinates, Surface.Coordinates.ZoomLevel).ToList(), out bounds, simplify: true);
                var region = new RegionProxy(geometry, boundary.DisplayParameter.ForeColor, boundary.DisplayParameter.Width, bounds);
                regions.Add(boundary, region);
            }

        }

        /// <summary>
        /// Draw the layer. 
        /// </summary>
        /// <param name="renderTime">Time since last render.</param>
        /// <remarks>include base.Render(renderTime); in overloads to preserve updating RenderTime field.</remarks>
        public override void Render(RenderTime renderTime)
        {
            base.Render(renderTime);


            var tx = Surface.RenderTarget2D.Transform;
            Surface.RenderTarget2D.Transform = Matrix3x2.Translation(-Surface.Coordinates.TopLeftXY.X, -Surface.Coordinates.TopLeftXY.Y);

            var coordinateBounds = Surface.Coordinates.GetBounds();


            if (MM_Repository.OverallDisplay.DisplayCounties)
            {
                foreach (var dxRegion in regions)
                {
                    var region = dxRegion.Value;

                    if (coordinateBounds.Overlaps(region.Bounds))
                    {
                        Surface.RenderTarget2D.DrawGeometry(region.Geometry, brushes.GetBrush(region.Color), region.Width);
                    }
                }
            }

            if (MM_Repository.OverallDisplay.DisplayStateBorder && state != null)
            {
                if (coordinateBounds.Overlaps(state.Bounds))
                {
                    Surface.RenderTarget2D.DrawGeometry(state.Geometry, brushes.GetBrush(state.Color), state.Width);
                }
            }

            Surface.RenderTarget2D.Transform = tx;
        }



    }
}