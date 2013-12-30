using System;
using System.Collections.Generic;
using System.Text;
using Macomber_Map.Data_Elements;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using Macomber_Map.Data_Connections;
using Macomber_Map.User_Interface_Components.NetworkMap.Generic;
using Macomber_Map.User_Interface_Components.NetworkMap.GDIPlus;

namespace Macomber_Map.User_Interface_Components.GDIPlus
{
    /// <summary>
    /// This class holds the information needed to render a line
    /// </summary>
    public class MM_GDI_Line
    {
        #region Variable declarations
        /// <summary>The graphic of our line flow</summary>
        public GraphicsPath LineFlow;

        /// <summary>The graphic of the line</summary>
        public GraphicsPath LineImage;



        /// <summary>Whether the line should move forwards or backwards</summary>
        public bool MoveForward = false;

        ///<summary>The paths the flow arrow will take when it's moving forward</summary>
        internal List<MM_NetworkMap_FlowPath> FlowPaths;

        /// <summary>The current position of the arrow within our specified path (e.g., between point 0 and 1</summary>
        public int PathPosition;

        /// <summary>The size for a flow</summary>
        public float FlowSize;

        /// <summary>How many pixels the line should increment by every timer trip</summary>
        public int FlowIncrement;

        /// <summary>Our collection of inflection points and their associated angles</summary>
        public Dictionary<PointF, float> FlowPoints;

        /// <summary>Whether the line is visible</summary>
        public bool Visible;

        /// <summary>Whether the line is visible due to a violation</summary>
        public bool ViolationVisible;

        /// <summary>The rotation needed to draw the line text</summary>
        public float LineAngle;



        /// <summary>The owner of the line</summary>
        private Network_Map_GDI OwnerForm;

        /// <summary>If this line is shared with other lines connecting the same substations, the definition for it</summary>
        public MM_GDI_MultipleLine MultipleLine = null;

        /// <summary>The center of the line</summary>
        private PointF CenterLine;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new GDI Line
        /// </summary>
        /// <param name="Line">The associated line</param>
        /// <param name="Coords">The current system coordinates</param>
        /// <param name="ShownViolations">Our list of visible violations</param>
        /// <param name="CallingForm">The owning form of the violations</param>
        /// <param name="State">The state boundary</param>
        public MM_GDI_Line(MM_Line Line, MM_Coordinates Coords, Dictionary<MM_AlarmViolation, ListViewItem> ShownViolations, Network_Map_GDI CallingForm, MM_Boundary State)
        {
            ComputeLine(Line, ShownViolations, CallingForm, Coords, State, true);
            this.OwnerForm = CallingForm;
            Line.ValuesChanged += new MM_Element.ValuesChangedDelegate(Line_ValuesChanged);
        }

        /// <summary>
        /// When a line's telemetry values change, update the appropriate information
        /// </summary>
        /// <param name="ChangedLine">The changed line</param>
        public void Line_ValuesChanged(MM_Element ChangedLine)
        {
            MM_Line Line = (MM_Line)ChangedLine;
            //Determine our presentation parameters
            if (Line is MM_DCTie)
            {
                this.MoveForward = Line.MWFlowDirection == Line.ConnectedStations[1];
                MM_DCTie Tie = Line as MM_DCTie;
                if (Tie.MW_Integrated > 0f)
                    FlowSize = Tie.MW_Integrated / Line.Limits[1];
                else
                    FlowSize = -Tie.MW_Integrated / Line.Limits[0];
            }
            else if (Line.Limits[0] == 0)
                FlowSize = FlowIncrement = 0;
            else
            {
                this.MoveForward = Line.MVAFlowDirection == Line.ConnectedStations[1];
                FlowSize = Line.MVAFlow / Line.Limits[0];
            }


            if (float.IsNaN(FlowSize) || FlowSize < 0)
                FlowSize = FlowIncrement = 0;
            else
                FlowIncrement = (int)Math.Ceiling(Line.KVLevel.MVASize * FlowSize);
        }

        /// <summary>
        /// Flip the coordiantes of a transmission line, because its from and to substations differ in ordering from the coordinates start/stop
        /// </summary>
        /// <param name="Line">The transmission line to flip coordinates</param>
        private void FlipCoordinates(MM_Line Line)
        {
            PointF[] UpdatedCoordinates = new PointF[Line.Coordinates.Length];
            for (int a = 0; a < UpdatedCoordinates.Length; a++)
                UpdatedCoordinates[a] = Line.Coordinates[Line.Coordinates.Length - a - 1];
            Line.Coordinates = UpdatedCoordinates;
        }

