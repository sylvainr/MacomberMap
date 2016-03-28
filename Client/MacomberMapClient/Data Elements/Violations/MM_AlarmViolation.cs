using MacomberMapClient.Data_Elements.Physical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MacomberMapClient.Integration;

namespace MacomberMapClient.Data_Elements.Violations
{
    /// <summary>
    /// (C) 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved
    /// This class covers alarms, violations, and other events that will be shown in the viewer
    /// </summary>
    public class MM_AlarmViolation : MM_Element, IComparable<MM_AlarmViolation>, IEquatable<MM_AlarmViolation>
    {
        #region Variable declarations
        /// <summary>The type of the alarm/violation/event</summary>
        public MM_AlarmViolation_Type Type;

        /// <summary>The violated element</summary>
        public MM_Element ViolatedElement;

        /// <summary>The contingency definition (if any) associated with the alarm/violation/event</summary>        
        public MM_Contingency Contingency;

        /// <summary>The alarm text</summary>
        public String Text;

        /// <summary>The date/time of the event</summary>
        public DateTime EventTime;

        /// <summary>When the violation was reported to start (e.g., OS)</summary>
        public DateTime ReportedStart;

        /// <summary>When the violation was reported to end (e.g., OS)</summary>
        public DateTime ReportedEnd;

        /// <summary>Whether this violation is flagged as new</summary>
        public bool New;

        /// <summary>The post-contingency flow or voltage on our contingency violation</summary>
        public float PostCtgValue = float.NaN;

        /// <summary>The pre-contingency flow or voltage on our contingency violation</summary>
        public float PreCtgValue = float.NaN;

        /// <summary>
        /// Report the recency on our item
        /// </summary>
        public String Recency
        {
            get
            {
                if (EventTime.ToOADate() == 0)
                    return "?";
                TimeSpan Diff = DateTime.Now - EventTime;
                if (Diff.TotalSeconds < 60)
                    return "Now";
                else if (Diff.TotalMinutes < 60)
                    return Diff.TotalMinutes.ToString("0") + " min.";
                else if (Diff.TotalHours < 24)
                    return Diff.TotalHours.ToString("0") + " hrs.";
                else
                    return Diff.TotalDays.ToString("0.0") + " days";
            }
        }

        /// <summary>
        /// Provide the substation or line associated with the violation
        /// </summary>
        public MM_Element SubstationOrLine
        {
            get
            {
                if (ViolatedElement is MM_Line)
                    return ViolatedElement;
                else if (ViolatedElement is MM_Substation)
                    return ViolatedElement;
                else if (ViolatedElement is MM_Contingency && ((MM_Contingency)ViolatedElement).ConElements.Count > 0)
                {
                    MM_Element element = null;
                    MM_Repository.TEIDs.TryGetValue( ((MM_Contingency)ViolatedElement).ConElements[0], out element);

                    if (element is MM_Line)
                        return element;
                    else 
                        return element.Substation;
                }
                else // if (ViolatedElement.Substation != null)
                    return ViolatedElement.Substation;
                //else
                //  throw new Exception("Unknown element type: " + ViolatedElement.GetType().ToString());
            }
        }

        /// <summary>
        /// Create a new alarm violation with no parameters
        /// </summary>
        public MM_AlarmViolation()
        {
        }

        /// <summary>
        /// Create a new alarm violation with values (from an XML savecase)
        /// </summary>
        /// <param name="Violation"></param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_AlarmViolation(XmlElement Violation, bool AddIfNew)
            : base(Violation, AddIfNew)
        {


        }
        #endregion

        #region Overrides for comparison
        /// <summary>
        /// Determine whether two alarm violations are comparable
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(MM_AlarmViolation other)
        {
            int TEIDDiff = ViolatedElement.TEID.CompareTo(other.TEID);
            if (TEIDDiff != 0)
                return TEIDDiff;

            int ViolType = Type.ViolationIndex.CompareTo(other.Type.ViolationIndex);
            if (ViolType != 0)
                return ViolType;

            if (Contingency != null && other.Contingency == null)
                return 1;
            else if (Contingency == null && other.Contingency != null)
                return -1;
            else
                return Contingency.CompareTo(other.Contingency);

        }


        /// <summary>
        /// Determine whether two violations are equal
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(MM_AlarmViolation other)
        {            
            return this.ViolatedElement.TEID.Equals(other.ViolatedElement.TEID) && this.Type.Equals(other.Type);
        }

        /// <summary>
        /// Return a hash code oriented around the TEID and violation type
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            if (ViolatedElement == null)
                return 0;
            if (Type == null)
                return 2020;
            if (Contingency != null)
                return ViolatedElement.TEID.GetHashCode() + Type.GetHashCode() + Contingency.GetHashCode();
            else
                return ViolatedElement.TEID.GetHashCode() + Type.GetHashCode();
        }

        #endregion

        /// <summary>
        /// Create a new violation
        /// </summary>
        /// <param name="ViolatedElement"></param>
        /// <param name="ViolText"></param>
        /// <param name="ViolType"></param>
        /// <param name="EventTime"></param>
        /// <param name="New"></param>
        /// <param name="ViolatedCtg"></param>
        /// <param name="PostCtgValue"></param>
        /// <param name="PreCtgValue"></param>
        /// <returns></returns>
        public static MM_AlarmViolation CreateViolation(MM_Element ViolatedElement, string ViolText, MM_AlarmViolation_Type ViolType, DateTime EventTime, bool New, MM_Contingency ViolatedCtg, float PreCtgValue, float PostCtgValue)
        {
            MM_AlarmViolation NewViol = new MM_AlarmViolation();
            NewViol.TEID = Integration.Data_Integration.GetTEID();
            NewViol.ViolatedElement = ViolatedElement;
            if (ViolatedElement is MM_Transformer)
                NewViol.KVLevel = (ViolatedElement as MM_Transformer).Winding1.KVLevel;
            else
                NewViol.KVLevel = ViolatedElement.KVLevel;
            NewViol.ElemType = ViolatedElement.ElemType;
            NewViol.Owner = ViolatedElement.Owner;
            NewViol.Operator = ViolatedElement.Operator;
            NewViol.Contingency = ViolatedCtg;
            NewViol.New = New;
            NewViol.Text = ViolText;
            NewViol.Type = ViolType;
            NewViol.EventTime = EventTime;
            NewViol.PreCtgValue = PreCtgValue;
            NewViol.PostCtgValue = PostCtgValue;
            return NewViol;
        }
    }
}
