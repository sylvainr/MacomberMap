using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEDESimulator.Elements
{
    public class Sim_Transformer : Sim_Element
    {
        public Sim_Substation Substation;
        public int TEID {get;set;}
        public String Name;
        public Sim_Company Owner;
        public double MW_Est, MVAR_Est;
        public Sim_Company Operator;
        public Sim_TransformerWinding Winding1, Winding2;
        public Guid ElemGuid { get; set; }

        /// <summary>
        /// Initialize a new transformer
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Sub"></param>
        /// <param name="MW_Est"></param>
        /// <param name="MVAR_Est"></param>
        /// <param name="Bus1"></param>
        /// <param name="Bus2"></param>
        /// <param name="Builder"></param>
        public Sim_Transformer(String Name, Sim_Substation Sub, double MW_Est, double MVAR_Est, Sim_Bus Bus1, Sim_Bus Bus2, Sim_Builder Builder)
        {
            this.Name = Name;
            this.Substation = Sub;
            this.ElemGuid = Guid.NewGuid();
            this.Substation.Elements.Add(this);
            this.Owner = Builder.GetOwner();
            this.Operator = Builder.GetOperator();
            this.MW_Est = MW_Est;
            this.MVAR_Est = MVAR_Est;
            this.TEID = Builder.NextTEID();
            this.Winding1 = new Sim_TransformerWinding(Name + "_1", this, Bus1, Owner, Operator, true, Builder);
            this.Winding2 = new Sim_TransformerWinding(Name + "_2", this, Bus2, Owner, Operator, false, Builder);
        }

        public double MVA_Est { get { return Math.Sqrt((MW_Est * MW_Est) + (MVAR_Est * MVAR_Est)); } }
        public bool IsPhaseShifter { get { return Winding1.Bus.Voltage == Winding2.Bus.Voltage; } }
        public string ElemType
        {
            get { return "Transformer"; }
        }

        /// <summary>
        /// Build our outgoing XML for our transformer, and write out our savecase data
        /// </summary>
        /// <param name="Builder"></param>
        /// <returns></returns>
        public String GetXml(Sim_Builder Builder)
        {
            Builder.Savecase.TransformerData.Add(new MacomberMapCommunications.Messages.EMS.MM_Transformer_Data() { AVR_XF = true, End1_Bus = Winding1.Bus.BusNumber, End2_Bus = Winding2.Bus.BusNumber, LIMIT1 = (float)MVA_Est * 1.2f, LIMIT2 = (float)MVAR_Est * 1.3f, LIMIT3 = (float)MVAR_Est * 1.4f, MVA_End1 = (float)MVA_Est, MVA_End2 = (float)-MVA_Est, Open_End1 = MW_Est == 0, Open_End2 = MW_Est == 0, R_End1 = (float)MVAR_Est, RMeas_End2 = (float)MVAR_Est, RMeas_End1 = (float)MVAR_Est, R_End2 = (float)-MVAR_Est, Tap = 5, TapMax = 8, TapMin = 2, ZTap = 4, ZTapMax = 7, ZTapMin = 3, WMeas_End1 = (float)MW_Est, WMeas_End2 = (float)-MW_Est, TEID_Xf = TEID, TEID_End1 = Winding1.TEID, TEID_REGND = Winding1.Bus.BusNumber, W_End1 = (float)MW_Est, W_End2 = (float)-MW_Est });
            return $"<{ElemType} TEID=\"{TEID}\" Name=\"{Name}\" Substation=\"{Substation.TEID}\"  Owner=\"{Owner.TEID}\" Operator=\"{Operator.TEID}\" ElemType=\"{ElemType}\" Winding1=\"{Winding1.TEID}\" Winding2=\"{Winding2.TEID}\" KVLevel1=\"{Winding1.Bus.Voltage}\" KVLevel2=\"{Winding2.Bus.Voltage}\" Windings=\"{Winding1.TEID},{Winding2.TEID}\" PhaseShifter=\"{IsPhaseShifter}\">\r\n\t\t" + Winding1.GetXml(Builder) + "\r\n\t\t" + Winding2.GetXml(Builder) + "\r\n\t</Transformer>";
        }

        public Sim_Bus[] GetBuses()
        {
            return new Sim_Bus[] { Winding1.Bus, Winding2.Bus };
        }

        /// <summary>
        /// Report an easy to read string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Transformer {Name} {Winding1.Bus.Voltage.NominalVoltage} KV to {Winding2.Bus.Voltage.NominalVoltage} KV";
        }

        public string GetOneLineXml(ref int XPos, int YPos)
        {
            StringBuilder sB = new StringBuilder();
            int XWidth = this.Name.Length * 8;
            int XPosText = XPos + 30;
            sB.AppendLine($"\t<{ElemType} BaseElement.Permitted=\"true\" ToplogyValidated=\"true\" Collision=\"false\" DescriptorArrow=\"false\" Orientation=\"Vertical\" Bounds=\"{XPos},{YPos},25,26\" Visible=\"true\" Text=\"{Name}\" rdfID=\"{ElemGuid}\" BaseElement.ElemType=\"{ElemType}\" BaseElement.KVLevel=\"{Winding1.Bus.Voltage}\" BaseElement.KVLevel1=\"{Winding1.Bus.Voltage}\" BaseElement.Winding1=\"{Winding1.TEID}\" BaseElement.KVLevel2=\"{Winding2.Bus.Voltage}\" BaseElement.Winding2=\"{Winding2.TEID}\" BaseElement.Windings=\"{Winding1.TEID},{Winding2.TEID}\" BaseElement.PhaseShifter=\"false\" BaseElement.Name=\"{Name}\" BaseElement.TEID=\"{TEID}\" BaseElement.Owner=\"{Owner.TEID}\" BaseElement.Operator=\"{Operator.TEID}\" BaseElement.Substation=\"{Substation.TEID}\" BaseElement.SubLongName=\"{Substation.Name}\" PUNElement=\"False\" BaseElement.SubName=\"{Substation.Name}\" BaseElement.TransformerName=\"{Name}\">");
            sB.AppendLine($"\t\t<Descriptor ToplogyValidated=\"false\" DescriptorArrow=\"false\" Orientation=\"Unknown\" Bounds=\"{XPosText},{YPos},{XWidth},14\" Font=\"Microsoft Sans Serif, 8.25pt\" ForeColor=\"Gray\" LabelBounds=\"0,-27,88,73,\" Collision=\"false\" Visible=\"true\" Text=\"{Name}\" rdfID=\"{ElemGuid}\" BaseElement.TEID=\"{TEID}\" BaseElement.ElemType=\"{ElemType}\" BaseElement.Owner=\"{Owner.TEID}\" BaseElement.Operator=\"{Operator.TEID}\" BaseElement.KVLevel=\"{Winding1.Bus.Voltage}\" BaseElement.Substation=\"{Substation.TEID}\" BaseElement.SubName=\"{Substation.Name}\" BaseElement.SubLongName=\"{Substation.Name}\" BaseElement.KVLevel1=\"{Winding1.Bus.Voltage}\" BaseElement.Winding1=\"{Winding1.TEID}\" BaseElement.KVLevel2=\"{Winding2.Bus.Voltage}\" BaseElement.Winding2=\"{Winding2.TEID}\" BaseElement.Windings=\"{Winding1.TEID},{Winding2.TEID}\" BaseElement.PhaseShifter=\"false\" BaseElement.Name=\"{Name}\" />");
            sB.AppendLine($"\t\t<Winding BaseElement.ElemType=\"{Winding1.ElemType}\" BaseElement.TEID=\"{Winding1.TEID}\" BaseElement.Name=\"{Winding1.Name}\" BaseElement.Operator=\"{Winding1.Operator.TEID}\" BaseElement.Owner=\"{Winding1.Owner.TEID}\" BaseElement.Permitted=\"true\" BaseElement.SubName=\"{Substation.Name}\" BaseElement.Substation=\"{Substation.TEID}\" BaseElement.Transformer=\"{TEID}\" Orientation=\"Up\" Bounds=\"0,0,25,13\" rdfID=\"{Winding1.ElemGuid}\" Visible=\"true\" BaseElement.IsPrimary=\"{Winding1.IsPrimary}\" BaseElement.AssociatedNodeName=\"{Winding1.WindingNodeTEID}\" BaseElement.AssociatedNode=\"{Winding1.WindingNodeTEID}\" BaseElement.KVLevel=\"{Winding1.Bus.Voltage}\"/>");
            sB.AppendLine($"\t\t<Winding BaseElement.ElemType=\"{Winding2.ElemType}\" BaseElement.TEID=\"{Winding2.TEID}\" BaseElement.Name=\"{Winding2.Name}\" BaseElement.Operator=\"{Winding2.Operator.TEID}\" BaseElement.Owner=\"{Winding2.Owner.TEID}\" BaseElement.Permitted=\"true\" BaseElement.SubName=\"{Substation.Name}\" BaseElement.Substation=\"{Substation.TEID}\" BaseElement.Transformer=\"{TEID}\" Orientation=\"Down\" Bounds=\"0,13,25,13\" rdfID=\"{Winding2.ElemGuid}\" Visible=\"true\" BaseElement.IsPrimary=\"{Winding2.IsPrimary}\" BaseElement.AssociatedNodeName=\"{Winding2.WindingNodeTEID}\" BaseElement.AssociatedNode=\"{Winding2.WindingNodeTEID}\" BaseElement.KVLevel=\"{Winding2.Bus.Voltage}\"/>");
            sB.Append($"\t</{ElemType}>");
            XPos = XPosText + XWidth + 10;
            return sB.ToString();
        }
    }
}
