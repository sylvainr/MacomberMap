using ProjNet.CoordinateSystems.Transformations;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TEDESimulator.GIS;

namespace TEDESimulator.Elements
{
    public class Sim_Boundary
    {
        private const double XOrigin = -118608900;
        private const double YOrigin = -91259500;
        private const double XYScale = 35979706.307677768;
        public List<PointF> Coordinates = new List<PointF>();
        public double Min_X = double.NaN, Min_Y = double.NaN, Max_X = double.NaN, Max_Y = double.NaN;
        public String Name;
        public GraphicsPath CountyBoundary = new GraphicsPath();
        public double Centroid_X = 0, Centroid_Y = 0;

        public Sim_Boundary(double Min_X, double Min_Y, double Max_X, double Max_Y, double Centroid_X, double Centroid_Y)
        {
            this.Min_X = Min_X;
            this.Min_Y = Min_Y;
            this.Max_X = Max_X;
            this.Max_Y = Max_Y;
            this.Centroid_X = Centroid_X;
            this.Centroid_Y = Centroid_Y;
            this.Name = "STATE";
            this.Coordinates.Add(new PointF((float)Min_X, (float)Min_Y));
            this.Coordinates.Add(new PointF((float)Min_X, (float)Max_Y));
            this.Coordinates.Add(new PointF((float)Max_X, (float)Max_Y));
            this.Coordinates.Add(new PointF((float)Min_X, (float)Max_Y));
        }

        public Sim_Boundary(XmlElement xElem, Sim_Builder Builder)
        {
            foreach (XmlAttribute xAttr in xElem.Attributes)
            {
                FieldInfo fI = typeof(Sim_Boundary).GetField(xAttr.Name);
                if (fI == null)
                { }
                else if (fI.FieldType == typeof(String))
                    fI.SetValue(this,xAttr.Value);
                else if (fI.FieldType == typeof(Double))
                    fI.SetValue(this,XmlConvert.ToDouble(xAttr.Value));
                else if (fI.FieldType==typeof(List<PointF>))
                {
                    List<PointF> InList = (List < PointF >) fI.GetValue(this);
                    String[] SplStr = xAttr.Value.Split(',');
                    for (int a = 0; a < SplStr.Length; a += 2)
                        InList.Add(new PointF(XmlConvert.ToSingle(SplStr[a]), XmlConvert.ToSingle(SplStr[a + 1])));
                }
            }
        }

        /// <summary>
        /// Create a new shape object
        /// </summary>
        /// <param name="InShape"></param>
        public Sim_Boundary(Shape InShape, ICoordinateTransformation XF, Sim_Builder Builder)
        {

            List<PointF> MagnifiedPoints = new List<PointF>();
            foreach (Shape.Part Part in InShape.Parts)
                foreach (Vertex v in Part)
                {
                    double[] NewPoint = XF.MathTransform.Transform(new double[] { v.X, v.Y });
                    CheckFunction(NewPoint[0], ref Min_X, ref Max_X, ref Centroid_X);
                    CheckFunction(NewPoint[1], ref Min_Y, ref Max_Y, ref Centroid_Y);
                    CheckFunction(NewPoint[0], ref Builder.Min_X, ref Builder.Max_X, ref Builder.Centroid_X);
                    CheckFunction(NewPoint[1], ref Builder.Min_Y, ref Builder.Max_Y, ref Builder.Centroid_Y);
                    Coordinates.Add(new PointF((float)NewPoint[0], (float)NewPoint[1]));
                    MagnifiedPoints.Add(new PointF((float)NewPoint[0] * 1000f, (float)NewPoint[1] * 1000f));
                }

            Centroid_X /= (double)Coordinates.Count;
            Centroid_Y /= (double)Coordinates.Count;
            Builder.CoordinateCount += Coordinates.Count;
            this.CountyBoundary = new GraphicsPath();
            this.CountyBoundary.AddLines(MagnifiedPoints.ToArray());
            this.CountyBoundary.CloseAllFigures();
            this.Name = (InShape.GetAttribute(0) ?? "?").ToString();
        }

        public bool HitTest(float Longitude, float Latitude)
        {
            return this.CountyBoundary.IsVisible(Longitude * 1000f, Latitude * 1000f);
        }



        private void CheckFunction(double InValue, ref double Min, ref double Max, ref double Centroid)
        {
            if (double.IsNaN(Min) || InValue < Min)
                Min = InValue;
            if (double.IsNaN(Max) || InValue > Max)
                Max = InValue;
            Centroid += InValue;
        }

        public string GetXml()
        {
            if (this.Name == "STATE")
                return $"<State Name=\"{Name}\" ElemType=\"State\" TEID=\"0\" Max=\"{Max_X},{Max_Y}\" Max_X=\"{Max_X}\" Max_Y=\"{Max_Y}\" Min_X=\"{Min_X}\" Min_Y=\"{Min_Y}\" Min=\"{Min_X},{Min_Y}\" Centroid_X=\"{Centroid_X}\" Centroid_Y=\"{Centroid_Y}\" Centroid=\"{Centroid_X},{Centroid_Y}\" Coordinates=\"{CoordinateString}\"/>";

            else
                return $"<County Name=\"{Name}\" ElemType=\"County\" TEID=\"0\" Max=\"{Max_X},{Max_Y}\" Max_X=\"{Max_X}\" Max_Y=\"{Max_Y}\" Min_X=\"{Min_X}\" Min_Y=\"{Min_Y}\" Min=\"{Min_X},{Min_Y}\" Centroid_X=\"{Centroid_X}\" Centroid_Y=\"{Centroid_Y}\" Centroid=\"{Centroid_X},{Centroid_Y}\" Coordinates=\"{CoordinateString}\"/>";
        }

        public String CoordinateString
        {
            get
            {
                StringBuilder sB = new StringBuilder();
                foreach (PointF pt in Coordinates)
                    sB.Append((sB.Length == 0 ? "" : ",") + pt.X + "," + pt.Y);
                return sB.ToString();
            }
        }

        public bool Contains(Sim_Boundary OtherShape)
        {
            return OtherShape.Centroid_X >= Min_X && OtherShape.Centroid_X <= Max_X && OtherShape.Centroid_Y >= Min_Y && OtherShape.Centroid_Y <= Max_Y;
        }

        public bool Contains(double Longitude, double Latitude)
        {
            return Longitude >= Min_X && Longitude <= Max_X && Latitude >= Min_Y && Latitude <= Max_Y;

        }
    }
}
