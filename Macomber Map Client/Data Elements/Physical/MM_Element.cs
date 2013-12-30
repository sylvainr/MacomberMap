using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Data.Common;
using System.Windows.Forms;
using Macomber_Map.Data_Connections;
using Macomber_Map.User_Interface_Components;
using System.Drawing;
using System.Data;
using System.Reflection;
using Macomber_Map.Data_Connections.Generic;
using System.Collections;

namespace Macomber_Map.Data_Elements
{
    /// <summary>
    /// This is the generic class for all CIM elements, which can be referenced by TEID. Every element can have violations.
    /// </summary>
    public class MM_Element: MM_Serializable, IComparable
    {
        #region Variable Declaration
        /// <summary>
        /// The TEID of the element
        /// </summary>
        public Int32 TEID;

        /// <summary>
        /// Our collection of mismatches, so we don't keep on flagging the user
        /// </summary>
        private static Dictionary<String, int> Mismatches = new Dictionary<string, int>();


        /// <summary>The name of the element</summary>
        public String Name=null;

        /// <summary>The owner of the company</summary>
        public MM_Company Owner = null;

        /// <summary>The operator of the company</summary>
        public MM_Company Operator = null;

        /// <summary>The name of the contingencies/breaker-to-breakers associated with the element</summary>
        public MM_Contingency[] Contingencies;

        /// <summary>The type of element this is</summary>
        public MM_Element_Type ElemType;

        /// <summary>Our collection of unknown elements that need to be adjusted</summary>
        public static Dictionary<Int32, Dictionary<MM_Element, MemberInfo>> UnknownElements = new Dictionary<int, Dictionary<MM_Element, MemberInfo>>();

        /// <summary>The bus number associated with the element</summary>
        public int BusNumber=-1;

        /// <summary>
        /// Report the bus associated with our element, if we can
        /// </summary>
        public MM_BusbarSection Bus
        {
            get
            {
                if (BusNumber == -1)
                    return null;
                MM_BusbarSection Bus = null;
                MM_Repository.BusNumbers.TryGetValue(BusNumber, out Bus);
                return Bus;
            }
        }

        /// <summary>Report the unit's island</summary>
        public MM_Island Island
        {
            get
            {
                MM_BusbarSection Bus = this as MM_BusbarSection;
                if (Bus == null && !MM_Repository.BusNumbers.TryGetValue(BusNumber, out Bus))
                    return null;

                MM_Island FoundIsland = null;
                MM_Repository.Islands.TryGetValue(Bus.IslandNumber, out FoundIsland);
                return FoundIsland;
            }
            set
            {               
                if (this is MM_Node)
                    (this as MM_Node).IslandNumber = value.ID;
                else if (this is MM_BusbarSection)
                { }
                else
                { }
            }
        }

        /// <summary>
        /// The collection of violations occuring within the element
        /// </summary>
        public Dictionary<MM_AlarmViolation, MM_AlarmViolation> Violations
        {
            get
            {
                if (_Violations == null)
                    return _Violations = new Dictionary<MM_AlarmViolation, MM_AlarmViolation>(10);
                else
                    return _Violations;
            }
        }

        private Dictionary<MM_AlarmViolation, MM_AlarmViolation> _Violations = null;
        /// <summary>
        /// The collection of notes occuring within the element
        /// </summary>
        public Dictionary<Int32, MM_Note> Notes
        {
            get
            {
                if (_Notes == null)
                    return _Notes = new Dictionary<Int32, MM_Note>(10);
                else
                    return _Notes;
            }
        }
        private Dictionary<Int32, MM_Note> _Notes = null;

        /// <summary>The KV level of the element</summary>
        public MM_KVLevel KVLevel;

        /// <summary>The substation in which the element resides (null for lines and substations)</summary>
        public MM_Substation Substation;

        /// <summary>The color for a non-active menu item</summary>
        public static Color MenuDeactivatedColor = Color.FromArgb(64, 64, 64);

        /// <summary>
        /// Report a value change in the element, leading to its being recomputed
        /// </summary>
        /// <param name="Element">The changed element</param>
        public delegate void ValuesChangedDelegate(MM_Element Element);

        /// <summary>Report a line's measurement change </summary>
        public event ValuesChangedDelegate ValuesChanged;

        /// <summary>The remedial action schemes associated with the element</summary>
        public MM_RemedialActionScheme[] RASs = null;

        /// <summary>Whether the element is inside a PUN</summary>
        public bool PUNElement=false;
        #endregion

        #region Initialization
        /// <summary>
        /// Parameter-free constructor
        /// </summary>
        public MM_Element()
        {
        }

        /// <summary>
        /// Initialize a new CIM element based on an XML element
        /// </summary>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        /// <param name="ElementSource">The XML element containing the TEID, name, owner and operator for the element in question</param>
        public MM_Element(XmlElement ElementSource, bool AddIfNew):base(ElementSource, AddIfNew)
        {
            if (this.ElemType == null)
                MM_Repository.ElemTypes.TryGetValue(ElementSource.Name, out this.ElemType);            
        }

