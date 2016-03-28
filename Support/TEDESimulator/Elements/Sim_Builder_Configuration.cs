using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TEDESimulator.Elements
{
    /// <summary>
    /// © 2016, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides the builder configuration information for a custom run
    /// </summary>
    public class Sim_Builder_Configuration
    {
        /// <summary>The file for our state boundary</summary>
        public string StateBoundary;

        /// <summary>The file for our county boundary</summary>
        public string CountyBoundary;

        /// <summary>The file for our base model</summary>
        public string BaseModel;

        /// <summary>The target file we should write</summary>
        public string TargetModelFile;

        /// <summary>The folder for our one-lines</summary>
        public string OneLineFolder;

        /// <summary>How many companies we have</summary>
        public int CompanyCount;

        /// <summary>How many substations we have</summary>
        public int SubstationCount;

        /// <summary>The probability of having a line</summary>
        public double LineProbability;

        /// <summary>The line distance threshold in miles</summary>
        public double LineDistance;

        /// <summary>Our line MW</summary>
        public Distribution LineMW;

        /// <summary>Our line loading average</summary>
        public Distribution LineLoad;

        /// <summary>Our unit probability</summary>
        public double UnitProbability;

        /// <summary>Our unit MW</summary>
        public Distribution UnitMW;

        /// <summary>Our unit's capacity</summary>
        public Distribution UnitCapacity;

        /// <summary>Our load probability</summary>
        public double LoadProbability;

        /// <summary>Our load information</summary>
        public Distribution LoadMW;
        
        /// <summary>Our transformer MW</summary>
        public Distribution TransformerMW;

        public double CapacitorProbability, CapacitorOpenProbability;

        public Distribution CapacitorMVAR;

        public double ReactorProbability, ReactorOpenProbability;

        public Distribution ReactorMVAR;

        public double BusVoltageStdDev;

        public double BlackstartCorridorProbability;

        public class Distribution
        {
            /// <summary>The average of our distribution</summary>
            public double Average;

            /// <summary>The standard deviation of our distribution</summary>
            public double StdDev;

            /// <summary>
            /// Initialize a new distribution
            /// </summary>
            /// <param name="Average"></param>
            /// <param name="StdDev"></param>
            public Distribution(double Average, double StdDev)
            {
                this.Average = Average;
                this.StdDev = StdDev;
            }
        }


        /// <summary>
        /// Create a new builder configuration based on parameters retrieved from a collection of controls
        /// </summary>
        /// <param name="ctl"></param>
        public Sim_Builder_Configuration(Control.ControlCollection ctl)
        {
            //Now, retrieve all of our parameters.
            foreach (FieldInfo fI in typeof(Sim_Builder_Configuration).GetFields())
                if (fI.FieldType == typeof(int))
                    fI.SetValue(this, Convert.ToInt32(ctl["txt" + fI.Name].Text));
                else if (fI.FieldType == typeof(double))
                    if (fI.Name.EndsWith("Probability"))
                        fI.SetValue(this, Convert.ToDouble(ctl["txt" + fI.Name].Text) / 100.0);
                    else
                        fI.SetValue(this, Convert.ToDouble(ctl["txt" + fI.Name].Text));

                else if (fI.FieldType == typeof(String))
                    fI.SetValue(this, ctl["btn" + fI.Name].Tag);
                else if (fI.FieldType == typeof(Distribution))
                    fI.SetValue(this, new Distribution(Convert.ToDouble(ctl["txt" + fI.Name + "Avg"].Text), Convert.ToDouble(ctl["txt" + fI.Name + "StdDev"].Text)));
        }
    }
}
