using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Xml;
using System.Windows.Forms;
using System.ComponentModel;
using Macomber_Map.Data_Connections;

namespace Macomber_Map.Data_Elements
{
    /// <summary>
    /// This class covers the various types of alarms, events, violations, etc., including their display parameters and aggregate numbers
    /// </summary>
    public class MM_AlarmViolation_Type: MM_DisplayParameter
    {
        #region Variable declarations
        /// <summary>The collection of violations of this particular type</summary>
        public Dictionary<MM_AlarmViolation, MM_AlarmViolation> Violations;

        /// <summary>The collection of violations of this particular type sorted by voltage</summary>
        public Dictionary<MM_KVLevel, Dictionary<MM_AlarmViolation, MM_AlarmViolation>> ViolationsByVoltage = new Dictionary<MM_KVLevel, Dictionary<MM_AlarmViolation, MM_AlarmViolation>>();

        /// <summary>The collection of violations of this particular type sorted by element type</summary>
        public Dictionary<MM_Element_Type, Dictionary<MM_AlarmViolation, MM_AlarmViolation>> ViolationsByElementType = new Dictionary<MM_Element_Type, Dictionary<MM_AlarmViolation, MM_AlarmViolation>>();

        /// <summary>The index/priority of the violation (for use with the ImageList)</summary>
        public int ViolationIndex;

        /// <summary>The name of the violation type</summary>
        public new String Name;

        /// <summary>The acronym of the violation</summary>
        private String _Acronym;       

        /// <summary>The possible modes of display for a violation</summary>
        public enum DisplayModeEnum { 
            /// <summary>Never display the violation</summary> 
            Never,
            /// <summary>Display the violation when selected in the violation viewer</summary>
            WhenSelected,
            /// <summary>Display the violation regardless of selection in the violation viewer</summary>
            Always};        

        
        private DisplayModeEnum _ViolationViewer = DisplayModeEnum.WhenSelected;
        
        
        /// <summary>
        /// How the violation type should be displayed in the violation viewer
        /// </summary>
        [Category("Visibility"), Description("How the violation type should be displayed in the violation viewer"), DefaultValue(DisplayModeEnum.WhenSelected)]
        public DisplayModeEnum ViolationViewer
        {
            get { return _ViolationViewer; }
            set { _ViolationViewer = value; }
        }

        /// <summary>
        /// Whether this violation should be spoken aloud
        /// </summary>
        [Category("Speech"), Description("Whether this violation should be spoken aloud")]
        public bool SpeakViolation
        {
            get { return _SpeakViolation; }
            set { _SpeakViolation = value; }
        }
        private bool _SpeakViolation = true;

        private bool _ViolationViewerDefault= true;
        /// <summary>
        /// Whether the violation is on in the violation viewer by default
        /// </summary>
        [Category("Visibility"), Description("Whether the violation is on in the violation viewer by default"), DefaultValue(true)]
        public bool ViolationViewerDefault
        {
            get { return _ViolationViewerDefault; }
            set { _ViolationViewerDefault = value; }
        }

        private DisplayModeEnum _MiniMap =  DisplayModeEnum.WhenSelected;
        /// <summary>
        /// How the violation should be displayed in the mini-map
        /// </summary>
        [Category("Visibility"), Description("How the violation should be displayed in the mini-map"), DefaultValue(MM_AlarmViolation_Type.DisplayModeEnum.WhenSelected)]
        public DisplayModeEnum MiniMap
        {
            get { return _MiniMap; }
            set { _MiniMap = value; }
        }


        private DisplayModeEnum _Substation =  DisplayModeEnum.WhenSelected;
        /// <summary>
        /// Whether the violation type should be shown by its substation
        /// </summary>
        [Category("Visibility"), Description("Whether the violation type should be shown by its substation"), DefaultValue(MM_AlarmViolation_Type.DisplayModeEnum.WhenSelected)]
        public DisplayModeEnum NetworkMap_Substation
        {
            get { return _Substation; }
            set { _Substation = value; }
        }

        private DisplayModeEnum _Line = DisplayModeEnum.WhenSelected;
        /// <summary>
        /// Whether the violation type should be shown by its Line
        /// </summary>
        [Category("Visibility"), Description("Whether the violation type should be shown by its Line"), DefaultValue(MM_AlarmViolation_Type.DisplayModeEnum.WhenSelected)]
        public DisplayModeEnum NetworkMap_Line
        {
            get { return _Line; }
            set { _Line = value; }
        }

        private DisplayModeEnum _OneLineElement = DisplayModeEnum.WhenSelected;
        /// <summary>
        /// Whether the violation type should be shown in a one-line diagram
        /// </summary>
        [Category("Visibility"), Description("Whether the violation type should be shown in a one-line diagram"), DefaultValue(MM_AlarmViolation_Type.DisplayModeEnum.WhenSelected)]
        public DisplayModeEnum OneLineElement
        {
            get { return _OneLineElement; }
            set { _OneLineElement = value; }
        }

        /// <summary>
        /// The acronym for the violation type
        /// </summary>
        [Category("Display"), Description("The acronym for the violation type")]
        public String Acronym
        {
            get { return _Acronym; }
            set { _Acronym = value; RebuildViolationGraphic(); }
        }
     
