﻿using MacomberMapClient.Integration;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MacomberMapClient.User_Interfaces.Generic;

namespace MacomberMapClient.Data_Elements.Physical
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class handles the ties (overloads on lines)
    /// </summary>
    public class MM_Tie : MM_Line
    {
        #region Variable declarations
        /// <summary>The unit associated with this DCTie</summary>
        public MM_Unit Unit;

        /// <summary>The load associated with this DCTie</summary>
        public MM_Load Load;

        /// <summary>The descriptor of the tie</summary>
        public String TieDescriptor;

        public string TyLn;

        public string OPA;

        public string UTIL;

        public bool IsDC = false;


        /// <summary>Our integrated MW (from RTGEN)</summary>
        private float _MW_Integrated = float.NaN;

        /// <summary>
        /// Handle our integrated MW
        /// </summary>
        public float MW_Integrated
        {
            get { return _MW_Integrated; }
            set
            {
                // todo: move the tie summation to the Overall indicator code. We should be summing by tie corridor integrated MW.
                //First, remove our old value from our indicator
                lock (Data_Integration.OverallIndicators)
                {
                    //First, make sure our DCTie overall indicator is setup up to the value not counting our current
                    if (float.IsNaN(Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.DCTieOut]))
                        Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.DCTieOut] = Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.DCTieIn] = 0f;
                    else if (float.IsNaN(_MW_Integrated))
                    { }
                    else if (_MW_Integrated > 0f)
                        Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.DCTieOut] -= _MW_Integrated;
                    else
                        Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.DCTieIn] -= _MW_Integrated;

                    //Now, update the appropriate indicator
                    if (!float.IsNaN(value))
                        if (value > 0f)
                            Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.DCTieOut] += value;
                        else
                            Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.DCTieIn] += value;
                    _MW_Integrated = value;
                }
            }
        }

        /// <summary>Our monitored MW (from RTMONI)</summary>
        public float[] MW_Monitored = new float[] { float.NaN, float.NaN };

        /// <summary>The line associated with the DCTie</summary>
        public MM_Line AssociatedLine;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new DCTie instance
        /// </summary>
        public MM_Tie()
        {
        }

        /// <summary>
        /// Integrate the information from a base line into the DCTie.
        /// </summary>
        /// <param name="BaseLine"></param>
        public void IntegrateFromLine(MM_Line BaseLine)
        {
            try
            {
                //Pull all data in from the base line
                foreach (FieldInfo fI in BaseLine.GetType().GetFields(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public))
                    this.GetType().GetField(fI.Name).SetValue(this, fI.GetValue(BaseLine));

                //Check our line length. If it's less than 10 miles, expand the side closest to the border to make it wider
                float MileLength = 20f;
                if (this.Length < MileLength)
                {
                    float[] Dist = new float[] {float.NaN, float.NaN};
                    foreach (PointF PtToCheck in MM_Repository.Counties["STATE"].Coordinates)
                        for (int a = 0; a < 2; a++)
                            if (float.IsNaN(Dist[a]) || CalculateDistance(ConnectedStations[a].LngLat, PtToCheck) < Dist[a])
                                Dist[a] = CalculateDistance(ConnectedStations[a].LngLat, PtToCheck);

                    //Now, determine which one is closer, and keep moving it until we are at our minimum mile length.
                    int i = 1000;
                    while (this.Length < MileLength && i < 1000)
                    {
                        //Now, find the station with the DC tie elements

                        int TieStation = Math.Max(0, Array.IndexOf(ConnectedStations, this.Unit == null ? this.Substation : this.Unit.Substation));

                        //int ShorterStation = (Dist[0] < Dist[1] ? 0 : 1);
                        ConnectedStations[TieStation].LngLat.X += .01f * Math.Sign(ConnectedStations[TieStation].LngLat.X - ConnectedStations[1 - TieStation].LngLat.X);
                        ConnectedStations[TieStation].LngLat.Y += .01f * Math.Sign(ConnectedStations[TieStation].LngLat.Y - ConnectedStations[1 - TieStation].LngLat.Y);
                        i++;
                    }
                }
            } catch (Exception ex)
            {
                MM_System_Interfaces.LogError(ex);
            }
        }



        /// <summary>
        /// Initialize a new DCTie around its base line.
        /// </summary>
        /// <param name="BaseLine">The line on which the tie is based</param>
        /// <param name="TieDescriptor">The descriptor of the tie</param>
        public MM_Tie(MM_Line BaseLine, String TieDescriptor)
        {
            //Retrieve our information from our base line
            IntegrateFromLine(BaseLine);

            //Assign our tie descriptor and KV Level.
            this.TieDescriptor = MM_Repository.TitleCase(TieDescriptor);
            this.ElemType = MM_Repository.FindElementType("Tie");
            //this.KVLevel = MM_Repository.KVLevels["DC Tie"];     //mn - was at "DCTIE vs "DC TIE" check to ensure what one is right - 20130610 -mn                  
        }


        private void MoveOutside(MM_Substation Sub)
        {
            //Take our substation's lat/long, and find the closest state inflection point
            PointF ClosestPoint = PointF.Empty;
            float ClosestDistance = float.NaN;
            foreach (PointF PtToCheck in MM_Repository.Counties["STATE"].Coordinates)
                if (float.IsNaN(ClosestDistance) || CalculateDistance(ConnectedStations[0].LngLat, PtToCheck) < ClosestDistance)
                {
                    ClosestDistance = CalculateDistance(ConnectedStations[0].LngLat, PtToCheck);
                    ClosestPoint = PtToCheck;
                }

            //Now, move that substation to the opposite side of the state border
            Sub.LngLat = new PointF((2f * ClosestPoint.X) - ConnectedStations[0].LngLat.X, (2f * ClosestPoint.Y) - ConnectedStations[0].LngLat.Y);
        }


        /// <summary>
        /// Return the number of lines connecting a substation
        /// </summary>
        /// <param name="Sub">The substation to check</param>
        /// <returns></returns>        
        private int CountLines(MM_Substation Sub)
        {
            int Count = 0;
            foreach (MM_Line Line in MM_Repository.Lines.Values)
                if (Line.ConnectedStations[0] == Sub || Line.ConnectedStations[1] == Sub)
                    Count++;
            return Count;
        }

        /// <summary>
        /// Initialize a new CIM Transformer
        /// </summary>
        /// <param name="ElementSource">The XML source for this line</param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Tie(XmlElement ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
            this.ElemType = MM_Repository.FindElementType(ElementSource.Name);
        }




        /// <summary>
        /// Initialize a new DCTie instance
        /// </summary>
        /// <param name="ElementSource">The element source</param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Tie(DbDataReader ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
            this.ElemType = MM_Repository.FindElementType("Tie");
        }

        /// <summary>
        /// Determine the distance between two points
        /// </summary>
        /// <param name="Point1">The first point</param>
        /// <param name="Point2">The second point</param>
        /// <returns></returns>
        private float CalculateDistance(PointF Point1, PointF Point2)
        {
            float DeltaX = Point1.X - Point2.X;
            float DeltaY = Point1.Y - Point2.Y;
            return (float)Math.Sqrt((DeltaX * DeltaX) + (DeltaY * DeltaY));
        }
        #endregion

        /// <summary>
        /// Update the telemetry on a DCTie from RTMONI
        /// </summary>
        /// <param name="Import">Whether this update is an import</param>
        /// <param name="CurrentMW">The current MW</param>
        /// <param name="LimitMW">The maximum MW</param>       
        public void UpdateMonitored(bool Import, float CurrentMW, float LimitMW)
        {
            int Index = (Import ? 0 : 1);
            this.MW_Monitored[Index] = CurrentMW;
            this.Limits[Index] = LimitMW;
        }

        /// <summary>
        /// Update the telemetry on a DCTie from RTGEN
        /// </summary>
        /// <param name="CurrentMW">The current MW</param>
        public void UpdateGenerated(float CurrentMW)
        {
            this.MW_Integrated = CurrentMW;
        }

        /// <summary>
        /// Report the MVA flow direction for DC Ties (which is really MW direction)
        /// </summary>
        public override MM_Substation MVAFlowDirection
        {
            get
            {
                if (this.Unit == null || !this.IsDC)
                    return base.MVAFlowDirection;

                int TieStn = Array.IndexOf(ConnectedStations, this.Unit == null ? this.Substation : this.Unit.Substation);
                if (float.IsNaN(MW_Integrated) || MW_Integrated == 0f)
                    return base.MVAFlowDirection;
                else if (MW_Integrated > 0f)
                    return ConnectedStations[TieStn];
                else
                    return ConnectedStations[1 - TieStn];
            }
        }



        /// <summary>
        /// Report the MW flow direction for DC ties
        /// </summary>
        public override MM_Substation MWFlowDirection
        {
            get
            {
                if (this.Unit == null || !this.IsDC)
                    return base.MWFlowDirection;

                int TieStn = Array.IndexOf(ConnectedStations, this.Unit == null ? this.Substation : this.Unit.Substation);
                if (float.IsNaN(MW_Integrated) || MW_Integrated == 0f)
                    return base.MWFlowDirection;
                else if (MW_Integrated > 0f)
                    return ConnectedStations[TieStn];
                else
                    return ConnectedStations[1 - TieStn];
            }
        }

        /// <summary>
        /// Report the MVAR flow direction of a DC tie (uses MW instead)
        /// </summary>
        public override MM_Substation MVARFlowDirection
        {
            get
            {
                if (this.Unit == null || !this.IsDC)
                    return base.MVARFlowDirection;

                int TieStn = Array.IndexOf(ConnectedStations, this.Unit == null ? this.Substation : this.Unit.Substation);
                if (float.IsNaN(MW_Integrated) || MW_Integrated == 0f)
                    return base.MVARFlowDirection;
                else if (MW_Integrated > 0f)
                    return ConnectedStations[TieStn];
                else
                    return ConnectedStations[1 - TieStn];
            }
        }

    }
}
