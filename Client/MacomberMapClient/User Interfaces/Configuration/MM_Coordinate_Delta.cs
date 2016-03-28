using MacomberMapClient.Data_Elements.Geographic;
using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Integration;
using MacomberMapClient.User_Interfaces.Generic;
using MacomberMapClient.User_Interfaces.NetworkMap;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using MacomberMapClient.Data_Elements.Display;

namespace MacomberMapClient.User_Interfaces.Configuration
{
    /// <summary>
    /// (C) 2013, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc.
    /// This class displays a form for editing deltas
    /// </summary>
    public partial class MM_Coordinate_Delta : MM_Form
    {
        #region Variable declarations
        /// <summary>Our collection of suggestions</summary>
        public Dictionary<MM_Element, MM_Coordinate_Suggestion> Suggestions = new Dictionary<MM_Element, MM_Coordinate_Suggestion>();

        /// <summary>Our pointer to the network map</summary>
        public MM_Network_Map_DX nMap;

        /// <summary>Our list of active changes</summary>
        public DataTable ActiveChanges;

        /// <summary>Our collection of line coordinates</summary>
        public DataTable LineCoordinates;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize our coordinate delta viewer
        /// </summary>
        /// <param name="nMap"></param>
        public MM_Coordinate_Delta(MM_Network_Map_DX nMap)
        {
            InitializeComponent();
            this.nMap = nMap;
            ActiveChanges = new DataTable("ActiveChanges");
            ActiveChanges.Columns.Add("ElemType", typeof(MM_Element_Type));
            ActiveChanges.Columns.Add("Name", typeof(string));
            ActiveChanges.Columns.Add("TEID", typeof(Int32));
            dgvChanges.DataSource = ActiveChanges;

            LineCoordinates = new DataTable("LineCoordinates");
            LineCoordinates.Columns.Add("Index", typeof(int));
            LineCoordinates.Columns.Add("Latitude", typeof(Single));
            LineCoordinates.Columns.Add("Longitude", typeof(Single));
            dgvLineLngLat.DataSource = LineCoordinates;
        }
        #endregion

        #region Element selection handling
        /// <summary>
        /// Hanlde our element selection on our mouse down. If no element has been selected, just make sure the element is highlighted.
        /// </summary>
        /// <param name="SelectedElement"></param>
        /// <param name="Coordinates"></param>
        /// <param name="PixelLocation"></param>
        /// <param name="HighlightedElement"></param>
        public MM_Coordinate_Suggestion HandleElementSelection(MM_Element SelectedElement, MM_Coordinates Coordinates, Point PixelLocation, ref MM_Element HighlightedElement)
        {
            MM_Coordinate_Suggestion FoundSuggestion = null;

            //Update the status of our element
            UpdateElement(SelectedElement);

            //Now, if our selected element is also highlighted, let's activate it
            if (HighlightedElement == SelectedElement)
            {
                if (!Suggestions.TryGetValue(SelectedElement, out FoundSuggestion))
                {
                    Suggestions.Add(SelectedElement, FoundSuggestion = new MM_Coordinate_Suggestion(SelectedElement, Coordinates));
                    ActiveChanges.Rows.Add(SelectedElement.ElemType, SelectedElement.Name, SelectedElement.TEID);
                }
                //If we're a line, determine how we should handle things based on our pixel location

                if (SelectedElement is MM_Line)
                {
                    int HitThreshold = 3;
                    FoundSuggestion.LineIndex = -1;

                    //First, go through and check to see if we have a close match
                    for (int a = 0; a < FoundSuggestion.SuggestedCoordinatesXY.Length; a++)
                        if (Math.Abs(FoundSuggestion.SuggestedCoordinatesXY[a].X - PixelLocation.X) < HitThreshold && Math.Abs(FoundSuggestion.SuggestedCoordinatesXY[a].Y - PixelLocation.Y) < HitThreshold)
                            FoundSuggestion.LineIndex = a;

                    //If not, continue looking in between our points                    
                    if (FoundSuggestion.LineIndex == -1)
                        for (int a = 1; a < FoundSuggestion.SuggestedCoordinatesXY.Length; a++)
                        {
                            Point Pt1 = FoundSuggestion.SuggestedCoordinatesXY[a - 1];
                            Point Pt2 = FoundSuggestion.SuggestedCoordinatesXY[a];
                            float Slope = (Single)(Pt1.Y - Pt2.Y) / (Single)(Pt1.X - Pt2.X);
                            float YInt = Pt1.Y - (Slope * Pt1.X);
                            float TestY = (Slope * PixelLocation.X) + YInt;
                            if (Math.Abs(TestY - PixelLocation.Y) < HitThreshold)
                                if ((PixelLocation.X >= Pt1.X && PixelLocation.X <= Pt2.X) || (PixelLocation.X <= Pt1.X && PixelLocation.X >= Pt2.X))
                                    if ((PixelLocation.Y >= Pt1.Y && PixelLocation.Y <= Pt2.Y) || (PixelLocation.Y <= Pt1.Y && PixelLocation.Y >= Pt2.Y))
                                    {
                                        List<PointF> UpdatedCoordinates = new List<PointF>(FoundSuggestion.SuggestedCoordinates);
                                        UpdatedCoordinates.Insert(a, MM_Coordinates.XYToLngLat(PixelLocation, Coordinates.ZoomLevel));
                                        (SelectedElement as MM_Line).Coordinates = FoundSuggestion.SuggestedCoordinates.ToList();
                                        FoundSuggestion.SuggestedCoordinates = UpdatedCoordinates.ToArray();

                                        List<Point> UpdatedCoordinateXY = new List<Point>(FoundSuggestion.SuggestedCoordinatesXY);
                                        UpdatedCoordinateXY.Insert(a, PixelLocation);
                                        FoundSuggestion.SuggestedCoordinatesXY = UpdatedCoordinateXY.ToArray();
                                        FoundSuggestion.LineIndex = a;
                                        break;
                                    }
                        }
                }
            }

            HighlightedElement = SelectedElement;
            return FoundSuggestion;
        }


