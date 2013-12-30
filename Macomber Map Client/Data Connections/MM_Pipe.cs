using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Threading;
using System.IO;
using System.Windows.Forms;
using Macomber_Map.Data_Elements;
using Macomber_Map.User_Interface_Components.NetworkMap;
using System.Drawing;
using Macomber_Map.User_Interface_Components;

namespace Macomber_Map.Data_Connections
{
    /// <summary>
    /// This class establishes a named pipe for inter-process communication
    /// </summary>
    public static class MM_Pipe
    {
        #region Imports
        /// <summary>
        /// Create a named pipe for communications between Macomber Map and other processes
        /// </summary>
        /// <param name="pipeName"></param>
        /// <param name="dwOpenMode"></param>
        /// <param name="dwPipeMode"></param>
        /// <param name="nMaxInstances"></param>
        /// <param name="nOutBufferSize"></param>
        /// <param name="nInBufferSize"></param>
        /// <param name="nDefaultTimeOut"></param>
        /// <param name="lpSecurityAttributes"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeFileHandle CreateNamedPipe(String pipeName, uint dwOpenMode, uint dwPipeMode, uint nMaxInstances, uint nOutBufferSize, uint nInBufferSize, uint nDefaultTimeOut, IntPtr lpSecurityAttributes);

        /// <summary>
        /// Connect to a named pipe
        /// </summary>
        /// <param name="hNamedPipe"></param>
        /// <param name="lpOverlapped"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int ConnectNamedPipe(SafeFileHandle hNamedPipe, IntPtr lpOverlapped);

        /// <summary>The map to be interfaced with</summary>
        public static MacomberMap Map;

        /// <summary>
        /// Create a file handle using native functions
        /// </summary>
        /// <param name="pipeName"></param>
        /// <param name="dwDesiredAccess"></param>
        /// <param name="dwShareMode"></param>
        /// <param name="lpSecurityAttributes"></param>
        /// <param name="dwCreationDisposition"></param>
        /// <param name="dwFlagsAndAttributes"></param>
        /// <param name="hTemplate"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern SafeFileHandle CreateFile(String pipeName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplate);
      
        #endregion

        #region Constants
        private const uint GENERIC_READ = (0x80000000);
        private const uint GENERIC_WRITE = (0x40000000);
        private const uint OPEN_EXISTING = 3;        
        private const uint DUPLEX = (0x00000003);
        private const uint FILE_FLAG_OVERLAPPED = (0x40000000);
        private static string PIPE_NAME;
        private const int BUFFER_SIZE = 4096;

        /// <summary>The command to be parsed by the pipe</summary>
        public static string CommandToParse = "";
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize the pipe handler to prepare for input
        /// </summary>
        /// <param name="Map">The map to interface with</param>
        public static void Initialize(MacomberMap Map)
        {
            MM_Pipe.Map = Map;
            PIPE_NAME = "\\\\.\\pipe\\MacomberMap-" + Environment.UserDomainName + "-" + Environment.UserName;
            ThreadPool.QueueUserWorkItem(new WaitCallback(PipeHandler));
        }

