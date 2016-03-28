using MacomberMapCommunications.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapCommunications.Messages.EMS
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class holds information on unit data
    /// </summary>
    [RetrievalCommand("GetUnitGenData"), UpdateCommand("UpdateUnitGenData"), FileName("Gen_Unit_Data.csv")]
    public class MM_Unit_Gen_Data
    {
        /// <summary></summary>
        public int TEID_UNIT { get; set; }

        /// <summary></summary>
        public string Key { get; set; }
        public float CAPMX_UNIT { get; set; }

   /// <summary></summary>
        public float RVSP_UNIT { get; set; }

        /// <summary>Cleared Reg Up</summary>
        public float CLREGUP_UNIT { get; set; }

        /// <summary>Cleared Reg DN</summary>
        public float CLREGDN_UNIT { get; set; }

        /// <summary>ECO MAX</summary>
        public float ECOMX_UNIT { get; set; }
		
        /// <summary></summary>
        public float HASL_UNIT { get; set; }

        /// <summary></summary>
        public float LASL_UNIT { get; set; }

        /// <summary></summary>
        public float HSL_UNIT { get; set; }

        /// <summary></summary>
        public float GEN_UNIT { get; set; }

        /// <summary></summary>
        public float LSL_UNIT { get; set; }

        /// <summary></summary>
        public float HDL_UNIT { get; set; }

        /// <summary></summary>
        public float LDL_UNIT { get; set; }

        /// <summary></summary>
        public float HEL_UNIT { get; set; }

        /// <summary></summary>
        public float LEL_UNIT { get; set; }

        /// <summary></summary>
        public float LMP_UNIT { get; set; }

        /// <summary></summary>
        public float SCEDBP_UNIT { get; set; }

        /// <summary></summary>
        public float PRCGN_UNIT { get; set; }

        /// <summary></summary>
        public float PRCHSYN_UNIT { get; set; }

        /// <summary></summary>
        public float RVBLKS_UNIT { get; set; }

        /// <summary></summary>
        public float RVRMRN_UNIT { get; set; }
      public float ECOMN_UNIT { get; set; }

        /// <summary>MOS MAX</summary>
        public float MOSCAPMX_UNIT { get; set; }

        /// <summary>MOS MIN</summary>
        public float MOSCAPMN_UNIT { get; set; }

        /// <summary>NET EMER MX</summary>
        public float NCEMGMX_UNIT { get; set; }

        /// <summary>Net EMER MIN</summary>
        public float NCEMGMN_UNIT { get; set; }

        /// <summary>Set Point</summary>
        public float SPPSETP_UNIT { get; set; }

        /// <summary>Ramped Set point</summary>
        public float SETPTRMP_UNIT { get; set; }

        /// <summary>MOS dispatch MW</summary>
        public float MOSBP_UNIT { get; set; }

        /// <summary>Reg Deployed</summary>
        public float REGDEP_UNIT { get; set; }

        /// <summary>Ramp rate up</summary>
        public float MOSRRUP_UNIT { get; set; }

        /// <summary>Control Mode</summary>
        public int UDSCMODE_UNIT { get; set; }

        /// <summary>Cleared Spin</summary>
        public float CLSPINCP_UNIT { get; set; }

        public string FUEL_UNIT { get; set; }
    }
}
