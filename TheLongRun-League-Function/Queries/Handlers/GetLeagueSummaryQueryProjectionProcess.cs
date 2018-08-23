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
        [FunctionName("GetLeagueSummaryQueryProjectionProcess")]
        public static async Task<HttpResponseMessage> GetLeagueSummaryQueryProjectionProcessRun(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage req,
            TraceWriter log)
        {
            #region Logging
            if (null != log)
            {
                log.Verbose("Function triggered HTTP ",
                    source: "GetLeagueSummaryQueryProjectionProcess");
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

            ProcessProjectionsGetLeagueSummaryQuery(queryId, log);

            return queryId == null
                ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a queryId on the query string or in the request body")
                : req.CreateResponse(HttpStatusCode.OK, $"Validated query {queryId}");
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
        private static void ProcessProjectionsGetLeagueSummaryQuery(string queryId, 
            TraceWriter log = null)
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
                        log.Verbose($"Projection processor created",
                            source: "RequestProjectionsGetLeagueSummaryQuery");
                    }
                    #endregion

                    // Run the query summary projection
                    Query_Summary_Projection qryProjection =
                            new Query_Summary_Projection(log);

                    getQueryState.Process(qryProjection);

                    if ((qryProjection.CurrentSequenceNumber > 0) || (qryProjection.ProjectionValuesChanged()))
                    {
                        // Process the query state as is now...
                        #region Logging
                        if (null != log)
                        {
                            log.Verbose($"Query { qryProjection.QueryName } projection run for {queryGuid } ",
                                source: "RequestProjectionsGetLeagueSummaryQuery");
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
                                log.Warning($"Query {queryGuid} state is {qryProjection.CurrentState} so no projections requested ",
                                    source: "RequestProjectionsGetLeagueSummaryQuery");
                            }
                            #endregion
                            return;
                        }


                        // Find out what projections have been already requested
                        Query_Projections_Projection qryProjectionsRequested = new Query_Projections_Projection();
                        getQueryState.Process(qryProjectionsRequested);

                        if ((qryProjectionsRequested.CurrentSequenceNumber > 0) || (qryProjectionsRequested.ProjectionValuesChanged()))
                        {
                            // Process the query state as is now...
                            if ((qryProjectionsRequested.UnprocessedRequests.Count == 1) && (qryProjectionsRequested.ProcessedRequests.Count == 0))
                            {
                                // Run the requested projection
                                var nextProjectionRequest = qryProjectionsRequested.UnprocessedRequests[0];
                                if (null != nextProjectionRequest )
                                {
                                    #region Logging
                                    if (null != log)
                                    {
                                        log.Verbose ($"Query {QUERY_NAME} running projection {nextProjectionRequest.ProjectionTypeName } for {queryGuid } ",
                                            source: "ProcessProjectionsGetLeagueSummaryQuery");
                                    }
                                    #endregion
                                    if (nextProjectionRequest.ProjectionTypeName == typeof(Leagues.League.projection.League_Summary_Information).Name )
                                    {
                                        // run the League_Summary_Information projection..
                                        Projection  leagueEvents = new Projection (nextProjectionRequest.DomainName,
                                            nextProjectionRequest.AggregateType,
                                            nextProjectionRequest.AggregateInstanceKey,
                                            typeof(Leagues.League.projection.League_Summary_Information).Name);

                                        if (null != leagueEvents)
                                        {
                                            Leagues.League.projection.League_Summary_Information prjLeagueInfo = new Leagues.League.projection.League_Summary_Information();
                                            leagueEvents.Process(prjLeagueInfo);
                                            if (null != prjLeagueInfo )
                                            {
                                                if ((prjLeagueInfo.CurrentSequenceNumber > 0 ) ||  (prjLeagueInfo.ProjectionValuesChanged()))
                                                {
                                                    // append the projection result to the query
                                                    QueryLogRecord.LogProjectionResult(queryGuid,
                                                            qryProjection.QueryName,
                                                            nameof(Leagues.League.projection.League_Summary_Information),
                                                            leagueEvents.DomainName,
                                                            leagueEvents.AggregateTypeName ,
                                                            leagueEvents.AggregateInstanceKey  ,
                                                            null,
                                                            prjLeagueInfo,
                                                            prjLeagueInfo.CurrentSequenceNumber );

                                                    #region Logging
                                                    if (null != log)
                                                    {
                                                        log.Verbose ($"Query {QUERY_NAME} projection {nextProjectionRequest.ProjectionTypeName } key {nextProjectionRequest.AggregateInstanceKey } run to sequence number {prjLeagueInfo.CurrentSequenceNumber } ",
                                                            source: "ProcessProjectionsGetLeagueSummaryQuery");
                                                    }
                                                    #endregion

                                                    // Call the next query in the command chain to send output
                                                    FunctionChaining funcChain = new FunctionChaining(log);
                                                    var queryParams = new System.Collections.Generic.List<Tuple<string, string>>();
                                                    queryParams.Add(new Tuple<string, string>("queryId", queryGuid.ToString()));
                                                    funcChain.TriggerCommandByHTTPS(@"Leagues", "GetLeagueSummaryOutputResults", queryParams, null);
                                                }
                                                else
                                                {
                                                    #region Logging
                                                    if (null != log)
                                                    {
                                                        log.Warning ($"Query {QUERY_NAME} running projection {nextProjectionRequest.ProjectionTypeName } for {queryGuid } returned no data",
                                                            source: "ProcessProjectionsGetLeagueSummaryQuery");
                                                    }
                                                    #endregion
                                                }
                                            }
                                        }
                                        else
                                        {
                                            #region Logging
                                            if (null != log)
                                            {
                                                log.Error ($"Query {QUERY_NAME} unable to create event stream for projection {nextProjectionRequest.ProjectionTypeName } key {nextProjectionRequest.AggregateInstanceKey } ",
                                                    source: "ProcessProjectionsGetLeagueSummaryQuery");
                                            }
                                            #endregion
                                        }
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
                                        log.Warning($"Query {QUERY_NAME} projection not yet requested for {queryGuid } ",
                                            source: "ProcessProjectionsGetLeagueSummaryQuery");
                                    }
                                    #endregion
                                }
                                if (qryProjectionsRequested.ProcessedRequests.Count == 1)
                                {
                                    #region Logging
                                    if (null != log)
                                    {
                                        log.Warning($"Query {QUERY_NAME} projection already processed for {queryGuid } ",
                                            source: "ProcessProjectionsGetLeagueSummaryQuery");
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
}
