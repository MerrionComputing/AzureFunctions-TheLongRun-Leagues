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
using Microsoft.Extensions.Logging;

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
            ILogger log)
        {

            #region Logging
            if (null != log)
            {
                log.LogDebug("Function triggered HTTP in GetLeagueSummaryOutputResults");
            }
            #endregion

            // Get the query identifier
            string queryId = req.GetQueryNameValuePairsExt()[@"QueryId"];

            if (queryId == null)
            {
                // Get request body
                dynamic data = await req.Content.ReadAsAsync<object>();
                queryId = data?.QueryId;
            }

            await OutputResultsGetLeagueSummaryQuery(queryId, log);

            return queryId == null
                ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a queryId on the query string or in the request body")
                : req.CreateResponse(HttpStatusCode.OK, $"Validated query {queryId}");

        }

        /// <summary>
        /// Run the projections that are needed to be run to answer this query
        /// </summary>
        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [AggregateRoot("League")]
        [QueryName("Get League Summary")]
        [FunctionName("GetLeagueSummaryOutputResultsActivity")]
        public static async Task GetLeagueSummaryOutputResultsActivity([ActivityTrigger] QueryRequest<Get_League_Summary_Definition> queryRequest,
            ILogger log)
        {

            if (null != log)
            {
                log.LogInformation($"GetLeagueSummaryOutputResultsActivity called for query : {queryRequest.QueryUniqueIdentifier}");
            }

            await OutputResultsGetLeagueSummaryQuery(queryRequest.QueryUniqueIdentifier.ToString(), log);
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
        private static async Task OutputResultsGetLeagueSummaryQuery(string queryId,
            ILogger log = null)
        {

            const string QUERY_NAME = @"get-league-summary";
            Guid queryGuid;

            if (Guid.TryParse(queryId, out queryGuid))
            {
                // Get the current state of the query...
                Projection getQueryState = new Projection(@"Query",
                    QUERY_NAME,
                    queryGuid.ToString(),
                    nameof(Query_Summary_Projection));

                if (null != getQueryState)
                {

                    #region Logging
                    if (null != log)
                    {
                        log.LogDebug ($"Projection processor created in OutputResultsGetLeagueSummaryQuery");
                    }
                    #endregion

                    // Run the query summary projection
                    Query_Summary_Projection qryProjection =
                            new Query_Summary_Projection(log);

                    await getQueryState.Process(qryProjection);

                    if ((qryProjection.CurrentSequenceNumber > 0) || (qryProjection.ProjectionValuesChanged()))
                    {
                        // Process the query state as is now...
                        #region Logging
                        if (null != log)
                        {
                            log.LogDebug ($"Query { qryProjection.QueryName } projection run for {queryGuid } in OutputResultsGetLeagueSummaryQuery");
                        }
                        #endregion

                        // Ignore queries in an invalid state or not yet validated...
                        if (qryProjection.CurrentState == Query_Summary_Projection.QueryState.Invalid)
                        {
                            // No need to run projections on an invalid query
                            #region Logging
                            if (null != log)
                            {
                                log.LogWarning ($"Query {queryGuid} state is {qryProjection.CurrentState} so no output processed in OutputResultsGetLeagueSummaryQuery");
                            }
                            #endregion
                            return;
                        }

                        // Check all the projections have been run..
                        Query_Projections_Projection qryProjectionState = new Query_Projections_Projection( log );
                        await getQueryState.Process(qryProjectionState);

                        if ((qryProjectionState.CurrentSequenceNumber > 0) || (qryProjectionState.ProjectionValuesChanged()))
                        {

                            if (qryProjectionState.UnprocessedRequests.Count == 0)
                            {
                                if (qryProjectionState.ProcessedRequests.Count > 0)
                                {
                                    // Turn the projections into a query return (This could include a collate step)
                                    Get_League_Summary_Definition_Return ret = new Get_League_Summary_Definition_Return(queryGuid,
                                        qryProjectionState.ProcessedRequests[0].AggregateInstanceKey);

                                    if (qryProjectionState.ProcessedRequests[0].ProjectionTypeName == typeof(Leagues.League.projection.League_Summary_Information).Name)
                                    {
                                        Leagues.League.projection.League_Summary_Information projectionResult = ((Newtonsoft.Json.Linq.JObject)qryProjectionState.ProcessedRequests[0].ReturnedValue).ToObject<Leagues.League.projection.League_Summary_Information>();
                                        if (null != projectionResult)
                                        {
                                            ret.Location = projectionResult.Location;
                                            ret.Date_Incorporated = projectionResult.Date_Incorporated;
                                            ret.Twitter_Handle = projectionResult.Twitter_Handle;
                                        }
                                        else
                                        {
                                            #region Logging
                                            if (null != log)
                                            {
                                                log.LogError ($"Unable to convert {qryProjectionState.ProcessedRequests[0].ReturnedValue} to {nameof(Leagues.League.projection.League_Summary_Information)} in OutputResultsGetLeagueSummaryQuery");
                                            }
                                            #endregion
                                        }
                                    }

                                    // Get all the output targets
                                    Query_Outputs_Projection qryOutputs = new Query_Outputs_Projection(log);
                                    await getQueryState.Process(qryOutputs);

                                    if ((qryOutputs.CurrentSequenceNumber > 0) || (qryOutputs.ProjectionValuesChanged()))
                                    {
                                        #region Logging
                                        if (null != log)
                                        {
                                            log.LogDebug($"Sending results to output targets {ret} in OutputResultsGetLeagueSummaryQuery");
                                        }
                                        #endregion
                                        foreach (string location in qryOutputs.Targets.Keys )
                                        {
                                            #region Logging
                                            if (null != log)
                                            {
                                                log.LogDebug($"Target : { location} - type {qryOutputs.Targets[location]} in OutputResultsGetLeagueSummaryQuery");
                                            }
                                            #endregion
                                            // Send the output to the location...
                                            QueryLogRecord.SendOutput(location, qryOutputs.Targets[location], ret);
                                        }
                                    }
                                }
                                else
                                {
                                    // No processed projections found
                                    #region Logging
                                    if (null != log)
                                    {
                                        log.LogWarning($"Query {queryGuid} state is has no processed projections so no output processed in OutputResultsGetLeagueSummaryQuery");
                                    }
                                    #endregion
                                }
                            }
                            else
                            {
                                #region Logging
                                if (null != log)
                                {
                                    log.LogWarning($"Query {queryGuid} still has unprocessed projections so no output processed in OutputResultsGetLeagueSummaryQuery");
                                }
                                #endregion
                            }
                        }
                    }
                }
            }
        }
    }
}
