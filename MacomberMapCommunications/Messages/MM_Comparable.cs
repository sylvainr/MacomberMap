using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapCommunications.Messages
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides a means by which same classes can be compared to each other
    /// </summary>
    public static class MM_Comparable
    {
        /// <summary>
        /// Compare two objects to determine if they're identical
        /// </summary>
        /// <param name="Obj1"></param>
        /// <param name="Obj2"></param>
        /// <returns></returns>
        public static bool IsDataIdentical(Object Obj1, Object Obj2)
        {
            foreach (PropertyInfo pI in Obj1.GetType().GetProperties())
            {
                Object obj1 = Obj1 == null ? null : pI.GetValue(Obj1);
                Object obj2 = Obj2 == null ? null : pI.GetValue(Obj2);
                if (obj1 == null && obj2 != null)
                    return false;
                else if (obj1 != null && obj2 == null)
                    return false;
                else if (obj1 != null && obj2 != null && !obj1.Equals(obj2))
                    return false;
            }
            return true;
        }
    }
}
