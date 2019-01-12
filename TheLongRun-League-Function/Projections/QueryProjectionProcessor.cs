using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TheLongRun.Common;
using TheLongRun.Common.Attributes;
using TheLongRun.Common.Bindings;
using TheLongRun.Common.Events.Query.Projections;
using TheLongRun.Common.Orchestration;

namespace TheLongRunLeaguesFunction.Projections
{
    /// <summary>
    /// Durable orchestrations and activities to run the projections requested for a query 
    /// </summary>
    /// <remarks>
    /// There are multiple triggers as I'm not sure yet if a queue or an async HTTP call makes most sense for this
    /// - all part of the experiment
    /// </remarks>
    public static class QueryProjectionProcessor
    {



        //Query_Projections_Projection_Request...
        [ApplicationName("The Long Run")]
        [FunctionName("QueryProjectionProcessor")]
        public static async Task<HttpResponseMessage> QueryProjectionProcessorRun(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequestMessage req,
            [OrchestrationClient] DurableOrchestrationClient runProjectionOrchestrationClient,
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

            string queryId = req.RequestUri.ParseQueryString()["QueryId"];
            string queryName = req.RequestUri.ParseQueryString()["QueryName"];
            string notifyOrchestration = req.RequestUri.ParseQueryString()["NotifyOrchestration"];
            string asOfDateString = req.RequestUri.ParseQueryString()["AsOfDate"];
            DateTime? asOfDate = null;
            if (!string.IsNullOrWhiteSpace(asOfDateString))
            {
                DateTime dtOut;
                if (DateTime.TryParse(asOfDateString, out dtOut))
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
                int.TryParse(retryWaitString, out retryWait);
            }

            dynamic eventData = await req.Content.ReadAsAsync<object>();
            if (null != eventData)
            {
                queryId = queryId ?? eventData?.QueryId;
                queryName = queryName ?? eventData?.QueryName;
                asOfDate = asOfDate ?? eventData?.AsOfDate;
                notifyOrchestration = notifyOrchestration ?? eventData?.NotifyOrchestration;
            }

            if (!string.IsNullOrWhiteSpace(queryId))
            {
                Query_Projections_Projection_Request payload = new Query_Projections_Projection_Request()
                {
                    QueryName = queryName,
                    UniqueIdentifier = queryId,
                    AsOfDate = asOfDate,
                    CallbackOrchestrationIdentifier = notifyOrchestration
                };

                // call the orchestrator...
                string instanceId = await runProjectionOrchestrationClient.StartNewAsync("QueryProjectionProcessorOrchestrator", payload);


                #region Logging
                if (null != log)
                {
                    log.LogInformation($"Started QueryProjectionProcessorOrchestrator - instance id: {instanceId }");
                }
                #endregion

                // Wait for it to complete
                TimeSpan timeout = TimeSpan.FromSeconds(timeoutLength);
                TimeSpan retryInterval = TimeSpan.FromSeconds(retryWait);

                return await runProjectionOrchestrationClient.WaitForCompletionOrCreateCheckStatusResponseAsync(
                    req,
                    instanceId,
                    timeout,
                    retryInterval);

            }
            else
            {
                return req.CreateResponse(System.Net.HttpStatusCode.BadRequest, "Please pass a query name and query identifier in the query string or in the request body");
            }

        }

