using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.NetworkMap.DX;

namespace MacomberMapClient.User_Interfaces.NetworkMap.Contours
{
    public class LineLoadingPercentageContourData : ContourDataProvider
    {
        /// <summary>
        /// Instantiate a contour data provider.
        /// </summary>
        /// <param name="palette">Palette use for rendering</param>
        public LineLoadingPercentageContourData(LinearGradientPalette palette) : base(palette, "LineLoadingPercentageContour")
        {
            UseAverageScale = false;
            AverageValue = 0.9f;
            MaximumValue = .98f;
            MinimumValue = .8f;
        }

        /// <summary>Gets the data needed to create a contour overlay.</summary>
        /// <returns>Collection of ContourData</returns>
        public override IEnumerable<ContourData> GetData()
        {
            var data = new List<ContourData>();
            foreach (var line in MM_Repository.Lines.Values)
            {
                float value = float.NaN;
                try
                {
                    value = line.LinePercentage;
                }
                catch
                {
                    continue;
                }

                if (float.IsNaN(value))
                    continue;
                if (value > .5 || (line.MVAFlow > 20 && value < .3))
                data.Add(new ContourData
                             {
                                 SourceElement = line,
                                 LngLat = line.CenterLngLat.ToRawVector2(),
                                 Value = value,
                                 Delta = GetContourPosition(value)
                             });
                value = Math.Abs(line.Estimated_MVA[0]) / line.NormalLimit;
                if (value > .7)
                data.Add(new ContourData
                {
                    SourceElement = line,
                    LngLat = line.Substation1.LngLat.ToRawVector2(),
                    Value = value,
                    Delta = GetContourPosition(value)
                });
                value = Math.Abs(line.Estimated_MVA[1]) / line.NormalLimit;
                if (value > .7)
                data.Add(new ContourData
                {
                    SourceElement = line,
                    LngLat = line.Substation2.LngLat.ToRawVector2(),
                    Value = value,
                    Delta = GetContourPosition(value)
                });

                if (line.KVLevel.Nominal > 200)
                    data.Add(new ContourData
                    {
                        SourceElement = line,
                        LngLat = line.CenterLngLat.ToRawVector2(),

                        Value = line.LinePercentage,
                        Delta = GetContourPosition(line.LinePercentage)
                    });
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
