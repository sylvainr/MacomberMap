using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Macomber_Map.Data_Elements;
using System.Xml;
using Macomber_Map.Data_Connections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Macomber_Map.Misc;
using System.Data;
using System.Text;
using System.Windows.Forms.VisualStyles;
using System.Threading;

namespace Macomber_Map.User_Interface_Components.OneLines
{
    /// <summary>
    /// This class holds the information for an individual one-line element
    /// </summary>
    public partial class MM_OneLine_Element : UserControl
    {       
        #region Variable declaration
        /// <summary>The drawing brush</summary>
        public Brush DrawBrush = Brushes.LightGray;
                        
        /// <summary>The rectangle in which our descriptor is rendered</summary>
        public Rectangle DescriptorRectangle = Rectangle.Empty;

        /// <summary>Our collection of values for our element</summary>
        public Dictionary<MM_Value_Locator, Object> Values = new Dictionary<MM_Value_Locator, object>(30);

        /// <summary>The parent element of a descriptor, poke point, or pricing vector</summary>
        public MM_OneLine_Element ParentElement;

        /// <summary>The string format that makes text centered</summary>
        public static StringFormat CenterFormat
        {
            get
            {
                if (_CenterFormat == null)
                {
                    _CenterFormat = new StringFormat(StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip | StringFormatFlags.NoWrap);
                    _CenterFormat.Alignment = _CenterFormat.LineAlignment = StringAlignment.Center;
                    _CenterFormat.Trimming = StringTrimming.None;
                }
                return _CenterFormat;
            }
        }
        private static StringFormat _CenterFormat;

        /// <summary>The image of our element, if any</summary>
        public Bitmap Image = null;

        /// <summary>The TEID of this item</summary>
        public Int32 TEID
        {
            get { if (BaseElement != null) return BaseElement.TEID; else return 0; }
        }

        /// <summary>
        /// Retrieve the KV level of the element
        /// </summary>
        public MM_KVLevel KVLevel
        {
            get
            {
                if (BaseElement == null)
                    return null;
                else
                    return BaseElement.KVLevel;
            }
        }

        /// <summary>The base element for this one-line rendering</summary>
        public MM_Element BaseElement;

        /// <summary>The foreground color and pen width</summary>
        public Pen ForePen;

        /// <summary>The font to draw text in (if applicable)</summary>
        public Font TextFont;

        /// <summary>The title of the one-line element</summary>
        public String Title;

        /// <summary>The position (relative to the origin) of text</summary>
        public Point TextPosition;

        /// <summary>If a polyline, etc., the series of points</summary>
        public Point[] Points;     

        /// <summary>The element type</summary>
        [Category("EMS"), Description("The element type")]        
        public enumElemTypes ElemType
        {
            get { return _ElemType; }
            set { _ElemType = value; }
        }
        private enumElemTypes _ElemType;

       
        /// <summary>The one-line viewer holding this element</summary>
        public OneLine_Viewer OneLineViewer;

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
            ArrowLine
        }
        /// <summary>
        /// The collection of orientations possible for an element
        /// </summary>
        public enum enumOrientations
        {
            /// <summary>Type unknown</summary>
            Unknown,
            /// <summary>Facing horizontally</summary>
            Horizontal,
            /// <summary>Facing vertically</summary>
            Vertical,
            /// <summary>Facing up</summary>
            Up,
            /// <summary>Facing down</summary>
            Down,
            /// <summary>Facing left</summary>
            Left,
            /// <summary>Facing right</summary>
            Right,
            /// <summary>A node (no direction)</summary>
            Node,
            /// <summary>A bus (direction determined by its points)</summary>
            Bus
        };

        /// <summary>
        /// Whether an arrow should be drawn from descriptor to this element
        /// </summary>
        [Category("Display"), Description("Whether an arrow should be drawn from descriptor to this element"), DefaultValue(false)]
        public bool DescriptorArrow
        {
            get { return _DescriptorArrow; }
            set { _DescriptorArrow = value; }
        }
        private bool _DescriptorArrow = false;


        /// <summary>The descriptor label associated with the element</summary>
        [Category("Display"), Description("The descriptor label associated with the element")]
        public MM_OneLine_Element Descriptor
        {
            get { return _Descriptor; }
            set
            {
                _Descriptor = value;
                if (Parent != null)
                    Parent.Refresh();
            }
        }
        private MM_OneLine_Element _Descriptor;

        /// <summary>The descriptor label associated with the element</summary>
        [Category("Display"), Description("The secondary descriptor associated with the element")]
        public MM_OneLine_Element SecondaryDescriptor
        {
            get { return _SecondaryDescriptor; }
            set
            {
                _SecondaryDescriptor = value; 
                if (Parent != null)
                    Parent.Refresh();
            }
        }
        private MM_OneLine_Element _SecondaryDescriptor;

        /// <summary>The current orientation of the element</summary>
        public enumOrientations Orientation;

        /// <summary>The elements connected to the current element, if a node or bus.</summary>
        public Int32[] ConnectedElements;       

        /// <summary>The violation viewer</summary>
        public Violation_Viewer ViolViewer;

        /// <summary>The GUID of the element</summary>
        public Guid Guid;

        /// <summary>
        /// The XML configuration element for this item.
        /// </summary>
        public XmlElement xElement;

        /// <summary>The data row for this line.</summary>
        public DataRow BaseRow;

        /// <summary>If a line, the data row for the other side</summary>
        public DataRow OtherRow;
        #endregion


        #region Initialization
        /// <summary>
        /// Initialize a new one-line element from a CIM element
        /// </summary>
        /// <param name="BaseElement">The element upon which the one-line element is based</param>        
        /// <param name="Orientation">The orientation of the element</param>
        /// <param name="olView"></param>
        /// <param name="ViolView"></param>
        public MM_OneLine_Element(MM_Element BaseElement, enumOrientations Orientation, Violation_Viewer ViolView, OneLine_Viewer olView)            
        {            
            this.Orientation = Orientation;
            this.BaseElement = BaseElement;
            this.ViolViewer = ViolView;
            this.OneLineViewer = olView;
            if (BaseElement != null)
            {
                this.Name = BaseElement.Name;
                if (Data_Integration.MMServer != null && Array.IndexOf(Data_Integration.MMServer.UserOperatorships, 999999) == -1 && Array.IndexOf(Data_Integration.MMServer.UserOperatorships, BaseElement.Operator.TEID) == -1)
                    DrawBrush = Brushes.Gray;
            }
        }     

        /// <summary>
        /// Initialize a new one line element
        /// </summary>
        /// <param name="xE">The XML configuration element for this one-line element</param>
        /// <param name="ViolViewer">The violation viewer</param>
        /// <param name="olView">The associated one-line viewer</param>
        public MM_OneLine_Element(XmlElement xE, Violation_Viewer ViolViewer, OneLine_Viewer olView)
        {
            this.ViolViewer = ViolViewer;
            this.OneLineViewer = olView;
            this.xElement = xE;
            this.Font = new Font("Arial", 8);
            this.ForeColor = Color.White;          

            //Now, if we have one, set our base element
            if (xE.HasAttribute("BaseElement.TEID") && !xE.Name.Contains("Descriptor"))
                if (true)//!MM_Repository.TEIDs.TryGetValue(Int32.Parse(xE.Attributes["BaseElement.TEID"].Value), out this.BaseElement) || this.BaseElement.ElemType.Name == "Unknown")
                {
                    this.BaseElement = MM_Element.CreateElement(xE, "BaseElement", false);
                    //Add in our special handling for a line's substations if needed
                    if (this.BaseElement is MM_Line)
                    {
                        MM_Line Line = this.BaseElement as MM_Line;
                        if (String.IsNullOrEmpty(Line.ConnectedStations[0].Name))
                            Line.ConnectedStations[0].Name = xE.Attributes["BaseElement.Sub1Name"].Value;
                        if (String.IsNullOrEmpty(Line.ConnectedStations[1].Name))
                            Line.ConnectedStations[1].Name = xE.Attributes["BaseElement.Sub2Name"].Value;
                    }                   
                }
              


            this.ElemType = (enumElemTypes)Enum.Parse(typeof(enumElemTypes), xE.Name);
            MM_Serializable.ReadXml(xE, this,false);
              

            //Add the descriptor if present                
            if (xE["Descriptor"] != null)
            {
                Descriptor = new MM_OneLine_Element(this, xE["Descriptor"], ViolViewer, olView);
                olView.Descriptors.Add(BaseElement, Descriptor);
                //olView.pnlElements.Controls.Add(Descriptor);               
            }
            if (xE["SecondaryDescriptor"] != null)
            {
                SecondaryDescriptor = new MM_OneLine_Element(this, xE["SecondaryDescriptor"], ViolViewer, olView);
                olView.SecondaryDescriptors.Add(BaseElement, SecondaryDescriptor);
                //olView.pnlElements.Controls.Add(SecondaryDescriptor);
            }
        }

        /// <summary>
        /// Initialize the descriptor
        /// </summary>
        /// <param name="ParentElement"></param>
        /// <param name="ElemConfig"></param>
        /// <param name="violViewer"></param>
        /// <param name="olView"></param>
        public MM_OneLine_Element(MM_OneLine_Element ParentElement, XmlElement ElemConfig, Violation_Viewer violViewer, OneLine_Viewer olView)
            : this(ElemConfig, violViewer, olView)
        {            
            this.ParentElement = ParentElement;
            this.BaseRow = ParentElement.BaseRow;
            this.BaseElement = ParentElement.BaseElement;

            //If we're connected to a server, make sure our operatorship is investigated
            if (Data_Integration.MMServer != null &&  Array.IndexOf(Data_Integration.MMServer.UserOperatorships, 999999) == -1 && Array.IndexOf(Data_Integration.MMServer.UserOperatorships, BaseElement.Operator.TEID) == -1)
                DrawBrush = Brushes.Gray;
            //if (this is MM_OneLine_PricingVector == false && this.ParentElement is MM_OneLine_PricingVector == false && (this.ElemType == enumElemTypes.Descriptor || this.ElemType == enumElemTypes.SecondaryDescriptor))
              //  this.Size = Size.Add(TextRenderer.MeasureText(this.DescriptorText, this.Font), new Size(4, 4));
        }       


