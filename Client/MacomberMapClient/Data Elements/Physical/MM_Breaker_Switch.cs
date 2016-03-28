using MacomberMapCommunications.Messages.EMS;
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
    /// This class holds information on a breaker, including its open/closed state
    /// </summary>
    public class MM_Breaker_Switch : MM_Element
    {
        #region Variable declarations
        /// <summary>The current state of the breaker</summary>
        public BreakerStateEnum BreakerState = BreakerStateEnum.Unknown;

        /// <summary>Whether the breaker has a syncrhoscope</summary>
        public bool HasSynchroscope;

        /// <summary>Whether the breaker has a synchrocheck relay</summary>
        public bool HasSynchrocheckRelay;

        /// <summary>Whether the breaker or switch is normally opened</summary>
        public bool NormalOpen;

        /// <summary>Whether the breaker is in a good or bad state</summary>
        public bool Good;

        /// <summary>Our minimum synchroscope bounds</summary>
        public float MinSynch;

        /// <summary>Our maximum synchroscope bounds</summary>
        public float MaxSynch;

        /// <summary>The possible states of the breaker</summary>
        public enum BreakerStateEnum
        {
            /// <summary>The breaker is closed</summary>
            Closed,
            /// <summary>The breaker is opened</summary>
            Open,
            /// <summary>The breaker's state is unknown</summary>
            Unknown
        };

        /// <summary>
        /// Whether the breaker/switch is opened
        /// </summary>
        public bool Open
        {
            get { return BreakerState == BreakerStateEnum.Open; }
            set { BreakerState = value ? BreakerStateEnum.Open : BreakerStateEnum.Closed; }
        }

        /// <summary>Our synchroscope data, if any</summary>
        public MM_Synchroscope_Data Synchroscope = null;

       
        
        
        #endregion

        /// <summary>
        /// Initialize a new breaker
        /// </summary>
        public MM_Breaker_Switch()
            : base()
        {
        }

        /// <summary>
        /// Initialize a new CIM Breaker or switch
        /// </summary>
        /// <param name="ElementSource">The XML source for this line</param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Breaker_Switch(XmlElement ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        { }

        /// <summary>
        /// Initialize a breaker or switch from CIM Server SQL
        /// </summary>
        /// <param name="ElementSource"></param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Breaker_Switch(DbDataReader ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
            this.BreakerState = BreakerStateEnum.Unknown;
        }

    }
}
