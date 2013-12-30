using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
namespace Macomber_Map.User_Interface_Components.NetworkMap.Generic
{   
    /// <summary>
    /// This class contains all of the information to send a MW/MVAR flow arrow from one side of a line to another
    /// </summary>
    public class MM_NetworkMap_FlowPath
    {
        #region Variable Declarations
        /// <summary>The starting point</summary> 
        public PointF StartPoint;

        /// <summary>The ending point</summary>
        public PointF EndPoint;

        /// <summary>The delta to be carried out each step</summary>
        public PointF Delta;

        /// <summary>The maximum number of points the line flow will traverse</summary>
        public int MaxDimension;

        /// <summary>The current dimension</summary>
        public int CurDimension = 0;

        /// <summary>The sinusoidal component of the flow line</summary>
        public float sint;

        /// <summary>The cosinusoidal component of the flow line</summary>
        public float cost;
        #endregion


        #region Initialization
        /// <summary>
        /// Initialize a new flow path
        /// </summary>
        /// <param name="Point1">The first part of the line</param>
        /// <param name="Point2">The second part of the line</param>        
        public MM_NetworkMap_FlowPath(Point Point1, Point Point2)
        {
            this.CurDimension = 0;
            RecomputeFlow(Point1, Point2);
        }

        /// <summary>
        /// Recompute the flow of a line
        /// </summary>
        /// <param name="Point1">The first part of the line</param>
        /// <param name="Point2">The second part of the line</param>                
        public void RecomputeFlow(Point Point1, Point Point2)
        {           
            this.CurDimension = 0;
            this.StartPoint = Point1;
            this.EndPoint = Point2;
            //Determine our angles
            float theta = (float)Math.Atan2(Point1.Y - Point2.Y, Point1.X - Point2.X);
            this.sint = (float)Math.Sin(theta);
            this.cost = (float)Math.Cos(theta);

            //Determine whether the X or Y coordinates are larger, and handle accordingly.
            this.Delta = new PointF(Point2.X - Point1.X, Point2.Y - Point1.Y);

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
        public PointF Current
        {
            get { return new PointF(StartPoint.X + (Delta.X * (float)CurDimension), StartPoint.Y + (Delta.Y * (float)CurDimension)); }
        }
        #endregion
    }
}
   

