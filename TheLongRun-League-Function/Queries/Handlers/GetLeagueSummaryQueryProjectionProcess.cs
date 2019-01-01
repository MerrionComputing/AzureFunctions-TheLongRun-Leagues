using Leagues.League.queryDefinition;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TheLongRun.Common;
using TheLongRun.Common.Attributes;
using TheLongRun.Common.Bindings;
using TheLongRun.Common.Events.Query.Projections;
using TheLongRun.Common.Orchestration;

namespace TheLongRunLeaguesFunction.Queries
{

    public static partial class GetLeagueSummaryQuery
    {

        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [AggregateRoot("League")]
        [QueryName("Get League Summary")]
        [FunctionName("GetLeagueSummaryQueryProjectionProcess")]
        public static async Task<HttpResponseMessage> GetLeagueSummaryQueryProjectionProcessRun(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequestMessage req,
            ILogger log)
        {
            #region Logging
            if (null != log)
            {
                log.LogDebug("Function triggered HTTP GetLeagueSummaryQueryProjectionProcess");
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

            string message = $"Process projections for query {queryId}";
            ActivityResponse resp = null;

            if (!string.IsNullOrWhiteSpace(queryId))
            {
                resp = await ProcessProjectionsGetLeagueSummaryQuery("get-league-summary", queryId, log);
                if (null != resp)
                {
                    message = resp.Message;
                    #region Logging
                    if (resp.FatalError )
                    {
                        log.LogError($"Error {resp.Message } running {resp.FunctionName }");
                    }
                    else
                    {
                        log.LogInformation($"{resp.FunctionName} succeeded {resp.Message} ");
                    }
                    #endregion
                }
            }

            if (string.IsNullOrEmpty(queryId))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a QueryId on the query string or in the request body");
            }
            else
            {
                return req.CreateResponse(HttpStatusCode.OK, $"Process projections for get-league-summary {queryId}, {message }");
            }
        }

        /// <summary>
        /// Run the projections that are needed to be run to answer this query
        /// </summary>
        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [AggregateRoot("League")]
        [QueryName("Get League Summary")]
        [FunctionName("GetLeagueSummaryQueryProjectionProcessActivity")]
        public static async Task<ActivityResponse> GetLeagueSummaryQueryProjectionProcessActivity(
            [ActivityTrigger] DurableActivityContext context,
            ILogger log)
        {

            ActivityResponse ret = new ActivityResponse() { FunctionName = "GetLeagueSummaryQueryProjectionProcessActivity" };

            try
            {
                QueryRequest<Get_League_Summary_Definition> queryRequest = context.GetInput<QueryRequest<Get_League_Summary_Definition>>();

                if (null != log)
                {
                    log.LogInformation($"GetLeagueSummaryQueryProjectionProcessActivity called for query : {queryRequest.QueryUniqueIdentifier}");
                }

                ret = await ProcessProjectionsGetLeagueSummaryQuery(queryRequest.QueryName ,
                    queryRequest.QueryUniqueIdentifier.ToString(), 
                    log);

                ret.FunctionName = "GetLeagueSummaryQueryProjectionProcessActivity";
                ret.Message = $"Projections processed for {queryRequest.QueryUniqueIdentifier}";
            }
            catch (Exception ex)
            {
                ret.Message = ex.Message;
                ret.FatalError = true;
            }

            return ret;
        }

