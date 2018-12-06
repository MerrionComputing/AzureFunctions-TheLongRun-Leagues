using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TheLongRun.Common.Events.Query.Projections;
using TheLongRun.Common.Bindings;

namespace TheLongRunLeaguesFunction.Queries
{
    public static class Query
    {
        [FunctionName("GetQueryStatus")]
        public static async Task<IActionResult> GetQueryStatusRun(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Get query status");

            string queryId = req.Query["QueryId"];
            string queryName = req.Query["QueryName"];

            string message = "";

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            queryId = queryId ?? data?.QueryId;
            queryName = queryName ?? data?.QueryName;

            // Get the current state of the query...
            Projection getQueryState = new Projection(@"Query",
                queryName,
                queryId,
                nameof(Query_Summary_Projection));

            if (null != getQueryState)
            {

                #region Logging
                if (null != log)
                {
                    log.LogInformation($"Projection processor created to get query state from RequestProjectionsGetLeagueSummaryQuery");
                }
                #endregion

                // Run the query summary projection
                Query_Summary_Projection qryProjection =
                        new Query_Summary_Projection(log);

                await getQueryState.Process(qryProjection);

                if (null != qryProjection )
                {
                    message = $"{qryProjection.QueryName} - {qryProjection.CurrentState } as of {qryProjection.CurrentAsOfDate} sequence {qryProjection.CurrentSequenceNumber}";
                }

            }

            return queryId != null
            ? (ActionResult)new OkObjectResult($"Query status for {queryId} is {message}")
            : new BadRequestObjectResult("Please pass a query unique id on the query string or in the request body");
        }
    }
}
