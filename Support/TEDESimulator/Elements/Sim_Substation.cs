using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEDESimulator.Elements
{
   public class Sim_Substation: Sim_Element
    {
        public Sim_Boundary Parent;
        public int TEID {get;set;}
        public double Latitude, Longitude;
        public String Name;
        public List<Sim_Element> Elements = new List<Sim_Element>();
        public Sim_Company Owner;
        public Sim_Company Operator;

        public Sim_Substation(String Name, double Longitude, double Latitude, Sim_Boundary Parent, Sim_Builder Builder)
        {
            this.Name = Name;
            this.Longitude = Longitude;
            this.Latitude = Latitude;
            this.Parent = Parent;
            this.Owner = Builder.GetOwner();
            this.ElemGuid = Guid.NewGuid();
            this.Operator = Builder.GetOperator();
            this.TEID = Builder.NextTEID();
        }

        /// <summary>
        /// Initialize a new substation
        /// </summary>
        /// <param name="InLine"></param>
        /// <param name="Parents"></param>
        public Sim_Substation(String[] InLine, List<Sim_Boundary> Parents)
        {
            this.Name = InLine[0];
            this.Longitude = ParseCoord(InLine[2]);
            this.Latitude = ParseCoord(InLine[1]);
            foreach (Sim_Boundary ShapeToCheck in Parents)
                if (ShapeToCheck.Contains(Longitude, Latitude))
                {
                    Parent = ShapeToCheck;
                    break;
                }

            //If we have no parent, assign to state
            if (Parent == null)
                Parent = Parents.Last();
        }

        public double ParseCoord(String InVal)
        {
            if (InVal.Contains("°"))
            {
                String[] SplStr = InVal.Split('°', '\'', '"');
                return (double.Parse(SplStr[0]) + (double.Parse(SplStr[1]) / 60.0) + (double.Parse(SplStr[2]) / 3600.0)) * (SplStr[3] == "W" || SplStr[3] == "S" ? -1 : 1);
            }
            else
                return double.Parse(InVal);
        }

        public String UnitString
        {
            get
            {
                StringBuilder sB = new StringBuilder();
                foreach (Sim_Unit Unit in Elements.OfType<Sim_Unit>())
                    sB.Append((sB.Length == 0 ? "" : ",") + Unit.TEID.ToString());
                if (sB.Length == 0)
                    return "";
                else
                    return "Units=\"" + sB.ToString() + "\" ";
            }
        }

        public String LoadString
        {
            get
            {
                StringBuilder sB = new StringBuilder();
                foreach (Sim_Load Load in Elements.OfType<Sim_Load>())
                    sB.Append((sB.Length == 0 ? "" : ",") + Load.TEID.ToString());
                if (sB.Length == 0)
                    return "";
                else
                    return "Loads=\"" + sB.ToString() + "\" ";
            }
        }

        public String ShuntCompensatorString
        {
            get
            {
                StringBuilder sB = new StringBuilder();
                foreach (Sim_ShuntCompensator sc in Elements.OfType<Sim_ShuntCompensator>())
                    sB.Append((sB.Length == 0 ? "" : ",") + sc.TEID.ToString());
                if (sB.Length == 0)
                    return "";
                else
                    return "ShuntCompensators=\"" + sB.ToString() + "\" ";
            }
        }

        public String TransformerString
        {
            get
            {
                StringBuilder sB = new StringBuilder();
                foreach (Sim_Transformer XF in Elements.OfType<Sim_Transformer>())
                    sB.Append((sB.Length == 0 ? "" : ",") + XF.TEID.ToString());
                if (sB.Length == 0)
                    return "";
                else
                    return "Transformers=\"" + sB.ToString() + "\" ";
            }
        }

        public String BusString
        {
            get
            {
                StringBuilder sB = new StringBuilder();
                foreach (Sim_Bus Bus in Elements.OfType<Sim_Bus>())
                    sB.Append((sB.Length == 0 ? "" : ",") + Bus.TEID.ToString());
                if (sB.Length == 0)
                    return "";
                else
                    return "BusbarSections=\"" + sB.ToString() + "\" ";
            }
        }

        public string ElemTypes
        {
            get
            {
                StringBuilder sB = new StringBuilder();
                if (Elements.OfType<Sim_Bus>().Count() > 0)
                    sB.Append((sB.Length == 0 ? "" : ",") + "BusbarSection");
                if (Elements.OfType<Sim_Unit>().Count() > 0)
                    sB.Append((sB.Length == 0 ? "" : ",") + "Unit");
                if (Elements.OfType<Sim_Load>().Count() > 0)
                    sB.Append((sB.Length == 0 ? "" : ",") + "Load");
                if (sB.Length == 0)
                    return "";
                else
                    return "ElemTypes=\"" + sB.ToString() + "\" ";
            }
        }


        public string KVLevels
        {
            get
            {
                List<Sim_VoltageLevel> Voltages = new List<Sim_VoltageLevel>();
                foreach (Sim_Element Elem in Elements.OfType<Sim_Bus>())
                    foreach (Sim_Bus Bus in Elem.GetBuses())
                    if (!Voltages.Contains(Bus.Voltage))
                        Voltages.Add(Bus.Voltage);
                
                Voltages.Sort();
                StringBuilder sB = new StringBuilder();
                foreach (Sim_VoltageLevel v in Voltages)
                    sB.Append((sB.Length == 0 ? "" : ",") + v.ToString());
                return "KVLevels=\"" + sB.ToString() + "\"";
            }
        }

        public string ElemType {  get { return "Substation"; } }

        public Guid ElemGuid { get; set; }
        

        /// <summary>
        /// Generate the XML for our substation
        /// </summary>
        /// <param name="Builder"></param>
        /// <returns></returns>
        public String GetXml(Sim_Builder Builder)
        {
            return $"<{ElemType} Name=\"{Name}\" TEID=\"{TEID}\" County=\"{Parent.Name}\" HasSynchroscope=\"False\" HasSynchrocheck=\"False\" LongName=\"{Name}\" Longitude=\"{Longitude}\" Latitude=\"{Latitude}\" LatLong=\"{Latitude},{Longitude}\" ElemType=\"Substation\" Owner=\"{Owner.TEID}\" Operator=\"{Operator.TEID}\" {KVLevels} {UnitString} {BusString} {ElemTypes} {LoadString} {ShuntCompensatorString} {TransformerString}/>";
        }

        /// <summary>
        /// Locate a bus 
        /// </summary>
        /// <param name="Builder"></param>
        /// <param name="RequiredVoltages"></param>
        /// <returns></returns>
        public Sim_Bus FindBus(Sim_Builder Builder, params Sim_VoltageLevel[] RequiredVoltages)
        {
            List<Sim_Bus> BusCandidates = new List<Sim_Bus>();
            foreach (Sim_Bus Bus in this.Elements.OfType<Sim_Bus>())
                if (RequiredVoltages.Length == 0 || Array.IndexOf(RequiredVoltages, Bus.Voltage) != -1)
                    BusCandidates.Add(Bus);

            //If we don't have a bus of the proper voltage, create it.
            if (RequiredVoltages.Length>0 && BusCandidates.Count==0)
            {
                Sim_Bus NewBus = new Sim_Bus(this, Builder.Buses.Count, "Bus" + Builder.Buses.Count.ToString(), Builder.Randomizer.GenerateNumber(1, Builder.Config.BusVoltageStdDev / 100.0), Builder);
                Builder.Buses.Add(NewBus);
                BusCandidates.Add(NewBus);
            }
            return BusCandidates[Builder.Randomizer.Next(0, BusCandidates.Count)];
        }

        public Dictionary<Sim_Substation, Sim_Line> ConnectedSubstations(List<Sim_Line> Lines)
        {
            Dictionary<Sim_Substation,Sim_Line> OutDic = new Dictionary<Sim_Substation, Sim_Line>();
            foreach (Sim_Line Line in Lines)
                if (Line.From == this)
                    OutDic.Add(Line.To, Line);
                else if (Line.To == this)
                    OutDic.Add(Line.From, Line);
            return OutDic;
        }

        /// <summary>
        /// Find a nearby substation
        /// </summary>
        /// <returns></returns>
        public Sim_Substation FindNearbySubstation(List<Sim_Substation> Substations, double Distance, List<Sim_Line> Lines, Sim_Builder Builder)
        {
            Dictionary<Sim_Substation,Sim_Line> AlreadySubs = ConnectedSubstations(Lines);

            List<SubstationDistance> Distances = new List<SubstationDistance>();
            foreach (Sim_Substation Sub in Substations)
                if (Sub != this && !AlreadySubs.ContainsKey(Sub))
                {
                    double Dist = Sub.DistanceTo(this);
                    if (Dist <= Distance)
                        Distances.Add(new SubstationDistance(Sub, Dist));
                }
            Distances.Sort();
            if (Distances.Count == 0)
                return null;
            else
                return Distances[Builder.Randomizer.Next(Distances.Count)].Sub;
        }

        /// <summary>
        /// Compute the distances between two lat/longs
        /// </summary>
        /// <param name="LngLat"></param>
        /// <param name="OtherLngLat"></param>
        /// <returns></returns>
        public double DistanceTo(Sim_Substation OtherSub)
        {
            double R = 3963.1; //Radius of the earth in miles
            double deltaLat = (Latitude - OtherSub.Latitude) * Math.PI / 180.0;
            double deltaLong = (Longitude - OtherSub.Longitude) * Math.PI / 180.0;
            double a = Math.Sin(deltaLat / 2.0) * Math.Sin(deltaLat / 2.0) + Math.Cos(OtherSub.Longitude * Math.PI / 180.0) * Math.Cos(Longitude * Math.PI / 180.0) * Math.Sin(deltaLong / 2.0) * Math.Sin(deltaLong / 2.0);
            double c = 2f * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1f - a));
            return R * c;
        }

        public override string ToString()
        {
            return Name;
        }

        public Sim_Bus[] GetBuses()
        {
            throw new NotImplementedException();
        }

        public string GetOneLineXml(ref int XPos, int YPos)
        {
            throw new NotImplementedException();
        }

        private class SubstationDistance : IComparable<SubstationDistance>
        {
            public Sim_Substation Sub;
            public double Dist;
            public SubstationDistance(Sim_Substation Sub, double Dist)
            {
                this.Sub = Sub;
                this.Dist = Dist;
            }

            public int CompareTo(SubstationDistance other)
            {
                return Dist.CompareTo(other.Dist);
            }

            public override string ToString()
            {
                return Sub.ToString() + " (" + Dist.ToString() + ")";
            }


            
        }
    }
}