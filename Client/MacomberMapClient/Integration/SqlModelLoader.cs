using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Data.SqlClient;
using System.Threading;
using Dapper;
using MacomberMapClient.Data_Elements.Display;
using MacomberMapClient.Data_Elements.Geographic;
using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Data_Elements.Violations;
using MacomberMapClient.User_Interfaces.Generic;
using MacomberMapCommunications.Extensions;

namespace MacomberMapClient.Integration
{
    public static class SqlExtensions
    {
        public static string SafeGetString(this SqlDataReader reader, int colIndex)
        {
            if (!reader.IsDBNull(colIndex))
                return reader.GetString(colIndex);
            else
                return string.Empty;
        }
    }
    /// <summary>
    /// Class to pull model data from SQL database to MM_Repository for XML export.
    /// </summary>
    public class SqlModelLoader
    {
        public string ConnectionString { get; set; }

        public Dictionary<string, MM_Transformer> Transformers = new Dictionary<string, MM_Transformer>();

        public void LoadStaticRepository()
        {
            SqlConnection connection = new SqlConnection();
            SqlCommand cmd = new SqlCommand();
            connection.ConnectionString = ConnectionString;
            MM_System_Interfaces.Console = true;
            try
            {
                connection.Open();
                cmd.Connection = connection;
                cmd.CommandTimeout = 360;

                try
                {   // create these indexes if they are missing.
                    cmd.CommandText = "CREATE NONCLUSTERED INDEX indx_zbr2_st ON [dbo].[EMS_ZBR2]([$SUB],[ST]) INCLUDE([$KEY], [CO])";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "CREATE NONCLUSTERED INDEX indx_st_id_vl ON [dbo].[EMS_ND] ([ST]) INCLUDE([$KEY], [$SUB], [ID], [VL])";
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {

                }

                MM_System_Interfaces.LogError("Loading borders...", null);
                LoadCoordinates(cmd);
                LoadCompanies(cmd);
                LoadKVLevels(cmd);
                MM_System_Interfaces.LogError("Loading substations...", null);
                LoadSubstations(cmd);
                LoadNodes(cmd);
                LoadLoads(cmd);
                MM_System_Interfaces.LogError("Loading compensators...", null);
                LoadShuntCompensators(cmd);
                MM_System_Interfaces.LogError("Loading units...", null);
                LoadUnits(cmd);
                //MM_System_Interfaces.LogError("Loading Lines...", null);
                MM_Repository.Lines.Clear();
                LoadLines(cmd);
                MM_System_Interfaces.LogError("Loading Zbrs...", null);
                LoadLines(cmd, true);
                MM_System_Interfaces.LogError("Loading Ties...", null);
                LoadLines(cmd, false, true);
                LoadLines(cmd, true, true);
                SqlTransaction trans = null;
                try
                {
                    trans = connection.BeginTransaction();
                    cmd.CommandText = "select top 10 * from EMS_EQUIPSCADA with (TABLOCK, HOLDLOCK)";
                    cmd.Transaction = trans;
                    cmd.ExecuteNonQuery();
                   
                }
                catch (Exception)
                {
                    Thread.Sleep(20000);
                }
                MM_System_Interfaces.LogError("Loading XFs...", null);
                LoadTransformers(cmd);
                try
                {
                    MM_Repository.Contingencies.Clear();
                    MM_System_Interfaces.LogError("Loading CTGs...", null);
                    LoadFlowgates(cmd, true);
                    LoadFGElements(cmd, true);
                    MM_System_Interfaces.LogError("Loading FGs...", null);
                    LoadFlowgates(cmd);
                    LoadFGElements(cmd);
                }
                catch (Exception ex)
                {
                    MM_System_Interfaces.WriteConsoleLine(ex.Message);
                    MM_System_Interfaces.WriteConsoleLine("Trying FGs again. Maybe the FG job was interferring...");
                    trans.Rollback();
                    Thread.Sleep(10000);
                    MM_Repository.Contingencies.Clear();
                    MM_System_Interfaces.LogError("Loading CTGs...", null);
                    LoadFlowgates(cmd, true);
                    LoadFGElements(cmd, true);
                    MM_System_Interfaces.LogError("Loading FGs...", null);
                    LoadFlowgates(cmd);
                    LoadFGElements(cmd);

                }
                try
                {
                    trans.Commit();
                }
                catch (Exception)
                {
                }
                MM_System_Interfaces.LogError("Loading CBs...", null);
                LoadCircuitBreakers(cmd);

                MM_System_Interfaces.LogError("Done loading equipment into memory.", null);
                //Add our violations
                MM_Repository.ViolationTypes.Add("ThermalWarning", new MM_AlarmViolation_Type(MM_Repository.ViolationTypes.Count, "ThermalWarning", "tw", true, MM_Repository.OverallDisplay.WarningColor, 1.5f, true));
                MM_Repository.ViolationTypes.Add("ThermalAlert", new MM_AlarmViolation_Type(MM_Repository.ViolationTypes.Count, "ThermalAlert", "TA", true, MM_Repository.OverallDisplay.ErrorColor, 1.5f, true));
                MM_Repository.ViolationTypes.Add("VoltageWarning", new MM_AlarmViolation_Type(MM_Repository.ViolationTypes.Count, "VoltageWarning", "vw", true, MM_Repository.OverallDisplay.WarningColor, 1.5f, true));
                MM_Repository.ViolationTypes.Add("VoltageAlert", new MM_AlarmViolation_Type(MM_Repository.ViolationTypes.Count, "VoltageAlert", "VA", true, MM_Repository.OverallDisplay.ErrorColor, 1.5f, true));
                MM_KVLevel.MonitoringChanged += Data_Integration.MM_KVLevel_MonitoringChanged;

                Data_Integration.RestartModel(MM_Server_Interface.Client);
            }
            catch (Exception ex)
            {
                MM_System_Interfaces.WriteConsoleLine("Problem with SQL Server Connection " + this.ConnectionString);
                MM_System_Interfaces.WriteConsoleLine(ex.Message);
                MM_System_Interfaces.WriteConsoleLine(ex.StackTrace);
                throw ex;
            }

        }

        private void LoadCoordinates(SqlCommand cmd)
        {
            try
            {
                //cmd.CommandText = "Select [State], Lat, Long from mmStates order by [Order] union all Select 'STATE', Lat, Long from mmSPPBorder order by [Order]";
                //cmd.CommandText = "Select 'STATE', Latitude, Longitude from SPP_FootPrintVertices order by [row]";
                //cmd.CommandText = "Select State, Latitude, Longitude from States_Vertices";
                cmd.CommandText = "select * from (Select[State], Lat, Long, [Order] from mmStates "
                 + " union all Select 'STATE', Lat, Long, [Order] from mmSPPBorder ) t order by[State], [Order]";

                string lastState = "";
                MM_Boundary curBoundary = null;
                MM_Repository.Counties.Clear();
                //foreach (var rm in MM_Repository.Counties.Where(x => x.Key == "STATE"))
                   //MM_Repository.Counties.Remove(rm.Key);
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        string state = rd.GetString(0);

                        if (lastState != state)
                        {
                            
                            MM_Boundary dicVal = null;
                            MM_Repository.Counties.TryGetValue(state, out dicVal);
                            if (dicVal != null)
                                curBoundary = dicVal;
                            else
                            {
                                curBoundary = new MM_Boundary();
                                if (state == "STATE")
                                    curBoundary.ElemType = MM_Repository.FindElementType("State");
                                else
                                    curBoundary.ElemType = MM_Repository.FindElementType("County");
                                curBoundary.Name = state;
                                MM_Repository.TEIDs.Add(curBoundary.TEID = Data_Integration.GetTEID(), curBoundary);
                                MM_Repository.Counties.Add(state, curBoundary);
                            }
                        }

                        PointF point = new PointF((float)rd.GetDouble(2), (float)rd.GetDouble(1));

                        curBoundary.Coordinates.Add(point);

                        if (float.IsNaN(curBoundary.Min_X) || point.X < curBoundary.Min_X)
                        {
                            curBoundary.Min_X = point.X;
                        }
                        if (float.IsNaN(curBoundary.Min_Y)|| point.Y < curBoundary.Min_Y)
                        {
                            curBoundary.Min_Y = point.Y;
                        }

                        if (float.IsNaN(curBoundary.Max_X) || point.X > curBoundary.Max_X)
                        {
                            curBoundary.Max_X = point.X;
                        }
                        if (float.IsNaN(curBoundary.Max_Y) || point.Y > curBoundary.Max_Y)
                        {
                            curBoundary.Max_Y = point.Y;
                        }
                        lastState = state;
                    }
                }


                foreach (var bound in MM_Repository.Counties)
                {
                    //bound.Value.AddLines(bound.Value.Coordinates.ToArray());
                    bound.Value.Centroid_X = (bound.Value.Min_X + bound.Value.Max_X) / 2;
                    bound.Value.Centroid_Y = (bound.Value.Min_Y + bound.Value.Max_Y) / 2;
                }

                MM_System_Interfaces.WriteConsoleLine("Done loading coordinates.");

            }
            catch (Exception ex)
            {
                MM_System_Interfaces.WriteConsoleLine("Problem reading coordinates.");
                MM_System_Interfaces.WriteConsoleLine(ex.Message);
                MM_System_Interfaces.WriteConsoleLine(ex.StackTrace);
                throw ex;
            }

        }

