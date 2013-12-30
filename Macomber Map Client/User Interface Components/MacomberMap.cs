using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Macomber_Map.Data_Elements;
using Macomber_Map.Data_Connections;
using System.Xml;
using Macomber_Map.User_Interface_Components;

namespace Macomber_Map
{
    /// <summary>
    /// This is the main UI for the Macomber Map system
    /// </summary>
    public partial class MacomberMap : MM_Form
    {
        /// <summary>Whether the violations are currently being shown</summary>
        public bool ViolationsShowing = true;

        /// <summary>Our startup form</summary>
        private Startup_Form Starter;

        /// <summary>Our hidden form for showing violations</summary>
        private Form ViolationForm;

        /// <summary>Our search results helper</summary>
        public SearchResultsHelper ResultsHelper;

        #region Initialization

        /// <summary>
        /// Initialize the network map
        /// </summary>        
        /// <param name="Coordinates">The coordinates to be used for the map</param>
        private void InitNetworkMap(MM_Coordinates Coordinates)
        {
            this.ctlNetworkMap = Macomber_Map.User_Interface_Components.NetworkMap.Network_Map.CreateMap(Coordinates);
            this.ctlNetworkMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ctlNetworkMap.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.ctlNetworkMap.Location = new System.Drawing.Point(0, 0);
            this.ctlNetworkMap.Name = "ctlNetworkMap";
            this.ctlNetworkMap.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.ctlNetworkMap.Size = new System.Drawing.Size(1135, 544);
            this.ctlNetworkMap.TabIndex = 0;
            this.splHorizontal.Panel2.Controls.Add(this.ctlNetworkMap);
            this.ViolationForm = new Form();
            this.ViolationForm.Text = "Violation Viewer - " + Data_Integration.UserName.ToUpper() + " - Macomber Map®";
            this.ViolationForm.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.ViolationForm.FormClosing += new FormClosingEventHandler(ViolationForm_FormClosing);
            this.ViolationForm.Icon = this.Icon;
        }

        /// <summary>
        /// Abort the X button on our form, hide instead
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViolationForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            (sender as Form).Hide();
        }


        /// <summary>
        /// Switch the MM view to the intended view
        /// </summary>
        /// <param name="ShowViolations"></param>
        /// <param name="ViolationsInSeparateWindow"></param>
        public void SwitchMMView(bool ShowViolations, bool ViolationsInSeparateWindow)
        {
            ViolationsShowing = ShowViolations;

            //Depending on our view, shift everything.
            if (ShowViolations && !ViolationsInSeparateWindow)
            {
                //Make sure our violations are in the left-hand window
                if (ViolationForm.Controls.Contains(ctlViolationViewer))
                {
                    ViolationForm.Hide();
                    ViolationForm.Controls.Remove(ctlViolationViewer);
                    ctlViolationViewer.Dock = DockStyle.None;
                }

                //Make sure all controls (mini-map, violation viewer) are associated with the current window
                if (ctlMiniMap.Parent.Parent == splVertical)
                {
                    splVertical.Panel2.Controls.Clear();
                    splVertical.Panel2.Controls.Add(splHorizontal);
                    splHorizontal.Panel1.Controls.Add(ctlViolationViewer);
                    splHorizontal.Panel1.Controls.Add(ctlMiniMap);
                    splHorizontal.Panel2.Controls.Add(ctlNetworkMap);
                }


                //Update the positioning of our element
                ctlMiniMap.Left = 0;
                ctlViolationViewer.Location = Point.Empty;
                ctlMiniMap.Width = splHorizontal.Panel1.DisplayRectangle.Width;
                ctlMiniMap.Height = ctlMiniMap.Width;
                ctlMiniMap.Top = splHorizontal.Panel1.DisplayRectangle.Bottom - ctlMiniMap.Height;
                ctlViolationViewer.Width = ctlMiniMap.Width;
                ctlViolationViewer.Height = ctlMiniMap.Top - 4;
            }
            else
            {
                //Move our minimap to the bottom-right of the display
                if (ctlMiniMap.Parent.Parent == splHorizontal)
                {
                    splHorizontal.Panel1.Controls.Clear();
                    splHorizontal.Panel2.Controls.Clear();
                    splVertical.Panel2.Controls.Add(ctlNetworkMap);
                    splVertical.Panel2.Controls.Add(ctlMiniMap);
                    ctlNetworkMap.BringToFront();
                    ctlMiniMap.BringToFront();
                }

                //Position our mini-map
                int NewSize = Math.Max(ctlNetworkMap.DisplayRectangle.Width / 8, ctlNetworkMap.DisplayRectangle.Height / 8);
                ctlMiniMap.Size = new Size(NewSize, NewSize);
                ctlMiniMap.Location = new Point(ctlNetworkMap.Width - ctlMiniMap.Width, ctlNetworkMap.DisplayRectangle.Height - ctlMiniMap.Height);

                //If needed, move our violation form out and display
                if (ViolationsInSeparateWindow)
                {
                    if (ViolationForm.Controls.Count == 0)
                    {
                        splHorizontal.Panel1.Controls.Remove(ctlViolationViewer);
                        ViolationForm.Controls.Add(ctlViolationViewer);
                        ctlViolationViewer.Dock = DockStyle.Fill;
                    }
                    ViolationForm.Show();
                }
                else
                {
                    ViolationForm.Hide();
                    ViolationForm.Controls.Remove(ctlViolationViewer);
                    ctlViolationViewer.Dock = DockStyle.None;
                }
            }

            Application.DoEvents();
        }



