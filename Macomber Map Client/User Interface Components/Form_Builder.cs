using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Macomber_Map.Data_Elements;
using Macomber_Map.Data_Connections;
using System.Threading;
using System.Drawing.Drawing2D;

using Macomber_Map.User_Interface_Components.NetworkMap;
using System.Reflection;
using System.IO;

using Macomber_Map.User_Interface_Components.OneLines;
using Macomber_Map.Data_Connections.Generic;

namespace Macomber_Map.User_Interface_Components
{
    /// <summary>
    /// This class contains the holding form for a display, including its building and architectural parameters
    /// </summary>
    public partial class Form_Builder : MM_Form
    {
        #region Variable declarations
        
        /// <summary>The element at the seed of the display</summary>
        public MM_Element BaseElement;

        /// <summary>Access to the network map, if desired</summary>
        public Network_Map nMap;

        /// <summary>The thread in which the form should be run</summary>
        public Thread FormThread;

        /// <summary>The base data for the display</summary>
        public DataSet_Base BaseData;

        /// <summary>The group boxes holding some standard controls on the left</summary>
        private GroupBox grpMiniMap, grpSummary, grpViolations;

        /// <summary>The mini-map control</summary>
        private Mini_Map viewMiniMap;

        /// <summary>The element summary control</summary>
        private Element_Summary viewSummary;

        /// <summary>The violation viewer control</summary>
        private Violation_Viewer viewViolations;
        
        /// <summary>The data table </summary>
        private Data_Table viewDataTable;

        /// <summary>The property page</summary>
        private Property_Page viewPropertyPage;

        /// <summary>The search text </summary>
        private String SearchText;

        /// <summary>The one-line viewer</summary>
        public OneLine_Viewer viewOneLine;

     
        #endregion        

        #region Initialization
        /// <summary>
        /// Initialize the form builder and its components
        /// </summary>
        public Form_Builder()
        {
            InitializeComponent();
        }
        #endregion

        #region Common initializers
        /// <summary>
        /// Complete the elements for the form.
        /// </summary>
        /// <param name="BorderingLines">Lines bordering around the substation or region</param>
        /// <param name="BaseElements">The base elements that comprise this view</param>
        /// <param name="BorderTopLeft">The top-left coordinate from the lassoed region</param>
        /// <param name="BorderBottomRight">The bottom-right coordinate from the lassoed region</param>
        /// <param name="MiddlePaneControl">The control to be drawn in the middle pane</param>
        private void CompleteForm(List<MM_Line> BorderingLines, MM_Element[] BaseElements, PointF BorderTopLeft, PointF BorderBottomRight, Control MiddlePaneControl)
        {            
            //Create our mini-map view
            grpMiniMap = new GroupBox();
            grpMiniMap.ForeColor = Color.LightGray;
            grpMiniMap.Text = "Grid View";
            grpMiniMap.Name = "MiniMap";
            viewMiniMap = new Mini_Map(false);
            viewMiniMap.Dock = DockStyle.Fill;
            grpMiniMap.Controls.Add(viewMiniMap);
            splMain.Panel1.Controls.Add(grpMiniMap);
            grpMiniMap.Size = new Size(splMain.Panel1.DisplayRectangle.Width, splMain.Panel1.DisplayRectangle.Width);
            grpMiniMap.Location = new Point(0, splMain.Panel1.DisplayRectangle.Height - grpMiniMap.Height);


            //Create our summary view
            grpSummary = new GroupBox();
            grpSummary.Name = "Summary";
            grpSummary.Text = "Summary";
            grpSummary.ForeColor = Color.LightGray;
            viewSummary = new Element_Summary();
            viewSummary.Dock = DockStyle.Fill;
            grpSummary.Controls.Add(viewSummary);
            splMain.Panel1.Controls.Add(grpSummary);

            //Create our violations view and set it up
            grpViolations = new GroupBox();
            grpViolations.Name = "Violations";
            grpViolations.Text = "Violations";
            grpViolations.ForeColor = Color.LightGray;
            viewViolations = new Violation_Viewer();
            viewViolations.Dock = DockStyle.Fill;
            grpViolations.Controls.Add(viewViolations);
            //grpViolations.Visible = false; ///////this lets the control even if added by another still remain non-usable
            splMain.Panel1.Controls.Add(grpViolations);/////////this allows map to resize
            //splMain.Panel1.Enabled = false; 

            //Now set up our event handlers
            splMain.SplitterMoved += new SplitterEventHandler(LassoOneLine_SplitterMoved);
            this.Resize += new EventHandler(LassoOneLine_SplitterMoved);

            //Assign our controls
            viewViolations.SetControls( viewMiniMap, nMap, BaseElements);
            viewSummary.SetControls( BaseData);
            viewMiniMap.SetControls( nMap, viewViolations, BaseData, BorderingLines.ToArray(), BorderTopLeft, BorderBottomRight);            
            viewSummary.TypeSelected += new Element_Summary.TypeSelectedDeletage(viewSummary_TypeSelected);

            //Now resize our left-hand pane
            if (viewSummary.Items.Count > 1)
                grpSummary.Height = (grpSummary.Height - viewSummary.Height) + viewSummary.Items[viewSummary.Items.Count - 1].GetBounds(ItemBoundsPortion.Entire).Bottom + 30;
            
                
            int MaxWidth = 10;
            foreach (ColumnHeader col in viewSummary.Columns)
            {
                int colWidth = col.Width;
                col.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
                col.Width = Math.Max(colWidth, col.Width);
                if (col.Index < 3)
                    MaxWidth += col.Width;
            }
            splMain.SplitterDistance = 215;

            //Complete the right-hand side
            //Create a splitter
            SplitContainer splRight = new SplitContainer();
            splRight.Orientation = Orientation.Horizontal;
            splRight.FixedPanel = FixedPanel.Panel2;                           
            splRight.Dock = DockStyle.Fill;
            splMain.Panel2.Controls.Add(splRight);

            
            //Build our data table and property pages
            viewPropertyPage = new Property_Page( viewViolations.networkMap, viewViolations, viewMiniMap);
            viewPropertyPage.Dock = DockStyle.Fill;
            viewPropertyPage.Visible = false;
            viewPropertyPage.Name = "PropertyPage";

            viewDataTable = new Data_Table(viewPropertyPage, nMap, viewViolations, viewMiniMap);
            viewDataTable.Dock = DockStyle.Fill;
            viewDataTable.Visible = true;
            viewDataTable.Name = "DataTable";
            
            //Now, if we have a middle pane control, put that to the left of the splitter, and the data table and property pages on the right
            if (MiddlePaneControl != null)
            {
                splRight.Panel1.Controls.Add(MiddlePaneControl);
                MiddlePaneControl.BackColor = MM_Repository.OverallDisplay.BackgroundColor;                
                splRight.Panel2.Controls.Add(viewDataTable);
                splRight.Panel2.Controls.Add(viewPropertyPage);
                if (MiddlePaneControl is OneLine_Viewer)
                    splRight.Panel2Collapsed = true;
            }
            else
            {
                splRight.Panel1.Controls.Add(viewDataTable);
                splRight.Panel2.Controls.Add(viewPropertyPage);
                viewPropertyPage.Visible = true;
            }

            splRight.SplitterDistance = splMain.Panel2.Width - splMain.Panel1.Width;
            splRight.SplitterWidth = 5;
            splRight.BackColor = Color.White;                       
        }
        #endregion
        
