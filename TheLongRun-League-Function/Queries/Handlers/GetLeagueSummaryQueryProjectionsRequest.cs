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
        /// <summary>
        /// Request the projections that are needed to be run to answer this query
        /// </summary>
        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [AggregateRoot("League")]
        [QueryName("Get League Summary")]
        [FunctionName("GetLeagueSummaryQueryProjectionsRequest")]
        public static async Task<HttpResponseMessage> GetLeagueSummaryQueryProjectionRequestRun(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage req, 
            TraceWriter log)
        {

            #region Logging
            if (null != log)
            {
                log.Verbose("Function triggered HTTP ",
                    source: "GetLeagueSummaryQueryProjectionsRequest");
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

            RequestProjectionsGetLeagueSummaryQuery(queryId, log);

            return queryId == null
                ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a queryId on the query string or in the request body")
                : req.CreateResponse(HttpStatusCode.OK, $"Validated query {queryId}");

        }

        /// <summary>
        /// Add projection requests for the projections that need to be run in order to get the data 
        /// for this Get League Summary query
        /// </summary>
        /// <param name="queryId">
        /// Unique identifier of the query
        /// </param>
        /// <param name="log">
        /// Optional tracing output
        /// </param>
        private static void RequestProjectionsGetLeagueSummaryQuery(string queryId,
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
                        if ( (qryProjection.CurrentState == Query_Summary_Projection.QueryState.Completed) ||
                             (qryProjection.CurrentState == Query_Summary_Projection.QueryState.Invalid ) )
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

                        // Get the league parameter
                        if (qryProjection.ParameterIsSet(nameof(Get_League_Summary_Definition.League_Name)))
                        {
                            string leagueNameParam = qryProjection.GetParameter<string>(nameof(Get_League_Summary_Definition.League_Name));
                            if (string.IsNullOrWhiteSpace(leagueNameParam))
                            {
                                #region Logging
                                if (null != log)
                                {
                                    log.Error($"Query {QUERY_NAME} :: {queryGuid} has a blank league name",
                                        source: "RequestProjectionsGetLeagueSummaryQuery");
                                }
                                #endregion
                            }
                            else
                            {
                                // Find out what projections have been already requested
                                Query_Projections_Projection qryProjectionsRequested = new Query_Projections_Projection();
                                getQueryState.Process(qryProjectionsRequested);

                                if ((qryProjectionsRequested.CurrentSequenceNumber > 0) || (qryProjectionsRequested.ProjectionValuesChanged()))
                                {
                                    // Process the query state as is now...
                                    if ((qryProjectionsRequested.UnprocessedRequests.Count == 0) && (qryProjectionsRequested.ProcessedRequests.Count == 0))
                                    {
                                        // No projections have been added to this so add the "get league summary" request
                                        QueryLogRecord.RequestProjection(queryGuid,
                                            qryProjection.QueryName,
                                            nameof(Leagues.League.projection.League_Summary_Information),
                                            "Leagues",
                                            "League",
                                            leagueNameParam,
                                            null);

#if FUNCTION_CHAINING
                                        // Call the next query in the command chain to process projections
                                        FunctionChaining funcChain = new FunctionChaining(log);
                                        var queryParams = new System.Collections.Generic.List<Tuple<string, string>>();
                                        queryParams.Add(new Tuple<string, string>("queryId", queryGuid.ToString()));
                                        funcChain.TriggerCommandByHTTPS(@"Leagues", "GetLeagueSummaryQueryProjectionProcess", queryParams, null);
#endif
                                    }
                                    else
                                    {
                                        if (qryProjectionsRequested.UnprocessedRequests.Count > 0)
                                        {
                                            #region Logging
                                            if (null != log)
                                            {
                                                log.Warning($"Query {QUERY_NAME} projection in progress for {queryGuid } ",
                                                    source: "RequestProjectionsGetLeagueSummaryQuery");
                                            }
                                            #endregion
                                        }
                                        if (qryProjectionsRequested.ProcessedRequests.Count > 0)
                                        {
                                            #region Logging
                                            if (null != log)
                                            {
                                                log.Warning($"Query {QUERY_NAME} projection already processed for {queryGuid } ",
                                                    source: "RequestProjectionsGetLeagueSummaryQuery");
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
    }
}
