using MacomberMapClient.Data_Elements.Display;
using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Data_Elements.Violations;
using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.Blackstart;
using MacomberMapClient.User_Interfaces.Menuing;
using MacomberMapClient.User_Interfaces.NetworkMap;
using MacomberMapClient.User_Interfaces.Violations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;
using MacomberMapClient.Properties;
using MacomberMapClient.User_Interfaces.Generic;
using MacomberMapClient.User_Interfaces.Startup;

namespace MacomberMapClient.User_Interfaces.Summary
{

    internal class rHandler : IRequestHandler
    {
        string user = MM_Repository.user;
        string pw = MM_Repository.pw;

        int reloadcount = 0;

        public bool OnBeforeBrowse(IWebBrowser browser, IRequest request, bool isRedirect, bool isMainFrame)
        {
            return false;
        }

        public bool OnCertificateError(IWebBrowser browser, CefErrorCode errorCode, string requestUrl)
        {
            return true;
        }

        public void OnPluginCrashed(IWebBrowser browser, string pluginPath)
        {
            if (reloadcount++ == 0)
                browser.Load(browser.Address);
            else
            {
                MessageBox.Show("Plugin crashed");
            }
        }

        public CefReturnValue OnBeforeResourceLoad(IWebBrowser browser, IRequest request, bool isMainFrame)
        {
            return CefReturnValue.Continue; 
        }

