using MacomberMapClient.Data_Elements.Geographic;
using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Data_Elements.Violations;
using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.Communications;
using MacomberMapCommunications.Messages;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace MacomberMapClient.User_Interfaces.OneLines
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class holds information on a one-line element, which is deserialized from the MM_OneLine XML.
    /// </summary>
    public class MM_OneLine_Element: MM_Serializable
    {
        #region Variable Declarations
        /// <summary>The base element associated with our component</summary>
        public MM_Element BaseElement;

        /// <summary>Whether an arrow should be drawn from label to element</summary>
        public bool DescriptorArrow = false;

        /// <summary>The bounds of our element representation</summary>        
        public Rectangle Bounds;

        /// <summary>The font to draw our label</summary>
        public Font Font = null;

        /// <summary>Our font for drawing a synchroscope</summary>
        public static Font SynchroscopeFont = new Font("Arial", 9, FontStyle.Bold);

        /// <summary>Our font for drawing a synchrocheck relay</summary>
        public static Font SynchrocheckFont = new Font("Arial", 9, FontStyle.Italic);

        /// <summary>Our font for drawing synchroscope and synchrocheck labels</summary>
        public static Font SynchroscopeAndCheckFont = new Font("Arial", 9, FontStyle.Italic | FontStyle.Bold);

        /// <summary>The foreground color of our object, if any</summary>
        public Color ForeColor = Color.White;

        /// <summary>The element type of our component</summary>
        public enumElemTypes ElemType = enumElemTypes.None;

        /// <summary>The parent element</summary>
        public MM_OneLine_Element ParentElement = null;
  
        /// <summary>An element's descriptor</summary>
        public MM_OneLine_Element Descriptor;

        /// <summary>An element's secondary descriptor</summary>
        public MM_OneLine_Element SecondaryDescriptor = null;

        /// <summary>Our transformer windings</summary>
        public MM_OneLine_TransformerWinding[] Windings = null;

        /// <summary>The orientation of our element</summary>
        public enumOrientations Orientation;

        /// <summary>The KV Level of our element</summary>
        public MM_KVLevel KVLevel;
        
        /// <summary>Our element's text</summary>
        public String Text = null;

        /// <summary>Our display state for a breaker or switch</summary>
        public CheckState Opened = CheckState.Indeterminate;

        /// <summary>The image to be drawn, if any</summary>
        public Image Image = null;

        /// <summary>The contingencies associated with our one-line</summary>
        public MM_Contingency[] Contingencies;

        /// <summary>The XML configuration for our one-line element</summary>
        public XmlElement xConfig;

        /// <summary>The string format that makes text centered</summary>
        public static StringFormat CenterFormat;

        /// <summary>Our tab spacing when left-aligning</summary>
        private static float[] Tabs = new float[] { 50, 50, 50 };
        #endregion

        #region Enumerations
        /// <summary>
        /// The collection of element types
        /// </summary>     
        public enum enumElemTypes
        {
            /// <summary>No elements</summary>
            None,
            /// <summary>An image</summary>
            Image,
            /// <summary>A transformer</summary>
            Transformer,
            /// <summary>A primary descriptor</summary>
            Descriptor,
            /// <summary>A secondary descriptor (resource node name)</summary>
            SecondaryDescriptor,
            /// <summary>A circuit breaker</summary>
            Breaker,
            /// <summary>A disconnector</summary>
            Switch,
            /// <summary>A generating unit</summary>
            Unit,
            /// <summary>A transmission line</summary>
            Line,
            /// <summary>A reactor</summary>
            Reactor,
            /// <summary>A reactor</summary>
            Load,
            /// <summary>A load acting as a resource (LaaR)</summary>
            LAAR,
            /// <summary>An end cap</summary>
            EndCap,
            /// <summary>A capacitor</summary>
            Capacitor,
            /// <summary>A static var compensator</summary>
            StaticVarCompensator,
            /// <summary>A node</summary>
            Node,
            /// <summary>A poke point</summary>
            PokePoint,
            /// <summary>A pricing vector</summary>
            PricingVector,
            /// <summary>A static label</summary>
            Label,
            /// <summary>A circle with no element linkage</summary>
            Circle,
            /// <summary>A rectangle</summary>
            Rectangle,
            /// <summary>A resistor</summary>
            Resistor,
            /// <summary>A component with resistance and reactance</summary>
            ResistorReactor,
            /// <summary>A component with resistance and capacitance</summary>
            ResistorCapacitor,
            /// <summary>A line with an arrow</summary>
            ArrowLine,
            /// <summary>A RAS status panel</summary>
            RAS_Status_Panel
        }

        /// <summary>The possible orientations for attributes of the element</summary>
        public enum enumOrientations
        {
            /// <summary>Unknown orientation</summary>
            Unknown,
            /// <summary>Upward orientation</summary>
            Up,
            /// <summary>Downward orientation</summary>
            Down,
            /// <summary>Leftward orientation</summary>
            Left,
            /// <summary>Rightward orientation</summary>
            Right,
            /// <summary>Horizontal orientation</summary>
            Horizontal,
            /// <summary>Vertical orientation</summary>
            Vertical
        };
        #endregion

        #region Static drawing
        /// <summary>
        /// Determine the center of a rectangle
        /// </summary>
        /// <param name="Rect"></param>
        /// <returns></returns>
        public static Point CenterRect(Rectangle Rect)
        {
            return new Point(Rect.Left + (Rect.Width / 2), Rect.Top + (Rect.Height / 2));

        }

        //When our one-line element is created, determine all of our formats
        static MM_OneLine_Element()
        {
            CenterFormat = new StringFormat(StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip | StringFormatFlags.NoWrap);
            CenterFormat.Alignment = StringAlignment.Center;
            CenterFormat.LineAlignment = StringAlignment.Center;
            CenterFormat.Trimming = StringTrimming.None;
        }

          

        /// <summary>
        /// Report the proper format for our components
        /// </summary>
        public StringFormat FormatForText(out bool ShiftText)
        {
            ShiftText = false;
            StringFormat OutFormat = new StringFormat(StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip | StringFormatFlags.NoWrap) { Trimming = StringTrimming.None };
            OutFormat.LineAlignment = StringAlignment.Center;
            OutFormat.Alignment = StringAlignment.Center;

            if (ElemType == enumElemTypes.Descriptor && ParentElement.ElemType == enumElemTypes.Transformer)
            {
                MM_OneLine_TransformerWinding XFW = ParentElement.Windings[ParentElement.Windings[1].IsPrimary ? 0 : 1];
                if (XFW.Orientation == enumOrientations.Left)
                {
                    OutFormat.Alignment = StringAlignment.Far;
                    OutFormat.LineAlignment = Bounds.Bottom > ParentElement.Bounds.Top ? StringAlignment.Near : StringAlignment.Far;
                }
                else if (XFW.Orientation == enumOrientations.Right)
                {
                    OutFormat.Alignment = StringAlignment.Near;
                    OutFormat.LineAlignment = Bounds.Bottom > ParentElement.Bounds.Top ? StringAlignment.Near : StringAlignment.Far;
                }
                else if (XFW.Orientation == enumOrientations.Up)
                {
                    OutFormat.Alignment = Bounds.Right > ParentElement.Bounds.Left ? StringAlignment.Near : StringAlignment.Far;
                    OutFormat.LineAlignment = StringAlignment.Near;
                }
                else if (XFW.Orientation == enumOrientations.Down)
                {
                    OutFormat.Alignment = Bounds.Right > ParentElement.Bounds.Left ? StringAlignment.Near : StringAlignment.Far;
                    OutFormat.LineAlignment = StringAlignment.Far;
                }
            }
            else if (this.Orientation == enumOrientations.Left)
            {
                OutFormat.Alignment = StringAlignment.Far;
                OutFormat.LineAlignment = StringAlignment.Center;
            }
            else if (this.Orientation == enumOrientations.Right)
            {
                OutFormat.Alignment = StringAlignment.Near;
                OutFormat.LineAlignment = StringAlignment.Center;
            }
            else if (this.Orientation == enumOrientations.Up)
            {
                OutFormat.Alignment = StringAlignment.Near;
                OutFormat.LineAlignment = StringAlignment.Near;
                ShiftText = true;
            }
            else if (this.Orientation == enumOrientations.Down)
            {
                OutFormat.Alignment = StringAlignment.Near;
                OutFormat.LineAlignment = StringAlignment.Far;
                ShiftText = true;
            }
            else
                return CenterFormat;
            OutFormat.SetTabStops(0, Tabs);
            return OutFormat;
        }


        /// <summary>
        /// Convert a series of integers into a series of points
        /// </summary>
        /// <param name="Points"></param>
        /// <returns></returns>
        public static Point[] IntToPoints(params int[] Points)
        {
            Point[] OutPts = new Point[Points.Length / 2];
            for (int a = 0; a < Points.Length; a += 2)
                OutPts[a / 2] = new Point(Points[a], Points[a + 1]);
            return OutPts;
        }

        /// <summary>
        /// Draw the header bar for our one
        /// </summary>
        /// <param name="g"></param>
        /// <param name="Bounds"></param>
        /// <param name="ParentElement"></param>
        public static void DrawHeader(Graphics g, Rectangle Bounds, MM_Element ParentElement)
        {
            String LongName="", Name="", County = "Unknown", Coord = "";
            

            if (ParentElement is MM_Substation)
            {
                MM_Substation Sub = (MM_Substation)ParentElement;
                LongName = Sub.LongName;
                Name = Sub.Name;
                Coord = String.Format("County: {0}\nLat={1:0.000}, Lng={2:0.000}", Sub.County, Sub.Latitude, Sub.Longitude);                
            }
            else if (ParentElement is MM_Contingency)
            {
                MM_Contingency Ctg = (MM_Contingency)ParentElement;
                LongName = Ctg.Description;                
                foreach (MM_Boundary Bound in ((MM_Contingency)ParentElement).Counties)
                    County += (County.Length == 0 ? "Counties: ": ", ") + Bound.Name;                               
            }
                      
            //Now, write out our header
            if (Data_Integration.SimulatorStatus == MacomberMapCommunications.Messages.EMS.MM_Simulation_Time.enumSimulationStatus.Running)
                g.FillRectangle(Brushes.SteelBlue, Bounds);
            else
                using (SolidBrush sB = new SolidBrush(Data_Integration.SimulatorStatusColor))
                    g.FillRectangle(sB, Bounds);

            using (Font Arial = new Font("Arial", 9))
                g.DrawString(Coord, Arial, Brushes.White, Bounds.Left + 2, Bounds.Top + 3);
            
            using (Font Arial = new Font("Arial", 15))
            {
                String OutText = (String.IsNullOrEmpty(LongName) || LongName.Equals(Name, StringComparison.CurrentCultureIgnoreCase)) ? Name : LongName + " (" + Name + ")";
                Size HeadSize = g.MeasureString(OutText, Arial).ToSize();
                g.DrawString(OutText, Arial, Brushes.White, Bounds.Left + ((Bounds.Width - HeadSize.Width) / 2), Bounds.Top + ((Bounds.Height - HeadSize.Height) / 2));
            }

          
        }

        /// <summary>
        /// Draw a switch
        /// </summary>
        /// <param name="g"></param>
        /// <param name="Bounds"></param>
        /// <param name="BackBrush"></param>
        /// <param name="Orientation"></param>
        /// <param name="DrawColor"></param>
        /// <param name="State"></param>
        /// <param name="NormalOpen"></param>
        /// <param name="ThicknessOverride"></param>
        public static void DrawSwitch(Graphics g, Rectangle Bounds, Brush BackBrush, enumOrientations Orientation, Color DrawColor, MM_Breaker_Switch.BreakerStateEnum State, bool NormalOpen, float ThicknessOverride = 1f)
        {
            bool IsVertical = Orientation == enumOrientations.Vertical || Orientation == enumOrientations.Up || Orientation == enumOrientations.Down;
            Point Center = CenterRect(Bounds);
            Size SlashSize = new Size(6, 6);

            //Now, build our drawing brush
            using (Pen DrawPen = new Pen(DrawColor, ThicknessOverride))
            {

                if (State == MM_Breaker_Switch.BreakerStateEnum.Closed)
                {
                    g.DrawLine(DrawPen, Point.Subtract(Center, SlashSize), Point.Add(Center, SlashSize));
                    if (IsVertical)
                        g.DrawLine(DrawPen, Center.X, Bounds.Top, Center.X, Bounds.Bottom - 1);
                    else
                        g.DrawLine(DrawPen, Bounds.Left, Center.Y, Bounds.Right - 1, Center.Y);
                }
                else if (IsVertical)
                {
                    g.DrawLine(Pens.Black, Center.X, Center.Y - SlashSize.Height, Center.X, Center.Y + SlashSize.Height);
                    g.DrawLines(DrawPen, IntToPoints(Center.X, Bounds.Top, Center.X, Center.Y - SlashSize.Height, Center.X + SlashSize.Width, Center.Y + SlashSize.Height));
                    g.DrawLine(DrawPen, Center.X, Center.Y + SlashSize.Height, Center.X, Bounds.Bottom - 1);
                }
                else
                {
                    g.DrawLine(Pens.Black, Center.X - SlashSize.Width, Center.Y, Center.X + SlashSize.Width, Center.Y);
                    g.DrawLines(DrawPen, IntToPoints(Bounds.Left, Center.Y, Center.X - SlashSize.Width, Center.Y, Center.X + SlashSize.Width, Center.Y - SlashSize.Height));
                    g.DrawLine(DrawPen, Center.X + SlashSize.Width, Center.Y, Bounds.Right - 1, Center.Y);
                }


                //Now, if we're in an unknown state, draw our question mark
                if (State == MM_Breaker_Switch.BreakerStateEnum.Unknown)
                {
                    DrawPen.DashStyle = DashStyle.Dot;
                    using (Font BoldFont = new Font("Arial", 14, FontStyle.Bold))
                        g.DrawString("?", BoldFont, DrawPen.Brush, Bounds, CenterFormat);
                }

                if (NormalOpen)
                    if (Orientation == enumOrientations.Horizontal)
                        g.DrawString("N.O.", SystemFonts.SmallCaptionFont, DrawPen.Brush, Bounds.Right, Bounds.Top);
                    else
                        g.DrawString("N.O.", SystemFonts.SmallCaptionFont, DrawPen.Brush, Bounds.Left , Bounds.Bottom);

            }
          
        }

        /// <summary>
        /// Draw an arrow from the corner of one rectangle to a point in another
        /// </summary>
        /// <param name="g"></param>
        /// <param name="SourceBounds"></param>
        /// <param name="TargetBounds"></param>
        /// <param name="ThicknessOverride"></param>
        public static void DrawArrow(Graphics g, Rectangle SourceBounds, Rectangle TargetBounds, float ThicknessOverride = 1f)
        {
            using (Pen LinePen = new Pen(Color.White, ThicknessOverride))
            using (GraphicsPath gp = new GraphicsPath(new Point[] { Point.Empty, new Point(-4, -4), Point.Empty, new Point(4, -4) }, new byte[] { 0, 1, 1, 129 }))
            using (CustomLineCap c = new CustomLineCap(null, gp))
            {
                LinePen.CustomStartCap = c;
                Point SourcePt, TargetPt;
                BuildArrowPath(SourceBounds, TargetBounds, out SourcePt, out TargetPt);
                g.DrawLine(LinePen, SourcePt, TargetPt);
            }
        }

        /// <summary>
        /// Determine the path from element to arrow target
        /// </summary>
        /// <param name="SourceBounds"></param>
        /// <param name="TargetBounds"></param>
        /// <param name="SourcePt"></param>
        /// <param name="TargetPt"></param>
        public static void BuildArrowPath(Rectangle SourceBounds, Rectangle TargetBounds, out Point SourcePt, out Point TargetPt)
        {
            SourcePt = Point.Empty;
            TargetPt = Point.Empty;
            if (SourceBounds.Right < TargetBounds.Left)
            {
                SourcePt.X = SourceBounds.Right;
                TargetPt.X = TargetBounds.Left;
            }
            else if (SourceBounds.Left > TargetBounds.Right)
            {
                SourcePt.X = SourceBounds.Left;
                TargetPt.X = TargetBounds.Right;
            }
            else
            {
                SourcePt.X = SourceBounds.Left + (SourceBounds.Width / 2);
                TargetPt.X = TargetBounds.Left + (TargetBounds.Width / 2);
            }

            if (SourceBounds.Bottom < TargetBounds.Top)
            {
                SourcePt.Y = SourceBounds.Bottom;
                TargetPt.Y = TargetBounds.Top;
            }
            else if (SourceBounds.Top > TargetBounds.Bottom)
            {
                SourcePt.Y = SourceBounds.Top;
                TargetPt.Y = TargetBounds.Bottom;
            }
            else
            {
                SourcePt.Y = SourceBounds.Top + (SourceBounds.Height / 2);
                TargetPt.Y = TargetBounds.Top + (TargetBounds.Height / 2);
            }
        }

        /// <summary>
        /// Draw the breaker at the requested location
        /// </summary>
        /// <param name="g"></param>
        /// <param name="Bounds"></param>
        /// <param name="BackBrush"></param>
        /// <param name="Orientation"></param>
        /// <param name="DrawColor"></param>
        /// <param name="State"></param>
        /// <param name="NormalOpen"></param>
        /// <param name="ThicknessOverride"></param>
        public static void DrawBreaker(Graphics g, Rectangle Bounds, Brush BackBrush, enumOrientations Orientation, Color DrawColor, MM_Breaker_Switch.BreakerStateEnum State, bool NormalOpen, float ThicknessOverride = 1f)
        {
            Rectangle DrawRect = new Rectangle(Bounds.Left + 2, Bounds.Top + 2, Bounds.Width - 5, Bounds.Height - 5);
            Point Center = CenterRect(Bounds);

            using (Font DrawFont = new Font("Arial", 13, FontStyle.Bold))
            using (SolidBrush ForeBrush = new SolidBrush(DrawColor))
            using (Pen DrawPen = new Pen(DrawColor, ThicknessOverride))
            {
                if (State == MM_Breaker_Switch.BreakerStateEnum.Open)
                {
                    g.FillRectangle(BackBrush, DrawRect);
                    g.DrawRectangle(DrawPen, DrawRect);
                    g.DrawString("O", DrawFont, ForeBrush, DrawRect, CenterFormat);
                }
                else if (State == MM_Breaker_Switch.BreakerStateEnum.Closed)
                {
                    g.FillRectangle(ForeBrush, DrawRect);
                    g.DrawString("C", DrawFont, BackBrush, DrawRect, CenterFormat);
                }
                else
                {
                    g.FillRectangle(BackBrush, DrawRect);
                    using (HatchBrush hb = new HatchBrush(HatchStyle.Percent50, Color.FromArgb(64, DrawColor), Color.Transparent))
                        g.FillRectangle(hb, DrawRect);
                    g.DrawRectangle(DrawPen, DrawRect);
                    g.DrawString("?", DrawFont, ForeBrush, DrawRect, CenterFormat);
                }

                if (Orientation == enumOrientations.Right || Orientation == enumOrientations.Horizontal)
                    g.DrawLine(DrawPen, Bounds.Left, Center.Y, DrawRect.Left, Center.Y);
                if (Orientation == enumOrientations.Left || Orientation == enumOrientations.Horizontal)
                    g.DrawLine(DrawPen, DrawRect.Right, Center.Y, Bounds.Right - 1, Center.Y);
                if (Orientation == enumOrientations.Down || Orientation == enumOrientations.Vertical)
                    g.DrawLine(DrawPen, Center.X, Bounds.Top, Center.X, DrawRect.Top);
                if (Orientation == enumOrientations.Up || Orientation == enumOrientations.Vertical)
                    g.DrawLine(DrawPen, Center.X, DrawRect.Bottom, Center.X, Bounds.Bottom - 1);


                //If we're normal open, draw our status.
                if (NormalOpen)
                        if (Orientation == enumOrientations.Horizontal)
                            g.DrawString("N.O.", SystemFonts.SmallCaptionFont, DrawPen.Brush, Bounds.Right, Bounds.Top);
                        else
                            g.DrawString("N.O.", SystemFonts.SmallCaptionFont, DrawPen.Brush, Bounds.Left, Bounds.Bottom);
                   
            }
        }

        /// <summary>
        /// Draw a capactior
        /// </summary>
        /// <param name="g"></param>
        /// <param name="Bounds"></param>
        /// <param name="Orientation"></param>
        /// <param name="DrawColor"></param>
        /// <param name="BackBrush"></param>
        /// <param name="GroundBar"></param>
        /// <param name="ThicknessOverride"></param>
        public static void DrawCapacitor(Graphics g, Rectangle Bounds, Brush BackBrush, enumOrientations Orientation, Color DrawColor, bool GroundBar, float ThicknessOverride=1f)
        {
          
            Point Center = CenterRect(Bounds);
            using (Pen DrawingPen = new Pen(DrawColor, ThicknessOverride))
                if (Orientation == enumOrientations.Up)
                {
                    g.DrawLine(DrawingPen, Center.X, Bounds.Top, Center.X, Bounds.Bottom);
                    g.DrawLine(DrawingPen, Bounds.Left, Bounds.Top, Bounds.Right - 1, Bounds.Top);
                    g.DrawArc(DrawingPen, Bounds.Left, Bounds.Top, Bounds.Width - 1, Bounds.Height - 1, 180, 180);
                }
                else if (Orientation == enumOrientations.Down)
                {
                    int Third = Bounds.Height / 3;

                    g.DrawLine(DrawingPen, Center.X, Bounds.Top, Center.X, Bounds.Top + (Bounds.Height / 4));
                    g.DrawLine(DrawingPen, Bounds.Left + 2, Bounds.Top + Third, Bounds.Right - 3, Bounds.Top + Third);
                    g.DrawArc(DrawingPen, Bounds.Left, Bounds.Top - Third - Third, Bounds.Width - 1, Bounds.Height - 3, 0, 180);
                }
                else if (Orientation == enumOrientations.Left || Orientation == enumOrientations.Unknown)
                {
                    g.DrawLine(DrawingPen, Bounds.Left, Center.Y, Bounds.Right - 1, Center.Y);
                    g.DrawLine(DrawingPen, Bounds.Left, Bounds.Top, Bounds.Left, Bounds.Bottom - 1);
                    g.DrawArc(DrawingPen, Bounds.Left + 1, Bounds.Top, Bounds.Width - 1, Bounds.Height - 1, 90, 180);
                }
                else if (Orientation == enumOrientations.Right)
                {
                    g.DrawLine(DrawingPen, Bounds.Left, Center.Y, Bounds.Right - 1, Center.Y);
                    g.DrawLine(DrawingPen, Bounds.Right - 1, Bounds.Top, Bounds.Right - 1, Bounds.Bottom - 1);
                    g.DrawArc(DrawingPen, Bounds.Left - 1, Bounds.Top, Bounds.Width - 1, Bounds.Height - 1, 270, 180);
                }
                else if (Orientation == enumOrientations.Horizontal)
                {
                    g.DrawLine(DrawingPen, Bounds.Left, Center.Y, Bounds.Right - 1, Center.Y);
                    g.DrawLine(DrawingPen, Bounds.Right - 1, Bounds.Top, Bounds.Right - 1, Bounds.Bottom - 1);
                    g.DrawArc(DrawingPen, Bounds.Left - 1, Bounds.Top, Bounds.Width - 1, Bounds.Height - 1, 270, 180);
                }
                else if (Orientation == enumOrientations.Vertical)
                {
                    g.DrawLine(DrawingPen, Center.X, Bounds.Top, Center.X, Bounds.Bottom - 1);
                    g.DrawLine(DrawingPen, Bounds.Left, Bounds.Bottom - 1, Bounds.Right - 1, Bounds.Bottom - 1);
                    g.DrawArc(DrawingPen, Bounds.Left, Bounds.Top - 1, Bounds.Width - 1, Bounds.Height - 1, 0, 180);
                }
           // if (GroundBar)
            //    DrawGroundBar(g, Bounds, BackBrush, Orientation, DrawColor);

        }

        /// <summary>
        /// Draw an endcap
        /// </summary>
        /// <param name="g"></param>
        /// <param name="Bounds"></param>
        /// <param name="BackBrush"></param>
        /// <param name="Orientation"></param>
        /// <param name="DrawColor"></param>
        public static void DrawEndCap(Graphics g, Rectangle Bounds, Brush BackBrush, enumOrientations Orientation, Color DrawColor)
        {
            using (Brush CircleBrush = new SolidBrush(DrawColor))
                g.FillEllipse(CircleBrush, Bounds);
        }

        /// <summary>
        /// Draw a LaaR
        /// </summary>
        /// <param name="g"></param>
        /// <param name="Bounds"></param>
        /// <param name="BackBrush"></param>
        /// <param name="Orientation"></param>
        /// <param name="DrawColor"></param>
        /// <param name="ThicknessOverride"></param>
        public static void DrawLaaR(Graphics g, Rectangle Bounds, Brush BackBrush, enumOrientations Orientation, Color DrawColor, float ThicknessOverride = 1f)
        {
            Point CenterBounds = CenterRect(Bounds);
            using (Pen ThisPen = new Pen(DrawColor, ThicknessOverride))
                if (Orientation == enumOrientations.Up)
                {
                    g.DrawLine(ThisPen, CenterBounds.X, CenterBounds.Y, CenterBounds.X, Bounds.Bottom - 1);
                    g.DrawPolygon(ThisPen, IntToPoints(Bounds.Left, CenterBounds.Y, Bounds.Right - 1, CenterBounds.Y, CenterBounds.X, Bounds.Top, Bounds.Left, CenterBounds.Y));
                    g.DrawLine(ThisPen, Bounds.Left, Bounds.Top, Bounds.Right - 1, Bounds.Bottom - 1);
                }
                else if (Orientation == enumOrientations.Down)
                {
                    g.DrawLine(ThisPen, CenterBounds.X, Bounds.Top, CenterBounds.X, CenterBounds.Y);
                    g.DrawPolygon(ThisPen, IntToPoints(Bounds.Left, CenterBounds.Y, Bounds.Right - 1, CenterBounds.Y, CenterBounds.X, Bounds.Bottom - 1, Bounds.Left, CenterBounds.Y));
                    g.DrawLine(ThisPen, Bounds.Left, Bounds.Top, Bounds.Right - 1, Bounds.Bottom - 1);
                }
                else if (Orientation == enumOrientations.Left)
                {
                    g.DrawLine(ThisPen, CenterBounds.X, CenterBounds.Y, Bounds.Right - 1, CenterBounds.Y);
                    g.DrawPolygon(ThisPen, IntToPoints(CenterBounds.X, Bounds.Top, CenterBounds.X, Bounds.Bottom - 1, 0, CenterBounds.Y, CenterBounds.X, Bounds.Top));
                    g.DrawLine(ThisPen, Bounds.Left, Bounds.Top, Bounds.Right - 1, Bounds.Bottom - 1);
                }
                else if (Orientation == enumOrientations.Right)
                {
                    g.DrawLine(ThisPen, Bounds.Left, CenterBounds.Y, CenterBounds.X, CenterBounds.Y);
                    g.DrawPolygon(ThisPen, IntToPoints(CenterBounds.X, Bounds.Top, CenterBounds.X, Bounds.Bottom - 1, Bounds.Right - 1, CenterBounds.Y, CenterBounds.X, Bounds.Top));
                    g.DrawLine(ThisPen, Bounds.Left, Bounds.Top, Bounds.Right - 1, Bounds.Bottom - 1);
                }
                else if (Orientation == enumOrientations.Horizontal || Orientation == enumOrientations.Unknown)
                {
                    g.DrawPolygon(ThisPen, IntToPoints(Bounds.Left, Bounds.Top, Bounds.Left, Bounds.Bottom - 1, CenterBounds.X, CenterBounds.Y, Bounds.Left, Bounds.Top));
                    g.DrawPolygon(ThisPen, IntToPoints(Bounds.Right - 1, Bounds.Top, Bounds.Right - 1, Bounds.Bottom - 1, CenterBounds.X, CenterBounds.Y, Bounds.Right - 1, Bounds.Top));
                    g.DrawLine(ThisPen, CenterBounds.X / 2, Bounds.Top, CenterBounds.X / 2, Bounds.Bottom - 1);
                    g.DrawLine(ThisPen, (CenterBounds.X / 2) + CenterBounds.X, Bounds.Top, (CenterBounds.X / 2) + CenterBounds.X, Bounds.Bottom - 1);
                }
                else if (Orientation == enumOrientations.Vertical)
                {
                    g.DrawPolygon(ThisPen, IntToPoints(Bounds.Left, Bounds.Top, Bounds.Right - 1, Bounds.Top, CenterBounds.X, CenterBounds.Y, Bounds.Left, Bounds.Top));
                    g.DrawPolygon(ThisPen, IntToPoints(Bounds.Left, Bounds.Bottom - 1, Bounds.Right - 1, Bounds.Bottom - 1, CenterBounds.X, CenterBounds.Y, Bounds.Left, Bounds.Bottom - 1));
                    g.DrawLine(ThisPen, Bounds.Left, CenterBounds.Y / 2, Bounds.Right - 1, CenterBounds.Y / 2);
                    g.DrawLine(ThisPen, Bounds.Left, (CenterBounds.Y / 2) + CenterBounds.Y, Bounds.Right - 1, (CenterBounds.Y / 2) + CenterBounds.Y);
                }
        }



        /// <summary>
        /// Draw a line
        /// </summary>
        /// <param name="g"></param>
        /// <param name="Bounds"></param>
        /// <param name="BackBrush"></param>
        /// <param name="Orientation"></param>
        /// <param name="DrawColor"></param>
        /// <param name="ThicknessOverride"></param>
        public static void DrawLine(Graphics g, Rectangle Bounds, Brush BackBrush, enumOrientations Orientation, Color DrawColor, float ThicknessOverride = 1f)
        {
            Point CenterBounds = CenterRect(Bounds);
            using (Pen ThisPen =  new Pen(DrawColor, ThicknessOverride))
                if (Orientation == enumOrientations.Up)
                {
                    g.DrawLine(ThisPen, CenterBounds.X, CenterBounds.Y, CenterBounds.X, Bounds.Bottom - 1);
                    g.FillPolygon(ThisPen.Brush, IntToPoints(Bounds.Left, CenterBounds.Y, Bounds.Right - 1, CenterBounds.Y, CenterBounds.X, Bounds.Top, Bounds.Left, CenterBounds.Y));
                }
                else if (Orientation == enumOrientations.Down)
                {
                    g.DrawLine(ThisPen, CenterBounds.X, Bounds.Top, CenterBounds.X, CenterBounds.Y);
                    g.FillPolygon(ThisPen.Brush, IntToPoints(Bounds.Left, CenterBounds.Y, Bounds.Right - 1, CenterBounds.Y, CenterBounds.X, Bounds.Bottom - 1, Bounds.Left, CenterBounds.Y));
                }
                else if (Orientation == enumOrientations.Left)
                {
                    g.DrawLine(ThisPen, CenterBounds.X, CenterBounds.Y, Bounds.Right - 1, CenterBounds.Y);
                    g.FillPolygon(ThisPen.Brush, IntToPoints(CenterBounds.X, Bounds.Top, CenterBounds.X, Bounds.Bottom - 1, Bounds.Left, CenterBounds.Y, CenterBounds.X, Bounds.Top));
                }
                else if (Orientation == enumOrientations.Right)
                {
                    g.DrawLine(ThisPen, Bounds.Left, CenterBounds.Y, CenterBounds.X, CenterBounds.Y);
                    g.FillPolygon(ThisPen.Brush, IntToPoints(CenterBounds.X, Bounds.Left, CenterBounds.X, Bounds.Bottom - 1, Bounds.Right - 1, CenterBounds.Y, CenterBounds.X, Bounds.Top));
                }
                else if (Orientation == enumOrientations.Horizontal || Orientation == enumOrientations.Unknown)
                {
                    g.FillPolygon(ThisPen.Brush, IntToPoints(Bounds.Left, Bounds.Top, Bounds.Left, Bounds.Bottom - 1, CenterBounds.X, CenterBounds.Y, Bounds.Left, Bounds.Top));
                    g.FillPolygon(ThisPen.Brush, IntToPoints(Bounds.Right - 1, Bounds.Top, Bounds.Right - 1, Bounds.Bottom - 1, CenterBounds.X, CenterBounds.Y, Bounds.Right - 1, Bounds.Top));
                    g.DrawLine(ThisPen, Bounds.Right - 1, Bounds.Top, Bounds.Right - 1, Bounds.Bottom - 1);
                }
                else if (Orientation == enumOrientations.Vertical)
                {
                    g.FillPolygon(ThisPen.Brush, IntToPoints(Bounds.Left, Bounds.Top, Bounds.Right - 1, Bounds.Top, CenterBounds.X, CenterBounds.Y, Bounds.Left, Bounds.Top));
                    g.FillPolygon(ThisPen.Brush, IntToPoints(Bounds.Left, Bounds.Bottom - 1, Bounds.Right - 1, Bounds.Bottom - 1, CenterBounds.X, CenterBounds.Y, Bounds.Left, Bounds.Bottom - 1));
                    g.DrawLine(ThisPen, Bounds.Left, Bounds.Bottom - 1, Bounds.Right - 1, Bounds.Bottom - 1);
                }
        }

        /// <summary>
        /// Draw a load
        /// </summary>
        /// <param name="g"></param>
        /// <param name="Bounds"></param>
        /// <param name="BackBrush"></param>
        /// <param name="Orientation"></param>
        /// <param name="DrawColor"></param>
        /// <param name="ThicknessOverride"></param>
        public static void DrawLoad(Graphics g, Rectangle Bounds, Brush BackBrush, enumOrientations Orientation, Color DrawColor, float ThicknessOverride = 1f)
        {
            Point CenterBounds = CenterRect(Bounds);
            using (Pen ThisPen =new Pen(DrawColor, ThicknessOverride))
                if (Orientation == enumOrientations.Up)
                {
                    g.DrawLine(ThisPen, CenterBounds.X, CenterBounds.Y, CenterBounds.X, Bounds.Bottom - 1);
                    g.DrawPolygon(ThisPen, IntToPoints(Bounds.Left, CenterBounds.Y, Bounds.Right - 1, CenterBounds.Y, CenterBounds.X, Bounds.Top, Bounds.Left, CenterBounds.Y));
                }
                else if (Orientation == enumOrientations.Down)
                {
                    g.DrawLine(ThisPen, CenterBounds.X, Bounds.Top, CenterBounds.X, CenterBounds.Y);
                    g.DrawPolygon(ThisPen, IntToPoints(Bounds.Left, CenterBounds.Y, Bounds.Right - 1, CenterBounds.Y, CenterBounds.X, Bounds.Bottom - 1, Bounds.Left, CenterBounds.Y));
                }
                else if (Orientation == enumOrientations.Left)
                {
                    g.DrawLine(ThisPen, CenterBounds.X, CenterBounds.Y, Bounds.Right - 1, CenterBounds.Y);
                    g.DrawPolygon(ThisPen, IntToPoints(CenterBounds.X, Bounds.Top, CenterBounds.X, Bounds.Bottom - 1, Bounds.Left, CenterBounds.Y, CenterBounds.X, Bounds.Top));
                }
                else if (Orientation == enumOrientations.Right)
                {
                    g.DrawLine(ThisPen, Bounds.Left, CenterBounds.Y, CenterBounds.X, CenterBounds.Y);
                    g.DrawPolygon(ThisPen, IntToPoints(CenterBounds.X, Bounds.Top, CenterBounds.X, Bounds.Bottom - 1, Bounds.Right - 1, CenterBounds.Y, CenterBounds.X, Bounds.Top));
                }
                else if (Orientation == enumOrientations.Horizontal || Orientation == enumOrientations.Unknown)
                {
                    g.DrawPolygon(ThisPen, IntToPoints(Bounds.Left, Bounds.Top, Bounds.Left, Bounds.Bottom - 1, CenterBounds.X, CenterBounds.Y, Bounds.Left, Bounds.Top));
                    g.DrawPolygon(ThisPen, IntToPoints(Bounds.Right - 1, Bounds.Top, Bounds.Right - 1, Bounds.Bottom - 1, CenterBounds.X, CenterBounds.Y, Bounds.Right - 1, Bounds.Top));
                }
                else if (Orientation == enumOrientations.Vertical)
                {
                    g.DrawPolygon(ThisPen, IntToPoints(Bounds.Left, Bounds.Top, Bounds.Right - 1, Bounds.Top, CenterBounds.X, CenterBounds.Y, Bounds.Left, Bounds.Top));
                    g.DrawPolygon(ThisPen, IntToPoints(Bounds.Left, Bounds.Bottom - 1, Bounds.Right - 1, Bounds.Bottom - 1, CenterBounds.X, CenterBounds.Y, Bounds.Left, Bounds.Bottom - 1));
                }
        }

        /// <summary>
        /// Build a path from a node out to a target element
        /// </summary>
        /// <param name="Node"></param>
        /// <param name="TargetElement"></param>
        /// <param name="PokePoints"></param>
        /// <returns></returns>
        public static GraphicsPath BuildNodePath(MM_OneLine_Element Node, MM_OneLine_Element TargetElement, MM_OneLine_Node[] PokePoints)
        {
            //First, if our target element is a TEID reference, convert it                       
            Point Center1, Center2;
            GraphicsPath OutPath = new GraphicsPath();

            //Now, go through all child nodes
            Rectangle TargetRect = Node.Bounds;            
            bool LastNode = true;
            foreach (MM_OneLine_Node Elem in PokePoints)
                {
                    Rectangle Bounds2 = Elem.Bounds;
                    bool IsJumper = Elem.ElemType == enumElemTypes.PricingVector || Elem.IsJumper;
                    GetStraightLine(TargetRect, Bounds2, out Center1, out Center2, LastNode && !IsJumper, LastNode = Elem.ElemType == enumElemTypes.PokePoint && !IsJumper);
                    OutPath.AddLine(Center1, Center2);
                    OutPath.CloseFigure();
                    TargetRect = Bounds2;
                }

            //Now, draw our final line            
            GetStraightLine(TargetRect, TargetElement.Bounds, out Center1, out Center2, LastNode, false);
            OutPath.AddLine(Center1, Center2);
            OutPath.CloseFigure();
            return OutPath;
        }


        /// <summary>
        /// Draw a poke point/jumper
        /// </summary>
        /// <param name="g"></param>
        /// <param name="Bounds"></param>
        /// <param name="BackBrush"></param>
        /// <param name="Orientation"></param>
        /// <param name="DrawColor"></param>
        /// <param name="IsJumper"></param>
        /// <param name="ThicknessOverride"></param>
        public static void DrawPokePoint(Graphics g, Rectangle Bounds, Brush BackBrush, enumOrientations Orientation, Color DrawColor, bool IsJumper, float ThicknessOverride = 1f)
        {

            if (Bounds.Size == new Size(4, 4))
                return;
            Point CenterBounds = CenterRect(Bounds);

            using (Pen pn =  new Pen(DrawColor, ThicknessOverride))
                if (IsJumper && (Orientation == enumOrientations.Horizontal || Orientation == enumOrientations.Left || Orientation == enumOrientations.Right || Orientation == enumOrientations.Unknown))
                {
                    int JumperSize = (int)Math.Ceiling((float)Bounds.Width / 4f);
                    //g.FillRectangle(BackBrush, Bounds.Left + JumperSize, CenterBounds.Y - 1, Bounds.Width - (JumperSize * 2), Bounds.Height - 1);
                    g.DrawArc(pn, Bounds.Left + JumperSize, Bounds.Top + 1, Bounds.Width - (JumperSize * 2), Bounds.Height - 2, 0, 180);
                    g.DrawLine(pn, Bounds.Left, CenterBounds.Y, Bounds.Left + JumperSize, CenterBounds.Y);
                    g.DrawLine(pn, Bounds.Right + 1 - JumperSize, CenterBounds.Y, Bounds.Right - 1, CenterBounds.Y);
                }
                else if (IsJumper && (Orientation == enumOrientations.Vertical || Orientation == enumOrientations.Up || Orientation == enumOrientations.Down))
                {
                    int JumperSize = (int)Math.Ceiling((float)Bounds.Height / 4f);
                    //g.FillRectangle(BackBrush, Bounds.Left, Bounds.Top + JumperSize, Bounds.Width - 1, Bounds.Height - (JumperSize * 2));
                    g.DrawArc(pn, Bounds.Left, Bounds.Top + JumperSize, Bounds.Width - 1, Bounds.Height - (JumperSize * 2), 90, 180);
                    g.DrawLine(pn, CenterBounds.X, Bounds.Top, CenterBounds.X, Bounds.Top + JumperSize);
                    g.DrawLine(pn, CenterBounds.X, Bounds.Bottom + 1 - JumperSize, CenterBounds.X, Bounds.Bottom - 1);
                }
                else
                    g.FillRectangle(pn.Brush, Bounds);
        }

        /// <summary>
        /// Draw a pricing vector
        /// </summary>
        /// <param name="g"></param>
        /// <param name="Bounds"></param>
        /// <param name="BackBrush"></param>
        /// <param name="Orientation"></param>
        /// <param name="DrawColor"></param>
        /// <param name="IsPositive"></param>
        /// <param name="ThicknessOverride"></param>
        public static void DrawPricingVector(Graphics g, Rectangle Bounds, Brush BackBrush, enumOrientations Orientation, Color DrawColor, bool IsPositive, float ThicknessOverride = 1f)
        {
            //First, draw our rectangle
            g.FillRectangle(BackBrush, Bounds.Left, Bounds.Top, Bounds.Width - 1, Bounds.Height - 1);
            using (Pen DrawPen = new Pen(DrawColor, ThicknessOverride))
                g.DrawRectangle(DrawPen, Bounds.Left, Bounds.Top, Bounds.Width - 1, Bounds.Height - 1);

            //Then, draw our arrow as appropriate
            using (Pen p = new Pen(DrawColor, ThicknessOverride * 3f))
            {
                if (IsPositive)
                    p.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
                else
                    p.StartCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;


                if (Orientation == enumOrientations.Left || Orientation == enumOrientations.Horizontal || Orientation == enumOrientations.Unknown)
                    g.DrawLine(p, Bounds.Right - 3, Bounds.Top + (Bounds.Height / 2), Bounds.Left + 3, Bounds.Top + (Bounds.Height / 2));
                else if (Orientation == enumOrientations.Right)
                    g.DrawLine(p, Bounds.Left + 3, Bounds.Top + (Bounds.Height / 2), Bounds.Right - 3, Bounds.Top + (Bounds.Height / 2));
                else if (Orientation == enumOrientations.Up || Orientation == enumOrientations.Vertical)
                    g.DrawLine(p, Bounds.Left + (Bounds.Width / 2), Bounds.Bottom - 3, Bounds.Left + (Bounds.Width / 2), Bounds.Top + 3);
                else if (Orientation == enumOrientations.Down)
                    g.DrawLine(p, Bounds.Left + (Bounds.Width / 2), Bounds.Top + 3, Bounds.Left + (Bounds.Width / 2), Bounds.Bottom - 3);
            }
        }

        /// <summary>
        /// Draw a resistor
        /// </summary>
        /// <param name="g"></param>
        /// <param name="Bounds"></param>
        /// <param name="BackBrush"></param>
        /// <param name="Orientation"></param>
        /// <param name="DrawColor"></param>
        /// <param name="GroundBar"></param>
        /// <param name="ThicknessOverride"></param>
        public static void DrawResistor(Graphics g, Rectangle Bounds, Brush BackBrush, enumOrientations Orientation, Color DrawColor, bool GroundBar, float ThicknessOverride=1f)
        {
            Point CenterPt = CenterRect(Bounds);

            Point DeltaPt = new Point(Bounds.Width / 6, Bounds.Height / 6);
            List<Point> OutPoints = new List<Point>();
            int Cur = 0;
            if (Orientation == enumOrientations.Vertical || Orientation == enumOrientations.Unknown || Orientation == enumOrientations.Up || Orientation == enumOrientations.Down)
            {
                OutPoints.Add(new Point(CenterPt.X, Bounds.Top));
                for (int a = Bounds.Top + 4; a < Bounds.Bottom - 3; a += 2)
                {
                    if (Cur == 0)
                        OutPoints.Add(new Point(CenterPt.X, a));
                    else if (Cur == 1)
                        OutPoints.Add(new Point(CenterPt.X - DeltaPt.X, a));
                    else
                        OutPoints.Add(new Point(CenterPt.X + DeltaPt.X, a));
                    Cur = (Cur + 1) % 3;
                }
                OutPoints.Add(new Point(CenterPt.X, Bounds.Bottom - 1));

            }
            else
            {
                OutPoints.Add(new Point(Bounds.Left, CenterPt.Y));
                for (int a = Bounds.Left + 4; a < Bounds.Right - 3; a += 2)
                {
                    if (Cur == 0)
                        OutPoints.Add(new Point(a, CenterPt.Y));
                    else if (Cur == 1)
                        OutPoints.Add(new Point(a, CenterPt.Y - DeltaPt.Y));
                    else
                        OutPoints.Add(new Point(a, CenterPt.Y + DeltaPt.Y));
                    Cur = (Cur + 1) % 3;
                }
                OutPoints.Add(new Point(Bounds.Right - 1, CenterPt.Y));
            }
            using (Pen ThisPen = new Pen(DrawColor, ThicknessOverride))
                g.DrawLines(ThisPen, OutPoints.ToArray());
            if (GroundBar)
                DrawGroundBar(g, Bounds, BackBrush, Orientation, DrawColor);

        }

        /// <summary>
        /// Draw a reactor
        /// </summary>
        /// <param name="g"></param>
        /// <param name="Bounds"></param>
        /// <param name="BackBrush"></param>
        /// <param name="Orientation"></param>
        /// <param name="DrawColor"></param>
        /// <param name="GroundBar"></param>
        /// <param name="ThicknessOverride"></param>
        public static void DrawReactor(Graphics g, Rectangle Bounds, Brush BackBrush, enumOrientations Orientation, Color DrawColor, bool GroundBar, float ThicknessOverride = 1f)
        {
            Point[] OutPoints = IntToPoints(0, 12, 1, 12, 2, 13, 3, 15, 4, 18, 5, 20, 6, 22, 7, 23, 8, 24, 10, 24, 12, 23, 14, 21, 15, 19, 16, 16, 17, 12, 17, 7, 16, 4, 15, 2, 12, 0, 10, 0, 8, 1, 6, 3, 5, 6, 5, 11, 7, 17, 8, 19, 10, 22, 11, 23, 13, 24, 15, 24, 16, 24, 18, 23, 19, 22, 21, 19, 22, 16, 23, 12, 23, 7, 22, 4, 21, 2, 18, 0, 16, 0, 14, 1, 13, 2, 12, 3, 11, 5, 10, 8, 10, 12, 11, 15, 13, 19, 15, 22, 18, 24, 20, 24, 22, 23, 23, 22, 25, 19, 26, 16, 27, 13, 28, 12, 30, 12, 31, 12);
            for (int a = 0; a < OutPoints.Length; a++)
                if (Orientation == enumOrientations.Vertical || Orientation == enumOrientations.Up || Orientation == enumOrientations.Down)
                    OutPoints[a] = new Point(OutPoints[a].Y + Bounds.Left, OutPoints[a].X + Bounds.Top);
                else
                    OutPoints[a] = new Point(OutPoints[a].X + Bounds.Left, OutPoints[a].Y + Bounds.Top);
            //using (SolidBrush BackBrush = new SolidBrush(BackColor))
            //    g.FillRectangle(BackBrush, Bounds);
            using (Pen ThisPen = new Pen(DrawColor, ThicknessOverride))
                g.DrawLines(ThisPen, OutPoints);
          //  if (GroundBar)
            //    DrawGroundBar(g, Bounds, BackBrush, Orientation, DrawColor);

        }

        /// <summary>
        /// Draw a resistor/reactor hybrid
        /// </summary>
        /// <param name="g"></param>
        /// <param name="Bounds"></param>
        /// <param name="BackBrush"></param>
        /// <param name="Orientation"></param>
        /// <param name="DrawColor"></param>
        public static void DrawResistorReactor(Graphics g, Rectangle Bounds, Brush BackBrush, enumOrientations Orientation, Color DrawColor)
        {
            if (Orientation == enumOrientations.Up || Orientation == enumOrientations.Vertical || Orientation == enumOrientations.Down || Orientation == enumOrientations.Unknown)
            {
                DrawReactor(g, new Rectangle(Bounds.Left, Bounds.Top, Bounds.Width, Bounds.Height / 2), BackBrush, Orientation, DrawColor, false);
                DrawResistor(g, new Rectangle(Bounds.Left, Bounds.Top + (Bounds.Height / 2), Bounds.Width, Bounds.Height / 2), BackBrush, Orientation, DrawColor, false);
            }
            else
            {
                DrawReactor(g, new Rectangle(Bounds.Left, Bounds.Top, Bounds.Width, Bounds.Height), BackBrush, Orientation, DrawColor, false);
                DrawResistor(g, new Rectangle(Bounds.Left + (Bounds.Width / 2), Bounds.Top, Bounds.Width / 2, Bounds.Height), BackBrush, Orientation, DrawColor, false);
            }
            DrawGroundBar(g, Bounds, BackBrush, Orientation, DrawColor);
        }

        /// <summary>
        /// Draw a resistor/reactor hybrid
        /// </summary>
        /// <param name="g"></param>
        /// <param name="Bounds"></param>
        /// <param name="BackBrush"></param>
        /// <param name="Orientation"></param>
        /// <param name="DrawColor"></param>
        public static void DrawResistorCapacitor(Graphics g, Rectangle Bounds, Brush BackBrush, enumOrientations Orientation, Color DrawColor)
        {
            if (Orientation == enumOrientations.Up || Orientation == enumOrientations.Vertical || Orientation == enumOrientations.Down || Orientation == enumOrientations.Unknown)
            {
                DrawCapacitor(g, new Rectangle(Bounds.Left, Bounds.Top, Bounds.Width, Bounds.Height / 2), BackBrush, Orientation, DrawColor, false);
                DrawResistor(g, new Rectangle(Bounds.Left, Bounds.Top + (Bounds.Height / 2), Bounds.Width, Bounds.Height / 2), BackBrush, Orientation, DrawColor, false);
            }
            else
            {
                DrawCapacitor(g, new Rectangle(Bounds.Left, Bounds.Top, Bounds.Width, Bounds.Height), BackBrush, Orientation, DrawColor, false);
                DrawResistor(g, new Rectangle(Bounds.Left + (Bounds.Width / 2), Bounds.Top, Bounds.Width / 2, Bounds.Height), BackBrush, Orientation, DrawColor, false);
            }
            DrawGroundBar(g, Bounds, BackBrush, Orientation, DrawColor);
        }

        /// <summary>
        /// Draw a ground bar
        /// </summary>
        /// <param name="g"></param>
        /// <param name="Bounds"></param>
        /// <param name="BackBrush"></param>
        /// <param name="Orientation"></param>
        /// <param name="DrawColor"></param>
        public static void DrawGroundBar(Graphics g, Rectangle Bounds, Brush BackBrush, enumOrientations Orientation, Color DrawColor)
        {
            //Draw our ground bar
            Point Center = CenterRect(Bounds);
            using (Pen DrawPen = new Pen(DrawColor))
            if (Orientation == enumOrientations.Up)
                g.DrawLine(DrawPen, Center.X, Center.Y, Center.X, Center.Y + 5);
            else if (Orientation == enumOrientations.Down)
            {
                g.DrawLine(DrawPen, Center.X, Center.Y, Center.X, Center.Y + 5);
                int HalfWidth = Bounds.Width / 2;
                for (int a = 2; a < 5; a++)
                    g.DrawLine(DrawPen, Center.X - HalfWidth + (a * 2), Center.Y + (a * 3) - 1, Center.X + HalfWidth - (a * 2), Center.Y + (a * 3) - 1);
            }
            else if (Orientation == enumOrientations.Left)
                g.DrawLine(DrawPen, Bounds.Left, Bounds.Top, Bounds.Left, Bounds.Bottom - 1);
            else if (Orientation == enumOrientations.Right)
                g.DrawLine(DrawPen, Bounds.Right - 1, Bounds.Top, Bounds.Right - 1, Bounds.Bottom - 1);

        }


        /// <summary>
        /// Draw a static var compensator
        /// </summary>
        /// <param name="g"></param>
        /// <param name="Bounds"></param>
        /// <param name="BackBrush"></param>
        /// <param name="Orientation"></param>
        /// <param name="DrawColor"></param>
        /// <param name="ThicknessOverride"></param>
        public static void DrawStaticVarCompensator(Graphics g, Rectangle Bounds, Brush BackBrush, enumOrientations Orientation, Color DrawColor, float ThicknessOverride=1f)
        {
            using (Pen DrawPen = new Pen(DrawColor, ThicknessOverride))
            {
                //g.FillRectangle(Brushes.Black, this.DisplayRectangle);
                if (Orientation == enumOrientations.Up || Orientation == enumOrientations.Vertical)
                {
                    List<Point> OutPoint = new List<Point>();
                    foreach (Point pt in IntToPoints(18, 0, 18, 1, 16, 2, 14, 3, 12, 4, 9, 6, 8, 7, 7, 8, 6, 10, 6, 12, 7, 13, 8, 14, 10, 15, 13, 16, 16, 17, 19, 17, 22, 17, 26, 16, 28, 15, 30, 14, 31, 12, 31, 10, 30, 8, 29, 7, 28, 6, 26, 5, 23, 4, 20, 4, 17, 5, 15, 6, 13, 7, 10, 9, 8, 11, 7, 12, 6, 14, 6, 16, 8, 18, 12, 20, 16, 21, 17, 21, 20, 21, 24, 20, 27, 19, 30, 17, 31, 15, 31, 13, 30, 11, 28, 10, 26, 9, 23, 8, 21, 8, 19, 8, 17, 9, 15, 10, 13, 11, 10, 13, 8, 15, 7, 16, 6, 17, 6, 19, 7, 20, 8, 21, 9, 22, 11, 23, 13, 24, 16, 25, 18, 25, 18, 26, 18, 35))
                        OutPoint.Add(new Point(pt.X + Bounds.Left, pt.Y + Bounds.Top));
                    g.DrawLines(DrawPen, OutPoint.ToArray());
                    g.DrawLine(DrawPen, Bounds.Left + 63, Bounds.Top + 2, Bounds.Left + 63, Bounds.Top + 8);
                    g.DrawLine(DrawPen, Bounds.Left + 55, Bounds.Top + 8, Bounds.Left + 71, Bounds.Top + 8);
                    g.DrawLine(DrawPen, Bounds.Left + 63, Bounds.Top + 10, Bounds.Left + 63, Bounds.Top + 35);
                    g.DrawArc(DrawPen, Bounds.Left + 55, Bounds.Top + 8, 16, 16, 180, 180);
                    g.DrawLine(DrawPen, Bounds.Left + 18, Bounds.Top + 35, Bounds.Left + 63, Bounds.Top + 35);
                    g.DrawLine(DrawPen, Bounds.Left + (Bounds.Width / 2), Bounds.Top + 35, Bounds.Left + Bounds.Width / 2, Bounds.Bottom);
                }
                else if (Orientation == enumOrientations.Down)
                {
                    List<Point> OutPoint = new List<Point>();
                    foreach (Point pt in IntToPoints(62, 36, 62, 42, 60, 43, 58, 44, 56, 45, 53, 47, 52, 48, 51, 49, 50, 51, 50, 53, 51, 54, 52, 55, 54, 56, 57, 57, 60, 58, 63, 58, 66, 58, 70, 57, 72, 56, 74, 55, 75, 53, 75, 51, 74, 49, 73, 48, 72, 47, 70, 46, 67, 45, 64, 45, 61, 46, 59, 47, 57, 48, 54, 50, 52, 52, 51, 53, 50, 55, 50, 57, 52, 59, 56, 61, 60, 62, 61, 62, 64, 62, 68, 61, 71, 60, 74, 58, 75, 56, 75, 54, 74, 52, 72, 51, 70, 50, 67, 49, 65, 49, 63, 49, 61, 50, 59, 51, 57, 52, 54, 54, 52, 56, 51, 57, 50, 58, 50, 60, 51, 61, 52, 62, 53, 63, 55, 64, 57, 65, 60, 66, 62, 66, 62, 67, 62, 69))
                        OutPoint.Add(new Point(pt.X + Bounds.Left, pt.Y + Bounds.Top));
                    g.DrawLines(DrawPen, OutPoint.ToArray());

                    g.DrawLine(DrawPen, Bounds.Left + 21, Bounds.Top + 36, Bounds.Left + 21, Bounds.Top + 48);
                    g.DrawLine(DrawPen, Bounds.Left + 13, Bounds.Top + 48, Bounds.Left + 29, Bounds.Top + 48);
                    g.DrawLine(DrawPen, Bounds.Left + 21, Bounds.Top + 50, Bounds.Left + 21, Bounds.Top + 66);
                    g.DrawArc(DrawPen, Bounds.Left + 13, Bounds.Top + 50, 16, 16, 180, 180);
                    g.DrawLine(DrawPen, Bounds.Left + 21, Bounds.Top + 36, Bounds.Left + 62, Bounds.Top + 36);
                    g.DrawLine(DrawPen, Bounds.Left + (Bounds.Width / 2), Bounds.Top, Bounds.Left + (Bounds.Width / 2), Bounds.Top + 36);
                }
                else if (Orientation == enumOrientations.Left || Orientation == enumOrientations.Horizontal || Orientation == enumOrientations.Unknown)
                {
                    List<Point> OutPoint = new List<Point>();
                    foreach (Point pt in IntToPoints(35, 27, 40, 27, 41, 28, 42, 30, 43, 33, 44, 35, 45, 37, 46, 38, 47, 39, 49, 39, 51, 38, 53, 36, 54, 34, 55, 31, 56, 27, 56, 22, 55, 19, 54, 17, 51, 15, 49, 15, 47, 16, 45, 18, 44, 21, 44, 26, 46, 32, 47, 34, 49, 37, 50, 38, 52, 39, 54, 39, 52, 32, 57, 38, 58, 37, 60, 34, 61, 31, 62, 27, 62, 22, 61, 19, 60, 17, 57, 15, 55, 15, 53, 16, 52, 17, 51, 18, 50, 20, 49, 23, 49, 27, 50, 30, 52, 34, 54, 37, 57, 39, 59, 39, 61, 38, 62, 37, 64, 34, 65, 31, 66, 28, 67, 27, 69, 27, 71, 27))
                        OutPoint.Add(new Point(pt.X + Bounds.Left, pt.Y + Bounds.Top));
                    g.DrawLines(DrawPen, OutPoint.ToArray());

                    g.DrawLine(DrawPen, Bounds.Left + 62, Bounds.Top + 53, Bounds.Left + 56, Bounds.Top + 53);
                    g.DrawLine(DrawPen, Bounds.Left + 56, Bounds.Top + 45, Bounds.Left + 56, Bounds.Top + 61);
                    g.DrawLine(DrawPen, Bounds.Left + 54, Bounds.Top + 53, Bounds.Left + 35, Bounds.Top + 53);
                    g.DrawArc(DrawPen, Bounds.Left + 38, Bounds.Top + 45, 16, 16, 270, 180);
                    g.DrawLine(DrawPen, Bounds.Left + 35, Bounds.Top + 27, Bounds.Left + 35, Bounds.Top + 53);
                    g.DrawLine(DrawPen, Bounds.Left, Bounds.Top + (Bounds.Height / 2), Bounds.Left + 35, Bounds.Top + (Bounds.Height / 2));
                }
                else if (Orientation == enumOrientations.Right)
                {
                    List<Point> OutPoint = new List<Point>();
                    foreach (Point pt in IntToPoints(1, 18, 2, 18, 3, 19, 4, 21, 5, 24, 6, 26, 7, 28, 8, 29, 9, 30, 11, 30, 13, 29, 15, 27, 16, 25, 17, 22, 18, 18, 18, 13, 17, 10, 16, 8, 13, 6, 11, 6, 9, 7, 7, 9, 6, 12, 6, 17, 8, 23, 9, 25, 11, 28, 12, 29, 14, 30, 16, 30, 17, 30, 19, 29, 20, 28, 22, 25, 23, 22, 24, 18, 24, 13, 23, 10, 22, 8, 19, 6, 17, 6, 15, 7, 14, 8, 13, 9, 12, 11, 11, 14, 11, 18, 12, 21, 14, 25, 16, 28, 19, 30, 21, 30, 23, 29, 24, 28, 26, 25, 27, 22, 28, 19, 29, 18, 31, 18, 33, 18))
                        OutPoint.Add(new Point(pt.X + Bounds.Left, pt.Y + Bounds.Top));
                    g.DrawLines(DrawPen, OutPoint.ToArray());


                    g.DrawLine(DrawPen, Bounds.Left + 13, Bounds.Top + 58, Bounds.Left + 7, Bounds.Top + 58);
                    g.DrawLine(DrawPen, Bounds.Left + 13, Bounds.Top + 50, Bounds.Left + 13, Bounds.Top + 66);
                    g.DrawLine(DrawPen, Bounds.Left + 33, Bounds.Top + 58, Bounds.Left + 15, Bounds.Top + 58);
                    g.DrawArc(DrawPen, Bounds.Left + 15, Bounds.Top + 50, 16, 16, 90, 180);
                    g.DrawLine(DrawPen, Bounds.Left + 34, Bounds.Top + 18, Bounds.Left + 34, Bounds.Top + 58);
                    g.DrawLine(DrawPen, Bounds.Left + 34, Bounds.Top + (Bounds.Height / 2), Bounds.Right, Bounds.Top + (Bounds.Height / 2));

                }
            }
        }


        /// <summary>
        /// Draw a transformer winding
        /// </summary>
        /// <param name="g"></param>
        /// <param name="Bounds"></param>
        /// <param name="BackBrush"></param>
        /// <param name="Orientation"></param>
        /// <param name="DrawColor"></param>
        /// <param name="Visible"></param>
        /// <param name="IsPhaseShifter"></param>
        /// <param name="ThicknessOverride"></param>
        public static void DrawTransformerWinding(Graphics g, Rectangle Bounds, Brush BackBrush, enumOrientations Orientation, Color DrawColor, bool Visible, bool IsPhaseShifter, float ThicknessOverride=1f)
        {
            if (!Visible)
                return;
            using (Pen ThisPen = new Pen(DrawColor, ThicknessOverride))
                if (IsPhaseShifter)
                {
                    Point Center = MM_OneLine_Element.CenterRect(Bounds);
                    g.DrawEllipse(ThisPen, Center.X - 5, Center.Y - 5, 10, 10);
                    using (Pen NewPen = new Pen(ThisPen.Color))
                    {
                        NewPen.EndCap = LineCap.ArrowAnchor;
                        if (Orientation == enumOrientations.Up)
                            g.DrawLine(NewPen, Bounds.Left, Bounds.Bottom, Bounds.Right, Bounds.Top);
                        else if (Orientation == enumOrientations.Right)
                            g.DrawLine(NewPen, Bounds.Left, Bounds.Top, Bounds.Right, Bounds.Bottom);
                        else if (Orientation == enumOrientations.Left)
                            g.DrawLine(NewPen, Bounds.Right, Bounds.Bottom, Bounds.Left, Bounds.Top);
                        else
                            g.DrawLine(NewPen, Bounds.Right, Bounds.Top, Bounds.Left, Bounds.Bottom);
                    }
                }
                else
                    for (int a = 0; a <= 24; a += 6)
                        if (Orientation == enumOrientations.Down)
                        {
                            g.DrawLine(ThisPen, a + Bounds.Left, 3 + Bounds.Top, a + Bounds.Left, (a == 12 ? 12 : 9) + Bounds.Top);
                            if (a <= 18)
                                g.DrawArc(ThisPen, a + Bounds.Left, Bounds.Top, 6, 6, 180, 180);
                        }
                        else if (Orientation == enumOrientations.Up)
                        {
                            g.DrawLine(ThisPen, Bounds.Left + a, Bounds.Top + (a == 12 ? 0 : 3), Bounds.Left + a, Bounds.Top + 9);
                            if (a <= 18)
                                g.DrawArc(ThisPen, Bounds.Left + a, Bounds.Top + 6, 6, 6, 0, 180);
                        }
                        else if (Orientation == enumOrientations.Left)
                        {
                            g.DrawLine(ThisPen, Bounds.Left + 3, Bounds.Top + a, Bounds.Left + (a == 12 ? 12 : 9), Bounds.Top + a);
                            if (a <= 18)
                                g.DrawArc(ThisPen, Bounds.Left, Bounds.Top + a, 6, 6, 90, 180);
                        }
                        else if (Orientation == enumOrientations.Right)
                        {
                            g.DrawLine(ThisPen, Bounds.Left + (a == 12 ? 0 : 3), Bounds.Top + a, Bounds.Left + 9, Bounds.Top + a);
                            if (a <= 18)
                                g.DrawArc(ThisPen, Bounds.Left + 6, Bounds.Top + a, 6, 6, 270, 180);
                        }
        }

        /// <summary>
        /// Draw a unit
        /// </summary>
        /// <param name="g"></param>
        /// <param name="Bounds"></param>
        /// <param name="BackBrush"></param>
        /// <param name="Orientation"></param>
        /// <param name="DrawColor"></param>
        /// <param name="GenerationLevel"></param>
        /// <param name="ThicknessOverride"></param>
        public static void DrawUnit(Graphics g, Rectangle Bounds, Brush BackBrush, enumOrientations Orientation, Color DrawColor, float GenerationLevel,float ThicknessOverride = 1f)
        {
            Point Center = CenterRect(Bounds);
            using (Pen ThisPen = new Pen(DrawColor, ThicknessOverride))
            if (Orientation == enumOrientations.Up || Orientation == enumOrientations.Vertical)
            {
                g.DrawLine(ThisPen, Center.X - 3, Bounds.Bottom - 4, Center.X + 3, Bounds.Bottom - 4);
                g.DrawLine(ThisPen, Center.X, Center.Y, Center.X, Bounds.Bottom - 1);
                g.FillPie(BackBrush, Bounds.Left, Center.Y - Bounds.Height, Bounds.Width - 1, Bounds.Height - 1, 0, 180);
                g.DrawLine(ThisPen, Bounds.Left, Bounds.Top, Bounds.Right - 1, Bounds.Top);
                g.DrawArc(ThisPen, Bounds.Left, Center.Y - Bounds.Height, Bounds.Width - 1, Bounds.Height - 1, 0, 180);
                g.FillPie(ThisPen.Brush, Bounds.Left, Center.Y - Bounds.Height, Bounds.Width - 1, Bounds.Height - 1, 0, 180f * GenerationLevel);
            }
            else if (Orientation == enumOrientations.Down)
            {
                g.DrawLine(ThisPen, Center.X - 3, Bounds.Top + 3, Center.X + 3, Bounds.Top + 3);
                g.DrawLine(ThisPen, Center.X, Bounds.Top, Center.X, Center.Y);
                g.FillPie(BackBrush, Bounds.Left, Center.Y, Bounds.Width - 1, Bounds.Height - 1, 180, 180);
                g.DrawLine(ThisPen, Bounds.Left, Bounds.Bottom - 1, Bounds.Right - 1, Bounds.Bottom - 1);
                g.DrawArc(ThisPen, Bounds.Left, Center.Y, Bounds.Width - 1, Bounds.Height - 1, 180, 180);
                g.FillPie(ThisPen.Brush, Bounds.Left, Center.Y, Bounds.Width - 1, Bounds.Height - 1, 180, 180f * GenerationLevel);

            }
            else if (Orientation == enumOrientations.Left || Orientation == enumOrientations.Horizontal || Orientation == enumOrientations.Unknown)
            {
                g.DrawLine(ThisPen, Bounds.Right - 4, Center.Y - 3, Bounds.Right - 4, Center.Y + 3);
                g.DrawLine(ThisPen, Center.X + 1, Center.Y, Bounds.Right - 1, Center.Y);
                g.FillPie(BackBrush, Center.X - Bounds.Width, Bounds.Top, Bounds.Width - 1, Bounds.Height - 1, 270, 180);
                g.DrawLine(ThisPen, Bounds.Left, Bounds.Top, Bounds.Left, Bounds.Bottom - 1);
                g.DrawArc(ThisPen, Center.X - Bounds.Width, Bounds.Top, Bounds.Width - 1, Bounds.Height - 1, 270, 180);
                g.FillPie(ThisPen.Brush, Center.X - Bounds.Width, Bounds.Top, Bounds.Width - 1, Bounds.Height - 1, 270, 180f * GenerationLevel);
            }
            else if (Orientation == enumOrientations.Right)
            {
                g.DrawLine(ThisPen, Bounds.Left + 3, Center.Y - 3, Bounds.Left + 3, Center.Y + 3);
                g.DrawLine(ThisPen, Bounds.Left, Center.Y, Center.X, Center.Y);
                g.FillPie(BackBrush, Center.X, Bounds.Left, Bounds.Width - 1, Bounds.Height - 1, 90, 180);
                g.DrawLine(ThisPen, Bounds.Right - 1, Bounds.Top, Bounds.Right - 1, Bounds.Bottom - 1);
                g.DrawArc(ThisPen, Center.X, Bounds.Top, Bounds.Width - 1, Bounds.Height - 1, 90, 180);
                g.FillPie(ThisPen.Brush, Center.X, Bounds.Top, Bounds.Width - 1, Bounds.Height - 1, 90, 180f * GenerationLevel);
            }
        }

        /// <summary>
        /// Retrieve a straight line
        /// </summary>
        /// <param name="FromRect"></param>
        /// <param name="ToRect"></param>
        /// <param name="Center1"></param>
        /// <param name="Center2"></param>
        /// <param name="MaintainCenter1"></param>
        /// <param name="MaintainCenter2"></param>
        public static void GetStraightLine(Rectangle FromRect, Rectangle ToRect, out Point Center1, out Point Center2, bool MaintainCenter1, bool MaintainCenter2)
        {
            Center1 = CenterRect(FromRect);
            Center2 = CenterRect(ToRect);

            if (Center2.X >= FromRect.Left && Center2.X <= FromRect.Right)
                Center1.X = Center2.X;
            else if (Center2.X > FromRect.Right)
                Center1.X = FromRect.Right;
            else if (Center2.X < FromRect.Left)
                Center1.X = FromRect.Left;
            if (Center2.Y >= FromRect.Top && Center2.Y <= FromRect.Bottom)
                Center1.Y = Center2.Y;
            else if (Center2.Y < FromRect.Top)
                Center1.Y = FromRect.Top;
            else if (Center2.Y > FromRect.Bottom)
                Center1.Y = FromRect.Bottom;

            if (Center1.X >= ToRect.Left && Center1.X <= ToRect.Right)
                Center2.X = Center1.X;
            else if (Center1.X > ToRect.Right)
                Center2.X = ToRect.Right;
            else if (Center1.X < ToRect.Left)
                Center2.X = ToRect.Left;

            if (Center1.Y >= ToRect.Top && Center1.Y <= ToRect.Bottom)
                Center2.Y = Center1.Y;
            else if (Center1.Y < ToRect.Top)
                Center2.Y = ToRect.Top;
            else if (Center1.Y > ToRect.Bottom)
                Center2.Y = ToRect.Bottom;

            //Shift our points out to the edge
            if (Center1.X == Center2.X && Center1.Y < Center2.Y)
            {
                Center1.Y = MaintainCenter1 ? CenterRect(FromRect).Y : FromRect.Bottom - 2;
                Center2.Y = MaintainCenter2 ? CenterRect(ToRect).Y : ToRect.Top + 2;
            }
            else if (Center1.X == Center2.X && Center1.Y > Center2.Y)
            {
                Center1.Y = MaintainCenter2 ? CenterRect(ToRect).Y : ToRect.Bottom - 2;
                Center2.Y = MaintainCenter1 ? CenterRect(FromRect).Y : FromRect.Top + 2;
            }
            else if (Center1.Y == Center2.Y && Center1.X < Center2.X)
            {
                Center1.X = MaintainCenter1 ? CenterRect(FromRect).X : FromRect.Right - 2;
                Center2.X = MaintainCenter2 ? CenterRect(ToRect).X : ToRect.Left + 2;
            }
            else if (Center1.Y == Center2.Y && Center1.X > Center2.X)
            {
                Center1.X = MaintainCenter2 ? CenterRect(ToRect).X : ToRect.Right - 2;
                Center2.X = MaintainCenter1 ? CenterRect(FromRect).X : FromRect.Left + 2;
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Clone a one-line element
        /// </summary>
        /// <param name="BaseElement"></param>
        public MM_OneLine_Element(MM_OneLine_Element BaseElement)
        {
            if (BaseElement != null)
                foreach (FieldInfo fI in typeof(MM_OneLine_Element).GetFields())
                    if (fI.Name == "ParentElement")
                        fI.SetValue(this, new MM_OneLine_Element((MM_OneLine_Element)fI.GetValue(BaseElement)));
                    else if (fI.Name=="Windings")
                    {
                        List<MM_OneLine_TransformerWinding> Windings = new List<MM_OneLine_TransformerWinding>();
                        foreach (MM_OneLine_TransformerWinding Winding in BaseElement.Windings)
                            Windings.Add(new MM_OneLine_TransformerWinding(Winding, this));
                        this.Windings = Windings.ToArray();
                    }
                    else
                        try
                        {
                            fI.SetValue(this, fI.GetValue(BaseElement));
                        }
                        catch (Exception ex)
                        { }
        }

        /// <summary>
        /// Initialize our one-line element
        /// </summary>
        /// <param name="xElem"></param>
        /// <param name="ParentElement"></param>
        public MM_OneLine_Element(XmlElement xElem, MM_OneLine_Element ParentElement = null)
        {
            try
            {
                //Read in our one-line information
                this.xConfig = xElem;
                MM_Serializable.ReadXml(xElem, this, false);
                Enum.TryParse<enumElemTypes>(xElem.Name, out this.ElemType);
                this.ParentElement = ParentElement;

                //Find our base element
                if (ParentElement != null)
                    this.BaseElement = ParentElement.BaseElement;
                else if (xElem.HasAttribute("BaseElement.TEID") && !MM_Repository.TEIDs.TryGetValue(XmlConvert.ToInt32(xElem.Attributes["BaseElement.TEID"].Value), out this.BaseElement))
                    this.BaseElement = MM_Element.CreateElement(xElem, "BaseElement", false);

                if (this.BaseElement != null)
                    this.KVLevel = this.BaseElement.KVLevel;


                //Go through our children and find any necessary components
                List<MM_OneLine_TransformerWinding> XFWs = new List<MM_OneLine_TransformerWinding>();
                foreach (XmlElement xChild in xElem.ChildNodes)
                    if (xChild.Name == "Descriptor")
                    {
                        this.Descriptor = new MM_OneLine_Element(xChild, this);
                        this.Descriptor.ComputeOrientation();
                    }
                    else if (xChild.Name == "SecondaryDescriptor")
                    {
                        this.SecondaryDescriptor = new MM_OneLine_Element(xChild, this);
                        this.SecondaryDescriptor.ComputeOrientation();
                    }
                    else if (xChild.Name == "Winding")
                        XFWs.Add(new MM_OneLine_TransformerWinding(xChild, null) { ParentElement = this });
                this.Windings = XFWs.ToArray();
            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        #region Instance drawing

        /// <summary>
        /// Report the descriptor text for our object
        /// </summary>
        /// <returns></returns>
        public String DescriptorText(MM_OneLine_Viewer Viewer)
        {
            StringBuilder sB = new StringBuilder();

            //Write out line information
            if (ParentElement.ElemType == enumElemTypes.Line)
            {
                MM_Line Line = (MM_Line)BaseElement;
                if (Viewer.BaseElement is MM_Substation)
                    sB.AppendLine((Viewer.BaseElement == Line.Substation1 ? Line.Substation2.LongName : Line.Substation1.LongName));
                else
                    sB.AppendLine(Line.Substation1.LongName + " to " + Line.Substation2.LongName);

                if (Viewer.btnPercent.Checked)
                    sB.AppendLine(((MM_Line)BaseElement).LinePercentageText);

                int FirstStation = 0;
                if (Viewer.BaseElement is MM_Substation)
                    FirstStation = Line.Substation1 == Viewer.BaseElement ? 0 : 1;
                float[] MVA = Line.Estimated_MVA;
                float[] MVAR = Line.Estimated_MVAR;
                float[] MW = Line.Estimated_MW;

                if (Viewer.btnMVA.Checked)
                    sB.AppendLine("MVA:\t" + OrientArrow(MVA[FirstStation], ParentElement.Orientation, true) + (Viewer.btnOtherLine.Checked ? "\t" + OrientArrow(MVA[1 - FirstStation], ParentElement.Orientation, false) : ""));
                if (Viewer.btnMW.Checked)
                    sB.AppendLine("MW:\t" + OrientArrow(MW[FirstStation], ParentElement.Orientation, true) + (Viewer.btnOtherLine.Checked ? "\t" + OrientArrow(MW[1 - FirstStation], ParentElement.Orientation, false) : ""));
                if (Viewer.btnMVAR.Checked)
                    sB.AppendLine("MVAR:\t" + OrientArrow(MVAR[FirstStation], ParentElement.Orientation, true) + (Viewer.btnOtherLine.Checked ? "\t" + OrientArrow(MVAR[1 - FirstStation], ParentElement.Orientation, false) : ""));
            }
            //Write out transfomrer information
            else if (ParentElement.ElemType == enumElemTypes.Transformer)
            {
                MM_Transformer XF = (MM_Transformer)ParentElement.BaseElement;
                MM_OneLine_TransformerWinding W1 = ParentElement.Windings[0];
                MM_OneLine_TransformerWinding W2 = ParentElement.Windings[1];
                MM_TransformerWinding X1 = (MM_TransformerWinding)W1.BaseElement;
                MM_TransformerWinding X2 = (MM_TransformerWinding)W2.BaseElement;

                bool IsHorizontal = W1.Orientation == enumOrientations.Left || W1.Orientation == enumOrientations.Right;

                if (Viewer.btnNames.Checked)
                    sB.AppendLine(X1.WindingType == MM_TransformerWinding.enumWindingType.Primary ? X1.Name : X2.Name);

                if (X1.WindingType == MM_TransformerWinding.enumWindingType.Primary)
                {
                    if (Viewer.btnMVA.Checked && !float.IsNaN(X1.Estimated_MVA) && X1.Voltage != 1)
                        sB.AppendLine("MVA:\t" + OrientArrow(X1.Estimated_MVA, W1.Orientation, IsHorizontal));
                    if (Viewer.btnMW.Checked && !float.IsNaN(X1.Estimated_MW) && X1.Voltage != 1)
                        sB.AppendLine("MW:\t" + OrientArrow(X1.Estimated_MW, W1.Orientation, IsHorizontal));
                    if (Viewer.btnMVAR.Checked && !float.IsNaN(X1.Estimated_MVAR) && X1.Voltage != 1)
                        sB.AppendLine("MVAR:\t" + OrientArrow(X1.Estimated_MVAR, W1.Orientation, IsHorizontal));
                }

                if (X2.WindingType == MM_TransformerWinding.enumWindingType.Primary)
                {
                    if (Viewer.btnMVA.Checked && !float.IsNaN(X2.Estimated_MVA) && X2.Voltage != 1)
                        sB.AppendLine("MVA:\t" + OrientArrow(X2.Estimated_MVA, W2.Orientation, IsHorizontal));
                    if (Viewer.btnMW.Checked && !float.IsNaN(X2.Estimated_MW) && X2.Voltage != 1)
                        sB.AppendLine("MW:\t" + OrientArrow(X2.Estimated_MW, W2.Orientation, IsHorizontal));
                    if (Viewer.btnMVAR.Checked && !float.IsNaN(X2.Estimated_MVAR) && X2.Voltage != 1)
                        sB.AppendLine("MVAR:\t" + OrientArrow(X2.Estimated_MVAR, W2.Orientation, IsHorizontal));
                }
            }
            else if (ParentElement.ElemType == enumElemTypes.Unit)
            {
                if (Viewer.btnNames.Checked)
                    sB.AppendLine(ParentElement.BaseElement.Name);
                MM_Unit Unit = (MM_Unit)BaseElement;
                float MW = Unit.Estimated_MW;
                float MVAR = Unit.Estimated_MVAR;
                if (Viewer.btnMW.Checked)
                    sB.AppendLine("MW:\t" + OrientArrow(MW, ParentElement.Orientation, false));
                if (Viewer.btnMVAR.Checked)
                    sB.AppendLine("MVAR:\t" + OrientArrow(MVAR, ParentElement.Orientation, false));
                sB.AppendLine((Unit.NoAVR ? "avr" : "AVR") + "\t" + (Unit.NoPSS ? "pss" : "PSS"));
                sB.AppendLine("Frq:\t" + Unit.Frequency.ToString("0.00") + " Hz");
                if (Unit.NearIsland != null)
                    sB.AppendLine("Isl:\t" + Unit.NearIsland.ID);
                   
                if (Unit.FrequencyControl)
                    sB.AppendLine("FRQ CTRL");
                if (Unit.NearIsland != null && Unit.NearIsland.FrequencyControl)                    
                    sB.AppendLine("ISL FRQ CTRL");
            }
            else if (ParentElement.ElemType == enumElemTypes.Load || ParentElement.ElemType == enumElemTypes.LAAR)
            {
                if (Viewer.btnNames.Checked)
                    sB.AppendLine(ParentElement.BaseElement.Name);
                MM_Load Load = (MM_Load)BaseElement;
                float Estimated_MW = Load.Estimated_MW;
                float Estimated_MVAR = Load.Estimated_MVAR;
                if (Viewer.btnMW.Checked)
                    sB.AppendLine("MW:\t" + OrientArrow(Estimated_MW, ParentElement.Orientation, true));
                if (Viewer.btnMVAR.Checked)
                    sB.AppendLine("MVAR:\t" + OrientArrow(Estimated_MVAR, ParentElement.Orientation, true));
            }
            else if (ParentElement.ElemType == enumElemTypes.Capacitor || ParentElement.ElemType == enumElemTypes.Reactor)
            {
                if (Viewer.btnNames.Checked)
                    sB.AppendLine(ParentElement.BaseElement.Name);
                MM_ShuntCompensator SC = (MM_ShuntCompensator)BaseElement;
                float Estimated_MVAR = SC.Estimated_MVAR;
                float Nominal_MVAR = SC.Nominal_MVAR;
                if (Viewer.btnMVAR.Checked)
                {
                    sB.AppendLine("MVAR:\t" + OrientArrow(Estimated_MVAR, ParentElement.Orientation, ParentElement.Orientation == enumOrientations.Left || ParentElement.Orientation == enumOrientations.Right));
                    sB.AppendLine("Nominal:\t" + OrientArrow(Nominal_MVAR, ParentElement.Orientation, ParentElement.Orientation == enumOrientations.Left || ParentElement.Orientation == enumOrientations.Right));
                }
            }
            else if (ParentElement.ElemType == enumElemTypes.StaticVarCompensator)
            {
                if (Viewer.btnNames.Checked)
                    sB.AppendLine(ParentElement.BaseElement.Name);
                MM_StaticVarCompensator SVC = (MM_StaticVarCompensator)BaseElement;
                if (Viewer.btnMVAR.Checked)
                {
                    sB.AppendLine("MVAR:\t" + OrientArrow(SVC.Estimated_MVAR, ParentElement.Orientation, false));
                    sB.AppendLine("Nominal:\t" + OrientArrow(SVC.Nominal_MVAR, ParentElement.Orientation, false));
                }
            }
            else if (ParentElement.ElemType == enumElemTypes.Node)
            {
                MM_Node Node = (MM_Node)ParentElement.BaseElement ;
                if (Viewer.btnNames.Checked)
                    sB.AppendLine(ParentElement.BaseElement.Name);

                //Find our associated bus
                if (Viewer.btnVoltage.Checked || Viewer.btnAngle.Checked || Viewer.btnFrequency.Checked)
                {
                    if (Viewer.btnVoltage.Checked && Node.AssociatedBus != null && !float.IsNaN(Node.AssociatedBus.Estimated_kV))
                        sB.AppendLine(Node.AssociatedBus.Estimated_kV.ToString("0.0 kV"));
                    if (Viewer.btnAngle.Checked && Node.AssociatedBus != null && !float.IsNaN(Node.AssociatedBus.Estimated_Angle))
                        sB.AppendLine(Node.AssociatedBus.Estimated_Angle.ToString("0.0 °"));
                    if (Viewer.btnFrequency.Checked && Node.AssociatedBus != null && Node.AssociatedBus.Island != null && !float.IsNaN(Node.AssociatedBus.Island.Frequency))
                        sB.AppendLine(Node.AssociatedBus.Island.Frequency.ToString("0.000 Hz"));
                }
            }
            else if (ParentElement.BaseElement.ElemType.Name == "SeriesCompensator")
            {
                MM_Line SC = (MM_Line)ParentElement.BaseElement;
                if (Viewer.btnNames.Checked)
                    sB.AppendLine(ParentElement.BaseElement.Name);
                if (Viewer.btnMVA.Checked)
                    sB.AppendLine("MVA:\t" + Math.Abs(SC.Estimated_MVA[0]).ToString("#,##0.0"));
                if (Viewer.btnMW.Checked)
                    sB.AppendLine("MW:\t" + Math.Abs(SC.Estimated_MW[0]).ToString("#,##0.0"));
                if (Viewer.btnMVAR.Checked)
                    sB.AppendLine("MVAR:\t" + Math.Abs(SC.Estimated_MVAR[0]).ToString("#,##0.0"));
            }
            else if (BaseElement == null)
                return this.Text;
            else if (ParentElement.ElemType == enumElemTypes.PricingVector)
            {
                MM_PricingVector PV = (MM_PricingVector)ParentElement.BaseElement;
                if (PV.EPSMeter != null && PV.EPSMeter.Name != null)
                    sB.AppendLine(PV.EPSMeter.Name);
                sB.AppendLine(PV.RID);
            }
            else if (ElemType == enumElemTypes.SecondaryDescriptor)
            {
                sB.AppendLine("? resourcenode?");
                //return (ParentElement as MM_OneLine_Node).ResourceNode;
            }
            else
                sB.AppendLine(BaseElement.Name);

            return sB.ToString().TrimEnd('\r', '\n');
        }

        /// <summary>
        /// Return a string corresponding to an orientation
        /// </summary>
        /// <param name="InVal"></param>
        /// <param name="ParentOrientation"></param>
        /// <param name="IsInDirection"></param>
        /// <returns></returns>
        private string OrientArrow(float InVal, enumOrientations ParentOrientation, bool IsInDirection)
        {
            if (float.IsNaN(InVal))
                return "? ";
            else if (InVal == 0f)
                return "0.0";
            bool IsPositive = IsInDirection ? InVal > 0f : InVal < 0f;

            if (false)
            {


                if (ParentOrientation == enumOrientations.Up)
                    return (IsPositive ? "  " : "▼") + Math.Abs(InVal).ToString(" #,##0.0 ") + (IsPositive ? "▲" : "  ");
                else if (ParentOrientation == enumOrientations.Down)
                    return (!IsPositive ? "  " : "▼") + Math.Abs(InVal).ToString(" #,##0.0 ") + (!IsPositive ? "▲" : "  ");
                else if (ParentOrientation == enumOrientations.Left)
                    return (IsPositive ? "◄" : "  ") + Math.Abs(InVal).ToString(" #,##0.0 ") + (IsPositive ? "  " : "►");
                else if (ParentOrientation == enumOrientations.Right)
                    return (!IsPositive ? "◄" : "  ") + Math.Abs(InVal).ToString(" #,##0.0 ") + (!IsPositive ? "  " : "►");
                else if (ParentOrientation == enumOrientations.Vertical)
                    return (IsPositive ? "▲" : "▼") + Math.Abs(InVal).ToString(" #,##0.0");
                else if (ParentOrientation == enumOrientations.Horizontal)
                    return (IsPositive ? "◄" : "►") + Math.Abs(InVal).ToString(" #,##0.0");
                else
                    return "? " + InVal.ToString("#,##0.0");
            }
            else
            {
                if (ParentOrientation == enumOrientations.Up || ParentOrientation == enumOrientations.Vertical)
                    return (IsPositive ? "▲" : "▼") + Math.Abs(InVal).ToString(" #,##0.0");
                else if (ParentOrientation == enumOrientations.Down)
                    return (IsPositive ? "▼" : "▲") + Math.Abs(InVal).ToString(" #,##0.0");
                else if (ParentOrientation == enumOrientations.Left || ParentOrientation == enumOrientations.Horizontal)
                    return (IsPositive ? "◄" : "►") + Math.Abs(InVal).ToString(" #,##0.0");
                else if (ParentOrientation == enumOrientations.Right)
                    return (IsPositive ? "►" : "◄") + Math.Abs(InVal).ToString(" #,##0.0");
                else
                    return "? " + InVal.ToString("#,##0.0");
            }
        }

        /// <summary>
        /// Determine the orientation of our element relative to our parent
        /// </summary>
        public void ComputeOrientation()
        {
            bool ToRight = Bounds.Left >= ParentElement.Bounds.Right;
            bool ToLeft = Bounds.Right <= ParentElement.Bounds.Left;
            bool ToTop = Bounds.Top >= ParentElement.Bounds.Bottom;
            bool ToBottom = Bounds.Bottom <= ParentElement.Bounds.Top;

            if (ToRight)
                Orientation = enumOrientations.Right;
            else if (ToLeft)
                Orientation = enumOrientations.Left;
            else if (ToTop)
                Orientation = enumOrientations.Up;
            else if (ToBottom)
                Orientation = enumOrientations.Down;
        }

        /// <summary>
        /// Handle the rendering of our one-line element
        /// </summary>
        /// <param name="g"></param>
        /// <param name="Viewer"></param>
        /// <param name="IsFlashing"></param>
        public void PaintElement(Graphics g, MM_OneLine_Viewer Viewer, bool IsFlashing)
        {
            try
            {
                Color DrawColor = KVLevel == null ? Color.Red : KVLevel.Energized.ForeColor;
                MM_AlarmViolation_Type WorstViol = null;
                if (IsFlashing && BaseElement!=null)
                {
                    WorstViol = BaseElement.WorstViolationOverall;
                    if (WorstViol != null)
                        DrawColor = WorstViol.ForeColor;
                }

                Rectangle DisplayRectangle = this.Bounds;
                if (Font == null)
                    Font = Viewer.Font;
                if (ElemType == enumElemTypes.Descriptor && BaseElement is MM_Breaker_Switch)
                {
                    MM_Breaker_Switch BS = (MM_Breaker_Switch)BaseElement;
                    if (BS.HasSynchroscope && BS.HasSynchrocheckRelay)
                        Font = SynchroscopeAndCheckFont;
                    else if (BS.HasSynchroscope)
                        Font = SynchroscopeFont;
                    else if (BS.HasSynchrocheckRelay)
                        Font = SynchrocheckFont;
                }

                //If we have a note, draw it
                if (BaseElement != null && BaseElement.Notes.Count > 0)
                    g.DrawImageUnscaled(MacomberMapClient.Properties.Resources.NoteHS, DisplayRectangle.Left, DisplayRectangle.Top);
                
               
                bool ShiftText;
                if (ElemType == enumElemTypes.Transformer)
                {
                    MM_Transformer XF = (MM_Transformer)BaseElement;
                    foreach (MM_OneLine_TransformerWinding Winding in Windings)
                    {
                        Rectangle WindingBounds = Winding.Bounds;
                        WindingBounds.X += DisplayRectangle.Left;
                        WindingBounds.Y += DisplayRectangle.Top;
                        if (WorstViol == null)
                            //Winding.KVLevel.Energized.ForeColor
                            MM_OneLine_Element.DrawTransformerWinding(g, WindingBounds, Brushes.Black, Winding.Orientation, Winding.KVLevel.Energized.ForeColor, true, Winding.IsPhaseShifter && ((MM_TransformerWinding)Winding.BaseElement).WindingType == MM_TransformerWinding.enumWindingType.Primary);
                        else
                            MM_OneLine_Element.DrawTransformerWinding(g, WindingBounds, Brushes.Black, Winding.Orientation, WorstViol.ForeColor, true, Winding.IsPhaseShifter && ((MM_TransformerWinding)Winding.BaseElement).WindingType == MM_TransformerWinding.enumWindingType.Primary);
                    }
                }
                else if (ElemType == enumElemTypes.Descriptor)
                {
                    //TODO: Handle our logic for drawing our rectangle when a mismatch occurs
                    String TextToWrite = DescriptorText(Viewer);
                    // Size RectSize = g.MeasureString(TextToWrite, Font, DisplayRectangle.Size, Format).ToSize();                   
                    using (StringFormat DrawFormat = FormatForText(out ShiftText))
                    {
                        Rectangle DrawRectangle = DisplayRectangle;
                        if (ShiftText)
                        {
                            int NewWidth = (int)g.MeasureString(TextToWrite, Font, DisplayRectangle.Size, DrawFormat).Width / 2;
                            DrawRectangle = new Rectangle(DisplayRectangle.Left + (DisplayRectangle.Width / 2) - NewWidth, DisplayRectangle.Top, NewWidth, DisplayRectangle.Height);
                        }
                        g.DrawString(TextToWrite, Font, Brushes.LightGray, DrawRectangle, DrawFormat);
                    }

                    //If we're a mothballed or retired unit, handle accordingly.
                    if (BaseElement is MM_Unit)
                    {
                        MM_Unit Unit = (MM_Unit)BaseElement;
                        if (Unit.PANType == MM_Unit.enumPanType.MothballedGeneration || Unit.PANType == MM_Unit.enumPanType.RetiredGeneration)
                            using (Pen DrawPen = new Pen(Color.LightPink, 4f))
                            using (StringFormat sF = new StringFormat() {  Alignment = StringAlignment.Center, LineAlignment= StringAlignment.Center})
                            using (Font DrawFont = new Font("Arial",14, FontStyle.Bold))
                        {
                            Rectangle DrawRect = Rectangle.FromLTRB(Math.Min(this.Bounds.Left, ParentElement.Bounds.Left), Math.Min(this.Bounds.Top, ParentElement.Bounds.Top), Math.Max(this.Bounds.Right, ParentElement.Bounds.Right), Math.Max(this.Bounds.Bottom, ParentElement.Bounds.Bottom));
//g.DrawRectangle(DrawPen, DrawRect);
                          //  g.DrawLine(DrawPen, DrawRect.Left, DrawRect.Top, DrawRect.Right, DrawRect.Bottom);
                         //   g.DrawLine(DrawPen, DrawRect.Right, DrawRect.Top, DrawRect.Left, DrawRect.Bottom);

                            //Rotate and draw our text
                            Point MidPoint = new Point(DrawRect.Left + (DrawRect.Width / 2), DrawRect.Top + (DrawRect.Height / 2));
                            g.TranslateTransform(MidPoint.X, MidPoint.Y);
                            g.RotateTransform(45);

                            SizeF RectSize = g.MeasureString(Unit.PANType.ToString(), DrawFont);
                            g.FillRectangle(Brushes.BlanchedAlmond, -RectSize.Width/2, -RectSize.Height/2, RectSize.Width, RectSize.Height);
                            g.DrawString(Unit.PANType.ToString(), DrawFont, Brushes.Red, PointF.Empty, sF);
                            g.RotateTransform(-45);
                            g.TranslateTransform(-MidPoint.X, -MidPoint.Y);

                        }
                    }
                }
                else if (ElemType == enumElemTypes.SecondaryDescriptor)
                    using (StringFormat DrawFormat = FormatForText(out ShiftText))
                        g.DrawString(((MM_OneLine_Node)this.ParentElement).ResourceNode, Font, Brushes.MediumPurple, DisplayRectangle, DrawFormat);
                else if (ElemType == enumElemTypes.Breaker)
                    MM_OneLine_Element.DrawBreaker(g, DisplayRectangle, Brushes.Black, Orientation, DrawColor, ((MM_Breaker_Switch)BaseElement).BreakerState, ((MM_Breaker_Switch)BaseElement).NormalOpen);
                else if (ElemType == enumElemTypes.Switch)
                    MM_OneLine_Element.DrawSwitch(g, DisplayRectangle, Brushes.Black, Orientation, DrawColor, ((MM_Breaker_Switch)BaseElement).BreakerState, ((MM_Breaker_Switch)BaseElement).NormalOpen);
                else if (ElemType == enumElemTypes.Unit)
                    MM_OneLine_Element.DrawUnit(g, DisplayRectangle, Brushes.Black, Orientation, DrawColor, .5f);
                else if (ElemType == enumElemTypes.Line)
                    MM_OneLine_Element.DrawLine(g, DisplayRectangle, Brushes.Black, Orientation, DrawColor);
                else if (ElemType == enumElemTypes.Reactor)
                    MM_OneLine_Element.DrawReactor(g, DisplayRectangle, Brushes.Black, Orientation, DrawColor, true);
                else if (ElemType == enumElemTypes.Load)
                    MM_OneLine_Element.DrawLoad(g, DisplayRectangle, Brushes.Black, Orientation, DrawColor);
                else if (ElemType == enumElemTypes.LAAR)
                    MM_OneLine_Element.DrawLaaR(g, DisplayRectangle, Brushes.Black, Orientation, DrawColor);
                else if (ElemType == enumElemTypes.EndCap)
                    MM_OneLine_Element.DrawEndCap(g, DisplayRectangle, Brushes.Black, Orientation, DrawColor);
                else if (ElemType == enumElemTypes.Capacitor)
                    MM_OneLine_Element.DrawCapacitor(g, DisplayRectangle, Brushes.Black, Orientation, DrawColor, true);
                else if (ElemType == enumElemTypes.StaticVarCompensator)
                    MM_OneLine_Element.DrawStaticVarCompensator(g, DisplayRectangle, Brushes.Black, Orientation, DrawColor);
                else if (ElemType == enumElemTypes.ResistorReactor)
                    MM_OneLine_Element.DrawResistorReactor(g, DisplayRectangle, Brushes.Black, Orientation, DrawColor);
                else if (ElemType == enumElemTypes.ResistorCapacitor)
                    MM_OneLine_Element.DrawResistorCapacitor(g, DisplayRectangle, Brushes.Black, Orientation, DrawColor);
                else if (ElemType == enumElemTypes.Resistor)
                    MM_OneLine_Element.DrawResistor(g, DisplayRectangle, Brushes.Black, Orientation, DrawColor, true);
                else if (this is MM_OneLine_Node)
                {
                    MM_OneLine_Node Node = (MM_OneLine_Node)this;
                    if (ElemType == enumElemTypes.Node || ElemType == enumElemTypes.PokePoint)
                        if (Node.IsJumper || Node.IsVisible)
                            MM_OneLine_Element.DrawPokePoint(g, DisplayRectangle, Brushes.Black, Orientation, DrawColor, Node.IsJumper);
                        else
                            using (Brush DrawBrush = new SolidBrush(DrawColor))
                                g.FillRectangle(DrawBrush, DisplayRectangle);
                    else if (ElemType == enumElemTypes.PricingVector)
                        MM_OneLine_Element.DrawPricingVector(g, DisplayRectangle, Brushes.Black, Orientation, DrawColor, Node.IsPositive, 1f);
                }
                else if (ElemType == enumElemTypes.Label)
                        g.DrawString(this.Text, this.Font, Brushes.White, DisplayRectangle.Location);
                else if (ElemType == enumElemTypes.Rectangle)
                    using (Pen DrawPen = new Pen(this.ForeColor))
                        g.DrawRectangle(DrawPen, DisplayRectangle.Left, DisplayRectangle.Top, DisplayRectangle.Width - 1, DisplayRectangle.Height - 1);
                else if (ElemType == enumElemTypes.Circle)
                    using (Pen DrawPen = new Pen(this.ForeColor))
                        g.DrawEllipse(DrawPen, DisplayRectangle.Left, DisplayRectangle.Top, DisplayRectangle.Width - 1, DisplayRectangle.Height - 1);
                else if (ElemType == enumElemTypes.ArrowLine)
                    using (Pen DrawPen = new Pen(this.ForeColor))
                    {
                        DrawPen.StartCap = LineCap.ArrowAnchor;
                        DrawPen.Width = 2;
                        g.DrawLine(DrawPen, DisplayRectangle.Left, DisplayRectangle.Top, DisplayRectangle.Right, DisplayRectangle.Bottom);
                    }
                else if (ElemType == enumElemTypes.Image && Image != null)
                    g.DrawImage(Image, DisplayRectangle);

              
                }
            catch 
            { }
        }
        #endregion

        /// <summary>
        /// Report the name of our object
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (ParentElement != null)
            {
            if (ElemType == enumElemTypes.Descriptor || ElemType == enumElemTypes.SecondaryDescriptor)
                return ParentElement.ElemType.ToString() + " " + ParentElement.BaseElement.Name + " (" + ElemType.ToString() + ")";
            else
                return ElemType.ToString() + " " + ParentElement.BaseElement.Name;
            }
            else
                return ElemType.ToString() + " " + BaseElement.Name;
        }
    }
    }