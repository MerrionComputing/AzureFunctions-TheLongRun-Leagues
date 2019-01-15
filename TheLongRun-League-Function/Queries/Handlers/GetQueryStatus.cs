using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Collections.Generic;
using TheLongRun.Common.Events.Query.Projections;
using TheLongRun.Common.Bindings;
using TheLongRun.Common.Attributes;
using TheLongRun.Common;
using TheLongRunLeaguesFunction.Identifier_Groups;

namespace TheLongRunLeaguesFunction.Queries
{
    public static class Query
    {
        [ApplicationName("The Long Run")]
        [DomainName("Query")]
        [FunctionName("GetAllQueryStatusByName")]
        public static async Task<HttpResponseMessage> GetAllQueryStatusByNameRun(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequestMessage req,
            [OrchestrationClient] DurableOrchestrationClient getAllQueryStatusByNameOrchestrationClient,
            ILogger log)
        {

            #region Logging
            if (null != log)
            {
                log.LogInformation("Get all query status by name");
            }
            #endregion

            int timeoutLength = 30;  // seconds
            int retryWait = 1; // seconds

            string queryName = req.RequestUri.ParseQueryString()["QueryName"];
            string asOfDateString = req.RequestUri.ParseQueryString()["AsOfDate"];
            DateTime? asOfDate = null;
            if (! string.IsNullOrWhiteSpace(asOfDateString ) )
            {
                DateTime dtOut;
                if (DateTime.TryParse(asOfDateString, out dtOut ))
                {
                    asOfDate = dtOut;
                }
            }
            string timeoutLengthString = req.RequestUri.ParseQueryString()["TimeOut"];
            if (!string.IsNullOrWhiteSpace(timeoutLengthString))
            {
                int.TryParse(timeoutLengthString, out timeoutLength);
            }
            string retryWaitString = req.RequestUri.ParseQueryString()["RetryWait"];
            if (!string.IsNullOrWhiteSpace(retryWaitString))
            {
                int.TryParse(retryWaitString, out retryWait );
            }

            dynamic eventData = await req.Content.ReadAsAsync<object>();
            queryName = queryName ?? eventData?.QueryName;
            asOfDate = asOfDate ?? eventData?.AsOfDate;

            if (! string.IsNullOrWhiteSpace(queryName) )
            {

                GetAllQueryStatusByNameRun_Request payload = new GetAllQueryStatusByNameRun_Request()
                {
                    QueryName = queryName,
                    AsOfDate = asOfDate
                };

                // Use durable functions to get the status of all the queries of the given name
                string instanceId = await getAllQueryStatusByNameOrchestrationClient.StartNewAsync("GetAllQueryStatusByNameOrchestrator", payload);

                #region Logging
                if (null != log)
                {
                    log.LogInformation($"Started GetAllQueryStatusByNameOrchestrator - instance id: {instanceId }");
                }
                #endregion

                // Wait for it to complete
                TimeSpan timeout = TimeSpan.FromSeconds(timeoutLength);
                TimeSpan retryInterval =  TimeSpan.FromSeconds(retryWait );

                return await getAllQueryStatusByNameOrchestrationClient.WaitForCompletionOrCreateCheckStatusResponseAsync(
                    req,
                    instanceId,
                    timeout,
                    retryInterval);

                // Get the results
                

            }
            else
            {
                return req.CreateResponse(System.Net.HttpStatusCode.BadRequest, "Please pass a query name in the query string or in the request body");
            }

            
        }


        [ApplicationName("The Long Run")]
        [DomainName("Query")]
        [FunctionName("GetAllQueryStatusByNameOrchestrator")]
        public static async Task<List<Query_Summary_Projection_Return>> GetAllQueryStatusByNameOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context,
            Microsoft.Extensions.Logging.ILogger log)
        {

            string queryName= string.Empty ;
            DateTime? asOfDate = null;

            GetAllQueryStatusByNameRun_Request payload = context.GetInput<GetAllQueryStatusByNameRun_Request>(); 
            if (null != payload)
            {
                queryName = payload.QueryName;
                asOfDate = payload.AsOfDate;
            }

            List <Query_Summary_Projection_Return> ret = new List<Query_Summary_Projection_Return>() ;
            // get every query id of the given name...

            #region Logging
            if (null != log)
            {
                log.LogInformation($"Creating identifier group processor for {queryName }");
            }
            #endregion
            context.SetCustomStatus($"Creating identifier group processor for {queryName }");

            AllQueriesIdentifierGroup_Request groupRequest = new AllQueriesIdentifierGroup_Request()
            {
               QueryName = queryName,
               AsOfDate= asOfDate
            };

            IEnumerable<string> allQueryIds = await context.CallActivityWithRetryAsync<IEnumerable<string>>("GetAllQueriesIdentifierGroupActivity",
                DomainSettings.QueryRetryOptions(),
                groupRequest );


            if (null != allQueryIds)
            {
                // This should be done by fan-out/fan-in
                List<Task<Query_Summary_Projection_Return>> allTasks = new List<Task<Query_Summary_Projection_Return>>();

                foreach (string queryId in allQueryIds )
                {

                    #region Logging
                    if (null != log)
                    {
                        log.LogInformation($"Queueing {queryId}");
                    }
                    #endregion
                    context.SetCustomStatus($"Queueing {queryId}");

                    // add each to the list...
                    Query_Summary_Projection_Request request = new Query_Summary_Projection_Request()
                    {
                        QueryName = queryName,
                        UniqueIdentifier = queryId
                    };

                    allTasks.Add(context.CallActivityWithRetryAsync <Query_Summary_Projection_Return>("GetQueryStatusInformationProjectionActivity",
                        DomainSettings.QueryRetryOptions(),
                        request));
                }

                #region Logging
                if (null != log)
                {
                    log.LogInformation($"Running {allTasks.Count } projections in parallel");
                }
                #endregion
                context.SetCustomStatus($"Running {allTasks.Count } projections in parallel");

                await Task.WhenAll(allTasks);

                foreach (var returnValue in allTasks)
                {
                    ret.Add(returnValue.Result);
                }
            }

            #region Logging
            if (null != log)
            {
                log.LogInformation($"Completed {ret.Count } projections in parallel for {queryName}");
            }
            #endregion
            return ret;
        }


        public class GetAllQueryStatusByNameRun_Request
        {

            /// <summary>
            /// The name of the specific query for which we want to get all the instances
            /// </summary>
            public string QueryName { get; set; }

            public DateTime? AsOfDate { get; set; }
        }
    }
}