        #region Lasso view
        /// <summary>
        /// Create a new lasso display, by going through all substations and lines within a county
        /// </summary>
        /// <param name="County"> The County to be rendered</param>
        /// <param name="nMap">The network map that called the lasso display</param>        
        /// <param name="ControlDown">Whether the control key was down when the lasso was called</param>
        public static void Lasso_Display(MM_Boundary County, Network_Map nMap, bool ControlDown)
        {
            Thread FormThread = new Thread(new ParameterizedThreadStart(Lasso_Build));
            FormThread.Name = "Lasso display";
            FormThread.Start(new object[] { County, null, nMap, FormThread, ControlDown });
        }

        /// <summary>
        /// Create a new lasso display, by going through all substations and lines within the region
        /// </summary>
        /// <param name="TopLeft">The top-left coordinate</param>
        /// <param name="BottomRight">The bottom-right coordinate</param>
        /// <param name="nMap">The network map that called the lasso display</param>        
        /// <param name="ControlDown">Whether the control key was down when the lasso was called</param>
        public static void Lasso_Display(PointF TopLeft, PointF BottomRight, Network_Map nMap, bool ControlDown)
        {
            Thread FormThread = new Thread(new ParameterizedThreadStart(Lasso_Build));
            FormThread.Name = "Lasso display";                        
            FormThread.Start(new object[] { TopLeft, BottomRight, nMap, FormThread, ControlDown });
        }

        /// <summary>
        /// Create a new lasso display, by going through all substations and lines within the region
        /// </summary>
        /// <param name="LassoPoints">The collection of lasso points</param>
        /// <param name="nMap">The network map that called the lasso display</param>        
        /// <param name="ControlDown">Whether the control key was down when the lasso was called</param>
        public static void Lasso_Display(List<PointF> LassoPoints, Network_Map nMap, bool ControlDown)
        {            
            Thread FormThread = new Thread(new ParameterizedThreadStart(Lasso_Build));
            FormThread.Name = "Lasso display";
            FormThread.Start(new object[] { LassoPoints, null, nMap, FormThread, ControlDown });
        }


