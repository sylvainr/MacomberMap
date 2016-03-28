using MacomberMapCommunications.Attributes;
using MacomberMapCommunications.Messages.EMS;
using MacomberMapCommunications.WCF;
using MacomberMapIntegratedService.Properties;
using MacomberMapIntegratedService.Service;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MacomberMapIntegratedService.EMS
{
    /// <summary>
    /// This class provides a static TCP listener, handling inputs from the TCP server
    /// </summary>
    public static class MM_EMSReader_TCP
    {
        #region Variable declarations
        /// <summary>Our listener</summary>
        private static TcpListener Listener;

        /// <summary>Our collection of input types</summary>
        public static Dictionary<String, Type> InputTypes = new Dictionary<string, Type>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>Our collection of update commands</summary>
        public static Dictionary<String, String> UpdateCommands = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
        #endregion

        /// <summary>
        /// Read our XML configuration file
        /// </summary>
        public static void DetermineFileTypes()
        {
            //Add in our list of file paths
            foreach (Type FoundType in typeof(MM_Line_Data).Assembly.GetTypes())
            {
                UpdateCommandAttribute UpdateCommand = null;
                RetrievalCommandAttribute RetrievalCommand = null;
                FileNameAttribute FileName = null;

                foreach (object obj in FoundType.GetCustomAttributes(false))
                    if (obj is UpdateCommandAttribute)
                        UpdateCommand = (UpdateCommandAttribute)obj;
                    else if (obj is FileNameAttribute)
                        FileName = (FileNameAttribute)obj;
                    else if (obj is RetrievalCommandAttribute)
                        RetrievalCommand = (RetrievalCommandAttribute)obj;

                if (UpdateCommand != null && FileName != null)
                {
                    MM_EMSReader_TCP.InputTypes.Add(FileName.FileName, FoundType);
                    MM_EMSReader_TCP.UpdateCommands.Add(FileName.FileName, UpdateCommand.UpdateCommand);
                    if (!String.IsNullOrEmpty(Settings.Default.TEDESourceFolder))
                        MM_EMSReader_File.FileInfo.Add(FileName.FileName, new MM_EMSReader_File(Path.Combine(Settings.Default.TEDESourceFolder, FileName.FileName), UpdateCommand.UpdateCommand, FoundType));
                }

                // FileInfo.Add(FileName.FileName, new MM_EMSReader_FileInformation(Path.Combine(Settings.Default.SourceFolder,FileName.FileName), UpdateCommand.UpdateCommand, FoundType));

                //Check our update command, make sure it's okay
                if (UpdateCommand != null && FileName != null && typeof(IMM_ConversationMessage_Types).GetMethod(UpdateCommand.UpdateCommand) == null)
                    MM_Notification.WriteLine(ConsoleColor.Yellow, "Unable to find update command {0} for type {1}", UpdateCommand.UpdateCommand, FileName.FileName);
                if (RetrievalCommand != null && FileName != null && typeof(IMM_EMS_Types).GetMethod(RetrievalCommand.RetrievalCommand) == null)
                    MM_Notification.WriteLine(ConsoleColor.Yellow, "Unable to find retrieval command {0} for type {1}", RetrievalCommand.RetrievalCommand, FileName.FileName);
            }
        }

        /// <summary>
        /// Initialize our TCP server
        /// </summary>
        public static void Initialize()
        {
            //First, build our dictionary of types
            DetermineFileTypes();
            ThreadPool.QueueUserWorkItem(new WaitCallback(StartListener));
        }

        /// <summary>
        /// Start our listener
        /// </summary>
        /// <param name="state"></param>
        private static void StartListener(object state)
        {
            Listener = new TcpListener(IPAddress.Any, Properties.Settings.Default.TCPListener);
            Listener.Start();
            MM_Notification.WriteLine(ConsoleColor.Green, "EMS: Listening for incoming TCP connections on {0}", Listener.LocalEndpoint);
            while (true)
                try
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessTCPMessage), Listener.AcceptTcpClient());
                }
                catch (Exception ex)
                {
                    MM_Notification.WriteLine(ConsoleColor.Red, "EMS: Unable to accept an incoming connection: {0}", ex);
                }
        }

        /// <summary>
        /// Process an incoming client
        /// </summary>
        /// <param name="state"></param>
        private static void ProcessTCPMessage(object state)
        {
            //Now, notify that we have a connection, let's create our stream reader around it, and retrieve our header
            TcpClient Conn = (TcpClient)state;
            String FileName = "";
            DateTime StartTime = DateTime.Now;

            //If our source IP address changes, notify the console                        
            IPAddress Address = ((IPEndPoint)Conn.Client.RemoteEndPoint).Address;
            if (Address.ToString() != MM_Server.LastTEDEAddress.ToString())
                MM_Notification.WriteLine(ConsoleColor.Yellow, "TEDE: IP Source Address change detected: {0}, last {1}", Address, MM_Server.LastTEDEAddress);
            MM_Server.LastTEDEAddress = Address;

            using (NetworkStream nRd = Conn.GetStream())
            using (StreamReader sRd = new StreamReader(nRd))
                try
                {
                    //Define our key variables
                    List<String> OutList = new List<string>();
                    bool AtBeginning = true;
                    PropertyInfo[] HeaderInfo = null;
                    DateTime BatchId = default(DateTime);
                    Type TargetType = null;

                    //Now, read in all of our lines of data, using reflection to store our data
                    String InLine;
                    while ((InLine = sRd.ReadLine()) != null)
                        if (InLine.StartsWith("#"))
                        {
                            String[] splStr = InLine.TrimStart('#').TrimEnd(',').Split(',');
                            MM_Server.LastTEDEUpdate = DateTime.Now;

                            //If we're at the beginning, process accordingly
                            if (AtBeginning)
                            {
                                StartTime = DateTime.Now;
                                //Pull in, and parse our first line.
                                BatchId = new DateTime(1970, 1, 1).AddSeconds(Convert.ToDouble(splStr[0]));
                                FileName = splStr[1];
                                if (!FileName.EndsWith(".csv", StringComparison.CurrentCultureIgnoreCase))
                                    FileName += ".csv";

                                if (InputTypes.TryGetValue(FileName, out TargetType))
                                {
                                    //Now that we have our first, line, build our outgoing list                                                              
                                    OutList.Clear();

                                    //Read our header
                                    String[] HeaderLine = sRd.ReadLine().TrimStart('#').Split(',');

                                    HeaderInfo = MM_Serialization.GetHeaderInfo(TargetType, HeaderLine);
                                    for (int a = 0; a < HeaderLine.Length; a++)
                                        if (HeaderInfo[a] == null)
                                            MM_Notification.WriteLine(ConsoleColor.Yellow, "Unknown variable {0} in {1}", HeaderLine[a], FileName);

                                    //Confirm all of our headers are present
                                    foreach (PropertyInfo pI in TargetType.GetProperties())
                                        if (Array.FindIndex<String>(HeaderLine, T => T.Equals(pI.Name, StringComparison.CurrentCultureIgnoreCase)) == -1)
                                            MM_Notification.WriteLine(ConsoleColor.Yellow, "Missing variable {0} ({1}) in {2} / {3}", pI.Name, pI.PropertyType.Name, TargetType.Name, FileName);
                                }
                                else
                                {
                                    //Console.WriteLine("Rereading file " + FileName);
                                    sRd.ReadLine();
                                }

                                AtBeginning = false;
                            }
                            else
                            {
                                DateTime EndTime = new DateTime(1970, 1, 1).AddSeconds(Convert.ToDouble(splStr[0]));
                                if (EndTime != BatchId)
                                    MM_Notification.WriteLine(ConsoleColor.Red, "Mismatch date on {0}: Start {1}, End {2}", FileName, BatchId, EndTime);
                                if (OutList != null)
                                    ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessData), new object[] { OutList.ToArray(), TargetType, HeaderInfo });
                                else
                                {
                                    //MM_Notification.WriteLine(ConsoleColor.Yellow, "Ignore processing data from {0}.", FileName);
                                }
                                AtBeginning = true;
                            }
                        }
                        else if (OutList != null)
                            OutList.Add(InLine);
                }
                catch (Exception ex)
                {
                    MM_Notification.WriteLine(ConsoleColor.Red, "Error reading {0} from {1}: {2}", FileName, Conn.Client.RemoteEndPoint, ex);
                }
        }

        /// <summary>
        /// Process our data
        /// </summary>
        /// <param name="state"></param>
        private static void ProcessData(object state)
        {
            object[] InVals = (object[])state;
            String[] InLines = (string[])InVals[0];
            Type TargetType = (Type)InVals[1];
            PropertyInfo[] HeaderInfo = (PropertyInfo[])InVals[2];
            try
            {
                if (TargetType != null)
                {
                Array OutList = Array.CreateInstance(TargetType, InLines.Length);
                for (int a = 0; a < InLines.Length; a++)
                    OutList.SetValue(MM_Serialization.Deserialize(HeaderInfo, InLines[a].Split(','), Activator.CreateInstance(TargetType)), a);
                MM_EMS_Data_Updater.ProcessUpdate(TargetType, OutList);
                OutList = null;
                }
            }
            catch (Exception ex)
            {
                MM_Notification.WriteLine(ConsoleColor.Red, "Error sending {0} data: {1}", (TargetType == null ? "UNKNOWN" : TargetType.Name), ex);
            }
        }
    }
}