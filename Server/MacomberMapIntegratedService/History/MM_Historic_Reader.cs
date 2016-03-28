using MacomberMapCommunications.Messages;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Xml;
using MacomberMapIntegratedService.Service;
using MacomberMapIntegratedService.Properties;

namespace MacomberMapIntegratedService.History
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides the static EMS reader application
    /// </summary>
    public static class MM_Historic_Reader
    {
        #region Variable declarations
        /// <summary>Report the start time of our server</summary>
        public static DateTime StartTime;

        /// <summary>Our connection to our History server</summary>
        public static MM_History_Server HistoryServer;

        /// <summary>Our collection of query results</summary>
        public static Dictionary<Guid, MM_Historic_Query> Queries = new Dictionary<Guid, MM_Historic_Query>();

        /// <summary>Our manual reset event for when a query is ready</summary>
        public static ManualResetEvent mreQueryReady = new ManualResetEvent(false);
        #endregion

        /// <summary>
        /// Start the Macomber Map History Interface server
        /// </summary>
        public static void StartServer()
        {
            MM_Notification.Notify("History: Macomber Map History Reader starting", "The Macomber Map Service is starting, ConsoleMode=" + Environment.UserInteractive.ToString());
            ThreadPool.QueueUserWorkItem(new WaitCallback(StartHistoryListener));
        }
         
        /// <summary>
        /// Start our History listener
        /// </summary>
        /// <param name="state"></param>
        private static void StartHistoryListener(object state)
        {
            //Load our XML configuration, and start up our History server
            try
            {
                HistoryServer = MM_History_Server.FindServer(Settings.Default.HistoryServer);
                HistoryServer.Open();
                MM_Notification.WriteLine(ConsoleColor.Green, "History: Connected to History Server {0}", HistoryServer.Name);
            }
            catch (Exception ex)
            {
                MM_Notification.WriteLine(ConsoleColor.Red, "History: Unable to connect to History server: {0}", ex.ToString());
            }

            //Now, create our Historype and handle appropriately
            StartTime = DateTime.Now;
            while (true)
            {
                mreQueryReady.Reset();

                //Check whether there are any queries worth running and pruning
                int RanQueries = 0, PrunedQueries = 0;
                foreach (MM_Historic_Query Query in Queries.Values.ToArray())
                    if (Query.State == MM_Historic_Query.enumQueryState.Queued)
                    {
                        Query.State = MM_Historic_Query.enumQueryState.InProgress;
                        RanQueries++;
                        ThreadPool.QueueUserWorkItem(new WaitCallback(RunQuery), Query);
                    }
                    else if (Query.State == MM_Historic_Query.enumQueryState.Retrieved)
                    {
                        PrunedQueries++;
                        Queries.Remove(Query.Id);
                    }

                MM_Notification.WriteLine(ConsoleColor.Green, "History: Ran {0} queries, removed {1} queries.", RanQueries, PrunedQueries);

                mreQueryReady.WaitOne();
            }
        }

        /// <summary>
        /// Run a History query in its own thread
        /// </summary>
        /// <param name="state"></param>
        private static void RunQuery(object state)
        {
            MM_Historic_Query Query = (MM_Historic_Query)state;

            Query.UpdateState(MM_Historic_Query.enumQueryState.InProgress);

            //If our timing information is bad, don't query.
            if (Query.StartTime == default(DateTime) || Query.EndTime == default(DateTime))
            {
                Query.UpdateState(MM_Historic_Query.enumQueryState.HadError);
                return;
            }
            //Start by retrieving all points that match our values
            try
            {
                DataTable OutTable = new DataTable(Query.Id.ToString());
                MM_Historic_DataPoint[] InPoints = HistoryServer.GetPoints("Tag='" + Query.QueryText + "'");

                //Start by building our table
                OutTable.PrimaryKey = new DataColumn[] { OutTable.Columns.Add("Date", typeof(DateTime)) };
                foreach (MM_Historic_DataPoint pt in InPoints)
                    OutTable.Columns.Add(pt.Name, typeof(double)).DefaultValue = DBNull.Value;


                //Now go through each point, and retrive the data            
                foreach (MM_Historic_DataPoint pt in InPoints)
                    foreach (MM_Historic_DataValue Val in pt.RetrieveValues(Query.StartTime, Query.EndTime, Query.NumPoints))
                    {
                        DataRow FoundRow;
                        if ((FoundRow = OutTable.Rows.Find(Val.TimeStamp)) == null)
                            FoundRow = OutTable.Rows.Add(Val.TimeStamp);
                        else
                            FoundRow[pt.Name] = Convert.ToDouble(Val.Value);
                    }

                //Now, update the query state to indicate completion
                Query.SerializeTable(OutTable);
                Query.UpdateState(MM_Historic_Query.enumQueryState.Completed);
            }
            catch (Exception ex)
            {
                DataTable OutTable = new DataTable("Error");
                OutTable.Columns.Add("Error", typeof(String));
                OutTable.Rows.Add(ex.ToString());
                Query.SerializeTable(OutTable);
                Query.UpdateState(MM_Historic_Query.enumQueryState.HadError);
            }
        }
    }
}