        /// <summary>
        /// Take an un-processed projection request from the Get League Summary query and process it
        /// </summary>
        /// <param name="queryId">
        /// The unique identifier of the query for which to process a projection
        /// </param>
        /// <param name="log">
        /// The trace output to log to (if set)
        /// </param>
        private static async Task<ActivityResponse>  ProcessProjectionsGetLeagueSummaryQuery(
            string queryName,
            string queryId, 
            ILogger log)
        {

            ActivityResponse ret = new ActivityResponse() { FunctionName = "ProcessProjectionsGetLeagueSummaryQuery" };

            Guid queryGuid;

            try
            {
                if (Guid.TryParse(queryId, out queryGuid))
                {
                    // Get the current state of the query...
                    Projection getQueryState = new Projection(Constants.Domain_Query ,
                        queryName,
                        queryId,
                        nameof(Query_Summary_Projection));

                    if (null != getQueryState)
                    {

                        #region Logging
                        if (null != log)
                        {
                            log.LogDebug($"Projection processor created in RequestProjectionsGetLeagueSummaryQuery");
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
                                log.LogDebug($"Query { qryProjection.QueryName } projection run for {queryGuid } in RequestProjectionsGetLeagueSummaryQuery");
                            }
                            #endregion

                            // Ignore queries in an invalid state...
                            if ((qryProjection.CurrentState == Query_Summary_Projection.QueryState.Completed) ||
                                 (qryProjection.CurrentState == Query_Summary_Projection.QueryState.Invalid))
                            {
                                // No need to validate a completed query
                                #region Logging
                                if (null != log)
                                {
                                    log.LogWarning($"Query {queryGuid} state is {qryProjection.CurrentState} so no projections requested in RequestProjectionsGetLeagueSummaryQuery");
                                }
                                #endregion
                                return ret;
                            }


                            // Find out what projections have been already requested
                            Query_Projections_Projection qryProjectionsRequested = new Query_Projections_Projection();
                            await getQueryState.Process(qryProjectionsRequested);

                            if ((qryProjectionsRequested.CurrentSequenceNumber > 0) || (qryProjectionsRequested.ProjectionValuesChanged()))
                            {
                                // Process the query state as is now...
                                if ((qryProjectionsRequested.UnprocessedRequests.Count == 1) && (qryProjectionsRequested.ProcessedRequests.Count == 0))
                                {
                                    // Run the requested projection
                                    var nextProjectionRequest = qryProjectionsRequested.UnprocessedRequests[0];
                                    if (null != nextProjectionRequest)
                                    {
                                        #region Logging
                                        if (null != log)
                                        {
                                            log.LogDebug($"Query {queryName} running projection {nextProjectionRequest.ProjectionTypeName } for {queryGuid } in ProcessProjectionsGetLeagueSummaryQuery");
                                        }
                                        #endregion
                                        ret.Message = $"Query {queryName} running projection {nextProjectionRequest.ProjectionTypeName } for {queryGuid } in ProcessProjectionsGetLeagueSummaryQuery";
                                        if (nextProjectionRequest.ProjectionTypeName == typeof(Leagues.League.projection.League_Summary_Information).Name)
                                        {
                                            // run the League_Summary_Information projection..
                                            Projection leagueEvents = new Projection(nextProjectionRequest.DomainName,
                                                nextProjectionRequest.AggregateType,
                                                nextProjectionRequest.AggregateInstanceKey,
                                                typeof(Leagues.League.projection.League_Summary_Information).Name);

                                            if (null != leagueEvents)
                                            {
                                                Leagues.League.projection.League_Summary_Information prjLeagueInfo = new Leagues.League.projection.League_Summary_Information();
                                                await leagueEvents.Process(prjLeagueInfo);
                                                if (null != prjLeagueInfo)
                                                {
                                                    if ((prjLeagueInfo.CurrentSequenceNumber > 0) || (prjLeagueInfo.ProjectionValuesChanged()))
                                                    {
                                                        // append the projection result to the query
                                                        await QueryLogRecord.LogProjectionResult(queryGuid,
                                                                qryProjection.QueryName,
                                                                nameof(Leagues.League.projection.League_Summary_Information),
                                                                leagueEvents.DomainName,
                                                                leagueEvents.AggregateTypeName,
                                                                leagueEvents.AggregateInstanceKey,
                                                                prjLeagueInfo.CurrentAsOfDate ,
                                                                prjLeagueInfo,
                                                                prjLeagueInfo.CurrentSequenceNumber);

                                                        #region Logging
                                                        if (null != log)
                                                        {
                                                            log.LogDebug($"Query {queryName } projection {nextProjectionRequest.ProjectionTypeName } key {nextProjectionRequest.AggregateInstanceKey } run to sequence number {prjLeagueInfo.CurrentSequenceNumber } in ProcessProjectionsGetLeagueSummaryQuery");
                                                        }
                                                        #endregion
                                                        ret.Message = $"Query {queryName } projection {nextProjectionRequest.ProjectionTypeName } key {nextProjectionRequest.AggregateInstanceKey } run to sequence number {prjLeagueInfo.CurrentSequenceNumber } ";
                                                        return ret;
                                                    }
                                                    else
                                                    {
                                                        #region Logging
                                                        if (null != log)
                                                        {
                                                            log.LogWarning($"Query {queryName } running projection {nextProjectionRequest.ProjectionTypeName } for {queryGuid } returned no data in ProcessProjectionsGetLeagueSummaryQuery");
                                                        }
                                                        #endregion
                                                        ret.Message = $"Query { queryName}  unable to create event stream for projection { nextProjectionRequest.ProjectionTypeName} returned no data in ProcessProjectionsGetLeagueSummaryQuery";
                                                        return ret;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                #region Logging
                                                if (null != log)
                                                {
                                                    log.LogError($"Query {queryName} unable to create event stream for projection {nextProjectionRequest.ProjectionTypeName } key {nextProjectionRequest.AggregateInstanceKey } in ProcessProjectionsGetLeagueSummaryQuery");
                                                }
                                                #endregion
                                                ret.Message = $"Query { queryName}  unable to create event stream for projection { nextProjectionRequest.ProjectionTypeName} key { nextProjectionRequest.AggregateInstanceKey}";
                                                return ret;
                                            }
                                        }
                                        else
                                        {
                                            return ret;
                                        }
                                    }
                                }
                                else
                                {
                                    if (qryProjectionsRequested.UnprocessedRequests.Count == 0)
                                    {
                                        #region Logging
                                        if (null != log)
                                        {
                                            log.LogWarning($"Query {queryName } projection not yet requested for {queryGuid } in ProcessProjectionsGetLeagueSummaryQuery");
                                        }
                                        #endregion
                                        ret.Message = $"Query {queryName } projection not yet requested for {queryGuid }";
                                        return ret;
                                    }
                                    if (qryProjectionsRequested.ProcessedRequests.Count == 1)
                                    {
                                        #region Logging
                                        if (null != log)
                                        {
                                            log.LogWarning($"Query {queryName } projection already processed for {queryGuid } in ProcessProjectionsGetLeagueSummaryQuery");
                                        }
                                        #endregion
                                        ret.Message = $"Query {queryName } projection already processed for {queryGuid }";
                                        return ret;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    #region Logging
                    if (null != log)
                    {
                        log.LogError($"Projection processor not passed a correct query identifier : {queryId} for query {queryName} ");
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                ret.Message = ex.ToString();
                ret.FatalError = true;
            }

            return ret;
        }
    }
}
