using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEDESimulator.Elements
{
    public class Sim_Load: Sim_Element
    {
        public Sim_Substation Substation;
        public int TEID {get;set;}
        public String Name;
        public double MW_Est, MVAR_Est;
        public double MVA_Est { get { return Math.Round(Math.Sqrt(MW_Est * MW_Est + MVAR_Est * MVAR_Est)); } }
        public Sim_Bus Bus;
        public Sim_Company Owner;
        public Sim_Company Operator;
        public Guid ElemGuid { get; set; }
        public bool IsControllable;

        public string ElemType
        {
            get
            {
                return IsControllable?"LaaR": "Load";
            }
        }

        public Sim_Load(String Name, Sim_Substation Parent, Sim_Bus Bus, double MW_Est, double MVAR_Est, Sim_Builder Builder)
        {
            this.Substation = Parent;
            this.ElemGuid = Guid.NewGuid();
            this.Substation.Elements.Add(this);
            this.Name = Name;
            this.MW_Est = MW_Est;
            this.MVAR_Est = MVAR_Est;
            this.Owner = Builder.GetOwner();
            this.Operator = Builder.GetOperator();
            this.TEID = Builder.NextTEID();
            this.Bus = Bus;
        }


        /// <summary>
        /// Write out the XML for our load, and add our load state to our savecase
        /// </summary>
        /// <param name="Builder"></param>
        /// <returns></returns>
        public String GetXml(Sim_Builder Builder)
        {
            Builder.Savecase.LoadData.Add(new MacomberMapCommunications.Messages.EMS.MM_Load_Data() { Conn_Bus = Bus.BusNumber, Manual_Ld = false, Open_Ld = MW_Est == 0, W_Ld = (float)MW_Est, Remove_Ld = false, RMeas_Ld = (float)MVAR_Est, R_Ld = (float)MVAR_Est, WMeas_Ld = (float)MW_Est, TEID_Ld = TEID, RMVEnabl_Ld = true });
            return $"<{ElemType} TEID=\"{TEID}\" Name=\"{Name}\" Substation=\"{Substation.TEID}\"  Owner=\"{Owner.TEID}\" Operator=\"{Operator.TEID}\" ElemType=\"Load\" KVLevel=\"{Bus.Voltage}\" Estimated_MW=\"{MW_Est}\" Estimated_MVAR=\"{MVAR_Est}\" Estimated_MVA=\"{MVA_Est}\" Telemetered_MW=\"{MW_Est}\" Telemetered_MVAR=\"{MVAR_Est}\" Telemetered_MVA=\"{MVA_Est}\"/>";
        }

        public Sim_Bus[] GetBuses()
        {
            return new Sim_Bus[] { Bus };
        }

        public override string ToString()
        {
            return $"Load {Name}: {Bus.Voltage.NominalVoltage} KV, {MW_Est:0} MW";
        }

        public string GetOneLineXml(ref int XPos, int YPos)
        {
            int XPosText = XPos + 30;
            int XTextWidth = 8 * Name.Length;
            String OutString = $"\t<{ElemType} BaseElement.Permitted=\"true\" ToplogyValidated=\"true\" Collision=\"false\" KVLevel=\"{Bus.Voltage}\" DescriptorArrow=\"false\" Orientation=\"Right\" Bounds=\"{XPos},{YPos},32,25\" Visible=\"true\" Text=\"{Name}\" rdfID=\"{ElemGuid}\" BaseElement.ElemType=\"{ElemType}\" BaseElement.KVLevel=\"{Bus.Voltage}\" BaseElement.Name=\"{Name}\" BaseElement.TEID=\"{TEID}\" BaseElement.Owner=\"{Owner.TEID}\" BaseElement.Operator=\"{Operator.TEID}\" BaseElement.Substation=\"{Substation.TEID}\" BaseElement.SubLongName=\"{Substation.Name}\" PUNElement=\"False\" BaseElement.SubName=\"{Substation.Name}\">\r\n\t\t<Descriptor ToplogyValidated=\"false\" DescriptorArrow=\"false\" Orientation=\"Unknown\" Bounds=\"{XPosText},{YPos},{XTextWidth},14\" Font=\"Microsoft Sans Serif, 8.25pt\" ForeColor=\"Gray\" LabelBounds=\"-9,0,46,20,\" Collision=\"false\" Visible=\"true\" Text=\"{Name}\" rdfID=\"{ElemGuid}\" BaseElement.TEID=\"{TEID}\" BaseElement.ElemType=\"{ElemType}\" BaseElement.Owner=\"{Owner.TEID}\" BaseElement.Operator=\"{Operator.TEID}\" BaseElement.KVLevel=\"{Bus.Voltage}\" BaseElement.Substation=\"{Substation.TEID}\" BaseElement.SubName=\"{Substation.Name}\" BaseElement.SubLongName=\"{Substation.Name}\" BaseElement.Name=\"{Name}\" />\r\n\t</{ElemType}>";
            XPos = XPosText + XTextWidth + 10;
            return OutString;
        }
    }
}
