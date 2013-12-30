using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Drawing2D;
using Macomber_Map.Data_Elements;
using Macomber_Map.Data_Connections;
using System.Windows.Forms;
using System.Drawing;

namespace Macomber_Map.User_Interface_Components.GDIPlus
{
    /// <summary>
    /// This class holds all of the relevant information on a substation
    /// </summary>
    public class MM_GDI_Substation
    {
        #region Variable declarations
        /// <summary>Whether or not the object is visible on the screen</summary>
        public bool Visible;

        /// <summary>Whether something connecting to this line is visible</summary>
        public bool ConnectorVisible = false;

        /// <summary>Whether the substation is visible due to a violation</summary>
        public bool ViolationVisible;

        /// <summary>Whether stations with units should be shown as diamonds, not squares.</summary>
        public static bool GenStnIsDiamond = true;

        /// <summary>The center of the substation</summary>
        public Point SubstationCenter;

        /// <summary>The standard display parameter for the element</summary>
        public MM_DisplayParameter StandardDisplayParameter;

        /// <summary>The worst violation display parameter</summary>
        public MM_DisplayParameter AlternateDisplayParameter;

        /// <summary>The image for rendering the substation</summary>
        public Bitmap SubstationImageName;

        /// <summary>The image for rendering the substation</summary>
        public Bitmap SubstationImageFull;

        /// <summary>The name-only text to be drawn for the substation</summary>
        public String SubstationTextName;

        /// <summary>The full text to be drawn for the substation</summary>
        public String SubstationTextFull;

        /// <summary>The network map owning this substation</summary>
        private Network_Map_GDI OwnerForm;

        /// <summary>The collection of auto-transformers</summary>
        public MM_Transformer[] AutoTransformers = null;
        #endregion


        #region Drawing
        /// <summary>
        /// Compute the parameters for drawing the substation
        /// </summary>
        /// <param name="Coordinates">Our system coordinates</param>
        /// <param name="Sub">The actual substation</param>
        /// <param name="ShownViolations">Our collection of shown violations</param>
        /// <param name="CallingObject">Our calling object</param>        
        public MM_GDI_Substation(MM_Coordinates Coordinates, MM_Substation Sub, Dictionary<MM_AlarmViolation, ListViewItem> ShownViolations, Network_Map_GDI CallingObject)
        {
            this.OwnerForm = CallingObject;
            ComputeSubstation(Coordinates, Sub, ShownViolations, CallingObject, true);
            Sub.ValuesChanged += new MM_Element.ValuesChangedDelegate(Sub_ValuesChanged);
            if (Sub.Transformers != null)
            {
                List<MM_Transformer> XFs = new List<MM_Transformer>();
                foreach (MM_Transformer XF in Sub.Transformers)
                    if (XF.KVLevel1 != null & XF.KVLevel2 != null)
                    if (XF.KVLevel1.Name.StartsWith("Other") == false && XF.KVLevel2.Name.StartsWith("Other") == false && XF.KVLevel1 != XF.KVLevel2)
                        XFs.Add(XF);
                if (XFs.Count > 0)
                    this.AutoTransformers = XFs.ToArray();
            }
        }

        /// <summary>
        /// Handle the value changes for a substation
        /// </summary>
        /// <param name="Element"></param>
        private void Sub_ValuesChanged(MM_Element Element)
        {
            this.StandardDisplayParameter = (Element as MM_Substation).SubstationDisplay(OwnerForm.violViewer.ShownViolations, OwnerForm);
        }



        /// <summary>
        /// Compute the parameters for drawing the substation
        /// </summary>
        /// <param name="Coordinates">Our system coordinates</param>
        /// <param name="Sub">The actual substation</param>
        /// <param name="ShownViolations">Our collection of shown violations</param>
        /// <param name="CallingForm">Our calling object</param>        
        /// <param name="RedrawText">Whether the text should be redrawn</param>
        public void ComputeSubstation(MM_Coordinates Coordinates, MM_Substation Sub, Dictionary<MM_AlarmViolation, ListViewItem> ShownViolations, Network_Map_GDI CallingForm, bool RedrawText)
        {
            this.StandardDisplayParameter = Sub.SubstationDisplay(ShownViolations, CallingForm);
            this.AlternateDisplayParameter = MM_Repository.SubstationDisplay;
            this.SubstationCenter = MM_Coordinates.LatLngToXY(Sub.LatLong, Coordinates.ZoomLevel);
            /*
            if (RedrawText)
                using (Graphics BaseGraphics = Graphics.FromHwnd(CallingForm.Handle))
                {
                    String TextToDraw = Sub.DisplayName();
                    if (TextToDraw != SubstationTextName)
                        SubstationImageName = CreateBitmap(TextToDraw, MM_Repository.OverallDisplay.NetworkMapFont, BaseGraphics);

                    TextToDraw = Sub.MapText(999);
                    if (TextToDraw != SubstationTextFull)
                        SubstationImageFull = CreateBitmap(TextToDraw, MM_Repository.OverallDisplay.NetworkMapFont, BaseGraphics);
                }*/
        }

