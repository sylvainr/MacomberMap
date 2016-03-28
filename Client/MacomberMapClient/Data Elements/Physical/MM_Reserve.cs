using MacomberMapClient.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace MacomberMapClient.Data_Elements.Physical
{
    /// <summary>
    /// This class holds information on an MM Reserve element
    /// </summary>
    public class MM_Reserve : MM_Serializable
    {
        #region Variable declarations
        /// <summary>The category for this reserve</summary>
        public String Category;

        /// <summary>The title of this element</summary>
        public String Name;

        /// <summary>The field for this value in the EMS system</summary>
        public String EMSValue;

        /// <summary>The display item for this element</summary>
        public ListViewItem DisplayItem = null;

        /// <summary>The current value for our element</summary>
        public float Value = float.NaN;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new reserve element
        /// </summary>
        /// <param name="xElem"></param>
        /// <param name="Parent"></param>
        /// <param name="AddIfNew"></param>
        public MM_Reserve(XmlElement xElem, ListView Parent, bool AddIfNew)
            : base(xElem, AddIfNew)
        {

        }

        /// <summary>
        /// Initialize a new reserve element
        /// </summary>
        /// <param name="xElem"></param>
        /// <param name="AddIfNew"></param>
        public MM_Reserve(XmlElement xElem, bool AddIfNew)
            : base(xElem, AddIfNew)
        {

        }

        /// <summary>
        /// Initialize a new reserve element
        /// </summary>
        /// <param name="Category"></param>
        /// <param name="Name"></param>
        public MM_Reserve(String Category, string Name)
        {
            this.Name = Name;
            this.Category = Category;
        }
        #endregion

        /// <summary>
        /// Create a listview item for our element
        /// </summary>
        /// <param name="Parent"></param>
        public void CreateDisplayItem(ListView Parent)
        {
            DisplayItem = Parent.Items.Add(Name);
            DisplayItem.Tag = this;
            DisplayItem.Group = Parent.Groups[Category];
            if (DisplayItem.Group == null)
                DisplayItem.Group = Parent.Groups.Add(Category, Category);
            DisplayItem.SubItems.Add(Value.ToString("#,##0") + " mw");
        }

        /// <summary>        
        /// Update our value for this reserve item
        /// </summary>
        /// <param name="InValue"></param>
        public void UpdateValue(Object InValue)
        {
            if (InValue == null || InValue is DBNull)
                Value = float.NaN;
            else
                Value = Convert.ToSingle(InValue);
        }


    }
}