        /// <summary>
        /// Update the line flow information on our line
        /// </summary>        
        /// <param name="Line">The line on which this image is based</param>
        /// <param name="ShownViolations">Our collection of visible violations</param>
        /// <param name="CallingForm">The network map calling the line</param>
        /// <param name="Coordinates">Our system coordinates</param>
        /// <param name="State">The state border</param>
        /// <param name="RedrawText">Whether the text should be redrawn</param>
        public void ComputeLine(MM_Line Line, Dictionary<MM_AlarmViolation, ListViewItem> ShownViolations, Network_Map_GDI CallingForm, MM_Coordinates Coordinates, MM_Boundary State, bool RedrawText)
        {
            //Ignore invalid lines
            if (float.IsNaN(Line.CenterLatLong.X)) return;

            //Determine if we need to flip coordinates because the substations have changed
            if (Line.Substation2.LatLong == Line.Coordinates[0] || Line.Substation1.LatLong == Line.Coordinates[Line.Coordinates.Length - 1])
                FlipCoordinates(Line);

            //Build our line
            LineImage = new GraphicsPath();
            List<Point> OutPts = new List<Point>(Line.Coordinates.Length);
            Point LastPoint = Point.Empty;
            PointF AggPoint = Point.Empty;
            foreach (PointF Pt in Line.Coordinates)
            {
                Point InPoint = MM_Coordinates.LatLngToXY(Pt, Coordinates.ZoomLevel);
                if (LastPoint.IsEmpty || LastPoint.X != InPoint.X || LastPoint.Y != InPoint.Y)
                {
                    AggPoint.X += InPoint.X;
                    AggPoint.Y += InPoint.Y;
                }
                OutPts.Add(InPoint);
                LastPoint = InPoint;

            }

            LineImage.AddLines(OutPts.ToArray());
            LastPoint = OutPts[OutPts.Count - 1];
            CenterLine = new PointF(AggPoint.X / (float)OutPts.Count, AggPoint.Y / (float)OutPts.Count);
            LineAngle = (float)Math.Atan2(OutPts[0].Y - LastPoint.Y, OutPts[0].X - LastPoint.X);
            if (LineAngle > (float)Math.PI / 2f)
            {
                LineAngle = (float)LineAngle - (float)Math.PI;
                Line.FlipSides = true;
            }
            else if (LineAngle < (float)Math.PI / -2f)
            {

                LineAngle = (float)LineAngle - (float)Math.PI;
                Line.FlipSides = true;
            }
            else
                Line.FlipSides = false;

            LineAngle = 180f * LineAngle / (float)Math.PI;
            //Build our flow line
            Line_ValuesChanged(Line);

            //Compute our line flow points
            SetFlowPoints(OutPts);
        }

        /// <summary>
        /// Recompute the points for the line
        /// </summary>
        /// <param name="InPoints">The series of incoming points</param>        
        public void SetFlowPoints(List<Point> InPoints)
        {
            //Initialize our flow paths
            if ((FlowPaths == null) || (FlowPaths.Count != InPoints.Count))
            {
                //If we haven't initialized the flow paths, do so.
                FlowPaths = new List<MM_NetworkMap_FlowPath>(InPoints.Count);
                for (int a = 0; a < InPoints.Count - 1; a++)
                    FlowPaths.Add(new MM_NetworkMap_FlowPath(InPoints[a], InPoints[a + 1]));
            }
            else
                //Otherwise, update them.
                for (int a = 0; a < InPoints.Count - 1; a++)
                    FlowPaths[a].RecomputeFlow(InPoints[a], InPoints[a + 1]);

            //Now, update our paths and position
            PathPosition = 0;
        }

        /// <summary>
        /// Increment the line flow's position based on the increment assigned to it.
        /// </summary>
        public void IncrementLineFlow()
        {

            if (FlowPaths == null)
                return;
            try
            {
                //Now, add in the appropriate number of 

                lock (this)
                    for (int a = 0; a < FlowIncrement; a++)
                    {
                        MM_NetworkMap_FlowPath CurFlow = FlowPaths[PathPosition];
                        if (MoveForward)
                        {
                            if ((PathPosition == FlowPaths.Count - 1) && (CurFlow.CurDimension == CurFlow.MaxDimension - 1))
                                FlowPaths[0].CurDimension = PathPosition = 0;
                            else if (CurFlow.CurDimension == CurFlow.MaxDimension - 1)
                                FlowPaths[++PathPosition].CurDimension = 0;
                            else
                                CurFlow.CurDimension++;
                        }
                        else
                        {
                            if ((PathPosition == 0) && (CurFlow.CurDimension == 0))
                            {
                                PathPosition = FlowPaths.Count - 1;
                                FlowPaths[PathPosition].CurDimension = FlowPaths[PathPosition].MaxDimension - 1;
                            }
                            else if (CurFlow.CurDimension == 0)
                            {
                                PathPosition--;
                                FlowPaths[PathPosition].CurDimension = FlowPaths[PathPosition].MaxDimension - 1;
                            }
                            else
                                CurFlow.CurDimension--;
                        }
                    }
            }
            catch (Exception)
            { }
        }
        #endregion

