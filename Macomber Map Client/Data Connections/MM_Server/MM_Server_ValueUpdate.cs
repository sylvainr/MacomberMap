using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Macomber_Map.Data_Connections.MM_Server
{
    /// <summary>
    /// This class holds information on a value update
    /// </summary>
    [Serializable, ComVisible(true)]
    public struct MM_Server_ValueUpdate
    {
        /// <summary>Our attribute</summary>
        public MM_Communication.Elements.MM_Element.enumAttributes Attribute;

        /// <summary>The index of our data point</summary>
        public Byte Index;

        /// <summary>Our incoming value</summary>
        public Object InValue;

        /// <summary>
        /// Initialize a new value update message
        /// </summary>
        /// <param name="Attribute"></param>
        /// <param name="Index"></param>
        /// <param name="InValue"></param>
        public MM_Server_ValueUpdate(MM_Communication.Elements.MM_Element.enumAttributes Attribute, Byte Index, Object InValue)
        {
            this.Attribute = Attribute;
            this.Index = Index;
            this.InValue = InValue;
        }

        /// <summary>
        /// Report an easy-to-read identifier for our element
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Attribute.ToString() + "-" + Index.ToString() + "-" + (InValue == null ? "(null)" : InValue.ToString());
        }
    }
}
