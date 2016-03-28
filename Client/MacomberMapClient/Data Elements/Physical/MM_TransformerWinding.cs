using MacomberMapClient.Integration;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MacomberMapClient.Data_Elements.Physical
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class holds the transformer winding information
    /// </summary>
    public class MM_TransformerWinding : MM_Element
    {
        #region Variable declarations
        /// <summary>The transformer with which the winding is connected</summary>
        public MM_Transformer Transformer;

        /// <summary>Estimated MW flow</summary>
        public float Estimated_MW = float.NaN;

        /// <summary>Telemetered MW flow</summary>
        public float Telemetered_MW = float.NaN;

        /// <summary>Estimated MVAR flow</summary>
        public float Estimated_MVAR = float.NaN;

        /// <summary>Estimated MVA flow</summary>
        public float Estimated_MVA = float.NaN;

        /// <summary>Telemetered MVA flow</summary>
        public float Telemetered_MVA = float.NaN;

        /// <summary>Telemetered MVAR flow</summary>
        public float Telemetered_MVAR = float.NaN;

        /// <summary>The transformer tap position</summary>
        public float Tap;

        /// <summary>The nominal voltage on the winding</summary>
        public float Voltage;

        /// <summary>The name of the node associated with the winding</summary>
        public String NodeName = "";

        /// <summary>The TEID of the node  assocaited with our transformer winding</summary>
        public int WindingNodeTEID;

        /// <summary>Our collection of winding types</summary>
        public enum enumWindingType
        {
            /// <summary>Unknown type</summary>
            Unknown,

            /// <summary>Primary winding</summary>
            Primary,

            /// <summary>Secondary winding</summary>
            Secondary
        }

        /// <summary>Our current winding type</summary>
        public enumWindingType WindingType = enumWindingType.Unknown;
        #endregion

        #region Initialization
        /// <summary>Initialize a new transformer winding</summary>
        public MM_TransformerWinding() : base() { this.ElemType = MM_Repository.FindElementType("TransformerWinding"); }

        /// <summary>Initialize a new transformer winding</summary>
        public MM_TransformerWinding(XmlElement ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
            this.ElemType = MM_Repository.FindElementType("TransformerWinding");
        }

        /// <summary>Initialize a new transformer winding</summary>
        public MM_TransformerWinding(DbDataReader ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
            this.ElemType = MM_Repository.FindElementType("TransformerWinding");
        }
        #endregion
    }
}