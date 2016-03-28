using MacomberMapCommunications.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapCommunications.Messages.EMS
{
    /// <summary>
    /// This class holds information on a unit's control parameters
    /// </summary>
    [RetrievalCommand("GetUnitControlStatus"), UpdateCommand("UpdateUnitControlStatus"), FileName("UnitControlStatus.csv"), RemovalCommand("","TEID"), DoNotRemove]
    public class MM_Unit_Control_Status
    {
        #region Variable declarations
        /// <summary>The TEID of our unit</summary>
        public int TEID { get; set; }

        /// <summary>Start ramping MW</summary>
        public bool StartRamping { get; set; }

        /// <summary>Start the unit on frequency control</summary>
        public bool StartFrequencyControl { get; set; }

        /// <summary>Show Isochronous frequency control</summary>
        public bool CheckedFrequencyControl { get; set; }

        /// <summary>Whether the current user is currently the owner of this control</summary>        
        [IgnoreDataMember, NonSerialized]
        public bool IsOwner = false;

        /// <summary>Track the opened windows for this unit</summary>
        public string UnitController { get; set; }
        
        /// <summary>Whether the unit is online</summary>
        public bool Online { get; set; }

        /// <summary>Whether the unit is in automatic control</summary>
        public bool InAGC { get; set; }

        /// <summary>Our Base RPM, which is current RPM with acceleration but not our sinusoidal oscillation</summary>
        public double BaseRPM { get; set; }

        /// <summary>Our Base Voltage</summary>
        public double BaseVoltage { get; set; }

        /// <summary>The timestamp for when ownership starts</summary>
        public DateTime OwnershipStart { get; set; }

        /// <summary>When our panel was opened</summary>
        public DateTime OpenTime { get; set; }

        /// <summary>The status of our unit</summary>
        public enumUnitStatus UnitStatus { get; set; }

        /// <summary>
        /// Our unit status enumeration
        /// </summary>
        public enum enumUnitStatus
        {
            /// <summary>Unit is offline</summary>
            Offline,
            /// <summary>Unit tripped offline</summary>
            Tripped,
            /// <summary>Unit is online</summary>
            Online,
            /// <summary>Unit has started</summary>
            Started,
            /// <summary>Unit is unavailable</summary>
            Unavailable
        }
        #endregion

        /// <summary>
        /// Initialize our unit control status
        /// </summary>
        public MM_Unit_Control_Status()
        {
            this.OpenTime = DateTime.Now;
            this.UnitStatus = enumUnitStatus.Offline;
        }
    }
}
