using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEDESimulator.Elements
{
    public class Sim_TransformerWinding: Sim_Element
    {
        public Sim_Substation Substation;
        public int TEID {get;set;}
        public String Name;
        public Sim_Company Owner;
        public Sim_Company Operator;
        public Sim_Bus Bus;
        public bool IsPrimary;
        public Sim_Transformer Transformer;
        public int WindingNodeTEID;

        public Sim_TransformerWinding(string Name, Sim_Transformer Transformer, Sim_Bus Bus, Sim_Company Owner, Sim_Company Operator, bool IsPrimary, Sim_Builder Builder)
        {
            this.Name = Name;
            this.TEID = Builder.NextTEID();
            this.Owner = Owner;
            this.Operator = Operator;
            this.Bus = Bus;
            this.IsPrimary = IsPrimary;
            this.Transformer = Transformer;
            this.Substation = Transformer.Substation;
            this.WindingNodeTEID = Builder.NextTEID();
        }

        public String WindingType { get { return IsPrimary ? "primary" : "secondary"; } }
        public string ElemType
        {
            get { return "TransformerWinding"; }
        }

        public Guid ElemGuid { get; set; }

        public String GetXml(Sim_Builder Builder)
        {
            return $"<{ElemType} TEID=\"{TEID}\" Name=\"{Name}\" Substation=\"{Substation.TEID}\"  Transformer=\"{Transformer.TEID}\" WindingType=\"{WindingType}\" WindingNodeTEID=\"{WindingNodeTEID}\" Owner=\"{Owner.TEID}\" Operator=\"{Operator.TEID}\" ElemType=\"{ElemType}\"  KVLevel=\"{Bus.Voltage}\"/>";
        }

        public Sim_Bus[] GetBuses()
        {
            return new Sim_Bus[] { Bus };
        }

        public string GetOneLineXml(ref int XPos, int YPos)
        {
            return "";
        }
    }
}
