using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLongRun.Common.Orchestration
{
    public interface IQueryRunner
    {

        /// <summary>
        /// Runs the specified query
        /// </summary>
        /// <param name="queryName">
        /// The domain qualified name of the query to run
        /// </param>
        /// <param name="instanceId">
        /// The global identifier of the query instance if we are re-using an existing one (to get an up-to-date value for instance)
        /// </param>
        /// <param name="queryParameters">
        /// 
        /// </param>
        /// <param name="asOfDate">
        /// The date up until which the query should run
        /// </param>
        /// <remarks>
        /// If the as-of-date is not supplied, the query is run to the latest date
        /// </remarks>
        Task<IQueryResponse> RunQueryAsync(string queryName, 
            string instanceId,
            JObject queryParameters,
            DateTime? asOfDate = null
            );

    }

    /// <summary>
    /// A response from running a query
    /// </summary>
    public interface IQueryResponse
    {

    }
}
