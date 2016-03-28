using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MacomberMapClient.Integration;

namespace MacomberMapClient.Data_Elements.Physical
{
    public class MM_Flowgate : MM_Contingency
    {
        public List<int> MonitoredElements { get; set; } 

        public List<int> ContingentElements { get; set; }
        /// <summary>
        /// percent loaded. The MM displays will automatically display this as a percent and multiply * 100 since it has the word "percent" in it.
        /// </summary>
        public float PercentLoaded
        {

            get
            {
                return (PCTGFlow/EmerLimit);
            }

        }

        public float Lodf { get; set; }

        public int Idc { get; set; }

        public string FlowgateType 
        {
            get;
            set;
        }

        public float NormLimit { get; set; }

        public float EmerLimit { get; set; }

        public float PCTGFlow { get; set; }

        public float IROLLimit { get; set; }

        public string Reason { get; set; }

        public string Hint { get; set; }


        public MM_Flowgate()
        {
            MonitoredElements= new List<int>();
            ContingentElements = new List<int>();
        }

        public MM_Flowgate(XmlElement ElementSource, bool AddIfNew)
           : base(ElementSource, AddIfNew)
        {
            this.ElemType = MM_Repository.FindElementType("Flowgate");
            MonitoredElements = new List<int>();
            ContingentElements = new List<int>();
        }
    }
}
