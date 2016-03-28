using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MacomberMapClient.Data_Elements.Geographic;
using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.User_Interfaces.NetworkMap.DX;
using SharpDX;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using RectangleF = SharpDX.RectangleF;

namespace MacomberMapClient.User_Interfaces.NetworkMap.Proxies
{
    public class LineSegment
    {
        /// <summary>The starting point</summary> 
        public RawVector2 Start;

        /// <summary>The ending point</summary>
        public RawVector2 End;

        /// <summary>The sinusoidal component of the flow line</summary>
        public float sint;

        /// <summary>The cosinusoidal component of the flow line</summary>
        public float cost;

        /// <summary>The angle of the line segment.</summary>
        public float theta;

        /// <summary> The length of the segment. </summary>
        public float length;

        public LineSegment(RawVector2 start, RawVector2 end)
        {
            var dx = end.X - start.X;
            var dy = end.Y - start.Y;

            Start = start;
            End = end;
            //Determine our angles
            theta = (float)Math.Atan2(dy, dx);
            sint = (float)Math.Sin(theta);
            cost = (float)Math.Cos(theta);

            length = (float)Math.Sqrt(dx * dx + dy * dy);

        }

        public RawVector2 GetPositionDistance(float distance, out float angle)
        {
            angle = theta;
            return GetSegmentPoint(Start, End, distance / length);
        }

        public RawVector2 GetPosition(float percentage, out float angle)
        {
            angle = theta;
            return GetSegmentPoint(Start, End, percentage);
        }

        public static RawVector2 GetSegmentPoint(RawVector2 start, RawVector2 end, float percentage)
        {
            return new RawVector2
            {
                X = start.X + (end.X -start.X) * percentage,
                Y = start.Y + (end.Y -start.Y) * percentage
            };
        }
    }

    public class LineProxy
    {
        public float Angle;
        public bool FlipSides;
        public RectangleF Bounds;
        public RawVector2 Center;
        public MM_Line BaseLine;
        public List<RawVector2> Coordinates;

        public string NameText;
        public TextLayout NameTextLayout;

        public string FlowgateText;
        public TextLayout FlowgateTextLayout;
        public bool Visible;
        private List<LineSegment> Segments;
        public float Length;
        public float FlowPositionPercentage;
        public float FlowVarPositionPercentage;
        public bool BlackStartHidden;
        public bool BlackStartDim;

        /// <summary>
        /// Test if a point is in the bounding rectangle of the line.
        /// </summary>
        /// <param name="point">Test point.</param>
        /// <returns>True if in bounds, otherwise false.</returns>
        public bool InBounds(RawVector2 point)
        {
            if (Bounds.IsEmpty) return false;

            return Bounds.Contains(point);
        }

        /// <summary>
        /// Test if a point is in the bounding rectangle of the line.
        /// </summary>
        /// <param name="rectangle">Test area.</param>
        /// <returns>True if in bounds, otherwise false.</returns>
        public bool InBounds(RectangleF rectangle)
        {
            if (Bounds.IsEmpty) return false;

            return ((RawRectangleF)Bounds).Overlaps(rectangle);
        }

        /// <summary>
        /// Test if a point touches a the line.
        /// </summary>
        /// <param name="point">Test point.</param>
        /// <param name="width">Line width for testing.</param>
        /// <returns>True if touching, otherwise false.</returns>
        public bool HitTest(RawVector2 point, float width)
        {
            return HitTestLine(point, width, Bounds, this.Coordinates);
        }

        public void CalculateCoords(MM_Line line, int zoomLevel)
        {
            LineProxy.CalculateCoords(this, line, zoomLevel);
        }

        public RawVector2 GetPosition(float percentage, out float angle)
        {
            return GetPositionDistance(percentage * Length, out angle);
        }

        public RawVector2 GetPositionDistance(float distance, out float angle)
        {
            angle = 0;
            if (Coordinates == null || Coordinates.Count == 0)
                return Vector2.Zero;

            if (Coordinates.Count == 1)
                return Coordinates[0];

            angle = Segments[0].theta;
            if (distance == 0)
                return Coordinates[0];
            

            if (distance >= Length)
            {
                return Coordinates[Coordinates.Count - 1];
            }

            float traversed = 0;
            for (int i = 0; i < Segments.Count; i++)
            {
                var traverstedPlusSegment = traversed + Segments[i].length;
                if (distance <= traverstedPlusSegment)
                {
                    return Segments[i].GetPositionDistance(distance - traversed, out angle);
                }

                traversed += Segments[i].length;
            }

            angle = Segments[Segments.Count - 1].theta;
            return Coordinates[Coordinates.Count - 1];
        }




