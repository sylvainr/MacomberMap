using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapCommunications.Messages.EMS
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class holds information on a one-line
    /// </summary>
    public class MM_OneLine_Data
    {
        /// <summary>The name of our element (e.g., substation or contingency name)</summary>
        public String ElementName { get; set; }

        /// <summary>Our collection of element types</summary>
        public enum enumElementType
        {
            /// <summary>A substation one-line</summary>
            Substation,
            /// <summary>A breaker-to-breaker one-line</summary>
            BreakerToBreaker,
            /// <summary>A company-wide one-line</summary>
            CompanyWide
        }

        /// <summary>The type of element associated with our substation</summary>
        public enumElementType ElementType { get; set; }

        /// <summary>The XML of our one-line</summary>
        public string OneLineXml { get; set; }        
    }
}