        public bool GetAuthCredentials(IWebBrowser browser, bool isProxy, string host, int port, string realm, string scheme, ref string username, ref string password)
        {
            try
            {
                if (pw == null || user == null || pw.Length == 0)
                {
                    frmWindowsSecurityDialog SecurityDialog = new frmWindowsSecurityDialog();
                    SecurityDialog.CaptionText = Application.ProductName + " " + Application.ProductVersion;
                    SecurityDialog.MessageText = "Enter the credentials to log into HistoricServer";
                    string domain;
                    SecurityDialog.ShowLoginDialog(out user, out pw, out domain);
                }
                username = user;
                password = pw;
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        #region ChromiumPlugin requirements
        public bool OnBeforePluginLoad(IWebBrowser browser, string url, string policyUrl, WebPluginInfo info)
        {
            return false;
        }

        public void OnRenderProcessTerminated(IWebBrowser browser, CefTerminationStatus status)
        {
           
        }

        public bool OnBeforeBrowse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, bool isRedirect)
        {
            throw new NotImplementedException();
        }

        public bool OnOpenUrlFromTab(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture)
        {
            throw new NotImplementedException();
        }

        public bool OnCertificateError(IWebBrowser browserControl, IBrowser browser, CefErrorCode errorCode, string requestUrl, ISslInfo sslInfo, IRequestCallback callback)
        {
            throw new NotImplementedException();
        }

        public void OnPluginCrashed(IWebBrowser browserControl, IBrowser browser, string pluginPath)
        {
            throw new NotImplementedException();
        }

        public CefReturnValue OnBeforeResourceLoad(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            throw new NotImplementedException();
        }

        public bool GetAuthCredentials(IWebBrowser browserControl, IBrowser browser, IFrame frame, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback)
        {
            throw new NotImplementedException();
        }

        public bool OnBeforePluginLoad(IWebBrowser browserControl, IBrowser browser, string url, string policyUrl, WebPluginInfo info)
        {
            throw new NotImplementedException();
        }

        public void OnRenderProcessTerminated(IWebBrowser browserControl, IBrowser browser, CefTerminationStatus status)
        {
            throw new NotImplementedException();
        }

        public bool OnQuotaRequest(IWebBrowser browserControl, IBrowser browser, string originUrl, long newSize, IRequestCallback callback)
        {
            throw new NotImplementedException();
        }

        public void OnResourceRedirect(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, ref string newUrl)
        {
            throw new NotImplementedException();
        }

        public bool OnProtocolExecution(IWebBrowser browserControl, IBrowser browser, string url)
        {
            throw new NotImplementedException();
        }

        public void OnRenderViewReady(IWebBrowser browserControl, IBrowser browser)
        {
            throw new NotImplementedException();
        }
        #endregion


        public void OnResourceLoadComplete(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength)
        {
            throw new NotImplementedException();
        }

        public bool OnResourceResponse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// The property page displays detailed information about a particular element (line, unit, load, breaker, switch, etc.)
    /// </summary>
    public partial class MM_Property_Page : TabControl
    {
        #region Variable declarations
        /// <summary>The element whose information should be displayed in the property page</summary>
        public MM_Element Element;

        /// <summary>The default style for the listview</summary>
        public ControlStyles DefaultStyle;

        /// <summary>The right-click item handler</summary>
        private MM_Popup_Menu RightClickMenu = new MM_Popup_Menu();

        /// <summary>The network map associated with the large display (for zooming/panning the map on right-click</summary>
        private MM_Network_Map_DX nMap;

        /// <summary>The violation viewer associated with this display</summary>
        private MM_Violation_Viewer violView;

        /// <summary>The mini-map associated with this display</summary>
        private MM_Mini_Map miniMap;


        string FlowgateUrl = Settings.Default.FlowgateBaseUrl;

        string HistoricServerUrl = Settings.Default.HistoricServerBaseUrl;

        string HistoricFrameworkPath = Settings.Default.HistoricFrameworkPath;

        #endregion

        static rHandler rh = null;

        static void AddCefPlugins()
        {
            string plgdir0 = @"C:\Program Files\Microsoft Silverlight\5.1.40905.0\npctrl.dll";
            string plgdir1 = @"C:\Program Files\Microsoft Silverlight\5.1.40728.0\npctrl.dll";
            string plgdir2 = @"C:\Program Files\Microsoft Silverlight\5.1.31211.0\npctrl.dll";
            string plgdir3 = @"C:\Program Files\Microsoft Silverlight\5.1.40416.0\npctrl.dll";

            try
            {
                if (File.Exists(plgdir0))
                    Cef.AddWebPluginPath(plgdir0);
                else if (File.Exists(plgdir1))
                    Cef.AddWebPluginPath(plgdir1);
                else if (File.Exists(plgdir2))
                    Cef.AddWebPluginPath(plgdir2);
                else if (File.Exists(plgdir3))
                    Cef.AddWebPluginPath(plgdir3);
                Cef.RefreshWebPlugins();
            }
            catch (Exception)
            {

            }
        }

        static MM_Property_Page()
        {
            try
            {
                try
                {
                    rh = new rHandler();
                    Cef.OnContextInitialized += AddCefPlugins;
                    Cef.Initialize();

                }
                catch (Exception ex)
                {
                    Cef.OnContextInitialized -= AddCefPlugins;
                    MM_System_Interfaces.LogError(ex);
                }
            }
            catch (Exception)
            {

            }
        }

        #region Initialization
        /// <summary>
        /// Create a new property page
        /// </summary>
        public MM_Property_Page(MM_Network_Map_DX nMap, MM_Violation_Viewer violView, MM_Mini_Map miniMap)
        {
            this.nMap = nMap;
            this.violView = violView;
            this.miniMap = miniMap;
            DetermineDefaultStyle();
            try
            {
                if (!Cef.IsInitialized)
                    Cef.Initialize();
            }
            catch (Exception ex)
            {
                MM_System_Interfaces.LogError(ex);
            }
        }


        /// <summary>
        /// Create a new property page
        /// </summary>
        /// <param name="Element">The element in question</param>
        public MM_Property_Page(MM_Element Element)
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
                SetStyle(Style, (DefaultStyle & Style) == Style);
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
                {
                    try
                    {
                        if (mI.Name != "Coordinates" && !InValues.ContainsKey(mI.Name))
                            if (mI is FieldInfo)
                                InValues.Add(mI.Name, ((FieldInfo)mI).GetValue(Element));
                            else if (mI is PropertyInfo)
                                InValues.Add(mI.Name, ((PropertyInfo)mI).GetValue(Element, null));
                    }
                    catch (Exception ex)
                    {
                        MM_System_Interfaces.LogError(ex);
                    }

                }

                //Now, handle our information
                MM_AlarmViolation_Type ViolTmp;
                foreach (KeyValuePair<String, Object> kvp in InValues)
                    try
                    {
                        AddValue(kvp.Key, kvp.Value, NewView.Nodes, out ViolTmp);
                    }
                    catch (Exception ex)
                    { }


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

                //Add in our tracking option
                TabPage TrackingPage = new TabPage();
                TrackingPage.Text = "Tracking";
                this.TabPages.Add(TrackingPage);
                FlowLayoutPanel flpTrack = new FlowLayoutPanel();
                flpTrack.ForeColor = Color.White;
                flpTrack.BackColor = Color.Black;
                flpTrack.AutoScroll = true;
                flpTrack.Dock = DockStyle.Fill;
                TrackingPage.Controls.Add(flpTrack);

                foreach (MemberInfo mI in Element.GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy))
                    if ((mI is FieldInfo && ((FieldInfo)mI).FieldType == typeof(float)) || (mI is PropertyInfo && ((PropertyInfo)mI).PropertyType == typeof(float)))
                    {
                        MM_LoadGen_Tracking_Operator Oper = new MM_LoadGen_Tracking_Operator(mI.Name, Element.ElemType, mI);
                        Oper.Elements.Add(Element);
                        flpTrack.Controls.Add(Oper);
                        Oper.BeginTracking();
                    }
                
                if (!string.IsNullOrWhiteSpace(Element.HistoricServerPath) && HistoricServerUrl != null && HistoricServerUrl.Length > 2)
                {
                    try
                    {
                        if (!HistoricServerUrl.EndsWith("/"))
                            HistoricServerUrl += "/";

                        TabPage historyPage = new TabPage();
                        historyPage.Text = "HistoricServer";
                        this.TabPages.Add(historyPage);
                        string url = HistoricServerUrl + @"/#/Displays/Adhoc?DataItems=" + TranslateElementPath(Element.HistoricServerPath) + "&mode=kiosk&hideToolBar";
                        

                        browser = new ChromiumWebBrowser(url);
                        browser.RequestHandler = rh;
                        historyPage.Controls.Add(browser);
                    }
                    catch (Exception ex)
                    {
                        MM_System_Interfaces.LogError(ex);
                    }
                }
                //If we have historic data access, add a history tab
                if (MM_Server_Interface.Client != null)
                {
                    TabPage NewPage = new TabPage();
                    NewPage.Text = "History";
                    this.TabPages.Add(NewPage);
                    MM_Historic_Viewer hView = new MM_Historic_Viewer(MM_Historic_Viewer.GraphModeEnum.HistoricalOnly, MM_Historic_Viewer.GetMappings(Element), new string[] { }, "");
                    hView.Dock = DockStyle.Fill;
                    NewPage.Controls.Add(hView);
                    SelectedTab = NewPage;
                    if (this.Parent.Controls.Count == 1)
                    {
                        TabPage NewPage2 = new TabPage();
                        NewPage2.Text = "Switch orientation";
                        this.TabPages.Add(NewPage2);
                    }
                }
                if (((Element is MM_Unit && !string.IsNullOrWhiteSpace(((MM_Unit)Element).MarketResourceName)) || (Element is MM_Contingency && ((MM_Contingency)Element).Type == "Flowgate")) && FlowgateUrl != null && FlowgateUrl.Length > 2)
                {
                    try
                    {
                        if (!FlowgateUrl.EndsWith("/"))
                            FlowgateUrl += "/";
                        TabPage FlowgatePage = new TabPage();
                        FlowgatePage.Text = "Flowgate";
                        this.TabPages.Add(FlowgatePage);
                        string url = "";
                        if (Element is MM_Unit)
                            url = FlowgateUrl + @"#/rc/gendetails?item=" + Element.Operator.Name.Replace("TOPOLOGY.", "") + "." + ((MM_Unit)Element).MarketResourceName;
                        else if (Element is MM_Contingency)
                        {
                            url = FlowgateUrl + @"#/rc/flowgatedetails?flowgate=" + ((MM_Contingency)Element).Name;
                        }
                        cbrowser = new ChromiumWebBrowser(url);
                        cbrowser.RequestHandler = rh;
                        cbrowser.LoadError += Cbrowser_LoadError;
                        FlowgatePage.Controls.Add(cbrowser);
                        SelectedTab = FlowgatePage;
                    }
                    catch (Exception ex)
                    {
                        MM_System_Interfaces.LogError(ex);
                    }
                }
                
                this.Visible = true;
                this.BringToFront();

                //If we're sharing this control with another, offer a back button
                if (this.Parent.Controls.Count != 1)
                    this.TabPages.Add("(back) ");
            }
        }

