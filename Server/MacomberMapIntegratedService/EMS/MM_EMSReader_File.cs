using MacomberMapCommunications.Messages.EMS;
using MacomberMapIntegratedService.Properties;
using MacomberMapIntegratedService.Service;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MacomberMapIntegratedService.EMS
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class holds information on an EMS information
    /// </summary>
    public class MM_EMSReader_File
    {
        #region Static variable declarations
        /// <summary>Our file system watcher</summary>
        private static FileSystemWatcher FileWatcher;

        /// <summary>Our collection of file last write times</summary>
        public static Dictionary<String, MM_EMSReader_File> FileInfo = new Dictionary<string, MM_EMSReader_File>(StringComparer.CurrentCultureIgnoreCase);
        #endregion 

        #region Variable declarations
        /// <summary>When our file was last written</summary>
        public DateTime LastFileWrite;

        /// <summary>The target type that is contained in the file</summary>
        public Type TargetType;

        /// <summary>The update command associated with our file</summary>
        public String UpdateCommand;

        /// <summary>The name of our file</summary>
        public String FileName;

        /// <summary>The thread responsible for execution</summary>
        public Thread RunThread;

        /// <summary>Our reset event for our file reader</summary>
        public ManualResetEvent ResetEvent;

        /// <summary>The latest batch ID</summary>
        public DateTime LatestBatchID;
        #endregion

        #region Initialization
        /// <summary>
        /// Initalize our file information
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="TargetType"></param>
        /// <param name="UpdateCommand"></param>
        public MM_EMSReader_File(String FileName, String UpdateCommand, Type TargetType)
        {
            this.FileName = FileName;
            this.UpdateCommand = UpdateCommand;
            this.TargetType = TargetType;
            RunThread = new Thread(ReadFileAndSendResults);
            RunThread.Name = "Reader for " + Path.GetFileNameWithoutExtension(FileName);
            ResetEvent = new ManualResetEvent(true);
            RunThread.Start();

        }
        #endregion

        #region File read and retrieval
        /// <summary>
        /// Retrieve our file, and create our output
        /// </summary>
        /// <returns></returns>
        public void ReadFileAndSendResults(object state)
        {
            while (true)
                try
                {
                    //First, wait for our trigger. If we timed out, set it, so that our next run works properly.
                    bool TriggerTimeout = !ResetEvent.WaitOne(TimeSpan.FromSeconds(55));
                    //Thread.Sleep(250);

                    //Now, read our file
                    bool Success = false;
                    int RetryCount = 0;
                    FileInfo fI = new FileInfo(FileName);
                    if (fI.Exists && fI.Length > 0 && (fI.LastWriteTime != LastFileWrite || TriggerTimeout))
                    {
                        LastFileWrite = fI.LastWriteTime;
                        do
                        {
                            try
                            {
                                DateTime StartTime = DateTime.Now;
                                using (FileStream fS = WaitForFile(FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                                    if (fS != null)
                                        using (StreamReader sRd = new StreamReader(fS, new UTF8Encoding(false)))
                                            ProcessStreamRead(sRd, FileName, TargetType, ref LatestBatchID);

                                Success = true;
                            }
                            catch (Exception ex)
                            {
                                MM_Notification.WriteLine(ConsoleColor.Red, "Error reading {0}: {1}", FileName, ex.Message);
                                RetryCount++;
                            }
                        } while (!Success && RetryCount < 2);
                        fI = null;
                    }
                    ResetEvent.Reset();
                }
                catch (Exception ex2)
                {
                    MM_Notification.WriteLine(ConsoleColor.Red, "Thread for {0} shutting down: {1}", FileName, ex2);
                }
        }

        /// <summary>
        /// Wait for a stream to become available
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="mode"></param>
        /// <param name="access"></param>
        /// <param name="share"></param>
        /// <returns></returns>
        private FileStream WaitForFile(string fullPath, FileMode mode, FileAccess access, FileShare share)
        {
            FileStream fS = null;
            for (int numTries = 1; numTries < 10; numTries++)
            {
                try
                {
                    fS = File.Open(fullPath, mode, access, share);
                    return fS;
                }
                catch (IOException ex)
                {
                    //Console.WriteLine("Error reading {0}: {1}", fullPath, ex.Message);
                    if (fS != null)
                        fS.Dispose();
                    fS = null;
                    Thread.Sleep(50);
                }
            }
            return null;
        }

        /// <summary>
        /// Process a stream, reading and parsing its content
        /// </summary>
        /// <param name="InLines"></param>
        /// <param name="FileName"></param>
        public static void ProcessStreamRead(StreamReader sRd, String FileName, Type TargetType, ref DateTime LatestBatchID)
        {
            //Now, parse our stream
            IList OutList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(TargetType));

            //Read our header
            String FirstLine = sRd.ReadLine().TrimStart('#').TrimEnd(',');
            String[] HeaderLine;
            Double ProperDate;
            DateTime BatchId = DateTime.Now;
            if (Double.TryParse(FirstLine, out ProperDate))
            {
                BatchId = new DateTime(1970, 1, 1).AddSeconds(ProperDate);
                HeaderLine = sRd.ReadLine().TrimStart('#').Split(',');
            }
            else
                HeaderLine = FirstLine.Split(',');

            if (BatchId < LatestBatchID)
            {
                MM_Notification.WriteLine(ConsoleColor.Red, "Aborting {0} Batch {1} because it's older than {1}", FileName, BatchId, LatestBatchID);
                return;
            }
            else
                LatestBatchID = BatchId;

            PropertyInfo[] HeaderInfo = MM_Serialization.GetHeaderInfo(TargetType, HeaderLine);
            for (int a = 0; a < HeaderLine.Length; a++)
                if (HeaderInfo[a] == null)
                    MM_Notification.WriteLine(ConsoleColor.Yellow, "Unknown variable {0} in {1}", HeaderLine[a], FileName);

            //Confirm all of our headers are present
            foreach (PropertyInfo pI in TargetType.GetProperties())
                if (Array.FindIndex<String>(HeaderLine, T => T.Equals(pI.Name, StringComparison.CurrentCultureIgnoreCase)) == -1)
                    MM_Notification.WriteLine(ConsoleColor.Yellow, "Missing variable {0} ({1}) in {2} / {3}", pI.Name, pI.PropertyType.Name, TargetType.Name, FileName);

            //Now, read in all of our lines of data, using reflection to store our data
            String InLine;
            while ((InLine = sRd.ReadLine()) != null)
                if (InLine.StartsWith("#"))
                {
                    DateTime EndTime = new DateTime(1970, 1, 1).AddSeconds(Convert.ToDouble(InLine.TrimStart('#').TrimEnd(',')));
                    if (EndTime != BatchId)
                        MM_Notification.WriteLine(ConsoleColor.Red, "Mismatch date on {0}: Start {1}, End {2}", FileName, BatchId, EndTime);
                }
                else
                {
                    Object OutObj = MM_Serialization.Deserialize(HeaderInfo, InLine.Split(','), Activator.CreateInstance(TargetType));
                    //if (OutObj is MacomberMapCommunications.Messages.EMS.MM_BreakerSwitch_Data)
                    //{
                    //    MacomberMapCommunications.Messages.EMS.MM_BreakerSwitch_Data bs = (MacomberMapCommunications.Messages.EMS.MM_BreakerSwitch_Data)OutObj;
                    //    if (bs.TEID_CB == 118964)
                    //        MM_Notification.WriteLine(ConsoleColor.Magenta, "   TEID=" + bs.TEID_CB.ToString() + "    status=" + (bs.Open_CB ? "Open" : "Closed"));
                    //}
                    OutList.Add(OutObj);
                }

            //Once our data are done, use the interprocess communication to update our data
            if (TargetType == typeof(MM_EMS_Command))
            {
                MM_Server.EMSCommands.Clear();
                foreach (Object obj in OutList)
                    MM_Server.EMSCommands.Add((MM_EMS_Command)obj);
            }
            else
                MM_EMS_Data_Updater.ProcessUpdate(TargetType, (Array)OutList.GetType().GetMethod("ToArray").Invoke(OutList, null));
            OutList.Clear();
            OutList = null;
        }
        #endregion

        /// <summary>
        /// Trigger an fire due to change
        /// </summary>
        /// <param name="e"></param>
        public void Trigger(FileSystemEventArgs e)
        {
            ResetEvent.Set();
        }

        #region File system watcher handling
        /// <summary>
        /// Initialize our file stream reader
        /// </summary>
        public static void Initialize()
        {
            if (String.IsNullOrEmpty(Settings.Default.TEDESourceFolder))
                return;
            
            //Start up our file system watcher
            FileWatcher = new FileSystemWatcher();
            FileWatcher.Path = Settings.Default.TEDESourceFolder;
            FileWatcher.NotifyFilter = NotifyFilters.LastWrite;
            FileWatcher.Created += FileWatcher_Event;
            FileWatcher.Changed += FileWatcher_Event;
            FileWatcher.Filter = "*.csv";
            
            //First, read all of our files
            foreach (FileInfo fI in new DirectoryInfo(FileWatcher.Path).GetFiles(FileWatcher.Filter))  
                FileWatcher_Event(FileWatcher, new FileSystemEventArgs(WatcherChangeTypes.Created, fI.DirectoryName, fI.Name));

            FileWatcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// This class handles creation/write updates to files.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void FileWatcher_Event(object sender, FileSystemEventArgs e)
        {
            //First, make sure we have a target type.
            MM_EMSReader_File FoundInfo;
            if (FileInfo.TryGetValue(e.Name, out FoundInfo))
                FoundInfo.Trigger(e);
        }
        #endregion
    }
}