        /// <summary>
        /// Draw the line
        /// </summary>
        /// <param name="Line">The line upon which the image is based</param>
        /// <param name="g">The graphics connector</param>
        /// <param name="ShownViolations">The collection of shown violations</param>
        /// <param name="CallingForm">The network map calling the line</param>
        /// <param name="DisplayAlternate">Display the alternate view if blinking on standard</param>
        public void DrawLine(MM_Line Line, Graphics g, Dictionary<MM_AlarmViolation, ListViewItem> ShownViolations, Network_Map_GDI CallingForm, bool DisplayAlternate)
        {
            MM_DisplayParameter Disp = Line.LineDisplay(ShownViolations, CallingForm, DisplayAlternate);
            using (Pen DrawPen = (Pen)Disp.ForePen.Clone())
            {
                if (MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.DimNonBlackstartElements && !Line.IsBlackstart)
                    DrawPen.Color = MM_Repository.OverallDisplay.BlackstartDim(Disp.ForeColor);
                else if (MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.DimNonOperatorElements && !Line.Operator.Equals(MM_Repository.OverallDisplay.DisplayCompany))
                    DrawPen.Color = MM_Repository.OverallDisplay.BlackstartDim(Disp.ForeColor);
                g.DrawPath(DrawPen, LineImage);
            }
            /*
            Color DrawColor = Disp.ForeColor;
            if (MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.DimNonBlackstartElements && !Line.IsBlackstart)
                DrawColor = MM_Repository.OverallDisplay.BlackstartDim(Disp.ForeColor);
            else if (MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.DimNonOperatorElements && !Line.Operator.Equals(MM_Repository.OverallDisplay.DisplayCompany))
                DrawColor = MM_Repository.OverallDisplay.BlackstartDim(Disp.ForeColor);
            using (Pen DrawPen = new Pen(DrawColor, Disp.Width))
                g.DrawPath(DrawPen, LineImage);*/
        }

        /// <summary>
        /// Draw the line text for the requested line
        /// </summary>
        /// <param name="Line">The underlying line ot use</param>
        /// <param name="g">The graphics connector</param>
        /// <param name="Coordinates"></param>
        public void DrawLineText(MM_Line Line, Graphics g, MM_Coordinates Coordinates)
        {
            //Draw our line text
            if (Line.KVLevel.ShowMVARText || Line.KVLevel.ShowMVAText || Line.KVLevel.ShowMWText || Line.KVLevel.ShowPercentageText || Line.KVLevel.ShowLineName)
            {
                String OutText = LineText(Line);
                if (!String.IsNullOrEmpty(OutText))
                {
                    Matrix CurMatrix = g.Transform;
                    using (Matrix RotateMatrix = new Matrix())
                    {
                        RotateMatrix.Translate(CurMatrix.OffsetX, CurMatrix.OffsetY);
                        RotateMatrix.RotateAt(LineAngle, CenterLine);
                        g.Transform = RotateMatrix;
                        g.DrawString(OutText, MM_Repository.OverallDisplay.NetworkMapFont, Brushes.Gray, CenterLine);
                        g.Transform = CurMatrix;
                    }
                }
            }
        }

