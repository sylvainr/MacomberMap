using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Integration;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapClient.Data_Elements.Geographic
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class holds information on a coordinate suggestion
    /// </summary>
    public class MM_Coordinate_Suggestion 
    {

        #region Variable declarations
        /// <summary>Our base line, if any</summary>
        public MM_Line BaseLine = null;

        /// <summary>
        /// Return a message that can be sent back to the MM server
        /// </summary>
        public MacomberMapCommunications.Messages.Communications.MM_Coordinate_Suggestion Message
        {
            get {  return new MacomberMapCommunications.Messages.Communications.MM_Coordinate_Suggestion(BaseSubstation==null ? BaseLine.TEID:BaseSubstation.TEID, BaseSubstation==null ? MacomberMapCommunications.Messages.Communications.MM_Coordinate_Suggestion.enumSuggestionType.Line : MacomberMapCommunications.Messages.Communications.MM_Coordinate_Suggestion.enumSuggestionType.Substation, this.OriginalCoordinates, this.SuggestedCoordinates, this.SuggestedCoordinatesXY);}
        }

        /// <summary>The index of our line that's being handled</summary>
        public int LineIndex { get; set; }

        /// <summary>Our base substation, if any</summary>
        public MM_Substation BaseSubstation = null;

        /// <summary>The coordinates originally associated with our content</summary>
        public PointF[] OriginalCoordinates { get; set; }

        /// <summary>The collection of suggested coordinates</summary>
        public PointF[] SuggestedCoordinates { get; set; }

        /// <summary>The collection of X/Y coordinates</summary>
        public Point[] SuggestedCoordinatesXY { get; set; }

        /// <summary>
        /// The bas
        /// </summary>
        public MM_Element BaseElement
        {
            set
            {
                if (value is MM_Substation)
                    BaseSubstation = (MM_Substation)value;
                else if (value is MM_Line)
                    BaseLine = (MM_Line)value;
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize our coordinate suggestion
        /// </summary>
        /// <param name="BaseElement"></param>
        /// <param name="Coordinates"></param>
        public MM_Coordinate_Suggestion(MM_Element BaseElement, MM_Coordinates Coordinates)
        {
            this.BaseElement = BaseElement;        
            if (BaseSubstation != null)
            {
                OriginalCoordinates = new PointF[] { BaseSubstation.LngLat };
                SuggestedCoordinates = new PointF[] { BaseSubstation.LngLat };
                SuggestedCoordinatesXY = new Point[] { MM_Coordinates.LngLatToXY(BaseSubstation.LngLat, Coordinates.ZoomLevel) };
            }
            else if (BaseLine != null)
            {
                OriginalCoordinates = (PointF[])BaseLine.Coordinates.ToArray();
                SuggestedCoordinates = BaseLine.Coordinates.ToArray();
                List<Point> InflectionPoints = new List<Point>();
                foreach (PointF pt in SuggestedCoordinates)
                    InflectionPoints.Add(MM_Coordinates.LngLatToXY(pt, Coordinates.ZoomLevel));
                SuggestedCoordinatesXY = InflectionPoints.ToArray();
            }

        }
        #endregion


        /// <summary>
        /// Update a substation coordinates and fix all lines
        /// </summary>
        /// <param name="NewPos"></param>
        /// <param name="PixelPoint"></param>
        /// <returns></returns>
        public List<MM_Line> UpdateSubstationCoordinates(PointF NewPos, Point PixelPoint)
        {
            BaseSubstation.LngLat = NewPos;
            SuggestedCoordinates = new PointF[] { NewPos };
            SuggestedCoordinatesXY = new Point[] { PixelPoint };
            List<MM_Line> OutLines = new List<MM_Line>();
            foreach (MM_Line Line in MM_Repository.Lines.Values)
            {
                if (Line.Substation1 == BaseSubstation)
                {
                    Line.Coordinates[0] = NewPos;
                    OutLines.Add(Line);
                }
                if (Line.Substation2 == BaseSubstation)
                {
                    Line.Coordinates[Line.Coordinates.Count - 1] = NewPos;
                    OutLines.Add(Line);
                }
            }
            return OutLines;
        }

        /// <summary>
        /// Update a line coordinates
        /// </summary>
        /// <param name="LngLat"></param>
        /// <param name="PixelPoint"></param>
        public void UpdateLineCoordinates(PointF LngLat, Point PixelPoint)
        {
            if (LineIndex >= 0 && LineIndex < SuggestedCoordinatesXY.Length)
            {
                SuggestedCoordinates[LineIndex] = BaseLine.Coordinates[LineIndex] = LngLat;
                SuggestedCoordinatesXY[LineIndex] = PixelPoint;
            }
        }
    }
}
