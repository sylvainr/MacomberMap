using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEDESimulator.Elements
{
    public class Sim_Breaker : Sim_Element
    {
        public Sim_Substation Substation;
        public int TEID {get;set;}
        public String Name;
        public Sim_Company Owner;
        public Sim_Company Operator;
        public Sim_Bus NearBus, FarBus;
        public bool Open;
        public bool IsBreaker;
       public  Guid ElemGuid { get; set; }

        public string ElemType
        {
            get
            {
                return IsBreaker ? "Breaker" : "Switch";
            }
        }

        public Sim_Breaker(String Name, Sim_Substation Sub, Sim_Bus NearBus, Sim_Bus FarBus, Sim_Builder Builder, bool Open, bool IsBreaker)
        {
            this.Name = Name;
            this.ElemGuid = Guid.NewGuid();
            this.IsBreaker = IsBreaker;
            this.Substation = Sub;
            this.NearBus = NearBus;
            this.FarBus = FarBus;
            this.Owner = Builder.GetOwner();
            this.Operator = Builder.GetOperator();
            this.TEID = Builder.NextTEID();
            this.Open = Open;
            this.Substation.Elements.Add(this);
        }

        /// <summary>
        /// Retrieve the XML for our breaker, and write out a breaker status in our savecase
        /// </summary>
        /// <param name="Builder"></param>
        /// <returns></returns>
        public String GetXml(Sim_Builder Builder)
        {
            Builder.Savecase.BreakerSwitchData.Add(new MacomberMapCommunications.Messages.EMS.MM_BreakerSwitch_Data() { TEID_CB = TEID, Open_CB = Open, Near_BS = NearBus.BusNumber, Far_BS = FarBus.BusNumber });
            return $"<Breaker Name=\"{Name}\" TEID=\"{TEID}\" PUNElement=\"False\" Owner=\"{Owner.TEID}\" Operator=\"{Operator.TEID}\" ElemType=\"Breaker\" KVLevel=\"{NearBus.Voltage}\" Substation=\"{Substation.TEID}\" HasSynchrocheckRelay=\"false\" HasSynchroscope=\"false\" NormalOpen=\"false\"/>";
        }

        public Sim_Bus[] GetBuses()
        {
            return new Sim_Bus[] { NearBus, FarBus };
        }

        public override string ToString()
        {
            return $"Breaker {Name}: Open: {Open}, Voltage {NearBus.Voltage.NominalVoltage}";
        }

        public string GetOneLineXml(ref int XPos, int YPos)
        {
            Guid BreakerGuid = Guid.NewGuid();
            int XWidth = Name.Length * 8;
            int XPosText = XPos + 30;
            string OpenChecked = Open ? "Checked" : "Unchecked";
            String OutLine = $"\t<{ElemType} IsVisible=\"true\" Bounds=\"{XPos},{YPos},26,26\" Orientation=\"Vertical\" Text=\"{Name}\" rdfID=\"{BreakerGuid}\" BaseElement.ElemType=\"Breaker\" BaseElement.KVLevel=\"{NearBus.Voltage}\" Opened=\"{OpenChecked}\" ScadaOpened=\"{OpenChecked}\" BaseElement.Name=\"{Name}\" BaseElement.TEID=\"{TEID}\" BaseElement.Owner=\"{Owner.TEID}\" BaseElement.Operator=\"{Operator.TEID}\" BaseElement.Substation=\"{Substation.TEID}\" BaseElement.SubLongName=\"{Substation.Name}\" BaseElement.HasSynchroscope=\"False\" BaseElement.HasSynchrocheckRelay=\"False\" PUNElement=\"False\" BaseElement.SubName=\"{Substation.Name}\">\r\n\t\t<Descriptor DescriptorText=\"{Name}\" Bounds=\"{XPosText},{YPos},{XWidth},14\" LabelBounds=\"{XPos},0,50,17\" Font=\"Arial, 8pt\" Text=\"{Name}\" rdfID=\"{BreakerGuid}\" BaseElement.TEID=\"{TEID}\" BaseElement.ElemType=\"Breaker\" BaseElement.Owner=\"{Owner.TEID}\" BaseElement.Operator=\"{Operator.TEID}\" BaseElement.KVLevel=\"{NearBus.Voltage}\" BaseElement.Substation=\"{Substation.TEID}\" BaseElement.SubName=\"{Name}\" BaseElement.SubLongName=\"{Name}\" Opened=\"{OpenChecked}\" ScadaOpened=\"{OpenChecked}\" BaseElement.Name=\"{Name}\"/>\r\n\t</{ElemType}>";
            XPos = XPosText + XWidth + 20;
            return OutLine;
        }
    }
}