        private void LoadKVLevels(SqlCommand cmd)
        {
            try
            {
                cmd.CommandText = "select distinct MaxKV"
                 + " from mmsubstations";
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    MM_Repository.KVLevels.Clear();

                    while (rd.Read())
                    {
                        string kv = rd.GetString(0);
                        var kvl = MM_Repository.FindKVLevel(kv.ToString() + " KV");

                        if (kvl.Name.Contains("Other"))
                        {
                            kvl = new MM_KVLevel() {Name = kv.ToString() + " KV"};
                            if (kvl.Energized.Name == null) kvl.Energized.Name = kvl.Name+".Energized";
                            if (kvl.DeEnergized.Name == null) kvl.DeEnergized.Name = kvl.Name+".DeEnergized";
                            if (kvl.PartiallyEnergized.Name == null) kvl.PartiallyEnergized.Name = kvl.Name+".PartiallyEnergized";
                            MM_Repository.KVLevels.Add(kv.ToString() + " KV", kvl);
                        }
                        kvl.ShowLineRouting = true;
                        if (float.Parse(kv) > 161)
                            kvl.Energized = new MM_DisplayParameter(ColorTranslator.FromHtml("Magenta"), 1, false);
                        else if (float.Parse(kv) > 69)
                            kvl.Energized = new MM_DisplayParameter(ColorTranslator.FromHtml("Blue"), 1, false);
                    }
                }
            }
            catch (Exception ex)
            {
                MM_System_Interfaces.WriteConsoleLine("Problem reading kv levels.");
                MM_System_Interfaces.WriteConsoleLine(ex.Message);
                MM_System_Interfaces.WriteConsoleLine(ex.StackTrace);
                throw ex;
            }
        }

        private void LoadCompanies(SqlCommand cmd)
        {
            try
            {
                cmd.CommandText = "select distinct Name, PrimaryPhone, Alias, DUNS, OperatesEquipment, TEID, HistoricServerPath"
                 + " from mmcompanies";
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    MM_Repository.Companies.Clear();

                    while (rd.Read())
                    {
                        var co = new MM_Company();
                        
                        co.ElemType = MM_Repository.FindElementType("Company");
                        int i = 0;
                        co.Name = rd.GetString(i++);
                        co.PrimaryPhone = rd.GetString(i++);
                        co.Alias = rd.GetString(i++);
                        co.DUNS = rd.GetString(i++);
                        co.OperatesEquipment = Boolean.Parse(rd.GetString(i++));
                        co.TEID = rd.GetInt32(i++);
                        co.HistoricServerPath = rd.GetString(i);

                        MM_Repository.TEIDs.Add(co.TEID, co);
                        MM_Repository.Companies.Add(co.Name, co);
                    }
                }
            }
            catch (Exception ex)
            {
                MM_System_Interfaces.WriteConsoleLine("Problem reading companies.");
                MM_System_Interfaces.WriteConsoleLine(ex.Message);
                MM_System_Interfaces.WriteConsoleLine(ex.StackTrace);
                throw ex;
            }
        }

        private MM_Element GetElement(int teid)
        {
            MM_Element element = null;
            MM_Repository.TEIDs.TryGetValue(teid, out element);

            return element;
        }

        private void LoadSubstations(SqlCommand cmd)
        {
            try
            {
                cmd.CommandText = "select TEID, Name, Area, LongName, IsInternal, IsMarket, County, Longitude, Latitude, Owner, Operator, HistoricServerPath, MaxKV"
                 + " from mmSubstations";
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    MM_Repository.Substations.Clear();

                    while (rd.Read())
                    {
                        int i = 0;
                        int teid = rd.GetInt32(i++);
                        var sub = GetElement(teid) as MM_Substation;
                        if (sub == null)
                        {
                            sub = new MM_Substation() {
                                TEID = teid
                            };
                            sub.ElemType = MM_Repository.FindElementType("Substation");
                            MM_Repository.TEIDs.Add(teid, sub);
                        }
                        sub.Name = rd.GetString(i++);
                        sub.Area = rd.GetString(i++);
                        sub.LongName = rd.GetString(i++);
                        sub.IsInternal = rd.GetInt32(i++) == 1;
                        sub.IsMarket = rd.GetInt32(i++) == 1;
                        MM_Boundary county = null;
                        string cString = rd.GetString(i++);

                        if (!MM_Repository.Counties.ContainsKey(sub.Area))
                        {
                            county = new MM_Boundary() { Name = sub.Area, ElemType = MM_Repository.FindElementType("County"), TEID = Data_Integration.GetTEID(), Coordinates = new List<PointF>() { new PointF(0, 0) } };
                            MM_Repository.Counties.Add(sub.Area, county);
                        } else
                        {
                            MM_Repository.Counties.TryGetValue(sub.Area, out county);
                        }
                        sub.County = county;
                        try
                        {
                            sub.Longitude = (float) rd.GetDouble(i++);
                            sub.Latitude = (float)rd.GetDouble(i++);
                        } catch (Exception)
                        {
                            sub.Longitude = (float)rd.GetFloat(--i);
                            sub.Latitude = (float)rd.GetFloat(++i);
                            i++;
                        }
                        sub.Owner = GetElement(rd.GetInt32(i++)) as MM_Company;
                        sub.Operator = GetElement(rd.GetInt32(i++)) as MM_Company;
                        sub.TEID = teid;
                        sub.HistoricServerPath = rd.GetString(i++);
                        sub.KVLevel = MM_Repository.FindKVLevel(rd.GetString(i) + " KV");
                        sub.KVLevels.Add(sub.KVLevel);
                        if (!MM_Repository.Substations.ContainsKey(sub.Name))
                            MM_Repository.Substations.Add(sub.Name, sub);
                    }
                }
            }
            catch (Exception ex)
            {
                MM_System_Interfaces.WriteConsoleLine("Problem reading substations.");
                MM_System_Interfaces.WriteConsoleLine(ex.Message);
                MM_System_Interfaces.WriteConsoleLine(ex.StackTrace);
                throw ex;
            }
        }

        private void LoadFGElements(SqlCommand cmd, bool onlyCtg = false)
        {
            cmd.CommandText = "select parentflowgate as fg, equipmenttype, id, iscontingency, istoendprimary"
             + " from vw_fgelements";

            if (onlyCtg)
                cmd.CommandText = "select ParentCTG as ctg, equipmenttype, id, istoendprimary"
             + " from vw_ctgelements";
            try
            {
                using (SqlDataReader rd = cmd.ExecuteReader())
                {

                    while (rd.Read())
                    {
                        int i = 0;
                        string fgName = rd.GetString(i++);
                        string eqType = rd.GetString(i++);
                        string eqID = rd.GetString(i++);
                        bool isContigency = onlyCtg ? true : rd.GetBoolean(i++);
                        bool isToEndPrimary = rd.GetBoolean(i);

                        MM_Contingency fgc = null;

                        MM_Repository.Contingencies.TryGetValue(fgName, out fgc);
                        MM_Flowgate fg = null;
                        if (fgc == null)
                            continue;
                        if (fgc is MM_Flowgate)
                            fg = (MM_Flowgate) fgc;

                        MM_Element element = null;

                        if (eqType == "LINE" || eqType == "ZBR")
                        {
                            MM_Line tLine = null;
                            MM_Repository.Lines.TryGetValue(eqID, out tLine);
                            if (tLine != null)
                                element = tLine;

                        } else if (eqType == "TRANSFORMER")
                        {
                            MM_Transformer tXF = null;
                            Transformers.TryGetValue(eqID, out tXF);
                            element = tXF;
                        } else if (eqType == "UNIT")
                        {
                            MM_Unit tUnit = null;
                            MM_Repository.Units.TryGetValue(eqID, out tUnit);
                            element = tUnit;
                        }

                        if (element != null)
                        {
                            if (element.Contingencies == null)
                                element.Contingencies = new List<MM_Contingency>();
                            if (!element.Contingencies.Any(cg => fgc.Name == cg.Name))
                                element.Contingencies.Add(fgc);
                            if (!fgc.ConElements.Contains(element.TEID))
                                fgc.ConElements.Add(element.TEID);
                            if (fg != null && isContigency)
                                fg.ContingentElements.Add(element.TEID);
                            else if (fg != null)
                            {
                                fg.MonitoredElements.Add(element.TEID);
                            }
                            // if (fg.ConElements.Count < 3)
                            // fg.Description += (isContigency ? " CON: " : " MON: ") + element.Name;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MM_System_Interfaces.WriteConsoleLine("Problem reading flowgates elements.");
                MM_System_Interfaces.WriteConsoleLine(ex.Message);
                MM_System_Interfaces.WriteConsoleLine(ex.StackTrace);
                throw ex;
            }

        }

        private void LoadFlowgates(SqlCommand cmd, bool onlyCtg = false)
        {
            try
            {
                cmd.CommandText = "select TEID, [$KEY] as Name, FromSubstation, ToSubstation, isnull(area, owner) as area, owner, iscontingency, element, HistoricServerPath, FromVoltage, ToVoltage"
                 + " from mmFGs where element is not null";

                if (onlyCtg)
                    cmd.CommandText = "select TEID, [$KEY] as Name, FromSubstation, ToSubstation, area as area, area as owner, element, FromVoltage, ToVoltage"
                     + " from mmCTGs where element is not null";

                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        int i = 0;
                        int teid = rd.GetInt32(i++);
                        var fg = GetElement(teid) as MM_Contingency;
                        if (fg == null)
                        {
                            fg = null;


                            if (onlyCtg)
                            {
                                fg = new MM_Contingency
                                     {
                                         TEID = teid,
                                         ElemType = MM_Repository.FindElementType("Contingency"),
                                         Type = "Contingency"
                                     };
                            } else
                            {
                                fg = new MM_Flowgate
                                     {
                                         TEID = teid,
                                         ElemType = MM_Repository.FindElementType("Flowgate"),
                                         Type = "Flowgate"
                                     };
                            }
                            MM_Repository.TEIDs.Add(teid, fg);
                        }
                        fg.Name = rd.GetString(i++);

                        if (fg.Substations == null)
                            fg.Substations = new MM_Substation[0];

                        MM_Substation to = null;
                        MM_Substation from =  null;

                        var st1 = rd.GetSqlString(i++);
                        var st2 = rd.GetSqlString(i++);

                        if (!st1.IsNull)
                            MM_Repository.Substations.TryGetValue(st1.Value, out from);
                        if (!st2.IsNull)
                            MM_Repository.Substations.TryGetValue(st2.Value, out to);

                        if (from != null && !fg.Substations.Any(s => s.Name == from.Name))
                            fg.Substations = fg.Substations.Add(from);
                        if (to != null && !fg.Substations.Any(s => s.Name == to.Name))
                            fg.Substations = fg.Substations.Add(to);
                        if (fg.Substations.Length > 0)
                            fg.Substation = fg.Substations[0];

                        fg.Active = true;

                        MM_Company co = null;
                        string area = rd.GetString(i++);
                        string owner = rd.GetString(i++);
                        
                        bool isContingency = onlyCtg ? true : rd.GetBoolean(i++);
                        MM_Repository.Companies.TryGetValue(area, out co);

                        fg.Owner = co;
                        fg.Operator = co;
                        if (string.IsNullOrWhiteSpace(fg.Description))
                            fg.Description = owner;

                        fg.Description += (isContingency ? " CON: " : " MON: ") + rd.GetString(i++);

                        if (fg.Description.Length > 70)
                            fg.Description = fg.Description.Substring(0, 70) + "...";

                        fg.TEID = teid;
                        if (!onlyCtg)
                            fg.HistoricServerPath = rd.GetString(i++);
                       
                        fg.KVLevel = MM_Repository.FindKVLevel(rd.GetString(i) + " KV");
                        
                        if (fg.KVLevels == null)
                            fg.KVLevels = new MM_KVLevel[0];
                        if (!fg.KVLevels.Any(k => k.Name == fg.KVLevel.Name))
                            fg.KVLevels = fg.KVLevels.Add(fg.KVLevel);
                        if (!MM_Repository.Contingencies.ContainsKey(fg.Name))
                            MM_Repository.Contingencies.Add(fg.Name, fg);
                    }
                }
            }
            catch (Exception ex)
            {
                MM_System_Interfaces.WriteConsoleLine("Problem reading flowgates.");
                MM_System_Interfaces.WriteConsoleLine(ex.Message);
                MM_System_Interfaces.WriteConsoleLine(ex.StackTrace);
                throw ex;
            }
        }

        private void LoadNodes(SqlCommand cmd)
        {
            try
            {
                cmd.CommandText = "select BusBarSection, TEID, Name, Substation, Owner, Operator, KVLevel"
                  + " from mmnodes";
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    MM_Repository.Busbars.Clear();

                    while (rd.Read())
                    {
                        int i = 0;
                        int teid = rd.GetInt32(i++);
                        int nodeTeid = rd.GetInt32(i++);
                        var bus = GetElement(teid) as MM_Bus;
                        if (bus == null)
                        {
                            bus = new MM_Bus()
                            {
                                TEID = teid
                            };
                            bus.ElemType = MM_Repository.FindElementType("BusbarSection");
                            //MM_Repository.TEIDs.Add(teid, bus);

                        }
                        bus.Name = rd.GetString(i++); // name is Substation.VL.NodeID
                        bus.Substation = GetElement(rd.GetInt32(i++)) as MM_Substation;
                        bus.Owner = GetElement(rd.GetInt32(i++)) as MM_Company;
                        bus.Operator = GetElement(rd.GetInt32(i++)) as MM_Company;
                        bus.TEID = teid;
                       // bus.HistoricServerPath = rd.GetString(i++);
                        bus.KVLevel = MM_Repository.FindKVLevel(rd.GetString(i) + " KV");
                        bus.Node = new MM_Node();
                        bus.Node.TEID = nodeTeid;
                        bus.Node.AssociatedBus = bus; // hope this will garbage collect
                        bus.Node.Name = bus.Name;
                        bus.Node.KVLevel = bus.KVLevel;
                        bus.Node.Substation = bus.Substation;
                        bus.Substation.BusbarSections.Add(bus);
                        
                        if (!MM_Repository.TEIDs.ContainsKey(nodeTeid))
                            MM_Repository.TEIDs.Add(nodeTeid, bus.Node);
                        if (!MM_Repository.Busbars.ContainsKey(bus.Name))
                            MM_Repository.Busbars.Add(bus.Name, bus);
                    }
                }
            }
            catch (Exception ex)
            {
                MM_System_Interfaces.WriteConsoleLine("Problem reading substations.");
                MM_System_Interfaces.WriteConsoleLine(ex.Message);
                MM_System_Interfaces.WriteConsoleLine(ex.StackTrace);
                throw ex;
            }
        }

        private void LoadLoads(SqlCommand cmd)
        {
            try
            {
                cmd.CommandText = "select TEID, Name, Node, Substation, Owner, Operator, HistoricServerPath, KVLevel"
                  + " from [mmLoads]";
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    MM_Repository.Loads.Clear();

                    while (rd.Read())
                    {
                        int i = 0;
                        int teid = rd.GetInt32(i++);

                        var load = GetElement(teid) as MM_Load;
                        if (load == null)
                        {
                            load = new MM_Load()
                            {
                                TEID = teid
                            };
                            load.ElemType = MM_Repository.FindElementType("Load");
                            MM_Repository.TEIDs.Add(teid, load);

                        }
                        load.Name = rd.GetString(i++);
                        MM_Bus bus = null;

                        string node = rd.GetString(i++);

                        
                        load.Substation = GetElement(rd.GetInt32(i++)) as MM_Substation;
                        load.Owner = GetElement(rd.GetInt32(i++)) as MM_Company;
                        load.Operator = GetElement(rd.GetInt32(i++)) as MM_Company;
                        load.TEID = teid;

                        var af = rd.GetSqlString(i++);
                        if (!af.IsNull)
                            load.HistoricServerPath = af.Value;

                        string vl = rd.GetString(i);

                        load.KVLevel = MM_Repository.FindKVLevel(vl + " KV");

                        if (load.Substation != null)
                        {
                            node = load.Substation.Name + "." + vl + "." + node;

                            MM_Repository.Busbars.TryGetValue(node, out bus);
                            if (bus != null && bus.Node != null)
                            {
                               bus.Node.ConnectedElements.Add(load);
                            }
                        }
                        if (!MM_Repository.Loads.ContainsKey(load.Name))
                            MM_Repository.Loads.Add(load.Name, load);
                    }
                }
            }
            catch (Exception ex)
            {
                MM_System_Interfaces.WriteConsoleLine("Problem reading loads.");
                MM_System_Interfaces.WriteConsoleLine(ex.Message);
                MM_System_Interfaces.WriteConsoleLine(ex.StackTrace);
                throw ex;
            }
        }


        private void LoadShuntCompensators(SqlCommand cmd)
        {
            try
            {
                cmd.CommandText = "select TEID,  MVARNOM, Name, Node, Substation, Owner, Operator, HistoricServerPath, KVLevel"
                  + " from mmShuntCompensators";
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    MM_Repository.ShuntCompensators.Clear();

                    while (rd.Read())
                    {
                        int i = 0;
                        int teid = rd.GetInt32(i++);
                        float mvarnom = (float)rd.GetDouble(i++);

                        var compensator = GetElement(teid) as MM_ShuntCompensator;
                        if (compensator == null)
                        {
                            compensator = new MM_ShuntCompensator()
                            {
                                TEID = teid
                            };
                            if (mvarnom > 0)
                                compensator.ElemType = MM_Repository.FindElementType("Capacitor");
                            else
                                compensator.ElemType = MM_Repository.FindElementType("Reactor");
                            MM_Repository.TEIDs.Add(teid, compensator);
                        }
                        compensator.Name = rd.GetString(i++);
                        MM_Bus bus = null;

                        string node = rd.GetString(i++);
                        compensator.Substation = GetElement(rd.GetInt32(i++)) as MM_Substation;
                        compensator.Owner = GetElement(rd.GetInt32(i++)) as MM_Company;
                        compensator.Operator = GetElement(rd.GetInt32(i++)) as MM_Company;
                        compensator.TEID = teid;
                        compensator.HistoricServerPath = rd.GetString(i++);
                        string vl = rd.GetString(i);
                        compensator.KVLevel = MM_Repository.FindKVLevel(vl + " KV");

                        if (compensator.Substation != null)
                        {
                            node = compensator.Substation.Name + "." + vl + "." + node;

                            MM_Repository.Busbars.TryGetValue(node, out bus);
                            if (bus != null && bus.Node != null)
                            {
                                bus.Node.ConnectedElements.Add(compensator);
                            }
                        }
                        if (!MM_Repository.ShuntCompensators.ContainsKey(compensator.Name))
                            MM_Repository.ShuntCompensators.Add(compensator.Name, compensator);
                    }
                }
            }
            catch (Exception ex)
            {
                MM_System_Interfaces.WriteConsoleLine("Problem reading substations.");
                MM_System_Interfaces.WriteConsoleLine(ex.Message);
                MM_System_Interfaces.WriteConsoleLine(ex.StackTrace);
                throw ex;
            }
        }


        private void LoadCircuitBreakers(SqlCommand cmd)
        {
            try
            {
                cmd.CommandText = "select TEID, [$Key] as Name, Node, Substation, Owner, Operator, KVLevel"
                  + " from mmCBs";
                using (SqlDataReader rd = cmd.ExecuteReader())
                {

                    while (rd.Read())
                    {
                        try
                        {
                            int i = 0;
                            int teid = rd.GetInt32(i++);


                            var cb = GetElement(teid) as MM_Breaker_Switch;
                            if (cb == null)
                            {
                                cb = new MM_Breaker_Switch()
                                     {
                                         TEID = teid
                                     };
                                MM_Repository.TEIDs.Add(teid, cb);
                            }
                            cb.Name = rd.GetString(i++).Replace("\"", "");

                            if (cb.Name.Contains(".CB."))
                                cb.ElemType = MM_Repository.FindElementType("Breaker");
                            else
                                cb.ElemType = MM_Repository.FindElementType("Switch");

                            MM_Bus bus = null;

                            string node = rd.GetString(i++).Replace("\"", "");


                            cb.Substation = GetElement(rd.GetInt32(i++)) as MM_Substation;
                            cb.Owner = GetElement(rd.GetInt32(i++)) as MM_Company;
                            cb.Operator = GetElement(rd.GetInt32(i++)) as MM_Company;
                            cb.TEID = teid;
                            string vl = rd.GetString(i);
                            cb.KVLevel = MM_Repository.FindKVLevel(vl + " KV");

                            if (cb.Substation != null)
                            {
                                node = cb.Substation.Name + "." + vl + "." + node;

                                MM_Repository.Busbars.TryGetValue(node, out bus);
                                if (bus != null && bus.Node != null)
                                {
                                    bus.Node.ConnectedElements.Add(cb);
                                }
                            }
                        }
                        catch (Exception exp)
                        {
                            MM_System_Interfaces.WriteConsoleLine(exp.StackTrace);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MM_System_Interfaces.WriteConsoleLine("Problem reading cbs.");
                MM_System_Interfaces.WriteConsoleLine(ex.Message);
                MM_System_Interfaces.WriteConsoleLine(ex.StackTrace);
                throw ex;
            }
        }


        private void LoadTransformers(SqlCommand cmd)
        {
            try
            {
                cmd.CommandText = "select Transformer, Teid, parent, Name, IsToEnd, Substation, KVLevel, NormalLimit, EmerLimit, LoadshedLimit, Node, Owner, Operator, HistoricServerPath"
                  + " from [mmXFWindings]";

                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    Transformers.Clear();

                    while (rd.Read())
                    {
                        int i = 0;
                        int parentTeid = rd.GetInt32(i++);
                        int teid = rd.GetInt32(i++);
                        string parentName = rd.GetString(i++);
                        string windingName = rd.GetString(i++);
                        bool isToEnd = rd.GetInt32(i++) == 1;

                        var transformer = GetElement(parentTeid) as MM_Transformer;
                        if (transformer == null)
                        {
                            transformer = new MM_Transformer()
                            {
                                TEID = parentTeid
                            };
                            transformer.ElemType = MM_Repository.FindElementType("Transformer");
                            MM_Repository.TEIDs.Add(parentTeid, transformer);
                        }
                        transformer.Name = parentName;
                        transformer.Substation = GetElement(rd.GetInt32(i++)) as MM_Substation;
                        string vl = rd.GetString(i++);
                        transformer.NormalLimit = (float)rd.GetDouble(i++);
                        transformer.EmergencyLimit = (float)rd.GetDouble(i++);
                        transformer.LoadshedLimit = (float)rd.GetDouble(i++);

                        string node = rd.GetString(i++);

                        transformer.Owner = GetElement(rd.GetInt32(i++)) as MM_Company;
                        transformer.Operator = GetElement(rd.GetInt32(i++)) as MM_Company;
                        transformer.HistoricServerPath = rd.GetString(i++);

                        MM_Bus bus = null;

                        transformer.KVLevel = MM_Repository.FindKVLevel(vl + " KV");
                   

                        if (isToEnd)
                        {
                            transformer.Winding1.ElemType = MM_Repository.FindElementType("TransformerWinding");
                            transformer.Winding1.Name = windingName;
                            transformer.Winding1.KVLevel = transformer.KVLevel;
                            transformer.Winding1.Voltage = transformer.KVLevel.Nominal;
                            transformer.Winding1.Substation = transformer.Substation;
                            transformer.KVLevel1 = transformer.KVLevel;
                            transformer.Winding1.WindingType = MM_TransformerWinding.enumWindingType.Primary;
                            transformer.Winding1.TEID = teid;
                            transformer.Winding1.Transformer = transformer;
                            if (!MM_Repository.TEIDs.ContainsKey(teid))
                                MM_Repository.TEIDs.Add(teid, transformer.Winding1);
                            
                        } else
                        {
                            transformer.Winding2.ElemType = MM_Repository.FindElementType("TransformerWinding");
                            transformer.Winding2.Name = windingName;
                            transformer.Winding2.KVLevel = transformer.KVLevel;
                            transformer.Winding2.Voltage = transformer.KVLevel.Nominal;
                            transformer.Winding2.Substation = transformer.Substation;
                            transformer.Winding2.WindingType = MM_TransformerWinding.enumWindingType.Secondary;
                            transformer.KVLevel2 = transformer.KVLevel;
                            transformer.Winding2.TEID = teid;
                            transformer.Winding2.Transformer = transformer;
                            if (!MM_Repository.TEIDs.ContainsKey(teid))
                                MM_Repository.TEIDs.Add(teid, transformer.Winding2);
                        }

                        if (transformer.Substation != null)
                        {
                            node = transformer.Substation.Name + "." + vl + "." + node;

                            MM_Repository.Busbars.TryGetValue(node, out bus);
                            if (bus != null && bus.Node != null)
                            {
                                if (isToEnd)
                                {
                                    bus.Node.ConnectedElements.Add(transformer.Winding1);
                                    transformer.Winding1.WindingNodeTEID = bus.Node.TEID;
                                    
                                } else
                                {
                                    bus.Node.ConnectedElements.Add(transformer.Winding2);
                                    transformer.Winding2.WindingNodeTEID = bus.Node.TEID;
                                }

                            }
                        }

                        if (!Transformers.ContainsKey(transformer.Name))
                        {
                            Transformers.Add(transformer.Name, transformer);
                            if (transformer.Substation != null)
                                transformer.Substation.Transformers.Add(transformer);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MM_System_Interfaces.WriteConsoleLine("Problem reading substations.");
                MM_System_Interfaces.WriteConsoleLine(ex.Message);
                MM_System_Interfaces.WriteConsoleLine(ex.StackTrace);
                throw ex;
            }
        }

        private void LoadUnits(SqlCommand cmd)
        {
            try
            {
                cmd.CommandText = "select TEID, Name, Node, u.Substation, Owner, Operator, u.GenmomName, u.MarketResourceName, MaxCapacity, u.ReserveZone, HistoricServerPath, KVLevel, u.PrimaryFuel, u.ResourceAssetType, u.ParticipantName, g.FriendlyName, g.OtherCompanyContact, g.Notes"
                  + " from mmUnits u left join opslog.dbo.generators g on g.resourceassetname = u.marketresourcename";
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    MM_Repository.Units.Clear();

                    while (rd.Read())
                    {
                        int i = 0;
                        int teid = rd.GetInt32(i++);

                        var unit = GetElement(teid) as MM_Unit;
                        if (unit == null)
                        {
                            unit = new MM_Unit()
                            {
                                TEID = teid,
                                
                            };
                            unit.ElemType = MM_Repository.FindElementType("Unit");
                           
                            MM_Repository.TEIDs.Add(teid, unit);
                        }
                        unit.Name = rd.GetString(i++);
                        MM_Bus bus = null;

                        string node = rd.GetString(i++);

                        unit.Substation = GetElement(rd.GetInt32(i++)) as MM_Substation;
                        unit.Owner = GetElement(rd.GetInt32(i++)) as MM_Company;
                        unit.Operator = GetElement(rd.GetInt32(i++)) as MM_Company;
                        unit.TEID = teid;
                      
                        var gn = rd.GetSqlString(i++);
                        if (!gn.IsNull)
                            unit.GenmomName = gn.Value;
                        var rn = rd.GetSqlString(i++);
                        if (!rn.IsNull)
                            unit.MarketResourceName = rn.Value;
                        unit.MaxCapacity = (int)rd.GetDouble(i++);
                        unit.ReserveZone = Int32.Parse(rd.GetString(i++));

                        unit.HistoricServerPath = rd.GetString(i++);
                        string vl = rd.GetString(i++);
                        string fuel = rd.SafeGetString(i++);
                        unit.KVLevel = MM_Repository.FindKVLevel(vl+ " KV");
                        unit.UnitType = MM_Repository.FindGenerationType(fuel);
                        unit.Fuel = fuel;
                        unit.MarketResourceType = rd.SafeGetString(i++);
                        unit.MarketParticipantName = rd.SafeGetString(i++);
                        unit.FriendlyName = rd.SafeGetString(i++);
                        unit.ContactInfo = rd.SafeGetString(i++);
                        unit.Description = rd.SafeGetString(i++);

                        if (unit.Substation != null)
                        {
                            node = unit.Substation.Name + "." + vl + "." + node;

                            MM_Repository.Busbars.TryGetValue(node, out bus);
                            if (bus != null && bus.Node != null)
                            {
                                bus.Node.ConnectedElements.Add(unit);
                            }
                            unit.Substation.Units.Add(unit);
                        }
                        if (!MM_Repository.Units.ContainsKey(unit.Name))
                            MM_Repository.Units.Add(unit.Name, unit);

                        unit.UnitType.Units.Add(unit);
                    }
                }
            }
            catch (Exception ex)
            {
                MM_System_Interfaces.WriteConsoleLine("Problem reading units.");
                MM_System_Interfaces.WriteConsoleLine(ex.Message);
                MM_System_Interfaces.WriteConsoleLine(ex.StackTrace);
                throw ex;
            }
        }


        private void LoadLines(SqlCommand cmd, bool isZbrs = false, bool isTie = false)
        {
            try
            {
                cmd.CommandText = "select TEID, Name, Substation1, Substation2, isNull(Latitude,0) as Latitude, isNull(Longitude,0) as Longitude, ToEndKey, FromEndKey, NormalLimit, EmerLimit, LoadshedLimit, Owner, Operator, HistoricServerPath, KVLevel"
                  + " from [mmLines] order by name, rownum";

                if (isZbrs)
                {
                    cmd.CommandText = "select TEID, Name, Substation1, Substation2, 0 as Latitude, 0 as Longitude, ToEndKey, FromEndKey, NormalLimit, EmerLimit, LoadshedLimit, Owner, Operator, HistoricServerPath, KVLevel"
                    + " from [mmZbrs] order by name";
                }

                if (isTie && !isZbrs)
                {
                    cmd.CommandText = "select TEID, TYLN_TEID, EmsGenMomKey, IsDC, Name, Substation1, Substation2, isNull(Latitude,0) as Latitude, isNull(Longitude,0) as Longitude, ToEndKey, FromEndKey, NormalLimit, EmerLimit, LoadshedLimit, Owner, Operator, HistoricServerPath, KVLevel, TyLn, OPA, UTIL"
                     + " from [mmTies] order by name, rownum";
                }
                else if (isTie)
                {
                    cmd.CommandText = "select TEID, TYLN_TEID, EmsGenMomKey, IsDC, Name, Substation1, Substation2, isNull(Latitude,0) as Latitude, isNull(Longitude,0) as Longitude, ToEndKey, FromEndKey, NormalLimit, EmerLimit, LoadshedLimit, Owner, Operator, HistoricServerPath, KVLevel, TyLn, OPA, UTIL"
                     + " from [mmTieZbrs] order by name, rownum";
                }

                MM_Line lastLine = null;

                using (SqlDataReader rd = cmd.ExecuteReader())
                {

                    while (rd.Read())
                    {
                        int i = 0;
                        int teid = rd.GetInt32(i++);
                        int tieTeid = 0;
                        string tieCooridor = "";
                        bool isDC = false;

                        if (isTie)
                        {
                            tieTeid = rd.GetInt32(i++);
                            tieCooridor = rd.GetString(i++).Replace("TOPOLOGY.", "");
                            isDC = rd.GetBoolean(i++);
                        }

                        var line = GetElement(teid) as MM_Line;

                        if (line == null)
                        {
                            line = new MM_Line()
                                   {
                                       TEID = teid
                                   };
                            line.ElemType = MM_Repository.FindElementType("Line");
                            
                            
                            MM_Repository.TEIDs.Add(teid, line);
                        }
                        line.IsZBR = isZbrs;
                        line.Name = rd.GetString(i++).Replace("\"", "");
                        MM_Tie tie = null;         
                        MM_Bus bus = null;
                        try
                        {
                            if (!rd.IsDBNull(i))
                                line.Substation1 = GetElement(rd.GetSqlInt32(i).Value) as MM_Substation;
                            i++;
                            if (!rd.IsDBNull(i))
                                line.Substation2 = GetElement(rd.GetSqlInt32(i).Value) as MM_Substation;
                            i++;
                        } catch (Exception ex)
                        {
                            MM_System_Interfaces.WriteConsoleLine(ex.Message);
                        }

                        if (line.Substation1 == null)
                            line.Substation1 = line.Substation2;
                        if (line.Substation2 == null)
                            line.Substation2 = line.Substation1;


                       line.Substation = line.Substation1;

                            // add substation coordinates
                            if (line._Coordinates.Count == 0 && line.Substation1 != null)
                        {
                            line.AddCoordinate(line.Substation1.LngLat);
                            if (line.Substation2 != null)
                                line.AddCoordinate(line.Substation2.LngLat);
                        }
                        if (!isZbrs)
                        {
                            float lat = 0;
                            float longitude =0;
                            try
                            {
                                var latDB = rd.GetSqlDouble(i++);
                                var longDB = rd.GetSqlDouble(i++);

                                lat = latDB.IsNull ? 0 : (float) latDB.Value;
                                longitude = longDB.IsNull ? 0 : (float) longDB.Value;
                            } catch (Exception)
                            {
                                lat = rd.GetFloat(--i);
                                longitude = rd.GetFloat(++i);
                                i++;
                            }
                            // add any non-zero coordinates if they don't already exist
                            if (lat != 0)
                            {
                                bool found = false;

                                for (int j = 0; j < line._Coordinates.Count; j++)
                                {
                                    if (Math.Abs(line._Coordinates[j].X - longitude) < .0001f && Math.Abs(line._Coordinates[j].Y - lat) < .0001f)
                                    {
                                        found = true;
                                        break;
                                    }
                                }
                                if (!found) // substation coordinates have already been added at this point
                                {
                                    if (line._Coordinates.Count > 0) // the to substation should be already added and kept at the end.
                                        line.InsertCoordinate(new PointF(longitude, lat));
                                    else
                                        line.AddCoordinate(new PointF(longitude, lat));
                                }

                            }
                        } else
                        {
                            i += 2;
                        }

                        if (lastLine == line)
                            continue;

                        string toEndKey = rd.GetString(i++).Replace("\"", "");
                        string fromEndKey = rd.GetString(i++).Replace("\"", "");
                        line.ToEndKey = toEndKey;
                        line.FromEndKey = fromEndKey;
                        line.NormalLimit = (float)rd.GetDouble(i++);
                        line.EmergencyLimit = (float)rd.GetDouble(i++);
                        line.LoadshedLimit = (float)rd.GetDouble(i++);

                        line.Owner = GetElement(rd.GetInt32(i++)) as MM_Company;
                        line.Operator = GetElement(rd.GetInt32(i++)) as MM_Company;
                        line.TEID = teid;
                        line.LineTEID = teid;

                        if (line.NormalLimit > 1f)
                        {
                            line.Estimated_MW[0] = 1f;
                            line.Estimated_MW[1] = .9f;
                            line.Estimated_MVA[0] = 1f;
                            line.Estimated_MVA[1] = .9f;
                            line.Telemetered_MW[0] = 1f;
                            line.Telemetered_MW[1] = .9f;
                            line.Estimated_MVAR[0] = .1f;
                            line.Estimated_MVAR[1] = .09f;
                        }
                        if (!rd.IsDBNull(i))
                            line.HistoricServerPath = rd.GetString(i).Replace("\"", "");
                        i++;
                        string kv = rd.GetString(i++);
                        line.KVLevel = MM_Repository.FindKVLevel(kv + " KV");

                        if (line.Substation1 != null)
                        {
                            int start = fromEndKey.IndexOf('.', 10);
                            start = fromEndKey.IndexOf('.', start + 1);
                            int end = fromEndKey.LastIndexOf('.');

                            string node = fromEndKey.Substring(start + 1, end - start - 1).Trim();

                            MM_Repository.Busbars.TryGetValue(node, out bus);
                            if (bus != null && bus.Node != null)
                            {
                                bus.Node.ConnectedElements.Add(line);
                            }
                            if (bus != null)
                                line.NodeTEIDs[0] = bus.Node.TEID;

                            line.Substation = line.Substation1;


                            bool found = false;

                            for (int j = 0;j < line._Coordinates.Count;j++)
                            {
                                if (Math.Abs(line._Coordinates[j].X  - line.Substation1.LngLat.X) < .0001f && Math.Abs(line._Coordinates[j].Y - line.Substation1.LngLat.Y) < .0001f)
                                {
                                    found = true;
                                    break;
                                }
                            }
                            if (!found)
                                line.AddCoordinate(line.Substation1.LngLat);

                            if (line.Substation1 != null && line.Substation2 != null)
                            {
                                if (line.Counties.Count == 0)
                                {
                                    line.Counties.Add(line.Substation1.County);
                                    line.Counties.Add(line.Substation2.County);
                                }
                            }

                        }
                        //int endPos = 0;
                        if (line.Substation2 != null)
                        {
                            int start = toEndKey.IndexOf('.', 10);
                            start = toEndKey.IndexOf('.', start + 1);
                            int end = toEndKey.LastIndexOf('.');

                            string node = toEndKey.Substring(start + 1, end - start - 1).Trim();

                            MM_Repository.Busbars.TryGetValue(node, out bus);
                            if (bus != null && bus.Node != null)
                            {
                                bus.Node.ConnectedElements.Add(line);
                            }
                            if (bus != null)
                                line.NodeTEIDs[1] = bus.Node.TEID;

                            bool found = false;
                            for (int j = 0;j < line._Coordinates.Count;j++)
                            {
                                if (Math.Abs(line._Coordinates[j].X - line.Substation2.LngLat.X) < .0001f && Math.Abs(line._Coordinates[j].Y - line.Substation2.LngLat.Y) < .0001f)
                                {
                                    found = true;
                                    //endPos = j;
                                    break;
                                }
                            }
                            if (!found)
                                line.AddCoordinate(line.Substation2.LngLat);
                        }

                        if (isTie && (line.Substation1 != null || line.Substation2 != null))
                        {
                            tie = GetElement(tieTeid) as MM_Tie;
                            if (tie == null)
                            {
                                tie = new MM_Tie(line, tieCooridor);
                                tie.ElemType = MM_Repository.FindElementType("Tie");
                                tie.TEID = tieTeid;
                                tie.TieDescriptor = tieCooridor;
                                tie.Name = line.Name;
                                tie._Coordinates = line._Coordinates;
                                tie.IsZBR = line.IsZBR;
                                tie.IsDC = isDC;
                                tie.TyLn = rd.GetString(i++);
                                tie.OPA = rd.GetString(i++);
                                tie.UTIL = rd.GetString(i++);
                                if (tie.TyLn != null)
                                    tie.Name = tie.Name + "_" + tie.TyLn;
                                MM_Repository.TEIDs.Add(tieTeid, tie);
                                MM_Repository.Ties.Add(tieCooridor, tie);
                            }
                            tie.AssociatedLine = line;
                            tie.Substation = line.Substation1;
                            tie.KVLevel = line.KVLevel;
                            tie.Substation1 = line.Substation1;
                            tie.Substation2 = line.Substation2;
                            tie.MW_Integrated = .01f;
                            tie.Coordinates = line.Coordinates;
                            
                        }
                        if (line.KVLevel == null)
                            line.KVLevel = line.KVLevel = new MM_KVLevel() { };
                        if (lastLine != line)
                        {
                            lastLine = line;
                            if (!MM_Repository.Lines.ContainsKey(line.Name))
                                MM_Repository.Lines.Add(line.Name, line);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MM_System_Interfaces.WriteConsoleLine("Problem reading lines or zbrs.");
                MM_System_Interfaces.WriteConsoleLine(ex.Message);
                MM_System_Interfaces.WriteConsoleLine(ex.StackTrace);
                throw ex;
            }

            MM_System_Interfaces.WriteConsoleLine("Lines Loaded");
        }

    }

}