        private int _Priority;
        /// <summary>
        /// The priority of the alarm
        /// </summary>
        [Category("Prioritization"), Description("The priority of the alarm")]
        public int Priority
        {
            get { return _Priority; }
            set { _Priority = value; }
        }

        /// <summary>
        /// The overall visibility of the alarm. If any are "when shown" or "always", return true.
        /// </summary>
        [Category("Display"), Description("The overall visibility of the alarm")]
        public bool SimpleVisibility
        {
            get { return ((_Line != DisplayModeEnum.Never) || (_MiniMap != DisplayModeEnum.Never) || (_OneLineElement != DisplayModeEnum.Never) || (_Substation != DisplayModeEnum.Never) || (_ViolationViewer != DisplayModeEnum.Never)); }
            set { _Line = _MiniMap = _OneLineElement = _Substation = _ViolationViewer = (value == true ? DisplayModeEnum.WhenSelected : DisplayModeEnum.Never); }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize the violation type
        /// </summary>
        /// <param name="xE">The XML Element to parse for the violation</param>
        /// <param name="AddIfNew">Whether to add the new elements</param>
        public MM_AlarmViolation_Type(XmlElement xE, bool AddIfNew)
            : base(xE,AddIfNew)
        {
            //Initialize the collections of alarms of this type
            Violations = new Dictionary<MM_AlarmViolation, MM_AlarmViolation>();
            ViolationsByVoltage = new Dictionary<MM_KVLevel, Dictionary<MM_AlarmViolation, MM_AlarmViolation>>(MM_Repository.KVLevels.Count);
            RebuildViolationGraphic();     
        }
        

        /*// <summary>
        /// Reinitialize the alarm violation type based on its XML configuration
        /// </summary>
        /// <param name="xE"></param>
        public new void ReinitializeDisplay(XmlElement xE)
        {
            this.Name = xE.Attributes["Name"].Value;

            if (xE.HasAttribute("Line"))
                this._Line = (DisplayModeEnum)Enum.Parse(typeof(DisplayModeEnum), xE.Attributes["Line"].Value, true);

            if (xE.HasAttribute("Substation"))
                this._Substation = (DisplayModeEnum)Enum.Parse(typeof(DisplayModeEnum), xE.Attributes["Substation"].Value, true);

            if (xE.HasAttribute("ViolationViewer"))
                this._ViolationViewer = (DisplayModeEnum)Enum.Parse(typeof(DisplayModeEnum), xE.Attributes["ViolationViewer"].Value, true);                    

            if (xE.HasAttribute("MiniMap"))
                this._MiniMap = (DisplayModeEnum)Enum.Parse(typeof(DisplayModeEnum), xE.Attributes["MiniMap"].Value, true);

            if (xE.HasAttribute("Acronym"))
                this.Acronym = xE.Attributes["Acronym"].Value;

            if (xE.HasAttribute("Priority"))
                this.Priority = XmlConvert.ToInt32(xE.Attributes["Priority"].Value);

            base.ReinitializeDisplay(xE);
        }*/

        /// <summary>
        /// Rebuild the graphic for a violation when its color changes
        /// </summary>       
        public void RebuildViolationGraphic()
        {
            Bitmap b = new Bitmap(16, 16, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Graphics g = Graphics.FromImage(b);
            StringFormat sF = new StringFormat();
            sF.Alignment = StringAlignment.Center;
            sF.LineAlignment = StringAlignment.Center;
            sF.Trimming = StringTrimming.None;
            sF.FormatFlags = StringFormatFlags.NoWrap;

            int ColorBrightness = ((ForeColor.R * 299) + (ForeColor.G * 587) + (ForeColor .B* 114)) / 1000;

            Color TextColor = (ColorBrightness >= 96 ? Color.FromArgb(32, 32, 32) : Color.FromArgb(223, 223, 223));            
            g.Clear(ForeColor);
            using (SolidBrush TextBrush = new SolidBrush(TextColor))
                g.DrawString(_Acronym, new Font("Arial", 8, FontStyle.Bold), TextBrush, new RectangleF(0, 0, 16, 16), sF);
            g.Dispose();

            //First, update the display view's violation
            
            
            if (MM_Repository.ViolationImages != null)
                if (MM_Repository.ViolationImages.Images.ContainsKey(this.Name))
                {
                    MM_Repository.ViolationImages.Images[this.ViolationIndex] = b;
                    MM_Repository.ViolationImages.Images.SetKeyName(this.ViolationIndex, this.Name);
                }
                else
                {
                    MM_Repository.ViolationImages.Images.Add(this.Name, b);
                    this.ViolationIndex = MM_Repository.ViolationImages.Images.Count - 1;
                }            
        }
        #endregion

        /// <summary>
        /// Return the name of the violation type
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name + " (" + Acronym + "; " + ViolationIndex.ToString() + ")";
        }

        /// <summary>
        /// Determine the worst violation of a collection
        /// </summary>
        /// <param name="InViolations"></param>
        /// <returns></returns>
        public static MM_AlarmViolation_Type WorstViolation(params MM_AlarmViolation_Type[] InViolations)
        {
            MM_AlarmViolation_Type OutViol = null;
            foreach (MM_AlarmViolation_Type Viol in InViolations)
                if (Viol == null)
                { }
                else if (OutViol == null)
                    OutViol = Viol;
                else if (OutViol.Priority < Viol.Priority)
                    OutViol = Viol;
            return OutViol;
        }
    }
}