        /// <summary>
        /// Create a new bitmap with the specified text
        /// </summary>
        /// <param name="TextToDraw">The text to draw</param>
        /// <param name="TextFont">The font to draw with</param>
        /// <param name="BaseGraphics">The base graphics (for string measurement)</param>
        /// <returns></returns>
        private Bitmap CreateBitmap(String TextToDraw, Font TextFont, Graphics BaseGraphics)
        {

            Size TextSize = BaseGraphics.MeasureString(TextToDraw, TextFont).ToSize();
            Bitmap OutImage = new Bitmap(TextSize.Width, TextSize.Height);
            Graphics g = Graphics.FromImage(OutImage);
            g.Clear(Color.Transparent);
            g.TextRenderingHint = MM_Repository.OverallDisplay.TextRenderingHint;
            g.TextContrast = MM_Repository.OverallDisplay.TextContrast;
            g.PixelOffsetMode = MM_Repository.OverallDisplay.PixelOffsetMode;
            g.SmoothingMode = MM_Repository.OverallDisplay.SmoothingMode;
            g.CompositingMode = MM_Repository.OverallDisplay.CompositingMode;
            g.CompositingQuality = MM_Repository.OverallDisplay.CompositingQuality;
            g.InterpolationMode = MM_Repository.OverallDisplay.InterpolationMode;
            g.DrawString(TextToDraw, TextFont, AlternateDisplayParameter.ForePen.Brush, PointF.Empty);
            g.Dispose();
            return OutImage;
        }

        /// <summary>
        /// Return a color corresponding to the value (0 is average)
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        private Color RelativeColor(float Value)
        {
            float R = 176;
            float G = 196;
            float B = 222;
            float Percentage = Value * 15f;


            if (float.IsNaN(Value))
            { }
            else if (Value > 0)
            {
                R = (255f * Percentage) + (176f * (1f - Percentage));
                G = 196 * (1f - Percentage);
                B = 222 * (1f - Percentage);
            }
            else
            {
                R = (176 * (1f + Percentage));
                G = 196 * (1f + Percentage);
                B = (-255f * Percentage) + (222 * (1f + Percentage));
            }
            return Color.FromArgb(Math.Max(0, Math.Min((int)R, 255)), Math.Max(0, Math.Min((int)G, 255)), Math.Max(0, Math.Min(255, (int)B)));
        }


