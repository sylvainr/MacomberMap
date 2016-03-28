using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEDESimulator.Elements
{
   public class Sim_Company
    {
        public int TEID {get;set;}
        public String Name;
        public String Alias;
        public String TelephoneNumber;
        public String DUNS;
        public Sim_Company(String Name, String Alias, String TelephoneNumber, String DUNS, Sim_Builder Builder)
        {
            this.Name = Name;
            this.Alias = Alias;
            this.TelephoneNumber = TelephoneNumber;
            this.DUNS = DUNS;
            this.TEID = Builder.NextTEID();
        }

        public String GetXml()
        {
            return $"<Company PrimaryPhone=\"{TelephoneNumber}\" DUNS=\"{DUNS}\" TEID=\"{TEID}\" Name=\"{Name}\" ElemType=\"Company\" Alias=\"{Alias}\" />";
        }
    }
}
