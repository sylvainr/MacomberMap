using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Reflection;
using Macomber_Map.Data_Elements;
using Macomber_Map.Data_Connections;
using Macomber_Map.Data_Connections.Generic;
using MM_Communication.Data_Integration;


namespace Macomber_Map.User_Interface_Components.OneLines
{
    /// <summary>
    /// This class displays a control panel, based on the configuration information provided in the display
    /// </summary>
    public partial class MM_ControlPanel : MM_Form
    {
        #region Variable declarations
        /// <summary>Our flag for editable mode</summary>
        private bool Editable
        {
            get { return (bool)this.Tag; }
        }

        /// <summary>The message associated with the control panel</summary>
        public object[] OutMessage;

        /// <summary>The element associated with the control panel</summary>
        public MM_OneLine_Element SourceElement;

        /// <summary>The configuration of our control panel</summary>
        public XmlElement xConfiguration;

        /// <summary>Our collection of controls</summary>
        public Dictionary<XmlElement, Control> DisplayedControls = new Dictionary<XmlElement, Control>();

        /// <summary>Our collection of values retrieved from the source system</summary>
        public Dictionary<String, Object> RetrievedValues = new Dictionary<string, object>();

        /// <summary>The source application for our display</summary>
        public String SourceApplication;

        /// <summary>The GUID of our data-bound query</summary>
        public Guid QueryGUID;

        /// <summary>Our collection of record locators</summary>
        public MM_OneLine_RecordLocator[] Locators;
        #endregion

        #region Data retrieval
        /// <summary>
        /// Retrieve a floating-point number from our incoming data
        /// </summary>
        /// <param name="Title"></param>
        /// <returns></returns>
        public Single GetSingle(String Title)
        {
            Object InObj;
            if (RetrievedValues.TryGetValue(Title, out InObj))
                if (InObj is DBNull)
                    return float.NaN;
                else
                    return Convert.ToSingle(InObj);
            else
                return float.NaN;
        }

        /// <summary>
        /// Retrieve a floating-point number from our incoming data
        /// </summary>
        /// <param name="Title"></param>
        /// <returns></returns>
        public Int32 GetInt(String Title)
        {
            Object InObj;
            if (RetrievedValues.TryGetValue(Title, out InObj))
                if (InObj is DBNull)
                    return 0;
                else
                    return Convert.ToInt32(InObj);
            else
                return 0;
        }

        /// <summary>
        /// Retrieve a boolean from our incoming data
        /// </summary>
        /// <param name="Title"></param>
        /// <returns></returns>
        public bool GetBoolean(String Title)
        {
            Object InObj;
            if (RetrievedValues.TryGetValue(Title, out InObj))
                if (InObj is DBNull)
                    return false;
                else
                    return Convert.ToBoolean(InObj);
            else
                return false;
        }

        /// <summary>
        /// Retrieve a string from our incoming data
        /// </summary>
        /// <param name="Title"></param>
        /// <returns></returns>
        public String GetString(String Title)
        {
            Object InObj;
            if (RetrievedValues.TryGetValue(Title, out InObj) && InObj != null)
                if (InObj is DBNull)
                    return "-";
            else
                    return InObj.ToString();
            else
                return "-";
        }
        #endregion


