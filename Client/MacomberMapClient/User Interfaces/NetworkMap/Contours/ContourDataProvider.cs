using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using MacomberMapClient.Data_Elements.Physical;
using SharpDX;
using SharpDX.Mathematics.Interop;

namespace MacomberMapClient.User_Interfaces.NetworkMap.Contours
{

    public class ContourData
    {
        public MM_Element SourceElement;
        public RawVector2 LngLat;
        public RawVector2 XY;
        public float Value;
        public float Delta;
    }

    /// <summary>
    /// Provides data for the contour layer.
    /// </summary>
    public abstract class ContourDataProvider
    {
        #region Static Members
        /// <summary>
        /// The the relative scaled distance from an average to range bounds.
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="min">Min value</param>
        /// <param name="max">Max value</param>
        /// <param name="avg">Average value</param>
        /// <returns>0 to 1f, position along range, where 0.5f is equal to average.</returns>
        public static float DistanceFromAverage(float value, float min, float max, float avg)
        {
            if (min > max)
            {
                float realMax = min;
                min = max;
                max = realMax;
            }

            float delta = 0;
            if (value > avg)
                delta = (value - avg) / (max - avg);
            else if (value < avg)
                delta = -(avg - value) / (avg - min);
            else
                delta = 0;

            return (delta + 1f) / 2f;
        }

        /// <summary>
        /// The the absolute scaled position of a value between two numbers.
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="min">Min value</param>
        /// <param name="max">Max value</param>
        /// <returns>0-1f, position along range.</returns>
        public static float AbsoluteDistance(float value, float min, float max)
        {
            if (min > max)
            {
                float realMax = min;
                min = max;
                max = realMax;
            }

            return (value - min) / (max - min);
        }

        #endregion

        /// <summary>
        /// Instantiate a contour data provider.
        /// </summary>
        /// <param name="palette">Palette use for rendering</param>
        /// <param name="name">Name of the contour</param>
        protected ContourDataProvider(LinearGradientPalette palette, string name)
        {
            Palette = palette;
            Name = name;
            UseAverageScale=true;
            MinimumValue=0.0f;
            MaximumValue=1.0f;
            AverageValue=0.5f;
            UseAverageScale = true;
        }

        /// <summary>
        /// Name of this contour.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// If true, scale values from average. Otherwise use absolute scale.
        /// </summary>
        public bool UseAverageScale { get; set; } 

        /// <summary>Minimum value for data.</summary>
        public float MinimumValue { get; protected set; } 

        /// <summary>Maximum value for data.</summary>
        public float MaximumValue { get; protected set; } 

        /// <summary>Average value for data.</summary>
        public float AverageValue { get; protected set; } 

        /// <summary>Number of indexed colors in palette.</summary>
        public int PaletteSize { get { return Palette != null ? Palette.PaletteSize: 0;}}

        /// <summary>
        /// The palette used to render this contour.
        /// </summary>
        public LinearGradientPalette Palette { get; set; }

        /// <summary>Gets the data needed to create a contour overlay.</summary>
        /// <returns>Collection of ContourData</returns>
        public abstract IEnumerable<ContourData> GetData();

        /// <summary>
        /// Updates the collection of items Value and delta fields.
        /// </summary>
        /// <param name="data">Collection of contour data.</param>
        public abstract void UpdateData(IEnumerable<ContourData> data);

        /// <summary>
        /// Updates the items Value and delta fields.
        /// </summary>
        /// <param name="item">Data item to update.</param>
        public abstract void UpdateItem(ref ContourData item);

        /// <summary>
        /// Get the contour position from a raw value.
        /// </summary>
        /// <param name="value">Raw value.</param>
        /// <returns>0-1f, position along contour.</returns>
        public virtual float GetContourPosition(float value)
        {
            if (UseAverageScale)
                return DistanceFromAverage(value, MinimumValue, MaximumValue, AverageValue);

            return AbsoluteDistance(value, MinimumValue, MaximumValue);
        }

        /// <summary>Get the palette color index of a raw value.</summary>
        /// <param name="value">Raw value</param>
        /// <returns>Index</returns>
        public int GetContourIndex(float value)
        {
            float relativePosition = GetContourPosition(value);
            return MathUtil.Clamp((int)(relativePosition * PaletteSize), 0, PaletteSize);
        }

        /// <summary>Get the indexed color of a value</summary>
        /// <param name="value"></param>
        /// <returns>Indexed color</returns>
        public RawColor4 GetIndexedColor(float value)
        {
            if (Palette == null) return Color.Black;

            float relativePosition = GetContourPosition(value);
            int index = MathUtil.Clamp((int)(relativePosition * PaletteSize), 0, PaletteSize);
            return Palette.GetIndexedColor(index);
        }
    }
}