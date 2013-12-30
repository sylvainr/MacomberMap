using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using System.Xml;
using Macomber_Map.Data_Connections;

namespace Macomber_Map.Data_Elements
{
    /// <summary>
    /// This class defines a generator/unit
    /// </summary>
    public class MM_Unit : MM_Element, IComparable
    {
        #region Variable declarations
        /// <summary>The type of generating unit this is</summary>
        public MM_Generation_Type UnitType;

        /// <summary>The next scheduled outage, if any</summary>
        public MM_ScheduledOutage UpcomingOutage=null;

        /// <summary>SCED base point</summary>       
        public float SCED_Basepoint = float.NaN;

        /// <summary>Locational marginal price</summary>
        public float LMP = float.NaN;

        /// <summary>The maximal capacity of the unit</summary>
        public float MaxCapacity = float.NaN;

        /// <summary>Settlement point price</summary>
        public float SPP = float.NaN;

        /// <summary>High ancillary service limit</summary>
        public float HASL = float.NaN;

        /// <summary>High emergency limit </summary>
        public float HEL = float.NaN;

        /// <summary>High sustained limit</summary>
        public float HSL = float.NaN;

        /// <summary>High dispatch limit</summary>
        public float HDL = float.NaN;

        /// <summary>Low ancillary service limit</summary>        
        public float LASL = float.NaN;

        /// <summary>Low emergency limit</summary>
        public float LEL = float.NaN;
        
        /// <summary>Low sustained limit</summary>
        public float LSL = float.NaN;

        /// <summary>Low dispatch limit</summary>
        public float LDL = float.NaN;

        /// <summary>Estimated MW flow</summary>
        public float Estimated_MW = float.NaN;

        /// <summary>Estimated MVAR flow</summary>
        public float Estimated_MVAR = float.NaN;

        /// <summary>Estimated MVA flow</summary>
        public float Estimated_MVA = float.NaN;

        /// <summary>Telemetered MW flow</summary>
        public float Telemetered_MW = float.NaN;

        /// <summary>Telemetered MVAR flow</summary>
        public float Telemetered_MVAR = float.NaN;

        /// <summary>The derived telemetered MVA of the load</summary>
        public float Telemetered_MVA = float.NaN;

        /// <summary>The spinning capacity of the unit</summary>
        public float Spinning_Capacity = float.NaN;

        /// <summary>The online PRC of the unit</summary>
        public float Physical_Responsive_Online = float.NaN;

        /// <summary>The PR sync</summary>
        public float Physical_Responsive_Sync = float.NaN;

        /// <summary>The blackstart capacity of the unit</summary>
        public float Blackstart_Capacity = float.NaN;

        /// <summary>The RMR capacity of the unit</summary>
        public float RMR_Capacity = float.NaN;

        /// <summary>Our collection of logical units</summary>
        public MM_Unit[] LogicalUnits = new MM_LogicalUnit[0];

        /// <summary>Determine whether the unit is physical</summary>
        public bool IsPhysical
        {
            get { return this.GetType().Name == "MM_Unit"; }
        }

        /// <summary>Whether the unit is linked to a combined cycle unit</summary>
        public bool IsRC;

        /// <summary>The TEID of the physical unit</summary>
        public Int32 UnitTEID;

        /// <summary>Whether the unit is on frequency control</summary>
        public bool FrequencyControl = false;

        /// <summary>Whether the unit's island is on frequency control</summary>
        public bool IslandFreqCtrl = false;
       
        /// <summary>The unit's frequency</summary>
        public float Frequency = 60;
        #endregion

        
        #region Initialization
        /// <summary>
        /// Initialize a new CIM Unit
        /// </summary>
        public MM_Unit()
        {
            this.ElemType = MM_Repository.FindElementType("Unit");
            if (this.UnitType == null)
                this.UnitType = MM_Repository.FindGenerationType("UNKNOWN");
        }
        

         /// <summary>
        /// Initialize a new CIM Transformer
        /// </summary>
        /// <param name="ElementSource">The XML source for this line</param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Unit(XmlElement ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
            this.ElemType = MM_Repository.FindElementType("Unit");
            if (this.UnitType == null)
                this.UnitType = MM_Repository.FindGenerationType("UNKNOWN");
        }

        /// <summary>
        /// Initialize a new CIM Unit
        /// </summary>
        /// <param name="ElementSource">The data source for this Unit</param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Unit(DbDataReader ElementSource, bool AddIfNew)
            : base(ElementSource,AddIfNew)
        {
            this.ElemType = MM_Repository.FindElementType("Unit");
            if (this.UnitType == null)
                this.UnitType = MM_Repository.FindGenerationType("UNKNOWN");
        }

        private void CheckElementValue(XmlElement ElementSource, string AttributeName, ref float OutValue)
        {
            if (!ElementSource.HasAttribute(AttributeName))
                return;
            OutValue = MM_Converter.ToSingle(ElementSource.Attributes[AttributeName].Value);
        }
        #endregion

      
        /// <summary>
        /// Return the percentage this unit is running at
        /// </summary>
        public String UnitPercentageText
        {
            get
            {
                if (float.IsNaN(HASL))
                    return "?";
                else if (Estimated_MW < HASL)
                    return (Estimated_MW / HASL).ToString("0%") + " HASL";
                else if (Estimated_MW < HSL)
                    return (Estimated_MW / HSL).ToString("0%") + " HSL";
                else if (Estimated_MW < HDL)
                    return (Estimated_MW / HDL).ToString("0%") + " HDL";
                else
                    return (Estimated_MW / HEL).ToString("0%") + " HEL";              
            }
        }

        /// <summary>
        /// Return the percentage this unit is loaded
        /// </summary>
        /// <param name="MW">The MW of the unit</param>        
        /// <returns></returns>
        public string UnitPercentageTextFromValues(float MW)
        {
            return (MW / HSL).ToString("0%") + " HSL";            
        }



        /// <summary>
        /// Return the percentage of the unit
        /// </summary>
        public float UnitPercentage
        {
            get
            {
                float Resp = Estimated_MW / HSL;
                return float.IsNaN(Resp) ? 0 : Resp;
            }
        }

        #region IComparable Members

        /// <summary>
        /// Compare two units
        /// </summary>
        /// <param name="Unit">The other unit</param>
        /// <returns></returns>
        public int CompareUnits(MM_Unit Unit)
        {
            if (Unit.TEID == TEID)
                return 0;
            else if (this.Estimated_MW == Unit.Estimated_MW)
                return -HSL.CompareTo(Unit.HSL);
            else
                return -Estimated_MW.CompareTo(Unit.Estimated_MW);
        }

        #endregion
    }
}
