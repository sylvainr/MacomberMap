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
    /// This class holds information on a connectivity node
    /// </summary>
    public class MM_Node : MM_Element
    {
        #region Variable declarations
        /// <summary>The elements to which the node is connected</summary>
        private List<MM_Element> _ConnectedElements;

        /// <summary>The elements to which the node is connected</summary>
        public List<MM_Element> ConnectedElements
        {
            get {
                if (_ConnectedElements == null)
                    _ConnectedElements = new List<MM_Element>();
                return _ConnectedElements; 
            }
            set { _ConnectedElements = value; }
        }

        /// <summary>The bus linked to the node, accdording to CIM</summary>
        public MM_Bus AssociatedBus;
       
        /// <summary>The electrical bus corresponding to the node</summary>
        private MM_Electrical_Bus _ElectricalBus = null;

        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new node from a data reader
        /// </summary>
        /// <param name="ElementSource"></param>
        /// <param name="AddIfNew"></param>
        public MM_Node(DbDataReader ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
            this.ElemType = MM_Repository.FindElementType("Node");
            String[] splStr = ((string)ElementSource["ConnectedElements"]).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            ConnectedElements = new List<MM_Element>(splStr.Length);
            for (int a = 0; a < splStr.Length; a++)
                ConnectedElements[a] = (MM_Element)MM_Serializable.RetrieveConvertedValue(typeof(MM_Element), splStr[a], this, AddIfNew);
        }

        /// <summary>
        /// Initialize a new node from an XML document
        /// </summary>
        /// <param name="ElementSource"></param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Node(XmlElement ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
            this.ElemType = MM_Repository.FindElementType("Node");
            String[] splStr = (ElementSource.Attributes["ConnectedElements"]).Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            ConnectedElements = new List<MM_Element>(splStr.Length);
            for (int a = 0; a < splStr.Length; a++)
                ConnectedElements[a] = (MM_Element)MM_Serializable.RetrieveConvertedValue(typeof(MM_Element), splStr[a], this, AddIfNew);
        }

        /// <summary>
        /// Initialize a parameterless node
        /// </summary>
        public MM_Node()
            : base()
        {
            this.ElemType = MM_Repository.FindElementType("Node");
        }
        #endregion
    }
}