        public static void CalculateCoords(LineProxy proxy, MM_Line line, int zoomLevel)
        {
            proxy.Coordinates.Clear();
            //Ignore invalid lines
            if (float.IsNaN(line.CenterLngLat.X))
                return;

            // flip the line coords
            if (line.Coordinates.Count > 0 && (line.Substation2.LngLat == line.Coordinates[0] || line.Substation1.LngLat == line.Coordinates[line.Coordinates.Count - 1]))
                line.Coordinates.Reverse();

            if (line.Coordinates.Count == 0)
            {
                line.Coordinates.Add(line.Substation1.LngLat);
                line.Coordinates.Add(line.Substation2.LngLat);
            }
            //Determine if we need to flip coordinates because the substations have changed
            if (line.Coordinates.Count > 0 && (line.Substation2.LngLat == line.Coordinates[0] || line.Substation1.LngLat == line.Coordinates[line.Coordinates.Count - 1]))
                line.Coordinates.Reverse();

            Vector2 lastPoint = Vector2.Zero;
            Vector2 aggPoint = Vector2.Zero;

            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;
            float maxX = float.MinValue;


            float length = 0;
            if (proxy.Segments == null)
                proxy.Segments = new List<LineSegment>();
            else
                proxy.Segments.Clear();
            if (line.Coordinates.Count > 0)
            {
                for (int i = 0; i < line.Coordinates.Count; i++)
                {
                    PointF Pt = line.Coordinates[i];
                    Vector2 currentPoint = MM_Coordinates.LngLatToScreenVector2(Pt, zoomLevel);
                    if (lastPoint.IsZero || lastPoint.X != currentPoint.X || lastPoint.Y != currentPoint.Y)
                    {
                        aggPoint.X += currentPoint.X;
                        aggPoint.Y += currentPoint.Y;
                    }
                    proxy.Coordinates.Add(currentPoint);


                    if (currentPoint.X < minX) minX = currentPoint.X;
                    if (currentPoint.X > maxX) maxX = currentPoint.X;

                    if (currentPoint.Y < minY) minY = currentPoint.Y;
                    if (currentPoint.Y > maxY) maxY = currentPoint.Y;

                    if (i > 0)
                    {
                        var segment = new LineSegment(lastPoint, currentPoint);
                        proxy.Segments.Add(segment);
                        length += segment.length;
                    }

                    lastPoint = currentPoint;
                }
            }
            proxy.Length = length;
            var bounds = new SharpDX.RectangleF(minX, minY, maxX - minX, maxY - minY);

            proxy.Bounds = bounds;

            lastPoint = proxy.Coordinates[proxy.Coordinates.Count - 1];
            proxy.Center = new Vector2(aggPoint.X / (float)proxy.Coordinates.Count, aggPoint.Y / (float)proxy.Coordinates.Count);

            var lineAngle = (float)Math.Atan2(proxy.Coordinates[0].Y - lastPoint.Y, proxy.Coordinates[0].X - lastPoint.X);

            // if (lineAngle < 0)
            // {
            //     lineAngle += (float)(Math.PI * 2);
            // }
            if (lineAngle > (float)Math.PI / 2f)
            {
                lineAngle = (float)lineAngle - (float)Math.PI;
                proxy.FlipSides = true;
            }
            else if (lineAngle < (float)Math.PI / -2f)
            {
                lineAngle = (float)lineAngle - (float)Math.PI;
                proxy.FlipSides = true;
            }
            else
                proxy.FlipSides = false;

            //lineAngle *= 180f / (float)Math.PI;

            proxy.Angle = lineAngle;

        }

        /// <summary>
        /// Test if a point touches a multi-segment line.
        /// </summary>
        /// <param name="testPoint">Point to test.</param>
        /// <param name="width">Test width of line.</param>
        /// <param name="bounds">Bounding rectagle of all points.</param>
        /// <param name="line"></param>
        /// <returns></returns>
        public static bool HitTestLine(RawVector2 testPoint, float width, SharpDX.RectangleF bounds, IList<RawVector2> line)
        {
            // if we are outside the bounds of the rectangle containing the line, false
            // TODO: fix bounds test
            // if (!bounds.Contains(testPoint)) return false;

            // if the line doesn't have any segments, return false
            if (line.Count < 2) return false;

            // test each segment
            for (int i = 0; i < line.Count - 1; i++)
            {
                if (HitTestLineSegment(testPoint, line[i], line[i + 1], width))
                    return true;
            }

            // no segments within range
            return false;
        }

