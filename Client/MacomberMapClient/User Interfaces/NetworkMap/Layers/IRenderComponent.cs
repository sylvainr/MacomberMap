using System;
using System.Collections.Generic;
using System.Diagnostics;
using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.User_Interfaces.NetworkMap.DX;
using SharpDX.Mathematics.Interop;

namespace MacomberMapClient.User_Interfaces.NetworkMap.Layers
{
    public interface IRenderComponent : IComparable<IRenderComponent>
    {
        /// <summary>
        /// Layer Order. Higher numbers are rendered on top of lower numbers.
        /// </summary>
        int Order { get; set; }

        /// <summary>
        /// The name of the layer
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Load Content and initialize unmanaged resources.
        /// </summary>
        void LoadContent();

        /// <summary>
        /// Unload content and cleanup unmanaged resources.
        /// </summary>
        void UnloadContent();

        /// <summary>
        /// Call when the presenter is reset and brushes should be recreated.
        /// </summary>
        void PresenterReset();

        /// <summary>
        /// Perform calculations and update data.
        /// </summary>
        /// <param name="updateTime">Time since last update.</param>
        void Update(RenderTime updateTime);

        /// <summary>
        /// Draw the layer.
        /// </summary>
        /// <param name="renderTime">Time since last render.</param>
        void Render(RenderTime renderTime);

        /// <summary>
        /// Returns a list of objects that are in the requested space.
        /// </summary>
        /// <param name="screenCoordinates">The test coordinates</param>
        /// <param name="radius">The radius to test.</param>
        /// <param name="onlyVisible">Only include elements visible on the screen.</param>
        /// <returns></returns>
        IEnumerable<MM_Element> HitTest(RawVector2 screenCoordinates, float radius, bool onlyVisible = true);
    }

    [DebuggerDisplay("{Name} [{Order}]")]
    public abstract class RenderLayer : IRenderComponent
    {
        /// <summary>
        /// The last UpdateTime received;
        /// </summary>
        protected RenderTime UpdateTime;

        /// <summary>
        /// The last RenderTime received;
        /// </summary>
        protected RenderTime RenderTime;

        /// <summary>
        /// The<see cref="Direct3DSurface"/> this layer belongs to.
        /// </summary>
        protected Direct3DSurface Surface;

        private bool _isLoaded;
        private bool _isUnloading;

        /// <summary>
        /// Create a new layer
        /// </summary>
        /// <param name="surface">The <see cref="Direct3DSurface"/> this layer belongs to.</param>
        /// <param name="name">The name of the layer.</param>
        /// <param name="order">The order of the layer</param>
        protected RenderLayer(Direct3DSurface surface, string name = null, int order = 0)
        {
            Surface = surface;
            Name = name;
            Order = order;
        }

        /// <summary>
        /// Layer Order. Higher numbers are rendered on top of lower numbers.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// The name of the layer
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Load Content and initialize unmanaged resources.
        /// </summary>
        public virtual void LoadContent()
        {
            _isLoaded = true;
        }

        /// <summary>
        /// Unload content and cleanup unmanaged resources.
        /// </summary>
        public virtual void UnloadContent()
        {
            _isLoaded = false;
            _isUnloading = true;
        }

        /// <summary>
        /// Call when the presenter is reset and brushes should be recreated.
        /// </summary>
        public virtual void PresenterReset()
        {
            UnloadContent();
            LoadContent();
        }
        /// Perform calculations and update data.
        /// </summary>
        /// <param name="updateTime">Time since last update.</param>
        /// <remarks>include base.Update(renderTime); in overloads to preserve updating UpdateTime field.</remarks>
        public virtual void Update(RenderTime updateTime)
        {
            UpdateTime = updateTime;
            if (!_isLoaded || _isUnloading) return;
        }

        /// <summary>
        /// Draw the layer. 
        /// </summary>
        /// <param name="renderTime">Time since last render.</param>
        /// <remarks>include base.Render(renderTime); in overloads to preserve updating RenderTime field.</remarks>
        public virtual void Render(RenderTime renderTime)
        {
            RenderTime = renderTime;
            if (!_isLoaded || _isUnloading) return;
        }

        /// <summary>
        /// Returns a list of objects that are in the requested space.
        /// </summary>
        /// <param name="screenCoordinates">The test coordinates</param>
        /// <param name="radius">The radius to test.</param>
        /// <param name="onlyVisible">Only include elements visible on the screen.</param>
        /// <returns></returns>
        public virtual IEnumerable<MM_Element> HitTest(RawVector2 screenCoordinates, float radius, bool onlyVisible = true)
        {
            yield break;
        }

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>. 
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public int CompareTo(IRenderComponent other)
        {
            if (Order > other.Order) return 1;
            if (Order == other.Order) return 0;
            return -1;
        }
    }
}