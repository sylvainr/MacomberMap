using MacomberMapCommunications.Attributes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapCommunications.Messages.Communications
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class holds information on an operator-provided suggestion for coordinate updates
    /// </summary>    
    public class MM_Coordinate_Suggestion
    {
        #region Variable declarations
        /// <summary>The TEID associated with our suggestion</summary>
        public int TEID { get; set; }

        /// <summary>The coordinates originally associated with our content</summary>
        public float[] OriginalCoordinates { get; set; }

        /// <summary>The collection of suggested coordinates</summary>
        public float[] SuggestedCoordinates { get; set; }

        /// <summary>The collection of X/Y coordinates</summary>
        public int[] SuggestedCoordinatesXY { get; set; }

        /// <summary>The user name associated with the suggestion</summary>
        public string SuggestedBy { get; set; }

        /// <summary>When the suggestion was made</summary>
        public DateTime SuggestedOn { get; set; }

        /// <summary>The machine name that made the suggestion</summary>
        public String SuggestedFrom { get; set; }

        /// <summary>Our suggestion types</summary>
        public enum enumSuggestionType
        {
            /// <summary>Unknown suggestion type</summary>
            Unknown,
            /// <summary>A suggestion on a line</summary>
            Line,
            /// <summary>A suggestion on a substation</summary>
            Substation
        };

        /// <summary>Our suggestion type</summary>
        public enumSuggestionType SuggestionType;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize an empty coordinate suggestion
        /// </summary>
        public MM_Coordinate_Suggestion()
        { }

        /// <summary>
        /// Initialize a coordiante suggestion
        /// </summary>
        /// <param name="TEID"></param>
        /// <param name="SuggestionType"></param>
        /// <param name="OriginalCoordinates"></param>
        /// <param name="SuggestedCoordinates"></param>
        /// <param name="SuggestedCoordinatesXY"></param>
        public MM_Coordinate_Suggestion(int TEID, enumSuggestionType SuggestionType, PointF[] OriginalCoordinates, PointF[] SuggestedCoordinates, Point[] SuggestedCoordinatesXY)
        {
            this.SuggestedBy = Environment.UserName;
            this.SuggestedFrom = Environment.MachineName;
            this.SuggestedOn = DateTime.Now;
            this.TEID = TEID;
            this.SuggestionType = SuggestionType;
            this.OriginalCoordinates = new float[OriginalCoordinates.Length * 2];
            this.SuggestedCoordinates = new float[SuggestedCoordinates.Length * 2];
            this.SuggestedCoordinatesXY = new int[SuggestedCoordinatesXY.Length * 2];
            for (int a=0; a <OriginalCoordinates.Length; a++)
            {
                this.OriginalCoordinates[a * 2] = OriginalCoordinates[a].X;
                this.OriginalCoordinates[(a * 2) + 1] = OriginalCoordinates[a].Y;
            }
            for (int a = 0; a < SuggestedCoordinates.Length; a++)
            {
                this.SuggestedCoordinates[a * 2] = SuggestedCoordinates[a].X;
                this.SuggestedCoordinates[(a * 2) + 1] = SuggestedCoordinates[a].Y;
            }
            for (int a = 0; a < SuggestedCoordinatesXY.Length; a++)
            {
                this.SuggestedCoordinatesXY[a * 2] = SuggestedCoordinatesXY[a].X;
                this.SuggestedCoordinatesXY[(a * 2) + 1] = SuggestedCoordinatesXY[a].Y;
            }
        }
        #endregion
    }
}