        /// <summary>
        /// Initialize the Macomber Map
        /// </summary>
        /// <param name="Coordinates">The coordinates to be used in the map</param>
        /// <param name="Starter">Our startup form</param>
        public MacomberMap(MM_Coordinates Coordinates, Startup_Form Starter)
        {
            InitializeComponent();
            this.Title = "Main Map";
            this.Starter = Starter;
            Program.LogError("MM: Initializing network map/coordinates");
            InitNetworkMap(Coordinates);

            if (!Data_Integration.Permissions.ShowViolations)
                SwitchMMView(false, false);


            //Assign our integration layer, add the thread and form to it

            Data_Integration.RunningForms.Add(this);
            Data_Integration.RunningThreads.Add(System.Threading.Thread.CurrentThread);

            //Assign our controls
            Program.LogError("MM: Starting network map");
            ctlNetworkMap.SetControls(ctlViolationViewer, ctlMiniMap, 0);
            Program.LogError("MM: Starting violation viewer");
            ctlViolationViewer.SetControls(ctlMiniMap, ctlNetworkMap);
            Program.LogError("MM: Starting key indicators");
            ctlKeyIndicators.SetControls(ctlNetworkMap, ctlMiniMap, ctlViolationViewer, MM_Repository.xConfiguration.DocumentElement["DisplayParameters"]["KeyIndicators"]);
            Program.LogError("MM: Starting mini-map");
            ctlMiniMap.SetControls(ctlNetworkMap, ctlViolationViewer);


            ResizeLeftSlider(this, null);

            //Set up our results helper
            ResultsHelper = new SearchResultsHelper(this);
            ResultsHelper.Visible = false;
            Controls.Add(ResultsHelper);
            ResultsHelper.BringToFront();
            (this.FindForm() as MacomberMap).ctlKeyIndicators.AssignResultsHelper(ResultsHelper);
            

            //Set up our main timer
            Program.MainFormChecker = new System.Threading.Timer(Program.ThreadChecker, System.Threading.Thread.CurrentThread, 15000, 15000);
        }


        #endregion

        /// <summary>Resize the violations and overview map size</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResizeLeftSlider(object sender, EventArgs e)
        {
            if (!Data_Integration.InitializationComplete)
                return;
            SwitchMMView(ViolationsShowing, ViolationForm.Controls.Count > 0);
        }


        /// <summary>
        /// Handle the resizing of the main window display
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MacomberMap_Resize(object sender, EventArgs e)
        {
            this.SuspendLayout();
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.FormBorderStyle = FormBorderStyle.None;
                ctlKeyIndicators.ShowMinMaxIndicators = true;
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.Sizable;
                ctlKeyIndicators.ShowMinMaxIndicators = false;
            }
            ResizeLeftSlider(sender, e);
            if (ctlNetworkMap != null)
                ctlNetworkMap.ResizeComplete();
            this.ResumeLayout(true);
            //ctlNetworkMap.SetControls(ctlViolationViewer, ctlMiniMap, 1); //mn//
        }

        /// <summary>
        /// Reset the network map
        /// </summary>
        public void ResetNetworkMap()
        {
            ctlNetworkMap.ResetMap();
            ctlNetworkMap.SetControls(ctlViolationViewer, ctlMiniMap, 0);
            GC.Collect();

        }

        /// <summary>
        /// Close the network map component
        /// </summary>
        public void ShutdownNetworkMap()
        {
            if (Data_Integration.Permissions.ShowViolations)
                splVertical.Panel2.Controls.Clear();

            if (ctlNetworkMap != null)
                this.ctlNetworkMap.Dispose();
            this.ctlNetworkMap = null;
            GC.Collect();
        }


        /// <summary>
        /// When the Macomber Map is first shown, flip our integration flag that initialization is complete.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnShown(EventArgs e)
        {
            Data_Integration.InitializationComplete = true;
            ctlNetworkMap.Focus();
            this.BringToFront();
            this.Focus();
            Starter.MapReady = true;
            MacomberMap_Resize(this, e);
            ctlKeyIndicators.UpdateElements();
            base.OnShown(e);
            if (Data_Integration.UsePipe)
                MM_Pipe.Initialize(this);
            Program.MapStarted = true;
            ctlKeyIndicators.UpdateElements();


            //Process any user interactions that are queued up
            //lock (Data_Integration.UserInteractionsToProcess)
            //    foreach (KeyValuePair<String, List<TCP_Message>> kvp in Data_Integration.UserInteractionsToProcess)
            //        Data_Integration.MMServer.ProcessUserInteractions(kvp.Key, kvp.Value);
        }

        /// <summary>
        /// Stop the network map from constantly resizing when the control is still being resized
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResizeBegin(EventArgs e)
        {
            ctlNetworkMap.Resizing = true;
            base.OnResizeBegin(e);
        }

        /// <summary>
        /// Return the network map to standard operating
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResizeEnd(EventArgs e)
        {
            ctlNetworkMap.Resizing = false;
            //ctlNetworkMap.ResizeComplete();
            base.OnResizeEnd(e);
        }

        /// <summary>
        /// When the user proceeds to shut down the form, automatically kill the process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MacomberMap_FormClosing(object sender, FormClosingEventArgs e)
        {
            Data_Integration.ShutDown();
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}