        /// <summary>
        /// Draw the substation
        /// </summary>
        /// <param name="Substation">The substation upon which the image is based</param>
        /// <param name="Coordinates">The current coordinates</param>
        /// <param name="g">The graphics connector</param>
        /// <param name="ShownViolations">The collection of shown violations</param>
        /// <param name="CallingForm">The network map calling the line</param>
        /// <param name="DisplayAlternate">Display the alternate view if blinking on standard</param>
        public void DrawSubstation(MM_Substation Substation, MM_Coordinates Coordinates, Graphics g, Dictionary<MM_AlarmViolation, ListViewItem> ShownViolations, Network_Map_GDI CallingForm, bool DisplayAlternate)
        {
            //Determine our display parameters, and draw the substation
            MM_DisplayParameter Disp = Substation.SubstationDisplay(ShownViolations, CallingForm);
            if (Disp.Blink && DisplayAlternate)
                Disp = AlternateDisplayParameter;

            bool HasOutage = MM_Repository.SubstationDisplay.ShowAutoTransformersOut && OutagedTransformer;
            MM_Substation.enumFrequencyControlStatus FreqCtrlStatus = MM_Substation.enumFrequencyControlStatus.None;
            if (MM_Repository.SubstationDisplay.ShowFrequencyControl)
                FreqCtrlStatus = Substation.FrequencyControlStatus;
            bool HasSynchroscope = MM_Repository.SubstationDisplay.ShowSynchroscopes && Substation.HasSynchroscope;


            Color DrawColor = Disp.ForeColor;

            //If drawing the substation with voltage profile, update the color accordingly
            if (MM_Repository.SubstationDisplay.ShowSubstationVoltages)
            {
                float WorstPu = Substation.Average_pU;
                if (!float.IsNaN(WorstPu) && (WorstPu >= 1f + (MM_Repository.OverallDisplay.ContourThreshold / 100f) || WorstPu <= 1f - (MM_Repository.OverallDisplay.ContourThreshold / 100f)))
                    DrawColor = RelativeColor(WorstPu - 1f);
            }
            if (MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.DimNonBlackstartElements && !Substation.IsBlackstart)
                DrawColor = MM_Repository.OverallDisplay.BlackstartDim(DrawColor);
            else if (MM_Repository.OverallDisplay.BlackstartMode == MM_Display.enumBlackstartMode.DimNonOperatorElements && Array.IndexOf(Substation.Operators,MM_Repository.OverallDisplay.DisplayCompany) == -1)
                DrawColor = MM_Repository.OverallDisplay.BlackstartDim(DrawColor);            
            
            using (SolidBrush br = new SolidBrush(DrawColor))
            {
                if (GenStnIsDiamond && Substation.Units != null)
                    g.FillPolygon(br, new PointF[] { new PointF(SubstationCenter.X - (Disp.Width * 2f), SubstationCenter.Y), new PointF(SubstationCenter.X, SubstationCenter.Y - (Disp.Width * 2f)), new PointF(SubstationCenter.X + (Disp.Width * 2f), SubstationCenter.Y), new PointF(SubstationCenter.X, SubstationCenter.Y + (Disp.Width * 2f)) });
                else
                    g.FillRectangle(br, SubstationCenter.X - Disp.Width, SubstationCenter.Y - Disp.Width, Disp.Width * 2f, Disp.Width * 2f);

                //Now, if needed, draw our text
                foreach (MM_KVLevel Voltage in Substation.KVLevels)
                    if (Coordinates.ZoomLevel >= Voltage.StationMW || Coordinates.ZoomLevel >= Voltage.StationNames)
                    {
                        if (HasOutage)
                            g.DrawString(Substation.MapText(Coordinates.ZoomLevel), MM_Repository.OverallDisplay.NetworkMapFont, br, (int)(SubstationCenter.X - Disp.Width), (int)(SubstationCenter.Y + Disp.Width + 8));
                        else
                            g.DrawString(Substation.MapText(Coordinates.ZoomLevel), MM_Repository.OverallDisplay.NetworkMapFont, br, (int)(SubstationCenter.X - Disp.Width), (int)(SubstationCenter.Y + Disp.Width));
                        break;
                    }
                //Draw the substation frequency control information if needed

                if ((FreqCtrlStatus == MM_Substation.enumFrequencyControlStatus.Island && DisplayAlternate) || FreqCtrlStatus == MM_Substation.enumFrequencyControlStatus.Unit)
                    using (Pen OutagePen = new Pen(Color.Purple, 2))
                    {
                        g.DrawEllipse(OutagePen, SubstationCenter.X - 10, SubstationCenter.Y - 10, 20, 20);
                        g.DrawLine(OutagePen, SubstationCenter.X, SubstationCenter.Y - 10, SubstationCenter.X, SubstationCenter.Y + 10);
                        g.DrawLine(OutagePen, SubstationCenter.X - 10, SubstationCenter.Y, SubstationCenter.X + 10, SubstationCenter.Y);
                    }
                else if (HasOutage)
                    using (Pen OutagePen = new Pen(Color.Red, 2))
                    {
                        g.DrawEllipse(OutagePen, SubstationCenter.X - 10, SubstationCenter.Y - 10, 20, 20);
                        g.DrawLine(OutagePen, SubstationCenter.X, SubstationCenter.Y - 10, SubstationCenter.X, SubstationCenter.Y + 10);
                        g.DrawLine(OutagePen, SubstationCenter.X - 10, SubstationCenter.Y, SubstationCenter.X + 10, SubstationCenter.Y);
                    }
                else if (HasSynchroscope)
                    using (Pen OutagePen = new Pen(Color.Yellow, 2))
                    {
                        g.DrawEllipse(OutagePen, SubstationCenter.X - 10, SubstationCenter.Y - 10, 20, 20);
                        g.DrawLine(OutagePen, SubstationCenter.X, SubstationCenter.Y - 10, SubstationCenter.X, SubstationCenter.Y + 10);
                        g.DrawLine(OutagePen, SubstationCenter.X - 10, SubstationCenter.Y, SubstationCenter.X + 10, SubstationCenter.Y);
                    }
                
            }
        }

        /// <summary>
        /// Determine if an auto-transformer is outaged.
        /// </summary>
        public bool OutagedTransformer
        {
            get
            {
                if (this.AutoTransformers == null)
                    return false;
                else
                    foreach (MM_Transformer XF in this.AutoTransformers)
                        if (float.IsNaN(XF.MVAFlow) || XF.MVAFlow <= MM_Repository.OverallDisplay.EnergizationThreshold)
                            return true;
                return false;
            }
        }

        #endregion

        /// <summary>
        /// Test whether a clicked point is over a substation
        /// </summary>
        /// <param name="ClickedPoint">The point clicked by the user</param>
        /// <param name="UseAlternateIfAvailable">Whether or not to use the alternate display means</param>        
        /// <param name="BaseSize">The base multiplier (when width =1)</param>        
        /// <param name="HasGen">Whether this subsatation has a unit</param>
        /// <returns></returns>
        public bool HitTest(Point ClickedPoint, bool UseAlternateIfAvailable, float BaseSize, bool HasGen)
        {
            if (float.IsNaN(SubstationCenter.X)) return false;
            MM_DisplayParameter Param = (UseAlternateIfAvailable && StandardDisplayParameter.Blink ? AlternateDisplayParameter : StandardDisplayParameter);
            float StnSize = (GenStnIsDiamond && HasGen ? Param.Width * BaseSize * 2f : Param.Width * BaseSize);
            if ((ClickedPoint.X < SubstationCenter.X - StnSize) || (ClickedPoint.X > SubstationCenter.X + StnSize) || (ClickedPoint.Y < SubstationCenter.Y - StnSize) || (ClickedPoint.Y > SubstationCenter.Y + StnSize))
                return false;
            else
                return true;
        }


    }
}
