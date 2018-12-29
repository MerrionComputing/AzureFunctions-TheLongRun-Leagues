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
            log.LogInformation("Get all query status by name");

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

                // Wait for it to complete
                TimeSpan timeout = TimeSpan.FromSeconds(30);
                TimeSpan retryInterval =  TimeSpan.FromSeconds(1);

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

            IdentifierGroup allQueries = new IdentifierGroup("Query",
                queryName,
                "All");

            // This should be done by fan-out/fan-in
            List<Task<Query_Summary_Projection_Return>> allTasks = new List<Task<Query_Summary_Projection_Return>>(); 
            foreach (string queryId in await allQueries.GetAll(asOfDate))
            {

                context.SetCustomStatus( $"Queueing {queryId}" );

                // add each to the list...
                Query_Summary_Projection_Request request = new Query_Summary_Projection_Request()
                {
                    QueryName = queryName,
                    UniqueIdentifier = queryId
                };

                allTasks.Add( context.CallActivityAsync<Query_Summary_Projection_Return>("GetQueryStatusInformationProjectionActivity",
                    request));
            }

            await Task.WhenAll( allTasks);

            foreach (var returnValue in allTasks )
            {
                ret.Add(returnValue.Result);
            }


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
