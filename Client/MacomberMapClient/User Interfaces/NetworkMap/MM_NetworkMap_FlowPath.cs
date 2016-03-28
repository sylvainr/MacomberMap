using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Mathematics.Interop;

namespace MacomberMapClient.User_Interfaces.NetworkMap
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class contains all of the information to send a MW/MVAR flow arrow from one side of a line to another
    /// </summary>
    public class MM_NetworkMap_FlowPath
    {
        #region Variable Declarations
        /// <summary>The starting point</summary> 
        public RawVector2 StartPoint;

        /// <summary>The ending point</summary>
        public RawVector2 EndPoint;

        /// <summary>The delta to be carried out each step</summary>
        public RawVector2 Delta;

        /// <summary>The maximum number of points the line flow will traverse</summary>
        public int MaxDimension;

        /// <summary>The current dimension</summary>
        public int CurDimension = 0;

        /// <summary>The sinusoidal component of the flow line</summary>
        public float sint;

        /// <summary>The cosinusoidal component of the flow line</summary>
        public float cost;

        /// <summary>The angle of the line segment.</summary>
        public float theta;
        #endregion


        #region Initialization
        /// <summary>
        /// Initialize a new flow path
        /// </summary>
        /// <param name="Point1">The first part of the line</param>
        /// <param name="Point2">The second part of the line</param>        
        public MM_NetworkMap_FlowPath(RawVector2 Point1, RawVector2 Point2)
        {
            this.CurDimension = 0;
            RecomputeFlow(Point1, Point2);
        }

        /// <summary>
        /// Recompute the flow of a line
        /// </summary>
        /// <param name="Point1">The first part of the line</param>
        /// <param name="Point2">The second part of the line</param>                
        public void RecomputeFlow(RawVector2 Point1, RawVector2 Point2)
        {
            this.CurDimension = 0;
            this.StartPoint = Point1;
            this.EndPoint = Point2;
            //Determine our angles
            this.theta = (float)Math.Atan2(Point1.Y - Point2.Y, Point1.X - Point2.X);
            this.sint = (float)Math.Sin(theta);
            this.cost = (float)Math.Cos(theta);

            //Determine whether the X or Y coordinates are larger, and handle accordingly.
            this.Delta = new Vector2(Point2.X - Point1.X, Point2.Y - Point1.Y);

            //If X > Y, adjust accordingly.
            if (Math.Abs(Delta.X) > Math.Abs(Delta.Y))
            {
                //Determine how many points we'll be outputting, and adjust accordingly.
                this.MaxDimension = (int)Math.Ceiling(Math.Abs(Delta.X));

                //Adjust the deltas to support our aggregation
                Delta.Y = Delta.Y / Math.Abs(Delta.X);
                Delta.X = Math.Sign(Delta.X);
            }
            else
            //If X < Y, adjust accordingly
            {
                //Determine how many points we'll be outputting, and adjust accordingly.
                this.MaxDimension = (int)Math.Ceiling(Math.Abs(Delta.Y));

                //Adjust the deltas to support our aggregation
                Delta.X = Delta.X / Math.Abs(Delta.Y);
                Delta.Y = Math.Sign(Delta.Y);
            }
        }
        #endregion

        #region Point retrieval
        /// <summary>
        /// Retrieve the current flow path point
        /// </summary>
        public RawVector2 Current
        {
            get { return new RawVector2 { X = StartPoint.X + (Delta.X * (float)CurDimension), Y = StartPoint.Y + (Delta.Y * (float)CurDimension)}; }
        }
        #endregion
    }
}


