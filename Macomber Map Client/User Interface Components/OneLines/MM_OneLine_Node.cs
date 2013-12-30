using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Xml;
using Macomber_Map.Data_Elements;

namespace Macomber_Map.User_Interface_Components.OneLines
{
    /// <summary>
    /// This class holds a node's indicator
    /// </summary>
    public class MM_OneLine_Node : MM_OneLine_Element
    {
        #region Variable declarations
        /// <summary>The collection of elements and our sub-points to which they're connected</summary>
        private Dictionary<MM_OneLine_Element, MM_OneLine_PokePoint[]> _ConnectionPoints =new Dictionary<MM_OneLine_Element,MM_OneLine_PokePoint[]>(10);

        /// <summary>The collection of elements and our sub-points to which they're connected</summary>
        public Dictionary<MM_OneLine_Element, MM_OneLine_PokePoint[]> ConnectionPoints
        {
            get { return _ConnectionPoints; }          
        }
        
        /// <summary>The graphics path for a series of points</summary>        
        private Dictionary<MM_OneLine_Element, GraphicsPath> _GraphicsPaths = new Dictionary<MM_OneLine_Element,GraphicsPath>(10);

        /// <summary>The graphics path for a series of points</summary>        
        public Dictionary<MM_OneLine_Element, GraphicsPath> GraphicsPaths
        {
            get { return _GraphicsPaths; }
        }

        /// <summary>
        /// The resource node name, if any
        /// </summary>
        private String _ResourceNode=null;

        /// <summary>
        /// The name of the resource node
        /// </summary>
        [Category("Display"), Description("The name of the resource node")]
        public String ResourceNode
        {
            get { return _ResourceNode; }
            set { _ResourceNode = value; }
        }

        /// <summary>The TEID for the busbar section component, if any</summary>
        private Int32 _BusbarSection;

        /// <summary>The TEID for the busbar section component, if any</summary>
        [Category("MacomberMap"), Description("The TEID for the electrical bus component, if any"), DefaultValue(0)]
        public Int32 BusbarSection
        {
            get { return _BusbarSection; }
            set { _BusbarSection = value;  }
        }

        /// <summary>The node to which the node is connected</summary>
        public MM_OneLine_Node BaseNode;
        #endregion

        /// <summary>
        /// Initialize a new node
        /// </summary>
        /// <param name="BaseNode"></param>
        /// <param name="Orientation"></param>
        /// <param name="olView"></param>
        /// <param name="ViolView"></param>
        public MM_OneLine_Node(MM_Element BaseNode, enumOrientations Orientation,Violation_Viewer ViolView, OneLine_Viewer olView)
            : base(BaseNode, Orientation, ViolView, olView)
        { }

