using MacomberMapClient.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace MacomberMapClient.User_Interfaces.Communications
{
    /// <summary>
    /// This class holds the communication links between the EMS systems
    /// </summary>
    public class MM_Communication_Linkage : MM_Serializable
    {
        #region Variable declarations
        /// <summary>The name of the linkage</summary>
        public String Name;

        /// <summary>The collection of possible connection states</summary>
        public enum ConnectionStateEnum
        {
            /// <summary>Unknown state</summary>
            Unknown,
            /// <summary>Bad state</summary>
            Bad,
            /// <summary>Good state</summary>
            Good
        };

        /// <summary>Our good value</summary>
        public bool Good = false;

        /// <summary>Our SDIS value</summary>
        public bool SDIS = false;


        /// <summary>The connection state of the linkage</summary>
        public ConnectionStateEnum ConnectionState;

        /// <summary>The group of the ICCP link</summary>
        public string Group;

        /// <summary>The name for the comm link from SCADA</summary>
        public string SCADAName;

        /// <summary>The associated value with the link</summary>
        public ListViewItem CommStatus;

        /// <summary>Whether this link is considered a critical one.</summary>
        public bool Critical = false;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new communications linkage
        /// </summary>
        /// <param name="Name">The name of the communication linkage</param>
        /// <param name="Group">The group to which the comm link is assigned</param>
        /// <param name="Good">Whether the communications linkage is good or not</param>
        /// <param name="SDIS">Whether the communications linkage is active</param>
        public MM_Communication_Linkage(String Name, String Group, bool Good, bool SDIS)
        {
            this.SCADAName = Name;
            String City = "";
            String QSETDSP = "";


            //Determine if we're looking at a QSE or TDSP
            if (Name.Contains("QSE"))
            {
                QSETDSP = "QSE";
                Name = Name.Replace("QSE", "");
            }
            else if (Name.Contains("TSP"))
            {
                QSETDSP = "TDSP";
                Name = Name.Replace("TSP", "");
            }


            //Determine if we're looking at Taylor or Austin
            if (Name.Contains("TAYLOR"))
            {
                City = "Taylor";
                Name = Name.Replace("TAYLOR", "");
            }
            else if (Name.Contains("AUSTIN"))
            {
                City = "Austin";
                Name = Name.Replace("AUSTIN", "");
            }

            //Now, determine if we're a critical measurement
            if (Name.Contains("CRITICAL"))
            {
                this.Critical = true;
                Name = Name.Replace("CRITICAL", "");
            }

            //Now remove all underlines
            Name = Name.Replace("_", "");

            //Determine if we have an ISD or overall ICCPLink point
            if (String.IsNullOrEmpty(City))
                City = "ISD";
            else if (Name == "ICCPLINK")
                Name = "Overall";
            else if (Name == "ISDLINK")
            {
                Name = "Overall";
                City = "ISD";
            }

            //Now, put it together

            this.Name = Name;
            this.Group = City + " - " + QSETDSP;
            SetStatus(Good, SDIS);
        }

        /// <summary>
        /// Initialize a new communications linkage
        /// </summary>
        /// <param name="ElementSource">The XML element source</param>
        /// <param name="AddIfNew">Whether to add any new elements that may be created</param>
        public MM_Communication_Linkage(XmlElement ElementSource, bool AddIfNew)
            : base(ElementSource, AddIfNew)
        {
            if (String.IsNullOrEmpty(SCADAName))
                SCADAName = Name;
        }

        /// <summary>
        /// Set the status of our ICCP link
        /// </summary>
        /// <param name="Good">Whether the link is good</param>
        /// <param name="SDIS">Whether the link is active</param>
        public void SetStatus(bool Good, bool SDIS)
        {
            if (!Good)
                this.ConnectionState = ConnectionStateEnum.Unknown;
            else if (!SDIS)
                this.ConnectionState = ConnectionStateEnum.Bad;
            else
                this.ConnectionState = ConnectionStateEnum.Good;
        }


        #endregion

        /// <summary>
        /// Update the status of our communications item
        /// </summary>
        public void UpdateStatus(ref int EMSStatus)
        {
            if (this.ConnectionState == ConnectionStateEnum.Good)
            {
                CommStatus.BackColor = MM_Repository.OverallDisplay.BackgroundColor;
                CommStatus.ForeColor = MM_Repository.OverallDisplay.ForegroundColor;
            }
            else if (this.ConnectionState == ConnectionStateEnum.Bad)
            {
                CommStatus.BackColor = MM_Repository.OverallDisplay.ErrorColor;
                CommStatus.ForeColor = MM_Repository.OverallDisplay.ErrorBackground;
                EMSStatus = Math.Max(2, EMSStatus);
            }
            else
            {
                CommStatus.BackColor = MM_Repository.OverallDisplay.WarningColor;
                CommStatus.ForeColor = MM_Repository.OverallDisplay.WarningBackground;
                EMSStatus = Math.Max(1, EMSStatus);
            }
        }
    }
}