        /// <summary>
        /// Initiate a new lasso view in the thread
        /// </summary>
        /// <param name="Parameters">Parameters for the display</param>
        private static void Lasso_Build(object Parameters)
        {
            try
            {
                //First, receive our parameters                        
                Object[] inObj = (Object[])Parameters;

                //Create our form, assign its parameters, and complete it.
                Form_Builder BuilderForm = new Form_Builder();
                BuilderForm.nMap = inObj[2] as Network_Map;
                BuilderForm.Title = (inObj[3] as Thread).Name;
                if (inObj[1] is PointF && (!BuilderForm.Lasso_Complete((PointF)inObj[0], (PointF)inObj[1], (bool)inObj[4])))
                {
                    Program.MessageBox("No elements found within the requested lasso bounds.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else if ((inObj[1] == null) && (inObj[0] is MM_Boundary) && (!BuilderForm.Lasso_Complete(inObj[0] as MM_Boundary, (bool)inObj[4])))
                {
                    Program.MessageBox("No elements found within the requested lasso bounds.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else if ((inObj[1] == null) && (inObj[0] is List<PointF>) && (!BuilderForm.Lasso_Complete((List<PointF>)inObj[0], (bool)inObj[4])))
                {
                    Program.MessageBox("No elements found within the requested lasso bounds.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else if ((inObj[1] == null) && (inObj[0] is MM_Island) && (!BuilderForm.Lasso_Complete((MM_Island)inObj[0], (bool)inObj[4])))
                {
                    Program.MessageBox("No elements found within the requested lasso bounds.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                Data_Integration.DisplayForm(BuilderForm, inObj[3] as Thread);
            }
            catch { }
        }


        /// <summary>
        /// Complete the lasso rendering
        /// </summary>
        /// <param name="Boundary">The county to be checked for </param>
        /// <param name="ControlDown">Whether the control key is down</param>
        /// <returns></returns>
        private bool Lasso_Complete(MM_Boundary Boundary, bool ControlDown)
        {
            //First, create our dataset.
            BaseData = new DataSet_Base("Lasso: " + Boundary.Name + " county");

            Dictionary<MM_Substation, int> Subs = new Dictionary<MM_Substation, int>();            
            List<MM_Line> BorderingLines = new List<MM_Line>();

            //Go through all substations  within our boundary
            foreach (MM_Substation Sub in Boundary.Substations)
            {
                if (Sub.Permitted && (nMap.IsVisible(Sub) || ControlDown))
                {
                    BaseData.AddDataElement(Sub);
                    Subs.Add(Sub, 0);
                }
            }

            //Now go through all elements. 
            foreach (MM_Element Elem in MM_Repository.TEIDs.Values)
                if (Elem is MM_Line)
                {
                    MM_Line Line = Elem as MM_Line;
                    if (Line.Permitted && (nMap.IsVisible(Line) || ControlDown))
                        if (Subs.ContainsKey(Line.ConnectedStations[0]) && Subs.ContainsKey(Line.ConnectedStations[1]))
                            BaseData.AddDataElement(Line);
                        else if (Subs.ContainsKey(Line.ConnectedStations[0]) || Subs.ContainsKey(Line.ConnectedStations[1]))
                            BorderingLines.Add(Line);
                }
                else if (Elem.Permitted && Elem.Substation != null && Subs.ContainsKey(Elem.Substation))
                    BaseData.AddDataElement(Elem);
            

            //Now tally our element count
            int ElementCount = 0;
            foreach (String TableName in new String[] { "Substation", "Line" })
                if (BaseData.Data.Tables.Contains(TableName))
                    ElementCount += BaseData.Data.Tables[TableName].Rows.Count;

            //Compute our base elements
            List<MM_Element> BaseElements = new List<MM_Element>(ElementCount);
            foreach (String TableName in new String[] { "Substation", "Line" })
                if (BaseData.Data.Tables.Contains(TableName))
                    foreach (DataRow dr in BaseData.Data.Tables[TableName].Rows)
                        if (!BaseElements.Contains(dr["TEID"] as MM_Element))
                            BaseElements.Add(dr["TEID"] as MM_Element);

            if (BaseElements.Count == 0)
                return false;

            //Complete the form
            CompleteForm(BorderingLines, BaseElements.ToArray(), Boundary.Min, Boundary.Max, null);           

            return true;
        }

        /// <summary>
        /// Complete the lasso rendering
        /// </summary>
        /// <param name="Island">The island to be checked for </param>
        /// <param name="ControlDown">Whether the control key is down</param>
        /// <returns></returns>
        private bool Lasso_Complete(MM_Island Island, bool ControlDown)
        {
            //First, create our dataset.
            BaseData = new DataSet_Base("Lasso: " + Island.ToString());

            Dictionary<MM_Substation, int> Subs = new Dictionary<MM_Substation, int>();
            List<MM_Line> BorderingLines = new List<MM_Line>();
           
             foreach (MM_Element Elem in MM_Repository.TEIDs.Values)
                 if (Elem.Permitted && (nMap.IsVisible(Elem) || ControlDown) && Island.Equals(Elem.Island))
                 {
                     BaseData.AddDataElement(Elem);
                     if (Elem is MM_Substation)
                         Subs.Add(Elem as MM_Substation, 0);
                 }

            //Now tally our element count
            int ElementCount = 0;
            foreach (String TableName in new String[] { "Substation", "Line" })
                if (BaseData.Data.Tables.Contains(TableName))
                    ElementCount += BaseData.Data.Tables[TableName].Rows.Count;

            //Compute our base elements
            List<MM_Element> BaseElements = new List<MM_Element>(ElementCount);
            foreach (String TableName in new String[] { "Substation", "Line" })
                if (BaseData.Data.Tables.Contains(TableName))
                    foreach (DataRow dr in BaseData.Data.Tables[TableName].Rows)
                        if (!BaseElements.Contains(dr["TEID"] as MM_Element))
                            BaseElements.Add(dr["TEID"] as MM_Element);

            if (BaseElements.Count == 0)
                return false;

            //Complete the form
            PointF StartPoint, EndPoint;
            Island.ComputeBounds(out StartPoint, out EndPoint, nMap, ControlDown);

            CompleteForm(BorderingLines, BaseElements.ToArray(), StartPoint, EndPoint, null);         
            return true;
        }



        /// <summary>
        /// Complete the lasso rendering
        /// </summary>
        /// <param name="StartPoint">The starting lat/lng</param>
        /// <param name="EndPoint">The ending lat/lng</param>
        /// <param name="ControlDown">Whether the control key is down</param>
        /// <returns></returns>
        private bool Lasso_Complete(PointF StartPoint, PointF EndPoint, bool ControlDown)
        {
            //First, create our dataset.
            BaseData = new DataSet_Base("Lasso: (" + StartPoint.X.ToString("0.000") + "," + StartPoint.Y.ToString("0.000") + ")-(" + EndPoint.X.ToString("0.000") + "," + EndPoint.Y.ToString("0.000") + ")");

            //Prepare to compare every point against the lasso boundaries
            CheckPoint(ref StartPoint, ref EndPoint);
            RectangleF TestRectangle = new RectangleF(StartPoint.X, StartPoint.Y, EndPoint.X - StartPoint.X, EndPoint.Y - StartPoint.Y);
            List<MM_Line> BorderingLines = new List<MM_Line>();

            //First, add all lines to the display, and check their boundaries.
            foreach (MM_Line Elem in MM_Repository.Lines.Values)
                if ((Elem is MM_Line) && Elem.Permitted && (ControlDown || nMap.IsVisible(Elem as MM_Line)))                    
                {
                    bool Sub1 = TestRectangle.Contains((Elem as MM_Line).ConnectedStations[0].LatLong);
                    bool Sub2 = TestRectangle.Contains((Elem as MM_Line).ConnectedStations[1].LatLong);
                    if (Sub1 && Sub2)
                        BaseData.AddDataElement(Elem);
                    else if (Sub1 || Sub2)
                        BorderingLines.Add(Elem as MM_Line);
                }

            //Now, add all substations within the region
            List<Int32> SubTEIDs = new List<Int32>();
            foreach (MM_Substation Elem in MM_Repository.Substations.Values)
                if (Elem.Permitted && TestRectangle.Contains((Elem as MM_Substation).LatLong) && (ControlDown || (nMap.IsVisible(Elem as MM_Substation)) || (MM_Repository.SubstationDisplay.ShowSubsOnLines && nMap.IsConnectorVisible(Elem as MM_Substation))))
                {
                    BaseData.AddDataElement(Elem);
                    SubTEIDs.Add(Elem.TEID);
                }

            //Now add all elements within those substations in our region            
                foreach (MM_Element Elem in new List<MM_Element>(MM_Repository.TEIDs.Values))
                    if (Elem.TEID >0 &&  Elem.Substation != null && Elem.Permitted && SubTEIDs.Contains(Elem.Substation.TEID))
                        BaseData.AddDataElement(Elem);



            //Now tally our element count
            int ElementCount = 0;
            foreach (String TableName in new String[] { "Substation", "Line" })
                if (BaseData.Data.Tables.Contains(TableName))
                    ElementCount += BaseData.Data.Tables[TableName].Rows.Count;

            //Compute our base elements
            List<MM_Element> BaseElements = new List<MM_Element>(ElementCount);
            foreach (String TableName in new String[] { "Substation", "Line" })
                if (BaseData.Data.Tables.Contains(TableName))
                    foreach (DataRow dr in BaseData.Data.Tables[TableName].Rows)
                        if (!BaseElements.Contains(dr["TEID"] as MM_Element))
                            BaseElements.Add(dr["TEID"] as MM_Element);

            if (BaseElements.Count == 0)
                return false;

            //Complete the form
            CompleteForm(BorderingLines, BaseElements.ToArray(), StartPoint, EndPoint, null);           
            return true;
        }

        /// <summary>
        /// For the lasso, when the user clicks (get more...), go into the database to retrieve all elements we haven't gotten already.
        /// </summary>
        public void Lasso_GetMoreElements()
        {
            if (Data_Integration.MMServer == null || !BaseData.Data.Tables.Contains("Substation"))
                return;
            viewSummary.Items.RemoveByKey("GetMore");            
            List<Int32> Subs = new List<Int32>();
            foreach (DataRow dr in BaseData.Data.Tables["Substation"].Rows)
                Subs.Add((dr["TEID"] as MM_Substation).TEID);
            foreach (MM_Element ToAdd in Data_Integration.MMServer.LocateElements(Subs.ToArray()))
                if (!MM_Repository.TEIDs.ContainsKey(ToAdd.TEID))
                    BaseData.AddDataElement(ToAdd);
            viewSummary.SetControls( BaseData);

            //Now resize our left-hand pane
            if (viewSummary.Items.Count > 1)
                grpSummary.Height = (grpSummary.Height - viewSummary.Height) + viewSummary.Items[viewSummary.Items.Count - 1].GetBounds(ItemBoundsPortion.Entire).Bottom + 30;
            int MaxWidth = 10;
            foreach (ColumnHeader col in viewSummary.Columns)
            {
                col.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
                MaxWidth += col.Width;
            }
            splMain.SplitterDistance = MaxWidth;

            //Simulate the splitter movement to complete the resizing.
            LassoOneLine_SplitterMoved(this, null);
        }

        /// <summary>
        /// Complete the lasso rendering
        /// </summary>
        /// <param name="LassoPoints">The collection of lasso points</param>
        /// <param name="ControlDown">Whether the control key is down</param>
        /// <returns></returns>
        private bool Lasso_Complete(List<PointF> LassoPoints, bool ControlDown)
        {
            if (LassoPoints.Count < 2) return false;

            //First, create our dataset.
            BaseData = new DataSet_Base("Lasso");
            PointF StartPoint = new PointF(float.NaN, float.NaN);
            PointF EndPoint = new PointF(float.NaN, float.NaN);

            //Create our graphics path to test everything, scaling by 1,000 for accuracy
            GraphicsPath gP = new GraphicsPath();
            PointF[] AdjustedPoints = new PointF[LassoPoints.Count+1];
            for (int a = 0; a < LassoPoints.Count; a++)
            {
                AdjustedPoints[a] = new PointF(LassoPoints[a].X * 1000f, LassoPoints[a].Y * 1000f);
                if (float.IsNaN(StartPoint.X) || StartPoint.X > LassoPoints[a].X)
                    StartPoint.X = LassoPoints[a].X;
                if (float.IsNaN(StartPoint.Y) || StartPoint.Y > LassoPoints[a].Y)
                    StartPoint.Y = LassoPoints[a].Y;
                if (float.IsNaN(EndPoint.X) || EndPoint.X < LassoPoints[a].X)
                    EndPoint.X = LassoPoints[a].X;
                if (float.IsNaN(EndPoint.Y) || EndPoint.Y < LassoPoints[a].Y)
                    EndPoint.Y = LassoPoints[a].Y;
            }
            AdjustedPoints[AdjustedPoints.Length - 1] = AdjustedPoints[0];
            gP.AddLines(AdjustedPoints);
            List<MM_Line> BorderingLines = new List<MM_Line>();


            //Now, test each line to see whether one or both legs are in our region.
            foreach (MM_Line Elem in MM_Repository.Lines.Values)
                if (Elem.Permitted && (ControlDown || nMap.IsVisible(Elem as MM_Line)))
                {
                    PointF Sub1 = new PointF((Elem as MM_Line).ConnectedStations[0].LatLong.X * 1000f, (Elem as MM_Line).ConnectedStations[0].LatLong.Y * 1000f);
                    PointF Sub2 = new PointF((Elem as MM_Line).ConnectedStations[1].LatLong.X * 1000f, (Elem as MM_Line).ConnectedStations[1].LatLong.Y * 1000f);
                    bool Sub1In = gP.IsVisible(Sub1);
                    bool Sub2In = gP.IsVisible(Sub2);                    
                    if (Sub1In && Sub2In)
                        BaseData.AddDataElement(Elem);
                    else if (Sub1In || Sub2In)
                        BorderingLines.Add(Elem as MM_Line);
                }


            //Now find all substations in our region
            List<Int32> SubTEIDs = new List<Int32>();
            foreach (MM_Substation Elem in MM_Repository.Substations.Values)
                if (Elem.Permitted && (gP.IsVisible(new PointF(Elem.LatLong.X * 1000f, Elem.LatLong.Y * 1000f)) && (ControlDown || (nMap.IsVisible(Elem as MM_Substation) || (MM_Repository.SubstationDisplay.ShowSubsOnLines && nMap.IsConnectorVisible(Elem as MM_Substation))))))
                    {
                        BaseData.AddDataElement(Elem);
                        SubTEIDs.Add(Elem.TEID);
                    }                


            //Now add all elements within those substations in our region
            foreach (MM_Element Elem in MM_Repository.TEIDs.Values)
                if (Elem.Substation != null && SubTEIDs.Contains(Elem.Substation.TEID) && Elem.Permitted)
                    BaseData.AddDataElement(Elem);

            //Now tally our element count
            int ElementCount = 0;
            foreach (String TableName in new String[] { "Substation", "Line" })
                if (BaseData.Data.Tables.Contains(TableName))
                    ElementCount += BaseData.Data.Tables[TableName].Rows.Count;

            //Compute our base elements
            List<MM_Element> BaseElements = new List<MM_Element>(ElementCount);
            foreach (String TableName in new String[] { "Substation", "Line" })
                if (BaseData.Data.Tables.Contains(TableName))
                    foreach (DataRow dr in BaseData.Data.Tables[TableName].Rows)
                        if (!BaseElements.Contains(dr["TEID"] as MM_Element))
                            BaseElements.Add(dr["TEID"] as MM_Element);

            if (BaseElements.Count == 0)
                return false;

            //Complete the form
            CompleteForm(BorderingLines, BaseElements.ToArray(), StartPoint, EndPoint, null);         
            return true;
        }


        /// <summary>
        /// Make sure pt1 is always less than pt2, and if not, swap them.
        /// </summary>
        /// <param name="pt1">The first point</param>
        /// <param name="pt2">The second point</param>
        private void CheckPoint(ref PointF pt1, ref PointF pt2)
        {
            if (pt1.X > pt2.X)
            {
                float tempX = pt1.X;
                pt1.X = pt2.X;
                pt2.X = tempX;
            }
            if (pt1.Y > pt2.Y)
            {
                float tempY = pt1.Y;
                pt1.Y = pt2.Y;
                pt2.Y = tempY;
            }
        }
        #endregion

        #region Property Page
        /// <summary>
        /// Initiate a new property page display, and show
        /// </summary>
        /// <param name="BaseElement">The root element of the one-line</param>
        /// <param name="nMap">The network map</param>
        public static void PropertyPage(MM_Element BaseElement, Network_Map nMap)
        {
            Thread FormThread = new Thread(new ParameterizedThreadStart(PropertyPage_Build));
            FormThread.Name = "Property Page: " + BaseElement.ElemType.Name + " " + BaseElement.Name;
            FormThread.Start(new object[] { BaseElement, nMap, FormThread });
        }

        /// <summary>
        /// Initiate a new one-line display in the thread
        /// </summary>
        /// <param name="Parameters">Parmeters for the display</param>
        private static void PropertyPage_Build(object Parameters)
        {
            //First, prepare to receive our input parameters
            Object[] inObj = Parameters as Object[];

            //Now create our form, assign its parameters, and complete it.
            Form_Builder BuilderForm = new Form_Builder();
            BuilderForm.BaseElement = inObj[0] as MM_Element;
            try
            {
                BuilderForm.Title = (inObj[2] as Thread).Name;
                BuilderForm.PropertyPage_Complete(inObj[0] as MM_Element, inObj[1] as Network_Map);
                BuilderForm.Width = 861;
                //Add it to the thread and forms collections in the integration layer
                Data_Integration.DisplayForm(BuilderForm, inObj[2] as Thread);
            }
            catch (Exception ex)
            {
                Program.MessageBox("Error building property page: " + ex.Message + "\n\n" + ex.StackTrace, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Complete the steps to completing the one-line UI.
        /// </summary>
        private void PropertyPage_Complete(MM_Element BaseElement, Network_Map nMap)
        {
            PointF TopLeft = new PointF(float.NaN, float.NaN);
            PointF BottomRight = new PointF(float.NaN, float.NaN);
                
           
            //Create our data set 
            Dictionary<Int32, MM_Element> SubTEIDs = new Dictionary<Int32,MM_Element>();
            if (BaseElement.Substation != null)
                SubTEIDs.Add(BaseElement.Substation.TEID, BaseElement.Substation);
            else if (BaseElement is MM_Boundary)
                foreach (MM_Substation Sub in (BaseElement as MM_Boundary).Substations)
                    SubTEIDs.Add(Sub.TEID, Sub);
            else if (BaseElement is MM_Substation)
                SubTEIDs.Add(BaseElement.TEID, BaseElement);





            BaseData = new DataSet_Base("Property page for " + BaseElement.ElemType.Name + " " + BaseElement.Name);
            BaseData.AddDataElement(BaseElement);
            BaseData.BaseElement = BaseElement;

            
            //Now, add all elements within our subs into the display
            foreach (MM_Element Elem in MM_Repository.TEIDs.Values)
                    if (Elem is MM_Blackstart_Corridor_Element == false &&  Elem.Substation != null && Elem.Permitted && SubTEIDs.ContainsKey(Elem.Substation.TEID))                      
                            BaseData.AddDataElement(Elem);                        

           
            //If we have a substation, track that.            
            List<MM_Line> BorderingLines = new List<MM_Line>();


            if (BaseElement is MM_Substation)
            {
                foreach (MM_Line Line in MM_Repository.Lines.Values)
                    if (Line.Permitted && (Line.Substation1.Equals(BaseElement) || Line.Substation2.Equals(BaseElement)))
                    {
                        BaseData.AddDataElement(Line);
                        if (!BorderingLines.Contains(Line))
                            BorderingLines.Add(Line);   
                    }
            }
            else if (BaseElement is MM_Line == false &&  BaseElement.Substation != null)
            {
                foreach (MM_Line Line in MM_Repository.Lines.Values)
                    if (Line.Permitted && (Line.Substation1.Equals(BaseElement.Substation) || Line.Substation2.Equals(BaseElement.Substation)))
                    {
                        BaseData.AddDataElement(Line);
                        if (!BorderingLines.Contains(Line))
                            BorderingLines.Add(Line);
                    }
                BaseData.AddDataElement(BaseElement.Substation);
                BaseData.BaseElement = BaseElement.Substation;
            }
            else if (BaseElement is MM_Boundary)
                foreach (MM_Substation Sub in (BaseElement as MM_Boundary).Substations)
                {
                    BaseData.AddDataElement(Sub);
                    foreach (MM_Line Line in MM_Repository.Lines.Values)
                        if (Line.Permitted)
                            if (SubTEIDs.ContainsKey(Line.ConnectedStations[0].TEID) && SubTEIDs.ContainsKey(Line.ConnectedStations[1].TEID))
                                BaseData.AddDataElement(Line);
                            else if (SubTEIDs.ContainsKey(Line.ConnectedStations[0].TEID) || SubTEIDs.ContainsKey(Line.ConnectedStations[1].TEID) && !BorderingLines.Contains(Line))
                                BorderingLines.Add(Line);
                    
                }
            else if (BaseElement is MM_RemedialActionScheme)
                foreach (MM_Element Elem in (BaseElement as MM_RemedialActionScheme).Elements)
                {
                    if (Elem is MM_Substation)
                        TestPoint((Elem as MM_Substation).LatLong, ref TopLeft, ref BottomRight);
                    else if (Elem is MM_Line)
                        for (int a = 0; a < 2; a++)
                            TestPoint((Elem as MM_Line).ConnectedStations[a].LatLong, ref TopLeft, ref BottomRight);
                    else if (Elem.Substation != null)
                        TestPoint(Elem.Substation.LatLong, ref TopLeft, ref BottomRight);
                    BaseData.AddDataElement(Elem);
                    if (Elem.Substation != null)
                        BaseData.AddDataElement(Elem.Substation);
                }
               
            
            
            BorderingLines.TrimExcess();


            if (BaseElement is MM_Boundary)
                CompleteForm(BorderingLines, new MM_Element[] { BaseElement }, (BaseElement as MM_Boundary).Min, (BaseElement as MM_Boundary).Max, null);
            else if (BaseElement is MM_Substation)
                CompleteForm(BorderingLines, new MM_Element[] { BaseElement }, PointF.Empty, PointF.Empty, null);
            else if (BaseElement is MM_RemedialActionScheme)
                CompleteForm(BorderingLines, new MM_Element[] { BaseElement }, TopLeft, BottomRight, null);
            else if (BaseElement is MM_Line)
            {
                MM_Line BaseLine = BaseElement as MM_Line;                
                CompleteForm(BorderingLines, new MM_Element[] { BaseElement }, new PointF(Math.Min(BaseLine.Substation1.Longitude, BaseLine.Substation2.Longitude), Math.Min(BaseLine.Substation2.Latitude, BaseLine.Substation1.Latitude)), new PointF(Math.Max(BaseLine.Substation1.Longitude, BaseLine.Substation2.Longitude), Math.Max(BaseLine.Substation2.Latitude, BaseLine.Substation1.Latitude)),null);
            }
            else if (BaseElement.Substation != null)
                CompleteForm(BorderingLines, new MM_Element[] { BaseElement.Substation }, PointF.Empty, PointF.Empty, null);
            
            



            
            this.nMap = nMap;            
            
            
            //viewPropertyPage.Dock = DockStyle.Fill;
            this.Title = BaseElement.ElemType.Name + " " + BaseElement.Name;
            this.Controls[0].Controls[1].Controls.Clear();
            this.Controls[0].Controls[1].Controls.Add(viewDataTable);            
            this.Controls[0].Controls[1].Controls.Add(viewPropertyPage);
            viewSummary_TypeSelected(BaseData.Data.Tables[BaseElement.ElemType.Name], BaseElement.KVLevel);            


            viewPropertyPage.Visible = true;
            viewPropertyPage.SetElement(BaseElement);
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.Size = new Size(800, 600);
            
        }

        #endregion

        #region One-line view
        /// <summary>
        /// Initiate a new one-line display, and show.
        /// </summary>
        /// <param name="BaseElement">The root element of the one-line</param>
        /// <param name="nMap">The network map associated with the one-line</param>
        public static void OneLine_Display(MM_Element BaseElement, Network_Map nMap)
        {
            Thread FormThread = new Thread(new ParameterizedThreadStart(OneLine_Build));
            FormThread.Name = "One Line: " + BaseElement.ElemType.Name + " " + BaseElement.Name;
            FormThread.SetApartmentState(ApartmentState.STA);
            FormThread.Start(new object[] { BaseElement, nMap, FormThread, Data_Integration.NetworkSource});
        }

        /// <summary>
        /// Initiate a new one-line display in the thread
        /// </summary>
        /// <param name="Parameters">Parmeters for the display</param>
        [STAThread]
        private static void OneLine_Build(object Parameters)
        {

            //First, prepare to receive our input parameters
            Object[] inObj = Parameters as Object[];

            //Now create our form, assign its parameters, and complete it.
            Form_Builder BuilderForm = new Form_Builder();

            BuilderForm.BaseElement = inObj[0] as MM_Element;            
            BuilderForm.nMap = inObj[1] as Network_Map;
            if (BuilderForm.BaseElement is MM_Substation)
                BuilderForm.Title = "One-Line: " + BuilderForm.BaseElement.ElemType.Name + " " + (BuilderForm.BaseElement as MM_Substation).LongName ;
            else
                BuilderForm.Title = "One-Line: " + BuilderForm.BaseElement.ElemType.Name + " " + BuilderForm.BaseElement.Name;
            
            try
            {
                BuilderForm.OneLine_Complete(BuilderForm.BaseElement, inObj[3] as MM_Data_Source);                

                //Add it to the thread and forms collections in the integration layer
                Data_Integration.DisplayForm(BuilderForm, inObj[2] as Thread);               
            }
            catch (Exception ex)
            {
                Program.MessageBox( "Error in one-line display (copied to clipboard):\n" + ex.Message + "\n" + ex.StackTrace, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (BuilderForm.viewOneLine != null)
                    BuilderForm.viewOneLine.ShutDownQuery();
            }
        }

        /// <summary>
        /// Complete the steps to completing the one-line UI.
        /// </summary>
        /// <param name="DataSource">The active data source for our element</param>
        /// <param name="BaseElement">The base element for the one-line</param>
        private void OneLine_Complete(MM_Element BaseElement, MM_Data_Source DataSource)
        {
            //Build up our base data set/tables and controls for minimap and other downstream systems
            List<MM_Line> BorderingLines = new List<MM_Line>();
            BaseData = new DataSet_Base(this.Title);
            BaseData.BaseElement = BaseElement;
            BaseData.AddDataElement(BaseElement);

            if (BaseElement is MM_Substation)
            foreach (MM_Element Elem in MM_Repository.TEIDs.Values)
                try
                {
                    if ((Elem is MM_Line) && (((Elem as MM_Line).ConnectedStations[0] == BaseElement as MM_Substation) || ((Elem as MM_Line).ConnectedStations[1] == BaseElement as MM_Substation)))
                        BorderingLines.Add(Elem as MM_Line);
                    else if (Elem.Substation == BaseElement)
                        BaseData.AddDataElement(Elem);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.Message);
                }

            /*if (BaseElement is MM_Substation)
                foreach (MM_Line Line in MM_Repository.Lines.Values)
                    if ((Line.ConnectedStations[0] == BaseElement) || (Line.ConnectedStations[1] == BaseElement))
                        BaseData.AddDataElement(Line);
            */

            //Create our one-line control
            viewOneLine = new OneLine_Viewer(false);
            viewOneLine.Name = "OneLine";
            viewOneLine.Dock = DockStyle.Fill;
            viewOneLine.OneLineElementClicked += new OneLine_Viewer.OneLineElementClickedDelegate(OneLine_OneLineElementClicked);            
            viewOneLine.OneLineDataUpdated += new OneLine_Viewer.OneLineDataUpdatedDelegate(viewOneLine_OneLineDataUpdated);

            //Complete the one-line form                        
            CompleteForm(BorderingLines, new MM_Element[] { BaseElement }, PointF.Empty, PointF.Empty, viewOneLine);
            viewViolations.SetControls(viewOneLine);
            viewOneLine.OneLineLoadCompleted += new EventHandler(viewOneLine_OneLineLoadCompleted);
            viewOneLine.SetControls(BaseElement, nMap, BaseData, viewViolations, DataSource);

            
            if (viewSummary.Items.Count > 1)
                grpSummary.Height = (grpSummary.Height - viewSummary.Height) + viewSummary.Items[viewSummary.Items.Count - 1].GetBounds(ItemBoundsPortion.Entire).Bottom + 30;


            //Now resize everything appropriately.
            //this.Size = new Size(splMain.Panel1.Right + (splMain.Panel2.Controls[0] as SplitContainer).Panel2.Width + viewOneLine.MaxSize.Width, viewOneLine.MaxSize.Height);
        }

        /// <summary>
        /// When a one-line has finished loading, update our data base
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void viewOneLine_OneLineLoadCompleted(object sender, EventArgs e)
        {
            viewSummary.SetControls(BaseData);
        }


        /// <summary>
        /// Handle closing by shutting down all queries
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            if (viewOneLine != null)
                viewOneLine.ShutDownQuery();
            else if (BaseData != null && BaseData.Data.DataSetName == "Element summary")
            {
                e.Cancel = true;
                this.Hide();
            }
            base.OnClosing(e);
        }


        /// <summary>
        /// Handle the updated data coming in
        /// </summary>
        /// <param name="OutData">The incoming data set</param>
        private void viewOneLine_OneLineDataUpdated(DataSet OutData)
        {
            viewSummary.SetControls( BaseData);
        }


        /// <summary>
        /// Handle the user's clicking on a one-line element
        /// </summary>
        /// <param name="ClickedElement"></param>
        /// <param name="e"></param>
        private void OneLine_OneLineElementClicked(MM_OneLine_Element ClickedElement, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                //If we have a server, show our history and details, otherwise pull up our table that the user can edit
                if (Data_Integration.MMServer != null)
                    viewPropertyPage.SetElement(ClickedElement.BaseElement);
                else
                {                    
                    foreach (ListViewItem lvI in viewSummary.Items)                        
                        if (lvI.Text != ClickedElement.ElemType.ToString())
                            lvI.Focused = lvI.Selected = false;
                        else
                        {
                            lvI.Focused = lvI.Selected = true;
                            viewSummary.BoxSubItem(lvI.SubItems[1]);                            
                        }
                    viewSummary_TypeSelected(BaseData.Data.Tables[ClickedElement.ElemType.ToString()],null);
                    viewDataTable.Select();
                    viewDataTable.ClearSelection();
                    foreach (DataGridViewRow dgr in viewDataTable.Rows)
                        if (dgr.Cells[0].Value == ClickedElement.BaseElement )                        
                            dgr.Selected = true;
                }
            }

            else
            {
                MM_Popup_Menu c = new MM_Popup_Menu();
                if (ClickedElement.ElemType == MM_OneLine_Element.enumElemTypes.Descriptor || ClickedElement.ElemType == MM_OneLine_Element.enumElemTypes.SecondaryDescriptor)
                    c.Show(ClickedElement, e.Location, ClickedElement.ParentElement.BaseElement, true, viewOneLine.DataSourceApplication);
                else
                    c.Show(ClickedElement, e.Location, ClickedElement.BaseElement, true, viewOneLine.DataSourceApplication);
                /*
                //ClickedElement.BaseElement.BuildMenuItems(c, false, true);
                if (ClickedElement.ConnectedElements != null)
                {
                    c.Items.Add("-");
                    foreach (Int32 Conn in ClickedElement.ConnectedElements)
                        c.Items.Add(" Connected to: " + viewOneLine.Elements[Conn].BaseElement.ElemType + " " + viewOneLine.Elements[Conn].BaseElement.Name);
                }
                c.Show(ClickedElement, e.Location);*/
            }
        }

       
      

        /// <summary>
        /// Handle a new selection for the type summary
        /// </summary>
        /// <param name="SelectedTable">The table selected</param>
        /// <param name="SelectedKVLevel">The KV Level selected</param>
        private void viewSummary_TypeSelected(DataTable SelectedTable, MM_KVLevel SelectedKVLevel)        
        {
            if (viewDataTable == null)
                return;
            viewDataTable.DataSource = null;

            //First, set our data
            BindingSource b = new BindingSource();
            b.DataSource = SelectedTable;

            if (SelectedKVLevel != null && SelectedTable.Columns.Contains("KV Levels"))
                b.Filter = "[KV Levels] LIKE '%" + SelectedKVLevel.Name.Split(' ')[0] + "%'";
            else if (SelectedKVLevel != null && SelectedTable.Columns.Contains("KV Level"))
                b.Filter = "[KV Level]='" + SelectedKVLevel.Name.Split(' ')[0] + "'";
            else
                b.RemoveFilter();
            viewDataTable.SetDataSource(b);                            
            

            //Now, if we're in a one-line view, make sure the property page is hidden.
            viewDataTable.Visible = true;

            if (viewPropertyPage != null)
                viewPropertyPage.SetElement(null);
            viewDataTable.BringToFront();
            
            //If we have a collapsed panel, show it
            if (viewDataTable.Parent is SplitterPanel && ((viewDataTable.Parent as SplitterPanel).Parent as SplitContainer).Panel2Collapsed)
                ((viewDataTable.Parent as SplitterPanel).Parent as SplitContainer).Panel2Collapsed = false;
        }

     
        
        /// <summary>
        /// Handle the splitter moving for the one-line and lasso views
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LassoOneLine_SplitterMoved(object sender, EventArgs e)
        {
            //First, size and position our mini-map appropriately.
            grpMiniMap.Size = new Size(splMain.Panel1.DisplayRectangle.Width, splMain.Panel1.DisplayRectangle.Width);

            //Now adjust the widths of the summary and violations view
            grpSummary.Width = grpViolations.Width = grpMiniMap.Width;            
            
            //Move the mini-map below the summary
            grpMiniMap.Top = grpSummary.Bottom;
                        
            //grpMiniMap.Top = splMain.Panel1.DisplayRectangle.Height - grpMiniMap.Height; 

            
            //Now move everything so the summary view is on top, and the violations sized to fit in the middle.
            grpViolations.Top = grpMiniMap.Bottom;
            grpViolations.Height = DisplayRectangle.Height - grpViolations.Top;                

            this.Invalidate();
        }

        #endregion

        #region Element Summary
        /// <summary>
        /// Whent the form is first shown, hide it.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnShown(EventArgs e)
        {
            if (BaseData != null && BaseData.Data.DataSetName == "Element summary")
                this.Hide();
            base.OnShown(e);
        }

        /// <summary>
        /// Initiate a new element summary display
        /// </summary>
        /// <param name="nMap"></param>
        /// <param name="ESWin"></param>
        public static void ElementSummaryDisplay(ToolStripMenuItem ESWin, Network_Map nMap)
        {
            Thread FormThread = new Thread(new ParameterizedThreadStart(ElementSummary_Build));
            FormThread.Name = "Element Summary";
            
            FormThread.Start(new object[] { nMap, FormThread, ESWin });
        }

        /// <summary>
        /// Initiate a new search display in the thread
        /// </summary>
        /// <param name="Parameters">Parmeters for the display</param>
        private static void ElementSummary_Build(object Parameters)
        {
            //First, prepare to receive our input parameters
            Object[] inObj = Parameters as Object[];

            //Now create our form, assign its parameters, and complete it.
            Form_Builder BuilderForm = new Form_Builder();
            BuilderForm.nMap = inObj[0] as Network_Map;
            BuilderForm.Title = (inObj[1] as Thread).Name;
            (inObj[2] as ToolStripMenuItem).Tag = BuilderForm;
            List<MM_Element> Elems = new List<MM_Element>();

            BuilderForm.BaseData = new DataSet_Base("Element summary");

            foreach (MM_Element Elem in MM_Repository.TEIDs.Values)
                if (Elem.TEID > 0 && Elem.ElemType != null && Elem is MM_Company == false)
                {
                    BuilderForm.BaseData.AddDataElement(Elem);
                    Elems.Add(Elem);
                }            
            BuilderForm.CompleteForm(new List<MM_Line>(), Elems.ToArray(), MM_Repository.Counties["STATE"].Min, MM_Repository.Counties["STATE"].Max, null);
            BuilderForm.Visible = false;
            Data_Integration.DisplayForm(BuilderForm, inObj[2] as Thread);
        }
        #endregion


        #region Searching
        /// <summary>
        /// Initiate a new one-line display, and show.
        /// </summary>        
        /// <param name="nMap">The network map associated with the one-line</param>
        /// <param name="TextToSearch">The text to search for</param>
        /// <param name="ControlPressed">Whether control is pressed</param>
        /// <param name="AltPressed">Whether alt is pressed</param>
        public static void SearchDisplay(Network_Map nMap, String TextToSearch, bool ControlPressed, bool AltPressed)
        {
            Thread FormThread = new Thread(new ParameterizedThreadStart(Search_Build));
            FormThread.Name = "Search: " + TextToSearch;
            FormThread.Start(new object[] { nMap, FormThread, TextToSearch, ControlPressed, AltPressed });
        }

        /// <summary>
        /// Initiate a new search display in the thread
        /// </summary>
        /// <param name="Parameters">Parmeters for the display</param>
        private static void Search_Build(object Parameters)
        {
            //First, prepare to receive our input parameters
            Object[] inObj = Parameters as Object[];

            //Now create our form, assign its parameters, and complete it.
            Form_Builder BuilderForm = new Form_Builder();                        
            BuilderForm.nMap = inObj[0] as Network_Map;
            BuilderForm.Title = (inObj[1] as Thread).Name;                           
            BuilderForm.SearchText = (String)inObj[2];
            
            if (!BuilderForm.Search_Complete((bool)inObj[3], (bool)inObj[4]))
                BuilderForm.Dispose();
            else
                Data_Integration.DisplayForm(BuilderForm, inObj[2] as Thread);         
        }

        /// <summary>
        /// Complete the steps to completing the one-line UI.
        /// </summary>
        /// <param name="AltPressed"></param>
        /// <param name="ControlPressed"></param>
        private bool Search_Complete(bool ControlPressed, bool AltPressed)
        {
            //Build our search window            
            Search_Results ctlSearch = new Search_Results();
            ctlSearch.Dock = DockStyle.Fill;

          

            //Assign the search window's controls
            if (!ctlSearch.SetControls( nMap, viewViolations, this.viewMiniMap,SearchText, this, out BaseData, viewSummary, ControlPressed, AltPressed))
                return false;
            
            //Determine our minima and maxima for the regional display
            PointF TopLeft = new PointF(float.NaN, float.NaN);
            PointF BottomRight = new PointF(float.NaN, float.NaN);
            foreach (MM_Element Elem in ctlSearch.BaseElements)
                if (Elem is MM_Substation)
                    TestPoint((Elem as MM_Substation).LatLong, ref TopLeft, ref BottomRight);
                else if (Elem is MM_Line)
                    for (int a = 0; a < 2; a++)
                        TestPoint((Elem as MM_Line).ConnectedStations[a].LatLong, ref TopLeft, ref BottomRight);
                else if (Elem.Substation != null)
                    TestPoint(Elem.Substation.LatLong, ref TopLeft, ref BottomRight);

            SizeF Offset = new SizeF(0.1f, 0.1f);
            TopLeft = PointF.Subtract(TopLeft, Offset);
            BottomRight = PointF.Add(BottomRight, Offset);

            //Pull in our other elements for the form
            CompleteForm(new List<MM_Line>(0), ctlSearch.BaseElements.ToArray(), TopLeft, BottomRight, ctlSearch);
            ctlSearch.miniMap = this.viewMiniMap;
            ctlSearch.SetViolationViews(viewViolations);
            ctlSearch.SetPropertyPage(viewPropertyPage);
            ctlSearch.SetViewSummary(viewSummary);
            return true;
        }

        /// <summary>
        /// Check a point to see whether its latitude and/or longitude should replace the min and/or max.
        /// </summary>
        /// <param name="LatLng"></param>
        /// <param name="TopLeft"></param>
        /// <param name="BottomRight"></param>
        private void TestPoint(PointF LatLng, ref PointF TopLeft, ref PointF BottomRight)
        {
            if (float.IsNaN(TopLeft.X) || (LatLng.X < TopLeft.X))
                TopLeft.X = LatLng.X;
            if (float.IsNaN(TopLeft.Y) || (LatLng.Y < TopLeft.Y))
                TopLeft.Y = LatLng.Y;
            if (float.IsNaN(BottomRight.X) || (LatLng.X > BottomRight.X))
                BottomRight.X = LatLng.X;
            if (float.IsNaN(BottomRight.Y) || (LatLng.Y > BottomRight.Y))
                BottomRight.Y = LatLng.Y;
        }

        /// <summary>
        /// Set the right-hand pane for the display
        /// </summary>
        /// <param name="RightPane">The control to insert into the right-hand pane</param>
        internal bool SetRighthandPane(Control RightPane)
        {
            if (splMain.Panel2.Controls[0] is SplitContainer)
            {
                (splMain.Panel2.Controls[0] as SplitContainer).Panel2.Controls.Clear();
                (splMain.Panel2.Controls[0] as SplitContainer).Panel2.Controls.Add(RightPane);
            }
            else
            {
                splMain.Panel2.Controls.Clear();
                splMain.Panel2.Controls.Add(RightPane);
            }
            return true;
        }
        #endregion

      
    }
}