        /// <summary>
        /// Initialize a new node
        /// </summary>
        /// <param name="xE"></param>
        /// <param name="violViewer"></param>
        /// <param name="olView"></param>
        public MM_OneLine_Node(XmlElement xE, Violation_Viewer violViewer,OneLine_Viewer olView)
            : base(xE, violViewer, olView)
        {
           

            List<MM_OneLine_Element> NewElems = new List<MM_OneLine_Element>();
            this.BaseNode = this;

            //Go through our child nodes, and pull in our node linkages
            foreach (XmlElement xChild in xE.ChildNodes)
                if (xChild.Name != "Descriptor" && xChild.Name != "SecondaryDescriptor")
                {
                    MM_OneLine_Element TargetElement = null;
                    foreach (MM_OneLine_Element Elem in olView.DisplayElements.Values)
                        if (Elem.TEID == Convert.ToInt32(xChild.Attributes["TEID"].Value))
                            TargetElement = Elem;

                    List<MM_OneLine_PokePoint> Pokes = new List<MM_OneLine_PokePoint>();
                    foreach (XmlElement xPoke in xChild.ChildNodes)
                        if (xPoke.Name == "PricingVector")
                        {
                            MM_OneLine_PricingVector PV = new MM_OneLine_PricingVector(xPoke, violViewer, olView);
                            Pokes.Add(PV);
                            if (xPoke["Descriptor"] != null)
                                PV.Descriptor = new MM_OneLine_Element(PV, xPoke["Descriptor"], violViewer, olView);
                            if (xPoke["SecondaryDescriptor"] != null)
                                PV.SecondaryDescriptor = new MM_OneLine_Element(PV, xPoke["SecondaryDescriptor"], violViewer, olView);
                            NewElems.Add(PV);
                        }
                        else
                        {
                            Rectangle PointLocation = (Rectangle)MM_Serializable.RetrieveConvertedValue(typeof(Rectangle), xPoke.Attributes["Bounds"].Value, this,false);
                            MM_OneLine_Element.enumOrientations Orientation = MM_OneLine_Element.enumOrientations.Unknown;
                            if (xPoke.HasAttribute("Orientation"))
                                Orientation = (MM_OneLine_Element.enumOrientations)MM_Serializable.RetrieveConvertedValue(typeof(MM_OneLine_Element.enumOrientations), xPoke.Attributes["Orientation"].Value, this,false);
                            bool IsJumper = xPoke.HasAttribute("IsJumper") ? Convert.ToBoolean(xPoke.Attributes["IsJumper"].Value) : false;
                            bool IsVisible = xPoke.HasAttribute("IsVisible") ? Convert.ToBoolean(xPoke.Attributes["IsVisible"].Value) : true;
                            MM_OneLine_PokePoint NewPoke = olView.LocatePoke(PointLocation, this, this.BaseElement, Orientation,IsJumper, IsVisible);
                            MM_Serializable.ReadXml(xPoke, NewPoke,false);
                            if (xPoke["Descriptor"] != null && NewPoke.Descriptor == null)
                            {
                                NewPoke.Descriptor = new MM_OneLine_Element(NewPoke, xPoke["Descriptor"], violViewer, olView);
                                NewElems.Add(NewPoke.Descriptor);
                            }
                            if (xPoke["SecondaryDescriptor"] != null && NewPoke.SecondaryDescriptor == null)
                            {
                                NewPoke.SecondaryDescriptor = new MM_OneLine_Element(NewPoke, xPoke["SecondaryDescriptor"], violViewer, olView);
                                NewElems.Add(NewPoke.SecondaryDescriptor);
                            }
                            Pokes.Add(NewPoke);
                        }
                    ConnectionPoints.Add(TargetElement, Pokes.ToArray());
                }
             RebuildPaths(olView.pnlElements.AutoScrollPosition);                         
        }


       
        /// <summary>
        /// Rebuild the graphics paths for the elements
        /// </summary>
        public void RebuildPaths(Point OffsetPoint)
        {
            GraphicsPaths.Clear();
            Point Center1, Center2;

            foreach (KeyValuePair<MM_OneLine_Element, MM_OneLine_PokePoint[]> kvp in ConnectionPoints)
            {
                GraphicsPath g = new GraphicsPath();
                Rectangle TargetRect = Bounds;
                bool LastNode = true;
                foreach (MM_OneLine_PokePoint Poke in kvp.Value)
                {
                    Rectangle Bounds2 = Poke.Bounds;
                    bool IsJumper = Poke is MM_OneLine_PricingVector || Poke.IsJumper;
                    GetStraightLine(TargetRect, Bounds2, Point.Empty, out Center1, out Center2, LastNode && !IsJumper, LastNode = !IsJumper && Poke is MM_OneLine_PricingVector == false);
                    g.AddLine(Center1, Center2);
                    g.CloseFigure();
                    TargetRect = Bounds2;
                }

                //Now, draw our final line
                Rectangle LastRect = kvp.Key.Bounds;
                GetStraightLine(TargetRect, LastRect, Point.Empty, out Center1, out Center2, LastNode, false);
                g.AddLine(Center1, Center2);
                g.CloseFigure();

                GraphicsPaths.Add(kvp.Key, g);
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
        /// <param name="OffsetPoint"></param>
        public static void GetStraightLine(Rectangle FromRect, Rectangle ToRect, Point OffsetPoint, out Point Center1, out Point Center2, bool MaintainCenter1, bool MaintainCenter2)
        {
            Center1 = CenterRect(FromRect);
            Center2 = CenterRect(ToRect);
            FromRect.X -= OffsetPoint.X;
            ToRect.X -= OffsetPoint.X;
            FromRect.Y -= OffsetPoint.Y;
            ToRect.Y -= OffsetPoint.Y;

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
        

        /// <summary>
        /// Offset a rectangle by a set point
        /// </summary>
        /// <param name="InRect">The original rectangle</param>
        /// <param name="OffsetPoint">The offset point</param>
        /// <returns></returns>
        public static Rectangle OffsetRectangle(Rectangle InRect, Point OffsetPoint)
        {
            return new Rectangle(InRect.Left - OffsetPoint.X, InRect.Top - OffsetPoint.Y, InRect.Width, InRect.Height);
        }
        


        #region Rendering
        /// <summary>
        /// Draw connecting lines between this node and its other components
        /// </summary>
        /// <param name="g">The graphics connector</param>
        public void DrawConnectingLines(Graphics g)
        {

            GraphicsPath FoundPath;
            foreach (KeyValuePair<MM_OneLine_Element, MM_OneLine_PokePoint[]> kvp in ConnectionPoints)
                if (GraphicsPaths.TryGetValue(kvp.Key, out FoundPath) && FoundPath.PointCount > 1)
                {
                    Color DrawColor;
                    if (KVLevel != null)
                        DrawColor = KVLevel.Energized.ForeColor;
                    else if (kvp.Key.BaseElement != null && kvp.Key.BaseElement.KVLevel != null)
                        DrawColor = kvp.Key.BaseElement.KVLevel.Energized.ForeColor;
                    else
                        DrawColor = Color.Red;

                    using (Pen ThisPen = new Pen(DrawColor))
                        g.DrawPath(ThisPen, FoundPath);
                    //  foreach (MM_OneLine_PokePoint Poke in kvp.Value)
                    //    Poke.DrawImage(g);
                }
        }



     

        #endregion

    }
}
