using MacomberMapCommunications.Messages;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TEDESimulator.GIS;

namespace TEDESimulator.Elements
{
    public class Sim_Builder
    {
        public Sim_Builder_Configuration Config;
        public Sim_Random Randomizer = new Sim_Random((int)DateTime.Now.ToBinary());
        public double CoordinateCount = 0, Min_X = double.NaN, Min_Y = double.NaN, Max_X = double.NaN, Max_Y = double.NaN, Centroid_X = 0, Centroid_Y = 0;
        public Dictionary<int, bool> TEIDs = new Dictionary<int, bool>();
        public List<Sim_VoltageLevel> Voltages = new List<Sim_VoltageLevel>();
        public List<Sim_Company> Companies = new List<Sim_Company>();
        public Sim_VoltageLevel GenVoltage = new Sim_VoltageLevel(13.8);
        public List<Sim_Boundary> Boundaries = new List<Sim_Boundary>();
        public List<Sim_UnitType> UnitTypes = new List<Sim_UnitType>();
        public MM_Savecase Savecase = new MM_Savecase();

        public List<Sim_Breaker> Breakers = new List<Sim_Breaker>();
        public List<Sim_Substation> Subs = new List<Sim_Substation>();
        public List<Sim_Unit> Units = new List<Sim_Unit>();
        public List<Sim_Load> Loads = new List<Sim_Load>();
        public List<Sim_Transformer> Transformers = new List<Sim_Transformer>();
        public List<Sim_Bus> Buses = new List<Sim_Bus>();
        public List<Sim_Line> Lines = new List<Sim_Line>();
        public List<Sim_ShuntCompensator> ShuntCompensators = new List<Sim_ShuntCompensator>();

