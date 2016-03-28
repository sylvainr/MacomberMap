using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEDESimulator.Elements
{
    public class Sim_Random : Random
    {
        public Sim_Random(int Seed) : base(Seed) { }

        public Sim_Random() : base() { }

        /// <summary>
        /// Generate a random number around a gaussian curve
        /// </summary>
        /// <param name="Mean"></param>
        /// <param name="StandardDeviation"></param>
        /// <returns></returns>
        public double GenerateNumber(double Mean, double StandardDeviation)
        {
            if (StandardDeviation == 0)
                return Mean;

            //First, generate a random number against a gaussian curve
            double u1 = NextDouble();
            double u2 = NextDouble();
            double r = Math.Sqrt(-2.0 * Math.Log(u1));
            double theta = 2.0 * Math.PI * u2;
            double z = r * Math.Sin(theta);
            return (z * StandardDeviation) + Mean;
        }

        /// <summary>
        /// Generate a random number from a distribution
        /// </summary>
        /// <param name="Distribution"></param>
        /// <returns></returns>
        public double GenerateNumber(Sim_Builder_Configuration.Distribution Distribution)
        {
            return GenerateNumber(Distribution.Average, Distribution.StdDev);
        }
    }
}