        /// <summary>
        /// Initialize a new CIM element based on a data reader element
        /// </summary>
        /// <param name="ElementSource">The data reader containing the TEID, name, owner and operator for the element in question</param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Element(DbDataReader ElementSource, bool AddIfNew): base(ElementSource, AddIfNew)
        {                        
        }
        #endregion

        #region Menu enhancing 
        /// <summary>
        /// Return a menu one-line description for this element
        /// </summary>
        /// <returns></returns>
        public String MenuDescription()
        {
            try
            {
                if (this is MM_BusbarSection)
                    return String.Format("Bus {0} ({1:0.0} kV, {2:0.0%} pU, {3:0.0}°)", this.Name, (this as MM_BusbarSection).Estimated_kV, (this as MM_BusbarSection).PerUnitVoltage, (this as MM_BusbarSection).Estimated_Angle);
                else if (this is MM_Transformer)
                    return "Transformer " + this.Name + " (" + (this as MM_Transformer).FlowPercentageText + " / " + (this as MM_Transformer).MVAFlow.ToString("0.0") + " mva to " + (this as MM_Transformer).MVAFlowDirection.Name + ")";
                else if (this is MM_Line)
                    return ((this as MM_Line).IsSeriesCompensator ? "SeriesCompensator" : this.ElemType.Name) + " " + this.Name + " (" + (this as MM_Line).LinePercentageText + ")";
                else if (this is MM_Load)
                    return this.ElemType.Name + " " + this.Name + " (" + (this as MM_Load).Estimated_MW.ToString("#,##0.0") + " MW)";
                else if (this is MM_Unit)
                    return (this as MM_Unit).UnitType.Name + " " + this.Name + " (" + (this as MM_Unit).UnitPercentageText + " / " + (this as MM_Unit).Estimated_MW.ToString("#,##0.0") + " MW)";
                else if (this is MM_ShuntCompensator)
                    return this.ElemType.Name + " " + this.Name + " (" + (this as MM_ShuntCompensator).Estimated_MVAR.ToString("#,##0.0") + "/" + (this as MM_ShuntCompensator).Nominal_MVAR.ToString("#,##0.0") + ")";
                else if (this is MM_Note)
                    return (this as MM_Note).Note;
                else if (this is MM_Substation)
                    return "Substation " + (this as MM_Substation).LongName + " (" + this.Name + ")";
                else if (this is MM_Contingency)
                    return this.Name + " / " + (this as MM_Contingency).Description;
                else
                    return this.ElemType.Name + " " + this.Name;
            }
            catch (Exception)
            {
                return null;
            }

        }
        /// <summary>
        /// Return a  description for this element
        /// </summary>
        /// <returns></returns>
        public String ElementDescription()
        {
            if (this is MM_Line)
                if ((this as MM_Line).IsSeriesCompensator)
                    return "SeriesCompensator " + this.Name + " (" + (this as MM_Line).Substation1.DisplayName() + ")";
                else
                    return this.Name + " (" + (this as MM_Line).Substation1.DisplayName() + " / " + (this as MM_Line).Substation2.DisplayName() + ")";
            else if (this is MM_AlarmViolation)
            {
                MM_AlarmViolation Viol = this as MM_AlarmViolation;
                return Viol.Type.Name + " " + Viol.ViolatedElement.ToString() + " " + Viol.Text + (Viol.Contingency != null ? " " + Viol.Contingency.Name : "") + " (" + Viol.Recency + ")";
            }
            else if (this is MM_Company)
                return (this as MM_Company).Alias + " " + this.Name;
            else if (this.ElemType.Name == "User")
                return this.Name;
            else if (this is MM_Substation)
                return this.ElemType.Acronym + " " + this.Name;
            else if (this.Substation != null)
                return this.Substation.DisplayName() + " " + this.ElemType.Acronym + " " + this.Name;

            else
                return this.ElemType.Acronym + " " + this.Name;
        }

        /// <summary>
        /// Determine the worst violation for an element, against the tally of the worst and the current
        /// </summary>
        /// <param name="CurrentViolation"></param>
        /// <returns></returns>
        public MM_AlarmViolation_Type WorstViolation(MM_AlarmViolation_Type CurrentViolation)
        {
            MM_AlarmViolation_Type CurWorst = WorstViolationOverall;
            if (Data_Integration.Permissions.OTS == true && this is MM_BusbarSection)//r-rtst
                return (this as MM_BusbarSection).Node.WorstViolation(CurrentViolation);//r-rtst

            if (CurrentViolation == null)
                return WorstViolationOverall;
            else if (CurWorst == null)
                return CurrentViolation;
            else if (CurWorst.Priority < CurrentViolation.Priority)
                return CurWorst;
            else
                return CurrentViolation;
        }