        /// <summary>
        /// Build our simulated model
        /// </summary>
        /// <param name="Config"></param>
        public Sim_Builder(Sim_Builder_Configuration Config, String SubstationText)
        {
            this.Config = Config;
            //Start by building our companies
            for (int a = 0; a < Config.CompanyCount; a++)
                Companies.Add(new Sim_Company("Company" + a.ToString(), "TCOM" + a.ToString(), "-Unknown-", "-Unknown-", this));

            //Add in our voltage levels
            Voltages.Add(new Sim_VoltageLevel(345));
            Voltages.Add(new Sim_VoltageLevel(230));
            Voltages.Add(new Sim_VoltageLevel(138));
            Voltages.Add(new Sim_VoltageLevel(69));

            //Add in our unit types
            UnitTypes.Add(new Sim_UnitType("NuclearGeneratingUnit"));
            UnitTypes.Add(new Sim_UnitType("ThermalGeneratingUnit"));
            UnitTypes.Add(new Sim_UnitType("WindGeneratingUnit"));



            //Add in our county and state boundaries
            if (Config.BaseModel != null)
                LoadModel(Config.BaseModel);
            else
            {
                LoadBoundary(Config.CountyBoundary, true);
                LoadBoundary(Config.StateBoundary, false);
            }

            //Now, add in our substations
            AddSubstations(Config.SubstationCount, SubstationText);

            //Now, for each sub, add our buses and breakers
            foreach (Sim_Substation Sub in Subs)
            {
                Sim_Bus NewBus = new Sim_Bus(Sub, Buses.Count, "Bus" + Buses.Count.ToString("000"), Randomizer.GenerateNumber(1, Config.BusVoltageStdDev / 100.0), this);
                Buses.Add(NewBus);

                bool IsOpen = NextRandom(0, 1) > 0.5;
                bool IsBreaker = NextRandom(0, 1) > 0.5;
                if (!IsOpen)
                    Breakers.Add(new Sim_Breaker((IsBreaker ? "Breaker" : "Switch") + Breakers.Count.ToString("000"), Sub, NewBus, NewBus, this, false, IsBreaker));
                else
                {
                    Sim_Bus FarBus = new Sim_Bus(Sub, Buses.Count, "Bus" + Buses.Count.ToString("000"), Randomizer.GenerateNumber(1, Config.BusVoltageStdDev / 100.0), this);
                    FarBus.Voltage = NewBus.Voltage;
                    Buses.Add(FarBus);
                    Breakers.Add(new Sim_Breaker((IsBreaker ? "Breaker" : "Switch") + Breakers.Count.ToString("000"), Sub, NewBus, FarBus, this, true, IsBreaker));
                }
            }




            //Now, add in our lines
            foreach (Sim_Substation FromSub in Subs)
                if (Randomizer.NextDouble() < Config.LineProbability)
                {
                    Sim_Substation ToSub = FromSub.FindNearbySubstation(Subs, Config.LineDistance, Lines, this);
                    if (ToSub != null && !FromSub.ConnectedSubstations(Lines).ContainsKey(ToSub))
                        Lines.Add(new Sim_Line(FromSub, ToSub, Lines, this, Randomizer.GenerateNumber(Config.LineMW), Randomizer.GenerateNumber(Config.LineMW) * 0.3, Math.Abs(Randomizer.GenerateNumber(Config.LineLoad) / 100.0)));
                }

            //Add in our shunt compensators, reactors and loads
            foreach (Sim_Substation Sub in Subs)
            {
                while (Randomizer.NextDouble() < Config.CapacitorProbability)
                {
                    double MVAR = Randomizer.GenerateNumber(Config.CapacitorMVAR);
                    bool IsOpen = Randomizer.NextDouble() >= Config.CapacitorOpenProbability;
                    ShuntCompensators.Add(new Sim_ShuntCompensator("Cap" + ShuntCompensators.Count.ToString("000"), Sub, Sub.FindBus(this), IsOpen, (IsOpen ? 0 : MVAR), MVAR, true, this));
                }

                while (Randomizer.NextDouble() < Config.ReactorProbability)
                {
                    double MVAR = Randomizer.GenerateNumber(Config.ReactorMVAR);
                    bool IsOpen = Randomizer.NextDouble() >= Config.ReactorOpenProbability;
                    ShuntCompensators.Add(new Sim_ShuntCompensator("Reac" + ShuntCompensators.Count.ToString("000"), Sub, Sub.FindBus(this), IsOpen, (IsOpen ? 0 : MVAR), MVAR, false, this));
                }

                while (Randomizer.NextDouble() < Config.UnitProbability)
                {
                    Sim_Bus FoundGenBus;
                    if (Randomizer.NextDouble() > 0.25)
                        FoundGenBus = Sub.FindBus(this, GenVoltage);
                    else
                        FoundGenBus = Sub.FindBus(this);
                    Units.Add(new Sim_Unit("Unit" + Units.Count.ToString("000"), FoundGenBus, Sub, UnitTypes[Randomizer.Next(0, UnitTypes.Count)], Randomizer.GenerateNumber(Config.UnitMW), Randomizer.GenerateNumber(Config.UnitMW) * 0.25, Randomizer.GenerateNumber(Config.UnitCapacity), this));
                }

                while (Randomizer.NextDouble() < Config.LoadProbability)
                    Loads.Add(new Sim_Load("Load" + Loads.Count.ToString("000"), Sub, Sub.FindBus(this), Randomizer.GenerateNumber(Config.LoadMW), Randomizer.GenerateNumber(Config.LoadMW) * 0.25, this));
            }

            //Now, add any transformers within substations with different voltages
            foreach (Sim_Substation Sub in Subs)
            {
                Dictionary<Sim_VoltageLevel, List<Sim_Bus>> Voltages = new Dictionary<Sim_VoltageLevel, List<Sim_Bus>>();
                foreach (Sim_Bus Bus in Sub.Elements.OfType<Sim_Bus>())
                {
                    if (!Voltages.ContainsKey(Bus.Voltage))
                        Voltages.Add(Bus.Voltage, new List<Sim_Bus>());
                    Voltages[Bus.Voltage].Add(Bus);
                }
                for (int a = 0; a < Voltages.Count - 1; a++)
                {
                    //Create a transformer between two of our voltages
                    Sim_VoltageLevel Volt1 = Voltages.Keys.ToArray()[0];
                    Sim_VoltageLevel Volt2 = Voltages.Keys.ToArray()[1];
                    Sim_Bus Bus1 = Voltages[Volt1][Randomizer.Next(0, Voltages[Volt1].Count)];
                    Sim_Bus Bus2 = Voltages[Volt2][Randomizer.Next(0, Voltages[Volt2].Count)];
                    Sim_Transformer NewXf = new Sim_Transformer("XF" + Transformers.Count.ToString("000"), Sub, Randomizer.GenerateNumber(Config.TransformerMW), Randomizer.GenerateNumber(Config.TransformerMW) * 0.25, Bus1, Bus2, this);
                    Transformers.Add(NewXf);
                }

            }
        }

