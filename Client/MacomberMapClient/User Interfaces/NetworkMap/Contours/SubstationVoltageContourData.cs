using System.Collections.Generic;
using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.NetworkMap.DX;

namespace MacomberMapClient.User_Interfaces.NetworkMap.Contours
{
    public class SubstationVoltageContourData : ContourDataProvider
    {
        /// <summary>
        /// Instantiate a contour data provider.
        /// </summary>
        /// <param name="palette">Palette use for rendering</param>
        public SubstationVoltageContourData(LinearGradientPalette palette) : base(palette, "Average KV")
        {
            UseAverageScale = false;
            AverageValue = 1.0f;
            MaximumValue = 1.1f;
            MinimumValue = 0.90f;
        }

        /// <summary>Gets the data needed to create a contour overlay.</summary>
        /// <returns>Collection of ContourData</returns>
        public override IEnumerable<ContourData> GetData()
        {
            foreach (var substation in MM_Repository.Substations.Values)
            {
                float value = float.NaN;
                try { value = substation.Average_pU; }
                catch
                {
                    continue;
                }

                if (float.IsNaN(value) || value < .5f) continue;

                yield return new ContourData
                {
                    SourceElement = substation,
                    LngLat = substation.LngLat.ToRawVector2(),
                    Value = value,
                    Delta = GetContourPosition(value)
                };
            }
        }

        /// <summary>
        /// Updates the collection of items Value and delta fields.
        /// </summary>
        /// <param name="data">Collection of contour data.</param>
        public override void UpdateData(IEnumerable<ContourData> data)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Updates the items Value and delta fields.
        /// </summary>
        /// <param name="item">Data item to update.</param>
        public override void UpdateItem(ref ContourData item)
        {
            throw new System.NotImplementedException();
        }
    }
}