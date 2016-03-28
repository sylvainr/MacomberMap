using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEDESimulator.Elements
{
    /// <summary>
    /// © 2016, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides a simulated line's information, which we can use for our one-line layout
    /// </summary>
    public class Sim_Line: Sim_Element
    {
        public Sim_Substation From;
        public Sim_Substation To;
        public int TEID {get;set;}
        public String Name;
        public double NormalLimit, EmergencyLimit, LoadshedLimit;
        public double MW_Est, MVAR_Est;
        public double MVA_Est;
        public Sim_Company Owner;
        public Sim_Company Operator;
        public Sim_Bus NearBus, FarBus;
        public Guid ElemGuid { get; set; }

        public string ElemType
        {
            get
            {
                return "Line";
            }
        }

        public Sim_Line(Sim_Substation From, Sim_Substation To, List<Sim_Line> Lines, Sim_Builder Builder, double MW_Est, double MVAR_Est, double LineLoading)
        {
            this.ElemGuid = Guid.NewGuid();
            this.From = From;
            this.To = To;
            this.Name = "Line" + Lines.Count.ToString("000");
            this.MW_Est = MW_Est;
            this.MVAR_Est = MVAR_Est;
            this.MVA_Est = Math.Sqrt(MW_Est * MW_Est + MVAR_Est * MVAR_Est);
            this.NormalLimit = this.MVA_Est/LineLoading;
            this.EmergencyLimit = this.NormalLimit + Builder.Randomizer.Next(10);
            this.LoadshedLimit = this.EmergencyLimit + Builder.Randomizer.Next(10);
            this.Owner = Builder.GetOwner();
            this.Operator = Builder.GetOperator();
            this.TEID = Builder.NextTEID();
            this.From.Elements.Add(this);
            this.To.Elements.Add(this);
            this.NearBus = From.FindBus(Builder);
            this.FarBus = To.FindBus(Builder,this.NearBus.Voltage);
        }

        /// <summary>
        /// Write out our transmission line XML, and our savecase components
        /// </summary>
        /// <param name="Builder"></param>
        /// <returns></returns>
        public String GetXml(Sim_Builder Builder)
        {
            Builder.Savecase.LineData.Add(new MacomberMapCommunications.Messages.EMS.MM_Line_Data() { End1_Bus = NearBus.BusNumber, End2_Bus = FarBus.BusNumber, LIMIT1 = (float)NormalLimit, LIMIT2 = (float)EmergencyLimit, LIMIT3 = (float)LoadshedLimit, MVA_End1 = (float)MVA_Est, MVA_End2 = -(float)MVA_Est, RMeas_End1 = (float)MVAR_Est, RMeas_End2 = (float)-MVAR_Est, R_End1 = (float)MVAR_Est, R_End2 = (float)-MVAR_Est, TEID_End1 = NearBus.NodeTEID, TEID_Ln = TEID, WMeas_End1 = (float)MW_Est, WMeas_End2 = (float)MW_Est, W_End1 = (float)MW_Est, W_End2 = (float)MW_Est });
            return $"<{ElemType} IsZBR=\"false\" PUNElement=\"false\" Name=\"{Name}\" TEID=\"{TEID}\" Substation1=\"{From.TEID}\" Substation2=\"{To.TEID}\" Sub1Name=\"{From.Name}\" Sub2Name=\"{To.Name}\" NormalLimit=\"{NormalLimit}\" EmergencyLimit=\"{EmergencyLimit}\" LoadshedLimit=\"{LoadshedLimit}\" ConnectedStations=\"{From.TEID},{To.TEID}\" Coordinates=\"{From.Longitude},{From.Latitude},{To.Longitude},{To.Latitude}\" IsSeriesCompensator=\"False\" IsMultipleSegment=\"False\" Owner=\"{Owner.TEID}\" Operator=\"{Operator.TEID}\" ElemType=\"Line\" KVLevel=\"{NearBus.Voltage}\" />";
        }

        /// <summary>
        /// Return an easy-to-read string name for our builder
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name + " (" + From.ToString() + " to " + To.ToString() + ")";
        }

        /// <summary>
        /// Retrieve the line's list of buses
        /// </summary>
        /// <returns></returns>
        public Sim_Bus[] GetBuses()
        {
            return new Sim_Bus[] { NearBus, FarBus };
        }

        /// <summary>
        /// Retrieve the one-line XML for our line
        /// </summary>
        /// <param name="XPos"></param>
        /// <param name="YPos"></param>
        /// <returns></returns>
        public string GetOneLineXml(ref int XPos, int YPos)
        {
            int XPosText = XPos + 30;
            int XTextWidth = 8 * Name.Length;
            String OutString = $"\t<{ElemType} BaseElement.Permitted=\"true\" ToplogyValidated=\"true\" Collision=\"false\" KVLevel=\"{NearBus.Voltage}\" DescriptorArrow=\"false\" Orientation=\"Up\" Bounds=\"{XPos},{YPos},32,25\" Visible=\"true\" Text=\"{Name}\" rdfID=\"{ElemGuid}\" BaseElement.ElemType=\"{ElemType}\" BaseElement.KVLevel=\"{NearBus.Voltage}\" BaseElement.Name=\"{Name}\" BaseElement.TEID=\"{TEID}\" BaseElement.Owner=\"{Owner.TEID}\" BaseElement.Operator=\"{Operator.TEID}\">\r\n\t\t<Descriptor ToplogyValidated=\"false\" DescriptorArrow=\"false\" Orientation=\"Unknown\" Bounds=\"{XPosText},{YPos},{XTextWidth},14\" Font=\"Microsoft Sans Serif, 8.25pt\" ForeColor=\"Gray\" LabelBounds=\"-9,0,46,20,\" Collision=\"false\" Visible=\"true\" Text=\"{Name}\" rdfID=\"{ElemGuid}\" BaseElement.TEID=\"{TEID}\" BaseElement.ElemType=\"{ElemType}\" BaseElement.Owner=\"{Owner.TEID}\" BaseElement.Operator=\"{Operator.TEID}\" BaseElement.KVLevel=\"{NearBus.Voltage}\" BaseElement.Name=\"{Name}\" />\r\n\t</{ElemType}>";
            XPos = XPosText + XTextWidth + 10;
            return OutString;
        }
    }
}