        public void Export()
        {
            //Now, write out our file
            using (StreamWriter sW = new StreamWriter(Config.TargetModelFile, false, new UTF8Encoding(false)))
            {
                sW.WriteLine("<?xml version=\"1.0\"?>");
                sW.WriteLine("<NetworkModel SaveTime=\"" + XmlConvert.ToString(DateTime.Now, XmlDateTimeSerializationMode.Utc) + "\" Model=\"Simulation\">");
                foreach (Sim_Boundary Bound in Boundaries)
                    sW.WriteLine("\t" + Bound.GetXml());
                foreach (Sim_Company Company in Companies)
                    sW.WriteLine("\t" + Company.GetXml());
                foreach (Sim_Substation Sub in Subs)
                    sW.WriteLine("\t" + Sub.GetXml(this));
                foreach (Sim_Unit Unit in Units)
                    sW.WriteLine("\t" + Unit.GetXml(this));
                foreach (Sim_Bus Bus in Buses)
                    sW.WriteLine("\t" + Bus.GetXml(this));
                foreach (Sim_Line Line in Lines)
                    sW.WriteLine("\t" + Line.GetXml(this));
                foreach (Sim_Breaker Breaker in Breakers)
                    sW.WriteLine("\t" + Breaker.GetXml(this));
                foreach (Sim_Load Load in Loads)
                    sW.WriteLine("\t" + Load.GetXml(this));
                foreach (Sim_ShuntCompensator ShuntCompensator in ShuntCompensators)
                    sW.WriteLine("\t" + ShuntCompensator.GetXml(this));
                foreach (Sim_Transformer Transformer in Transformers)
                    sW.WriteLine("\t" + Transformer.GetXml(this));

                sW.WriteLine("</NetworkModel>");
            }

            //Now, write out our one-lines
            foreach (Sim_Substation Sub in Subs)
                GenerateOneLine(Sub, Config.OneLineFolder);

            //Write our our savecase
            using (FileStream fsOut = new FileStream(Path.Combine(Path.GetDirectoryName(Config.TargetModelFile), Path.GetFileNameWithoutExtension(Config.TargetModelFile) + ".MM_Savecase"), FileMode.Create))
            {
                DataContractSerializer dcs = new DataContractSerializer(typeof(MM_Savecase));
                XmlDictionaryWriter xdw = XmlDictionaryWriter.CreateBinaryWriter(fsOut);
                dcs.WriteObject(xdw, Savecase);
                xdw.Flush();
            }
        } 


