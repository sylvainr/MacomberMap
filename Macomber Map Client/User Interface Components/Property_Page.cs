using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Macomber_Map.Data_Connections;
using Macomber_Map.Data_Elements;
using System.Reflection;
using Macomber_Map.User_Interface_Components.NetworkMap;
using System.Collections;

namespace Macomber_Map.User_Interface_Components
{
    /// <summary>
    /// The property page displays detailed information about a particular element (line, unit, load, breaker, switch, etc.)
    /// </summary>
    public partial class Property_Page : TabControl
    {
        #region Variable declarations
        /// <summary>The element whose information should be displayed in the property page</summary>
        public MM_Element Element;

        /// <summary>The default style for the listview</summary>
        public ControlStyles DefaultStyle;

        /// <summary>The right-click item handler</summary>
        private MM_Popup_Menu RightClickMenu = new MM_Popup_Menu();

        /// <summary>The network map associated with the large display (for zooming/panning the map on right-click</summary>
        private Network_Map nMap;

        /// <summary>The violation viewer associated with this display</summary>
        private Violation_Viewer violView;

        /// <summary>The mini-map associated with this display</summary>
        private Mini_Map miniMap;
        #endregion


        #region Initialization
        /// <summary>
        /// Create a new property page
        /// </summary>
        public Property_Page(Network_Map nMap, Violation_Viewer violView, Mini_Map miniMap)
        {
            this.nMap = nMap;
            this.violView = violView;
            this.miniMap = miniMap;
            DetermineDefaultStyle();
            
        }

        
        /// <summary>
        /// Create a new property page
        /// </summary>
        /// <param name="Element">The element in question</param>
        public Property_Page(MM_Element Element)
        {
            this.Element = Element;
            DetermineDefaultStyle();
        }


        

     
        /// <summary>
        /// Go through all our display styles, and add those ones that are set to our collection
        /// </summary>
        private void DetermineDefaultStyle()
        {
            foreach (ControlStyles Style in Enum.GetValues(typeof(ControlStyles)))
                if (GetStyle(Style))
                    DefaultStyle |= Style;

        }

        private void SetDefaultStyle()
        {
            foreach (ControlStyles Style in Enum.GetValues(typeof(ControlStyles)))
                    SetStyle(Style,(DefaultStyle & Style) == Style);
        }
        #endregion


