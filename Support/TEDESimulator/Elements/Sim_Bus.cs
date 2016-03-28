using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEDESimulator.Elements
{
    /// <summary>
    /// © 2016, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class holds information on a simulated bus element
    /// </summary>
    public class Sim_Bus: Sim_Element
    {
        public Sim_Substation Substation;
        public Sim_VoltageLevel Voltage;
        public int TEID {get;set;}
        public int NodeTEID;
        public String Name;
        public double PerUnitVoltage;
        public Sim_Company Owner;
        public Sim_Company Operator;
        public int BusNumber;
        public int Y;
        public Guid ElemGuid { get; set; }
        public Guid NodeGuid;
        public int XWidth;

        public double CurrentVoltage
        { get { return Voltage.NominalVoltage * PerUnitVoltage; } }

        public string ElemType
        {
            get
            {
                return "Node";
            }
        }

        /// <summary>
        /// Initialize our bus
        /// </summary>
        /// <param name="Sub"></param>
        /// <param name="BusNumber"></param>
        /// <param name="Name"></param>
        /// <param name="PerUnitVoltage"></param>
        /// <param name="Builder"></param>
        public Sim_Bus(Sim_Substation Sub, int BusNumber, String Name, double PerUnitVoltage, Sim_Builder Builder)
        {
            this.ElemGuid = Guid.NewGuid();
            this.Name = Name;
            this.Substation = Sub;
            this.Voltage = Builder.GetVoltage();
            this.PerUnitVoltage = PerUnitVoltage;
            this.Substation.Elements.Add(this);
            this.BusNumber = BusNumber;
            this.Owner = Builder.GetOwner();
            this.ElemGuid = Guid.NewGuid();
            this.NodeGuid = Guid.NewGuid();
            this.Operator = Builder.GetOperator();
            this.TEID = Builder.NextTEID();
            this.NodeTEID = Builder.NextTEID();
        }

        /// <summary>
        /// Write out our XML for our bus, and our savecase information
        /// </summary>
        /// <returns></returns>
        public String GetXml(Sim_Builder Builder)
        {
            Builder.Savecase.BusData.Add(BusNumber, new MacomberMapCommunications.Messages.EMS.MM_Bus_Data() { Angle = 0, Bus_Num = BusNumber, Dead = PerUnitVoltage == 0, Island_ID = 0, NomKv = (float)Voltage.NominalVoltage, Open = PerUnitVoltage == 0, TEID_Nd = NodeTEID, TEID_St = Substation.TEID, V_Pu = (float)PerUnitVoltage });
            return $"\r\n<Node Name=\"{Name}\" TEID=\"{NodeTEID}\" Owner=\"{Owner.TEID}\" Operator=\"{Operator.TEID}\" ElemType=\"Node\" KVLevel=\"{Voltage}\" Substation=\"{Substation.TEID}\" />\r\n<BusbarSection PUNElement=\"False\" Name=\"{Name}\" TEID=\"{TEID}\" Owner=\"{Owner.TEID}\" Operator=\"{Operator.TEID}\" ElemType=\"BusbarSection\" NodeTEID=\"{NodeTEID}\" KVLevel=\"{Voltage}\" Substation=\"{Substation.TEID}\" />";
        }

        public Sim_Bus[] GetBuses()
        {
            return new Sim_Bus[] { this };
        }

        /// <summary>
        /// Report an easy to read string for our bus name
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Bus {Name}: {CurrentVoltage:0} KV";
        }

        public string GetOneLineXml(ref int XPos, int YPos)
        {
            StringBuilder sB = new StringBuilder();
            //Determine all of the elements attached to our bus
            String ConnectedElements = "";
            List<Sim_Element> Elements = new List<Sim_Element>();
            foreach (Sim_Element Elem in Substation.Elements)
                if (Elem is Sim_Bus == false && Elem.GetBuses().Contains(this))
                {
                    Elements.Add(Elem);
                    ConnectedElements += (ConnectedElements == "" ? "" : ",") + Elem.TEID.ToString();
                }

            sB.AppendLine($"\t<{ElemType} IsVisible =\"true\" Bounds=\"{XPos},{YPos},{XWidth},8\" Orientation=\"Vertical\" Text=\"{Name}\" rdfID=\"{NodeGuid}\" BaseElement.ElemType=\"{ElemType}\" BaseElement.KVLevel=\"{Voltage}\" BaseElement.Name=\"{Name}\" BaseElement.TEID=\"{NodeTEID}\" BaseElement.Owner=\"{Owner.TEID}\" BaseElement.Operator=\"{Operator.TEID}\" BaseElement.Substation=\"{Substation.TEID}\" BaseElement.SubLongName=\"{Substation.Name}\" PUNElement=\"False\" BaseElement.SubName=\"{Substation.Name}\">");

           

            foreach (Sim_Element Element in Elements)
                sB.AppendLine($"\t\t<{Element.ElemType} rdfID=\"{Element.ElemGuid}\" TEID=\"{Element.TEID}\"/>");
            sB.Append($"\t</{ElemType}>");
            return sB.ToString();
        }
    }
}