        private void Cbrowser_LoadError(object sender, LoadErrorEventArgs e)
        {
             MM_System_Interfaces.LogError(e.ErrorText);
        }

        ChromiumWebBrowser browser;
        ChromiumWebBrowser cbrowser; // Flowgate browser

        /// <summary>
        /// Handle the changing of the tab page to handle the orientation switch
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSelected(TabControlEventArgs e)
        {
            if (e.Action == TabControlAction.Selected && e.TabPage != null && e.TabPage.Text == "Switch orientation")
            {
                SplitterPanel sC = Parent as SplitterPanel;
                if (sC != null)
                {
                    SplitContainer sC2 = sC.Parent as SplitContainer;
                    sC2.Orientation = sC2.Orientation == Orientation.Horizontal ? Orientation.Vertical : Orientation.Horizontal;
                }
                this.SelectedIndex = e.TabPageIndex - 1;
            }
            if (Element == null)
                return;
            if (e.Action == TabControlAction.Selected && e.TabPage != null && e.TabPage.Text == "Flowgate" && Element != null)
            {
               if (Element is MM_Unit)
                    cbrowser.Load(FlowgateUrl + @"#/rc/gendetails?item=" + Element.Operator.Name.Replace("TOPOLOGY.", "") + "." + ((MM_Unit)Element).MarketResourceName);
               else if (Element is MM_Contingency)
               {
                    cbrowser.Load(FlowgateUrl + @"#/rc/flowgatedetails?flowgate=" + ((MM_Contingency)Element).Name);
               }
                cbrowser.Visible = true;

            }
            else if (e.Action == TabControlAction.Selected && e.TabPage != null && e.TabPage.Text == "HistoricServer" && Element != null && Element.HistoricServerPath != null)
            {
                var ap = HistoricFrameworkPath;
                if (ap != null && !ap.EndsWith("\\"))
                    ap += "\\";
                if (!ap.StartsWith("\\"))
                    ap = "\\\\" + ap;
                
                if (Element is MM_Line)
                {
                    var line = (MM_Line)Element;
                    if (!line.HistoricServerPath.Contains(";") && line.ToEndKey != null)
                    {
                        string segment = line.Name.Substring(line.Name.LastIndexOf("."));


                        string toPath = "";
                        string fromPath = "";
                        if (line.ToEndKey.IndexOf(line.Substation1.Name, StringComparison.OrdinalIgnoreCase) > 0)
                            toPath = ap + @"OperatingAreas\" + line.Substation1.Area + "\\Substations\\" + line.Substation1.Name + "\\Lines\\" + line.ToEndKey.Substring(line.ToEndKey.IndexOf(line.Substation1.Name) ) + segment;
                        else
                            toPath = ap + @"OperatingAreas\" + line.Substation2.Area + "\\Substations\\" + line.Substation2.Name + "\\Lines\\" + line.ToEndKey.Substring(line.ToEndKey.IndexOf(line.Substation2.Name)) + segment;
                        if (line.FromEndKey.IndexOf(line.Substation2.Name, StringComparison.OrdinalIgnoreCase) > 0)
                            fromPath = ap + @"OperatingAreas\" + line.Substation2.Area + "\\Substations\\" + line.Substation2.Name + "\\Lines\\" + line.FromEndKey.Substring(line.FromEndKey.IndexOf(line.Substation2.Name)) + segment;
                        else
                            fromPath = ap + @"OperatingAreas\" + line.Substation1.Area + "\\Substations\\" + line.Substation1.Name + "\\Lines\\" + line.FromEndKey.Substring(line.FromEndKey.IndexOf(line.Substation1.Name)) + segment;
                        line.HistoricServerPath = fromPath + "|MW;" + fromPath + "|MW|Estimated;" + toPath + "|MW;" + toPath + "|MW|Estimated";
                    }
                    browser.Load(HistoricServerUrl + @"#/Displays/Adhoc?DataItems=" + Element.HistoricServerPath + "&mode=kiosk&hideToolBar");
                } else if (Element is MM_Unit)
                    browser.Load(HistoricServerUrl + @"#/Displays/50606/BUSESUN?Asset=" + TranslateElementPath(Element.HistoricServerPath) + "&mode=kiosk&hideToolBar");
                else if (Element is MM_TransformerWinding)
                {
                    browser.Load(HistoricServerUrl + @"#/Displays/50604/BUSESXF2?Asset=" + TranslateElementPath(Element.HistoricServerPath) + "&mode=kiosk&hideToolBar");
                } else if (Element is MM_Load)
                {
                    browser.Load(HistoricServerUrl + @"#/Displays/50756/BUSESLD?Asset=" + TranslateElementPath(Element.HistoricServerPath) + "&mode=kiosk&hideToolBar");
                } else
                    browser.Load(HistoricServerUrl + @"#/Displays/Adhoc?DataItems=" + TranslateElementPath(Element.HistoricServerPath));
                
                browser.Visible = true;
            }


            base.OnSelected(e);
        }

