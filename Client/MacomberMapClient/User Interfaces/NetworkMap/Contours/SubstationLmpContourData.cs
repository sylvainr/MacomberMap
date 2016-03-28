using System.Collections.Generic;
using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.NetworkMap.DX;

namespace MacomberMapClient.User_Interfaces.NetworkMap.Contours
{
    public class SubstationLmpContourData : ContourDataProvider
    {
        /// <summary>
        /// Instantiate a contour data provider.
        /// </summary>
        /// <param name="palette">Palette use for rendering</param>
        public SubstationLmpContourData(LinearGradientPalette palette) : base(palette, "Average LMP")
        {
            UseAverageScale = true;
        }

        /// <summary>Gets the data needed to create a contour overlay.</summary>
        /// <returns>Collection of ContourData</returns>
        public override IEnumerable<ContourData> GetData()
        {
            float min = float.MaxValue;
            float max = float.MinValue;
            float avg = 0;
            float total = 0;
            int count = 0;

            // keep a reference here so we can recalculate the deltas when complete.
            var data = new List<ContourData>();
            foreach (var substation in MM_Repository.Substations.Values)
            {
                float value = substation.GetAverageLMP();

                if (float.IsNaN(value) || value == 0) continue;

                data.Add(new ContourData
                {
                    SourceElement = substation,
                    LngLat = substation.LngLat.ToRawVector2(),
                    Value = value,
                });

                total += value;
                if (value < min) min = value;
                if (value > max) max = value;
                count++;
                // try { data = substation.Value.Average_pU; }
                // if (Math.Abs(data) < 0.01) continue;
            }

            avg = total / count;

            AverageValue = avg;
            MaximumValue = max;
            MinimumValue = min;

            // calculate deltas
            foreach (var item in data)
            {
                item.Delta = GetContourPosition(item.Value);
            }

            return data;
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