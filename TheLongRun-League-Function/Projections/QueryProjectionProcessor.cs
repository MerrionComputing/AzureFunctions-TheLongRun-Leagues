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

            if (! string.IsNullOrWhiteSpace(queryId) )
            {
                Query_Projections_Projection_Request payload = new Query_Projections_Projection_Request()
                {
                    QueryName = queryName,
                    UniqueIdentifier = queryId,
                    AsOfDate = asOfDate,
                    CallbackOrchestrationIdentifier= notifyOrchestration
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

                // run dem


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


        /// <summary>
        /// Log the result from a projection projections that are needed to be run to answer this query
        /// </summary>
        [ApplicationName("The Long Run")]
        [DomainName(Constants.Domain_Query)]
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
