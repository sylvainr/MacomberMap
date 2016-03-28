using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEDESimulator.Elements
{
    public class Sim_VoltageLevel : IComparable<Sim_VoltageLevel>
    {
        public double NominalVoltage;

        public Sim_VoltageLevel(double NominalVoltage)
        {
            this.NominalVoltage = NominalVoltage;
        }

        public int CompareTo(Sim_VoltageLevel other)
        {
            return NominalVoltage.CompareTo(other.NominalVoltage);
        }

        public override string ToString()
        {
            if (NominalVoltage < 65)
                return "Other KV";
            else
                return NominalVoltage.ToString() + " KV";
        }
    }
}
