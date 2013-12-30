using System;
using System.Collections.Generic;
using System.Text;

namespace Macomber_Map.Data_Elements
{
    /// <summary>
    /// This class is responsible for performing core conversions
    /// </summary>
    public static class MM_Converter
    {
        /// <summary>
        /// Retrieve a single-precision number, first checking for NaN
        /// </summary>
        /// <param name="Incoming">The incoming value</param>
        /// <returns></returns>
        public static Single ToSingle(Object Incoming)
        {
            if (Incoming is DBNull)
                return float.NaN;
            else if (Incoming is Single)
                return (Single)Incoming;
            else if (Incoming is Decimal)
                return Convert.ToSingle(Incoming);
            else if ((String)Incoming == "NaN")
                return float.NaN;
            else
                return Convert.ToSingle((String)Incoming);
        }

        /// <summary>
        /// Convert an object to a Int32 (needed for TEIDs) if possible
        /// </summary>
        /// <param name="InValue">Our incoming value</param>
        /// <param name="OutVal">Our outgoing value</param>
        /// <returns>Whether the import was succesful</returns>
        public static bool ToInt32(object InValue, out Int32 OutVal)
        {
            if (InValue is Int32)
            {
                OutVal = Convert.ToInt32(InValue);
                return true;
            }
            else if (InValue is Decimal)
            {
                OutVal = Convert.ToInt32(InValue);
                return true;
            }
            else
            {
                OutVal = 0;
                return false;
            }
            
        }
    }
}