        [ApplicationName("The Long Run")]
        [FunctionName("QueryProjectionProcessorOrchestrator")]
        public static async Task<ActivityResponse> QueryProjectionProcessorOrchestrator(
             [OrchestrationTrigger] DurableOrchestrationContext context,
             Microsoft.Extensions.Logging.ILogger log)
        {

            ActivityResponse response = new ActivityResponse() { FunctionName = "QueryProjectionProcessorOrchestrator" };

            Query_Projections_Projection_Request request = context.GetInput<Query_Projections_Projection_Request>();
            
            if (null != request)
            {

                // get all the projection requests for the query
                List<Query_Projections_Projection_Return> allProjections = await context.CallActivityAsync<List<Query_Projections_Projection_Return>>("GetQueryProjectionsStatusProjectionActivity", request);
                if (null != allProjections)
                {
                    #region Logging
                    if (null != log)
                    {
                        log.LogInformation($"Query {request.QueryName}.{request.UniqueIdentifier} has {allProjections.Count} projections total ");
                    }
                    #endregion

                    // Run them - This should be done by fan-out/fan-in
                    List<Task<ProjectionResultsRecord<object >>> allProjectionTasks = new List<Task<ProjectionResultsRecord<object >>>();

                    // run all the outstanding projections in parallel
                    foreach (Query_Projections_Projection_Return projectionRequest in allProjections)
                    {
                        if (projectionRequest.ProjectionState == Query_Projections_Projection_Return.QueryProjectionState.Queued)
                        {
                            ProjectionRequest projRequest = new ProjectionRequest()
                            {
                                ParentRequestName = request.QueryName ,
                                CorrelationIdentifier = new Guid(request.UniqueIdentifier) ,
                                DomainName = projectionRequest.Projection.DomainName  ,
                                AggregateTypeName = projectionRequest.Projection.AggregateTypeName ,
                                EntityUniqueIdentifier = projectionRequest.Projection.InstanceKey,
                                AsOfDate = null,
                                ProjectionName = projectionRequest.Projection.ProjectionTypeName
                            };

                            // mark it as in-flight
                            response = await context.CallActivityAsync<ActivityResponse>("LogQueryProjectionInFlightActivity", projRequest);

                            if (null != response)
                            {
                                context.SetCustomStatus(response);
                            }

                            // and start running it...
                            allProjectionTasks.Add(context.CallActivityAsync<ProjectionResultsRecord<object>>("RunProjectionActivity", projRequest));
                        }

                       // TODO : Run the projections in parallel...
                       await  Task.WhenAll(allProjectionTasks);

                        // and save their results to the query 
                        foreach (var returnValue in allProjectionTasks)
                        {
                            response = await context.CallActivityAsync<ActivityResponse>("LogQueryProjectionResultActivity", returnValue ); 
                            if (null != response)
                            {
                                context.SetCustomStatus(response);
                            }
                        }
                    }
                }

                // when all done - trigger the calling orchestration to come out of hibernation
                if (! string.IsNullOrWhiteSpace(request.CallbackOrchestrationIdentifier ) )
                {
                      
                }

                
            }
            else
            {
                response.Message = $"Unable to read projection request data from context {context.InstanceId}";
                response.FatalError = true;
            }

            return response;
        }

