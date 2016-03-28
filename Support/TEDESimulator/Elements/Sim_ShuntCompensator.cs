using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEDESimulator.Elements
{
    public class Sim_ShuntCompensator: Sim_Element
    {
        public Sim_Substation Substation;
        public int TEID {get;set;}
        public String Name;
        public Sim_Company Owner;
        public Sim_Company Operator;
        public Sim_Bus Bus;
        public bool Open;
        public bool IsCapacitor;
        public double Estimated_MVAR = float.NaN;
        public double Nominal_MVAR = float.NaN;


        public Sim_ShuntCompensator(String Name, Sim_Substation Sub, Sim_Bus Bus, bool Open,  double MVAR_Est, double MVAR_Nominal, bool IsCapacitor, Sim_Builder Builder)
        {
            this.Name = Name;
            this.Substation = Sub;
            this.Bus = Bus;
            this.Owner = Builder.GetOwner();
            this.Operator = Builder.GetOperator();
            this.TEID = Builder.NextTEID();
            this.Open = Open;
            this.Nominal_MVAR = MVAR_Nominal;
            Sub.Elements.Add(this);
            this.Estimated_MVAR = MVAR_Est;
            this.IsCapacitor = IsCapacitor;
            this.ElemGuid = Guid.NewGuid();
        }

        public string ElemType
        {
            get { return IsCapacitor ? "Capacitor" : "Reactor"; }
        }

        public Guid ElemGuid { get; set; }

        /// <summary>
        /// Generate the XML and savecase information for our shunt compensator
        /// </summary>
        /// <param name="Builder"></param>
        /// <returns></returns>
        public String GetXml(Sim_Builder Builder)
        {
            return $"<{ElemType} TEID=\"{TEID}\" Name=\"{Name}\" Substation=\"{Substation.TEID}\"  Owner=\"{Owner.TEID}\" Operator=\"{Operator.TEID}\" ElemType=\"{ElemType}\" KVLevel=\"{Bus.Voltage}\" Open=\"{Open}\" Nominal_MVAR=\"{Nominal_MVAR}\" Estimated_MVAR=\"{Estimated_MVAR}\"/>";
        }

        public Sim_Bus[] GetBuses()
        {
            return new Sim_Bus[] { Bus };
        }

        public override string ToString()
        {
            return $"{ElemType} {Name} {Bus.Voltage.NominalVoltage:0} KV, {Nominal_MVAR} MVAR Nominal, Open {Open}";
        }

        public string GetOneLineXml(ref int XPos, int YPos)
        {
            int XPosText = XPos + 30;
            int XTextWidth = 8 * Name.Length;
            String OutString= $"\t<{ElemType} BaseElement.Permitted=\"true\" ToplogyValidated=\"true\" Collision=\"false\" KVLevel=\"{Bus.Voltage}\" DescriptorArrow=\"false\" Orientation=\"Right\" Bounds=\"{XPos},{YPos},32,25\" Visible=\"true\" Text=\"{Name}\" rdfID=\"{ElemGuid}\" BaseElement.ElemType=\"{ElemType}\" BaseElement.KVLevel=\"{Bus.Voltage}\" BaseElement.Name=\"{Name}\" BaseElement.TEID=\"{TEID}\" BaseElement.Owner=\"{Owner.TEID}\" BaseElement.Operator=\"{Operator.TEID}\" BaseElement.Substation=\"{Substation.TEID}\" BaseElement.SubLongName=\"{Substation.Name}\" PUNElement=\"False\" BaseElement.SubName=\"{Substation.Name}\">\r\n\t\t<Descriptor ToplogyValidated=\"false\" DescriptorArrow=\"false\" Orientation=\"Unknown\" Bounds=\"{XPosText},{YPos},{XTextWidth},14\" Font=\"Microsoft Sans Serif, 8.25pt\" ForeColor=\"Gray\" LabelBounds=\"-9,0,46,20,\" Collision=\"false\" Visible=\"true\" Text=\"{Name}\" rdfID=\"{ElemGuid}\" BaseElement.TEID=\"{TEID}\" BaseElement.ElemType=\"{ElemType}\" BaseElement.Owner=\"{Owner.TEID}\" BaseElement.Operator=\"{Operator.TEID}\" BaseElement.KVLevel=\"{Bus.Voltage}\" BaseElement.Substation=\"{Substation.TEID}\" BaseElement.SubName=\"{Substation.Name}\" BaseElement.SubLongName=\"{Substation.Name}\" BaseElement.Name=\"{Name}\" />\r\n\t</{ElemType}>";
            XPos = XPosText + XTextWidth + 10;
            return OutString;
        }
    }
}
