using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data.Common;

namespace Macomber_Map.Data_Elements
{
    /// <summary>
    /// (C) 2012, Michael E. Legatt, Ph.D.
    /// This class holds information on a remedial action scheme
    /// </summary>
    public class MM_RemedialActionScheme : MM_Element
    {
        #region Variable declarations
        /// <summary>The contingency associated with our remedial action scheme</summary>
        public MM_Contingency Contingency;

        /// <summary>The elements associated with our remedial action scheme</summary>
        public MM_Element[] Elements;

        /// <summary>
        /// The long name of the RAS
        /// </summary>
        public String Description;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new CIM Contingency Definition
        /// </summary>
        public MM_RemedialActionScheme()
        {
            this.ElemType = MM_Repository.FindElementType("RemedialActionScheme");
        }

        /// <summary>
        /// Initialize a new remedial action scheme against an XML element
        /// </summary>
        /// <param name="xElem"></param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_RemedialActionScheme(XmlElement xElem, bool AddIfNew)
            : base(xElem, AddIfNew)
        {
            this.ElemType = MM_Repository.FindElementType("RemedialActionScheme");
        }

        /// <summary>
        /// Intialize a new remedial action scheme against a database entry
        /// </summary>
        /// <param name="dRd"></param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_RemedialActionScheme(DbDataReader dRd, bool AddIfNew)
            : base(dRd, AddIfNew)
        {
            this.ElemType = MM_Repository.FindElementType("RemedialActionScheme");
        }
        #endregion

        /// <summary>
        /// Report the name of the contingency
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {

            return ElemType.Name + " " +  Name + " / " + Description;
        }
    }
}