        /// <summary>
        /// Return the worst violation overall, regardless of its visibility
        /// </summary>
        public MM_AlarmViolation_Type WorstViolationOverall
        {
            get
            {
                MM_AlarmViolation_Type WorstViol = null;
                foreach (MM_AlarmViolation Viol in this.Violations.Values)
                    if (WorstViol == null || Viol.Type.Priority < WorstViol.Priority)
                        WorstViol = Viol.Type;
                if (this is MM_Transformer)
                    foreach (MM_TransformerWinding Winding in (this as MM_Transformer).Windings)
                        foreach (MM_AlarmViolation Viol in Winding.Violations.Values)
                            if (WorstViol == null || Viol.Type.Priority < WorstViol.Priority)
                                WorstViol = Viol.Type;
                if (this is MM_Boundary)
                    foreach (MM_Substation Sub in (this as MM_Boundary).Substations)
                        WorstViol = Sub.WorstViolation(WorstViol);
                /*/if (this is MM_Substation)
                {
                    MM_Substation Sub = this as MM_Substation;
                    foreach (MM_Element[] Elem in new MM_Element[][] { Sub.ElectricalBuses, Sub.Loads, Sub.ShuntCompensators, Sub.Transformers, Sub.Units })
                        if (Elem != null)
                            WorstViol = WorstViolation(WorstViol);
                }*/
                   

                return WorstViol;
            }
        }

        /// <summary>
        /// Retrieve the image corresponding to the worst violation type
        /// </summary>
        public Image WorstViolationImage
        {
            get
            {
                if (Data_Integration.Permissions.OTS == true && this is MM_BusbarSection) //r-rtst
                    return (this as MM_BusbarSection).Node.WorstViolationImage; //r-rtst
                MM_AlarmViolation_Type WorstViol = this.WorstViolationOverall;
                if (WorstViol != null)
                    return MM_Repository.ViolationImages.Images[WorstViol.Name];
                else if (FindNotes().Length > 0)
                    return MM_Repository.ViolationImages.Images["Note"];
                else
                    return null;
            }
        }

         
        #endregion

        #region String - represent as TEID
        /// <summary>
        /// Override the ToString function, so that the data tables are sorting by TEID
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ElementDescription();
        }
        #endregion

        #region Violation handling
        /// <summary>
        /// Determine the worst violation type within the substation
        /// </summary>
        public MM_AlarmViolation_Type WorstVisibleViolation(Dictionary<MM_AlarmViolation, ListViewItem> ShownViolations, Object CallingForm)
        {            
                MM_AlarmViolation_Type WorstViolation = null;
            
                if (CallingForm is Mini_Map)
                {
                    foreach (MM_AlarmViolation Viol in Violations.Values)                        
                        if ((Viol.Type.MiniMap == MM_AlarmViolation_Type.DisplayModeEnum.Always) || ((Viol.Type.MiniMap == MM_AlarmViolation_Type.DisplayModeEnum.WhenSelected) && (ShownViolations.ContainsKey(Viol))))
                            if ((WorstViolation == null) || (Viol.Type.ViolationIndex < WorstViolation.ViolationIndex))
                                WorstViolation = Viol.Type;
                }   
                else if (this is MM_Line)
                {
                    foreach (MM_AlarmViolation Viol in Violations.Values)
                        if ((Viol.Type.NetworkMap_Line == MM_AlarmViolation_Type.DisplayModeEnum.Always) || ((Viol.Type.NetworkMap_Line == MM_AlarmViolation_Type.DisplayModeEnum.WhenSelected) && (ShownViolations.ContainsKey(Viol))))
                            if ((WorstViolation == null) || (Viol.Type.ViolationIndex < WorstViolation.ViolationIndex))
                                WorstViolation = Viol.Type;
                }
                else if (this is MM_Substation)
                {
                    foreach (MM_AlarmViolation Viol in Violations.Values)
                        if ((Viol.Type.NetworkMap_Substation == MM_AlarmViolation_Type.DisplayModeEnum.Always) || ((Viol.Type.NetworkMap_Substation == MM_AlarmViolation_Type.DisplayModeEnum.WhenSelected) && (ShownViolations.ContainsKey(Viol))))
                            if ((WorstViolation == null) || (Viol.Type.ViolationIndex < WorstViolation.ViolationIndex))
                                WorstViolation = Viol.Type;
                }
                else if (this is MM_Node && Violations.Count == 0 && ShownViolations.Count > 0)
                {
                    foreach (MM_AlarmViolation Viol in ShownViolations.Keys)
                        if (Viol.ViolatedElement.ElemType.Name == "BusbarSection" && Viol.ViolatedElement.Substation.Equals(this.Substation) && Viol.ViolatedElement.Name.Equals(this.Name))
                            if ((Viol.Type.OneLineElement == MM_AlarmViolation_Type.DisplayModeEnum.Always) || ((Viol.Type.OneLineElement == MM_AlarmViolation_Type.DisplayModeEnum.WhenSelected)))
                                if ((WorstViolation == null) || (Viol.Type.ViolationIndex < WorstViolation.ViolationIndex))
                                    WorstViolation = Viol.Type;
                }
                else
                {
                    foreach (MM_AlarmViolation Viol in Violations.Values)
                        if ((Viol.Type.OneLineElement == MM_AlarmViolation_Type.DisplayModeEnum.Always) || ((Viol.Type.OneLineElement == MM_AlarmViolation_Type.DisplayModeEnum.WhenSelected) && (ShownViolations.ContainsKey(Viol))))
                            if ((WorstViolation == null) || (Viol.Type.ViolationIndex < WorstViolation.ViolationIndex))
                                WorstViolation = Viol.Type;
                }
                return WorstViolation;           
        }
        #endregion



