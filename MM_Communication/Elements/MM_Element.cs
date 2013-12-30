using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MM_Communication.Elements
{
    /// <summary>
    /// (C) 2012, Electric Reliability Council of Texas, Inc.
    /// This class holds information on an element, including those to which it is linked
    /// </summary>
    public class MM_Element
    {
        #region Enumerations
        /// <summary>The collection of element types</summary>
        public enum enumElementType:byte
        {
            Unknown,
            ACE_SystemWide,
            SystemWide,
            CSCAndIROL,
            VoltageLevel,
            Company,
            Substation,       
            EndCap,                 
            Line,
            ZBR,
            Transformer,
            TransformerWinding,
            Node,
            Capacitor,
            Reactor,
            ShuntCompensator,
            SeriesCompensator,
            StaticVarCompensator,
            Breaker,
            BusbarSection,
            Switch,
            ElectricalBus,
            Load,
            LoadForecast,
            LaaR,
            DCTie,
            LoadZone,
            FODTransformers,
            FODUnits,
            FODLines,
            FODViolations,
            WeatherZone,
            County,
            State,
            Unit,
            LogicalUnit,
            CombinedCycleConfiguration,
            RemedialActionScheme,
            RAP,
            SPS,
            TStamp,
            ICCPLinkage,
            ISDLinkage,
            Contingency,
            BasecaseBranchViolation,
            BasecaseViolations,
            BasecaseHighVoltageViolation,
            BasecaseLowVoltageViolation,
            ContingencyBranchViolations,
            ContingencyVoltageViolation,
            ForcedOutage,
            ForcedExtension,
            UnavoidableExtension,
            Note,
            Message,
            GenerationType,
            Island,
            Island_Completed,
            SystemInformation,
            Blackstart_Corridor,
            Blackstart_Target,
            Blackstart_Element,
            PricingVector,
            EPSMeter, 
            KVLevel,
            OneLine,
            IslandOTS,
            ControlPanel,
            UnitOTS
        }

        /// <summary>The GUID for blackstart corridors</summary>
        public static Guid BlackstartCorridorGuid = new Guid("{F2ACC59B-78BE-4DED-909B-2DBDB9BE5FAE}");

        /// <summary>
        /// The collection of value types for incoming data
        /// </summary>
        public enum enumValueType : byte
        {
            Unknown = 0,
            Telemetered=1,
            RealTime=2,
            Study=3
        }
            

        /// <summary>
        /// The collection of data linkages/attributes associated with a reference
        /// </summary>
        public enum enumAttributes : byte
        {
            Unknown,
            F3__CTVL,
            TYPE,
            MONELM,
            PERCLIM1,
            PERCLIM2,
            PERCLIM3,
            TIMIN,
            TSTVIOL, //
            TWSVIOL, //
            WSVIOL, //
            BASE1,
            BASE2,
            BASE3,
            VIOLTYP,
            LIMIT1, //
            LIMIT2, //
            LIMIT3, //
            DISPLAY, //
            SUB, //
            WARNVIOL,//
            PRECTG,
            CURVAL,
            NOMKV,
            NLB__CTVL,
            STID,
            UNID,
            EQID1,
            EQID2,
            OTMW,
            TSTART,
            TEND,
            TMPLST,
            TMPLED,
            ACTIVE,
            NEW,
            OSTYP,
            OTTYP2,
            OTTYP3,
            NMLOPEN,
            ACK,
            ALARM,
            FORCED,
            FORCEEXT,
            NEWOUT,
            OUTELEM,
            UNAVOEXT,
            MW,
            MW_Integrated,
            MWINMX,
            MWOUTMX,
            MWINT,
            MWINTLH,
            F__SAMPLE,
            ACE,
            FHZ,
            Timestamp,
            KeyIndx,
            ID,
            RTOTMW,
            LTOTMW,
            LD,
            GEN,
            MWDISTL,
            DBAND,
            REGUL,
            REGULI,
            ASS,
            EME,
            PRCT,
            IBPTCAPT,
            DBPTCAPT,
            HASLT,
            HSLT,
            HDLT,
            RGUASST,
            RGDASST,
            RVRRCPT,
            RVNSPT,
            RVSP,
            RVBLKS,
            RVRMRN,
            TAGCLAST,
            TEID,
            Estimated_MVA,
            Estimated_MW,
            Estimated_MVAR,
            Nominal_MVAR,
            Telemetered_MVA,
            Telemetered_MW,
            Telemetered_MVAR,
            Estimated_kV,
            Telemetered_kV,
            Telemetered_Angle,
            Estimated_Angle,
            PerUnitVoltage,
            NormalLimit,
            EmergencyLimit,
            LoadshedLimit,
            LowNormalLimit,
            LowEmergencyLimit,
            HighNormalLimit,
            HighEmergencyLimit,
            Owner,
            Operator,
            Longitude,
            Latitude,
            Communication_Linkage,
            Contingency,
            Violation,
            Note,
            KVLevel,
            RASs,
            Substation,
            Substation1,
            Substation2,
            Substations,
            ConnectedStations,
            Transformers,
            Contingencies,
            Elements,
            AssociatedLine,
            LineTEID,
            IsSeriesCompensator,
            IsMultipleSegment,
            ShuntCompensators,
            BusbarSections,
            Loads,
            Units,
            LatLong,
            KVLevels,
            KVLevel1,
            KVLevel2,
            ElemTypes,
            ViolatedElement,
            Description,
            Dead,
            Name,
            LongName,
            WindingName,
            Winding1,
            Winding2,
            Windings,
            Transformer,
            New,
            PreContingencyValue,
            PostContingencyValue,
            EventTime,
            Min,
            Min_X,
            Min_Y,
            Max,
            Max_X,
            Max_Y,
            Centroid,
            Centroid_X,
            Centroid_Y,
            Coordinates,
            Website,
            WeatherStation,
            WeatherForecast,
            HasSynchroscope,
            BreakerSwitchState,
            ScheduledOutage,
            AssociatedNode,
            Unit,
            UnitConfiguration,
            IsCritical,
            PrimaryPhone,
            SecondaryPhone,
            DUNS,
            County,
            IsActive,
            Load,
            Integrated_MW,
            Monitored_MW,
            Line,
            IsPhysical,
            Current,
            Boundary,
            ElectricalBus,
            Node,
            BusNumber,
            BusbarSection,
            Author,
            OutageStart,
            OutageEnd,
            Open,
            FrequencyControlStatus,
            FrequencyControl,
            PhaseShifter,
            Estimated_Tap,
            Telemetered_Tap,
            GenerationType,
            SCED_Basepoint,
            LMP,
            SPP,
            HASL,
            HEL,
            HSL,
            HDL,
            LDL,
            LSL,
            LEL,
            LASL,
            Spinning_Capacity,
            Physical_Responsive_Online,
            Physical_Responsive_Sync,
            Blackstart_Capacity,
            RMR_Capacity,
            LogicalUnit,
            LogicalUnits,
            PhysicalUnit,
            CombinedCycleConfiguration,
            IslandFrequencyControl,
            Island,
            Frequency,
            UnitType,
            UnitTEID,
            IsRC,
            Configurations,
            Blackstart_Corridor,
            Alias,
            PUNElement,
            IsZBR,
            Notes,
            ElemType,
            Nominal,
            EnergizedForeColor,
            Primary,
            Secondary,
            Target,
            Blackstart_Target,
            Blackstart_Targets,
            Action,
            AssociatedElement,
            Good,
            SDIS,
            OnlineCapacity,
            Generation,
            MWLosses,
            External,
            Island_Load,
            InProg,
            STName,
            IslandFreqCtrl,
            CTG_ID
        }

        /// <summary>The possible states of the breaker</summary>
        public enum enumBreakerSwitchState: byte
        { 
            /// <summary>The breaker/switch is closed</summary>
            Closed,
            /// <summary>The breaker/switch is opened</summary>
            Open,
            /// <summary>The breaker/switch state is unknown</summary>
            Unknown 
        };

        /// <summary>The collection of possible element activities</summary>
        public enum enumBlackstartElementActivity: byte
        {
            /// <summary>Energize a unit</summary>
            EnergizeUnit,
            /// <summary>Close a breaker</summary>
            Close_Breaker,
            /// <summary>Energize a line</summary>
            EnergizeLine,
            /// <summary>Reach a synchronization point</summary>
            ReachSynchPoint,
            /// <summary>Close a switch</summary>
            Close_Switch
        }

        /// <summary>
        /// The frequency control status of an island
        /// </summary>
        public enum enumFrequencyControlStatus: byte
        {
            /// <summary>No frequency control</summary>
            None = 0,
            /// <summary>Unit on frequency control</summary>
            Unit = 1,
            /// <summary>Island on frequency control</summary>
            Island = 2
        }

        #endregion


        #region Event indicators
        /// <summary>
        /// A thread-safe delegate to handle event invocation of value changes
        /// </summary>
        /// <param name="Element"></param>
        /// <param name="AttributeType"></param>        
        /// <param name="UpdatedValue"></param>
        /// <param name="OldValue"></param>
        /// <param name="Index"></param>
        public delegate void ValueChangedDelegate(MM_Element Element, enumAttributes AttributeType, byte Index, object UpdatedValue, object OldValue);

        /// <summary>The event trigger for a value change</summary>
        public event ValueChangedDelegate ValueChanged;
        #endregion

        #region Variable declarations
        /// <summary>The TEID of our element</summary>
        public Int32 TEID;
     
        /// <summary>The element type shown</summary>
        public enumElementType ElemType;

        #endregion

        #region Identification
        /// <summary>
        /// Report the TEID of our element
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ElemType.ToString() + " " + TEID.ToString("#,##0");
        }
        #endregion

        /// <summary>
        /// Indicate a value change to the user
        /// </summary>
        /// <param name="Attribute">The attribute that has changed</param>
        /// <param name="NewValue">The current value</param>
        /// <param name="CurVal">The prior value</param>
        public void TriggerValueChange(enumAttributes Attribute, byte Index, object NewValue, object CurVal)
        {
            
                if (ValueChanged != null)
                    ValueChanged(this, Attribute, Index, NewValue, CurVal);
            
        }
    }
}
