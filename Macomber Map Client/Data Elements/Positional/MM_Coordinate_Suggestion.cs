using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Macomber_Map.Data_Elements.Positional
{
    /// <summary>
    /// This class holds information on a coordinate suggestion
    /// </summary>
    public class MM_Coordinate_Suggestion
    {
        #region Variable Declarations
        /// <summary>The base element associated with our suggestion</summary>
        public MM_Element BaseElement;

        /// <summary>The coordinates originally associated with our content</summary>
        public PointF[] OriginalCoordinates;

        /// <summary>The collection of suggested coordinates</summary>
        public PointF[] SuggestedCoordinates;

        /// <summary>The collection of X/Y coordinates</summary>
        public Point[] SuggestedCoordinatesXY;

        /// <summary>The index of our line that's being handled</summary>
        public int LineIndex;
        #endregion

        #region Initializtaion
        /// <summary>
        /// Initialize our coordinate suggestion
        /// </summary>
        /// <param name="BaseElement"></param>
        /// <param name="Coordinates"></param>
        public MM_Coordinate_Suggestion(MM_Element BaseElement, MM_Coordinates Coordinates)
        {
            this.BaseElement = BaseElement;
            if (BaseElement is MM_Substation)
            {
                OriginalCoordinates = new PointF[] { (BaseElement as MM_Substation).LatLong };
                SuggestedCoordinates = new PointF[] { (BaseElement as MM_Substation).LatLong };
                SuggestedCoordinatesXY = new Point[] { MM_Coordinates.LatLngToXY((BaseElement as MM_Substation).LatLong, Coordinates.ZoomLevel) };
            }
            else if (BaseElement is MM_Line)
            {
                OriginalCoordinates = (PointF[])(BaseElement as MM_Line).Coordinates.Clone();
                SuggestedCoordinates = (BaseElement as MM_Line).Coordinates = (PointF[])(BaseElement as MM_Line).Coordinates.Clone();
                List<Point> InflectionPoints = new List<Point>();
                foreach (PointF pt in SuggestedCoordinates)
                    InflectionPoints.Add(MM_Coordinates.LatLngToXY(pt, Coordinates.ZoomLevel));
                SuggestedCoordinatesXY = InflectionPoints.ToArray();
            }

        }
        #endregion


        /// <summary>
        /// Update a substation coordinates and fix all lines
        /// </summary>
        /// <param name="NewPos"></param>
        /// <returns></returns>
        public List<MM_Line> UpdateSubstationCoordinates(PointF NewPos)
        {
            (BaseElement as MM_Substation).LatLong = NewPos;
            List<MM_Line> OutLines = new List<MM_Line>();
            foreach (MM_Line Line in MM_Repository.Lines.Values)
            {
                if (Line.Substation1 == BaseElement)
                {
                    Line.Coordinates[0] = NewPos;
                    OutLines.Add(Line);
                }
                if (Line.Substation2 == BaseElement)
                {
                    Line.Coordinates[Line.Coordinates.Length - 1] = NewPos;
                    OutLines.Add(Line);
                }
            }
            return OutLines;
        }

        /// <summary>
        /// Update a line coordinates
        /// </summary>
        /// <param name="LatLng"></param>
        /// <param name="PixelPoint"></param>
        public void UpdateLineCoordinates(PointF LatLng, Point PixelPoint)
        {
            if (LineIndex >= 0 && LineIndex < SuggestedCoordinatesXY.Length)
            {
                SuggestedCoordinates[LineIndex] = (BaseElement as MM_Line).Coordinates[LineIndex] = LatLng;
                SuggestedCoordinatesXY[LineIndex] = PixelPoint;
            }
        }
    }
}
