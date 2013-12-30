using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Data.Common;

namespace Macomber_Map.Data_Elements
{
    /// <summary>
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