        /// <summary>
        /// Draw the flow line
        /// </summary>
        /// <param name="g">The graphics connector</param>
        /// <param name="Line">The underlying line ot use</param>        
        /// <param name="CallingForm">The calling form</param>
        /// <param name="DisplayAlternate">Whether the alternate display should be used</param>
        /// <param name="ShownViolations">The collection of shown violations</param>
        /// <param name="ZoomLevel">The current zoom level</param>
        public void DrawFlowLine(Graphics g, MM_Line Line, Dictionary<MM_AlarmViolation, ListViewItem> ShownViolations, Network_Map_GDI CallingForm, bool DisplayAlternate, float ZoomLevel)
        {
            MM_DisplayParameter Disp = Line.LineDisplay(ShownViolations, CallingForm, DisplayAlternate);

            //Determine how large our arrow should be
            float FlowSize = this.FlowSize * ZoomLevel / 1.5f;

            //Now, draw the line as appropriate
            if (FlowPaths.Count == 0)
                return;

            PointF ArrowTip = FlowPaths[PathPosition].Current;
            PointF Arrow1 = PointF.Empty, Arrow2 = PointF.Empty;

            if (MoveForward)
            {
                Arrow1.X = ArrowTip.X + (FlowSize * FlowPaths[PathPosition].cost - FlowSize * FlowPaths[PathPosition].sint);
                Arrow1.Y = ArrowTip.Y + (FlowSize * FlowPaths[PathPosition].sint + FlowSize * FlowPaths[PathPosition].cost);

                Arrow2.X = ArrowTip.X + (FlowSize * FlowPaths[PathPosition].cost + FlowSize * FlowPaths[PathPosition].sint);
                Arrow2.Y = ArrowTip.Y - (FlowSize * FlowPaths[PathPosition].cost - FlowSize * FlowPaths[PathPosition].sint);
            }
            else
            {
                Arrow1.X = ArrowTip.X - (FlowSize * FlowPaths[PathPosition].cost - FlowSize * FlowPaths[PathPosition].sint);
                Arrow1.Y = ArrowTip.Y - (FlowSize * FlowPaths[PathPosition].sint + FlowSize * FlowPaths[PathPosition].cost);

                Arrow2.X = ArrowTip.X - (FlowSize * FlowPaths[PathPosition].cost + FlowSize * FlowPaths[PathPosition].sint);
                Arrow2.Y = ArrowTip.Y + (FlowSize * FlowPaths[PathPosition].cost - FlowSize * FlowPaths[PathPosition].sint);
            }

            Color DrawColor = Disp.ForeColor;
            if (MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.DimNonBlackstartElements && !Line.IsBlackstart)
                DrawColor = MM_Repository.OverallDisplay.BlackstartDim(Disp.ForeColor);
            else if (MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.DimNonOperatorElements && !Line.Operator.Equals(MM_Repository.OverallDisplay.DisplayCompany))
                DrawColor = MM_Repository.OverallDisplay.BlackstartDim(Disp.ForeColor);



            using (Brush NewBrush = new SolidBrush(DrawColor))
                g.FillPolygon(NewBrush, new PointF[] { ArrowTip, Arrow1, Arrow2 });
            //g.FillPolygon(Param.ForePen.Brush, new PointF[] { ArrowTip, Arrow1, Arrow2 });
        }

        /// <summary>
        /// Rebuild our line text
        /// </summary>
        /// <param name="Line">The line for which text should be built</param>
        public String LineText(MM_Line Line)
        {
            //Build our text for the line
            StringBuilder OutLine = new StringBuilder();

            if (Line.KVLevel.ShowLineName)
                OutLine.Append(" " + Line.Name);

            if (Line.KVLevel.ShowPercentageText)
                if (Line.MVAFlowDirection == null)
                    OutLine.Append(" " + Line.LinePercentageText);
                else if (Line.MVAFlowDirection == Line.ConnectedStations[0] ^ !Line.FlipSides)
                    OutLine.Append(" ◄ " + Line.LinePercentageText);
                else
                    OutLine.Append(" ► " + Line.LinePercentageText);

            if (Line.KVLevel.ShowMVAText)
                if (Line.MVAFlowDirection == null)
                    if (float.IsNaN(Line.MVAFlow))
                        OutLine.Append(" ? mva");
                    else
                        OutLine.Append(" " + Line.MVAFlow.ToString("0") + " mva");
                else if (Line.MVAFlowDirection == Line.ConnectedStations[0] ^ !Line.FlipSides)
                    OutLine.Append(" ◄ " + Line.MVAFlow.ToString("0") + " mva");
                else
                    OutLine.Append(" ► " + Line.MVAFlow.ToString("0") + " mva");

            if (Line.KVLevel.ShowMWText)
                if (Line.MWFlowDirection == null)
                    if (float.IsNaN(Line.MWFlow))
                        OutLine.Append(" ? mw");
                    else
                        OutLine.Append(" " + Line.MWFlow.ToString("0") + " mw");
                else if (Line.MWFlowDirection == Line.ConnectedStations[0] ^ !Line.FlipSides)
                    OutLine.Append(" ◄ " + Line.MWFlow.ToString("0") + " mw");
                else
                    OutLine.Append(" ► " + Line.MWFlow.ToString("0") + " mw");


            if (Line.KVLevel.ShowMVARText)
                if (Line.MVARFlowDirection == null)
                    if (float.IsNaN(Line.MVARFlow))
                        OutLine.Append(" ? mw");
                    else
                        OutLine.Append(" ◄ ► " + Line.MVARFlow.ToString("0") + " mvar");
                else if (Line.MVARFlowDirection == Line.ConnectedStations[0] ^ !Line.FlipSides)
                    OutLine.Append(" ◄ " + Line.MVARFlow.ToString("0") + " mvar");
                else
                    OutLine.Append(" ► " + Line.MVARFlow.ToString("0") + " mvar");





            //Make sure we have text, and that it's valid
            return OutLine.ToString(1, OutLine.Length - 1);
        }


    }
}

