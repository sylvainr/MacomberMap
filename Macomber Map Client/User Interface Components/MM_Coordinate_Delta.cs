using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Macomber_Map.Data_Elements.Display;
using Macomber_Map.Data_Elements;
using Macomber_Map.User_Interface_Components.NetworkMap;
using System.Xml;
using System.IO;
using Macomber_Map.Data_Elements.Positional;

namespace Macomber_Map.User_Interface_Components
{
    /// <summary>
    /// (C) 2013, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc.
    /// This class displays a form for editing deltas
    /// </summary>
    public partial class MM_Coordinate_Delta : MM_Form
    {
        #region Variable declarations
        /// <summary>Our collection of suggestions</summary>
        public Dictionary<MM_Element, MM_Coordinate_Suggestion> Suggestions = new Dictionary<MM_Element,MM_Coordinate_Suggestion>();

        /// <summary>Our pointer to the network map</summary>
        public Network_Map nMap;

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
        public MM_Coordinate_Delta(Network_Map nMap)
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
            dgvLineLatLong.DataSource = LineCoordinates;
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
                                        UpdatedCoordinates.Insert(a, MM_Coordinates.XYToLatLng(PixelLocation, Coordinates.ZoomLevel));
                                        (SelectedElement as MM_Line).Coordinates = FoundSuggestion.SuggestedCoordinates = UpdatedCoordinates.ToArray();

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
                dgvLineLatLong.Visible = false;
            }
            else
            {
                MM_Line BaseLine = SelectedElement as MM_Line;
                txtSubLatitude.Visible = txtSubLongitude.Visible = false;
                dgvLineLatLong.Visible = true;
                LineCoordinates.Rows.Clear();
                for (int a = 0; a < BaseLine.Coordinates.Length; a++)
                    LineCoordinates.Rows.Add(a, BaseLine.Coordinates[a].Y, BaseLine.Coordinates[a].X);
            }


        }

        /// <summary>
        /// Locate a substation's county by its latitude/longitude
        /// </summary>
        /// <param name="LatLng"></param>
        /// <returns></returns>
        public MM_Boundary LookupCounty(PointF LatLng)
        {
            foreach (MM_Boundary Bound in MM_Repository.Counties.Values)
                if (Bound.Name != "STATE" && LatLng.X >= Bound.Min.X && LatLng.Y >= Bound.Min.Y && LatLng.X <= Bound.Max.X && LatLng.Y <= Bound.Max.Y && Bound.HitTest(LatLng))
                    return Bound;
            return null;
        }

        /// <summary>
        /// Determine the angle between two lat/lng points
        /// </summary>
        /// <param name="LatLng1"></param>
        /// <param name="LatLng2"></param>
        /// <returns></returns>
        private Double ComputeAngle(PointF LatLng1, PointF LatLng2)
        {
            Single DeltaLong = LatLng2.X - LatLng1.X;
            return Math.Atan2(Math.Sin(DeltaLong) * Math.Cos(LatLng2.Y), (Math.Cos(LatLng1.X) * Math.Sin(LatLng2.Y)) - (Math.Sin(LatLng1.Y) * Math.Cos(LatLng2.Y) * Math.Cos(DeltaLong)));
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
            using (XmlTextWriter xW = new XmlTextWriter(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "MacomberMapCoordinateSuggestions-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".xml"), new UTF8Encoding(false)))
            {
                xW.Formatting = Formatting.Indented;
                xW.WriteStartDocument();
                xW.WriteStartElement("Coordinate_Suggestions");
                xW.WriteAttributeString("Exported_On", XmlConvert.ToString(DateTime.Now, XmlDateTimeSerializationMode.Unspecified));
                xW.WriteAttributeString("CollectedBy", Environment.UserName);
                xW.WriteAttributeString("System", Environment.MachineName);
                foreach (KeyValuePair<MM_Element, MM_Coordinate_Suggestion> kvp in Suggestions)
                {
                    xW.WriteStartElement(kvp.Key is MM_Substation ? "Substation" : "Line");
                    xW.WriteAttributeString("ElemType", kvp.Key.ElemType.Name);
                    xW.WriteAttributeString("TEID", kvp.Key.TEID.ToString());
                    if (kvp.Key is MM_Substation)
                    {
                        MM_Substation Sub = (MM_Substation)kvp.Key;
                        xW.WriteAttributeString("LatLong", Sub.LatLong.X.ToString() + "," + Sub.LatLong.Y.ToString());
                        xW.WriteAttributeString("Latitude", Sub.Latitude.ToString());
                        xW.WriteAttributeString("Longitude", Sub.Longitude.ToString());
                        xW.WriteAttributeString("County", Sub.County.Name);                        
                    }
                    else
                    {
                        MM_Line Line = (MM_Line)kvp.Key;
                        StringBuilder sB = new StringBuilder();
                        foreach (PointF pt in Line.Coordinates)
                            sB.Append((sB.Length == 0 ? "" : ",") + pt.X.ToString() + "," + pt.Y.ToString());
                        xW.WriteAttributeString("Coordinates", sB.ToString());                        
                    }

                    xW.WriteEndElement();
                }
                xW.WriteEndElement();
                xW.WriteEndDocument();
            }

        }

        /// <summary>
        /// Close out our form, keeping all requested changes visible
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Hide();
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
                    (kvp.Key as MM_Substation).LatLong = kvp.Value.OriginalCoordinates[0];
                    ElementsToUpdate.Add(kvp.Key );
                    foreach (MM_Line Line in MM_Repository.Lines.Values)
                        if (Line.Substation1 == kvp.Key || Line.Substation2 == kvp.Key)
                            ElementsToUpdate.Add(Line);
                }
                else if (kvp.Key is MM_Line)
                {
                    (kvp.Key as MM_Line).Coordinates = kvp.Value.OriginalCoordinates;
                    ElementsToUpdate.Add(kvp.Key);
                }
               
            //Now, tell the main map to recompute the appropirate points
            foreach (MM_Element Elem in ElementsToUpdate)
                if (Elem is MM_Substation)
                    nMap.UpdateSubstationInformation((MM_Substation)Elem);
                else if (Elem is MM_Line)
                    nMap.UpdateLineInformation((MM_Line)Elem, false);
        }

        /// <summary>
        /// Override the closing of our form to hide it
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
        #endregion
    }
}
