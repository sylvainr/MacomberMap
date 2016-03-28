using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEDESimulator.Elements
{
    /// <summary>
    /// This class provides generic classes to our element
    /// </summary>
    public interface Sim_Element
    {
        /// <summary>
        /// Get the XML for our component
        /// </summary>
        /// <returns></returns>
         String GetXml(Sim_Builder Builder);

        /// <summary>
        /// Get our collection of buses
        /// </summary>
        /// <returns></returns>
        Sim_Bus[] GetBuses();

        String GetOneLineXml(ref int XPos, int YPos);
       
        String ElemType { get;}

        Guid ElemGuid { get; set; }

        int TEID { get; set; }
    }
}