        /// <summary>
        /// Determine the available notes for our element
        /// </summary>
        /// <returns></returns>
        public MM_Note[] FindNotes()
        {
            List<MM_Note> OutNotes = new List<MM_Note>();
            
            //First, look at notes for this item;
            OutNotes.AddRange(Notes.Values);
            //Now, if a substation, pull in notes on all elements within
            if (this is MM_Substation)
                foreach (MM_Element Elem in MM_Repository.TEIDs.Values)
                    if (Elem.Substation != null && Elem.Substation == this)
                        OutNotes.AddRange(Elem.Notes.Values);

            if (this is MM_Boundary)
                foreach (MM_Element Elem in MM_Repository.TEIDs.Values)
                    try
                    {
                        if (Elem is MM_Substation && (Elem as MM_Substation).County.Equals(this))
                            OutNotes.AddRange(Elem.Notes.Values);
                        else if (Elem is MM_Line && ((Elem as MM_Line).Substation1.County.Equals(this) || (Elem as MM_Line).Substation2.County.Equals(this)))
                            OutNotes.AddRange(Elem.Notes.Values);
                        else if (Elem.Substation != null && Elem.Substation.County.Equals(this))
                            OutNotes.AddRange(Elem.Notes.Values);
                    }
                    catch (Exception)
                    {
                    }
            if (this is MM_AlarmViolation)
                OutNotes.AddRange((this as MM_AlarmViolation).ViolatedElement.Notes.Values);
            return OutNotes.ToArray();
        }

        private CheckState _Permitted = CheckState.Indeterminate;

        /// <summary>
        /// Whether this substation can be displayed, given its permission levels
        /// </summary>
        public bool Permitted
        {
            set
            {
                this._Permitted = value ? CheckState.Checked : CheckState.Unchecked;
            }
            get
            {
                if (_Permitted == CheckState.Checked)
                    return true;
                else if (_Permitted == CheckState.Unchecked)
                    return false;
                else if (this is MM_Substation)
                {
                    foreach (MM_KVLevel KVLevel in (this as MM_Substation).KVLevels)
                        if (KVLevel.Permitted && KVLevel.Name != "Other KV")
                            return true;
                    if ((this as MM_Substation).KVLevels.Count == 1)
                        return (this as MM_Substation).KVLevels[0].Permitted;
                    return false;
                }
                else if (this is MM_Transformer)
                {
                    foreach (MM_TransformerWinding Winding in (this as MM_Transformer).Windings)
                        if (Winding.Permitted)
                            return true;
                    return false;
                }
                else if (this.KVLevel == null)
                    return true;
                else
                    return this.KVLevel.Permitted;
            }
        }




