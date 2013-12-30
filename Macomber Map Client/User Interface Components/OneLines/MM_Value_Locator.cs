using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Macomber_Map.User_Interface_Components.OneLines
{
    /// <summary>
    /// This structure holds information on a value locator for an element, by attribute and index
    /// </summary>
    public struct MM_Value_Locator : IComparable, IComparable<MM_Value_Locator>, IEquatable<MM_Value_Locator>
    {
        #region Variable declarations
        /// <summary>The attribute associated with our element</summary>
        public MM_Communication.Elements.MM_Element.enumAttributes Attribute;

        /// <summary>The index of our attribute (e.g., first/second substation for a line, voltage for a winding)</summary>
        public byte Index;

        /// <summary>The attribute associated with our element</summary>
        public MM_Communication.Elements.MM_Element.enumValueType ValueType;
        #endregion

        #region Initialization
       
        /// <summary>
        /// Intiialize a new value locator
        /// </summary>
        /// <param name="Attribute"></param>
        /// <param name="Index"></param>
        /// <param name="ValueType"></param>
        public MM_Value_Locator(MM_Communication.Elements.MM_Element.enumAttributes Attribute, byte Index, MM_Communication.Elements.MM_Element.enumValueType ValueType)
        {
            this.Attribute = Attribute;
            this.Index = Index;
            this.ValueType = ValueType;
        }
        #endregion


        #region Comparisons
        /// <summary>
        /// Determine whether two value locators are equal
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(MM_Value_Locator other)
        {
            return ValueType.Equals(other.ValueType) && Attribute.Equals(other.Attribute) && Index.Equals(other.Index);
        }

        /// <summary>
        /// Determine whether two value locators are equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            MM_Value_Locator other = (MM_Value_Locator)obj;
            return ValueType.Equals(other.ValueType) && Attribute.Equals(other.Attribute) && Index.Equals(other.Attribute);
        }


        /// <summary>
        /// Compare two element types
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(MM_Value_Locator other)
        {
            int Diff = ValueType.CompareTo(other.ValueType);
            if (Diff != 0)
                return Diff;

            Diff = Attribute.CompareTo(other.Attribute);
            if (Diff != 0)
                return Diff;
            else
                return Index.CompareTo(other.Index);
        }

        /// <summary>
        /// Compare two value locators
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            MM_Value_Locator other = (MM_Value_Locator)obj;
            int Diff = ValueType.CompareTo(other.ValueType);
            if (Diff != 0)
                return Diff;
            Diff = Attribute.CompareTo(other.Attribute);
            if (Diff != 0)
                return Diff;
            else
                return Index.CompareTo(other.Index);   
        }


        /// <summary>
        /// Report a hash code for our item
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return ValueType.GetHashCode() + Attribute.GetHashCode() + Index.GetHashCode();
        }

        /// <summary>
        /// Report a human-readable string for our element
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ValueType.ToString() + " " + Attribute.ToString() + " " + Index.ToString();
        }
        #endregion
    }
}
