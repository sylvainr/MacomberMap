using MacomberMapClient.Data_Elements.Physical;
using MacomberMapClient.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace MacomberMapClient.Data_Elements.SystemInformation
{
    /// <summary>
    /// This class holds the information on a command that can be executed
    /// </summary>
    public class MM_Command : MM_Serializable
    {
        #region Variable declarations
        /// <summary>The data connector type with which the command is associated</summary>
        public Type ConnectorType;

        /// <summary>The type for which the command is available</summary>
        public Type TargetType;

        /// <summary>The title of the command</summary>
        public String Title;

        /// <summary>The text of the command</summary>
        public String Command;

        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new MM command
        /// </summary>
        /// <param name="xConfig"></param>
        /// <param name="AddIfNew"></param>
        public MM_Command(XmlElement xConfig, bool AddIfNew)
            : base(xConfig, AddIfNew)
        {
        }
        #endregion

        #region Validation
        /// <summary>
        /// Determine whether a command applies to a particular element
        /// </summary>
        /// <param name="BaseElement"></param>
        /// <returns></returns>
        public bool Validate(MM_Element BaseElement)
        {
            return (ConnectorType == typeof(MM_Server_Interface) && MM_Server_Interface.Client != null);
        }
        #endregion

        /// <summary>
        /// Execute the command within its own thread
        /// </summary>
        /// <param name="Elem"></param>
        /// 
        public void Execute(MM_Element Elem)
        {
            //Thread.CurrentThread.SetApartmentState(ApartmentState.STA);
            String InCmd = Command;

            //Replace the {word} with the values for that element
            int Bracket = InCmd.IndexOf("{");
            while (Bracket > -1)
            {
                String InWord = InCmd.Substring(Bracket + 1, InCmd.IndexOf("}", Bracket) - Bracket - 1);
                Object InValue = null;
                foreach (MemberInfo mI in Elem.GetType().GetMember(InWord))
                    if (mI is FieldInfo)
                        InValue = (mI as FieldInfo).GetValue(Elem);
                    else if (mI is PropertyInfo)
                        InValue = (mI as PropertyInfo).GetValue(Elem, null);
                InCmd = InCmd.Substring(0, Bracket) + InValue.ToString() + InCmd.Substring(InCmd.IndexOf('}', Bracket) + 1);
                Bracket = InCmd.IndexOf("{");
            }

            //Now, send our command if appropriate
            if (MM_Server_Interface.SendCommand(InCmd,"") == CheckState.Unchecked)
                MessageBox.Show("Unable to send command. Please retry.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