        /// <summary>
        /// Test if a point touches a line segment.
        /// </summary>
        /// <param name="testPoint">Point to test.</param>
        /// <param name="start">Start of line segment.</param>
        /// <param name="end">End of line segment.</param>
        /// <param name="width">Width of line.</param>
        /// <returns></returns>
        public static bool HitTestLineSegment(RawVector2 testPoint, RawVector2 start, RawVector2 end, float width)
        {
            return HitTestLineSegment(testPoint.X, testPoint.Y, start.X, start.Y, end.X, end.Y, width);
        }

        /// <summary>
        /// Test if a point touches a line segment.
        /// </summary>
        /// <param name="testX">Point to test.</param>
        /// <param name="testY">Point to test.</param>
        /// <param name="startX">Start of line segment.</param>
        /// <param name="startY">Start of line segment.</param>
        /// <param name="endX">End of line segment.</param>
        /// <param name="endY">End of line segment.</param>
        /// <param name="width">Width of line.</param>
        /// <returns></returns>
        public static bool HitTestLineSegment(float testX, float testY, float startX, float startY, float endX, float endY, float width)
        {
            float halfWidth = (width / 2 < 1) ? 1 : width / 2;
            float dist = DistanceToLine(testX, testY, startX, startY, endX, endY);
            return (dist >= -halfWidth && dist <= halfWidth);
        }

        public static List<RawVector2> SortPointsClosest(List<RawVector2> points, bool preferNextValue)
        {

            if (points == null || points.Count <= 2) return points;

            int start = points.Count / 2;
            List<RawVector2> sortedPoints = new List<RawVector2> { points[start] };
            points.RemoveAt(start);

            while (points.Count > 0)
            {
                var last = sortedPoints.Last();

                int ixClosest = 0;
                float dNext = DistanceSquared(last.X, last.Y, points[0].X, points[0].Y);
                float closestD = float.MaxValue;

                for (int i = 0; i < points.Count; i++)
                {
                    var p = points[i];
                    var d = DistanceSquared(last.X, last.Y, p.X, p.Y);
                    if (d < closestD)
                    {
                        closestD = d;
                        ixClosest = i;
                    }
                }

                if (ixClosest != 0)
                {
                    if (!(closestD < dNext * 0.2f))
                    {
                        ixClosest = 0;
                    }
                }

                if (Vector2.DistanceSquared(points[ixClosest], last) > 0.00001f)
                    sortedPoints.Add(points[ixClosest]);
                points.RemoveAt(ixClosest);
            }

            return sortedPoints;
        }

        public static bool HitTestLineSegmentF(float testX, float testY, float startX, float startY, float endX, float endY, float width)
        {
            float halfWidth = width / 2;
            float dist = DistanceToLine(testX, testY, startX, startY, endX, endY);
            return (dist >= -halfWidth && dist <= halfWidth);
        }

        /// <summary>
        /// Test the distance of a point from a line.
        /// </summary>
        /// <param name="testPoint">Point to test.</param>
        /// <param name="start">Start of line segment.</param>
        /// <param name="end">End of line segment.</param>
        /// <returns>Minimum distance to the line</returns>
        public static float DistanceToLine(RawVector2 testPoint, RawVector2 start, RawVector2 end)
        {
            return DistanceToLine(testPoint.X, testPoint.Y, start.X, start.Y, end.X, end.Y);
        }

        public static float DistanceSquared(float x1, float y1, float x2, float y2)
        {
            return ((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
        }

        /// <summary>
        /// Test the distance of a point from a line.
        /// </summary>
        /// <param name="x">Test point X coordinate.</param>
        /// <param name="y">Test point Y coordinate.</param>
        /// <param name="x1">Start of line X coordinate.</param>
        /// <param name="y1">Start of line Y coordinate.</param>
        /// <param name="x2">End of line X coordinate.</param>
        /// <param name="y2">End of line Y coordinate.</param>
        /// <returns>Minimum distance to the line</returns>
        public static float DistanceToLine(float x, float y, float x1, float y1, float x2, float y2)
        {
            float A = x - x1;
            float B = y - y1;
            float C = x2 - x1;
            float D = y2 - y1;

            // project vector
            float dot = A * C + B * D;
            float len_sq = C * C + D * D;
            float param = -1;
            if (len_sq != 0) //in case of 0 length line
                param = dot / len_sq;

            float xx, yy;

            if (param < 0)
            {
                xx = x1;
                yy = y1;
            }
            else if (param > 1)
            {
                xx = x2;
                yy = y2;
            }
            else
            {
                xx = x1 + param * C;
                yy = y1 + param * D;
            }

            var dx = x - xx;
            var dy = y - yy;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
    }

    public class MultiLineProxy
    {
        public List<LineProxy> Lines = new List<LineProxy>(2);
    }
}