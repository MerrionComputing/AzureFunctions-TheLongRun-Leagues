using Leagues.League.queryDefinition;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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


        /// <summary>
        /// Request the projections that are needed to be run to answer this query
        /// </summary>
        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [AggregateRoot("League")]
        [QueryName("Get League Summary")]
        [FunctionName("GetLeagueSummaryQueryProjectionsProcess")]
        public static async Task<IActionResult> GetLeagueSummaryQueryProjectionsProcessRun(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequest req,
            [OrchestrationClient] DurableOrchestrationClient orchestrationClient,
            ILogger log)
        {

            #region Logging
            if (null != log)
            {
                log.LogDebug("Function triggered HTTP in GetLeagueSummaryQueryProjectionsProcess");
            }
            #endregion

            // Get the query identifier
            string queryId = req.Query["QueryId"];

            if (string.IsNullOrWhiteSpace(queryId))
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                queryId = queryId ?? data?.QueryId;
            }

            QueryRequest<Get_League_Summary_Definition> queryRequest = new QueryRequest<Get_League_Summary_Definition>();
            queryRequest.QueryUniqueIdentifier = new Guid(queryId);
            queryRequest.QueryName = "";

            // start an orchestrator to run all the projections...
            string instanceId = await orchestrationClient.StartNewAsync("GetLeagueSummaryQueryProjectionProcessOrchestrator", queryRequest);

            return queryId == null
                ? new BadRequestObjectResult("Please pass a queryId on the query string or in the request body")
                : (ActionResult)new OkObjectResult($"Run projections for query {queryId} using orcherstration id {instanceId} ");

        }


        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [AggregateRoot("League")]
        [QueryName("Get League Summary")]
        [EventTopicSourceName("Get-League-Summary-Query")]
        [FunctionName("GetLeagueSummaryQueryProjectionProcessOrchestrator")]
        public static async Task<ActivityResponse> GetLeagueSummaryQueryProjectionProcessOrchestrator
            ([OrchestrationTrigger] DurableOrchestrationContext context,
            Microsoft.Extensions.Logging.ILogger log)
        {

            ActivityResponse resp = new ActivityResponse() { FunctionName = "GetLeagueSummaryQueryProjectionProcessOrchestrator" };

            // Get the query definition form the context...
            QueryRequest<Get_League_Summary_Definition> queryRequest = context.GetInput<QueryRequest<Get_League_Summary_Definition>>();

            // Get all the outstanding projection requests
            Query_Projections_Projection_Request projectionQueryRequest = new Query_Projections_Projection_Request() { UniqueIdentifier = queryRequest.QueryUniqueIdentifier.ToString(), QueryName = queryRequest.QueryName };
            IEnumerable<Query_Projections_Projection_Return> allProjections = await context.CallActivityAsync<IEnumerable<Query_Projections_Projection_Return>>("GetQueryProjectionsStatusProjectionActivity", queryRequest);

            if (null != allProjections)
            {

                // This should be done by fan-out/fan-in
                List<Task<ProjectionResultsRecord<Get_League_Summary_Definition_Return>>> allProjectionTasks = new List<Task<ProjectionResultsRecord<Get_League_Summary_Definition_Return>>>();

                // run all the outstanding projections in parallel
                foreach (var projectionRequest in allProjections)
                {
                    if (projectionRequest.ProjectionState == Query_Projections_Projection_Return.QueryProjectionState.Queued)
                    {
                        // mark it as in-flight

                        // and start running it...
                        ProjectionRequest projRequest = new ProjectionRequest()
                        {
                            EntityUniqueIdentifier = projectionRequest.Projection.InstanceKey,
                            AsOfDate = null,
                            ProjectionName = projectionRequest.Projection.ProjectionTypeName
                        };

                        // and start running it...
                        allProjectionTasks.Add(context.CallActivityAsync<ProjectionResultsRecord<Get_League_Summary_Definition_Return>>("RunLeagueSummaryInformationProjectionActivity", projRequest));
                    }
                }

                #region Logging
                if (null != log)
                {
                    log.LogInformation($"Running {allProjectionTasks.Count } projections in parallel");
                }
                #endregion

                // and persist their results to the query
                context.SetCustomStatus($"Running {allProjectionTasks.Count } projections in parallel");

                await Task.WhenAll(allProjectionTasks);

                #region Logging
                if (null != log)
                {
                    log.LogInformation($"Completed running {allProjectionTasks.Count } projections in parallel");
                }
                #endregion

                foreach (var returnValue in allProjectionTasks)
                {
                    ProjectionResultsRecord<Get_League_Summary_Definition_Return> projectionResponse = returnValue.Result;
                    // add in the extra details
                    projectionResponse.CorrelationIdentifier = queryRequest.QueryUniqueIdentifier;
                    projectionResponse.ParentRequestName = queryRequest.QueryName;
                    // log the result...
                    resp = await context.CallActivityAsync<ActivityResponse>("LogQueryProjectionResultActivity", projectionResponse);

                    #region Logging
                    if (null != log)
                    {
                        if (null != resp)
                        {
                            log.LogInformation($"{resp.FunctionName} complete: {resp.Message } ");
                        }
                    }
                    #endregion


                    if (null != resp)
                    {
                        context.SetCustomStatus(resp);
                    }

                }
            }

            return resp;

        }

        /// <summary>
        /// Log the result from a projection projections that are needed to be run to answer this query
        /// </summary>
        [ApplicationName("The Long Run")]
        [DomainName(Constants.Domain_Query )]
        [FunctionName("LogQueryProjectionResultActivity")]
        public static async Task<ActivityResponse> LogQueryProjectionResultActivity(
            [ActivityTrigger] DurableActivityContext context,
            ILogger log,
            CQRSAzure.EventSourcing.IWriteContext writeContext = null)
        {

            ActivityResponse resp = new ActivityResponse() { FunctionName = "LogQueryProjectionResultActivity" };

            // get the ProjectionResultsRecord
            ProjectionResultsRecord<object> data = context.GetInput<ProjectionResultsRecord<object>>();
            if (null != data)
            {
                await QueryLogRecord.LogProjectionResult(data.CorrelationIdentifier ,
                                                         data.ParentRequestName ,
                                                         data.ProjectionName ,
                                                         data.DomainName,
                                                         data.AggregateTypeName,
                                                         data.EntityUniqueIdentifier,
                                                         data.CurrentAsOfDate,
                                                         data.Result ,
                                                         data.CurrentSequenceNumber,
                                                         writeContext);

                resp.Message = $"Saved projection result to query {data.ParentRequestName} - {data.CorrelationIdentifier} ";
            }
            else
            {
                resp.Message = "Unable to get projection result from context";
                resp.FatalError = true;
            }

            return resp;

        }


        //LogQueryProjectionInFlightActivity
        [ApplicationName("The Long Run")]
        [DomainName(Constants.Domain_Query)]
        [FunctionName("LogQueryProjectionInFlightActivity")]
        public static async Task<ActivityResponse> LogQueryProjectionInFlightActivity(
            [ActivityTrigger] DurableActivityContext context,
            ILogger log,
            CQRSAzure.EventSourcing.IWriteContext writeContext = null)
        {

            ActivityResponse resp = new ActivityResponse() { FunctionName = "LogQueryProjectionInFlightActivity" };

            ProjectionRequest projectionRequest = context.GetInput<ProjectionRequest>();
            if (null != projectionRequest )
            {
                await QueryLogRecord.LogProjectionStarted(new Guid(projectionRequest.EntityUniqueIdentifier),
                    projectionRequest.EntityUniqueIdentifier ,
                    projectionRequest.ProjectionName ,
                    Constants.Domain_Query ,
                    "",
                    projectionRequest.EntityUniqueIdentifier ,
                    projectionRequest.AsOfDate ,
                    "",
                    writeContext 
                    );

                resp.Message = $"Started running projection  query {projectionRequest.ProjectionName} - {projectionRequest.EntityUniqueIdentifier} ({projectionRequest.AsOfDate}) ";
            }
            else
            {
                resp.Message = "Unable to get projection request from context";
                resp.FatalError = true;
            }

            return resp;
        }
    }
}
