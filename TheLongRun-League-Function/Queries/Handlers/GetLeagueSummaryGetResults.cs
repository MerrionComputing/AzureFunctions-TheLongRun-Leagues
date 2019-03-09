using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TheLongRun.Common.Attributes;
using System.Net;
using System.Net.Http;
using TheLongRun.Common;
using Leagues.League.queryDefinition;
using TheLongRun.Common.Events.Query.Projections;
using TheLongRun.Common.Bindings;

namespace TheLongRunLeaguesFunction.Queries.Handlers
{
    public static partial class GetLeagueSummaryQuery
    {
        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [AggregateRoot("League")]
        [QueryName("Get League Summary")]
        [FunctionName("GetLeagueSummaryGetResults")]
        public static async Task<HttpResponseMessage> GetLeagueSummaryGetResultsRun(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequestMessage req,
            ILogger log)
        {

            #region Logging
            if (null != log)
            {
                log.LogDebug("Function triggered HTTP in GetLeagueSummaryQueryProjectionsRequest");
            }
            #endregion

            // Get the query identifier
            string queryId = req.GetQueryNameValuePairsExt()[@"QueryId"];

            Get_League_Summary_Definition_Return value = null;
            string results = $"No results for {queryId}";

            if (queryId == null)
            {
                // Get request body
                dynamic data = await req.Content.ReadAsAsync<object>();
                queryId = data?.QueryId;
            }

            value = await GetLeagueSummaryGetResults("get-league-summary",
               queryId,
               log);

            if (null != value)
            {
                results = $"Results from {queryId} - {value.LeagueName} incorporated {value.Date_Incorporated} at {value.Location} ({value.Twitter_Handle})";
            }
     
            return queryId == null
                    ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a queryId on the query string or in the request body")
                    : req.CreateResponse(HttpStatusCode.OK, results );
        }


        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [AggregateRoot("League")]
        [QueryName("Get League Summary")]
        [FunctionName("GetLeagueSummaryGetResultsActivity")]
        public static async Task<Get_League_Summary_Definition_Return> GetLeagueSummaryGetResultsActivity(
            [ActivityTrigger] DurableActivityContext context,
            ILogger log
            )
        {

            QueryRequest<Get_League_Summary_Definition> queryRequest = context.GetInput<QueryRequest<Get_League_Summary_Definition>>();

            if (null != log)
            {
                log.LogInformation($"GetLeagueSummaryQueryProjectionRequestActivity called for query : {queryRequest.QueryUniqueIdentifier}");
            }

            return await GetLeagueSummaryGetResults(queryRequest.QueryName,
                queryRequest.QueryUniqueIdentifier.ToString(),
                log);

        }

        /// <summary>
        /// Get the league summary results as they currently are in this query
        /// </summary>
        /// <returns></returns>
        private static async Task<Get_League_Summary_Definition_Return >  GetLeagueSummaryGetResults(string queryName,
            string queryId,
            ILogger log)
        {

            Guid queryGuid;

            if (Guid.TryParse(queryId, out queryGuid))
            {
                // Get the current state of the query...
                Projection getQueryState = new Projection(@"Query",
                    queryName,
                    queryGuid.ToString(),
                    nameof(Query_Summary_Projection));

                if (null != getQueryState)
                {

                    #region Logging
                    if (null != log)
                    {
                        log.LogDebug($"Projection processor created in OutputResultsGetLeagueSummaryQuery");
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
                            log.LogDebug($"Query { qryProjection.QueryName } projection run for {queryGuid } in OutputResultsGetLeagueSummaryQuery");
                        }
                        #endregion

                        // Ignore queries in an invalid state or not yet validated...
                        if (qryProjection.CurrentState == Query_Summary_Projection.QueryState.Invalid)
                        {
                            // No need to run projections on an invalid query
                            #region Logging
                            if (null != log)
                            {
                                log.LogWarning($"Query {queryGuid} state is {qryProjection.CurrentState} so no output processed in OutputResultsGetLeagueSummaryQuery");
                            }
                            #endregion
                            return null;
                        }

                        // Check all the projections have been run..
                        Query_Projections_Projection qryProjectionState = new Query_Projections_Projection(log);
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
                                        dynamic  projectionResult = (qryProjectionState.ProcessedRequests[0].ReturnedValue);
                                        if (null != projectionResult )
                                        {
                                            ret.Location = projectionResult.Location;
                                            ret.Date_Incorporated = projectionResult.Date_Incorporated;
                                            ret.Twitter_Handle = projectionResult.Twitter_Handle;
                                            return ret;
                                        }
                                        else
                                        {
                                            #region Logging
                                            if (null != log)
                                            {
                                                log.LogError($"Unable to convert {qryProjectionState.ProcessedRequests[0].ReturnedValue} to {nameof(Leagues.League.projection.League_Summary_Information)} in OutputResultsGetLeagueSummaryQuery");
                                            }
                                            #endregion
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

            return null;
        }
    }
}