        #region IComparable Members
        /// <summary>
        /// Handle comparing the two elements
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            if (obj is MM_Element == false)
                return 1;
            else if (obj is MM_KVLevel)
                return ((MM_KVLevel)this).Nominal.CompareTo(((MM_KVLevel)obj).Nominal);
            else if (obj is MM_Island && this is MM_Island)
                return ((MM_Island)this).ID.CompareTo(((MM_Island)obj).ID);
            else
                return TEID.CompareTo(((MM_Element)obj).TEID);
            /*
            MM_Element OtherElem = obj as MM_Element;
            int FirstLevel = 0;
            if (this.ElemType != OtherElem.ElemType)
                FirstLevel = this.ElemType.Index.CompareTo(OtherElem.ElemType.Index);
            else if (this is MM_ShuntCompensator)
                FirstLevel = (this as MM_ShuntCompensator).CompareShuntCompensators(obj as MM_ShuntCompensator);
            else if (this is MM_BusbarSection)
                FirstLevel = (this as MM_BusbarSection).CompareBusbarSections(obj as MM_BusbarSection);
            else if (this is MM_Unit)
                FirstLevel = (this as MM_Unit).CompareUnits(obj as MM_Unit);
            else if (this is MM_Load)
                FirstLevel = (this as MM_Load).CompareLoads(obj as MM_Load);

            if (FirstLevel != 0)
                return FirstLevel;
            else if (OtherElem.TEID == TEID)
                return 0;
            else
            {

                MM_Substation Sub = (this is MM_Substation ? this as MM_Substation : this.Substation);
                MM_Substation OtherSub = (obj is MM_Substation ? ((MM_Substation)obj) : OtherElem.Substation);
                if (Sub == null || OtherSub == null)
                {
                    if (OtherElem.Name != null&& this.Name != null)
                        return this.Name.CompareTo(OtherElem.Name);
                    else
                        return 0;
                }
                String ThisName = Sub.DisplayName();
                String OtherName = OtherSub.DisplayName();

                if (String.IsNullOrEmpty(ThisName) || String.IsNullOrEmpty(OtherName))
                    return this.Name.CompareTo(OtherElem.Name);
                else
                {
                    int SubComp = ThisName.CompareTo(OtherName);
                    if (SubComp != 0)
                        return SubComp;
                    else if (this is MM_TransformerWinding && OtherElem is MM_TransformerWinding)
                        return this.TEID.CompareTo(OtherElem.TEID);
                    else
                        return this.Name.CompareTo(OtherElem.Name);
                }
            }*/
        }

        #endregion

        /// <summary>
        /// Trigger the value change event
        /// </summary>
        public void TriggerValueChange()
        {
            if (ValuesChanged != null)
                ValuesChanged(this);
        }

        /// <summary>
        /// Create a new element
        /// </summary>
        /// <param name="TEID"></param>
        /// <param name="ElemType"></param>
        /// <param name="AddIfNew"></param>
        ///  <param name="BaseElement"></param>
        ///  <param name="mI"></param>
        /// <returns></returns>
        public static MM_Element CreateElement(Int32 TEID, Type ElemType, bool AddIfNew, MM_Element BaseElement, MemberInfo mI)
        {
            
            MM_Element OutElement;
            if (MM_Repository.TEIDs.TryGetValue(TEID, out OutElement))
                return OutElement;

            OutElement = Activator.CreateInstance(ElemType) as MM_Element;
            OutElement.TEID = TEID;
            
            if (AddIfNew)
            {

                MM_Repository.TEIDs.Add(TEID, OutElement);                
                if (ElemType == typeof(MM_Element) || OutElement.ElemType == null)
                {
                    Dictionary<MM_Element, MemberInfo> Mapping;
                    if (!UnknownElements.TryGetValue(TEID, out Mapping))
                        UnknownElements.Add(TEID, Mapping = new Dictionary<MM_Element, MemberInfo>(5));
                    Mapping.Add(BaseElement, mI);
                }
            }
            return OutElement;

        }
        
        /// <summary>
        /// Create a new element
        /// </summary>
        /// <param name="TEID"></param>
        /// <param name="ElemType"></param>
        /// <param name="AddIfNew"></param>
        /// <returns></returns>
        public static MM_Element CreateElement(Int32 TEID, MM_Communication.Elements.MM_Element.enumElementType ElemType, bool AddIfNew)
        {           
            MM_Element OutElement;
            if (MM_Repository.TEIDs.TryGetValue(TEID, out OutElement))
                return OutElement;
            else if (ElemType == MM_Communication.Elements.MM_Element.enumElementType.Breaker || ElemType == MM_Communication.Elements.MM_Element.enumElementType.Switch)
                OutElement = new MM_Breaker_Switch();
            else if (ElemType == MM_Communication.Elements.MM_Element.enumElementType.DCTie)
                OutElement = new MM_DCTie();
            else if (ElemType == MM_Communication.Elements.MM_Element.enumElementType.StaticVarCompensator)
                OutElement = new MM_StaticVarCompensator();
            else if (ElemType == MM_Communication.Elements.MM_Element.enumElementType.ElectricalBus)
                OutElement = new MM_Electrical_Bus();
            else if (ElemType == MM_Communication.Elements.MM_Element.enumElementType.Line)
                OutElement = new MM_Line();
            else if (ElemType == MM_Communication.Elements.MM_Element.enumElementType.Load)
                OutElement = new MM_Load();
            else if (ElemType == MM_Communication.Elements.MM_Element.enumElementType.Unit)
                OutElement = new MM_Unit();
            else if (ElemType == MM_Communication.Elements.MM_Element.enumElementType.LogicalUnit)
                OutElement = new MM_LogicalUnit();
            else if (ElemType == MM_Communication.Elements.MM_Element.enumElementType.SeriesCompensator)
                (OutElement = new MM_Line()).ElemType = MM_Repository.FindElementType("SeriesCompensator");
            else if (ElemType == MM_Communication.Elements.MM_Element.enumElementType.Capacitor)
                (OutElement = new MM_ShuntCompensator()).ElemType = MM_Repository.FindElementType("Capacitor");
            else if (ElemType == MM_Communication.Elements.MM_Element.enumElementType.Reactor)
                (OutElement = new MM_ShuntCompensator()).ElemType = MM_Repository.FindElementType("Reactor");
            else if (ElemType == MM_Communication.Elements.MM_Element.enumElementType.Transformer)
                OutElement = new MM_Transformer();
            else if (ElemType == MM_Communication.Elements.MM_Element.enumElementType.TransformerWinding)
                OutElement = new MM_TransformerWinding();
            else if (ElemType == MM_Communication.Elements.MM_Element.enumElementType.Node)
                OutElement = new MM_Node();
            else if (ElemType == MM_Communication.Elements.MM_Element.enumElementType.VoltageLevel)
                OutElement = new MM_KVLevel("?", "Red");
            else if (ElemType == MM_Communication.Elements.MM_Element.enumElementType.BusbarSection)
                OutElement = new MM_BusbarSection();
            else if (ElemType == MM_Communication.Elements.MM_Element.enumElementType.PricingVector)
                OutElement = new MM_PricingVector();
            else if (ElemType == MM_Communication.Elements.MM_Element.enumElementType.EPSMeter)
                OutElement = new MM_EPSMeter();
            else if (ElemType == MM_Communication.Elements.MM_Element.enumElementType.County || ElemType == MM_Communication.Elements.MM_Element.enumElementType.State)
                OutElement = new MM_Boundary();
            else if (ElemType == MM_Communication.Elements.MM_Element.enumElementType.Company)
                OutElement = new MM_Company();
            else if (ElemType == MM_Communication.Elements.MM_Element.enumElementType.Contingency)
                OutElement = new MM_Contingency();
            else if (ElemType == MM_Communication.Elements.MM_Element.enumElementType.Substation)
                OutElement = new MM_Substation();
            else if (ElemType == MM_Communication.Elements.MM_Element.enumElementType.CombinedCycleConfiguration)
                OutElement = new MM_CombinedCycleConfiguration();
            else if (ElemType == MM_Communication.Elements.MM_Element.enumElementType.RAP)
                OutElement = new MM_RemedialActionScheme();
            else if (ElemType == MM_Communication.Elements.MM_Element.enumElementType.Blackstart_Corridor)
                OutElement = new MM_Blackstart_Corridor();
            else if (ElemType == MM_Communication.Elements.MM_Element.enumElementType.Blackstart_Target)
                OutElement = new MM_Blackstart_Corridor_Target();
            else if (ElemType == MM_Communication.Elements.MM_Element.enumElementType.Blackstart_Element)
                OutElement = new MM_Blackstart_Corridor_Element();
            else if (ElemType == MM_Communication.Elements.MM_Element.enumElementType.Island)
                OutElement = new MM_Island();
            else
                OutElement = new MM_Element();
            OutElement.ElemType = MM_Repository.FindElementType(ElemType.ToString());
            OutElement.TEID = TEID;
            if (AddIfNew && !MM_Repository.TEIDs.ContainsKey(TEID))
                MM_Repository.TEIDs.Add(TEID, OutElement);
            return OutElement;
        }

        /// <summary>
        /// Replace instances of one element with another.
        /// </summary>
        /// <param name="TEID"></param>
        /// <param name="ElemType"></param>
        /// <param name="AddIfNew"></param>
        /// <param name="FoundElem"></param>
        /// <returns></returns>
        private static MM_Element ReplaceElement(Int32 TEID, MM_Communication.Elements.MM_Element.enumElementType ElemType, bool AddIfNew, MM_Element FoundElem)
        {
            //make sure we have an element to replace
            Dictionary<MM_Element, MemberInfo> Mappings;
            if (!UnknownElements.TryGetValue(TEID, out Mappings))
                return FoundElem;

            //First, remove the element from our TEID collection
            MM_Repository.TEIDs.Remove(TEID);
            MM_Element OutElem = CreateElement(TEID, ElemType, AddIfNew);

            
            foreach (KeyValuePair<MM_Element, MemberInfo> kvp in Mappings)
            {
                Type TargetType = kvp.Value is FieldInfo ? (kvp.Value as FieldInfo).FieldType : (kvp.Value as PropertyInfo).PropertyType;
                if (TargetType.IsArray)
                {
                    Array InArray = (Array)(kvp.Value is FieldInfo ? (kvp.Value as FieldInfo).GetValue(kvp.Key) : (kvp.Value as PropertyInfo).GetValue(kvp.Key, null));
                    for (int a = 0; a < InArray.Length; a++)
                        if (((MM_Element)InArray.GetValue(a)).TEID == TEID)
                            InArray.SetValue(OutElem, a);
                }
                else
                {
                    if (kvp.Value is PropertyInfo)
                        (kvp.Value as PropertyInfo).SetValue(kvp.Key, OutElem, null);
                    else
                        (kvp.Value as FieldInfo).SetValue(kvp.Key, OutElem);
                }
            }
            UnknownElements.Remove(TEID);
            return OutElem;
        }

        /// <summary>
        /// Process a value, inserting it into our element
        /// </summary>
        /// <param name="Elem"></param>
        /// <param name="ValueUpdate"></param>
        public static void ProcessValue(MM_Element Elem, Macomber_Map.Data_Connections.MM_Server.MM_Server_ValueUpdate ValueUpdate)
        {            
            MemberInfo[] memInfo = Elem.GetType().GetMember(ValueUpdate.Attribute.ToString(), BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (memInfo.Length == 0)
            {
                // Console.WriteLine("Unable to locate type " + ValueUpdate.Attribute.ToString() + " for " + Elem.GetType().Name + " " + Elem.TEID.ToString("#,##0"));
            }
            else
            {
                Type TargetType = memInfo[0] is PropertyInfo ? ((PropertyInfo)memInfo[0]).PropertyType : ((FieldInfo)memInfo[0]).FieldType;
                try
                {
                    //If we have an int[] -> TEID/Element situation, adjust accordingly.
                    if (TargetType == ValueUpdate.InValue.GetType())
                    { }
                    else if (ValueUpdate.InValue is Int32 && (TargetType == typeof(MM_Element) || TargetType.IsSubclassOf(typeof(MM_Element))))
                    {
                        if (TargetType == typeof(MM_Island))
                        {
                            MM_Island FoundIsland;
                            if (!MM_Repository.Islands.TryGetValue((int)ValueUpdate.InValue, out FoundIsland))
                            {
                                FoundIsland = new MM_Island((int)ValueUpdate.InValue);
                                if ((int)ValueUpdate.InValue != 0)
                                    MM_Repository.Islands.Add((int)ValueUpdate.InValue, FoundIsland);
                            }

                            if (Elem is MM_Node)
                                (Elem as MM_Node).IslandNumber = (int)ValueUpdate.InValue;
                            ValueUpdate.InValue = FoundIsland;
                        }
                        else
                            ValueUpdate.InValue = MM_Element.CreateElement(Convert.ToInt32(ValueUpdate.InValue), TargetType, true, Elem, memInfo[0]);
                    }
                    else if (TargetType.IsArray && (TargetType.GetElementType() == typeof(MM_Element) || TargetType.GetElementType().IsSubclassOf(typeof(MM_Element))))
                    {
                        Array OutArray = Array.CreateInstance(TargetType.GetElementType(), ((Array)ValueUpdate.InValue).Length);
                        for (int a = 0; a < OutArray.Length; a++)
                            OutArray.SetValue(MM_Element.CreateElement(((int[])ValueUpdate.InValue)[a], TargetType.GetElementType(), true, Elem, memInfo[0]), a);
                        ValueUpdate.InValue = OutArray;
                    }

                    else if (TargetType == typeof(List<MM_Element_Type>))
                    {
                        List<MM_Element_Type> OutList = new List<MM_Element_Type>(((Array)ValueUpdate.InValue).Length);
                        for (int a = 0; a < OutList.Capacity; a++)
                            OutList.Add(MM_Repository.FindElementType(((Array)ValueUpdate.InValue).GetValue(a).ToString()));
                        ValueUpdate.InValue = OutList;
                    }
                    else if (TargetType == typeof(List<MM_KVLevel>))
                    {
                        List<MM_KVLevel> OutList = new List<MM_KVLevel>(((Array)ValueUpdate.InValue).Length);
                        for (int a = 0; a < OutList.Capacity; a++)
                            OutList.Add(MM_Element.CreateElement(((int[])ValueUpdate.InValue)[a], MM_Communication.Elements.MM_Element.enumElementType.VoltageLevel, true) as MM_KVLevel);
                        ValueUpdate.InValue = OutList;
                    }


                    //Perform a special check for nodes with busbar sections missing values
                    Object[] ObjectToWrite = new object[] { Elem };
                    MM_BusbarSection Bus;
                    if (Elem is MM_Node && (Bus = (Elem as MM_Node).Bus) != null && TargetType.IsArray == false)
                    {
                        MemberInfo[] BusInfo = typeof(MM_BusbarSection).GetMember(memInfo[0].Name);
                        if (BusInfo.Length == 1 && BusInfo[0] is PropertyInfo && (BusInfo[0] as PropertyInfo).GetValue(Bus, null) == null)
                            ObjectToWrite = new object[] { Elem, Bus };
                        else if (BusInfo.Length == 1 && BusInfo[0] is FieldInfo && (BusInfo[0] as FieldInfo).GetValue(Bus) == null)
                            ObjectToWrite = new object[] { Elem, Bus };

                    }

                    //Now, write out our values
                    foreach (Object obj in ObjectToWrite)
                    {
                        if (TargetType.IsArray && !ValueUpdate.InValue.GetType().IsArray)
                        {
                            Array TargetArray = (Array)(memInfo[0] is PropertyInfo ? ((PropertyInfo)memInfo[0]).GetValue(obj, null) : ((FieldInfo)memInfo[0]).GetValue(obj));
                            TargetArray.SetValue(ValueUpdate.InValue, ValueUpdate.Index);
                        }
                        else if (memInfo[0] is PropertyInfo)
                        {
                            if (((PropertyInfo)memInfo[0]).CanWrite)
                                ((PropertyInfo)memInfo[0]).SetValue(obj, ValueUpdate.InValue, null);
                        }
                        else
                            ((FieldInfo)memInfo[0]).SetValue(obj, ValueUpdate.InValue);
                    }

                    //add new code

                    if (Elem is MM_Node && ValueUpdate.InValue is Int32 && (Elem as MM_Node).BusbarSection != null && Elem.BusNumber != -1)
                    {
                        (Elem as MM_Node).BusbarSection.BusNumber = Elem.BusNumber;
                        if (!MM_Repository.BusNumbers.ContainsKey((Elem as MM_Node).BusbarSection.BusNumber))
                            MM_Repository.BusNumbers.Add(Elem.BusNumber, (Elem as MM_Node).BusbarSection);
                        else
                            MM_Repository.BusNumbers[Elem.BusNumber] = (Elem as MM_Node).BusbarSection;
                    }
                    

                    //If requested, write out our value
                    if (Data_Integration.ValueChangeLog != null)
                        Data_Integration.ValueChangeLog.WriteLine("<ValueChange TimeStamp=\"" + XmlConvert.ToString(DateTime.Now, XmlDateTimeSerializationMode.Unspecified) + "\" TEID=\"" + Elem.TEID.ToString() + "\" " + (Elem.Substation == null ? "" : "Substation=\"" + Elem.Substation.Name + "\" ") + "Name=\"" + Elem.Name + "\" " + (Elem.Operator == null ? "" : "Operator=\"" + Elem.Operator.Alias + "\" ") + " Attribute=\"" + ValueUpdate.Attribute.ToString() + "\" Index=\"" + ValueUpdate.Index.ToString() + "\" NewValue=\"" + ValueUpdate.InValue.ToString() + "\"/>");
                }
                catch (Exception ex)
                {
                    String Mismatch = ValueUpdate.Attribute.ToString() + "/" + Elem.GetType().Name;
                    if (Mismatches.ContainsKey(Mismatch))
                        Mismatches[Mismatch]++;
                    else
                    {
                        Mismatches.Add(Mismatch, 1);
                        Program.LogError("Error trying to write " + ValueUpdate.ToString() + " into " + Elem.GetType().Name + " " + Elem.TEID.ToString("#,##0") + ": " + ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Create a new element based on its definition
        /// </summary>
        /// <param name="xElem">The definition for the element</param>
        /// <param name="Prefix">The prefix for the element, if any</param>
        /// <param name="AddIfNew">Whether to add a new element to our master repository</param>        
        /// <returns></returns>
        public static MM_Element CreateElement(XmlElement xElem, string Prefix, bool AddIfNew)
        {
            MM_Element OutElement;
           // String ElemType;
            //if (xElem.HasAttribute("ElemType"))
            String ElemType = String.IsNullOrEmpty(Prefix) ? xElem.Attributes["ElemType"].Value : xElem.HasAttribute(Prefix + ".ElemType") ? xElem.Attributes[Prefix + ".ElemType"].Value : Prefix;
            //else
            //ElemType = xElem.Name;

            if (xElem.HasAttribute("BaseElement.IsSeriesCompensator") && XmlConvert.ToBoolean(xElem.Attributes["BaseElement.IsSeriesCompensator"].Value))
                OutElement = new MM_Line();
            else if (ElemType == "Breaker" || ElemType == "Switch")
                OutElement = new MM_Breaker_Switch();
            else if (ElemType == "DCTie")
                OutElement = new MM_DCTie();
            else if (ElemType == "ElectricalBus")
                OutElement = new MM_Electrical_Bus();
            else if (ElemType == "Line")
                OutElement = new MM_Line();
            else if (ElemType == "Load" || ElemType.Equals("LaaR", StringComparison.CurrentCultureIgnoreCase))
                OutElement = new MM_Load();
            else if (ElemType == "Unit")
                OutElement = new MM_Unit();
            else if (ElemType == "Capacitor" || ElemType == "Reactor")
                OutElement = new MM_ShuntCompensator();
            else if (ElemType == "Transformer")
                OutElement = new MM_Transformer();
            else if (ElemType == "TransformerWinding")
                OutElement = new MM_TransformerWinding();
            else if (ElemType == "StaticVarCompensator")
                OutElement = new MM_StaticVarCompensator();
            else if (ElemType == "Node")
                OutElement = new MM_Node();
            else if (ElemType == "BusbarSection")
                OutElement = new MM_BusbarSection();
            else if (ElemType == "PricingVector")
                OutElement = new MM_PricingVector();
            else if (ElemType == "EPSMeter")
                OutElement = new MM_EPSMeter();
            else if (ElemType == "County" || ElemType == "State")
                OutElement = new MM_Boundary();
            else if (ElemType == "Company")
                OutElement = new MM_Company();
            else if (ElemType == "Contingency")
                OutElement = new MM_Contingency();
            else if (ElemType == "Substation")
                OutElement = new MM_Substation();
            else if (ElemType == "VoltageLevel")
                OutElement = new MM_KVLevel();
            else
                OutElement = new MM_Element();
            OutElement.ElemType = MM_Repository.FindElementType(ElemType);
            
           

            //Now, pull in our attributes
            foreach (XmlAttribute xAttr in xElem.Attributes)
                if ((String.IsNullOrEmpty(Prefix) && xAttr.Name.IndexOf('.') == -1))
                    MM_Serializable.AssignValue(xAttr.Name, xAttr.Value, OutElement, AddIfNew);
                else if (!String.IsNullOrEmpty(Prefix) && xAttr.Name.StartsWith(Prefix + "."))
                    MM_Serializable.AssignValue(xAttr.Name.Substring(xAttr.Name.IndexOf('.')+1), xAttr.Value, OutElement, AddIfNew);

            //If we're a transformer, pull in our windings
            if (ElemType == "Transformer")
            {
                List<MM_TransformerWinding> Windings = new List<MM_TransformerWinding>();
                foreach (XmlElement xWind in xElem.SelectNodes("Winding"))
                    Windings.Add(MM_Element.CreateElement(xWind, "BaseElement", AddIfNew) as MM_TransformerWinding);
                if (xElem.HasAttribute("BaseElement.KVLevel1"))
                    Windings[0].Voltage = XmlConvert.ToSingle(xElem.Attributes["BaseElement.KVLevel1"].Value.Split(' ')[0]);
                if (xElem.HasAttribute("BaseElement.KVLevel2"))
                    Windings[1].Voltage = XmlConvert.ToSingle(xElem.Attributes["BaseElement.KVLevel2"].Value.Split(' ')[0]);
                (OutElement as MM_Transformer).Windings = Windings.ToArray();
            }

            //If we're a line, check for series compensator status to upgrade our type
            if (OutElement is MM_Line && xElem.HasAttribute("BaseElement.IsSeriesCompensator") && XmlConvert.ToBoolean(xElem.Attributes["BaseElement.IsSeriesCompensator"].Value))
                OutElement.ElemType = MM_Repository.FindElementType("SeriesCompensator");
            //Return our new element
            return OutElement;
        }
    }

}
