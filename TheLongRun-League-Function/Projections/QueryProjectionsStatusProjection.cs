using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TheLongRun.Common;
using TheLongRun.Common.Attributes;
using TheLongRun.Common.Bindings;
using TheLongRun.Common.Events.Query.Projections;

using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net;
using System.Collections.Generic;
using TheLongRun.Common.Events.Query;

namespace TheLongRunLeaguesFunction.Projections
{
    /// <summary>
    /// Run the projection to get the list of projections (requested or complete)
    /// linked to a given query identifier
    /// </summary>
    [ApplicationName("The Long Run")]
    [DomainName(Constants.Domain_Query)]
    [ProjectionName("Query Projections")]
    public static class QueryProjectionsStatusProjection
    {

        [ApplicationName("The Long Run")]
        [DomainName(Constants.Domain_Query)]
        [ProjectionName("Query Summary")]
        [FunctionName("GetQueryProjectionsStatusProjection")]
        public static async Task<HttpResponseMessage> GetQueryProjectionsStatusProjectionRun(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequestMessage req,
            ILogger log)
        {

            #region Logging
            if (null != log)
            {
                log.LogDebug("Function triggered HTTP in GetQueryProjectionsStatusProjection");
            }
            #endregion

            // get the query id and query name
            string queryId;
            string queryName;

            queryId = req.GetQueryNameValuePairsExt()[@"QueryId"];
            queryName = req.GetQueryNameValuePairsExt()[@"QueryName"];

            if (string.IsNullOrWhiteSpace(queryId))
            {
                // Get request body
                dynamic data = await req.Content.ReadAsAsync<object>();
                queryId = data?.QueryId;
                queryName = data?.QueryName;
            }

            IEnumerable<Query_Projections_Projection_Return> ret = null;

            ret= await ProcessQueryProjectionsStatusProjection(
                queryName,
                queryId,
                log);

            if ( (string.IsNullOrWhiteSpace(queryId)) || (string.IsNullOrWhiteSpace(queryName)))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest,
                    "Please pass a Query Id and Query Name  on the query string or in the request body");
            }
            else
            {
                return req.CreateResponse(HttpStatusCode.OK, ret);
            }
        }


        [ApplicationName("The Long Run")]
        [DomainName(Constants.Domain_Query)]
        [ProjectionName("Query Summary")]
        [FunctionName("GetQueryProjectionsStatusProjectionActivity")]
        public static async Task<List<Query_Projections_Projection_Return>> GetQueryProjectionsStatusProjectionActivity(
                        [ActivityTrigger] DurableActivityContext context,
                        ILogger log
                        )
        {

            Query_Projections_Projection_Request queryInfo = context.GetInput<Query_Projections_Projection_Request>();

            #region Logging
            if (null != log)
            {
                log.LogInformation($"GetQueryProjectionsStatusProjectionActivity called for query ID: {queryInfo.UniqueIdentifier }");
            }
            #endregion

            return await ProcessQueryProjectionsStatusProjection(
                queryInfo.QueryName,
                queryInfo.UniqueIdentifier,
                log);
        }

        private static async Task<List<Query_Projections_Projection_Return>> ProcessQueryProjectionsStatusProjection(
            string queryName,
            string queryId,
            ILogger log)
        {

            List<Query_Projections_Projection_Return> ret = new List<Query_Projections_Projection_Return>();
            Guid queryGuid;

            // use custom assembly resolve handler
            using (new AzureFunctionsResolveAssembly())
            {
                if (Guid.TryParse(queryId, out queryGuid))
                {
                    #region Logging
                    if (null != log)
                    {
                        log.LogDebug($"Getting projection details of query  {queryName} - ID: {queryId} in ProcessQueryProjectionsStatusProjection");
                    }
                    #endregion

                    // Get the current state of the command...
                    Projection getQueryProjections = new Projection(Constants.Domain_Query,
                        queryName,
                        queryGuid.ToString(),
                        nameof(Query_Projections_Projection));

                    if (null != getQueryProjections)
                    {

                        #region Logging
                        if (null != log)
                        {
                            log.LogDebug($"Projection processor created in ProcessQueryProjectionsStatusProjection");
                        }
                        #endregion

                        Query_Projections_Projection qryProjection =
                            new Query_Projections_Projection(log);

                        await getQueryProjections.Process(qryProjection);

                        if ((qryProjection.CurrentSequenceNumber > 0) || (qryProjection.ProjectionValuesChanged()))
                        {
                            // 1 - add the completed projections
                            foreach (TheLongRun.Common.Events.Query.ProjectionValueReturned prj in qryProjection.ProcessedRequests)
                            {
                                ret.Add(new Query_Projections_Projection_Return()
                                {
                                    ProjectionState = Query_Projections_Projection_Return.QueryProjectionState.Complete,
                                    Projection = new ProjectionAttribute(
                                        prj.DomainName, 
                                        prj.AggregateType, 
                                        prj.AggregateInstanceKey, 
                                        prj.ProjectionTypeName)
                                });
                            }

                            // 2- add the in-progress ones
                            foreach (TheLongRun.Common.Events.Query.ProjectionRunStarted prj in qryProjection.RequestsInProgress)
                            {
                                ret.Add(new Query_Projections_Projection_Return()
                                {
                                    ProjectionState = Query_Projections_Projection_Return.QueryProjectionState.InProgress,
                                    Projection = new ProjectionAttribute(
                                        prj.DomainName, 
                                        prj.AggregateType, 
                                        prj.AggregateInstanceKey, 
                                        prj.ProjectionTypeName)
                                });
                            }

                            // 3 - add the queued ones
                            foreach (TheLongRun.Common.Events.Query.ProjectionRequested prj in qryProjection.UnprocessedRequests)
                            {
                                ret.Add(new Query_Projections_Projection_Return()
                                {
                                    ProjectionState = Query_Projections_Projection_Return.QueryProjectionState.Queued,
                                    Projection = new ProjectionAttribute(
                                        prj.DomainName, 
                                        prj.AggregateType, 
                                        prj.AggregateInstanceKey, 
                                        prj.ProjectionTypeName)
                                });
                            }
                        }
                    }
                }
            }


            #region Logging
            if (null != log)
            {
                log.LogDebug($"Returning {ret.Count} projection records in ProcessQueryProjectionsStatusProjection");
            }
            #endregion
            // Return all the projections we found back to the calling code
            return ret;
        }
    }
}
