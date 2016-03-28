using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.Communications;
using MacomberMapClient.User_Interfaces.Generic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace MacomberMapClient.User_Interfaces.OneLines
{
    /// <summary>
    /// This class displays a control panel, based on the configuration information provided in the display
    /// </summary>
    public partial class MM_ControlPanel : MM_Form
    {
        #region Variable declarations
        /// <summary>Our flag for editable mode</summary>
        private bool Editable = false;

        /// <summary>The element associated with the control panel</summary>
        public MM_OneLine_Element SourceElement;

        /// <summary>The configuration of our control panel</summary>
        public XmlElement xConfiguration;

        /// <summary>Our collection of controls</summary>
        public Dictionary<XmlElement, Control> DisplayedControls = new Dictionary<XmlElement, Control>();

        /// <summary>The one-line viewer that owns us</summary>
        public MM_OneLine_Viewer OneLineViewer;
        /// <summary>Our panel for sizing</summary>
        private Panel SizePanel;
        #endregion




        /// <summary>
        /// Initialize a new control panel
        /// </summary>
        /// <param name="SourceElement">Our source element</param>
        /// <param name="xConfiguration">The configuration for the control panel</param>
        /// <param name="OneLineViewer">The one-line viewer</param>
        public MM_ControlPanel(MM_OneLine_Element SourceElement, XmlElement xConfiguration, MM_OneLine_Viewer OneLineViewer)
        {
            //xConfiguration.InnerXml = Clipboard.GetText();
            this.OneLineViewer = OneLineViewer;
            InitializeComponent();
            this.xConfiguration = xConfiguration;
            this.SourceElement = SourceElement;

            Keys CurKeys = Control.ModifierKeys;
            if (CurKeys == (Keys.Control | Keys.Shift | Keys.Alt))
            {
                Editable = true;
                this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            }
            else
                Editable = false;

            //Create our Communications view
            btnComm = new Button();
            btnComm.Text = "Comm";
            btnComm.Size = new System.Drawing.Size(78, 17);
            btnComm.Font = new System.Drawing.Font("Arial", 10);
            btnComm.Click += btnComm_Click;
            btnComm.Paint += btnComm_Paint;
            Controls.Add(btnComm);

            SizePanel = new Panel();
            SizePanel.Location = new Point(4, 4);
            SizePanel.AutoScroll = true;
            SizePanel.AutoSize = false;
            SizePanel.HorizontalScroll.Enabled = true;
            SizePanel.VerticalScroll.Enabled = true;
            SizePanel.HorizontalScroll.Visible = true;
            SizePanel.VerticalScroll.Visible = true;
            Controls.Add(SizePanel);
            //Now, initialize the window, and sublements
            MM_Serializable.ReadXml(xConfiguration, this, false);
            this.Text = xConfiguration.Attributes["Title"].Value + " - " + SourceElement.BaseElement.SubLongName + " " + SourceElement.BaseElement.ElemType.Name + " " + SourceElement.BaseElement.Name;
            foreach (XmlElement xElem in xConfiguration.ChildNodes.OfType<XmlElement>())
                if (xElem.Name != "DataSource")
                {
                    AddElement(SizePanel, xElem);
                }
            btnComm.Location = new Point(DisplayRectangle.Width - btnComm.Width+4, 2);//DisplayRectangle.Height - btnComm.Height);                

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


           
            UpdateDisplay();
        }
        /// <summary>
        /// When our form is shown, autosize everything
        /// </summary>
        /// <param name="e"></param>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            SizePanel.AutoSize = true;
            this.AutoSize = true;
            Application.DoEvents();
            Size CurSize = this.Size;
            SizePanel.AutoSize = false;
            this.AutoSize = false;
            this.MaximumSize = CurSize;
            this.Size = CurSize;             
        }
        /// <summary>
        /// Handle the size of our control panel changing
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            try
            {
                btnComm.Left = DisplayRectangle.Width - btnComm.Width - 4;
                SizePanel.Size = new Size(DisplayRectangle.Width - 8, DisplayRectangle.Height - 8);
                this.Invalidate();
            }
            catch { }
        }

        private Button btnComm;

        /// <summary>
        /// Handle our comm button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnComm_Click(object sender, EventArgs e)
        {
            MM_Communication_Viewer.CommViewer.HandleClick(sender, e);
        }

        /// <summary>
        /// Handle our comm event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnComm_Paint(object sender, PaintEventArgs e)
        {
            MM_Communication_Viewer.CommViewer.HandlePaint(sender, e);
            ControlPaint.DrawBorder3D(e.Graphics,btnComm.DisplayRectangle);
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
        private void AddElement(Control ParentControl, XmlElement xElem)
        {
            Control NewCtl = null;

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
            {
                MM_Element[] NodeElems = OneLineViewer.ElementNodes(SourceElement);
                NewCtl = new MM_OneLine_Synchroscope((MM_Breaker_Switch)SourceElement.BaseElement, (MM_Node) NodeElems[0], (MM_Node)NodeElems[1]);
            }
            else if (xElem.Name == "GeneratorControlPanel")
            {
                NewCtl = new MM_Generator_ControlPanel(SourceElement, OneLineViewer);
                this.Closed += ((MM_Generator_ControlPanel)NewCtl).Generator_ControlPanel_Close;
                Panel TempPanel = new Panel();
                TempPanel.Size = NewCtl.Size;
                TempPanel.AutoSize = true;
                TempPanel.AutoScroll = true;
                TempPanel.Margin = new System.Windows.Forms.Padding(0);
                NewCtl.Margin = new System.Windows.Forms.Padding(0);
                TempPanel.Controls.Add(NewCtl);
                NewCtl = TempPanel;
            }
            else if (xElem.Name == "GroupBox")
            {
                if (Editable || !xElem.HasAttribute("IfTrue") || (bool)SourceElement.BaseElement.GetType().GetField(xElem.Attributes["IfTrue"].Value).GetValue(SourceElement.BaseElement))
                {
                    NewCtl = new GroupBox();
                    NewCtl.ForeColor = Color.White;
                    foreach (XmlElement xSubElem in xElem.ChildNodes)
                        AddElement(NewCtl, xSubElem);
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
                            AddElement(NewPage, xSubElem2);
                    }
            }
            if (xElem.HasAttribute("OnClick") || (xElem.Name == "CheckBox" && xElem.HasAttribute("OnChange")))
                NewCtl.Click += new EventHandler(NewCtl_Click);


            //Go through and deserialize all attributes on our element
            if (NewCtl == null)
                return;

            if (Editable && NewCtl is GroupBox == false && NewCtl is TabPage == false)
            {
                NewCtl.BackColor = Color.Purple;
                NewCtl.MouseDown += new MouseEventHandler(NewCtl_MouseDown);
                NewCtl.MouseMove += new MouseEventHandler(NewCtl_MouseMove);
                NewCtl.MouseUp += new MouseEventHandler(NewCtl_MouseUp);
            }
            MM_Serializable.ReadXml(xElem, NewCtl, false);

            NewCtl.Top += 4;
            NewCtl.Tag = xElem;

            if (NewCtl is GroupBox)
            {
                //Shift to the right as needed
                int RightMost = 0;
                foreach (Control ctl in SizePanel.Controls)
                    if (ctl != btnComm)
                    RightMost = Math.Max(ctl.Right, RightMost);
                NewCtl.Left = RightMost + 4;
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
        /// Handle clicking on the item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewCtl_Click(object sender, EventArgs e)
        {
            if (Editable)
                return;
            XmlElement xElem = (XmlElement)(sender as Control).Tag;
            if (!xElem.HasAttribute("OnClick") && !xElem.HasAttribute("OnChange"))
                return;

            //Process any change commands
            if (xElem.HasAttribute("OnChange"))
            {
                String OutCmd;
                String LastValue = "(null)";
                if (sender is CheckBox)
                    OutCmd = ParseCommand(xElem.Attributes["OnChange"].Value, ((CheckBox)sender).Checked);
                else 
                    OutCmd = ParseCommand(xElem.Attributes["OnChange"].Value, null);
                if (sender is CheckBox)
                {
                    ((CheckBox)sender).Checked ^= true;
                   LastValue = ((CheckBox)sender).Checked.ToString();
                }

                CheckState Resp = MM_Server_Interface.SendCommand(OutCmd, LastValue);
                if (Resp == CheckState.Checked)
                {
                    if (!OneLineViewer.ValueChanges.ContainsKey(SourceElement.BaseElement))
                        OneLineViewer.ValueChanges.Add(SourceElement.BaseElement, OutCmd);
                }
                else if (Resp == CheckState.Unchecked)
                {
                    MessageBox.Show("Unable to send command. Please retry.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            //Process all of our commands
            if (xElem.HasAttribute("OnClick"))
            foreach (String str in xElem.Attributes["OnClick"].Value.Split('|'))
            {
                String[] Cmd = str.Split('(', ')', '/');
                String TargetDataSource = "";
                XmlNode FoundNode = xElem;
                while (FoundNode.Name != "ControlPanel")
                    FoundNode = FoundNode.ParentNode;
                if (xElem.HasAttribute("DataSource") && ((FoundNode = FoundNode.SelectSingleNode("AdditionalDataSource/DataSource[@Name='" + xElem.Attributes["DataSource"].Value + "']")) != null))
                    TargetDataSource = FoundNode.ParentNode.Attributes["Name"].Value;

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
        /// Parse an incoming command to handle updates
        /// </summary>
        /// <param name="InCommand"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        private String ParseCommand(String InCommand, Object Value)
        {

            //Attempt to search for, and replace all keyed properties
            StringBuilder UpdatedCommand = new StringBuilder();
            int FirstBracket = InCommand.IndexOf('{');
            int EndBracket = 0;
            while (FirstBracket != -1)
            {
                String InLine = InCommand.Substring(EndBracket, FirstBracket - EndBracket );
                UpdatedCommand.Append(InLine); 
                EndBracket = InCommand.IndexOf('}', FirstBracket)+1;
                String Word = InCommand.Substring(FirstBracket+1, EndBracket - FirstBracket-2 );

                if (Word.Equals("Value", StringComparison.CurrentCultureIgnoreCase))
                    if (Value is bool)
                        UpdatedCommand.Append((bool)Value ? "T" : "F");
                    else
                        UpdatedCommand.Append(Value.ToString());
                else if (Word.Equals("USERID", StringComparison.CurrentCultureIgnoreCase))
                {
                    UpdatedCommand.Append(MM_Server_Interface.UserName + "@" + Environment.MachineName);
                }
                else if (Word.StartsWith("!"))
                {
                    Object InObj = null;
                    if (SourceElement.BaseElement.GetType().GetField(Word.TrimStart('!')) != null)
                        InObj = SourceElement.BaseElement.GetType().GetField(Word.TrimStart('!')).GetValue(SourceElement.BaseElement);
                    else if (SourceElement.BaseElement.GetType().GetProperty(Word.TrimStart('!')) != null)
                        InObj = SourceElement.BaseElement.GetType().GetProperty(Word.TrimStart('!')).GetValue(SourceElement.BaseElement);
                    if (InObj is bool)
                        UpdatedCommand.Append((bool)InObj ? "F" : "T");
                    else
                        UpdatedCommand.Append(InObj.ToString());
                }
                else
                {
                    Object InObj = RetrievedValues(Word);                 
                    UpdatedCommand.Append(InObj.ToString());
                }
                FirstBracket = InCommand.IndexOf('{', FirstBracket + 1);
            }            
            return UpdatedCommand.ToString();
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
            if (xElem.HasAttribute("OnChange"))
            {
                Object SourceObj = SourceElement.BaseElement;
                String[] splStr = xElem.Attributes["DataSource"].Value.Split('.');
                FieldInfo fI=null;
                PropertyInfo pI=null;
                for (int a=0; a < splStr.Length-1; a++)
                {
                    fI = SourceObj.GetType().GetField(splStr[a]);
                    if (fI != null)
                        pI = null;
                    else
                        pI = SourceObj.GetType().GetProperty(splStr[a]);

                    if (fI != null)
                        SourceObj = fI.GetValue(SourceObj);
                    else
                        SourceObj = pI.GetValue(SourceObj);
                }
                fI = SourceObj.GetType().GetField(splStr[splStr.Length-1]);
                if (fI == null)
                    pI = SourceObj.GetType().GetProperty(splStr[splStr.Length - 1]);
                //FieldInfo fI = SourceElement.BaseElement.GetType().GetField(xElem.Attributes["DataSource"].Value);
                try
                {
                    object OutVal = Convert.ChangeType(((TextBox)sender).Text.Trim(), fI != null ? fI.FieldType : pI.PropertyType);
                    object LastVal = fI != null ? fI.GetValue(SourceObj) : pI.GetValue(SourceObj);
                    if (LastVal == null) LastVal = "(null)";
                    
                    if (!OneLineViewer.ValueChanges.ContainsKey(SourceElement.BaseElement)) 
                        OneLineViewer.ValueChanges.Add(SourceElement.BaseElement, OutVal);
                    if (MM_Server_Interface.SendCommand(ParseCommand(xElem.Attributes["OnChange"].Value, OutVal),LastVal.ToString()) == CheckState.Unchecked)
                        MessageBox.Show(String.Format("Unable to send command: Set value '{0}' for [{1}]", ((TextBox)sender).Text, xElem.Attributes["DataSource"].Value), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(String.Format("Unable to write value '{0}' for [{1}] ({2})", ((TextBox)sender).Text, xElem.Attributes["DataSource"].Value, ex.Message), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            (sender as TextBox).Modified = false;
            this.Focus();



            /*MM_OneLine_RecordLocator FoundLoc = null;
            foreach (MM_OneLine_RecordLocator rLoc in Locators)
                if (rLoc.Name == xElem.Attributes["DataSource"].Value)
                    FoundLoc = rLoc;
            if (FoundLoc != null)
            {
                String TargetDataSource = "";
                XmlNode FoundNode = xElem;
                int Subscript = Convert.ToInt32(RetrievedValues["Subscript"]);
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
                Data_Integration.MMServer.UpdateValue(QueryGUID, Subscript, FoundLoc.ColumnName, FoundLoc.Name, (sender as TextBox).Text, OldValue, SourceElement.BaseElement.Substation.Name, SourceElement.ElemType.ToString(), SourceElement.BaseElement.Name, TargetDataSource);
                (sender as TextBox).Modified = false;
                this.Focus();
            }*/
        }

        /// <summary>
        /// Display the name of our panel on display
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            //When painting, draw our border            
            base.OnPaint(e);
            using (Pen DrawPen = new Pen(Data_Integration.SimulatorStatusColor, 3))
                e.Graphics.DrawRectangle(DrawPen, 1, 1, DisplayRectangle.Width - 3, DisplayRectangle.Height - 3);
        }

        private delegate void SafeUpdateDisplay();

        /// <summary>
        /// Update our data as appropriate
        /// </summary>
        private void UpdateDisplay()
        {
            if (InvokeRequired)
                Invoke(new SafeUpdateDisplay(UpdateDisplay));           
            else if (this.IsDisposed)
            { }
            else
                foreach (KeyValuePair<XmlElement, Control> kvp in DisplayedControls)
                {
                    Object FoundObject = null;
                    if ((kvp.Value is Label || kvp.Value is TextBox) && kvp.Key.HasAttribute("DataSource"))
                    {
                        if (kvp.Value is TextBox && (kvp.Value as TextBox).Modified)
                            break;

                        if (kvp.Key.Attributes["DataSource"].Value.Contains('.'))
                        {
                            Object SourceObject = SourceElement.BaseElement;
                            String[] splStr = kvp.Key.Attributes["DataSource"].Value.Split('.');
                            for (int a=0; a < splStr.Length; a++)
                                if (SourceObject != null)
                                    if (SourceObject.GetType().GetField(splStr[a]) != null)
                                        SourceObject = SourceObject.GetType().GetField(splStr[a]).GetValue(SourceObject);
                                    else if (SourceObject.GetType().GetProperty(splStr[a]) != null)
                                        SourceObject = SourceObject.GetType().GetProperty(splStr[a]).GetValue(SourceObject);
                            FoundObject = SourceObject;
                        }
                        else if (!RetrievedValues(kvp.Key.Attributes["DataSource"].Value, out FoundObject))
                            FoundObject = null;
                        else if (FoundObject is Array && kvp.Key.HasAttribute("DataIndex"))
                            FoundObject = ((Array)FoundObject).GetValue(XmlConvert.ToInt32(kvp.Key.Attributes["DataIndex"].Value));
                        
                        
                        if (FoundObject == null)
                            kvp.Value.Text = "?";                            
                        else if (FoundObject is Single && float.IsNaN((Single)FoundObject) && !Editable)
                            kvp.Value.Text = "";
                        else if (kvp.Key.HasAttribute("NumberFormat"))
                            kvp.Value.Text = Convert.ToSingle(FoundObject).ToString(kvp.Key.Attributes["NumberFormat"].Value);
                        else if (FoundObject is MM_Bus)
                            kvp.Value.Text = ((MM_Bus)FoundObject).BusNumber.ToString();
                        else if (FoundObject is MM_Island)
                            kvp.Value.Text = ((MM_Island)FoundObject).ID.ToString();
                        else if (FoundObject is MM_Element)
                            kvp.Value.Text = ((MM_Element)FoundObject).Name;
                        else
                            kvp.Value.Text = FoundObject.ToString();
                      }
                      else if (kvp.Value is CheckBox && kvp.Key.HasAttribute("DataSource"))
                      {
                          bool Invert = kvp.Key.HasAttribute("Invert") ? XmlConvert.ToBoolean(kvp.Key.Attributes["Invert"].Value) : false;
                          CheckState OutState = CheckState.Indeterminate;
                          foreach (String str in kvp.Key.Attributes["DataSource"].Value.Split('|'))
                              if (RetrievedValues(kvp.Key.Attributes["DataSource"].Value, out FoundObject))
                                  OutState = (Invert ^ Convert.ToBoolean(FoundObject)) ? CheckState.Checked : CheckState.Unchecked;
                              else
                              { }
                              
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
                            TargetEditable = !(RetrievedValues(kvp.Key.Attributes["Editable"].Value.Substring(1), out FoundObject) ? Convert.ToBoolean(FoundObject) : false);
                        else
                            TargetEditable = RetrievedValues(kvp.Key.Attributes["Editable"].Value, out FoundObject) ? Convert.ToBoolean(FoundObject) : false;

                        //TODO: Trap
                        TargetEditable = true;


                        kvp.Value.Enabled = TargetEditable;
                        if (kvp.Value is TextBox)
                            (kvp.Value as TextBox).BorderStyle = TargetEditable ? BorderStyle.Fixed3D : BorderStyle.None;
                    }

                    if (kvp.Key.HasAttribute("VisibleIfTrue"))
                    {
                        bool ShouldBeVisible = true;
                        foreach (string Field in kvp.Key.Attributes["VisibleIfTrue"].Value.Split(','))
                            if (Field.StartsWith("!"))
                            {
                                Object Resp = RetrievedValues(Field.Substring(1));
                                if (Resp is bool && (bool)Resp)
                                ShouldBeVisible = false;
                            }                                                                
                            else 
                            {
                                Object Resp = RetrievedValues(Field);
                                if (Resp is bool && !(bool)Resp)
                                    ShouldBeVisible=false;
                            }                                
                        kvp.Value.Visible = ShouldBeVisible;
                    }
                    if (kvp.Key.HasAttribute("VisibleIfFalse"))
                    {
                        bool ShouldBeVisible = true;
                        foreach (string Field in kvp.Key.Attributes["VisibleIfFalse"].Value.Split(','))
                            if (Field.StartsWith("!"))
                            {
                                Object Resp = RetrievedValues(Field.Substring(1));
                                if (Resp is bool && (!(bool)Resp))
                                    ShouldBeVisible=false;                                
                            }
                            else
                            {
                                Object Resp = RetrievedValues(Field);
                                if (Resp is bool && (bool)Resp)
                                    ShouldBeVisible=false;
                            } 
                        kvp.Value.Visible = ShouldBeVisible;
                    }
                    else if (kvp.Key.HasAttribute("VisibleIfGreater"))
                    {
                        String[] splStr = kvp.Key.Attributes["VisibleIfGreater"].Value.Split(',');
                        bool Visible = false;
                        for (int a = 0; a < splStr.Length; a += 2)
                            if (Convert.ToSingle(RetrievedValues(splStr[a])) > Convert.ToSingle(splStr[a + 1]))
                                Visible = true;
                        kvp.Value.Visible = Visible;
                    }
                    else if (kvp.Key.HasAttribute("VisibleIfLessThanOrEqual"))
                    {
                        String[] splStr = kvp.Key.Attributes["VisibleIfLessThanOrEqual"].Value.Split(',');
                        bool Visible = false;
                        for (int a = 0; a < splStr.Length; a += 2)
                            if (Convert.ToSingle(RetrievedValues(splStr[a])) <= Convert.ToSingle(splStr[a + 1]))
                                Visible = true;
                        kvp.Value.Visible = Visible;
                    }
                    else if (kvp.Key.HasAttribute("VisibleIfNotEqual"))
                    {
                        String[] splStr = kvp.Key.Attributes["VisibleIfNotEqual"].Value.Split(',');
                        bool Visible = true;
                        for (int a = 0; a < splStr.Length; a += 2)
                            if (Convert.ToSingle(RetrievedValues(splStr[a])) == Convert.ToSingle(splStr[a + 1]))
                                Visible = false;
                        kvp.Value.Visible = Visible;
                    }
                    if (kvp.Key.HasAttribute("ConditionalColor"))
                    {
                        String[] splStr = kvp.Key.Attributes["ConditionalColor"].Value.Split(',');
                        if (RetrievedValues(splStr[0], out FoundObject) && FoundObject is bool)
                            kvp.Value.ForeColor = System.Drawing.ColorTranslator.FromHtml(splStr[(bool)FoundObject ? 1 : 2]);
                    }

                }

            foreach (Control ctl in this.DisplayedControls.Values)
                if (ctl is MM_OneLine_Synchroscope)
                    (ctl as MM_OneLine_Synchroscope).Refresh();

        }

        /// <summary>
        /// Mimic the TryGetValue approach - retrieve a value if we can, and return whether it was succesful.
        /// </summary>
        /// <param name="ItemToSearch"></param>
        /// <param name="RetrievedValue"></param>
        /// <returns></returns>
        private bool RetrievedValues(String ItemToSearch, out Object RetrievedValue)
        {
            try
            {
                //We need to hard-wire a few items, like near node and far node
                if (ItemToSearch == "NearNodeName")
                {
                    RetrievedValue = OneLineViewer.ElementNodes(SourceElement)[0];
                    return true;
                }
                else if (ItemToSearch == "FarNodeName")
                {
                    MM_Element[] Nodes = OneLineViewer.ElementNodes(SourceElement);
                    RetrievedValue = Nodes[Nodes.Length - 1];
                    return true;
                }
                else if (ItemToSearch == "FromKV")
                {
                    RetrievedValue = SourceElement.Windings[0].xConfig.Attributes["BaseElement.KVLevel"].Value;
                    return true;
                }
                else if (ItemToSearch == "ToKV")
                {
                    RetrievedValue = SourceElement.Windings[1].xConfig.Attributes["BaseElement.KVLevel"].Value;
                    return true;
                }

                Object SourceObject = SourceElement.BaseElement;
                String[] splStr = ItemToSearch.Split('.');
                FieldInfo fI;
                PropertyInfo pI;
                for (int a = 0; a < splStr.Length; a++)
                {
                    if ((fI = SourceObject.GetType().GetField(splStr[a])) != null)
                        SourceObject = fI.GetValue(SourceObject);
                    else if ((pI = SourceObject.GetType().GetProperty(splStr[a])) != null)
                        SourceObject = pI.GetValue(SourceObject);
                    else
                    {
                        RetrievedValue = null;
                        return false;
                    }
                }
                RetrievedValue = SourceObject;
                return true;
            }
            catch
            {
                RetrievedValue = null;
                return false;
            }
        }

        /// <summary>
        /// Mimic the [value] approach - retrieve a value if we can, and return whether it was succesful.
        /// </summary>
        /// <param name="ItemToSearch"></param>
        /// <returns></returns>
        private object RetrievedValues(String ItemToSearch)
        {
            Object InVal;
             if (RetrievedValues(ItemToSearch, out InVal))
                return InVal;
            else
                return null;
        }

        /// <summary>
        /// Every second, update our display
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmrUpdate_Tick(object sender, EventArgs e)
        {
            btnComm.Refresh();
            UpdateDisplay();
        }
    }

}
