using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapIntegratedService.History
{
    /// <summary>
    /// This class provides an interface to a historic data server
    /// </summary>
    public class MM_History_Server
    {
        #region Variable declarations
        /// <summary>
        /// The name of our historic server
        /// </summary>
        public String Name;
        #endregion


        /// <summary>
        /// Locate a server with a particular name
        /// </summary>
        /// <param name="ServerName"></param>
        /// <returns></returns>
        public static MM_History_Server FindServer(String ServerName)
        {
            return new MM_History_Server(ServerName);
        }

        /// <summary>
        /// Initialize a new server with a particular name
        /// </summary>
        /// <param name="Name"></param>
        private MM_History_Server(String Name)
        {
            this.Name = Name;
        }

        /// <summary>
        /// Open our connection to our history server
        /// </summary>
        public void Open()
        {

        }

        /// <summary>
        /// Retrieve an array of our points
        /// </summary>
        /// <param name="SQL"></param>
        /// <returns></returns>
        public MM_Historic_DataPoint[] GetPoints(String SQL)
        {
            return new MM_Historic_DataPoint[0];
        }
    }
}