        #region Element assignment
        /// <summary>
        /// Assign a new element to the property page
        /// </summary>
        /// <param name="Element"></param>
        public void SetElement(MM_Element Element)
        {

            //Don't go through the work of reassigning the same element
            this.ImageList = MM_Repository.ViolationImages;
            if ((Element != null) && (this.Element == Element))
                return;

            //Assign the element
            this.Element = Element;


            //Clear all tabs            
            this.TabPages.Clear();

            if (Element == null)
                SetStyle(ControlStyles.UserPaint, true);
            else
            {
                //Reset to our default style
                SetDefaultStyle();

                //Now add in the information from our XML configuration file for this item.
                TabPage LocalPage = new TabPage(Element.ElemType.Name + " " + Element.Name);
                this.TabPages.Add(LocalPage);

                TreeView NewView = new TreeView();
                NewView.NodeMouseClick += new TreeNodeMouseClickEventHandler(NewView_NodeMouseClick);
                NewView.Dock = DockStyle.Fill;

                SortedDictionary<String, Object> InValues = new SortedDictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);
                InValues.Add("Name", Element.Name);
                if (Element.Substation != null)
                    InValues.Add("Substation", Element.Substation);
                if (Element.KVLevel != null)
                    InValues.Add("KV Level", Element.KVLevel);



                //Now, pull in all of the members
                MemberInfo[] inMembers = Element.GetType().GetMembers();
                foreach (MemberInfo mI in inMembers)
                    if (mI.Name != "Coordinates" && !InValues.ContainsKey(mI.Name))
                        if (mI is FieldInfo)
                            InValues.Add(mI.Name, ((FieldInfo)mI).GetValue(Element));
                        else if (mI is PropertyInfo)
                            InValues.Add(mI.Name, ((PropertyInfo)mI).GetValue(Element, null));



                //Now, handle our information
                MM_AlarmViolation_Type ViolTmp;
                foreach (KeyValuePair<String, Object> kvp in InValues)
                    AddValue(kvp.Key, kvp.Value, NewView.Nodes, out ViolTmp);


                //If we're a substation, find our associated lines
                if (Element is MM_Substation)
                {
                    TreeNode LineNode = NewView.Nodes.Add("Lines");
                    Dictionary<MM_KVLevel, List<MM_Line>> Lines = new Dictionary<MM_KVLevel, List<MM_Line>>();
                    foreach (MM_Line TestLine in MM_Repository.Lines.Values)
                        if (TestLine.Permitted)
                            if (Array.IndexOf(TestLine.ConnectedStations, Element) != -1)
                            {
                                if (!Lines.ContainsKey(TestLine.KVLevel))
                                    Lines.Add(TestLine.KVLevel, new List<MM_Line>());
                                Lines[TestLine.KVLevel].Add(TestLine);
                            }
                    foreach (KeyValuePair<MM_KVLevel, List<MM_Line>> kvp in Lines)
                    {
                        TreeNode KVNode = LineNode.Nodes.Add(kvp.Key.ToString());
                        foreach (MM_Line LineToAdd in kvp.Value)
                            (KVNode.Nodes.Add(LineToAdd.MenuDescription()) as TreeNode).Tag = LineToAdd;
                    }
                }

                //If we have violations, show them.
                if (Element.Violations.Count > 0)
                {
                    TreeNode NewNode = NewView.Nodes.Add("Violations");
                    foreach (MM_AlarmViolation Viol in Element.Violations.Values)
                    {
                        TreeNode ViolNode = (NewNode.Nodes.Add(Viol.MenuDescription()) as TreeNode);
                        ViolNode.Tag = Viol;
                        ViolNode.ImageIndex = Viol.Type.ViolationIndex;
                    }
                }
                LocalPage.Controls.Add(NewView);



                //If we have historic data access, add a history tab                
                TabPage NewPage = new TabPage();
                NewPage.Text = "History";
                this.TabPages.Add(NewPage);
                Historical_Viewer hView = new Historical_Viewer(Historical_Viewer.GraphModeEnum.HistoricalOnly, Historical_Viewer.GetMappings(Element), new string[] { });
                hView.Dock = DockStyle.Fill;
                NewPage.Controls.Add(hView);
                SelectedTab = NewPage;

                //If we're the only control, offer a 'switch orientation' button.
                if (this.Parent.Controls.Count == 1)
                {
                    TabPage NewPage2 = new TabPage();
                    NewPage2.Text = "Switch orientation";
                    this.TabPages.Add(NewPage2);
                }


                //If we're sharing this control with another, offer a back button
                if (this.Parent.Controls.Count != 1)
                    this.TabPages.Add("(back) ");

                this.Visible = true;
                this.BringToFront();
            }
        }

        /// <summary>
        /// Handle the changing of the tab page to handle the orientation switch
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSelected(TabControlEventArgs e)
        {
            if (e.Action == TabControlAction.Selected && e.TabPage != null && e.TabPage.Text ==   "Switch orientation")
            {
                SplitterPanel sC = Parent as SplitterPanel;
                if (sC != null)
                {
                    SplitContainer sC2 = sC.Parent as SplitContainer;
                    sC2.Orientation = sC2.Orientation == Orientation.Horizontal ? Orientation.Vertical : Orientation.Horizontal;
                }
                this.SelectedIndex = e.TabPageIndex - 1;
            }
            base.OnSelected(e);
        }


