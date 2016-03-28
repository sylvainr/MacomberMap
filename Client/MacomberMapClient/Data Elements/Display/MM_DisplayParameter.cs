using MacomberMapClient.Integration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MacomberMapClient.Data_Elements.Display
{

    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class covers the parameters by which a particular type is displayed
    /// </summary>
    public class MM_DisplayParameter : MM_Serializable
    {
        #region Variable declarations
        /// <summary>The name of the display parameter</summary>
        public String Name;

        /// <summary>Whether the user is permitted to see these violations</summary>
        public bool Permitted;

        private bool _Blink = false;
        private float _Width = 1f;
        private Color _ForeColor;
        private Pen _ForePen;
        private DashStyle _DashStyle = DashStyle.Solid;


        /// <summary>The foreground color to display this event</summary>
        [Category("Display"), Description("The foreground color of the item")]
        public Color ForeColor
        {
            get { return _ForeColor; }
            set
            {
                lock (ForePen)
                    ForePen.Color = _ForeColor = value;
            }
        }

        /// <summary>When drawing a substation or line of this type, what width/size should be shown</summary>
        [Category("Display"), Description("The width of the item on a line or substation, or font weight"), DefaultValue(1f)]
        public float Width
        {
            get { return _Width; }
            set
            {
                lock (ForePen)
                    ForePen.Width = _Width = value;
            }
        }

        /// <summary>When drawing a substation or line of this type, should it blink?</summary>        
        [Category("Display"), Description("Whether this display parameter should blink between itself and another"), DefaultValue(false)]
        public bool Blink
        {
            get { return _Blink; }
            set { _Blink = value; }
        }

        /// <summary>Retrieve a pen for the view</summary>
        [Category("Display"), Description("The pen used when rendering this item"), DefaultValue(false)]
        public Pen ForePen
        {
            get
            {
                lock (this)
                    if (_ForePen == null)
                    {
                        _ForePen = new Pen(_ForeColor, _Width);
                        _ForePen.DashStyle = DashStyle;
                    }

                return _ForePen;
            }
        }

        /// <summary>The pen type for the line</summary>
        [Category("Display"), Description("The dashed style for the item"), DefaultValue(DashStyle.Solid)]
        public DashStyle DashStyle
        {
            get { return _DashStyle; }
            set
            {
                lock (ForePen)
                    ForePen.DashStyle = _DashStyle = value;
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize the display parameter
        /// </summary>
        /// <param name="ElementSource">The XML element containing the parameter's information</param>        
        /// <param name="AddIfNew"></param>
        public MM_DisplayParameter(XmlElement ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
            if (ElementSource.HasAttribute("Permitted"))
                this.Permitted = XmlConvert.ToBoolean(ElementSource.Attributes["Permitted"].Value);
            else
                this.Permitted = true;
            if (ElementSource.ParentNode.Name == "VoltageLevel")
                this.Name = ElementSource.ParentNode.Attributes["Name"].Value + "." + ElementSource.Name;
            else
                this.Name = ElementSource.Name;
        }

        /// <summary>
        /// Initialize a display parameter by color and thickness
        /// </summary>
        /// <param name="ForeColor">The foreground color of the line</param>
        /// <param name="Thickness">The thickness of the line</param>
        /// <param name="Blinking">Whether the parameter is blinking</param>
        public MM_DisplayParameter(Color ForeColor, float Thickness, bool Blinking)
        {
            this.Permitted = true;
            this.ForeColor = ForeColor;
            this.Width = Thickness;
            this.Blink = true;
        }

        /// <summary>
        /// Initialize a display parameter 
        /// </summary>
        public MM_DisplayParameter()
        {
            this.Permitted = true;
            this.ForeColor = Color.White;
            this.Width = 1;
            this.Blink = true;
        }      
        #endregion

        /// <summary>
        /// Return the name of the display parameter
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
