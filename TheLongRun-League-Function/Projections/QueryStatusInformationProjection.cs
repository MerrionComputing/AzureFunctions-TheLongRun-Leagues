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

namespace TheLongRunLeaguesFunction.Projections
{

    /// <summary>
    /// Get the current status of a given query by running the status information projection over it
    /// </summary>
    [ApplicationName("The Long Run")]
    [DomainName(Constants.Domain_Query)]
    [ProjectionName("Query Summary")]
    public static class QueryStatusInformationProjection
    {


        [ApplicationName("The Long Run")]
        [DomainName(Constants.Domain_Query)]
        [ProjectionName("Query Summary")]
        [FunctionName("GetQueryStatusInformationProjection")]
        public static async Task<HttpResponseMessage> GetQueryStatusInformationProjectionnRun(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequestMessage req,
        ILogger log)
        {
            #region Logging
            if (null != log)
            {
                log.LogDebug("Function triggered HTTP in GetQueryStatusInformationProjection");
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

            // Run the projection....ProcessCommandStatusInformationProjection
            Query_Summary_Projection_Return ret = await ProcessQueryStatusInformationProjection(
                queryName,
                queryId,
                log);


            if ((string.IsNullOrWhiteSpace(queryId)) || (string.IsNullOrEmpty(queryName)))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest,
                    "Please pass a query Id and query name on the query string or in the request body");
            }
            else
            {
                return req.CreateResponse(HttpStatusCode.OK, ret);
            }
        }

        [ApplicationName("The Long Run")]
        [DomainName(Constants.Domain_Query)]
        [ProjectionName("Query Summary")]
        [FunctionName("GetQueryStatusInformationProjectionActivity")]
        public static async Task<Query_Summary_Projection_Return> GetQueryStatusInformationProjectionActivity(
        [ActivityTrigger] DurableActivityContext context,
        ILogger log
        )
        {

            Query_Summary_Projection_Request queryInfo = context.GetInput<Query_Summary_Projection_Request>();

            #region Logging
            if (null != log)
            {
                log.LogInformation($"GetQueryStatusInformationProjectionActivity called for query : {queryInfo.QueryName } - ID: {queryInfo.UniqueIdentifier }");
            }
            #endregion

            return await ProcessQueryStatusInformationProjection(queryInfo.QueryName,
                queryInfo.UniqueIdentifier,
                log);
        }


        private static async Task<Query_Summary_Projection_Return> ProcessQueryStatusInformationProjection(
            string queryName,
            string queryId,
            ILogger log)
        {

            Query_Summary_Projection_Return ret = null;
            Guid queryGuid;

            // use custom assembly resolve handler
            using (new AzureFunctionsResolveAssembly())
            {
                if (Guid.TryParse(queryId, out queryGuid))
                {
                    #region Logging
                    if (null != log)
                    {
                        log.LogDebug($"Getting details of query {queryName} - {queryId} in ProcessQueryStatusInformationProjection");
                    }
                    #endregion

                    // Get the current state of the command...
                    Projection getQueryState = new Projection(Constants.Domain_Query,
                        queryName,
                        queryGuid.ToString(),
                        nameof(Query_Summary_Projection));

                    if (null != getQueryState)
                    {

                        #region Logging
                        if (null != log)
                        {
                            log.LogDebug($"Projection processor created in ProcessCommandStatusInformationProjection");
                        }
                        #endregion

                        Query_Summary_Projection qryProjection =
                            new Query_Summary_Projection(log);

                        await getQueryState.Process(qryProjection);

                        if ((qryProjection.CurrentSequenceNumber > 0) || (qryProjection.ProjectionValuesChanged()))
                        {
                            ret = new Query_Summary_Projection_Return()
                            {
                                AsOfDate = qryProjection.CurrentAsOfDate,
                                AsOfStepNumber = (int)qryProjection.CurrentSequenceNumber,
                                Status = qryProjection.CurrentState.ToString(),
                                QueryName = queryName,
                                CorrelationIdentifier = queryId // for now the correlation id is the query id.. this may change
                            };
                        }
                    }

                }
            }

            return ret;
        }


    }
}
