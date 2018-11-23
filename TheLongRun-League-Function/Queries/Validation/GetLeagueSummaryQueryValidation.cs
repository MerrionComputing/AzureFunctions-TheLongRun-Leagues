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
        [FunctionName("GetLeagueSummaryQueryValidation")]
        public static async Task<HttpResponseMessage> GetLeagueSummaryQueryValidationRun(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequestMessage req, TraceWriter log)
        {

            #region Logging
            if (null != log)
            {
                log.Verbose("Function triggered HTTP ",
                    source: "GetLeagueSummaryQueryValidationRun");
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

            ValidateGetLeagueSummaryQuery(queryId, log);

            return queryId == null
                ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a queryId on the query string or in the request body")
                : req.CreateResponse(HttpStatusCode.OK, $"Validated query {queryId}");
        }

        /// <summary>
        /// Perform the validation on the parameters of this "Get League Summary" query and, if all OK, mark it
        /// as allowed to continue
        /// </summary>
        /// <param name="queryId">
        /// Unique identifier of the query event stream for this "Get League Summary" query
        /// </param>
        /// <param name="log">
        /// If set, output progress to this trace writer
        /// </param>
        private static void ValidateGetLeagueSummaryQuery(string queryId, 
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
                            source: "ValidateGetLeagueSummaryQuery");
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
                                source: "ValidateGetLeagueSummaryQuery");
                        }
                        #endregion

                        if (qryProjection.CurrentState == Query_Summary_Projection.QueryState.Completed )
                        {
                            // No need to validate a completed query
                            #region Logging
                            if (null != log)
                            {
                                log.Warning($"Query {queryGuid} is complete so no need to validate ",
                                    source: "ValidateGetLeagueSummaryQuery");
                            }
                            #endregion
                            return;
                        }

                        if (qryProjection.CurrentState == Query_Summary_Projection.QueryState.Invalid )
                        {
                            // No need to validate an already invalid query
                            #region Logging
                            if (null != log)
                            {
                                log.Warning($"Query {queryGuid} is already marked as invalid so no need to validate ",
                                    source: "ValidateGetLeagueSummaryQuery");
                            }
                            #endregion
                            return;
                        }

                        if (qryProjection.CurrentState == Query_Summary_Projection.QueryState.Validated )
                        {
                            // No need to validate an already validated query
                            #region Logging
                            if (null != log)
                            {
                                log.Warning($"Query {queryGuid} is already validated so no need to validate ",
                                    source: "ValidateGetLeagueSummaryQuery");
                            }
                            #endregion
                            return;
                        }

                        // Validations - 1: Check league name is not empty
                        if (qryProjection.ParameterIsSet(nameof(Get_League_Summary_Definition.League_Name )))
                        {
                            string leagueNameParam = qryProjection.GetParameter<string>(nameof(Get_League_Summary_Definition.League_Name));
                            if (string.IsNullOrWhiteSpace(leagueNameParam))
                            {
                                QueryLogRecord.LogQueryValidationError(queryGuid, QUERY_NAME, true, "League name may not be blank");
                                #region Logging
                                if (null != log)
                                {
                                    log.Warning($"Query {QUERY_NAME} :: {queryGuid} has a blank league name",
                                        source: "ValidateGetLeagueSummaryQuery");
                                }
                                #endregion
                            }
                            else
                            {
                                // any additional validation could go here (?)..

#if FUNCTION_CHAINING
                                // Call the next query in the command chain to request projections
                                FunctionChaining funcChain = new FunctionChaining(log);
                                var queryParams = new System.Collections.Generic.List<Tuple<string, string>>();
                                queryParams.Add(new Tuple<string, string>("queryId", queryGuid.ToString()));
                                funcChain.TriggerCommandByHTTPS(@"Leagues", "GetLeagueSummaryQueryProjectionsRequest", queryParams, null);
#endif
                            }
                        }
                        else
                        {
                            // Parameter is mandatory but may not be set yet so leave the query as is
#region Logging
                            if (null != log)
                            {
                                log.Warning ($"Query { qryProjection.QueryName } has no value specified for the parameter {nameof(Get_League_Summary_Definition.League_Name)} ",
                                    source: "ValidateGetLeagueSummaryQuery");
                            }
#endregion
                        }
                    }

                }

            }

        }
    }
}
