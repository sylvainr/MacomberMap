using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapCommunications.Messages.EMS
{
    /// <summary>
    /// © 2015, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class holds information on an EMS command
    /// </summary>
    public class MM_EMS_Command: IComparable<MM_EMS_Command>
    {
        /// <summary>The name of the user that issued the command</summary>
        public String UserName { get; set; }

        /// <summary>The name of the computer that issued the command</summary>
        public String ComputerName { get; set; }

        /// <summary>The timestamp of the command issue</summary>
        public DateTime IssuedOn { get; set; }

        /// <summary>The simulator time</summary>
        public DateTime SimulatorTime { get; set; }

        /// <summary>The TEID of the affected object</summary>
        public int TEID { get; set; }


        /// <summary>The command type</summary>
        public string CommandType { get; set; }

        /// <summary>The command being executed</summary>
        public String CommandName { get; set; }

        /// <summary>The family being referenced</summary>
        public String Family { get; set; }

        /// <summary>The application being referenced</summary>
        public String Application { get; set; }

        /// <summary>The database being referenced</summary>
        public String Database { get; set; }

        /// <summary>The record being referenced</summary>
        public String Record { get; set; }

        /// <summary>The field being referenced</summary>
        public String Field { get; set; }

        /// <summary>The target value</summary>
        public String Value { get; set; }

        /// <summary>The old value, that the operator saw when the command was issued</summary>
        public String OldValue { get; set; }
      
        /// <summary>The server associated with the command</summary>
        [IgnoreDataMember]
        public String Server { get; set; }

        /// <summary>Initialize an empty EMS command</summary>
        public MM_EMS_Command()
        { }

        /// <summary>
        /// Initialize a new EMS command
        /// </summary>
        /// <param name="InCommand"></param>
        /// <param name="ComputerName"></param>
        /// <param name="SimulatorTime"></param>
        /// <param name="UserName"></param>
        /// <param name="OldValue"></param>
        public MM_EMS_Command(String InCommand, String UserName, String ComputerName, DateTime SimulatorTime, String OldValue)
        {
            try
            {
            String[] splStr = InCommand.Split(',');
            CommandType = splStr[0];
            Application = splStr[1];
            Family = splStr[2];
            CommandName = splStr[3];
            Database = splStr[4];
            Record = splStr[5];
            Field = splStr[6];
            TEID = Convert.ToInt32(splStr[7]);
            Value = splStr[8];
            this.UserName = UserName;
            this.ComputerName = ComputerName;
            this.IssuedOn = DateTime.Now;
            this.SimulatorTime = SimulatorTime;
            this.OldValue = OldValue;
            }
            catch { }
        }

        
        /// <summary>
        /// Report our header line
        /// </summary>
        public static String HeaderLine()
        {
            StringBuilder sB = new StringBuilder();
            foreach (PropertyInfo pI in typeof(MM_EMS_Command).GetProperties())
                if (pI.CanRead && pI.CanWrite)
                    sB.Append((sB.Length == 0 ? "" : ",") + pI.Name);
            return sB.ToString();
        }

        /// <summary>
        /// Report our header line
        /// </summary>
        public String BuildLine()
        {
            StringBuilder sB = new StringBuilder();
            foreach (PropertyInfo pI in typeof(MM_EMS_Command).GetProperties())
                if (pI.CanRead && pI.CanWrite)
                    sB.Append((sB.Length == 0 ? "" : ",") + (pI.GetValue(this) == null ? "" : pI.PropertyType == typeof(DateTime) ? ((DateTime)pI.GetValue(this) - new DateTime(1970, 1, 1)).TotalSeconds.ToString() : pI.GetValue(this).ToString()));
            return sB.ToString();
        }

        /// <summary>
        /// Build a command for simulation purposes
        /// </summary>
        /// <returns></returns>
        public String BuildOutgoingLine()
        {
            return CommandType + "," + Application + "," + Family + "," + CommandName + "," + Database + "," + Record + "," + Field + "," + TEID + "," + Value;
        }

       /// <summary>
       /// Compare two EMS commands
       /// </summary>
       /// <param name="other"></param>
       /// <returns></returns>
        public int CompareTo(MM_EMS_Command other)
        {
            return IssuedOn.CompareTo(other.IssuedOn);
        }
    }
}