        private string TranslateElementPath(string path)
        {
            string newPath = path; 
            if (!path.StartsWith(HistoricFrameworkPath))
            {
                if (newPath.StartsWith("\\"))
                    newPath = newPath.Substring(1);
                if (newPath.StartsWith("\\"))
                    newPath = newPath.Substring(1);
                if (newPath.StartsWith("\\"))
                    newPath = newPath.Substring(1);
                newPath = HistoricFrameworkPath + path;            
            } else
            {
                newPath = newPath.Replace("s\\\\", "s\\");
            }
            newPath = newPath.Replace("s\\\\", "s\\");
            newPath = newPath.Replace(HistoricFrameworkPath + "\\", HistoricFrameworkPath);
            if (!newPath.StartsWith("\\"))
                newPath = "\\\\" + newPath;
            newPath = newPath.Replace(HistoricFrameworkPath + "\\" + HistoricFrameworkPath, HistoricFrameworkPath + "\\");
            return newPath;
            //return WebUtility.UrlEncode(newPath);
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
                NewNode = Nodes.Add(Title + ": " + (OutObj as MM_Substation).DisplayName());
            else if (OutObj is MM_Element)
                NewNode = Nodes.Add(Title + ": " + (OutObj as MM_Element).Name);
            else if (OutObj is MM_DisplayParameter)
                NewNode = Nodes.Add(Title + ": " + (OutObj as MM_DisplayParameter).Name);
            else if (OutObj is Single[])
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
            if (inObj == null)
                return "";
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
