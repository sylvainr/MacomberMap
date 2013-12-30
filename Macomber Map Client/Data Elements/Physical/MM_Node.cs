using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Data.Common;

namespace Macomber_Map.Data_Elements
{
    /// <summary>
    /// This class holds information on a connectivity node
    /// </summary>
    public class MM_Node : MM_Element
    {
        #region Variable declarations
        /// <summary>The elements to which the node is connected</summary>
        private MM_Element[] _ConnectedElements;

        /// <summary>The elements to which the node is connected</summary>
        public MM_Element[] ConnectedElements
        {
            get { return _ConnectedElements; }
            set { _ConnectedElements = value; }
        }

        /// <summary>The busbar section corresponding to the node</summary>
        private MM_BusbarSection _BusbarSection = null;

        /// <summary>
        /// The busbar section corresponding to the node
        /// </summary>
        public MM_BusbarSection BusbarSection
        {
            get { return _BusbarSection; }
            set { _BusbarSection = value; }
        }

        /// <summary>
        /// THe estimated angle of the bus
        /// </summary>
        public float Estimated_Angle
        {
            get
            {
                if (_BusbarSection != null)
                    return _BusbarSection.Estimated_Angle;
                else
                    return float.NaN;
            }
            set
            {
                if (_BusbarSection != null)
                    _BusbarSection.Estimated_Angle = value;
            }
        }
       

        /// <summary>Whether the node is dead</summary>
        public bool Dead= false;


        /// <summary>Whether the node is open</summary>
        public bool Open = false;

        /// <summary>
        /// The estimated KV of the bus
        /// </summary>
        public float Estimated_kV
        {
            get
            {
                if (_BusbarSection != null)
                    return _BusbarSection.Estimated_kV;
                else
                    return float.NaN;
            }
            set
            {
                if (_BusbarSection != null)
                    _BusbarSection.Estimated_kV = value;
            }
        }

        /// <summary>
        /// The estimated KV of the bus
        /// </summary>
        public float PerUnitVoltage
        {
            get
            {
                if (_BusbarSection != null)
                    return _BusbarSection.PerUnitVoltage;
                else
                    return float.NaN;
            }
            set
            {
                if (_BusbarSection != null)
                    _BusbarSection.PerUnitVoltage = value;
            }
        }



        /// <summary>
        /// The island number associated with the bus
        /// </summary>
        public int IslandNumber
        {
            get
            {
                if (_BusbarSection != null)
                    return _BusbarSection.IslandNumber;
                else
                    return -1;
            }
            set
            {
                if (_BusbarSection == null)
                {
                    _BusbarSection = new MM_BusbarSection();
                    _BusbarSection.Substation = Substation;
                    _BusbarSection.Name = Name;
                    _BusbarSection.Owner = Owner;
                    _BusbarSection.Operator = Operator;
                    _BusbarSection.BusNumber = BusNumber;
                    _BusbarSection.TEID = Macomber_Map.Data_Connections.Data_Integration.GetTEID();
                    _BusbarSection.KVLevel = KVLevel;
                    MM_Repository.TEIDs.Add(_BusbarSection.TEID, _BusbarSection);
                    _BusbarSection.Node = this;
                    MM_Repository.BusNumbers.Add(BusNumber, _BusbarSection);
                }
                _BusbarSection.IslandNumber = value;
            }
        }

    

        /// <summary>
        /// The low normal limit on voltage
        /// </summary>
        public float LowNormalLimit
        {
            get { return _BusbarSection == null ? float.NaN : _BusbarSection.Limits[1]; }
            set { _BusbarSection.Limits[1] = value; }
        }

        /// <summary>
        /// The low emergency limit on voltage
        /// </summary>
        public float LowEmergencyLimit
        {
            get { return _BusbarSection == null ? float.NaN : _BusbarSection.Limits[0]; }
            set { _BusbarSection.Limits[0] = value; }
        }


        /// <summary>
        /// The high normal limit on voltage
        /// </summary>
        public float HighNormalLimit
        {
            get { return _BusbarSection == null ? float.NaN : _BusbarSection.Limits[2]; }
            set { _BusbarSection.Limits[2] = value; }
        }

        /// <summary>
        /// The high emergency limit on voltage
        /// </summary>
        public float HighEmergencyLimit
        {
            get { return _BusbarSection == null ? float.NaN : _BusbarSection.Limits[3]; }
            set { _BusbarSection.Limits[3] = value; }
        }



        /// <summary>The electrical bus corresponding to the node</summary>
        private MM_Electrical_Bus _ElectricalBus = null;

        /// <summary>
        /// The electrical bus corresponding to the node
        /// </summary>
        public MM_Electrical_Bus ElectricalBus
        {
            get { return _ElectricalBus; }
            set { _ElectricalBus = value; }
        }
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
            ConnectedElements = new MM_Element[splStr.Length];
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
            ConnectedElements = new MM_Element[splStr.Length];
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