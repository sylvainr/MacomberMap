using MacomberMapCommunications.Messages;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapCommunications.WCF
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides the interface and class to our historic types
    /// </summary>
    [ServiceContract]
    public interface IMM_Historic_Types
    {

        /// <summary>
        /// Initiate a historic systems query, and return a Guid of the request
        /// </summary>
        /// <param name="TagSQL"></param>
        /// <param name="EndTime"></param>
        /// <param name="NumPoints"></param>
        /// <param name="StartTime"></param>
        /// <returns></returns>
        [OperationContract]
        Guid QueryTags(String TagSQL, DateTime StartTime, DateTime EndTime, int NumPoints);

        

        /// <summary>
        /// Check on the status of a historic query
        /// </summary>
        /// <param name="QueryID"></param>
        /// <returns></returns>
        [OperationContract]
        MM_Historic_Query.enumQueryState CheckQueryStatus(Guid QueryID);

        /// <summary>
        /// Retrieve the results of a query
        /// </summary>
        /// <param name="QueryID"></param>
        /// <returns></returns>
        [OperationContract]
        MM_Historic_Query RetrieveQueryResults(Guid QueryID);
            
            
    }
}
