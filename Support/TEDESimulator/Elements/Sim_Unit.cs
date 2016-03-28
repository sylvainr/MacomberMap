using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEDESimulator.Elements
{
    public class Sim_Unit: Sim_Element
    {
        public Sim_Substation Substation;
        public int TEID {get;set;}
        public int UnitTEID;
        public String Name;
        public double MW_Est, MVAR_Est,MVA_Est,Capacity,LMP;
        public Sim_Company Owner;
        public Sim_Company Operator;
        public Sim_Bus Bus;
        public Sim_UnitType UnitType;
        public string ElemType { get { return "Unit"; } }

        public Guid ElemGuid { get; set; }

        public Sim_Unit(String Name,  Sim_Bus Bus, Sim_Substation Parent, Sim_UnitType UnitType, double MW_Est, double MVAR_Est, double PercentageCapacity, Sim_Builder Builder)
        {
            this.Substation = Parent;
            this.Substation.Elements.Add(this);
            this.ElemGuid = Guid.NewGuid();
            this.Name = Name;
            this.UnitType = UnitType;
            this.MW_Est = MW_Est;
            this.MVAR_Est = MVAR_Est;
            this.LMP = Builder.NextRandom(2, 50);
            this.MVA_Est= Math.Round(Math.Sqrt(MW_Est * MW_Est + MVAR_Est * MVAR_Est));
            this.Capacity = this.MW_Est / PercentageCapacity;
            this.TEID = Builder.NextTEID();
            this.UnitTEID = Builder.NextTEID();
            this.Owner = Builder.GetOwner();
            this.Operator = Builder.GetOperator();
            this.Bus = Bus;
        }

        /// <summary>
        /// Get the XML for our unit, and add in our unit information
        /// </summary>
        /// <param name="Builder"></param>
        /// <returns></returns>
        public String GetXml(Sim_Builder Builder)
        {
            //HSL =\"{Capacity}\"  Estimated_MW =\"{MW_Est}\" Estimated_MVAR=\"{MVAR_Est}\" Estimated_MVA=\"{MVA_Est}\" Telemetered_MW=\"{MW_Est}\" Telemetered_MVAR=\"{MVAR_Est}\" Telemetered_MVA=\"{MVA_Est}\" 
            Builder.Savecase.UnitData.Add(new MacomberMapCommunications.Messages.EMS.MM_Unit_Data() { Open_Un = MW_Est == 0, TEID_Un = TEID, W_Un = (float)MW_Est, R_Un = (float)MVAR_Est, WMeas_Un = (float)MW_Est, RMeas_Un = (float)MVAR_Est, Conn_Bus = Bus.BusNumber, Reg_Bus = Bus.BusNumber, NoAVR_Un = true, NoPSS_Un = true });
            Builder.Savecase.UnitGenData.Add(new MacomberMapCommunications.Messages.EMS.MM_Unit_Gen_Data() { CAPMX_UNIT = (float)Capacity, GEN_UNIT = (float)MW_Est, HSL_UNIT = (float)Capacity, HDL_UNIT = (float)Capacity * .98f, HASL_UNIT = (float)Capacity * .99f, HEL_UNIT = (float)Capacity * 1.02f, Key = Substation.Name + "." + Name, FUEL_UNIT = "Gas", LEL_UNIT = 2, LDL_UNIT = 3, LSL_UNIT = 4, LASL_UNIT = 3, LMP_UNIT = (float)LMP, SPPSETP_UNIT = (float)LMP, TEID_UNIT = TEID, SCEDBP_UNIT = (float)MW_Est });
            Builder.Savecase.UnitSimulationData.Add(new MacomberMapCommunications.Messages.EMS.MM_Unit_Simulation_Data() { AGC_Plc = true, Fhz_Un = 60, FrqCtrl_Un = false, FrqTarg_SIsl = 60, Isl_Num = 0, TEID_Un = TEID });
            return $"<Unit TEID=\"{TEID}\" UnitTEID=\"{UnitTEID}\" Name=\"{Name}\" Substation=\"{Substation.TEID}\" UnitType=\"Thermal\" Owner=\"{Owner.TEID}\" Operator=\"{Operator.TEID}\" ElemType=\"Unit\" KVLevel=\"Other KV\" PrimaryFuelType=\"Gas\" MaxCapacity=\"{Capacity}\" MVARCapabilityCurve=\"20,-28.4,52.8,50,-19.9,41.9,60,-12.5,35,65,-8.5,30.9,72,0,0\" PANType=\"None\" />";
        }

        public override string ToString()
        {
            return $"Unit {Name}: {Bus.Voltage.NominalVoltage} KV, {MW_Est} MW";
        }

        public Sim_Bus[] GetBuses()
        {
            return new Sim_Bus[] { Bus };
        }

        /// <summary>
        /// Report the one-line XML for our unit
        /// </summary>
        /// <param name="XPos"></param>
        /// <param name="YPos"></param>
        /// <returns></returns>
        public string GetOneLineXml(ref int XPos, int YPos)
        {
            Guid UnitGuid = Guid.NewGuid();
            int XWidth = Name.Length * 8;
            int XPosText = XPos + 30;
            String OutLine= $"\t<{ElemType} BaseElement.Permitted=\"true\" GenerationLevel=\"0\" ToplogyValidated=\"true\" Collision=\"false\" KVLevel=\"{Bus.Voltage}\" DescriptorArrow=\"false\" Orientation=\"Up\" Bounds=\"{XPos},{YPos},26,26,\" Visible=\"true\" Text=\"{Name}\" rdfID=\"{UnitGuid}\" BaseElement.ElemType=\"Unit\" BaseElement.KVLevel=\"{Bus.Voltage}\" BaseElement.IsRC=\"false\" BaseElement.UnitType=\"{UnitType.Name}\" BaseElement.Name=\"{Name}\" BaseElement.TEID=\"{TEID}\" BaseElement.Owner=\"{Owner.TEID}\" BaseElement.Operator=\"{Operator.TEID}\" BaseElement.Substation=\"{Substation.TEID}\" BaseElement.SubLongName=\"{Substation.Name}\" PUNElement=\"False\" BaseElement.SubName=\"{Substation.Name}\">\r\n\t\t<Descriptor ToplogyValidated=\"false\" DescriptorArrow=\"false\" Orientation=\"Unknown\" Bounds=\"{XPosText},{YPos},{XWidth},15,\" Font=\"Arial, 9pt\" ForeColor=\"Gray\" LabelBounds=\"-22,-1,90,46,\" Collision=\"false\" Visible=\"true\" Text=\"{Name}\" rdfID = \"{UnitGuid}\" BaseElement.TEID=\"{TEID}\" BaseElement.ElemType=\"Unit\" BaseElement.Owner=\"{Owner.TEID}\" BaseElement.Operator=\"{Operator.TEID}\" BaseElement.KVLevel=\"{Bus.Voltage}\" BaseElement.Substation=\"{Substation.TEID}\" BaseElement.SubName=\"{Name}\" BaseElement.SubLongName=\"{Name}\" BaseElement.IsRC=\"false\" BaseElement.UnitType=\"{UnitType.Name}\" BaseElement.Name=\"{Name}\" />\r\n\t</Unit>";
            XPos = XPosText + XWidth + 20;
            return OutLine;
        }


    }
}