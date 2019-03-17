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
using TheLongRun.Common.Orchestration;
using Newtonsoft.Json;

namespace TheLongRunLeaguesFunction.Queries
{

    public static partial class GetLeagueSummaryQuery
    {
        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [AggregateRoot("League")]
        [QueryName("Get League Summary")]
        [FunctionName("GetLeagueSummaryOutputResults")]
        public static async Task<HttpResponseMessage> GetLeagueSummaryOutputResultsRun(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequestMessage req, 
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

            await OutputResultsGetLeagueSummaryQuery("get-league-summary",
                queryId, 
                log);

            return queryId == null
                ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a queryId on the query string or in the request body")
                : req.CreateResponse(HttpStatusCode.OK, $"Sent output for query {queryId}");

        }

        /// <summary>
        /// Run the projections that are needed to be run to answer this query
        /// </summary>
        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [AggregateRoot("League")]
        [QueryName("Get League Summary")]
        [FunctionName("GetLeagueSummaryOutputResultsActivity")]
        public static async Task<ActivityResponse> GetLeagueSummaryOutputResultsActivity(
            [ActivityTrigger] DurableActivityContext context,
            ILogger log)
        {

            ActivityResponse ret = new ActivityResponse() { FunctionName = "GetLeagueSummaryOutputResultsActivity" };

            try
            {
                QueryRequest<Get_League_Summary_Definition> queryRequest = context.GetInput<QueryRequest<Get_League_Summary_Definition>>();
                if (null != log)
                {
                    log.LogInformation($"GetLeagueSummaryOutputResultsActivity called for query : {queryRequest.QueryUniqueIdentifier}");
                }

                await OutputResultsGetLeagueSummaryQuery(queryRequest.QueryName ,
                    queryRequest.QueryUniqueIdentifier.ToString(), 
                    log);

                ret.Message = $"Output Results for : {queryRequest.QueryUniqueIdentifier  }  ";
            }
            catch (Exception ex)
            {
                if (null != log)
                {
                    // Unable to get the request details from the orchestration
                    log.LogError($"GetLeagueSummaryOutputResultsActivity : error {ex.Message} ");
                }
                ret.Message = ex.Message;
                ret.FatalError = true;
            }

            return ret;
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
        private static async Task<ActivityResponse> OutputResultsGetLeagueSummaryQuery(
            string queryName,
            string queryId,
            ILogger log)
        {

            ActivityResponse ret = new ActivityResponse() { FunctionName = "OutputResultsGetLeagueSummaryQuery" };

            Guid queryGuid;

            if (Guid.TryParse(queryId, out queryGuid))
            {
                // Get the current state of the query...
                Projection getQueryState = new Projection(Constants.Domain_Query,
                    queryName,
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

                            ret.Message = $"Query {queryGuid} state is {qryProjection.CurrentState} so no output processed in OutputResultsGetLeagueSummaryQuery";
                            ret.FatalError = true;

                            return ret;
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
                                    Get_League_Summary_Definition_Return projectionReturn = new Get_League_Summary_Definition_Return(queryGuid,
                                        qryProjectionState.ProcessedRequests[0].AggregateInstanceKey);


                                    if (qryProjectionState.ProcessedRequests[0].ProjectionTypeName == typeof(Leagues.League.projection.League_Summary_Information).Name)
                                    {
                                        dynamic projectionResult = (qryProjectionState.ProcessedRequests[0].ReturnedValue);
                                        if (null != projectionResult)
                                        {
                                            projectionReturn.Location = projectionResult.Location;
                                            projectionReturn.Date_Incorporated = projectionResult.Date_Incorporated;
                                            projectionReturn.Twitter_Handle = projectionResult.Twitter_Handle; 
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


                                    // Call the outputs processing sub-orchestration
                                    

                                    // Get all the output targets
                                    Query_Outputs_Projection qryOutputs = new Query_Outputs_Projection(log);
                                    await getQueryState.Process(qryOutputs);

                                    if ((qryOutputs.CurrentSequenceNumber > 0) || (qryOutputs.ProjectionValuesChanged()))
                                    {
                                        #region Logging
                                        if (null != log)
                                        {
                                            log.LogDebug($"Sending results to output targets in OutputResultsGetLeagueSummaryQuery");
                                        }
                                        #endregion


                                        foreach (string location in qryOutputs.WebhookTargets )
                                        {
                                            #region Logging
                                            if (null != log)
                                            {
                                                log.LogDebug($"Target : { location} - being sent by webhook in OutputResultsGetLeagueSummaryQuery");
                                            }
                                            #endregion

                                            if (null != projectionReturn)
                                            {
                                                var payloadAsJSON = new StringContent(JsonConvert.SerializeObject(projectionReturn));
                                                using (var client = new HttpClient())
                                                {
                                                    var response = await client.PostAsync(location, payloadAsJSON);
                                                    if (! response.IsSuccessStatusCode )
                                                    {
                                                        ret.Message = $"Failed to send output to {location} webhook - {response.StatusCode} : {response.ReasonPhrase} ";
                                                        #region Logging
                                                        if (null != log)
                                                        {
                                                            log.LogError($"{ret.FunctionName } : {ret.Message}" );
                                                        }
                                                        #endregion
                                                    }
                                                }
                                            }
                                        }

                                        foreach (string location in qryOutputs.EventGridTargets)
                                        {
                                            #region Logging
                                            if (null != log)
                                            {
                                                log.LogDebug($"Target : { location} - being sent by event grid message in OutputResultsGetLeagueSummaryQuery");
                                            }
                                            #endregion

                                        }

                                        foreach (string location in qryOutputs.BlobTargets)
                                        {
                                            #region Logging
                                            if (null != log)
                                            {
                                                log.LogDebug($"Target : { location} - being persisted as a blob in OutputResultsGetLeagueSummaryQuery");
                                            }
                                            #endregion

                                        }

                                        foreach (string location in qryOutputs.DurableFunctionOrchestrationTargets)
                                        {
                                            #region Logging
                                            if (null != log)
                                            {
                                                log.LogDebug($"Target : { location} - being used to trigger a durable function to wake up in OutputResultsGetLeagueSummaryQuery");
                                            }
                                            #endregion

                                        }

                                        ret.Message = $"Sent results to output targets ({qryOutputs.Targets.Keys.Count}) in OutputResultsGetLeagueSummaryQuery";
                                        return ret;
                                    }
                                    else
                                    {
                                        // No outputs set
                                        #region Logging
                                        if (null != log)
                                        {
                                            log.LogWarning($"No output targets found in OutputResultsGetLeagueSummaryQuery");
                                        }
                                        #endregion
                                        ret.Message = $"No output targets found in OutputResultsGetLeagueSummaryQuery";
                                        return ret;
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
                                    ret.Message = $"Query {queryGuid} state is has no processed projections so no output processed in OutputResultsGetLeagueSummaryQuery";
                                    return ret;
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
                                ret.Message = $"Query {queryGuid} still has unprocessed projections so no output processed in OutputResultsGetLeagueSummaryQuery";
                                return ret;
                            }
                        }
                        else
                        {
                            ret.Message = $"Projection run but has no results";
                            ret.FatalError = true;
                            return ret;
                        }
                    }
                }
            }


            ret.Message = $"Query identifier blank or not set when running OutputResultsGetLeagueSummaryQuery";
            ret.FatalError = true;
            return ret;


        }
    }
}
