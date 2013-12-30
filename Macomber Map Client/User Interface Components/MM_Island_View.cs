using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Macomber_Map.User_Interface_Components.NetworkMap;
using Macomber_Map.Data_Elements;
using System.Threading;

namespace Macomber_Map.User_Interface_Components
{
    /// <summary>
    /// (C) 2012, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc.
    /// This class provides a view into our collection of islands
    /// </summary>
    public partial class MM_Island_View : MM_Form
    {
        /// <summary>Our main network map</summary>
        private Network_Map MainMap;

        /// <summary>Our collection of islands</summary>
        private Dictionary<MM_Island, Mini_Map> IslandMaps = new Dictionary<MM_Island, Mini_Map>(); //should this be sorted dictionary - nataros merge question

        /// <summary>The thread for updating island information</summary>
        private Thread IslandUpdater;


        /// <summary>
        /// Initialize our island view
        /// </summary>
        /// <param name="MainMap"></param>
        public MM_Island_View(Network_Map MainMap)
        {
            InitializeComponent();
            this.MainMap = MainMap;
            this.Text = "Island Summary - Macomber Map®";
            IslandUpdater = new Thread(UpdateIsland);
            IslandUpdater.Name = "Island updater";
            IslandUpdater.IsBackground = true;
            IslandUpdater.Start();
        }

        /// <summary>
        /// Update our island information
        /// </summary>
        /// <param name="state"></param>
        private void UpdateIsland(object state)
        {
            //Ensure our islands are set up
            Mini_Map FoundIsland;
            while (true)
            {
                try
                {
                    //Remove any islands that don't belong anymore
                    foreach (KeyValuePair<MM_Island, Mini_Map> kvp in IslandMaps)
                        if (!MM_Repository.Islands.ContainsValue(kvp.Key))
                        {
                            RemoveMiniMap(kvp.Value);
                            IslandMaps.Remove(kvp.Key);
                        }
                    //remove elements in islands to rebuild
                    foreach (MM_Island Island in MM_Repository.Islands.Values)
                    {
                        if (IslandMaps.TryGetValue(Island, out FoundIsland))
                        {
                            FoundIsland.BaseData.Clear();
                        }
                    }

                    lock (MM_Repository.Islands)
                    {
                        //Go through the ones that do, and update as appropriate.
                        foreach (MM_Island Island in MM_Repository.Islands.Values)
                        {
                            //Create our new island if appropriate
                            if (!IslandMaps.TryGetValue(Island, out FoundIsland))
                                IslandMaps.Add(Island, FoundIsland = GenerateMap(Island));

                            //Ensure our lines are added
                            foreach (MM_Line Line in MM_Repository.Lines.Values)
                                if (Line.Permitted && Line.Bus != null && Line.Bus.IslandNumber == Island.ID)
                                    FoundIsland.BaseData.AddDataElement(Line);



                    //Run through and ensure our substations, lines, loads, units, caps/reacs are in our list
                    PointF StartPoint = new PointF(float.NaN, float.NaN);
                    PointF EndPoint = new PointF(float.NaN, float.NaN);
                    //Go through our data elements that are in our island            
                    foreach (MM_Substation Sub in MM_Repository.Substations.Values)
                        if (Sub.Permitted)
                        {
                            bool Include = false;

                            //Check through our buses, to determine whether the substation is added
                            if (Sub.BusbarSections != null)
                                foreach (MM_BusbarSection Bus in Sub.BusbarSections)
                                    if (Bus.IslandNumber == Island.ID)
                                    {
                                        Include = true;
                                        FoundIsland.BaseData.AddDataElement(Bus);
                                    }

                            //Check through our loads, to determine whether any of the loads should be included
                            if (Include && Sub.Loads != null)
                                foreach (MM_Load Load in Sub.Loads)
                                    if (Load.Bus != null && Load.Bus.IslandNumber == Island.ID)                                        
                                        FoundIsland.BaseData.AddDataElement(Load);

                            //Check through our units, to determine whether any of the loads should be included
                            if (Include && Sub.Units != null)
                                foreach (MM_Unit Unit in Sub.Units)
                                    if (Unit.Bus != null && Unit.Bus.IslandNumber == Island.ID)
                                        FoundIsland.BaseData.AddDataElement(Unit);

                            //Check through our reactive devices, to determine whether any of the loads should be included
                            if (Include && Sub.ShuntCompensators != null)
                                foreach (MM_ShuntCompensator SC in Sub.ShuntCompensators)
                                    if (SC.Bus != null && SC.Bus.IslandNumber == Island.ID)
                                        FoundIsland.BaseData.AddDataElement(SC);

                            //Check through our transformers, to determine whether any of the loads should be included
                            if (Include && Sub.Transformers != null)
                                foreach (MM_Transformer XF in Sub.Transformers)
                                    if (XF.Bus != null && XF.Bus.IslandNumber == Island.ID)
                                        FoundIsland.BaseData.AddDataElement(XF);
                            

                            if (Include)
                            {
                                if (float.IsNaN(StartPoint.X) || Sub.Longitude < StartPoint.X)
                                    StartPoint.X = Sub.Longitude;
                                if (float.IsNaN(StartPoint.Y) || Sub.Latitude < StartPoint.Y)
                                    StartPoint.Y = Sub.Latitude;
                                if (float.IsNaN(EndPoint.X) || Sub.Longitude > EndPoint.X)
                                    EndPoint.X = Sub.Longitude;
                                if (float.IsNaN(EndPoint.Y) || Sub.Latitude > EndPoint.Y)
                                    EndPoint.Y = Sub.Latitude;
                            }

                        }
                   

                            //Update the bounds based on our components
                            FoundIsland.TopLeft = StartPoint;
                            FoundIsland.BottomRight = EndPoint;
                        }
                    }
                    Thread.Sleep(10000);
                }
                catch (Exception)
                {
                }
            }
        }

