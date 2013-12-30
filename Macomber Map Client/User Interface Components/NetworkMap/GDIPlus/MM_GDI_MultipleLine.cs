using System;
using System.Collections.Generic;
using System.Text;
using Macomber_Map.User_Interface_Components.GDIPlus;
using Macomber_Map.Data_Elements;
using System.Drawing;
using System.Windows.Forms;
using Macomber_Map.User_Interface_Components.NetworkMap.Generic;
using System.Drawing.Drawing2D;

namespace Macomber_Map.User_Interface_Components.NetworkMap.GDIPlus
{
    /// <summary>
    /// This class holds information on a multiple line (multiple lines between the same two substations) 
    /// </summary>
    public class MM_GDI_MultipleLine
    {
        #region Variable declarations
        /// <summary>The collection of visible lines</summary>
        public Dictionary<MM_Line,MM_GDI_Line> LineCollection = new Dictionary<MM_Line,MM_GDI_Line>(3);

        /// <summary>The maximum width for any of the lines</summary>
        public float MaxWidth;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new multiple line collection
        /// </summary>
        /// <param name="FirstLine"></param>
        /// <param name="SecondLine"></param>
        /// <param name="FirstLineGDI"></param>
        /// <param name="SecondLineGDI"></param>
        public MM_GDI_MultipleLine(MM_Line FirstLine, MM_GDI_Line FirstLineGDI, MM_Line SecondLine, MM_GDI_Line SecondLineGDI)
        {
            LineCollection.Add(FirstLine,FirstLineGDI);
            LineCollection.Add(SecondLine,SecondLineGDI);
            SecondLineGDI.MultipleLine = FirstLineGDI.MultipleLine = this;
            MaxWidth = Math.Max(FirstLine.KVLevel.Energized.Width, SecondLine.KVLevel.Energized.Width);
        }

        /// <summary>
        /// Add a new line to the collection
        /// </summary>
        /// <param name="NewLine"></param>
        /// <param name="NewLineGDI"></param>
        public void AddLine(MM_Line NewLine,  MM_GDI_Line NewLineGDI)
        {
            if (!LineCollection.ContainsKey(NewLine))
            {
                LineCollection.Add(NewLine, NewLineGDI);
                NewLineGDI.MultipleLine = this;
                MaxWidth = Math.Max(MaxWidth, NewLine.KVLevel.Energized.Width);
            }
        }
        #endregion

        /// <summary>
        /// Determine if any of the lines are visible
        /// </summary>
        public bool Visible
        {
            get {
                int VisibleTally = 0;
                foreach (MM_GDI_Line Line in LineCollection.Values)
                    if (Line.Visible)
                        VisibleTally++;
                return VisibleTally > 0; //Changed from 1 to 0 as logic was not right for case of multiple KV lines and only 1 of multi set was visiable would show none 20130610 - mn
            }
        }

        /// <summary>
        /// Determine if any of the lines are violation visible
        /// </summary>
        public bool ViolationVisible
        {
            get {
                foreach (MM_GDI_Line Line in LineCollection.Values)
                    if (Line.ViolationVisible)
                        return true;
                return false;
            }
        }

        /// <summary>
        /// Draw the underpinning highlight
        /// </summary>
        /// <param name="g"></param>
        public void DrawMultipleLine(Graphics g)
        {
            bool IsBlackstart = false;
            bool IsCompany = false;
            GraphicsPath DrawPath=null;
            foreach (KeyValuePair<MM_Line, MM_GDI_Line> Line in LineCollection)
            {
                IsBlackstart |= Line.Key.IsBlackstart;
                if (!IsCompany && MM_Repository.OverallDisplay.DisplayCompany != null && Line.Key.Operator.Equals(MM_Repository.OverallDisplay.DisplayCompany))
                    IsCompany = true;
                DrawPath= Line.Value.LineImage;
            }

            Color DrawColor = Color.Thistle;
            if (MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.DimNonBlackstartElements && !IsBlackstart)
                DrawColor = MM_Repository.OverallDisplay.BlackstartDim(Color.Thistle);
            else if (MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.HideNonOperatorElements && !IsCompany)
                DrawColor = MM_Repository.OverallDisplay.BlackstartDim(Color.Thistle);

            if (MM_Repository.OverallDisplay.MultipleLineWidthAddition > 0)
                using (Pen DrawPen = new Pen(DrawColor, MaxWidth + MM_Repository.OverallDisplay.MultipleLineWidthAddition))
                    g.DrawPath(DrawPen, DrawPath);
        }

        /// <summary>
        /// Determine the line that best represents the multiple line, based on voltage, energization state, etc.
        /// </summary>
        public KeyValuePair<MM_Line, MM_GDI_Line> BestLine
        {
            get 
            {
                float MaxVal = float.NaN;
                KeyValuePair<MM_Line, MM_GDI_Line> OutVal = default(KeyValuePair<MM_Line, MM_GDI_Line>);                
                foreach (KeyValuePair<MM_Line, MM_GDI_Line> Line in LineCollection)                                                            
                    if (Line.Value.Visible && (float.IsNaN(MaxVal) || MaxVal < Line.Key.LinePercentage))
                    {
                        MaxVal = Line.Key.LinePercentage;
                        OutVal = Line;
                    }

                return OutVal;
            }


        }
        
    }
}