        /// <summary>
        /// Add a value to our elements
        /// </summary>
        /// <param name="Title"></param>
        /// <param name="OutObj"></param>
        /// <param name="Nodes"></param>
        /// <param name="OutViolation"></param>
        private TreeNode AddValue(String Title, Object OutObj, TreeNodeCollection Nodes, out MM_AlarmViolation_Type OutViolation)
        {
            TreeNode NewNode;
            OutViolation = null;
            if (OutObj == null)
                return null;
            else if (OutObj is Point)
            {
                NewNode = Nodes.Add(Title);
                NewNode.Nodes.Add("Latitude: " + ((PointF)OutObj).X);
                NewNode.Nodes.Add("Longitude: " + ((PointF)OutObj).Y);
            }
            else if (OutObj is MM_Substation)            
                NewNode = Nodes.Add(Title+ ": " + (OutObj as MM_Substation).DisplayName());
            else if (OutObj is MM_Element)
                NewNode = Nodes.Add(Title + ": " + (OutObj as MM_Element).Name);
            else if (OutObj is MM_DisplayParameter)
            NewNode =             Nodes.Add(Title + ": " + (OutObj as MM_DisplayParameter).Name);
             else if (OutObj  is Single[])
                    {
                        NewNode = Nodes.Add(Title);
                        Single[] TSystemWiderse = (Single[])OutObj;
                        String[] Descriptor = new string[TSystemWiderse.Length];

                        if (TSystemWiderse.Length == 2)
                        {
                            Descriptor[0] = "From: ";
                            Descriptor[1] = "To: ";
                        }
                        else if (TSystemWiderse.Length == 3)
                        {
                            Descriptor[0] = "Norm: ";
                            Descriptor[1] = "2 Hour: ";
                            Descriptor[2] = "15 Min: ";
                        }
                        else
                            for (int a = 0; a < TSystemWiderse.Length; a++)
                                Descriptor[a] = a.ToString("#,##0") + ": ";
                        for (int a = 0; a < TSystemWiderse.Length; a++)
                            NewNode.Nodes.Add(Descriptor[a] + TSystemWiderse[a].ToString());
             }
            else if (OutObj is IEnumerable && (OutObj is String == false))
            {
                NewNode = Nodes.Add(Title);
                MM_AlarmViolation_Type ThisViol = null;
                foreach (Object obj in (IEnumerable)OutObj)
                {
                    AddValue(GetName(obj), obj, NewNode.Nodes, out ThisViol);
                    OutViolation = MM_AlarmViolation_Type.WorstViolation(ThisViol, OutViolation);
                }
                if (OutViolation != null)
                {
                    NewNode.Text = "[" + OutViolation.Acronym + "] " + NewNode.Text;
                    NewNode.ForeColor = OutViolation.ForeColor;
                }
            }
            else
                NewNode = Nodes.Add(Title + ": " + OutObj.ToString());
            
            NewNode.Tag = OutObj;
            if (OutObj is MM_Element)
            {
                MM_AlarmViolation_Type WorstViol = (OutObj as MM_Element).WorstViolationOverall;
                if (WorstViol != null)
                {
                    NewNode.Text = "[" + WorstViol.Acronym + "] " + NewNode.Text;                        
                    NewNode.ForeColor = WorstViol.ForeColor;
                    OutViolation = MM_AlarmViolation_Type.WorstViolation(OutViolation, WorstViol);
                }
                if ((OutObj as MM_Element).FindNotes().Length > 0)
                    NewNode.NodeFont = new Font(NewNode.NodeFont, FontStyle.Italic);
            }
            return NewNode;
        }

        /// <summary>
        /// Return the name for an object
        /// </summary>
        /// <param name="inObj"></param>
        /// <returns></returns>
        public string GetName(Object inObj)
        {
            if (inObj is MM_Substation)
                return (inObj as MM_Substation).DisplayName();
            else if (inObj is MM_Element)
                return ((inObj as MM_Element).ElemType == null ? "" : (inObj as MM_Element).ElemType.Name) + " " + (inObj as MM_Element).Name;
            else
            {
                MemberInfo[] mII = inObj.GetType().GetMember("Name");
                Object inObj2 = null;
                if (mII.Length == 0)
                    inObj2 = inObj;
                else if (mII[0] is FieldInfo)
                    inObj2 = (mII[0] as FieldInfo).GetValue(inObj);
                else if (mII[0] is PropertyInfo)
                    inObj2 = (mII[0] as PropertyInfo).GetValue(inObj, null);
                else
                    return inObj2.ToString();
                if (inObj2 != null && inObj is MM_Element)
                    return ((inObj2 as MM_Element).MenuDescription());
                else
                    return inObj2.ToString();
            }
        }

        /// <summary>
        /// Handle a node mouse click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.Node.Tag is MM_Element)
                RightClickMenu.Show(this, e.Location, e.Node.Tag as MM_Element, true);
        }     
        #endregion

        #region Tab page change
        /// <summary>
        /// Check to see when the tab page changes, in order to refresh the data
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSelectedIndexChanged(EventArgs e)
        {               
            if (this.SelectedTab != null && this.SelectedTab.Text == "(back) ")
            {
                TabPages.Clear();
                this.SendToBack();
                this.Hide();                
            }
            base.OnSelectedIndexChanged(e);            
   
        }
        #endregion


        #region Background drawing
        /// <summary>
        /// Draw the background of the image to a black color
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(Brushes.Black, e.ClipRectangle); 
        }
        
        #endregion
    }
}