        /// <summary>
        /// Initialize a new control panel
        /// </summary>
        /// <param name="SourceElement">Our source element</param>
        /// <param name="xConfiguration">The configuration for the control panel</param>
        /// <param name="SourceApplication"></param>
        /// <param name="DataSource"></param>
        public MM_ControlPanel(MM_OneLine_Element SourceElement, XmlElement xConfiguration, String SourceApplication, MM_Data_Source DataSource)
        {
            InitializeComponent();
            this.xConfiguration = xConfiguration;

            Keys CurKeys = Control.ModifierKeys;
            if (CurKeys == (Keys.Control | Keys.Shift | Keys.Alt))
            {
                this.Tag = true;
                this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            }
            else
                this.Tag = false;

            //Prepare to receive our data
            MM_Communication.Elements.MM_Element.enumElementType ElemType = (MM_Communication.Elements.MM_Element.enumElementType)Enum.Parse(typeof(MM_Communication.Elements.MM_Element.enumElementType), xConfiguration.ParentNode.ParentNode.Attributes["Name"].Value);

            MM_OneLine_TableLocator[] LocatorsToSearch;
            if (MM_Communication.Data_Integration.MM_Integration.OneLineRecordLocators.TryGetValue(ElemType, out LocatorsToSearch))
            foreach (MM_OneLine_TableLocator tLoc in LocatorsToSearch)
                foreach (KeyValuePair<String, MM_OneLine_RecordLocator[]> kvp in tLoc.ControlPanels)
                    if (kvp.Key.Equals(xConfiguration.Attributes["Title"].Value))
                    {
                        Locators = kvp.Value;
                        foreach (MM_OneLine_RecordLocator rLoc in Locators)
                            if (RetrievedValues.ContainsKey(rLoc.Name))
                                Console.WriteLine("Duplicate: " + rLoc.Name);
                            else
                                RetrievedValues.Add(rLoc.Name, null);

                        break;
                    }
            

           
            //Go through and initialize each component
            PropertyInfo pI = null;
            this.Text = xConfiguration.Attributes["Title"].Value + " - " + SourceElement.BaseElement.ElemType.Name + " " + SourceElement.BaseElement.Name + " (" + SourceApplication + ")";
            foreach (XmlAttribute xAttr in xConfiguration.Attributes)
                if ((pI = this.GetType().GetProperty(xAttr.Name)) != null)
                    pI.SetValue(this, MM_Serializable.RetrieveConvertedValue(pI.PropertyType, xAttr.Value, this, false), null);

            this.SourceElement = SourceElement;
            this.xConfiguration = xConfiguration;
            this.SourceApplication = SourceApplication;
            foreach (XmlElement xElem in xConfiguration.ChildNodes)
                if (xElem.Name == "DataSource")
                { }
                else
                    AddElement(this, xElem,DataSource);                

            //If we are in editable mode, add in our propery page
            if (Editable)
            {
                PropertyGrid pG = new PropertyGrid();
                pG.Name = "PropertyGrid";
                pG.Width = 400;
                pG.Height = this.Height;
                pG.Left = this.Width;
                pG.PropertyValueChanged += new PropertyValueChangedEventHandler(pG_PropertyValueChanged);
                this.Controls.Add(pG);
            }

            //Now initiate our query
            QueryGUID = Guid.NewGuid();
            if (Data_Integration.MMServer != null)
                Data_Integration.MMServer.LoadOneLineControlPanel(this, SourceElement, xConfiguration, SourceApplication, QueryGUID);

        }

        /// <summary>
        /// When a property value changes, update everything.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void pG_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            Control SelControl = (s as PropertyGrid).SelectedObject as Control;
            XmlElement SelElem = SelControl.Tag as XmlElement;


            String VariableToChange = e.ChangedItem.Label;
            Object OutValue = e.ChangedItem.Value;

            if (OutValue is Font)
                OutValue = new FontConverter().ConvertToString(OutValue);
            else if (VariableToChange == "Size")
            {
                OutValue = new Rectangle(SelControl.Location, (Size)OutValue);
                VariableToChange = "Bounds";
            }
            else if (OutValue is Point)
                OutValue = ((Point)OutValue).X + "," + ((Point)OutValue).Y;


            XmlAttribute Attr = SelElem.OwnerDocument.CreateAttribute(VariableToChange);
            Attr.Value = OutValue.ToString();
            SelElem.Attributes.Append(Attr);
            XmlNode ParentNode = SelElem.ParentNode;
            while (ParentNode.Name != "ControlPanel")
                ParentNode = ParentNode.ParentNode;
            Clipboard.SetText(ParentNode.OuterXml);
        }

