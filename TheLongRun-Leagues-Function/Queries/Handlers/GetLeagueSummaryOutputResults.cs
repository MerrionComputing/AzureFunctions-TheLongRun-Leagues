using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

using Leagues.League.queryDefinition;
using TheLongRun.Common;
using TheLongRun.Common.Attributes;
using TheLongRun.Common.Bindings;
using TheLongRun.Common.Events.Query;
using TheLongRun.Common.Events.Query.Projections;
using System;

namespace TheLongRunLeaguesFunction.Queries
{

    public static partial class Query
    {
        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [AggregateRoot("League")]
        [QueryName("Get League Summary")]
        [FunctionName("GetLeagueSummaryOutputResults")]
        public static async Task<HttpResponseMessage> GetLeagueSummaryOutputResultsRun(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage req, 
            TraceWriter log)
        {

            #region Logging
            if (null != log)
            {
                log.Verbose("Function triggered HTTP ",
                    source: "GetLeagueSummaryOutputResults");
            }
            #endregion

            // Get the query identifier
            string queryId = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "QueryId", true) == 0)
                .Value;

            if (queryId == null)
            {
                // Get request body
                dynamic data = await req.Content.ReadAsAsync<object>();
                queryId = data?.QueryId;
            }

            OutputResultsGetLeagueSummaryQuery(queryId, log);

            return queryId == null
                ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a queryId on the query string or in the request body")
                : req.CreateResponse(HttpStatusCode.OK, $"Validated query {queryId}");

        }

        /// <summary>
        /// Send out the results for a completed "Get-League-Summary" query
        /// </summary>
        /// <param name="queryId">
        /// Unique identifier of the query for which we want to send out the results
        /// </param>
        /// <param name="log">
        /// Trace target for logging the outcomes of this operation
        /// </param>
        private static void OutputResultsGetLeagueSummaryQuery(string queryId, 
            TraceWriter log = null)
        {
            throw new NotImplementedException();
        }
    }
}