        private void AddSubstations(int Count, String SubstationText)
        {
            if (SubstationText == "per county (random position)")
            {
                foreach (Sim_Boundary Bound in Boundaries)
                    if (Bound.Name != "STATE")
                        for (int a=0;a< Count;a++)
                        {
                            double Longitude = NextRandom(Bound.Min_X, Bound.Max_X);
                            double Latitude = NextRandom(Bound.Min_Y, Bound.Max_Y);
                            while (!Bound.HitTest((float)Longitude, (float)Latitude))
                            {
                                Longitude = NextRandom(Bound.Min_X, Bound.Max_X);
                                Latitude = NextRandom(Bound.Min_Y, Bound.Max_Y);
                            }
                            Subs.Add(new Sim_Substation("Sub" + Subs.Count.ToString("000"), Longitude, Latitude, Bound, this));
                        }
            }
                else if (SubstationText == "per county (in center)")
            {

                foreach (Sim_Boundary Bound in Boundaries)
                    if (Bound.Name != "STATE")
                        for (int a = 0; a < Count; a++)
                        {
                            double Longitude = Bound.Min_X + (Bound.Max_X - Bound.Min_X) / 2.0;
                            double Latitude = Bound.Min_Y + (Bound.Max_Y - Bound.Min_Y) / 2.0;
                            Subs.Add(new Sim_Substation("Sub" + Subs.Count.ToString("000"), Longitude, Latitude, Bound, this));
                        }
            }
            else if (SubstationText == "per state")
            {
                foreach (Sim_Boundary Bound in Boundaries)
                    if (Bound.Name == "STATE")
                        for (int a = 0; a < Count; a++)
                        {
                            double Longitude = NextRandom(Bound.Min_X, Bound.Max_X);
                            double Latitude = NextRandom(Bound.Min_Y, Bound.Max_Y);
                            while (!Bound.HitTest((float)Longitude, (float)Latitude))
                            {
                                Longitude = NextRandom(Bound.Min_X, Bound.Max_X);
                                Latitude = NextRandom(Bound.Min_Y, Bound.Max_Y);
                            }
                            Subs.Add(new Sim_Substation("Sub" + Subs.Count.ToString("000"), Longitude, Latitude, Bound, this));
                        }
            }
        }

        public double NextRandom(double Min, double Max)
        {
            double Diff = Max - Min;
            return ((Randomizer.NextDouble() * Diff) + Min);
        }

        private void LoadModel(String FileName)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(FileName);
            foreach (XmlElement xElem in xDoc.DocumentElement.ChildNodes.OfType<XmlElement>())
                if (xElem.Name == "County" || xElem.Name == "State")
                    Boundaries.Add(new Sim_Boundary(xElem, this));
        }


        private void LoadBoundary(String FileName, bool Counties)
        {
            CoordinateTransformationFactory ctfac = new CoordinateTransformationFactory();
            ICoordinateSystem SourceProj = ProjNet.Converters.WellKnownText.CoordinateSystemWktReader.Parse(File.ReadAllText(FileName.ToLower().Replace(".shp", ".prj"))) as ICoordinateSystem;
            ICoordinateSystem DestProj = GeographicCoordinateSystem.WGS84;
            ICoordinateTransformation XF = ctfac.CreateFromCoordinateSystems(SourceProj, DestProj);
            using (ShapeFile sF = ShapeFile.Open(FileName))
                foreach (Shape shp in sF.GetAllShapes())
                {
                    Sim_Boundary NewBoundary = new Sim_Boundary(shp, XF, this);
                    if (Counties)
                        Boundaries.Add(NewBoundary);
                    else if (NewBoundary.Contains(Boundaries[0]))
                    {
                        NewBoundary.Name = "STATE";
                        Boundaries.Add(NewBoundary);
                    }
                }

            if (Counties)
            {
                Centroid_X /= CoordinateCount;
                Centroid_Y /= CoordinateCount;
            }
        }