        /// <summary>
        /// Handle the painting of the display
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                if (Visible)
                {
                    //Determine the color to be used to draw the element

                    Color DrawColor = this.ForeColor;
                    MM_AlarmViolation_Type WorstViolation = null;

                    if (BaseElement == null)
                        DrawColor = Color.White;
                    else
                    {
                   
                        MM_Element CoreElement;
                        if (!MM_Repository.TEIDs.TryGetValue(BaseElement.TEID, out CoreElement))
                            CoreElement = BaseElement;
                            
                        if (ViolViewer != null && (WorstViolation = CoreElement.WorstVisibleViolation(ViolViewer.ShownViolations, this)) != null)
                            DrawColor = WorstViolation.ForeColor;
                        else if (ViolViewer != null && (WorstViolation = CoreElement.WorstVisibleViolation(ViolViewer.ShownViolations, this)) != null)
                            DrawColor = WorstViolation.ForeColor;
                        else if (CoreElement.KVLevel == null) //core elem or base elem? - nataros merge question
                            DrawColor = Color.White;
                        else
                            DrawColor = CoreElement.KVLevel.Energized.ForeColor; //core elem or base elem? - nataros merge question
                        if (BaseElement.PUNElement && ElemType != enumElemTypes.Descriptor && ElemType != enumElemTypes.SecondaryDescriptor)
                            e.Graphics.Clear(Color.DarkSlateGray);
                    }

                    if (ElemType == enumElemTypes.LAAR && OneLineViewer.loadsToolStripMenuItem.Checked)
                        DrawLaaR(e.Graphics, DrawColor);
                    else if (ElemType == enumElemTypes.Load && OneLineViewer.loadsToolStripMenuItem.Checked)
                        DrawLoad(e.Graphics, DrawColor);
                    else if (ElemType == enumElemTypes.Breaker && OneLineViewer.breakersToolStripMenuItem.Checked)
                        DrawBreaker(e.Graphics, DrawColor);
                    else if (ElemType == enumElemTypes.Switch && OneLineViewer.switchesToolStripMenuItem.Checked)
                        DrawSwitch(e.Graphics, DrawColor);
                    else if (ElemType == enumElemTypes.Node && OneLineViewer.mnuNodes.Checked)
                        DrawNode(e.Graphics, DrawColor);
                    else if (ElemType == enumElemTypes.Line && OneLineViewer.mnuLines.Checked)
                        DrawLine(e.Graphics, DrawColor);
                    else if (ElemType == enumElemTypes.Reactor && OneLineViewer.reactorsToolStripMenuItem.Checked)
                        DrawReactor(e.Graphics, DisplayRectangle, DrawColor);
                    else if (ElemType == enumElemTypes.Unit && OneLineViewer.unitsToolStripMenuItem.Checked)
                        DrawUnit(e.Graphics, DrawColor);
                    else if (ElemType == enumElemTypes.EndCap && OneLineViewer.endcapsToolStripMenuItem.Checked)
                        DrawEndCap(e.Graphics, DrawColor);
                    else if (ElemType == enumElemTypes.Transformer && OneLineViewer.transformersToolStripMenuItem.Checked)
                        DrawTransformer(e.Graphics, WorstViolation);
                    else if (ElemType == enumElemTypes.Capacitor && OneLineViewer.capacitorsToolStripMenuItem.Checked)
                        DrawCapacitor(e.Graphics, DisplayRectangle, DrawColor);
                    else if (ElemType == enumElemTypes.ResistorReactor && OneLineViewer.reactorsToolStripMenuItem.Checked)
                        DrawResistorReactor(e.Graphics, DisplayRectangle, DrawColor);
                    else if (ElemType == enumElemTypes.ResistorCapacitor && OneLineViewer.capacitorsToolStripMenuItem.Checked)
                        DrawResistorCapacitor(e.Graphics, DisplayRectangle, DrawColor);
                    else if (ElemType == enumElemTypes.PricingVector && OneLineViewer.pricingVectorsToolStripMenuItem.Checked)
                        DrawPricingVector(e.Graphics, DrawColor, (this as MM_OneLine_PricingVector).IsPositive);
                    else if (ElemType == enumElemTypes.PokePoint && OneLineViewer.mnuNodes.Checked)
                        DrawPokePoint(e.Graphics, DrawColor);
                    else if (ElemType == enumElemTypes.Label && OneLineViewer.shapesToolStripMenuItem.Checked)
                        using (Brush DrawBrush = new SolidBrush(this.ForeColor))
                            e.Graphics.DrawString(this.Text, this.Font, DrawBrush, DisplayRectangle);
                    else if (ElemType == enumElemTypes.Rectangle && OneLineViewer.shapesToolStripMenuItem.Checked)
                        using (Pen DrawPen = new Pen(this.ForeColor))
                            e.Graphics.DrawRectangle(DrawPen, DisplayRectangle.Left, DisplayRectangle.Top, DisplayRectangle.Width - 1, DisplayRectangle.Height - 1);
                    else if (ElemType == enumElemTypes.Circle && OneLineViewer.shapesToolStripMenuItem.Checked)
                        using (Pen DrawPen = new Pen(this.ForeColor))
                            e.Graphics.DrawEllipse(DrawPen, DisplayRectangle.Left, DisplayRectangle.Top, DisplayRectangle.Width - 1, DisplayRectangle.Height - 1);
                    else if (ElemType == enumElemTypes.ArrowLine && OneLineViewer.arrowsToolStripMenuItem.Checked)
                        using (Pen DrawPen = new Pen(this.ForeColor))
                        {
                            DrawPen.StartCap = LineCap.ArrowAnchor;
                            DrawPen.Width = 2;
                            e.Graphics.DrawLine(DrawPen, DisplayRectangle.Left, DisplayRectangle.Top, DisplayRectangle.Right, DisplayRectangle.Bottom);
                        }
                    else if (ElemType == enumElemTypes.Image && Image != null && OneLineViewer.shapesToolStripMenuItem.Checked)
                        e.Graphics.DrawImage(Image, DisplayRectangle);
                    else if (ElemType == enumElemTypes.StaticVarCompensator)
                        DrawStaticVarCompensator(e.Graphics, DrawColor);
                    else if (ElemType == enumElemTypes.SecondaryDescriptor || ElemType == enumElemTypes.Descriptor)
                        DrawDescriptor(e.Graphics);

                }
            }
            catch (RowNotInTableException)
            {
            }
        }

        /// <summary>
        /// Draw a transformer winding
        /// </summary>
        /// <param name="g"></param>                                        
        /// <param name="WorstViolation"></param>
        public void DrawTransformer(Graphics g, MM_AlarmViolation_Type WorstViolation)
        {
            if (!Visible)
                return;

            for (int ThisWinding = 0; ThisWinding < (this as MM_OneLine_Transformer).TransformerWindings.Length; ThisWinding++)
            {
                MM_OneLine_TransformerWinding Winding = (this as MM_OneLine_Transformer).TransformerWindings[ThisWinding];
                using (Pen ThisPen = new Pen(WorstViolation == null ? Winding.BaseWinding.KVLevel.Energized.ForeColor : WorstViolation.ForeColor))
                    if (Winding.BaseWinding.Transformer.PhaseShifter && ThisWinding == 0)
                    {                        
                        Point Center = MM_OneLine_Element.CenterRect(Winding.Bounds);
                        g.DrawEllipse(ThisPen, Center.X - 5, Center.Y - 5, 10, 10);
                        using (Pen NewPen = new Pen(ThisPen.Color))
                        {
                            NewPen.EndCap = LineCap.ArrowAnchor;
                            g.DrawLine(NewPen, Winding.Bounds.Left, Winding.Bounds.Top, Winding.Bounds.Right, Winding.Bounds.Bottom);
                        }
                    }
                    else
                        for (int a = 0; a <= 24; a += 6)
                            if (Winding.Orientation == enumOrientations.Down)
                            {
                                g.DrawLine(ThisPen, a + Winding.Bounds.Left, 3 + Winding.Bounds.Top, a + Winding.Bounds.Left, (a == 12 ? 12 : 9) + Winding.Bounds.Top);
                                if (a <= 18)
                                    g.DrawArc(ThisPen, a + Winding.Bounds.Left, Winding.Bounds.Top, 6, 6, 180, 180);
                            }
                            else if (Winding.Orientation == enumOrientations.Up)
                            {
                                g.DrawLine(ThisPen, Winding.Bounds.Left + a, Winding.Bounds.Top + (a == 12 ? 0 : 3), Winding.Bounds.Left + a, Winding.Bounds.Top + 9);
                                if (a <= 18)
                                    g.DrawArc(ThisPen, Winding.Bounds.Left + a, Winding.Bounds.Top + 6, 6, 6, 0, 180);
                            }
                            else if (Winding.Orientation == enumOrientations.Left)
                            {
                                g.DrawLine(ThisPen, Winding.Bounds.Left + 3, Winding.Bounds.Top + a, Winding.Bounds.Left + (a == 12 ? 12 : 9), Winding.Bounds.Top + a);
                                if (a <= 18)
                                    g.DrawArc(ThisPen, Winding.Bounds.Left, Winding.Bounds.Top + a, 6, 6, 90, 180);
                            }
                            else if (Winding.Orientation == enumOrientations.Right)
                            {
                                g.DrawLine(ThisPen, Winding.Bounds.Left + (a == 12 ? 0 : 3), Winding.Bounds.Top + a, Winding.Bounds.Left + 9, Winding.Bounds.Top + a);
                                if (a <= 18)
                                    g.DrawArc(ThisPen, Winding.Bounds.Left + 6, Winding.Bounds.Top + a, 6, 6, 270, 180);
                            }
            }
        }

        /// <summary>
        /// Draw an endcap
        /// </summary>
        /// <param name="g"></param>
        /// <param name="ForeColor"></param>
        public void DrawEndCap(Graphics g, Color ForeColor)
        {
            using (Brush CircleBrush = new SolidBrush(ForeColor))
                g.FillEllipse(CircleBrush, DisplayRectangle);
        }

        /// <summary>
        /// Return an appropriately-shifted rectangle.
        /// </summary>
        /// <param name="InRectangle"></param>
        /// <param name="ShiftPoint"></param>
        /// <returns></returns>
        public static Rectangle ShiftRectangle(Rectangle InRectangle, Point ShiftPoint)
        {
            return new Rectangle(InRectangle.X - ShiftPoint.X, InRectangle.Y - ShiftPoint.Y, InRectangle.Width, InRectangle.Height);
        }

        /// <summary>
        /// Draw an arrow from the corner of one rectangle to a point in another
        /// </summary>
        /// <param name="g"></param>
        /// <param name="SourceBounds"></param>
        /// <param name="TargetBounds"></param>
        /// <param name="DrawBrush"></param>
        public static void DrawArrow(Graphics g, Rectangle SourceBounds, Rectangle TargetBounds, Brush DrawBrush)
        {

            using (Pen LinePen = new Pen(DrawBrush))
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
        /// Determine if there's a SCADA mismatch        
        /// </summary>
        /// <param name="AnalogTolerance"></param>
        /// <param name="IgnoreNAN"></param>
        /// <param name="ShowBreaker"></param>
        /// <param name="ShowSwitch"></param>
        /// <param name="AnalogsToSearch"></param>
        /// <returns></returns>
        public bool CheckSCADAMismatch(float AnalogTolerance, bool IgnoreNAN, bool ShowBreaker, bool ShowSwitch, List<String> AnalogsToSearch)
        {
            if (OneLineViewer.btnDataSource.Text == "Telemetered")
                return false;
            else if (this.BaseElement is MM_Breaker_Switch)
            {
                if (ElemType == enumElemTypes.Breaker && !ShowBreaker)
                    return false;
                else if (ElemType == enumElemTypes.Switch && !ShowSwitch)
                    return false;
                CheckState CurrentState = CheckState.Indeterminate;
                CheckState SCADAState = CheckState.Indeterminate;

                String ColName = OneLineViewer.btnDataSource.Text + "\\Open";
                if (BaseRow != null && BaseRow.Table.Columns.Contains(ColName) && (BaseRow[ColName] is Boolean))
                    CurrentState = ((bool)BaseRow[ColName] ? CheckState.Checked : CheckState.Unchecked);
                ColName = "Telemetered\\Open";
                if (BaseRow != null && BaseRow.Table.Columns.Contains(ColName) && (BaseRow[ColName] is Boolean))
                    SCADAState = ((bool)BaseRow[ColName] ? CheckState.Checked : CheckState.Unchecked);
                return CurrentState != SCADAState;
            }

            foreach (String str in AnalogsToSearch)
                foreach (String Suffix in ElemType == enumElemTypes.Line ? new String[] { "_1", "_2" } : new String[] { "" })
                {
                    float CurrentState = float.NaN, SCADAState = float.NaN;
                    String ColName = OneLineViewer.btnDataSource.Text + "\\" + str + " (Est)" + Suffix;
                    if (BaseRow != null)
                        if (BaseRow.Table.Columns.Contains(ColName) && (BaseRow[ColName] is Single))
                            CurrentState = (float)BaseRow[ColName];

                    ColName = "Telemetered\\" + str + Suffix;
                    if (BaseRow != null && BaseRow.Table.Columns.Contains(ColName) && (BaseRow[ColName] is Single))
                        SCADAState = ((float)BaseRow[ColName]);

      
                    if (float.IsNaN(CurrentState) && float.IsNaN(SCADAState))
                    { }
                    else if (float.IsNaN(CurrentState) || float.IsNaN(SCADAState))
                    {
                        if (!IgnoreNAN)
                            return true;
                    }
                    else
                    {
                        float Max = Math.Max(CurrentState, SCADAState);
                        float Min = Math.Min(CurrentState, SCADAState);
                        if ((Max - Min)/Max >= AnalogTolerance)
                            return true;
                        
                        //if (Math.Abs((CurrentState - SCADAState) / (Math.Min(CurrentState, SCADAState))) > AnalogTolerance)
                         //   return true;
                    }
                }
            return false;
        }


        /// <summary>
        /// Draw the breaker/switch label
        /// </summary>
        /// <param name="g"></param>
        private void DrawBreakerSwitchLabel(Graphics g)
        {
            MM_Breaker_Switch BaseCBDsc = (MM_Breaker_Switch)this.BaseElement;
            FontStyle DrawFont = FontStyle.Regular;
            if (BaseCBDsc.HasSynchrocheckRelay)
                DrawFont |= FontStyle.Italic;
            if (BaseCBDsc.HasSynchroscope)
                DrawFont |= FontStyle.Bold;

            using (Font f = new Font(this.Font, DrawFont))
            {
                SizeF DrawSize = g.MeasureString(BaseElement.Name, this.Font, this.Bounds.Width, CenterFormat);
                g.DrawString(BaseElement.Name, f, DrawBrush, this.Bounds, CenterFormat);
                Point Center = CenterRect(Bounds);
                DescriptorRectangle = Rectangle.Round(new RectangleF(Center.X - (DrawSize.Width / 2), Center.Y - (DrawSize.Height / 2f), DrawSize.Width, DrawSize.Height));
            }
        }


        /// <summary>
        /// Draw a breaker
        /// </summary>
        /// <param name="g">Graphics connector</param>
        /// <param name="ThisColor">Color to draw the element</param>
        private void DrawBreaker(Graphics g, Color ThisColor)
        {
            CheckState CurrentState = CheckState.Indeterminate;


            String ColName = OneLineViewer.btnDataSource.Text + "\\Open";
            if (BaseRow != null && BaseRow.Table.Columns.Contains(ColName) && (BaseRow[ColName] is Boolean))
                CurrentState = ((bool)BaseRow[ColName] ? CheckState.Checked : CheckState.Unchecked);


            //Determine if we're violated, and if so, change to our worst violation
            using (Pen DrawPen = new Pen(ThisColor))
            using (Brush BackBrush = new SolidBrush(BackColor))
            using (Font DrawFont = new Font("Arial", 13, FontStyle.Bold))
            {
                Point Center = new Point(Width / 2, Height / 2);
                Rectangle DrawRect = new Rectangle(2, 2, Width - 5, Height - 5);

                if (CurrentState == CheckState.Checked)
                {
                    g.FillRectangle(BackBrush, DrawRect);
                    g.DrawRectangle(DrawPen, DrawRect);
                    g.DrawString("O", DrawFont, DrawPen.Brush, DrawRect, CenterFormat);
                }
                else if (CurrentState == CheckState.Unchecked)
                {
                    g.FillRectangle(DrawPen.Brush, DrawRect);
                    g.DrawString("C", DrawFont, BackBrush, DrawRect, CenterFormat);
                }
                else if (CurrentState == CheckState.Indeterminate)
                {
                    g.FillRectangle(BackBrush, DrawRect);
                    using (HatchBrush hb = new HatchBrush(HatchStyle.Percent50, Color.FromArgb(64, KVLevel == null ? Color.Red : KVLevel.Energized.ForeColor), Color.Transparent))
                        g.FillRectangle(hb, DrawRect);
                    g.DrawRectangle(DrawPen, DrawRect);
                    g.DrawString("?", DrawFont, DrawPen.Brush, DisplayRectangle, CenterFormat);
                }

                if (Orientation == enumOrientations.Right || Orientation == enumOrientations.Horizontal)
                    g.DrawLine(DrawPen, 0, Center.Y, DrawRect.Left, Center.Y);
                if (Orientation == enumOrientations.Left || Orientation == enumOrientations.Horizontal)
                    g.DrawLine(DrawPen, DrawRect.Right, Center.Y, Width - 1, Center.Y);
                if (Orientation == enumOrientations.Down || Orientation == enumOrientations.Vertical)
                    g.DrawLine(DrawPen, Center.X, 0, Center.X, DrawRect.Top);
                if (Orientation == enumOrientations.Up || Orientation == enumOrientations.Vertical)
                    g.DrawLine(DrawPen, Center.X, DrawRect.Bottom, Center.X, Height - 1);
            }
        }



        /// <summary>
        /// Draw a switch
        /// </summary>
        /// <param name="g">Graphics connector</param>
        /// <param name="ThisColor">Color to draw the element</param>
        private void DrawSwitch(Graphics g, Color ThisColor)
        {
            //Retrieve the current and telemetered states
            CheckState CurrentState = CheckState.Indeterminate;

            try
            {
                String ColName = OneLineViewer.btnDataSource.Text + "\\Open";
                if (BaseRow != null && BaseRow.Table.Columns.Contains(ColName) && (BaseRow[ColName] is Boolean))
                    CurrentState = ((bool)BaseRow[ColName] ? CheckState.Checked : CheckState.Unchecked);
                //Now, build our drawing brush
                using (Pen DrawPen = new Pen(ThisColor))
                {
                    if (CurrentState == CheckState.Indeterminate)
                    {
                        DrawPen.DashStyle = DashStyle.Dot;
                        using (Font DrawFont = new Font("Arial", 13, FontStyle.Bold))
                            g.DrawString("?", DrawFont, DrawPen.Brush, DisplayRectangle, CenterFormat);
                    }
                    Point Center = new Point(Width / 2, Height / 2);
                    Size SlashSize = new Size(6, 6);
                    bool IsVertical = Orientation == enumOrientations.Vertical || Orientation == enumOrientations.Up || Orientation == enumOrientations.Down;
                    if (CurrentState == CheckState.Unchecked)
                    {
                        g.DrawLine(DrawPen, Point.Subtract(Center, SlashSize), Point.Add(Center, SlashSize));
                        if (IsVertical)
                            g.DrawLine(DrawPen, Center.X, 0, Center.X, Height - 1);
                        else
                            g.DrawLine(DrawPen, 0, Center.Y, Width - 1, Center.Y);
                    }
                    else if (IsVertical)
                    {
                        g.DrawLines(DrawPen, IntToPoints(Center.X, 0, Center.X, Center.Y - SlashSize.Height, Center.X + SlashSize.Width, Center.Y + SlashSize.Height));
                        g.DrawLine(DrawPen, Center.X, Center.Y + SlashSize.Height, Center.X, Height - 1);
                    }
                    else
                    {
                        g.DrawLines(DrawPen, IntToPoints(0, Center.Y, Center.X - SlashSize.Width, Center.Y, Center.X + SlashSize.Width, Center.Y - SlashSize.Height));
                        g.DrawLine(DrawPen, Center.X + SlashSize.Width, Center.Y, Width - 1, Center.Y);

                    }
                }
            }
            catch (RowNotInTableException)
            {
                return;
            }
        }

        /// <summary>
        /// Draw a connectivity node
        /// </summary>
        /// <param name="g"></param>
        /// <param name="ThisColor"></param>
        private void DrawNode(Graphics g, Color ThisColor)
        {
            g.Clear(ThisColor);
        }

        /// <summary>
        /// Draw a poke point
        /// </summary>
        /// <param name="g"></param>
        /// <param name="ThisColor"></param>
        private void DrawPokePoint(Graphics g, Color ThisColor)
        {       
            int CenterX = this.Width / 2, CenterY = this.Height / 2;
            MM_OneLine_PokePoint Poke = this as MM_OneLine_PokePoint;
            Point CenterDisplayRectangle = CenterRect(DisplayRectangle);
            using (Pen pn = new Pen(BaseElement == null || BaseElement.KVLevel == null ? Color.Red : BaseElement.KVLevel.Energized.ForeColor))
                if (Poke.IsJumper && (Orientation == enumOrientations.Horizontal || Orientation == enumOrientations.Left || Orientation == enumOrientations.Right || Orientation == enumOrientations.Unknown))
                    using (SolidBrush BackBrush = new SolidBrush(BackColor))
                    {
                        int JumperSize = (int)Math.Ceiling((float)Width / 4f);
                        g.DrawArc(pn, 0 + JumperSize, 0 + 1, Width - (JumperSize * 2), Height - 2, 0, 180);
                        g.DrawLine(pn, 0, CenterY, 0 + JumperSize, CenterY);
                        g.DrawLine(pn, Width + 1 - JumperSize, CenterY, Width - 1, CenterY);
                    }
                else if (Poke.IsJumper && (Orientation == enumOrientations.Vertical || Orientation == enumOrientations.Up || Orientation == enumOrientations.Down))
                    using (SolidBrush BackBrush = new SolidBrush(BackColor))
                    {
                        int JumperSize = (int)Math.Ceiling((float)Height / 4f);
                        g.DrawArc(pn, 0, 0 + JumperSize, Width - 1, Height - (JumperSize * 2), 90, 180);
                        g.DrawLine(pn, CenterX, 0, CenterX, 0 + JumperSize);
                        g.DrawLine(pn, CenterX, Height + 1 - JumperSize, CenterX, Height - 1);
                    }
                else if (Poke.IsVisible)
                    g.FillRectangle(pn.Brush, DisplayRectangle);
        }

        /// <summary>
        /// Draw a pricing vector
        /// </summary>
        /// <param name="g"></param>
        /// <param name="IsPositive"></param>
        /// <param name="DrawColor"></param>
        public void DrawPricingVector(Graphics g, Color DrawColor, bool IsPositive)
        {
            //First, draw our rectangle
            Rectangle Bounds = this.DisplayRectangle;
            
            //Then, draw our arrow as appropriate
            using (Pen p = new Pen(DrawColor))
            {
                g.DrawRectangle(p, Bounds.Left, Bounds.Top, Bounds.Width - 1, Bounds.Height - 1);
                p.Width = 3;
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
        /// Compute the alignment between an element and its label
        /// </summary>
        /// <param name="ElementBounds"></param>
        /// <param name="LabelBounds"></param>
        /// <param name="vAlign"></param>
        /// <param name="hAlign"></param>
        public static void ComputeAlignment(Rectangle ElementBounds, Rectangle LabelBounds, out StringAlignment vAlign, out StringAlignment hAlign)
        {
            if (LabelBounds.Bottom <= ElementBounds.Top)
                vAlign = StringAlignment.Far;
            else if (LabelBounds.Top >= ElementBounds.Bottom)
                vAlign = StringAlignment.Near;
            else
                vAlign = StringAlignment.Center;

            //Do the same for horizontal
            if (LabelBounds.Right <= ElementBounds.Left)
                hAlign = StringAlignment.Far;
            else if (LabelBounds.Left >= ElementBounds.Right)
                hAlign = StringAlignment.Near;
            else
                hAlign = StringAlignment.Center;

        }

        /// <summary>
        /// Draw a descriptor or secondary descriptor
        /// </summary>
        /// <param name="g"></param>
        public void DrawDescriptor(Graphics g)
        {
            StringAlignment vAlign, hAlign;
            Rectangle ParentBounds = ShiftRectangle(ParentElement.Bounds, OneLineViewer.pnlElements.AutoScrollPosition);
            
            ComputeAlignment(ParentBounds, Bounds, out vAlign, out hAlign);


            //If we have notes, draw the note icon.
            MM_Element CoreElement;
            if (MM_Repository.TEIDs.TryGetValue(BaseElement.TEID, out CoreElement))
                if (CoreElement.FindNotes().Length > 0)
                    g.DrawImageUnscaled(Macomber_Map.Properties.Resources.NoteHS, ParentBounds.Left - 16, ParentBounds.Top, 16,16);


            //g.FillRectangle(Brushes.Purple, Bounds);            
            using (StringFormat sF = new StringFormat(StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip | StringFormatFlags.NoWrap))
            {

                sF.Alignment = hAlign;
                sF.LineAlignment = vAlign;
                sF.Trimming = StringTrimming.None;
                sF.SetTabStops(0, new float[] { 75f });
                if (ParentElement.ElemType == enumElemTypes.Breaker || ParentElement.ElemType == enumElemTypes.Switch)
                {
                    if (OneLineViewer.btnNames.Checked)
                        DrawBreakerSwitchLabel(g);
                }
                else if (ElemType == enumElemTypes.SecondaryDescriptor)
                    using (Font ItalicFont = new Font(this.Font, FontStyle.Italic))
                        if (xElement.ParentNode.Attributes["ResourceNode"] != null)
                            g.DrawString(xElement.ParentNode.Attributes["ResourceNode"].Value, ItalicFont, DrawBrush, this.Bounds, sF);
                        else if (xElement.Attributes["DescriptorText"] != null)
                            g.DrawString(xElement.Attributes["DescriptorText"].Value, ItalicFont, DrawBrush, this.Bounds, sF);
                        else if (xElement.Attributes["Text"] != null)
                            g.DrawString(xElement.Attributes["Text"].Value, ItalicFont, DrawBrush, this.Bounds, sF);
                        else
                            g.DrawString("?", ItalicFont, DrawBrush, this.Bounds, sF);


                else
                    try
                    {                        
                        String DescriptorText = this.DescriptorText;
                        SizeF StringSize = g.MeasureString(DescriptorText, this.Font, this.Width, sF);
                        float Left = hAlign == StringAlignment.Near ? this.Left : hAlign == StringAlignment.Far ? this.Right - StringSize.Width : this.Center.X - (StringSize.Width / 2f);
                        float Top = vAlign == StringAlignment.Near ? this.Top : vAlign == StringAlignment.Far ? this.Bottom - StringSize.Height : this.Center.Y - (StringSize.Height / 2f);
                        DescriptorRectangle = Rectangle.Round(new RectangleF(Left, Top, StringSize.Width, StringSize.Height));
                        g.DrawString(DescriptorText, this.Font, DrawBrush, this.Bounds, sF);
                    }
                    catch (Exception ex)
                    {
                        g.DrawString(ex.Message, this.Font, Brushes.Red, this.Bounds, sF);
                    }
            }
        }

        /// <summary>
        /// Draw a capactior
        /// </summary>
        /// <param name="g"></param>
        /// <param name="ThisColor"></param>
        /// <param name="Bounds"></param>
        private void DrawCapacitor(Graphics g, Rectangle Bounds, Color ThisColor)
        {
            //Prepare to draw the switch, set up its color/width, and descriptor label
            Point Center = CenterRect(Bounds);
            using (Pen DrawingPen = new Pen(ThisColor))
                if (Orientation == enumOrientations.Up)
                {
                    g.DrawLine(DrawingPen, Center.X, Bounds.Top, Center.X, Bounds.Bottom - 1);
                    g.DrawLine(DrawingPen, Bounds.Left, Bounds.Top, Bounds.Right - 1, Bounds.Top);
                    g.DrawArc(DrawingPen, Bounds.Left, Bounds.Top + 1, Bounds.Width - 1, Bounds.Height - 1, 180, 180);
                }
                else if (Orientation == enumOrientations.Down || Orientation == enumOrientations.Vertical)
                {
                    g.DrawLine(DrawingPen, Center.X, Bounds.Top, Center.X, Bounds.Bottom - 1);
                    g.DrawLine(DrawingPen, Bounds.Left, Bounds.Bottom - 1, Bounds.Right - 1, Bounds.Bottom - 1);
                    g.DrawArc(DrawingPen, Bounds.Left, Bounds.Top - 1, Bounds.Width - 1, Bounds.Height - 1, 0, 180);
                }
                else if (Orientation == enumOrientations.Left || Orientation == enumOrientations.Unknown)
                {
                    g.DrawLine(DrawingPen, Bounds.Left, Center.Y, Bounds.Right - 1, Center.Y);
                    g.DrawLine(DrawingPen, Bounds.Left, Bounds.Top, Bounds.Left, Bounds.Bottom - 1);
                    g.DrawArc(DrawingPen, Bounds.Left + 1, Bounds.Top, Bounds.Width - 1, Bounds.Height - 1, 90, 180);
                }
                else if (Orientation == enumOrientations.Right || Orientation == enumOrientations.Horizontal)
                {
                    g.DrawLine(DrawingPen, Bounds.Left, Center.Y, Bounds.Right - 1, Center.Y);
                    g.DrawLine(DrawingPen, Bounds.Right - 1, Bounds.Top, Bounds.Right - 1, Bounds.Bottom - 1);
                    g.DrawArc(DrawingPen, Bounds.Left - 1, Bounds.Top, Bounds.Width - 1, Bounds.Height - 1, 270, 180);
                }
              }

        /// <summary>
        /// Draw a reactor
        /// </summary>
        /// <param name="g"></param>
        /// <param name="Bounds"></param>
        /// <param name="ThisColor"></param>
        private void DrawReactor(Graphics g, Rectangle Bounds, Color ThisColor)
        {
            Point[] OutPoints = IntToPoints(0, 12, 1, 12, 2, 13, 3, 15, 4, 18, 5, 20, 6, 22, 7, 23, 8, 24, 10, 24, 12, 23, 14, 21, 15, 19, 16, 16, 17, 12, 17, 7, 16, 4, 15, 2, 12, 0, 10, 0, 8, 1, 6, 3, 5, 6, 5, 11, 7, 17, 8, 19, 10, 22, 11, 23, 13, 24, 15, 24, 16, 24, 18, 23, 19, 22, 21, 19, 22, 16, 23, 12, 23, 7, 22, 4, 21, 2, 18, 0, 16, 0, 14, 1, 13, 2, 12, 3, 11, 5, 10, 8, 10, 12, 11, 15, 13, 19, 15, 22, 18, 24, 20, 24, 22, 23, 23, 22, 25, 19, 26, 16, 27, 13, 28, 12, 30, 12, 31, 12);
            for (int a = 0; a < OutPoints.Length; a++)
                if (Orientation == enumOrientations.Vertical || Orientation == enumOrientations.Up || Orientation == enumOrientations.Down)
                    OutPoints[a] = new Point(OutPoints[a].Y + Bounds.Left, OutPoints[a].X + Bounds.Top);
                else
                    OutPoints[a] = new Point(OutPoints[a].X + Bounds.Left, OutPoints[a].Y + Bounds.Top);            

            using (Pen ThisPen = new Pen(ThisColor))
                g.DrawLines(ThisPen, OutPoints);
        }

        /// <summary>
        /// Draw a resistor
        /// </summary>
        /// <param name="g"></param>
        /// <param name="Bounds"></param>
        /// <param name="DrawColor"></param>
        public void DrawResistor(Graphics g, Rectangle Bounds, Color DrawColor)
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

            using (Pen ThisPen = new Pen(DrawColor))
                g.DrawLines(ThisPen, OutPoints.ToArray());
        }

        /// <summary>
        /// Draw a resistor/reactor hybrid
        /// </summary>
        /// <param name="g"></param>
        /// <param name="Bounds"></param>
        /// <param name="DrawColor"></param>
        public void DrawResistorCapacitor(Graphics g, Rectangle Bounds, Color DrawColor)
        {
            if (Orientation == enumOrientations.Up || Orientation == enumOrientations.Vertical || Orientation == enumOrientations.Down || Orientation == enumOrientations.Unknown)
            {
                DrawCapacitor(g, new Rectangle(Bounds.Left, Bounds.Top, Bounds.Width, Bounds.Height / 2), DrawColor);
                DrawResistor(g, new Rectangle(Bounds.Left, Bounds.Top + (Bounds.Height / 2), Bounds.Width, Bounds.Height / 2), DrawColor);
            }
            else
            {
                DrawCapacitor(g, new Rectangle(Bounds.Left, Bounds.Top, Bounds.Width, Bounds.Height), DrawColor);
                DrawResistor(g, new Rectangle(Bounds.Left + (Bounds.Width / 2), Bounds.Top, Bounds.Width / 2, Bounds.Height), DrawColor);
            }
        }

        /// <summary>
        /// Draw a resistor/reactor hybrid
        /// </summary>
        /// <param name="g"></param>
        /// <param name="Bounds"></param>
        /// <param name="DrawColor"></param>
        public void DrawResistorReactor(Graphics g, Rectangle Bounds, Color DrawColor)
        {
            if (Orientation == enumOrientations.Up || Orientation == enumOrientations.Vertical || Orientation == enumOrientations.Down || Orientation == enumOrientations.Unknown)
            {
                DrawReactor(g, new Rectangle(Bounds.Left, Bounds.Top, Bounds.Width, Bounds.Height / 2), DrawColor);
                DrawResistor(g, new Rectangle(Bounds.Left, Bounds.Top + (Bounds.Height / 2), Bounds.Width, Bounds.Height / 2), DrawColor);
            }
            else
            {
                DrawReactor(g, new Rectangle(Bounds.Left, Bounds.Top, Bounds.Width, Bounds.Height), DrawColor);
                DrawResistor(g, new Rectangle(Bounds.Left + (Bounds.Width / 2), Bounds.Top, Bounds.Width / 2, Bounds.Height), DrawColor);
            }
        }

        /// <summary>
        /// Draw a static var compensator
        /// </summary>
        /// <param name="g"></param>
        /// <param name="ForeColor"></param>
        public void DrawStaticVarCompensator(Graphics g, Color ForeColor)
        {
            using (Pen DrawPen = new Pen(ForeColor))
            {
                //g.FillRectangle(Brushes.Black, this.DisplayRectangle);
                if (Orientation == enumOrientations.Up || Orientation == enumOrientations.Vertical)
                {
                    List<Point> OutPoint = new List<Point>();
                    foreach (Point pt in IntToPoints(18, 0, 18, 1, 16, 2, 14, 3, 12, 4, 9, 6, 8, 7, 7, 8, 6, 10, 6, 12, 7, 13, 8, 14, 10, 15, 13, 16, 16, 17, 19, 17, 22, 17, 26, 16, 28, 15, 30, 14, 31, 12, 31, 10, 30, 8, 29, 7, 28, 6, 26, 5, 23, 4, 20, 4, 17, 5, 15, 6, 13, 7, 10, 9, 8, 11, 7, 12, 6, 14, 6, 16, 8, 18, 12, 20, 16, 21, 17, 21, 20, 21, 24, 20, 27, 19, 30, 17, 31, 15, 31, 13, 30, 11, 28, 10, 26, 9, 23, 8, 21, 8, 19, 8, 17, 9, 15, 10, 13, 11, 10, 13, 8, 15, 7, 16, 6, 17, 6, 19, 7, 20, 8, 21, 9, 22, 11, 23, 13, 24, 16, 25, 18, 25, 18, 26, 18, 35))
                        OutPoint.Add(new Point(pt.X + 0, pt.Y + 0));
                    g.DrawLines(DrawPen, OutPoint.ToArray());
                    g.DrawLine(DrawPen, 0 + 63, 0 + 2, 0 + 63, 0 + 8);
                    g.DrawLine(DrawPen, 0 + 55, 0 + 8, 0 + 71, 0 + 8);
                    g.DrawLine(DrawPen, 0 + 63, 0 + 10, 0 + 63, 0 + 35);
                    g.DrawArc(DrawPen, 0 + 55, 0 + 8, 16, 16, 180, 180);
                    g.DrawLine(DrawPen, 0 + 18, 0 + 35, 0 + 63, 0 + 35);
                    g.DrawLine(DrawPen, 0 + (Bounds.Width / 2), 0 + 35, 0 + Bounds.Width / 2, Height);
                }
                else if (Orientation == enumOrientations.Down)
                {
                    List<Point> OutPoint = new List<Point>();
                    foreach (Point pt in IntToPoints(62, 36, 62, 42, 60, 43, 58, 44, 56, 45, 53, 47, 52, 48, 51, 49, 50, 51, 50, 53, 51, 54, 52, 55, 54, 56, 57, 57, 60, 58, 63, 58, 66, 58, 70, 57, 72, 56, 74, 55, 75, 53, 75, 51, 74, 49, 73, 48, 72, 47, 70, 46, 67, 45, 64, 45, 61, 46, 59, 47, 57, 48, 54, 50, 52, 52, 51, 53, 50, 55, 50, 57, 52, 59, 56, 61, 60, 62, 61, 62, 64, 62, 68, 61, 71, 60, 74, 58, 75, 56, 75, 54, 74, 52, 72, 51, 70, 50, 67, 49, 65, 49, 63, 49, 61, 50, 59, 51, 57, 52, 54, 54, 52, 56, 51, 57, 50, 58, 50, 60, 51, 61, 52, 62, 53, 63, 55, 64, 57, 65, 60, 66, 62, 66, 62, 67, 62, 69))
                        OutPoint.Add(new Point(pt.X + 0, pt.Y + 0));
                    g.DrawLines(DrawPen, OutPoint.ToArray());

                    g.DrawLine(DrawPen, 0 + 21, 0 + 36, 0 + 21, 0 + 48);
                    g.DrawLine(DrawPen, 0 + 13, 0 + 48, 0 + 29, 0 + 48);
                    g.DrawLine(DrawPen, 0 + 21, 0 + 50, 0 + 21, 0 + 66);
                    g.DrawArc(DrawPen, 0 + 13, 0 + 50, 16, 16, 180, 180);
                    g.DrawLine(DrawPen, 0 + 21, 0 + 36, 0 + 62, 0 + 36);
                    g.DrawLine(DrawPen, 0 + (Bounds.Width / 2), 0, 0 + (Bounds.Width / 2), 0 + 36);
                }
                else if (Orientation == enumOrientations.Left || Orientation == enumOrientations.Horizontal || Orientation == enumOrientations.Unknown)
                {
                    List<Point> OutPoint = new List<Point>();
                    foreach (Point pt in IntToPoints(35, 27, 40, 27, 41, 28, 42, 30, 43, 33, 44, 35, 45, 37, 46, 38, 47, 39, 49, 39, 51, 38, 53, 36, 54, 34, 55, 31, 56, 27, 56, 22, 55, 19, 54, 17, 51, 15, 49, 15, 47, 16, 45, 18, 44, 21, 44, 26, 46, 32, 47, 34, 49, 37, 50, 38, 52, 39, 54, 39, 52, 32, 57, 38, 58, 37, 60, 34, 61, 31, 62, 27, 62, 22, 61, 19, 60, 17, 57, 15, 55, 15, 53, 16, 52, 17, 51, 18, 50, 20, 49, 23, 49, 27, 50, 30, 52, 34, 54, 37, 57, 39, 59, 39, 61, 38, 62, 37, 64, 34, 65, 31, 66, 28, 67, 27, 69, 27, 71, 27))
                        OutPoint.Add(new Point(pt.X + 0, pt.Y + 0));
                    g.DrawLines(DrawPen, OutPoint.ToArray());

                    g.DrawLine(DrawPen, 0 + 62, 0 + 53, 0 + 56, 0 + 53);
                    g.DrawLine(DrawPen, 0 + 56, 0 + 45, 0 + 56, 0 + 61);
                    g.DrawLine(DrawPen, 0 + 54, 0 + 53, 0 + 35, 0 + 53);
                    g.DrawArc(DrawPen, 0 + 38, 0 + 45, 16, 16, 270, 180);
                    g.DrawLine(DrawPen, 0 + 35, 0 + 27, 0 + 35, 0 + 53);
                    g.DrawLine(DrawPen, 0, 0 + (Bounds.Height / 2), 0 + 35, 0 + (Bounds.Height / 2));
                }
                else if (Orientation == enumOrientations.Right)
                {
                    List<Point> OutPoint = new List<Point>();
                    foreach (Point pt in IntToPoints(1, 18, 2, 18, 3, 19, 4, 21, 5, 24, 6, 26, 7, 28, 8, 29, 9, 30, 11, 30, 13, 29, 15, 27, 16, 25, 17, 22, 18, 18, 18, 13, 17, 10, 16, 8, 13, 6, 11, 6, 9, 7, 7, 9, 6, 12, 6, 17, 8, 23, 9, 25, 11, 28, 12, 29, 14, 30, 16, 30, 17, 30, 19, 29, 20, 28, 22, 25, 23, 22, 24, 18, 24, 13, 23, 10, 22, 8, 19, 6, 17, 6, 15, 7, 14, 8, 13, 9, 12, 11, 11, 14, 11, 18, 12, 21, 14, 25, 16, 28, 19, 30, 21, 30, 23, 29, 24, 28, 26, 25, 27, 22, 28, 19, 29, 18, 31, 18, 33, 18))
                        OutPoint.Add(new Point(pt.X + 0, pt.Y + 0));
                    g.DrawLines(DrawPen, OutPoint.ToArray());


                    g.DrawLine(DrawPen, 0 + 13, 0 + 58, 0 + 7, 0 + 58);
                    g.DrawLine(DrawPen, 0 + 13, 0 + 50, 0 + 13, 0 + 66);
                    g.DrawLine(DrawPen, 0 + 33, 0 + 58, 0 + 15, 0 + 58);
                    g.DrawArc(DrawPen, 0 + 15, 0 + 50, 16, 16, 90, 180);
                    g.DrawLine(DrawPen, 0 + 34, 0 + 18, 0 + 34, 0 + 58);
                    g.DrawLine(DrawPen, 0 + 34, 0 + (Bounds.Height / 2), Width, 0 + (Bounds.Height / 2));

                }
            }
        }


        /// <summary>
        /// Draw a unit
        /// </summary>
        /// <param name="g"></param>
        /// <param name="ThisColor"></param>
        private void DrawUnit(Graphics g, Color ThisColor)
        {
            float GenerationLevel = 0.5f;
            Point Center = CenterRect(DisplayRectangle);
            using (Pen ThisPen = new Pen(ThisColor))
            using (Brush BackBrush = new SolidBrush(BackColor))
                if (Orientation == enumOrientations.Up || Orientation == enumOrientations.Vertical)
                {
                    g.DrawLine(ThisPen, Center.X - 3, Height - 4, Center.X + 3, Height - 4);
                    g.DrawLine(ThisPen, Center.X, Center.Y, Center.X, Height - 1);
                    g.FillPie(BackBrush, 0, 1 - Center.Y, Width - 1, Height - 1, 0, 180);
                    g.DrawLine(ThisPen, 0, 0, Width - 1, 0);
                    g.DrawArc(ThisPen, 0, 1 - Center.Y, Width - 1, Height - 1, 0, 180);
                    g.FillPie(ThisPen.Brush, 0, 1 - Center.Y, Width - 1, Height - 1, 0, 180f * GenerationLevel);
                }
                else if (Orientation == enumOrientations.Down)
                {
                    g.DrawLine(ThisPen, Center.X - 3, 3, Center.X + 3, 3);
                    g.DrawLine(ThisPen, Center.X, 0, Center.X, Center.Y);
                    g.FillPie(BackBrush, 0, Center.Y, Width - 1, Height - 1, 180, 180);
                    g.DrawLine(ThisPen, 0, Height - 1, Width - 1, Height - 1);
                    g.DrawArc(ThisPen, 0, Center.Y, Width - 1, Height - 1, 180, 180);
                    g.FillPie(ThisPen.Brush, 0, Center.Y, Width - 1, Height - 1, 180, 180f * GenerationLevel);

                }
                else if (Orientation == enumOrientations.Left || Orientation == enumOrientations.Horizontal || Orientation == enumOrientations.Unknown)
                {
                    g.DrawLine(ThisPen, Width - 4, Center.Y - 3, Width - 4, Center.Y + 3);
                    g.DrawLine(ThisPen, Center.X + 1, Center.Y, Width - 1, Center.Y);
                    g.FillPie(BackBrush, 1 - Center.X, 0, Width - 1, Height - 1, 270, 180);
                    g.DrawLine(ThisPen, 0, 0, 0, Height - 1);
                    g.DrawArc(ThisPen, 1 - Center.X, 0, Width - 1, Height - 1, 270, 180);
                    g.FillPie(ThisPen.Brush, 1 - Center.X, 0, Width - 1, Height - 1, 270, 180f * GenerationLevel);
                }
                else if (Orientation == enumOrientations.Right)
                {
                    g.DrawLine(ThisPen, 3, Center.Y - 3, 3, Center.Y + 3);
                    g.DrawLine(ThisPen, 0, Center.Y, Center.X, Center.Y);
                    g.FillPie(BackBrush, Center.X, 0, Width - 1, Height - 1, 90, 180);
                    g.DrawLine(ThisPen, Width - 1, 0, Width - 1, Height - 1);
                    g.DrawArc(ThisPen, Center.X, 0, Width - 1, Height - 1, 90, 180);
                    g.FillPie(ThisPen.Brush, Center.X, 0, Width - 1, Height - 1, 90, 180f * GenerationLevel);
                }                                 
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
        /// Draw a line graphic and text on the screen
        /// </summary>
        /// <param name="g"></param>
        /// <param name="ThisColor"></param>
        private void DrawLine(Graphics g, Color ThisColor)
        {
            Point CenterBounds = CenterRect(DisplayRectangle);
            using (Pen ThisPen = new Pen(ThisColor))
            if (Orientation == enumOrientations.Up)
            {
                g.DrawLine(ThisPen, CenterBounds.X, CenterBounds.Y, CenterBounds.X, Height - 1);
                g.FillPolygon(ThisPen.Brush, IntToPoints(0, CenterBounds.Y, Width - 1, CenterBounds.Y, CenterBounds.X, 0, 0, CenterBounds.Y));
            }
            else if (Orientation == enumOrientations.Down)
            {
                g.DrawLine(ThisPen, CenterBounds.X, 0, CenterBounds.X, CenterBounds.Y);
                g.FillPolygon(ThisPen.Brush, IntToPoints(0, CenterBounds.Y, Width - 1, CenterBounds.Y, CenterBounds.X, Height - 1, 0, CenterBounds.Y));
            }
            else if (Orientation == enumOrientations.Left)
            {
                g.DrawLine(ThisPen, CenterBounds.X, CenterBounds.Y, Width - 1, CenterBounds.Y);
                g.FillPolygon(ThisPen.Brush, IntToPoints(CenterBounds.X, 0, CenterBounds.X, Height - 1, 0, CenterBounds.Y, CenterBounds.X, 0));
            }
            else if (Orientation == enumOrientations.Right)
            {
                g.DrawLine(ThisPen, 0, CenterBounds.Y, CenterBounds.X, CenterBounds.Y);
                g.FillPolygon(ThisPen.Brush, IntToPoints(CenterBounds.X, 0, CenterBounds.X, Height - 1, Width - 1, CenterBounds.Y, CenterBounds.X, 0));
            }
            else if (Orientation == enumOrientations.Horizontal || Orientation == enumOrientations.Unknown)
            {
                g.FillPolygon(ThisPen.Brush, IntToPoints(0, 0, 0, Height - 1, CenterBounds.X, CenterBounds.Y, 0, 0));
                g.FillPolygon(ThisPen.Brush, IntToPoints(Width - 1, 0, Width - 1, Height - 1, CenterBounds.X, CenterBounds.Y, Width - 1, 0));
                g.DrawLine(ThisPen, Width - 1, 0, Width - 1, Height - 1);
            }
            else if (Orientation == enumOrientations.Vertical)
            {
                g.FillPolygon(ThisPen.Brush, IntToPoints(0, 0, Width - 1, 0, CenterBounds.X, CenterBounds.Y, 0, 0));
                g.FillPolygon(ThisPen.Brush, IntToPoints(0, Height - 1, Width - 1, Height - 1, CenterBounds.X, CenterBounds.Y, 0, Height - 1));
                g.DrawLine(ThisPen, 0, Height - 1, Width - 1, Height - 1);
            }
        }

        /// <summary>
        /// Set the appropriately-arrowed flow direction
        /// </summary>
        /// <param name="inValue">The numeric value</param>
        /// <param name="FlowCharacters">The two characters used for flow (out then in)</param>
        /// <param name="OutText">The text box to be written to</param>
        private void SetNumericValue(Control OutText, TelemComparer inValue, String FlowCharacters)
        {
            if (float.IsNaN(inValue.Value))
                OutText.Text = " ?";
            else if (inValue.Value > 0)
                OutText.Text = FlowCharacters[0] + " " + inValue.Value.ToString("#,##0.0");
            else if (inValue.Value < 0)
                OutText.Text = FlowCharacters[1] + " " + Math.Abs(inValue.Value).ToString("#,##0.0");
            else
                OutText.Text = "  0";
            (OutText as Label).BackColor = Color.Transparent;
            (OutText as Label).Font = (inValue.Diff ? new Font("Times New Roman", 8.5f, FontStyle.Underline): new Font("Times New Roman",8.5f));                
        }

        /// <summary>
        /// Pull in the specified telemetry. If none is available, return NAN. If SCADA is available, compare it.
        /// </summary>
        /// <param name="splStr">The data lookups</param>
        /// <param name="SourceCol">The source column name</param>
        /// <param name="BaseRow">The line's current station telemetry</param>                
        /// <returns>Whether this telemetry differs from SCADA</returns>
        private TelemComparer CheckTelemetry(string[] splStr, string SourceCol, DataRow BaseRow)
        {

            if (splStr[0] == "SCADA")
            {
                if (BaseRow["SCADA\\" + SourceCol] is float)
                    return new TelemComparer((float)BaseRow["SCADA\\" + SourceCol], false);
                else
                    return new TelemComparer(float.NaN, false);
                
            }
            else
            {
                float OutValue = float.NaN;
                if (splStr.Length == 2 && BaseRow.Table.Columns.Contains(splStr[0] + "\\" + SourceCol + " " + splStr[1]) && (BaseRow[splStr[0] + "\\" + SourceCol + " " + splStr[1]] is float))
                    OutValue = (float)BaseRow[splStr[0] + "\\" + SourceCol + " " + splStr[1]];
                else if ((splStr.Length == 2 && BaseRow.Table.Columns.Contains(splStr[0] + "\\" + SourceCol) && (BaseRow[splStr[0] + "\\" + SourceCol] is float)))
                    OutValue = (float)BaseRow[splStr[0] + "\\" + SourceCol];

                if (BaseRow.Table.Columns.Contains("SCADA\\" + SourceCol) && BaseRow["SCADA\\" + SourceCol] is float)
                {
                    float SCADAVal = (float)BaseRow["SCADA\\" + SourceCol];
                    return new TelemComparer(OutValue, PercentageDiff(OutValue, SCADAVal));
                }
                else
                    return new TelemComparer(OutValue, false);
            }
        }

        /// <summary>
        /// Returns true if the two values are sufficiently different
        /// </summary>
        /// <param name="Val1">The first value</param>
        /// <param name="Val2">The second value</param>
        /// <returns></returns>
        private bool PercentageDiff(float Val1, float Val2)
        {
            float Diff = (Math.Abs(Val1 - Val2));
            if (float.IsNaN(Diff))
                return true;
            float Max = Math.Max(Math.Abs(Val1), Math.Abs(Val2));
            return (Diff/Max) >=  OneLineViewer.SCADAThreshold;
        }


        /// <summary>
        /// Build a series of points, in the format (x,y,x,y,x,y)
        /// </summary>
        /// <param name="inNumbers">The collection of numbers</param>
        /// <returns></returns>
        private Point[] BuildPoints(params int[] inNumbers)
        {
            Point[] outPoints = new Point[(inNumbers.Length / 2)];
            for (int a = 0; a < outPoints.Length; a++)
                if (a == 0)
                    outPoints[a] = new Point(inNumbers[0], inNumbers[1]);
                else
                    outPoints[a] = new Point(inNumbers[a * 2] + outPoints[a - 1].X, inNumbers[(a * 2) + 1] + outPoints[a - 1].Y);
            //outPoints[outPoints.Length - 1] = outPoints[0];
            return outPoints;
        }

        /// <summary>
        /// Draw a LaaR
        /// </summary>
        /// <param name="g"></param>
        /// <param name="DrawColor"></param>
        public void DrawLaaR(Graphics g, Color DrawColor)
        {
            Point CenterBounds = new Point(Width / 2, Height / 2);
            using (Pen ThisPen = new Pen(DrawColor))
                if (Orientation == enumOrientations.Up)
                {
                    g.DrawLine(ThisPen, CenterBounds.X, CenterBounds.Y, CenterBounds.X, Height - 1);
                    g.DrawPolygon(ThisPen, IntToPoints(0, CenterBounds.Y, Width - 1, CenterBounds.Y, CenterBounds.X, 0, 0, CenterBounds.Y));
                    g.DrawLine(ThisPen, 0, 0, Width - 1, Height - 1);
                }
                else if (Orientation == enumOrientations.Down)
                {
                    g.DrawLine(ThisPen, CenterBounds.X, 0, CenterBounds.X, CenterBounds.Y);
                    g.DrawPolygon(ThisPen, IntToPoints(0, CenterBounds.Y, Width - 1, CenterBounds.Y, CenterBounds.X, Height - 1, 0, CenterBounds.Y));
                    g.DrawLine(ThisPen, 0, 0, Width - 1, Height - 1);
                }
                else if (Orientation == enumOrientations.Left)
                {
                    g.DrawLine(ThisPen, CenterBounds.X, CenterBounds.Y, Width - 1, CenterBounds.Y);
                    g.DrawPolygon(ThisPen, IntToPoints(CenterBounds.X, 0, CenterBounds.X, Height - 1, 0, CenterBounds.Y, CenterBounds.X, 0));
                    g.DrawLine(ThisPen, 0, 0, Width - 1, Height - 1);
                }
                else if (Orientation == enumOrientations.Right)
                {
                    g.DrawLine(ThisPen, 0, CenterBounds.Y, CenterBounds.X, CenterBounds.Y);
                    g.DrawPolygon(ThisPen, IntToPoints(CenterBounds.X, 0, CenterBounds.X, Height - 1, Width - 1, CenterBounds.Y, CenterBounds.X, 0));
                    g.DrawLine(ThisPen, 0, 0, Width - 1, Height - 1);
                }
                else if (Orientation == enumOrientations.Horizontal || Orientation == enumOrientations.Unknown)
                {
                    g.DrawPolygon(ThisPen, IntToPoints(0, 0, 0, Height - 1, CenterBounds.X, CenterBounds.Y, 0, 0));
                    g.DrawPolygon(ThisPen, IntToPoints(Width - 1, 0, Width - 1, Height - 1, CenterBounds.X, CenterBounds.Y, Width - 1, 0));
                    g.DrawLine(ThisPen, CenterBounds.X / 2, 0, CenterBounds.X / 2, Height - 1);
                    g.DrawLine(ThisPen, (CenterBounds.X / 2) + CenterBounds.X, 0, (CenterBounds.X / 2) + CenterBounds.X, Height - 1);
                }
                else if (Orientation == enumOrientations.Vertical)
                {
                    g.DrawPolygon(ThisPen, IntToPoints(0, 0, Width - 1, 0, CenterBounds.X, CenterBounds.Y, 0, 0));
                    g.DrawPolygon(ThisPen, IntToPoints(0, Height - 1, Width - 1, Height - 1, CenterBounds.X, CenterBounds.Y, 0, Height - 1));
                    g.DrawLine(ThisPen, 0, CenterBounds.Y / 2, Width - 1, CenterBounds.Y / 2);
                    g.DrawLine(ThisPen, 0, (CenterBounds.Y / 2) + CenterBounds.Y, Width - 1, (CenterBounds.Y / 2) + CenterBounds.Y);
                }
        }

        /// <summary>
        /// Draw a load
        /// </summary>
        /// <param name="g"></param>
        /// <param name="ThisColor"></param>
        private void DrawLoad(Graphics g, Color ThisColor)
        {
            Point CenterBounds = CenterRect(DisplayRectangle);
            using (Pen ThisPen = new Pen(ThisColor))
            if (Orientation == enumOrientations.Up)
            {
                g.DrawLine(ThisPen, CenterBounds.X, CenterBounds.Y, CenterBounds.X, Height - 1);
                g.DrawPolygon(ThisPen, IntToPoints(0, CenterBounds.Y, Width - 1, CenterBounds.Y, CenterBounds.X, 0, 0, CenterBounds.Y));
            }
            else if (Orientation == enumOrientations.Down)
            {
                g.DrawLine(ThisPen, CenterBounds.X, 0, CenterBounds.X, CenterBounds.Y);
                g.DrawPolygon(ThisPen, IntToPoints(0, CenterBounds.Y, Width - 1, CenterBounds.Y, CenterBounds.X, Height - 1, 0, CenterBounds.Y));
            }
            else if (Orientation == enumOrientations.Left)
            {
                g.DrawLine(ThisPen, CenterBounds.X, CenterBounds.Y, Width - 1, CenterBounds.Y);
                g.DrawPolygon(ThisPen, IntToPoints(CenterBounds.X, 0, CenterBounds.X, Height - 1, 0, CenterBounds.Y, CenterBounds.X, 0));
            }
            else if (Orientation == enumOrientations.Right)
            {
                g.DrawLine(ThisPen, 0, CenterBounds.Y, CenterBounds.X, CenterBounds.Y);
                g.DrawPolygon(ThisPen, IntToPoints(CenterBounds.X, 0, CenterBounds.X, Height - 1, Width - 1, CenterBounds.Y, CenterBounds.X, 0));
            }
            else if (Orientation == enumOrientations.Horizontal || Orientation == enumOrientations.Unknown)
            {
                g.DrawPolygon(ThisPen, IntToPoints(0, 0, 0, Height - 1, CenterBounds.X, CenterBounds.Y, 0, 0));
                g.DrawPolygon(ThisPen, IntToPoints(Width - 1, 0, Width - 1, Height - 1, CenterBounds.X, CenterBounds.Y, Width - 1, 0));
            }
            else if (Orientation == enumOrientations.Vertical)
            {
                g.DrawPolygon(ThisPen, IntToPoints(0, 0, Width - 1, 0, CenterBounds.X, CenterBounds.Y, 0, 0));
                g.DrawPolygon(ThisPen, IntToPoints(0, Height - 1, Width - 1, Height - 1, CenterBounds.X, CenterBounds.Y, 0, Height - 1));
            }          
        }

        /// <summary>
        /// Return an appropriately-formatted float
        /// </summary>
        /// <param name="InValue"></param>
        /// <returns></returns>
        float HandleFloat(Object InValue)
        {
            if (InValue is DBNull)
                return float.NaN;
            else
                return (float)InValue;
        }

        /// <summary>
        /// Attempt to find a value in our table
        /// </summary>
        /// <param name="ColumnName"></param>
        /// <param name="Result"></param>
        /// <returns></returns>
        private bool TryFindVal(String ColumnName, out object Result)
        {
            DataColumn dCol;
            if (ParentElement.BaseRow == null || (dCol = ParentElement.BaseRow.Table.Columns[ColumnName]) == null)
            {
                Result = null;
                return false;
            }
            else
            {
                Result = ParentElement.BaseRow[dCol];
                return true;
            }
        }


        /// <summary>
        /// Retrieve the image text for the item
        /// </summary>
        public String DescriptorText
        {
            get
            {
               
                String DataSource = OneLineViewer.btnDataSource.Text == "Telemetered" ? "" : " (Est)";
                Object FoundVal;
                if (ParentElement.ElemType == enumElemTypes.Line )
                {
                    MM_Line BaseLine = ParentElement.BaseElement as MM_Line;
                    if (BaseLine == null)
                        return "Unknown line";
                    MM_Substation BaseStation = OneLineViewer.BaseElement as MM_Substation;
                    String Sub1 = String.IsNullOrEmpty(BaseLine.Substation1.LongName) ? BaseLine.Substation1.Name : BaseLine.Substation1.LongName;
                    String Sub2 = String.IsNullOrEmpty(BaseLine.Substation2.LongName) ? BaseLine.Substation2.Name : BaseLine.Substation2.LongName;
                    StringBuilder sB = new StringBuilder();

                    if (OneLineViewer.btnNames.Checked)
                    {
                        if (BaseStation == null)
                            sB.AppendLine(Sub1 + " to " + Sub2);
                        else if (BaseStation == BaseLine.Substation1 && BaseLine.Substation2 != null)
                            sB.AppendLine(Sub2);
                        else if (BaseStation == BaseLine.Substation2 && BaseLine.Substation1 != null)
                            sB.AppendLine(Sub1);
                        sB.AppendLine("(" + BaseLine.Name + ")");
                    }
                    if (ParentElement.BaseRow == null)
                        return sB.ToString();


                    //Now, determine our information on ratings

                    String NearSub = "_1", FarSub = "_2";
                    String Sub1Name = ParentElement.BaseRow[OneLineViewer.btnDataSource.Text + "\\Substation_1"].ToString();
                    if (BaseStation != null && !String.Equals(BaseStation.Name, Sub1Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        NearSub = "_2"; FarSub = "_1";
                    }


                    float[] MW = new float[] { HandleFloat(ParentElement.BaseRow[OneLineViewer.btnDataSource.Text + "\\MW" + DataSource + NearSub]), HandleFloat(ParentElement.BaseRow[OneLineViewer.btnDataSource.Text + "\\MW" + DataSource + FarSub]) };
                    float[] MVAR = new float[] { HandleFloat(ParentElement.BaseRow[OneLineViewer.btnDataSource.Text + "\\MVAR" + DataSource + NearSub]), HandleFloat(ParentElement.BaseRow[OneLineViewer.btnDataSource.Text + "\\MVAR" + DataSource + FarSub]) };
                    float[] MVA = new float[] { HandleFloat(ParentElement.BaseRow[OneLineViewer.btnDataSource.Text + "\\MVA" + DataSource + NearSub]), HandleFloat(ParentElement.BaseRow[OneLineViewer.btnDataSource.Text + "\\MVA" + DataSource + FarSub]) };

                    if (OneLineViewer.btnMVA.Checked)
                        sB.AppendLine("MVA: " + OrientArrow(MVA[0], ParentElement.Orientation, false) + (OneLineViewer.btnOtherLine.Checked ? " / " + OrientArrow(MVA[1], ParentElement.Orientation, true) : ""));
                    if (OneLineViewer.btnMW.Checked)
                        sB.AppendLine("MW: " + OrientArrow(MW[0], ParentElement.Orientation, false) + (OneLineViewer.btnOtherLine.Checked ? " / " + OrientArrow(MW[1], ParentElement.Orientation, true) : ""));
                    if (OneLineViewer.btnMVAR.Checked)
                        sB.AppendLine("MVAR: " + OrientArrow(MVAR[0], ParentElement.Orientation, false) + (OneLineViewer.btnOtherLine.Checked ? " / " + OrientArrow(MVAR[1], ParentElement.Orientation, true) : ""));
                    return sB.ToString();
                }
                else if (ParentElement.ElemType == enumElemTypes.Transformer)
                {
                    StringBuilder sB = new StringBuilder();
                    
                    //Now, determine our information on ratings
                    MM_OneLine_Transformer XF = this.ParentElement as MM_OneLine_Transformer;
                    MM_OneLine_TransformerWinding W1 = XF.TransformerWindings[0];
                    MM_OneLine_TransformerWinding W2 = XF.TransformerWindings[1];
                    float[] MW = new float[] { HandleFloat(W1.BaseRow[OneLineViewer.btnDataSource.Text + "\\MW" + DataSource]), HandleFloat(W2.BaseRow[OneLineViewer.btnDataSource.Text + "\\MW" + DataSource]) };
                    float[] MVAR = new float[] { HandleFloat(W1.BaseRow[OneLineViewer.btnDataSource.Text + "\\MVAR" + DataSource]), HandleFloat(W2.BaseRow[OneLineViewer.btnDataSource.Text + "\\MVAR" + DataSource]) };
                    float[] MVA = new float[] { HandleFloat(W1.BaseRow[OneLineViewer.btnDataSource.Text + "\\MVA" + DataSource]), HandleFloat(W2.BaseRow[OneLineViewer.btnDataSource.Text + "\\MVA" + DataSource]) };
                    if (OneLineViewer.btnMVA.Checked && !float.IsNaN(MVA[0]) && W1.BaseWinding.Voltage != 1)
                        sB.AppendLine("MVA: " + OrientArrow(MVA[0], W1.Orientation, true));                    
                    if (OneLineViewer.btnMW.Checked && !float.IsNaN(MW[0]) && W1.BaseWinding.Voltage != 1)
                        sB.AppendLine("MW: " + OrientArrow(MW[0], W1.Orientation, true));
                    if (OneLineViewer.btnMVAR.Checked && !float.IsNaN(MVAR[0]) && W1.BaseWinding.Voltage != 1)
                        sB.AppendLine("MVAR: " + OrientArrow(MVAR[0], W1.Orientation, true));
                    if (OneLineViewer.btnNames.Checked)
                        sB.AppendLine(ParentElement.xElement.Attributes["BaseElement.TransformerName"].Value);
                    if (OneLineViewer.btnMVA.Checked && !float.IsNaN(MVA[1]) && W2.BaseWinding.Voltage != 1)
                        sB.AppendLine("MVA: " + OrientArrow(MVA[1], W2.Orientation, true));
                    if (OneLineViewer.btnMW.Checked && !float.IsNaN(MW[1]) && W2.BaseWinding.Voltage != 1)
                        sB.AppendLine("MW: " + OrientArrow(MW[1], W2.Orientation, true));
                    if (OneLineViewer.btnMVAR.Checked && !float.IsNaN(MVAR[1]) && W2.BaseWinding.Voltage != 1)
                        sB.AppendLine("MVAR: " + OrientArrow(MVAR[1], W2.Orientation, true));

                    return sB.ToString();
                }
                else if (ParentElement.ElemType == enumElemTypes.Unit)
                {
                    StringBuilder sB = new StringBuilder();
                    if (OneLineViewer.btnNames.Checked)
                        sB.AppendLine(ParentElement.BaseElement.Name);
                    if (ParentElement.BaseRow == null)
                        return sB.ToString();

                    if (OneLineViewer.btnMW.Checked && TryFindVal(OneLineViewer.btnDataSource.Text + "\\MW" + DataSource, out FoundVal) && FoundVal is Single)
                    {
                        String OutMW = "MW: " + OrientArrow((float)FoundVal, ParentElement.Orientation, true);
                        if (TryFindVal(OneLineViewer.btnDataSource.Text + "\\Frequency", out FoundVal) && FoundVal is Single)
                            OutMW += "\tFrq: " + ((Single)FoundVal).ToString("0.00") + " Hz";
                        sB.AppendLine(OutMW);
                    }
                    if (OneLineViewer.btnMVAR.Checked && TryFindVal(OneLineViewer.btnDataSource.Text + "\\MVAR" + DataSource, out FoundVal) && FoundVal is Single)
                    {
                        String OutIsland = "MVAR: " + OrientArrow((float)FoundVal, ParentElement.Orientation, true);
                        if (TryFindVal(OneLineViewer.btnDataSource.Text + "\\Island", out FoundVal) && FoundVal is Single)
                            OutIsland += "\tIsl: " + FoundVal.ToString();
                        sB.AppendLine(OutIsland);
                    }


                        
                    String AVRPSS = "";
                    if (TryFindVal(OneLineViewer.btnDataSource.Text + "\\NoAVR", out FoundVal) && FoundVal is bool)
                        AVRPSS = (bool)FoundVal ? "avr" : "AVR";
                    if (TryFindVal(OneLineViewer.btnDataSource.Text + "\\NoPSS", out FoundVal) && FoundVal is bool)
                        AVRPSS += (String.IsNullOrEmpty(AVRPSS) ? "" :" ") + ((bool)FoundVal ? "pss" : "PSS");
                    if (!String.IsNullOrEmpty(AVRPSS))
                        sB.AppendLine(AVRPSS);                    
                    if (TryFindVal(OneLineViewer.btnDataSource.Text + "\\FrequencyControl", out FoundVal) && FoundVal is bool && (bool)FoundVal)
                        sB.AppendLine("FRQ CTRL");
                    if (TryFindVal(OneLineViewer.btnDataSource.Text + "\\IslandFreqCtrl", out FoundVal) && FoundVal is bool && (bool)FoundVal)
                        sB.AppendLine("ISL FRQ CTRL");
                    
                    
                    
                    return sB.ToString();
                }
                else if (ParentElement.ElemType == enumElemTypes.Load || ParentElement.ElemType == enumElemTypes.LAAR)
                {
                    StringBuilder sB = new StringBuilder();
                    if (OneLineViewer.btnNames.Checked)
                        sB.AppendLine(ParentElement.BaseElement.Name);
                    if (ParentElement.BaseRow == null)
                        return sB.ToString();

                    float Estimated_MW = HandleFloat(ParentElement.BaseRow[OneLineViewer.btnDataSource.Text + "\\MW" + DataSource]);
                    float Estimated_MVAR = HandleFloat(ParentElement.BaseRow[OneLineViewer.btnDataSource.Text + "\\MVAR" + DataSource]);
                    if (OneLineViewer.btnMW.Checked)
                        sB.AppendLine("MW: " + OrientArrow(Estimated_MW, ParentElement.Orientation, false));
                    if (OneLineViewer.btnMVAR.Checked)
                        sB.AppendLine("MVAR: " + OrientArrow(Estimated_MVAR, ParentElement.Orientation, false));

                    return sB.ToString();
                }
                else if (ParentElement.ElemType == enumElemTypes.Capacitor)
                {
                    StringBuilder sB = new StringBuilder();
                    if (OneLineViewer.btnNames.Checked)
                        sB.AppendLine(ParentElement.BaseElement.Name);
                    if (ParentElement.BaseRow == null)
                        return sB.ToString();

                    float Estimated_MVAR = HandleFloat(ParentElement.BaseRow[OneLineViewer.btnDataSource.Text + "\\MVAR"]);
                    float Nominal_MVAR = HandleFloat(ParentElement.BaseRow[OneLineViewer.btnDataSource.Text + "\\Nominal MVAR"]);
                    if (OneLineViewer.btnMVAR.Checked)
                    {
                        sB.AppendLine("MVAR: " + OrientArrow(Estimated_MVAR, ParentElement.Orientation, true));
                        sB.AppendLine("Nom: " + OrientArrow(Nominal_MVAR, ParentElement.Orientation, true));
                    }
                    return sB.ToString();
                }
                else if (ParentElement.ElemType == enumElemTypes.StaticVarCompensator)
                {
                    StringBuilder sB = new StringBuilder();
                    if (OneLineViewer.btnNames.Checked)
                        sB.AppendLine(ParentElement.BaseElement.Name);
                    if (ParentElement.BaseRow == null)
                        return sB.ToString();

                    float Estimated_MVAR = HandleFloat(ParentElement.BaseRow[OneLineViewer.btnDataSource.Text + "\\MVAR" + DataSource]);
                     if (OneLineViewer.btnMVAR.Checked)
                    
                        sB.AppendLine("MVAR: " + OrientArrow(Estimated_MVAR, ParentElement.Orientation, true));
                    return sB.ToString();
                }
                else if (ParentElement.ElemType == enumElemTypes.Node)
                {
                    StringBuilder sB = new StringBuilder();
                    if (OneLineViewer.btnNames.Checked)
                        sB.AppendLine(ParentElement.BaseElement.Name);
                    if (OneLineViewer.btnVoltage.Checked)
                    {
                        float Voltage = HandleFloat(ParentElement.BaseRow[OneLineViewer.btnDataSource.Text + "\\KV"]);
                        if (!float.IsNaN(Voltage))
                            sB.AppendLine(Voltage.ToString("0.0 kV"));
                    }
                    if (OneLineViewer.btnFrequency.Checked)
                    {
                        float Freq = HandleFloat(ParentElement.BaseRow[OneLineViewer.btnDataSource.Text + "\\Frequency"]);
                        if (!float.IsNaN(Freq))
                            sB.AppendLine(Freq.ToString("0.000 Hz"));
                    }
                    return sB.ToString();
                }
                else if (ParentElement.BaseElement.ElemType.Name == "SeriesCompensator")
                {
                    
                    StringBuilder sB = new StringBuilder();
                    if (OneLineViewer.btnNames.Checked)
                        sB.AppendLine(ParentElement.BaseElement.Name);
                    if (OneLineViewer.btnMVA.Checked && TryFindVal(OneLineViewer.btnDataSource.Text + "\\MVA" + DataSource, out FoundVal) && FoundVal is float)
                        sB.AppendLine("MVA: " + Math.Abs((float)FoundVal).ToString("#,##0.0"));
                    if (OneLineViewer.btnMW.Checked && TryFindVal(OneLineViewer.btnDataSource.Text + "\\MW" + DataSource, out FoundVal) && FoundVal is float)
                        sB.AppendLine("MW: " + Math.Abs((float)FoundVal).ToString("#,##0.0"));
                    if (OneLineViewer.btnMVAR.Checked && TryFindVal(OneLineViewer.btnDataSource.Text + "\\MVAR" + DataSource, out FoundVal) && FoundVal is float)
                        sB.AppendLine("MVAR: " + Math.Abs((float)FoundVal).ToString("#,##0.0"));
                    return sB.ToString();
                }
                else if (ParentElement.ElemType == enumElemTypes.Reactor)
                {
                    StringBuilder sB = new StringBuilder();
                    if (OneLineViewer.btnNames.Checked)
                        sB.AppendLine(ParentElement.BaseElement.Name);
                    if (ParentElement.BaseRow == null)
                        return sB.ToString();

                    float Estimated_MVAR = HandleFloat(ParentElement.BaseRow[OneLineViewer.btnDataSource.Text + "\\MVAR"]);
                    DataColumn Nominal_MVAR_Col = ParentElement.BaseRow.Table.Columns[OneLineViewer.btnDataSource.Text + "\\Nominal MVAR"];
                    float Nominal_MVAR = float.NaN;
                    if (Nominal_MVAR_Col != null)
                        Nominal_MVAR = HandleFloat(ParentElement.BaseRow[Nominal_MVAR_Col]);
                    if (OneLineViewer.btnMVAR.Checked)
                    {
                        sB.AppendLine("MVAR: " + OrientArrow(Estimated_MVAR, ParentElement.Orientation, false));
                        if (Nominal_MVAR_Col != null)
                            sB.AppendLine("Nom: " + OrientArrow(Nominal_MVAR, ParentElement.Orientation, false));
                    }
                    return sB.ToString();
                }
                else
                    if (ParentElement.BaseElement == null)
                        return this.ToString();
                    else if (ParentElement.ElemType == enumElemTypes.PricingVector)
                        return (ParentElement as MM_OneLine_PricingVector).EPSMeter.Name + "\n" + (ParentElement as MM_OneLine_PricingVector).RID;
                    else if (ElemType == enumElemTypes.SecondaryDescriptor && ParentElement.ElemType == enumElemTypes.Node)
                        return (ParentElement as MM_OneLine_Node).ResourceNode;
                    else if (ElemType == enumElemTypes.SecondaryDescriptor && ParentElement.ElemType == enumElemTypes.PokePoint)
                        return (ParentElement as MM_OneLine_PokePoint).BaseNode.ResourceNode;
                    else
                        return ParentElement.BaseElement.Name;
            }
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
            bool IsPositive = InVal > 0f ^ IsInDirection;
            if (ParentOrientation == enumOrientations.Up || ParentOrientation == enumOrientations.Vertical)
                return (IsPositive ? "↑" : "↓") + Math.Abs(InVal).ToString(" #,##0.0");
            else if (ParentOrientation == enumOrientations.Down)
                return (IsPositive ? "↓" : "↑") + Math.Abs(InVal).ToString(" #,##0.0");
            else if (ParentOrientation == enumOrientations.Left || ParentOrientation == enumOrientations.Horizontal)
                return (IsPositive ? "←" : "→") + Math.Abs(InVal).ToString(" #,##0.0");
            else if (ParentOrientation == enumOrientations.Right)
                return (IsPositive ? "→" : "←") + Math.Abs(InVal).ToString(" #,##0.0");
            else
                return "? " + InVal.ToString("#,##0.0");
        }


        /// <summary>
        /// Pull out an object's information by XML, and return the appropriate response
        /// </summary>
        /// <param name="XmlText"></param>
        /// <param name="OutType"></param>
        /// <returns></returns>
        public Object AssignValues(String XmlText, Type OutType)
        {
            //First, retrieve all of our parameters.
            Dictionary<String, String> Parameters = new Dictionary<string, string>();
            foreach (String ToAdd in XmlText.Trim('{', '[', ']', '}').Split(',', ':'))
                if (ToAdd.Contains("="))
                    Parameters.Add(ToAdd.Trim().Split('=')[0], ToAdd.Trim().Split('=')[1]);

            if (OutType == typeof(Point))
                return Point.Round(new PointF(float.Parse(XmlText.Split(',')[0]), float.Parse(XmlText.Split(',')[1])));
            else if (OutType == typeof(Size))
                return new Size(int.Parse(Parameters["Width"]), int.Parse(Parameters["Height"]));
            else if (OutType == typeof(Font))
                return new Font(Parameters["Name"], float.Parse(Parameters["Size"]));
            throw new InvalidOperationException("Unknown type " + OutType.ToString());

        }

        /// <summary>
        /// Return the center of the item
        /// </summary>
        public PointF Center
        {
            get { return new PointF((float)Left + ((float)Width / 2f), (float)Top + ((float)Height / 2f)); }
        }

        /// <summary>
        /// Return some information on the element
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (BaseElement == null)
                return TEID.ToString("#,##0") + " (no element associated)";
            else
                return TEID.ToString("#,##0") + " (" + BaseElement.ElemType.Name + " " + BaseElement.Name + ")";
        }
        #endregion

        #region Animation
        /// <summary>
        /// Update a frame change
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void OnFrameChanged(object o, EventArgs e)
        {
            this.Invalidate();
        }

        /// <summary>
        /// When the user clicks on an element, let's refresh it, just to be sure.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClick(EventArgs e)
        {            
            this.Refresh();
            base.OnClick(e);
        }
        #endregion

        /// <summary>
        /// Return the center point of a rectangle
        /// </summary>
        /// <param name="Rect"></param>
        /// <returns></returns>
        public static Point CenterRect(Rectangle Rect)
        {
            return new Point(Rect.Left + (Rect.Width / 2), Rect.Top + (Rect.Height / 2));
        }

        /// <summary>
        /// This structure returns a floating-point value and a flag as to whether it's significantly different from SCADA.
        /// </summary>
        private struct TelemComparer
        {
            /// <summary>The value retrieved from the table</summary>
            public float Value;

            /// <summary>Whether the value is different</summary>
            public bool Diff;

            /// <summary>
            /// Instantiate a new telemetry/estimated value
            /// </summary>
            /// <param name="Value">The incoming value</param>
            /// <param name="Diff">Whether the value is different from SCADA</param>
            public TelemComparer(float Value, bool Diff)
            {
                this.Value = Value;
                this.Diff = Diff;
            }
        }

    }
}