        /// <summary>
        /// Add an element to our display
        /// </summary>
        /// <param name="ParentControl"></param>
        /// <param name="xElem"></param>
        /// <param name="DataSource"></param>
        private void AddElement(Control ParentControl, XmlElement xElem, MM_Data_Source DataSource)
        {
            Control NewCtl = null;
            PropertyInfo pI = null;

            if (xElem.Name == "Label")
            {
                NewCtl = new Label();
                NewCtl.AutoSize = true;
                NewCtl.Text = "?";
            }
            else if (xElem.Name == "TextBox")
            {
                NewCtl = new TextBox();
                NewCtl.KeyPress += new KeyPressEventHandler(NewCtl_KeyPress);
                NewCtl.LostFocus += new EventHandler(NewCtl_LostFocus);
                NewCtl.BackColor = Color.Black;
                NewCtl.ForeColor = Color.White;
                NewCtl.GotFocus += new EventHandler(NewCtl_GotFocus);
            }
            else if (xElem.Name == "CheckBox")
            {
                NewCtl = new CheckBox();
                NewCtl.Text = "";
                NewCtl.AutoSize = true;
            }
            else if (xElem.Name == "Button")
                NewCtl = new Button();
            else if (xElem.Name == "Synchroscope")
                NewCtl = new MM_OneLine_Synchroscope(SourceElement, xElem, SourceApplication, DataSource, this);
            else if (xElem.Name == "GroupBox")
            {
                if (Editable || !xElem.HasAttribute("IfTrue") || (bool)SourceElement.BaseElement.GetType().GetField(xElem.Attributes["IfTrue"].Value).GetValue(SourceElement.BaseElement))
                {
                    NewCtl = new GroupBox();
                    NewCtl.ForeColor = Color.White;
                    foreach (XmlElement xSubElem in xElem.ChildNodes)
                        AddElement(NewCtl, xSubElem, DataSource);                    
                    (NewCtl as GroupBox).AutoSize = true;

                   
                }
            }
            else if (xElem.Name == "TabControl")
            {
                NewCtl = new TabControl();
                foreach (XmlElement xSubElem in xElem.ChildNodes)
                    if (xSubElem.Name == "TabPage")
                    {
                        TabPage NewPage = new TabPage(xSubElem.Attributes["Title"].Value);
                        (NewCtl as TabControl).TabPages.Add(NewPage);
                        foreach (XmlElement xSubElem2 in xSubElem.ChildNodes)
                            AddElement(NewPage, xSubElem2, DataSource);
                    }
            }
            if (xElem.HasAttribute("OnClick"))
                NewCtl.Click += new EventHandler(NewCtl_Click);


            //Go through and deserialize all attributes on our element
            if (NewCtl == null)
                return;

            if (Editable && NewCtl is GroupBox == false && NewCtl is TabPage ==false)
            {
                NewCtl.BackColor = Color.Purple;
                NewCtl.MouseDown += new MouseEventHandler(NewCtl_MouseDown);
                NewCtl.MouseMove += new MouseEventHandler(NewCtl_MouseMove);
                NewCtl.MouseUp += new MouseEventHandler(NewCtl_MouseUp);
            }
            foreach (XmlAttribute xAttr in xElem.Attributes)
                if ((pI = NewCtl.GetType().GetProperty(xAttr.Name)) != null)
                    pI.SetValue(NewCtl, MM_Serializable.RetrieveConvertedValue(pI.PropertyType, xAttr.Value, NewCtl, false), null);
            NewCtl.Top += 4;
            NewCtl.Tag = xElem;

            if (NewCtl is GroupBox)
            {
                //Shift to the right as needed
                int RightMost = 0;
                foreach (Control ctl in this.Controls)
                    RightMost = Math.Max(ctl.Right, RightMost);
                NewCtl.Left = RightMost+4;
            }

            DisplayedControls.Add(xElem, NewCtl);
            ParentControl.Controls.Add(NewCtl);


        }

        /// <summary>
        /// When a text box gets focus, highlight the whole box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewCtl_GotFocus(object sender, EventArgs e)
        {
            (sender as TextBox).SelectAll();
        }

        #region Editing controls
        /// <summary>Our tracking of our element movement</summary>
        private Point MouseDownPoint = Point.Empty;

        /// <summary>
        /// Handle the mouse up event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewCtl_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                XmlElement xElem = (sender as Control).Tag as XmlElement;
                if (xElem.HasAttribute("Location"))
                {
                    Point P = (sender as Control).Location;
                    xElem.Attributes["Location"].Value = String.Format("{0},{1}", P.X, P.Y);
                }