        #region One-line generation
        /// <summary>
        /// Generate our one-line
        /// </summary>
        /// <param name="Substation"></param>
        /// <param name="TargetDirectory"></param>
        /// <returns></returns>
        public void GenerateOneLine(Sim_Substation Substation, String TargetDirectory)
        {
            //First, build our list of buses and connected equipment
            Dictionary<Sim_Bus, List<Sim_Element>> Elements = new Dictionary<Sim_Bus, List<Sim_Element>>();
            Dictionary<Sim_Bus, int> ElementRight = new Dictionary<Sim_Bus, int>();

            foreach (Sim_Bus Bus in Substation.Elements.OfType<Sim_Bus>())
                Elements.Add(Bus, new List<Sim_Element>());

            foreach (Sim_Element Elem in Substation.Elements)
                if (Elem is Sim_Transformer)
                {
                    Sim_TransformerWinding Winding1 = ((Sim_Transformer)Elem).Winding1;
                    Sim_TransformerWinding Winding2 = ((Sim_Transformer)Elem).Winding2;
                    if (Elements.ContainsKey(Winding1.Bus) && !Elements[Winding1.Bus].Contains(Winding1))
                        Elements[Winding1.Bus].Add(Winding1);
                    if (Elements.ContainsKey(Winding2.Bus) && !Elements[Winding2.Bus].Contains(Winding2))
                        Elements[Winding2.Bus].Add(Winding2);
                }
                else if (Elem is Sim_Bus == false)
                {
                    Sim_Bus Bus = Elem.GetBuses()[0];
                    if (Elements.ContainsKey(Bus) && !Elements[Bus].Contains(Elem))
                        Elements[Bus].Add(Elem);
                }

           

            using (StreamWriter sW = new StreamWriter(Path.Combine(Config.OneLineFolder, Substation.Name + ".MM_OneLine"), false, new UTF8Encoding(false)))
            {
                sW.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>");
                sW.WriteLine($"<One_Line BaseElement.ElemType=\"Substation\" BaseElement.TEID=\"{Substation.TEID}\" BaseElement.ConnectedLines=\"\" BaseElement.County=\"{Substation.Parent.Name}\" BaseElement.DisplayName=\"{Substation.Name}\" BaseElement.{Substation.ElemTypes} BaseElement.{Substation.KVLevels} BaseElement.Latitude=\"{Substation.Latitude}\" BaseElement.LatLong=\"{Substation.Latitude},{Substation.Longitude}\" BaseElement.Longitude=\"{Substation.Longitude}\" BaseElement.Operator=\"{Substation.Operator.TEID}\" BaseElement.Owner=\"{Substation.Owner.TEID}\" BaseElement.Permitted=\"true\" Font=\"Microsoft Sans Serif, 8.25pt\" BaseElement.Name=\"{Substation.Name}\" BaseElement.LongName=\"{Substation.Name}\">");

                //Now, determine the Y coordinate of the bus
                int YVal = 50;
                foreach (Sim_Bus Bus in Elements.Keys)
                {
                    Bus.Y = YVal;
                    YVal += 50;
                }

                sW.WriteLine("<Elements>");
                foreach (KeyValuePair<Sim_Bus, List<Sim_Element>> kvp in Elements)
                {
                    int XVal = 50;
                    foreach (Sim_Element Elem in kvp.Value)
                        if (Elem is Sim_TransformerWinding)
                            sW.WriteLine(((Sim_TransformerWinding)Elem).Transformer.GetOneLineXml(ref XVal, kvp.Key.Y));
                        else
                            sW.WriteLine(Elem.GetOneLineXml(ref XVal, kvp.Key.Y));
                    ElementRight.Add(kvp.Key, XVal);
                }
                sW.WriteLine("</Elements>");


                sW.WriteLine("<Nodes>");
                foreach (Sim_Bus Bus in Elements.Keys)
                {
                    int XVal = 25;
                    Bus.XWidth = ElementRight[Bus];
                    sW.WriteLine(Bus.GetOneLineXml(ref XVal, Bus.Y));
                }
                sW.WriteLine("</Nodes>");
                sW.WriteLine("</One_Line>");
            }


        }

        #endregion

        #region Value lookups
        /// <summary>
        /// Locate our next TEID
        /// </summary>
        /// <returns></returns>
        public int NextTEID()
        {
            int FoundTEID = Randomizer.Next(999999);
            while (TEIDs.ContainsKey(FoundTEID))
                FoundTEID = Randomizer.Next(999999);
            TEIDs.Add(FoundTEID, true);
            return FoundTEID;
        }

        public Sim_Company GetOperator()
        {
            return Companies[Randomizer.Next(Companies.Count)];
        }

        public Sim_Company GetOwner()
        {
            return Companies[Randomizer.Next(Companies.Count)];
        }
        

        public Sim_VoltageLevel GetVoltage(bool GenType = false)
        {
            if (GenType)
                return Randomizer.NextDouble() < 0.1 ? Voltages[Randomizer.Next(Voltages.Count)] : GenVoltage;
            else
                return Voltages[Randomizer.Next(Voltages.Count)];
        }

        #endregion



    }


}
