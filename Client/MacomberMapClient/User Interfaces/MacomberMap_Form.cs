using MacomberMapClient.Data_Elements.Geographic;
using MacomberMapClient.Data_Elements.SystemInformation;
using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.Generic;
using MacomberMapClient.User_Interfaces.NetworkMap;
using MacomberMapClient.User_Interfaces.Startup;
using MacomberMapClient.User_Interfaces.Summary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using SharpDX;
using SharpDX.Windows;
using Point = System.Drawing.Point;

namespace MacomberMapClient.User_Interfaces
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This is the main UI for the Macomber Map system
    /// </summary>
    public partial class MacomberMap_Form : MacomberMapClient.User_Interfaces.Generic.MM_Form
    {
        #region Variable declarations
        /// <summary>Whether the violations are currently being shown</summary>
        public bool ViolationsShowing = true;

        /// <summary>Our startup form</summary>
        private MM_Startup_Form Starter;

        /// <summary>Our hidden form for showing violations</summary>
        private Form ViolationForm;

        private System.Threading.Timer ReloadModelTimer;

        /// <summary>Our search results helper</summary>
        public MM_Search_Results_Helper ResultsHelper;
        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the network map
        /// </summary>        
        /// <param name="Coordinates">The coordinates to be used for the map</param>
        private void InitNetworkMap(MM_Coordinates Coordinates)
        {

            this.ctlNetworkMap = new MM_Network_Map_DX(Coordinates);
            this.ctlNetworkMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ctlNetworkMap.ForeColor = System.Drawing.Color.FromArgb(224, 224, 224);
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
           // ReloadModelTimer = new System.Threading.Timer(cb => { ReloadModel(); }, null, 60 * 1000, 2 * 60 * 1000);
        }

        private void ReloadModel()
        {
            MM_Server_Interface.LoadNetworkModel();
            Data_Integration.RestartModel(MM_Server_Interface.Client);
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
            splHorizontal.Panel1Collapsed = !ShowViolations || ViolationsInSeparateWindow;

            if (ViolationsInSeparateWindow)
            {
                if (ctlViolationViewer.Parent == splHorizontal.Panel1)
                    splHorizontal.Panel1.Controls.Remove(ctlViolationViewer);

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

                if (ctlViolationViewer.Parent != splHorizontal.Panel1)
                    splHorizontal.Panel1.Controls.Add(ctlViolationViewer);

                ctlViolationViewer.Dock = DockStyle.Fill;
            }
        }

        /// <summary>
        /// Initialize the Macomber Map
        /// </summary>
        /// <param name="Coordinates">The coordinates to be used in the map</param>
        /// <param name="Starter">Our startup form</param>
        public MacomberMap_Form(MM_Coordinates Coordinates, MM_Startup_Form Starter)
        {
            InitializeComponent();

            
            var size = ClientSize;
            this.Size = Screen.PrimaryScreen.WorkingArea.Size;
            if (this.Size.Width >= 1900)
                size = new Size(1900, this.Size.Height);
            if (this.Size.Height >= 1200)
                size = new Size(this.Size.Width, 1180);
            this.ClientSize = new Size(size.Width-10, size.Height-10);
            this.Size = ClientSize;

            this.Title = "Main Map";
            this.Starter = Starter;
            MM_System_Interfaces.LogError("MM: Initializing network map/coordinates");
            InitNetworkMap(Coordinates);

            if (!Data_Integration.Permissions.ShowViolations)
                SwitchMMView(false, false);


            //Assign our integration layer, add the thread and form to it

            Data_Integration.RunningForms.Add(this);
            Data_Integration.RunningThreads.Add(System.Threading.Thread.CurrentThread);
            System.Threading.Thread.Sleep(3000); // wait for model kv levels to load
            //Assign our controls
            MM_System_Interfaces.LogError("MM: Starting network map");
            ctlNetworkMap.SetControls(ctlViolationViewer, 0);
            MM_System_Interfaces.LogError("MM: Starting violation viewer");
            ctlViolationViewer.SetControls(ctlNetworkMap);
            MM_System_Interfaces.LogError("MM: Starting key indicators");

            ctlKeyIndicators.SetControls(ctlNetworkMap, ctlViolationViewer, MM_Repository.xConfiguration.DocumentElement["DisplayParameters"]["KeyIndicators"]);

            ResizeLeftSlider(this, null);

            //Set up our results helper
            ResultsHelper = new MM_Search_Results_Helper();
            ResultsHelper.Visible = false;
            Controls.Add(ResultsHelper);
            ResultsHelper.BringToFront();
            (this.FindForm() as MacomberMap_Form).ctlKeyIndicators.AssignResultsHelper(ResultsHelper);



            //Set up our main timer
            Program.MainFormChecker = new System.Threading.Timer(Program.ThreadChecker, System.Threading.Thread.CurrentThread, 15000, 15000);

            StartRender();

        }

        private async void StartRender()
        {
            var renderTimer = new Stopwatch();
            await Task.Delay(1000);
            RenderLoop.Run(this, () =>
            {
                if (ctlNetworkMap.Disposing || ctlNetworkMap.IsDisposed)
                    return;

                // if this window does not have focus, or we are zoomed out to not see flow arrows and we aren't interacting slow down rendering.
                bool slowMode = ((!ctlNetworkMap.HandlingMouse) && (ctlNetworkMap.Coordinates.ZoomLevel < MM_Repository.OverallDisplay.LineFlows)) || !this.ContainsFocus;
                renderTimer.Restart();
                ctlNetworkMap.RenderLoop();
                renderTimer.Stop();


                // calculate sleep time needed to hit target FPS. Max sleep time is 50ms, minimum is 4ms (~240fps).
                // minimum is in place to process windows events
                int sleepTime = (int)MathUtil.Clamp((1000f / MM_Repository.OverallDisplay.TargetFPS) - renderTimer.ElapsedMilliseconds, 4, 250f);
                Thread.Sleep(slowMode ? 150 : MM_Repository.OverallDisplay.TargetFPS > 0 ? sleepTime : 4);
            });
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
        /// Reset the network mapResetNetworkMap
        /// </summary>
        public void ResetNetworkMap()
        {
            ctlNetworkMap.ResetMap();
            ctlNetworkMap.SetControls(ctlViolationViewer, 0);
            //GC.Collect();

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
            //GC.Collect();
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


            //TODO: Bring back user interaction history
            /*   lock (Data_Integration.UserInteractionsToProcess)
                   foreach (KeyValuePair<String, List<TCP_Message>> kvp in Data_Integration.UserInteractionsToProcess)
                       MM_Server_Interface.Client.ProcessUserInteractions(kvp.Key, kvp.Value);*/
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