        /// <summary>
        /// Attempt to establish a pipe and send a command.
        /// </summary>
        /// <param name="Command"></param>
        /// <returns></returns>
        public static bool SendCommand(String Command)
        {
            PIPE_NAME = "\\\\.\\pipe\\MacomberMap-" + Environment.UserDomainName + "-" + Environment.UserName;
            SafeFileHandle pipeHandle = CreateFile(PIPE_NAME, GENERIC_READ | GENERIC_WRITE, 0, IntPtr.Zero, OPEN_EXISTING, FILE_FLAG_OVERLAPPED, IntPtr.Zero);
            if (pipeHandle.IsInvalid)
                return false;
            else
                try
                {
                    using (FileStream fStream = new FileStream(pipeHandle, FileAccess.ReadWrite, BUFFER_SIZE, true))
                    using (StreamWriter sW = new StreamWriter(fStream))
                        foreach (String str in Command.Split(new string[] { "\\n" }, StringSplitOptions.RemoveEmptyEntries))
                            sW.WriteLine(str);
                    return true;
                }
                catch (Exception )
                {
                    return false;
                }
        }            
    
        
        /// <summary>
        /// This thread processes the pipe
        /// </summary>
        /// <param name="state"></param>
        private static void PipeHandler(object state)
        {
            //First, process any information, if it's passed via command line.
            if (!String.IsNullOrEmpty(CommandToParse))
            {
                String[] InCmd = CommandToParse.Split(new string[] { "\\n" }, StringSplitOptions.RemoveEmptyEntries);
                CommandToParse=null;
                ProcessCommand(InCmd);
            }

            SafeFileHandle clientPipeHandle;
            while (true)
            {
                //Try and create the pipe
                clientPipeHandle = CreateNamedPipe(PIPE_NAME, DUPLEX | FILE_FLAG_OVERLAPPED, 0, 255, BUFFER_SIZE, BUFFER_SIZE, 0, IntPtr.Zero);

                //failed to create named pipe
                if (clientPipeHandle.IsInvalid)
                    break;

                //Indicate the success of the process
                if (ConnectNamedPipe(clientPipeHandle, IntPtr.Zero) != 1)
                    break;
                
                
                //Open our stream and read everything in
                try
                {
                    List<String>InLine= new List<string>();                    
                    using (FileStream fStream = new FileStream(clientPipeHandle, FileAccess.ReadWrite, BUFFER_SIZE, true))
                    using (StreamReader sRd = new StreamReader(fStream))
                    while (!sRd.EndOfStream)
                        InLine.Add(sRd.ReadLine());
            
                    //Now, process accordingly. Command: [Zoom/Property/OneLine] [Element Type] [Name or TEID]
                    ProcessCommand(InLine.ToArray());                               
                }
                catch (Exception ex)
                {
                    Program.MessageBox( "Error receiving out-of-process command: " + ex.Message + "\n\n" + ex.StackTrace, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        /// <summary>
        /// Process the command
        /// </summary>
        /// <param name="InLine"></param>
        private static void ProcessCommand(String[] InLine)
        {
            MM_Element Element = null;
            if (InLine.Length == 3)
                if (InLine[1].Equals("Substation", StringComparison.CurrentCultureIgnoreCase))
                {
                    MM_Substation FoundSub;
                    if (MM_Repository.Substations.TryGetValue(InLine[2], out FoundSub))
                        Element = FoundSub;
                    else
                        foreach (MM_Substation Sub in MM_Repository.Substations.Values)
                            if (Sub.LongName.Equals(InLine[2], StringComparison.CurrentCultureIgnoreCase))
                            {
                                Element = Sub;
                                break;
                            }
                }
                else if (InLine[1].Equals("Line", StringComparison.CurrentCultureIgnoreCase))
                    Element = MM_Repository.Lines[InLine[2]];
                else if (InLine[1].Equals("Contingency", StringComparison.CurrentCultureIgnoreCase))
                    Element = MM_Repository.Contingencies[InLine[2]];
                else
                    Element = MM_Repository.LocateElement(InLine[2].Split('.')[0], InLine[2].Split('.')[1], InLine[1], "");
            else if (InLine.Length == 2)
            {
                Int32 TryNum;
                if (!Int32.TryParse(InLine[1], out TryNum) || !MM_Repository.TEIDs.TryGetValue(TryNum, out Element))
                    return;
            }


            if (InLine[0].Equals("Search", StringComparison.CurrentCultureIgnoreCase))
                Form_Builder.SearchDisplay(Map.ctlNetworkMap, InLine[1], false, false);
            else if (InLine[0].Equals("Message", StringComparison.CurrentCultureIgnoreCase))
                Program.MessageBox( "Message received: " + String.Join(" ", InLine, 1, InLine.Length - 1), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (InLine[0].Equals("Zoom", StringComparison.CurrentCultureIgnoreCase))
            {
                if (Element is MM_AlarmViolation)
                    Element = (Element as MM_AlarmViolation).ViolatedElement;
                if (Element is MM_Substation)
                {
                    Map.ctlMiniMap.ZoomPanToPoint((Element as MM_Substation).LatLong);
                    Map.ctlMiniMap.Coord.UpdateZoom(MM_Repository.OverallDisplay.StationZoomLevel);
                }
                else if (Element is MM_Line)
                {
                    Map.ctlMiniMap.ZoomPanToPoint(((Element as MM_Line).Midpoint));
                    Map.ctlMiniMap.Coord.UpdateZoom(MM_Repository.OverallDisplay.StationZoomLevel);
                }
                else if (Element.Substation != null)
                {
                    Map.ctlMiniMap.ZoomPanToPoint(Element.Substation.LatLong);
                    Map.ctlMiniMap.Coord.UpdateZoom(MM_Repository.OverallDisplay.StationZoomLevel);
                }
            }
            else if (InLine[0].Equals("Property", StringComparison.CurrentCultureIgnoreCase))
                Form_Builder.PropertyPage(Element, Map.ctlNetworkMap);
            else if (InLine[0].Equals("OneLine", StringComparison.CurrentCultureIgnoreCase))
                Form_Builder.OneLine_Display(Element, Map.ctlNetworkMap);
        }

        #endregion
    }
}
    