        /// <summary>
        /// Update the status of our element
        /// </summary>
        /// <param name="SelectedElement"></param>
        public void UpdateElement(MM_Element SelectedElement)
        {
            //Show our element in our selection list
            lblElementValue.Text = SelectedElement.ToString();
            if (SelectedElement is MM_Substation)
            {
                MM_Substation BaseSubstation = SelectedElement as MM_Substation;
                txtSubLatitude.Visible = txtSubLongitude.Visible = true;
                txtSubLongitude.Text = BaseSubstation.Longitude.ToString();
                txtSubLatitude.Text = BaseSubstation.Latitude.ToString();
                dgvLineLngLat.Visible = false;
            }
            else
            {
                MM_Line BaseLine = SelectedElement as MM_Line;
                txtSubLatitude.Visible = txtSubLongitude.Visible = false;
                dgvLineLngLat.Visible = true;
                LineCoordinates.Rows.Clear();
                for (int a = 0; a < BaseLine.Coordinates.Count; a++)
                    LineCoordinates.Rows.Add(a, BaseLine.Coordinates[a].Y, BaseLine.Coordinates[a].X);
            }


        }

        /// <summary>
        /// Locate a substation's county by its latitude/longitude
        /// </summary>
        /// <param name="LngLat"></param>
        /// <returns></returns>
        public MM_Boundary LookupCounty(PointF LngLat)
        {
            foreach (MM_Boundary Bound in MM_Repository.Counties.Values)
                if (Bound.Name != "STATE" && LngLat.X >= Bound.Min.X && LngLat.Y >= Bound.Min.Y && LngLat.X <= Bound.Max.X && LngLat.Y <= Bound.Max.Y && Bound.HitTest(LngLat))
                    return Bound;
            return null;
        }

        /// <summary>
        /// Determine the angle between two lat/lng points
        /// </summary>
        /// <param name="LngLat1"></param>
        /// <param name="LngLat2"></param>
        /// <returns></returns>
        private Double ComputeAngle(PointF LngLat1, PointF LngLat2)
        {
            Single DeltaLong = LngLat2.X - LngLat1.X;
            return Math.Atan2(Math.Sin(DeltaLong) * Math.Cos(LngLat2.Y), (Math.Cos(LngLat1.X) * Math.Sin(LngLat2.Y)) - (Math.Sin(LngLat1.Y) * Math.Cos(LngLat2.Y) * Math.Cos(DeltaLong)));
        }
        #endregion

        #region Button handling
        /// <summary>
        /// Send out an email with our requested changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEmail_Click(object sender, EventArgs e)
        {
            try
            {
                List<MacomberMapCommunications.Messages.Communications.MM_Coordinate_Suggestion> OutSuggestions = new List<MacomberMapCommunications.Messages.Communications.MM_Coordinate_Suggestion>();
                foreach (MM_Coordinate_Suggestion Suggestion in Suggestions.Values)
                    OutSuggestions.Add(Suggestion.Message);
                MM_Server_Interface.Client.PostCoordinateSuggestions(OutSuggestions.ToArray());
                MessageBox.Show("Sent coordinate updates. Thank you.", Application.ProductVersion, MessageBoxButtons.OK, MessageBoxIcon.Information);
                ActiveChanges.Clear();
                Suggestions.Clear();
                LineCoordinates.Clear();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error posting coordinate suggestions:" + ex.ToString(), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Close out our form, keeping all requested changes visible
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Handle our cancel button, restoring all values
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            List<MM_Element> ElementsToUpdate = new List<MM_Element>();
            foreach (KeyValuePair<MM_Element, MM_Coordinate_Suggestion> kvp in Suggestions)
                if (kvp.Key is MM_Substation)
                {
                    (kvp.Key as MM_Substation).LngLat = kvp.Value.OriginalCoordinates[0];
                    ElementsToUpdate.Add(kvp.Key);
                    foreach (MM_Line Line in MM_Repository.Lines.Values)
                        if (Line.Substation1 == kvp.Key || Line.Substation2 == kvp.Key)
                            ElementsToUpdate.Add(Line);
                }
                else if (kvp.Key is MM_Line)
                {
                    (kvp.Key as MM_Line).Coordinates = kvp.Value.OriginalCoordinates.ToList();
                    ElementsToUpdate.Add(kvp.Key);
                }

            //Now, tell the main map to recompute the appropirate points
            foreach (MM_Element Elem in ElementsToUpdate)
                if (Elem is MM_Substation)
                    nMap.IsDirty = true;
                else if (Elem is MM_Line)
                    nMap.IsDirty = true;
            ActiveChanges.Clear();
            Suggestions.Clear();
            LineCoordinates.Clear();
        }

        /// <summary>
        /// Override the closing of our form to hide it
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            ActiveChanges.Clear();
            Suggestions.Clear();
            LineCoordinates.Clear();
            this.Hide();
        }
        #endregion
    }
}