        //RunProjectionActivity
        /// <summary>
        /// Run the specified projection and return the results to the caller orchestration
        /// </summary>
        /// <returns></returns>
        [ApplicationName("The Long Run")]
        [FunctionName("RunProjectionActivity")]
        public static async Task<ProjectionResultsRecord<object>> RunProjectionActivity(
            [ActivityTrigger] DurableActivityContext context,
            ILogger log)
        {

            ProjectionRequest request = context.GetInput<ProjectionRequest>();

            if (null != request )
            {

                Projection projectionEvents = new Projection(request.DomainName ,
                    request.AggregateTypeName ,
                    request.EntityUniqueIdentifier ,
                    request.ProjectionName );


                if (null != projectionEvents)
                {
                    // Get an instance of the projection we want to run....
                    CQRSAzure.EventSourcing.ProjectionBaseUntyped projectionToRun = Projection.CreateProjectionInstance(request.ProjectionName ) ;
                    
                    if (null != projectionToRun )
                    {
                        IProjectionResponse projectionResponse = await projectionEvents.ProcessAsync(projectionToRun); 
                        if (null != projectionResponse )
                        {

                            // make a response object
                            IEnumerable<object> projectionResultObjects = Projection.GetProjectionResults(projectionToRun.CurrentValues);

                            object results = null;
                            if (null != projectionResultObjects)
                            {
                                if (projectionResultObjects.Count() == 1)
                                {
                                    results = projectionResultObjects.FirstOrDefault();
                                }
                                else
                                {
                                    results = projectionResultObjects;
                                }
                            }

                            return new ProjectionResultsRecord<object>()
                            {
                                DomainName = request.DomainName,
                                AggregateTypeName = request.AggregateTypeName,
                                EntityUniqueIdentifier = request.EntityUniqueIdentifier,
                                CurrentAsOfDate = projectionResponse.AsOfDate.GetValueOrDefault(DateTime.UtcNow) ,
                                CorrelationIdentifier = request.CorrelationIdentifier,
                                ParentRequestName = request.ParentRequestName,
                                Result = results 
                            };
                        }
                        else
                        {
                            #region Logging
                            if (null != log)
                            {
                                log.LogWarning($"Unable to read projection request details from {context.InstanceId} ");
                            }
                            #endregion
                            return new ProjectionResultsRecord<object>() {
                                DomainName = request.DomainName,
                                AggregateTypeName = request.AggregateTypeName,
                                EntityUniqueIdentifier = request.EntityUniqueIdentifier ,
                                CurrentAsOfDate= request.AsOfDate.GetValueOrDefault(DateTime.UtcNow ),
                                CorrelationIdentifier= request.CorrelationIdentifier ,
                                ParentRequestName= request.ParentRequestName ,
                                Result = null 
                            };

                        }
                    }
                }

            }
            else
            {
                #region Logging
                if (null != log)
                {
                    log.LogWarning($"Unable to read projection request details from {context.InstanceId} ");
                }
                #endregion
            }

            // No results 
            return null;
        }


        /// <summary>
        /// Log the result from a projection projections that are needed to be run to answer this query
        /// </summary>
        [ApplicationName("The Long Run")]
        [DomainName(Constants.Domain_Query)]
        [FunctionName("LogQueryProjectionResultActivity")]
        public static async Task<ActivityResponse> LogQueryProjectionResultActivity(
            [ActivityTrigger] DurableActivityContext context,
            ILogger log
            )
        {

            CQRSAzure.EventSourcing.IWriteContext writeContext = null; // TODO: Pass this as a parameter

            ActivityResponse resp = new ActivityResponse() { FunctionName = "LogQueryProjectionResultActivity" };

            // get the ProjectionResultsRecord
            ProjectionResultsRecord<object> data = context.GetInput<ProjectionResultsRecord<object>>();
            if (null != data)
            {
                await QueryLogRecord.LogProjectionResult(data.CorrelationIdentifier,
                                                         data.ParentRequestName,
                                                         data.ProjectionName,
                                                         data.DomainName,
                                                         data.AggregateTypeName,
                                                         data.EntityUniqueIdentifier,
                                                         data.CurrentAsOfDate,
                                                         data.Result,
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


        [ApplicationName("The Long Run")]
        [DomainName(Constants.Domain_Query)]
        [FunctionName("LogQueryProjectionInFlightActivity")]
        public static async Task<ActivityResponse> LogQueryProjectionInFlightActivity(
            [ActivityTrigger] DurableActivityContext context,
            ILogger log)
        {

            // TODO: Pass this as a parameter
            CQRSAzure.EventSourcing.IWriteContext writeContext = null;

            ActivityResponse resp = new ActivityResponse() { FunctionName = "LogQueryProjectionInFlightActivity" };

            ProjectionRequest projectionRequest = context.GetInput<ProjectionRequest>();
            if (null != projectionRequest)
            {
                await QueryLogRecord.LogProjectionStarted(projectionRequest.CorrelationIdentifier,
                    projectionRequest.ParentRequestName,
                    projectionRequest.ProjectionName,
                    projectionRequest.DomainName,
                    projectionRequest.AggregateTypeName,
                    projectionRequest.EntityUniqueIdentifier,
                    projectionRequest.AsOfDate,
                    projectionRequest.CorrelationIdentifier.ToString(),
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
