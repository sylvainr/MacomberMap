using MacomberMapClient.Integration;
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
    /// This class holds information on transformers
    /// </summary>
    public class MM_Transformer : MM_Element
    {
        #region Variable declarations
        /// <summary>Estimated MW flow at each terminal</summary>
        public float[] Estimated_MW
        {
            get
            {
                float[] OutMW = new float[Windings.Length];
                for (int a = 0; a < Windings.Length; a++)
                    OutMW[a] = Windings[a].Estimated_MW;
                return OutMW;
            }
            set
            {
                for (int a = 0; a < value.Length; a++)
                    Windings[a].Estimated_MW = value[a];
            }
        }



        /// <summary>Telemetered MW flow at each terminal</summary>
        public float[] Telemetered_MW
        {
            get
            {
                float[] OutMW = new float[Windings.Length];
                for (int a = 0; a < Windings.Length; a++)
                    OutMW[a] = Windings[a].Telemetered_MW;
                return OutMW;
            }
            set
            {
                for (int a = 0; a < value.Length; a++)
                    Windings[a].Telemetered_MW = value[a];
            }
        }

        /// <summary>Estimated MVAR flow at each terminal</summary>
        public float[] Estimated_MVAR
        {
            get
            {
                float[] OutMVAR = new float[Windings.Length];
                for (int a = 0; a < Windings.Length; a++)
                    OutMVAR[a] = Windings[a].Estimated_MVAR;
                return OutMVAR;
            }
            set
            {
                for (int a = 0; a < value.Length; a++)
                    Windings[a].Estimated_MVAR = value[a];
            }
        }



        /// <summary>Telemetered MVAR flow at each terminal</summary>
        public float[] Telemetered_MVAR
        {
            get
            {
                float[] OutMVAR = new float[Windings.Length];
                for (int a = 0; a < Windings.Length; a++)
                    OutMVAR[a] = Windings[a].Telemetered_MVAR;
                return OutMVAR;
            }
            set
            {
                for (int a = 0; a < value.Length; a++)
                    Windings[a].Telemetered_MVAR = value[a];
            }
        }

        /// <summary>Estimated MVA flow at each terminal</summary>
        public float[] Estimated_MVA
        {
            get
            {
                float[] OutMVA = new float[Windings.Length];
                for (int a = 0; a < Windings.Length; a++)
                    OutMVA[a] = Windings[a].Estimated_MVA;
                return OutMVA;
            }
            set
            {
                for (int a = 0; a < value.Length; a++)
                    Windings[a].Estimated_MVA = value[a];
            }
        }



        /// <summary>Telemetered MVA flow at each terminal</summary>
        public float[] Telemetered_MVA
        {
            get
            {
                float[] OutMVA = new float[Windings.Length];
                for (int a = 0; a < Windings.Length; a++)
                    OutMVA[a] = Windings[a].Telemetered_MVA;
                return OutMVA;
            }
            set
            {
                for (int a = 0; a < value.Length; a++)
                    Windings[a].Telemetered_MVA = value[a];
            }
        }

        /// <summary>Voltage levels</summary>
        public float[] Voltage
        {
            get
            {
                float[] OutVoltages = new float[Windings.Length];
                for (int a = 0; a < Windings.Length; a++)
                    if (Windings[a].Voltage != 0 && !float.IsNaN(Windings[a].Voltage))
                        OutVoltages[a] = Windings[a].Voltage;
                    else
                        OutVoltages[a] = Windings[a].KVLevel.Nominal;
                return OutVoltages;
            }
            set
            {
                for (int a = 0; a < value.Length; a++)
                    Windings[a].Voltage = value[a];
            }
        }

        /// <summary>Transformer tap positions</summary>
        public float[] Estimated_Tap
        {
            get
            {
                float[] OutTap = new float[Windings.Length];
                for (int a = 0; a < Windings.Length; a++)
                    OutTap[a] = Windings[a].Tap;
                return OutTap;
            }
            set
            {
                for (int a = 0; a < value.Length; a++)
                    Windings[a].Tap = value[a];
            }
        }

        /// <summary>Get/set the normal limit of our winding</summary>
        public float NormalLimit
        {
            get { return Limits[0]; }
            set { Limits[0] = value; }
        }

        /// <summary>Get/set the load shed limit of our winding</summary>
        public float LoadshedLimit
        {
            get { return Limits[1]; }
            set { Limits[1] = value; }
        }

        /// <summary>Get/set the emergency limit of our winding</summary>
        public float EmergencyLimit
        {
            get { return Limits[2]; }
            set { Limits[2] = value; }
        }

        /// <summary>The limits of MVA flow on the transformer</summary>
        public float[] Limits = new float[] { float.NaN, float.NaN, float.NaN };

        /// <summary>The transformer windings of the transformer</summary>
        public MM_TransformerWinding[] Windings = new MM_TransformerWinding[] { new MM_TransformerWinding(), new MM_TransformerWinding() };

        /// <summary>Whether the transformer is a phase shifter</summary>
        public bool PhaseShifter;

        /// <summary>The first transformer winding</summary>
        public MM_TransformerWinding Winding1
        {
            get { return Windings[0]; }
            set { Windings[0] = value; }
        }

        /// <summary>The second transformer winding</summary>
        public MM_TransformerWinding Winding2
        {
            get { return Windings[1]; }
            set { Windings[1] = value; }
        }


        /// <summary>Report the names of our windings</summary>
        public String[] WindingName
        {
            get { return new String[] { Winding1.Name, Winding2.Name }; }
            set { Winding1.Name = value[0]; Winding2.Name = value[1]; }
        }

        /// <summary>The first transformer KVLevel</summary>
        public MM_KVLevel KVLevel1
        {
            get { return Windings[0].KVLevel; }
            set { Windings[0].KVLevel = value; }
        }

        /// <summary>The second transformer KVLevel</summary>
        public MM_KVLevel KVLevel2
        {
            get { return Windings[1].KVLevel; }
            set { Windings[1].KVLevel = value; }
        }

        /// <summary>Whether our transformer is considered to have both legs on transmission voltages</summary>
        public bool IsAutoTransformer { get; set; }

        /// <summary></summary>
        public bool AVR;


        /// <summary></summary>
        public bool WAVR;

        /// <summary></summary>
        public bool WAVRN;

        /// <summary></summary>
        public bool OffAVR;

        /// <summary></summary>
        public bool ManualTarget;

        /// <summary></summary>
        public int[] TapMin = new int[2];

        /// <summary></summary>
        public int[] TapMax = new int[2];

        /// <summary></summary>
        public float MVARating = float.NaN;

        /// <summary></summary>
        public float TargetVoltage = float.NaN;

        /// <summary></summary>
        public float TargetDeviation = float.NaN;

        /// <summary></summary>
        public float RegulationTargetVoltage = float.NaN;

        /// <summary></summary>
        public float RegulationDeviation = float.NaN;

        /// <summary>Whether the transformer is regulated (containing a regulated note from EMS)</summary>
        public bool Regulated = false;

        /// <summary>Phase shifter data</summary>
        public MM_Transformer_PhaseShifter_Data PhaseShifterData = null;

        /// <summary>Our primary winding</summary>
        public MM_TransformerWinding PrimaryWinding
        {
            get { return Windings[0].WindingType == MM_TransformerWinding.enumWindingType.Primary ? Windings[0] : Windings[1]; }
        }

        /// <summary>Our secondary winding</summary>
        public MM_TransformerWinding SecondaryWinding
        {
            get { return Windings[0].WindingType == MM_TransformerWinding.enumWindingType.Secondary ? Windings[0] : Windings[1]; }            
        }

        #endregion

        #region Data retrieval
        /// <summary>
        /// Return the substation to which the MVA flow is heading
        /// </summary>
        public MM_KVLevel MVAFlowDirection
        {
            get
            {
                if (Estimated_MVA[0] > 0)
                    return KVLevel2;
                else
                    return KVLevel1;
            }
        }
        /// <summary>
        /// Return the largest MVA flow (in absolute values)
        /// </summary>
        public float MVAFlow
        {
            get
            {
                return Math.Max(Math.Abs(Estimated_MVA[0]), Math.Abs(Estimated_MVA[1]));
            }
        }

        /// <summary>
        /// Return the largest MW flow (in absolute values)
        /// </summary>
        public float MVARFlow
        {
            get
            {
                return Math.Max(Math.Abs(Estimated_MVAR[0]), Math.Abs(Estimated_MVAR[1]));
            }
        }

        /// <summary>
        /// Return the largest MW flow (in absolute values)
        /// </summary>
        public float MWFlow
        {
            get
            {
                return Math.Max(Math.Abs(Estimated_MW[0]), Math.Abs(Estimated_MW[1]));
            }
        }

        /// <summary>
        /// Return the substation to which the MVAR flow is heading
        /// </summary>
        public MM_KVLevel MVARFlowDirection
        {
            get
            {
                if ((Estimated_MVAR[0] > 0) && (Estimated_MVAR[1] < 0))
                    return KVLevel1;
                else if ((Estimated_MVAR[0] < 0) && (Estimated_MVAR[1] > 0))
                    return KVLevel2;
                else
                    return null;
            }
        }

        /// <summary>
        /// Return a string of KV levels within the substation
        /// </summary>
        public String KVLevelList
        {
            get
            {
                return (KVLevel1 != null ? KVLevel1.Name.Split(' ')[0] + "," : "") + (KVLevel2 != null ? KVLevel2.Name.Split(' ')[0] : "");
            }
        }

        /// <summary>
        /// Return the percentage this transformer is loaded
        /// </summary>
        public String FlowPercentageText
        {
            get
            {
                float CurMVA = Math.Max(Math.Abs(Estimated_MVA[0]), Math.Abs(Estimated_MVA[1]));
                if (Limits[0] == 0)
                    return "?";
                else if (CurMVA <= Limits[0])
                    return (CurMVA / Limits[0]).ToString("0%") + " Norm";
                else if (CurMVA <= Limits[1])
                    return (CurMVA / Limits[1]).ToString("0%") + " 2 Hr";
                else
                    return (CurMVA / Limits[2]).ToString("0%") + " 15 Min";
            }
        }

        /// <summary>
        /// Return the substation to which the flow is heading
        /// </summary>
        public MM_KVLevel MWFlowDirection
        {
            get
            {
                if (Estimated_MW[0] > 0)
                    return KVLevel1;
                else
                    return KVLevel2;
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new CIM Transformer
        /// </summary>
        /// <param name="ElementSource">The XML source for this line</param>
        /// <param name="AddIfNew">Whether to add the new element</param>
        public MM_Transformer(XmlElement ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        { }


        /// <summary>
        /// Instantiate a new transformer element from the CIM Server
        /// </summary>
        /// <param name="ElementSource">The data row for the element</param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Transformer(DbDataReader ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
            this.ElemType = MM_Repository.FindElementType("Transformer");
            foreach (MM_TransformerWinding Winding in Windings)
            {
                Winding.Transformer = this;
                Winding.Owner = this.Owner;
                Winding.Operator = this.Operator;
            }
        }


        /// <summary>
        /// Initialize a blank transformer
        /// </summary>
        public MM_Transformer()
        {
            this.ElemType = MM_Repository.FindElementType("Transformer");
        }

        /// <summary>
        /// Check an element for its value, and add it in.
        /// </summary>
        /// <param name="ElementSource"></param>
        /// <param name="AttributeName"></param>
        /// <param name="OutValues"></param>
        private void CheckElementValue(XmlElement ElementSource, string AttributeName, ref float[] OutValues)
        {
            if (!ElementSource.HasAttribute(AttributeName))
                return;
            String[] splStr = ElementSource.Attributes[AttributeName].Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int a = 0; a < OutValues.Length; a++)
                OutValues[a] = MM_Converter.ToSingle(splStr[a]);
        }

        #endregion
    }
}
