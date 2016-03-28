using MacomberMapClient.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Drawing;

namespace MacomberMapClient.User_Interfaces.Summary
{
    /// <summary>
    /// This class provides a label for a high-level label
    /// </summary>
    public class MM_Key_Indicator_Label : Label
    {
        #region Variable declarations
        /// <summary>The format for this label</summary>
        public String Format;

        /// <summary>The title of the application</summary>
        public String Title;

        /// <summary>The values to be displayed</summary>
        public Data_Integration.OverallIndicatorEnum Indicator;

        /// <summary>Our comparison indicator</summary>
        public Data_Integration.OverallIndicatorEnum Comparison;

        /// <summary>The axis type</summary>
        public String Axis;

        /// <summary>The percentage format to be used when comparisons are made</summary>
        public String PercentageFormat;

        /// <summary>Whether this label should be compared to another indicator</summary>
        public bool ComparisonAxis = false;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new key indicator level
        /// </summary>
        /// <param name="xElem"></param>
        /// <param name="InForm"></param>
        /// <param name="Click"></param>
        public MM_Key_Indicator_Label(XmlElement xElem, Form InForm, EventHandler Click)
        {
            if(!MM_Server_Interface.ISQse)
            this.Tag = InForm;
            InForm.Visible = false;
            this.Click += Click;
            this.AutoSize = true;
            this.Font = MM_Repository.OverallDisplay.KeyIndicatorSimpleFont;
           
            MM_Serializable.ReadXml(xElem, this, true);
            this.BackColor = Color.Transparent;
        }
        #endregion


        /// <summary>
        /// Handle painting of the key indicator label
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            String[] splStr = this.Text.Split(':');
            SizeF DrawSize = e.Graphics.MeasureString(splStr[0] + ":", MM_Repository.OverallDisplay.KeyIndicatorLabelFont);
            using (Brush DrawBrush = new SolidBrush(this.ForeColor))
            {
                e.Graphics.DrawString(splStr[0] + ":", this.Font, DrawBrush, PointF.Empty);
                e.Graphics.DrawString(splStr[1].TrimStart(' '), MM_Repository.OverallDisplay.KeyIndicatorValueFont, DrawBrush, DrawSize.Width - 5, 0);
            }
        }

        #region Updating
        /// <summary>
        /// Update the current information
        /// </summary>
        /// <param name="ShowValueType">Show the value type</param>
        /// <param name="Compare">Whether the value should be compared to a second</param>
        public void UpdateText(bool ShowValueType, bool Compare)
        {
            float CurValue = Data_Integration.OverallIndicators[(int)Indicator];

            if (Compare && ComparisonAxis)
                this.Text = Title + ": " + CurValue.ToString(Format) + (ShowValueType ? " " + Axis : "") + " " + (CurValue / Data_Integration.OverallIndicators[(int)Comparison]).ToString(PercentageFormat);
            else
                this.Text = Title + ": " + CurValue.ToString(Format) + (ShowValueType ? " " + Axis : "");



            if (Title == "ACE")
                ForeColor = Data_Integration.ACEColor;
            else if (Title == "Freq")
            {
                if (CurValue > 59.950001f && CurValue < 60.049999f)
                    ForeColor = Color.White;
                else if (CurValue >= 59.700001f && CurValue <= 60.200001f)
                    ForeColor = Color.Yellow;
                else
                    ForeColor = Color.Red;
            }
            else if (Indicator == Data_Integration.OverallIndicatorEnum.IslandCount)
                ForeColor = (Data_Integration.OverallIndicators[(int)Data_Integration.OverallIndicatorEnum.IslandCount] == 1) ? Color.White : Color.Red;
        }
        #endregion
    }
}