                if (xElem.HasAttribute("Bounds"))
                {
                    Rectangle R = (sender as Control).Bounds;
                    xElem.Attributes["Bounds"].Value = String.Format("{0},{1},{2},{3}", R.Left, R.Top, R.Width, R.Height);
                }

                XmlNode ParentNode = xElem.ParentNode;
                while (ParentNode.Name != "ControlPanel")
                    ParentNode = ParentNode.ParentNode;
                Clipboard.SetText(ParentNode.OuterXml);
            }
        }

        /// <summary>
        /// Our mouse handler for a mouse down event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewCtl_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                MouseDownPoint = e.Location;
                (this.Controls["PropertyGrid"] as PropertyGrid).SelectedObject = sender;
            }
        }

        /// <summary>
        /// Our mouse handler for a mouse move event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewCtl_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Control Sender = sender as Control;
                Point TargetPoint = new Point(Sender.Left + e.X - MouseDownPoint.X, Sender.Top + e.Y - MouseDownPoint.Y);

                if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                {
                    TargetPoint.X = (TargetPoint.X / 5) * 5;
                    TargetPoint.Y = (TargetPoint.Y / 5) * 5;
                }

                Sender.Location = TargetPoint;
            }
        }
        #endregion



        /// <summary>
        /// When our form closes, shut the query down.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            ShutDownQuery();
            base.OnClosed(e);
        }
       

        /// <summary>
        /// Shut down our query 
        /// </summary>
        public void ShutDownQuery()
        {
            if (Data_Integration.MMServer != null)
            Data_Integration.MMServer.ShutdownOneLine(QueryGUID);
        }

     

        /// <summary>
        /// Handle clicking on the item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewCtl_Click(object sender, EventArgs e)
        {
            if (Editable)
                return;
            XmlElement xElem = (XmlElement)(sender as Control).Tag;
            if (!xElem.HasAttribute("OnClick"))
                return;

            //Process all of our commands
            foreach (String str in xElem.Attributes["OnClick"].Value.Split('|'))
            {
                String[] Cmd = str.Split('(', ')', '/');
                String TargetDataSource = "";
                XmlNode FoundNode = xElem;
                while (FoundNode.Name != "ControlPanel")
                    FoundNode = FoundNode.ParentNode;
                if (xElem.HasAttribute("DataSource") && ((FoundNode = FoundNode.SelectSingleNode("AdditionalDataSource/DataSource[@Name='" + xElem.Attributes["DataSource"].Value + "']")) != null))
                    TargetDataSource = FoundNode.ParentNode.Attributes["Name"].Value;

                if (Cmd[0] == "ToggleBoolean")
                {
                    //Restore our checkbox to its original condition, and send our values
                    (sender as CheckBox).Checked ^= true;                    
                    Data_Integration.MMServer.ToggleBoolean(QueryGUID, Convert.ToInt32(RetrievedValues["Subscript"]), Cmd[1], Cmd[2], !Convert.ToBoolean(RetrievedValues[Cmd[2]]), SourceElement.BaseElement.Substation.Name, SourceElement.ElemType.ToString(), SourceElement.BaseElement.Name, TargetDataSource);
                }
                else if (Cmd[0] == "SendCommand")
                {
                    Object OldVal = null;
                    if (Cmd.Length >= 4)
                        OldVal = RetrievedValues[Cmd[3]];
                    if (OldVal == null)
                        Data_Integration.MMServer.SendMessage(QueryGUID, Convert.ToInt32(RetrievedValues["Subscript"]), Cmd[1], Cmd[2], SourceElement.BaseElement.Substation.Name, SourceElement.ElemType.ToString(), SourceElement.BaseElement.Name, "", "", TargetDataSource);
                    else if (OldVal is Boolean)
                        Data_Integration.MMServer.SendMessage(QueryGUID, Convert.ToInt32(RetrievedValues["Subscript"]), Cmd[1], Cmd[2], SourceElement.BaseElement.Substation.Name, SourceElement.ElemType.ToString(), SourceElement.BaseElement.Name, (bool)OldVal ? Cmd[4] : Cmd[5], (bool)OldVal ? Cmd[5] : Cmd[4], TargetDataSource);
                    else
                        Data_Integration.MMServer.SendMessage(QueryGUID, Convert.ToInt32(RetrievedValues["Subscript"]), Cmd[1], Cmd[2], SourceElement.BaseElement.Substation.Name, SourceElement.ElemType.ToString(), SourceElement.BaseElement.Name, OldVal.ToString(), "", TargetDataSource);
                }
                else if (Cmd[0] == "Close")
                    this.Close();
            }
        }

        /// <summary>
        /// When our control loses focus, consider it available for updating
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewCtl_LostFocus(object sender, EventArgs e)
        {
            (sender as TextBox).Modified = false;           
        }

        /// <summary>
        /// When the user presses enter, update our value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewCtl_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\r' && e.KeyChar != '\n')
            {
                (sender as TextBox).Modified = true;
                return;
            }
            XmlElement xElem = (XmlElement)(sender as Control).Tag;
            MM_OneLine_RecordLocator FoundLoc = null;
            foreach (MM_OneLine_RecordLocator rLoc in Locators)
                if (rLoc.Name == xElem.Attributes["DataSource"].Value)
                    FoundLoc = rLoc;
            if (FoundLoc != null)
            {
                String TargetDataSource = "";
                XmlNode FoundNode = xElem;
                int Subscript =  Convert.ToInt32(RetrievedValues["Subscript"]);
                while (FoundNode.Name != "ControlPanel")
                    FoundNode = FoundNode.ParentNode;
                if ((FoundNode = FoundNode.SelectSingleNode("AdditionalDataSource/DataSource[@Name='" + xElem.Attributes["DataSource"].Value + "']")) != null)
                {
                    TargetDataSource = FoundNode.ParentNode.Attributes["Name"].Value;
                    Subscript = Convert.ToInt32(RetrievedValues[FoundNode.ParentNode.SelectSingleNode("DataSource[@ColumnName='__SUB']").Attributes["Name"].Value]);
                }

                //Overide subscript if needed
                if (xElem.HasAttribute("Subscript"))
                    Subscript = Convert.ToInt32(RetrievedValues[xElem.Attributes["Subscript"].Value]);

                String OldValue = RetrievedValues[FoundLoc.Name].ToString();
                Data_Integration.MMServer.UpdateValue(QueryGUID,Subscript, FoundLoc.ColumnName, FoundLoc.Name, (sender as TextBox).Text, OldValue, SourceElement.BaseElement.Substation.Name, SourceElement.ElemType.ToString(), SourceElement.BaseElement.Name, TargetDataSource);
                (sender as TextBox).Modified = false;
                this.Focus();
            }
        }
       
        /// <summary>
        /// Display the name of our panel on display
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            //When painting, draw our border
            base.OnPaint(e);
            //MN//20130607
            //if (User_Interface_Components.GDIPlus.Network_Map_GDI.LockForDataUpdate)
            //{
            //    using (Pen DrawPen = new Pen(SourceApplication == "Study" ? Color.LightGray : SourceApplication == "Telemetered" ? Color.LightGray : Color.LightGray, 3))
            //        e.Graphics.DrawRectangle(DrawPen, 1, 1, DisplayRectangle.Width - 3, DisplayRectangle.Height - 3);
           // }
           // else
           // {
                using (Pen DrawPen = new Pen(SourceApplication == "Study" ? Color.Pink : SourceApplication == "Telemetered" ? Color.MediumAquamarine : Color.SteelBlue, 3))
                    e.Graphics.DrawRectangle(DrawPen, 1, 1, DisplayRectangle.Width - 3, DisplayRectangle.Height - 3);
           // }
        }


        /// <summary>
        /// Update our data as appropriate
        /// </summary>
        private void UpdateDisplay()
        {
            if (InvokeRequired)
                Invoke(new MM_Query_Configuration.UpdateProcedureDelegate(UpdateDisplay));
            else if (this.IsDisposed)
                Data_Integration.MMServer.ShutdownOneLine(QueryGUID);
            else
                foreach (KeyValuePair<XmlElement, Control> kvp in DisplayedControls)
                {
                    Object FoundObject = null;
                    if ((kvp.Value is Label || kvp.Value is TextBox) && kvp.Key.HasAttribute("DataSource"))
                    {
                        if (kvp.Value is TextBox && (kvp.Value as TextBox).Modified)
                            break;

                        if (!RetrievedValues.TryGetValue(kvp.Key.Attributes["DataSource"].Value, out FoundObject))
                            kvp.Value.Text = "?";
                        else if (FoundObject is Single && float.IsNaN((Single)FoundObject) && !Editable)
                            kvp.Value.Text = "";
                        else if (kvp.Key.HasAttribute("NumberFormat"))
                            kvp.Value.Text = Convert.ToSingle(FoundObject).ToString(kvp.Key.Attributes["NumberFormat"].Value);
                        else
                            kvp.Value.Text = FoundObject.ToString();
                    }
                    else if (kvp.Value is CheckBox && kvp.Key.HasAttribute("DataSource"))
                    {
                        bool Invert = kvp.Key.HasAttribute("Invert") ? XmlConvert.ToBoolean(kvp.Key.Attributes["Invert"].Value) : false;
                        CheckState OutState = CheckState.Indeterminate;
                        foreach (String str in kvp.Key.Attributes["DataSource"].Value.Split('|'))
                            if (RetrievedValues.TryGetValue(kvp.Key.Attributes["DataSource"].Value, out FoundObject))
                                OutState = (Invert ^ Convert.ToBoolean(FoundObject)) ? CheckState.Checked : CheckState.Unchecked;
                        (kvp.Value as CheckBox).CheckState = OutState;

                    }

                    if (Editable)
                        kvp.Value.Enabled = true;
                    else if (kvp.Key.HasAttribute("Editable"))
                    {
                        bool TargetEditable;
                        if (kvp.Key.Attributes["Editable"].Value == "true")
                            TargetEditable = true;
                        else if (kvp.Key.Attributes["Editable"].Value == "false")
                            TargetEditable = false;
                        else if (kvp.Key.Attributes["Editable"].Value.StartsWith("!"))
                            TargetEditable = !(RetrievedValues.TryGetValue(kvp.Key.Attributes["Editable"].Value.Substring(1), out FoundObject) ? Convert.ToBoolean(FoundObject) : false);
                        else
                            TargetEditable = RetrievedValues.TryGetValue(kvp.Key.Attributes["Editable"].Value, out FoundObject) ? Convert.ToBoolean(FoundObject) : false;

                        kvp.Value.Enabled = TargetEditable;
                        if (kvp.Value is TextBox)
                            (kvp.Value as TextBox).BorderStyle = TargetEditable ? BorderStyle.Fixed3D : BorderStyle.None;
                    }

                    if (kvp.Key.HasAttribute("VisibleIfGreater"))
                    {
                        String[] splStr = kvp.Key.Attributes["VisibleIfGreater"].Value.Split(',');
                        bool Visible = false;
                        for (int a = 0; a < splStr.Length; a += 2)
                            if (Convert.ToSingle(RetrievedValues[splStr[a]]) > Convert.ToSingle(splStr[a + 1]))
                                Visible = true;
                        kvp.Value.Visible = Visible;
                    }
                    else if (kvp.Key.HasAttribute("VisibleIfLessThanOrEqual"))
                    {
                        String[] splStr = kvp.Key.Attributes["VisibleIfLessThanOrEqual"].Value.Split(',');
                        bool Visible = false;
                        for (int a = 0; a < splStr.Length; a += 2)
                            if (Convert.ToSingle(RetrievedValues[splStr[a]]) <= Convert.ToSingle(splStr[a + 1]))
                                Visible = true;
                        kvp.Value.Visible = Visible;
                    }
                    if (kvp.Key.HasAttribute("ConditionalColor"))
                    {
                        String[] splStr = kvp.Key.Attributes["ConditionalColor"].Value.Split(',');
                        if (RetrievedValues.TryGetValue(splStr[0], out FoundObject) && FoundObject is bool)
                            kvp.Value.ForeColor = System.Drawing.ColorTranslator.FromHtml(splStr[(bool)FoundObject ? 1 : 2]);
                    }

                }

            foreach (Control ctl in this.DisplayedControls.Values)
                if (ctl is MM_OneLine_Synchroscope)
                    (ctl as MM_OneLine_Synchroscope).DataUpdated();

        }
    }
}
