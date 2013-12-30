using System;
using System.Collections.Generic;
using System.Text;
using Macomber_Map.Data_Elements;
using System.Xml;
using System.Reflection;
using System.Threading;

namespace Macomber_Map.Data_Connections.Generic
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
            foreach (MM_DataConnector Conn in Data_Integration.DataConnections.Values)
                if (Conn.GetType().IsAssignableFrom(ConnectorType))
                    return BaseElement.GetType().IsAssignableFrom(TargetType);
            if (typeof(MM_Server_Connector).IsAssignableFrom(ConnectorType))
                return BaseElement.GetType().IsAssignableFrom(TargetType);
            return false;
        }
        #endregion

        /// <summary>
        /// Execute the command within its own thread
        /// </summary>
        /// <param name="AssignedObject"></param>
        /// 
    [STAThread]
        private void ExecuteCommandInThread(Object AssignedObject) 
        {
            //Thread.CurrentThread.SetApartmentState(ApartmentState.STA);
            String InCmd = Command;

            //Replace the {word} with the values for that element
            int Bracket = InCmd.IndexOf("{");
            while (Bracket > -1)
            {
                String InWord = InCmd.Substring(Bracket + 1, InCmd.IndexOf("}", Bracket) - Bracket - 1);
                Object InValue = null;
                foreach (MemberInfo mI in AssignedObject.GetType().GetMember(InWord))
                    if (mI is FieldInfo)
                        InValue = (mI as FieldInfo).GetValue(AssignedObject);
                    else if (mI is PropertyInfo)
                        InValue = (mI as PropertyInfo).GetValue(AssignedObject, null);
                InCmd = InCmd.Substring(0, Bracket) + InValue.ToString() + InCmd.Substring(InCmd.IndexOf('}', Bracket) + 1);
                Bracket = InCmd.IndexOf("{");
            }

            //Now, locate the appropriate connector
            //TODO: Send the appropriate command            
        }

        /// <summary>
        /// Execute the command based on the element
        /// </summary>
        /// <param name="AssignedObject">The assigned object, from which parameters should be retrieved</param>
        public void Execute(Object AssignedObject)
        {
            //vs below, init a thred new line, ////nataros
            //ThreadPool.QueueUserWorkItem(new WaitCallback(ExecuteCommandInThread), AssignedObject);
            Thread newThread = new Thread(ExecuteCommandInThread);//new ParameterizedThreadStart(ExecuteCommandInThread));
            newThread.TrySetApartmentState(ApartmentState.STA);
            newThread.Start(AssignedObject);
        }
    }
}
