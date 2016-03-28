using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace MacomberMap.UI
{
    /// <summary>
    /// DataGridView Column for displaying generator under/over dispatch grapically. Extends DataGridViewTextBoxColumn.
    /// </summary>
    [ToolboxItem(true), ToolboxBitmap(typeof(DataGridViewBarGraphDevColumn), "MacomberMap.UI.Icons.BarGraphDevColumn.png")]
    public class DataGridViewBarGraphDevColumn : DataGridViewTextBoxColumn
    {
        /// <summary>
        /// DataGridView Column for displaying generator under/over dispatch grapically. Extends DataGridViewTextBoxColumn.
        /// </summary>
        public DataGridViewBarGraphDevColumn()
        {
            this.CellTemplate = new DataGridViewBarGraphDevCell();
            this.ReadOnly = true;
        }


    }

    /// <summary>
    /// DataGridView Cell for displaying generator under/over dispatch grapically. Extends DataGridViewTextBoxCell.
    /// </summary>
    [ToolboxItem(true), ToolboxBitmap(typeof(DataGridViewBarGraphDevCell), "MacomberMap.UI.Icons.BarGraphDevColumn.png")]
    public class DataGridViewBarGraphDevCell : DataGridViewTextBoxCell
    {
        protected override void Paint(
            Graphics graphics,
            Rectangle clipBounds,
            Rectangle cellBounds,
            int rowIndex,
            DataGridViewElementStates cellState,
            object value,
            object formattedValue,
            string errorText,
            DataGridViewCellStyle cellStyle,
            DataGridViewAdvancedBorderStyle advancedBorderStyle,
            DataGridViewPaintParts paintParts)
        {
            base.Paint(graphics, clipBounds,
                cellBounds, rowIndex, cellState,
                value, "", errorText,
                cellStyle, advancedBorderStyle,
                paintParts);

            DeviationBarData cellVal = value as DeviationBarData ?? new DeviationBarData();

            double maxActDisp = Math.Max(cellVal.Actual, cellVal.Dispatch);
            double maxVal = Math.Max(maxActDisp, cellVal.Max);

            float barWidthDev = 0;
            float barWidthDisp = 0;

            int drawHeight = cellBounds.Height - 1;
            int drawWidth = cellBounds.Width - 1;

            // Draw Background
            Rectangle background = new Rectangle(cellBounds.X, cellBounds.Y, drawWidth, drawHeight);
            graphics.FillRectangle(Brushes.Black, background);
            //graphics.SetClip(background);
            if (maxVal > 0 && maxActDisp > 0)
            {

                barWidthDisp = Math.Max(0, (float)(cellVal.Dispatch / maxVal * drawWidth));
                barWidthDev = (float)(Math.Abs(cellVal.Deviation / maxVal) * drawWidth);
                //barWidthMW = (float)(cellVal.Actual / cellVal.Max) * drawWidth;

                //if (barWidthDisp < 0) { barWidthDisp = 0; }
                //if (barWidthMW < 0) { barWidthMW = 0; }
                if (cellVal.Dispatch < 0 && barWidthDev > barWidthDisp) { barWidthDev = barWidthDisp; }

                // Draw Dispatch Blue Bar
                Rectangle barDisp = new Rectangle(cellBounds.X, cellBounds.Y, (int)barWidthDisp, drawHeight);
                if (cellVal.IsDispatched)
                    graphics.FillRectangle(new SolidBrush(Color.Navy), barDisp);
                else
                    graphics.FillRectangle(new SolidBrush(Color.ForestGreen), barDisp);

                // Draw Deviation Bar
                Rectangle barDEV;
                if (cellVal.Deviation >= 0) // Green +
                {
                    barDEV = new Rectangle(cellBounds.X + (int)barWidthDisp, cellBounds.Y, (int)barWidthDev, drawHeight);
                    graphics.FillRectangle(Brushes.Lime, barDEV);
                }
                else // Red -
                {
                    barDEV = new Rectangle(cellBounds.X + (int)barWidthDisp - (int)(barWidthDev), cellBounds.Y, (int)barWidthDev, drawHeight);
                    graphics.FillRectangle(Brushes.Red, barDEV);
                }

                // Draw End Line
                Pen penCap = new Pen(Brushes.Yellow, 2.0f);
                int endCapX = cellBounds.X;
                if ((int)barWidthDisp - 1 > 0)
                    endCapX = endCapX + (int)barWidthDisp - 1;

                Point start = new Point(endCapX, cellBounds.Y);
                Point end = new Point(endCapX, cellBounds.Y + drawHeight);
                graphics.DrawLine(penCap, start, end);



                // Draw Super-awesome gradient overlay
                Color PageStartColor = Color.FromArgb(50, Color.Gray);
                Color PageEndColor = Color.FromArgb(80, Color.White);
                Rectangle barAreaTop;
                Rectangle barAreaBottom;
                int barWidth = (int)Math.Max(0, (float)(maxActDisp / maxVal * drawWidth));
                barAreaTop = new Rectangle(cellBounds.X, cellBounds.Y, barWidth, cellBounds.Height / 2);
                barAreaBottom = new Rectangle(cellBounds.X, cellBounds.Y + (cellBounds.Height / 2), barWidth, (cellBounds.Height / 2) - 1);

                System.Drawing.Drawing2D.LinearGradientBrush gradBrushHighlight;
                System.Drawing.Drawing2D.LinearGradientBrush gradBrushLowlight;

                gradBrushHighlight = new System.Drawing.Drawing2D.LinearGradientBrush(
                            new Point(cellBounds.X, cellBounds.Y),
                            new Point(cellBounds.X, cellBounds.Y + cellBounds.Height),
                            PageStartColor, PageEndColor);

                PageStartColor = Color.FromArgb(25, Color.White);
                PageEndColor = Color.FromArgb(100, Color.Black);
                gradBrushLowlight = new System.Drawing.Drawing2D.LinearGradientBrush(
                            new Point(cellBounds.X, cellBounds.Y),
                            new Point(cellBounds.X, cellBounds.Y + cellBounds.Height),
                            PageStartColor, PageEndColor);

                graphics.FillRectangle(gradBrushHighlight, barAreaTop);
                graphics.FillRectangle(gradBrushLowlight, barAreaBottom);


            }

            // Draw Border
            //Pen penBorder = new Pen(Brushes.LightGray, 1.0f);
            //Rectangle border = background;
            //border.Width--;
            //border.Height--;
            ////graphics.DrawRectangle(penBorder, border);

        }
    }

    [Designer("System.Windows.Forms.Design.ParentControlDesigner, System.Design", typeof(IDesigner))]
    public class DeviationBarData: INotifyPropertyChanged
    {
        private double _Max;
        public double Max
        {
            get { return _Max; }
            set { _Max = value; OnPropertyChanged("Max"); }
        }

        private double _Actual;
        public double Actual
        {
            get { return _Actual; }
            set { _Actual = value; OnPropertyChanged("Actual"); OnPropertyChanged("Deviation"); }
        }

        private bool _IsDispatched;
        public bool IsDispatched
        {
            get { return _IsDispatched; }
            set { _IsDispatched = value; OnPropertyChanged("IsDispatched"); }
        }

        private double _Dispatch;
        public double Dispatch
        {
            get { 
                if (IsDispatched)
                    return _Dispatch;
                return Actual;
            }
            set { 
                _Dispatch = value;
                OnPropertyChanged("Dispatch");
                OnPropertyChanged("Deviation"); 
            }
        }


        public double Deviation
        {
            get { return (Actual - Dispatch); }
        }

        public override string ToString()
        {
            return Math.Abs(Deviation).ToString("00000.000");
        }

        #region Operator Overrides


        private static bool matchFields(DeviationBarData a, DeviationBarData m)
        {
            // Match Fields Here, e.g. a.Field1 == b.Field1 && a.Field2 == b.Field2
            return (a.Max == m.Max && a.Actual == m.Actual && a.Dispatch == m.Dispatch);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            DeviationBarData i = obj as DeviationBarData;
            if ((System.Object)i == null)
            {
                return false;
            }

            // Return true if the fields match:
            return matchFields(this, i);
        }

        public bool Equals(DeviationBarData p)
        {
            // If parameter is null return false:
            if ((object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return matchFields(this, p);
        }

        public static bool operator ==(DeviationBarData a, DeviationBarData b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return matchFields(a, b);
        }

        public static bool operator !=(DeviationBarData a, DeviationBarData b)
        {
            return !(a == b);
        }

        public static bool operator >(DeviationBarData a, DeviationBarData b)
        {
            return (Math.Abs(a.Deviation) > Math.Abs(b.Deviation));
        }
        public static bool operator >=(DeviationBarData a, DeviationBarData b)
        {
            return (Math.Abs(a.Deviation) >= Math.Abs(b.Deviation));
        }
        public static bool operator <(DeviationBarData a, DeviationBarData b)
        {
            return (Math.Abs(a.Deviation) < Math.Abs(b.Deviation));
        }
        public static bool operator <=(DeviationBarData a, DeviationBarData b)
        {
            return (Math.Abs(a.Deviation) <= Math.Abs(b.Deviation));
        }

        public override int GetHashCode()
        {

            int result = 17;
            result = result * 37 + Max.GetHashCode();
            result = result * 37 + Actual.GetHashCode();
            result = result * 37 + Dispatch.GetHashCode();
            return result;
        }


        // Create the OnPropertyChanged method to raise the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
			

        #endregion

    }
}
