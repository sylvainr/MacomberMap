using MacomberMapAdministrationService;
using MacomberMapCommunications.Messages;
using MacomberMapCommunications.Messages.Communications;
using MacomberMapIntegratedService.EMS;
using MacomberMapIntegratedService.Properties;
using MacomberMapIntegratedService.Service;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.OracleClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 618
namespace MacomberMapIntegratedService.Database
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides the static EMS reader application
    /// </summary>
    public static class MM_Database_Connector
    {
        #region Variable declarations
        /// <summary>Our time stamp from our network model</summary>
        public static DateTime LastModelWriteTime = DateTime.MinValue;

        /// <summary>Our connection to the database server</summary>
        public static DbConnection oConn;

        /// <summary>Our collection of database models</summary>
        public static MM_Database_Model[] Models = new MM_Database_Model[0];

        /// <summary>The model file to send to the Macomber Map clients</summary>
        public static byte[] MM_Model_File;

        /// <summary>Our collection of training levels</summary>
        public static MM_Training_Level[] Levels = new MM_Training_Level[0];
        #endregion

        /// <summary>
        /// Start the Macomber Map Database Interface server
        /// </summary>
        public static void StartServer()
        {
            MM_Notification.Notify("Macomber Map Database Connector starting", "The Macomber Map Service is starting, ConsoleMode=" + Environment.UserInteractive.ToString());

            //Open our XML configuration 
            OpenDatabaseConnection();

            //Load in our collection of models and notes, then start our server
            LoadModels();
        }

        /// <summary>
        /// Load our network model flat file
        /// </summary>
        public static void LoadNetworkModelFlatFile()
        {
            if (!File.Exists(Settings.Default.ModelFilePath))
                MM_Database_Connector.MM_Model_File = new byte[0];
            else if (File.GetLastWriteTime(Settings.Default.ModelFilePath) == LastModelWriteTime)
                return;
            else
                using (MemoryStream mS = new MemoryStream())
                using (GZipStream gS = new GZipStream(mS, CompressionMode.Compress))
                using (FileStream fS = new FileStream(Settings.Default.ModelFilePath, FileMode.Open))
                using (BufferedStream bS = new BufferedStream(fS))
                {
                    bS.CopyTo(gS);
                    gS.Flush();
                    gS.Close();
                    mS.Flush();
                    MM_Database_Connector.MM_Model_File = mS.ToArray();
                }
        }

        /// <summary>
        /// Load in our collection of models
        /// </summary>
        private static void LoadModels()
        {
            LoadNetworkModelFlatFile();

            if (oConn == null || oConn.State != System.Data.ConnectionState.Open)
                return;
            List<MM_Database_Model> Models = new List<MM_Database_Model>();
            using (DbCommand oCmd = CreateCommand(MM_Serialization.BuildSQLQuery<MM_Database_Model>(), oConn))
            using (DbDataReader oRd = oCmd.ExecuteReader())
            {
                PropertyInfo[] HeaderInfo = MM_Serialization.GetHeaderInfo<MM_Database_Model>(oRd);
                while (oRd.Read())
                    Models.Add(MM_Serialization.Deserialize<MM_Database_Model>(HeaderInfo, oRd));
            }
            MM_Database_Connector.Models = Models.ToArray();

            List<MM_Training_Level> Levels = new List<MM_Training_Level>();
            using (DbCommand oCmd = CreateCommand(MM_Serialization.BuildSQLQuery<MM_Training_Level>(), oConn))
            using (DbDataReader oRd = oCmd.ExecuteReader())
            {
                PropertyInfo[] HeaderInfo = MM_Serialization.GetHeaderInfo<MM_Training_Level>(oRd);
                while (oRd.Read())
                    Levels.Add(MM_Serialization.Deserialize<MM_Training_Level>(HeaderInfo, oRd));
            }
            MM_Database_Connector.Levels = Levels.ToArray();
        }
        


        /// <summary>
        /// Open our database connection
        /// </summary>
        private static void OpenDatabaseConnection()
        {
            String ConnectionParameter = Settings.Default.DatabaseConnectionString;
            if (!String.IsNullOrEmpty(Settings.Default.DatabaseEncryptedPassword))
                ConnectionParameter = ConnectionParameter.Replace("[Password]", MM_Encryption.Decrypt(Settings.Default.DatabaseEncryptedPassword));
            Assembly DatabaseAssembly = Assembly.Load(Settings.Default.DatabaseAssembly);
            oConn = (DbConnection)Activator.CreateInstance(DatabaseAssembly.GetType(Settings.Default.DatabaseConnectionType));
            oConn.ConnectionString = Settings.Default.DatabaseConnectionString;
            try
            {
                oConn.Open();
                MM_Notification.WriteLine(ConsoleColor.Green, "Database: Connected to {0} ({1}).", oConn.DataSource, oConn.ServerVersion);
            }
            catch(Exception ex)
            {
                MM_Notification.WriteLine(ConsoleColor.Red, "Database: Error connecting to {0}: {1}", oConn.DataSource, ex);
            }
        }



        /// <summary>
        /// Log a database command
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="User"></param>
        /// <param name="TransferMode"></param>
        public static void LogCommand(string Command, MM_User User, string TransferMode)
        {
            if (oConn != null && oConn.State == System.Data.ConnectionState.Open)
                using (DbCommand oCmd = CreateCommand("INSERT INTO MM_Command_Log (CommandText, UserName, ComputerName, CommandTime, TransferMode, ServerName) VALUES (:CommandText, :UserName, :ComputerName, :CommandTime, :TransferMode, :ServerName)", MM_Database_Connector.oConn))
                {
                    AddParameter(oCmd,"CommandText", Command);
                    AddParameter(oCmd,"UserName", User.UserName);
                    AddParameter(oCmd,"ComputerName", User.MachineName);
                    AddParameter(oCmd,"CommandTime", DateTime.Now);
                    AddParameter(oCmd,"TransferMode", "File");
                    AddParameter(oCmd,"ServerName", MM_Server.ServerURI.Host + ":" + MM_Server.ServerURI.Port.ToString());
                    oCmd.ExecuteNonQuery();
                }
        }

        /// <summary>
        /// Log a coordinate suggestion
        /// </summary>
        /// <param name="Suggestions"></param>
        public static void PostCoordinateSuggestions(MacomberMapCommunications.Messages.Communications.MM_Coordinate_Suggestion[] Suggestions)
        {
            if (oConn != null && oConn.State == System.Data.ConnectionState.Open)
                foreach (MM_Coordinate_Suggestion Suggestion in Suggestions)
                using (DbCommand oCmd = CreateCommand("INSERT INTO MM_Coordinate_Suggestion (ID, TEID, OriginalCoordinates,SuggestedCoordinates,SuggestedCoordinatesXY,SuggestedBy,SuggestedOn,SuggestedFrom,SuggestionType) VALUES (SEQ_MM_COORDINATE_SUGGESTION.NEXTVAL,:TEID, :OriginalCoordinates,:SuggestedCoordinates,:SuggestedCoordinatesXY,:SuggestedBy,:SuggestedOn,:SuggestedFrom,:SuggestionType)", oConn))                
                {
                    AddParameter(oCmd,"TEID", Suggestion.TEID);
                    AddParameter(oCmd,"OriginalCoordinates", GenerateString(Suggestion.OriginalCoordinates));
                    AddParameter(oCmd,"SuggestedCoordinates", GenerateString(Suggestion.SuggestedCoordinates));
                    AddParameter(oCmd,"SuggestedCoordinatesXY", GenerateString(Suggestion.SuggestedCoordinatesXY));
                    AddParameter(oCmd,"SuggestedBy", Suggestion.SuggestedBy);
                    AddParameter(oCmd,"SuggestedOn", Suggestion.SuggestedOn);
                    AddParameter(oCmd,"SuggestedFrom", Suggestion.SuggestedFrom);
                    AddParameter(oCmd,"SuggestionType", Suggestion.SuggestionType.ToString());
                    oCmd.ExecuteNonQuery();
                }
        }

        /// <summary>
        /// Add in our parameter
        /// </summary>
        /// <param name="dCommand"></param>
        /// <param name="Name"></param>
        /// <param name="Value"></param>
        public static DbParameter AddParameter(DbCommand dCommand, String Name, Object Value)
        {
            if (dCommand is OracleCommand)
               return ((OracleCommand)dCommand).Parameters.AddWithValue(Name, Value);
            else
            {
                DbParameter Param = dCommand.CreateParameter();
                Param.ParameterName = Name;
                Param.Value = Value;
                dCommand.Parameters.Add(Param);
                return Param;
            }
        }

        /// <summary>
        /// Create a new SQL command
        /// </summary>
        /// <param name="SQLText"></param>
        /// <param name="dConn"></param>
        /// <returns></returns>
        public static DbCommand CreateCommand(String SQLText, DbConnection dConn)
        {
            DbCommand NewCommand = dConn.CreateCommand();
            NewCommand.CommandText = SQLText;
            return NewCommand;
        }


        /// <summary>
        /// Convert an array to a string
        /// </summary>
        /// <param name="InArray"></param>
        /// <returns></returns>
        private static byte[] GenerateString(int[] InArray)
        {
            byte[] OutBytes = new byte[InArray.Length * 4];
            for (int a = 0; a < InArray.Length; a++)
                Buffer.BlockCopy(BitConverter.GetBytes(InArray[a]), 0, OutBytes, a * 4, 4);
            return OutBytes;
        }

        /// <summary>
        /// Convert an array to a string
        /// </summary>
        /// <param name="InArray"></param>
        /// <returns></returns>
        private static byte[] GenerateString(float[] InArray)
        {
            byte[] OutBytes = new byte[InArray.Length * 4];
            for (int a = 0; a < InArray.Length; a++)
                Buffer.BlockCopy(BitConverter.GetBytes(InArray[a]), 0, OutBytes, a * 4, 4);
            return OutBytes;
        }


    }
}
#pragma warning restore 618