        private delegate Mini_Map SafeGenerateMap(MM_Island Island);

        /// <summary>
        /// Generate a new map
        /// </summary>
        /// <param name="Island"></param>
        /// <returns></returns>
        private Mini_Map GenerateMap(MM_Island Island)
        {
            if (InvokeRequired)
                return Invoke(new SafeGenerateMap(GenerateMap), Island) as Mini_Map;
            else
            {
                Mini_Map OutMap = new Mini_Map(false);
                OutMap.ShowRegionalView = false;
                OutMap.Island = Island;
                DataSet_Base BaseData = new DataSet_Base("Island " + Island.ID.ToString());
                BaseData.BaseElement = Island;
                OutMap.SetControls(MainMap, MainMap.violViewer, BaseData, new MM_Line[0], PointF.Empty, PointF.Empty);
                pnlMain.Controls.Add(OutMap);
                return OutMap;
            }
        }

        private delegate void SafeRemoveMiniMap(Mini_Map MapToRemove);

        /// <summary>
        /// Remove a mini-map control
        /// </summary>
        /// <param name="MapToRemove"></param>
        private void RemoveMiniMap(Mini_Map MapToRemove)
        {
            if (InvokeRequired)
                Invoke(new SafeRemoveMiniMap(RemoveMiniMap), MapToRemove);
            else            
                pnlMain.Controls.Remove(MapToRemove);
                
        }

        /// <summary>
        /// When our timer kicks off, make sure everything shows up/is refreshed        
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmrUpdate_Tick(object sender, EventArgs e)
        {
            Mini_Map FoundMap;
            foreach (MM_Island Island in MM_Repository.Islands.Values)
                if (IslandMaps.TryGetValue(Island, out FoundMap))
                    FoundMap.Refresh();
        }



        /// <summary>
        /// Handle the closing of the form (to hide instead of shut down)
        /// </summary>
        /// <param name="e"></param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Visible = false;
            }

        }

        /// <summary>
        /// Go through our collection of mini-maps, and update the sizes as appropriate
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSizeChanged(EventArgs e)
        {
            this.AutoSize = true; //Ensure that this is true for the next series, it was not staying true, so forced it here, will get better fix later
            int LastTop = 0; //This wont work unless you are resizing parent container to fit contents as well, else you are just streaching parent contents with fixed value
            foreach (Mini_Map Map in IslandMaps.Values)
            {
                Map.Top = LastTop;
                Map.Width = Map.Height = (int)(this.Width - 30);//pnlMain.ClientRectangle.Width;
                LastTop = Map.Bottom;
            }
            base.OnSizeChanged(e);
        }

        /// <summary>
        /// When display becomes visiable ensure that size is proper, otherwise, 
        /// screen calls while not visible will not repost properly as we never destroy this window,
        /// only hide it when not wanted.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MM_Island_View_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible == true)
            {
                int LastTop = 0;
                //double ToFit = 0.12; //Please do not delete, this is for a reference for scale
                //int ToFit = 30; //Please do not delete, this is for a reference for scale

                foreach (Mini_Map Map in IslandMaps.Values)
                {
                    Map.Top = LastTop;
                    Map.Width = Map.Height = (int)(this.Width - 30);//Finds width and then accounts for scroll bar (30)
                    LastTop = Map.Bottom;
                }
                base.OnSizeChanged(e);
            }

        }

    }
}
