using System;
using System.Collections.Generic;
using System.Text;
using Macomber_Map.Data_Elements;
using System.Xml;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace Macomber_Map.Data_Connections.Generic
{
    /// <summary>
    /// This class holds information on data sources
    /// </summary>
    public class MM_Data_Source : MM_Serializable
    {
        #region Variable declarations
        /// <summary>The application driving the data source</summary>
        public string Application;

        /// <summary>The database driving the data source</summary>        
        public string Database;

        /// <summary>The display name for the data source</summary>
        public string Name;       

        /// <summary>Whether the data source offers telemetered data</summary>
        public bool Telemetry;

        /// <summary>Whether the data source offers estimated data</summary>
        public bool Estimates;

        /// <summary>Whether this connector should be kept up in real-time</summary>
        public bool RefreshOnly = false;

        /// <summary>The connector type for the data source</summary>
        public Type ConnectorType;
       
        /// <summary>The background color for the data source</summary>
        public Color BackColor;

        /// <summary>The foreground color for the data source</summary>
        public Color ForeColor;

        /// <summary>This data source is the default one</summary>
        public bool Default;

        /// <summary>This data source is associated with the master/real-time information</summary>
        public bool Master;

        /// <summary>The first host, if specified</summary>
        public String Primary = null;

        /// <summary>The second host, if specified</summary>
        public String Secondary = null;

        /// <summary>The application holding violation information</summary>
        public String ViolationApp = "RTCA";

        /// <summary>Whether the master data source should be used for violations</summary>
        public bool UseMasterForViolations = true;

        /// <summary>The specified port, if any</summary>
        public int Port = 0;

        /// <summary>Whether or not the connection should use NioArc</summary>
        public bool NioArc = false;

        /// <summary>The connection state of our binding object</summary>
        public ConnectionState State = ConnectionState.Connecting;

        /// <summary>Our EMS connector</summary>
        public MM_DataConnector Connector;

        /// <summary>The collection of connection statuses for our source</summary>
        public Dictionary<String, int> ConnectionStatuses = new Dictionary<string, int>();
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a pseudo-data source from a savecase
        /// </summary>
        /// <param name="xElem"></param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Data_Source(XmlElement xElem, bool AddIfNew)
            : base(xElem, AddIfNew)
        { }


        /// <summary>
        /// The data source related to the assigned connector
        /// </summary>
        /// <param name="xConfig">The Xml configuration for the data source</param>
        /// <param name="ConnectorType">The connector type</param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Data_Source(XmlElement xConfig, Type ConnectorType, bool AddIfNew)
            : base(xConfig, AddIfNew)
        {
            this.ConnectorType = ConnectorType;           
        }

        /// <summary>
        /// Initialize a blank (for model usage) data source
        /// </summary>
        public MM_Data_Source()
        {
            this.Estimates = true;
        }

        #endregion


        #region Comparison
        /// <summary>
        /// Determine whether this data source is equivalent to another (same type, APname and Pri/Sec/Port/Nioarc
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool DataSourceEquivalentTo(MM_Data_Source other)
        {
            return ConnectorType.Equals(other.ConnectorType) && Primary.Equals(other.Primary) && Secondary.Equals(other.Secondary) && Port.Equals(other.Port) && NioArc.Equals(other.NioArc);
        }

        /// <summary>
        /// Report an easy to read name
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.ConnectorType.Name;
        }
        #endregion

        #region Encryption/Decryption routines
        /// <summary>
        /// Decrypt the specified encrypted text
        /// </summary>
        /// <param name="EncryptedText">The encrypted text</param>
        /// <returns></returns>
        public static String Decrypt(String EncryptedText)
        {
            TripleDESCryptoServiceProvider TDes = new TripleDESCryptoServiceProvider();
            TDes.IV = new byte[] { 26, 166, 202, 5, 16, 205, 201, 40 };
            TDes.Key = new byte[] { 30, 66, 158, 216, 209, 0, 217, 152, 182, 114, 159, 38, 190, 120, 97, 232, 8, 14, 105, 181, 241, 27, 135, 164 };
            byte[] inBytes = Convert.FromBase64String(EncryptedText);
            return Encoding.UTF8.GetString(TDes.CreateDecryptor().TransformFinalBlock(inBytes, 0, inBytes.Length));

        }

        /// <summary>
        /// Encrypt the specified clear text
        /// </summary>
        /// <param name="ClearText">The decrypted text</param>
        /// <returns></returns>
        public static String Encrypt(String ClearText)
        {
            TripleDESCryptoServiceProvider TDes = new TripleDESCryptoServiceProvider();
            TDes.IV = new byte[] { 26, 166, 202, 5, 16, 205, 201, 40 };
            TDes.Key = new byte[] { 30, 66, 158, 216, 209, 0, 217, 152, 182, 114, 159, 38, 190, 120, 97, 232, 8, 14, 105, 181, 241, 27, 135, 164 };
            byte[] EncryptedBytes = Encoding.UTF8.GetBytes(ClearText);
            return Convert.ToBase64String(TDes.CreateEncryptor().TransformFinalBlock(EncryptedBytes, 0, EncryptedBytes.Length));
        }
        #endregion
    }
}
