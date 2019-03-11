using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using TheLongRun.Common.Attributes;
using System.Threading.Tasks;
using TheLongRun.Common.Orchestration;
using Microsoft.Extensions.Logging;
using TheLongRun.Common;
using System.Net.Http;
using Newtonsoft.Json;
using TheLongRun.Common.Bindings;
using TheLongRun.Common.Events.Query.Projections;
using System;
using System.Collections.Generic;

namespace TheLongRunLeaguesFunction.Queries.Output
{
    /// <summary>
    /// Different ways that the output from a query can be sent out from the query
    /// </summary>
    public static class QueryOutput
    {


        /// <summary>
        /// Log a specified output for the given query
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        [ApplicationName("The Long Run")]
        [FunctionName("QueryLogOutputTargetActivity")]
        public static async Task<ActivityResponse> QueryLogOutputTargetActivity(
            [ActivityTrigger] DurableActivityContext context,
            ILogger log)
        {

            ActivityResponse ret = new ActivityResponse() { FunctionName = "QueryLogOutputTargetActivity" };

            #region Logging
            if (null != log)
            {
                log.LogDebug($"Logging query output  in {ret.FunctionName} ");
            }
            #endregion

            QueryRequest<object> queryRequest = context.GetInput<QueryRequest<object>>();
            if (null != queryRequest )
            {
                EventStream queryEvents = new EventStream(Constants.Domain_Query,
                    queryRequest.QueryName,
                    queryRequest.QueryUniqueIdentifier.ToString());

                if (null != queryEvents)
                {

                    // Set the context for the events to be written using
                    queryEvents.SetContext(new WriteContext(ret.FunctionName, context.InstanceId));

                    // set the parameter(s)
                    await queryEvents.AppendEvent(new TheLongRun.Common.Events.Query.OutputLocationSet
                        (queryRequest.ReturnPath, queryRequest.ReturnTarget ));

                    if (null != log)
                    {
                        // Unable to get the request details from the orchestration
                        log.LogInformation($"{ret.FunctionName } : Set output path {queryRequest.ReturnPath} : {queryRequest.ReturnTarget} ");
                    }

                    ret.Message = $"Set output path {queryRequest.ReturnPath} : {queryRequest.ReturnTarget} ";
                }
                else
                {
                    ret.Message = $"Unable to get the event stream for {queryRequest.QueryName} : {queryRequest.QueryUniqueIdentifier }";
                    ret.FatalError = true;
                }
            }

            return ret;
        }

        /// <summary>
        /// Sub orchestration to output all the results from a query to all of the ouput locations registered for it
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        [ApplicationName("The Long Run")]
        [FunctionName("QueryOutputProcessorOrchestrator")]
        public static async Task<ActivityResponse> QueryOutputProcessorOrchestrator(
             [OrchestrationTrigger] DurableOrchestrationContext context,
             Microsoft.Extensions.Logging.ILogger log)
        {
            ActivityResponse response = new ActivityResponse() { FunctionName = "QueryOutputProcessorOrchestrator" };

            // Get the Query_Outputs_Request from the context...
            Query_Outputs_Request request = context.GetInput<Query_Outputs_Request>();

            if (null != request)
            {
                // Read the outputs for the given query
                Guid queryGuid;

                if (Guid.TryParse(request.UniqueIdentifier, out queryGuid))
                {
                    // Get the current state of the query...
                    Projection getQueryState = new Projection(Constants.Domain_Query,
                        request.QueryName,
                        queryGuid.ToString(),
                        nameof(Query_Summary_Projection));


                    if (null != getQueryState)
                    {
                        // Get all the output targets
                        Query_Outputs_Projection qryOutputs = new Query_Outputs_Projection(log);
                        await getQueryState.Process(qryOutputs);

                        if ((qryOutputs.CurrentSequenceNumber > 0) || (qryOutputs.ProjectionValuesChanged()))
                        {
                            #region Logging
                            if (null != log)
                            {
                                log.LogDebug($"Sending results to output targets from {request.QueryName} : {request.UniqueIdentifier} ");
                            }
                            #endregion

                            List<Task<ActivityResponse>> allOutputTasks = new List<Task<ActivityResponse>>();

                            foreach (string location in qryOutputs.WebhookTargets)
                            {
                                #region Logging
                                if (null != log)
                                {
                                    log.LogDebug($"Target : { location} - being sent by webhook in OutputResultsGetLeagueSummaryQuery");
                                }
                                #endregion

                                if (null != request.Results)
                                {
                                    // Create a QueryOutputRecord<object>
                                    QueryOutputRecord<object> outputRequest = QueryOutputRecord<object>.Create(request.Results,
                                        location,
                                        request.QueryName,
                                        queryGuid);

                                    // add a task to ouputit it via webhook....
                                    allOutputTasks.Add(context.CallActivityWithRetryAsync<ActivityResponse>("QueryOutputToWebhookActivity",
                                            DomainSettings.QueryRetryOptions(),
                                            outputRequest));
                                }
                            }


                            // TODO: All the other output methods

                            // Await for all the outputs to have run in parallel...
                            await Task.WhenAll(allOutputTasks);

                            foreach (var returnedResponse in allOutputTasks)
                            {
                                if (returnedResponse.Result.FatalError )
                                {
                                    response.FatalError = true;
                                    response.Message = returnedResponse.Result.Message;
                                }

                                #region Logging
                                if (null != log)
                                {
                                    log.LogDebug($"Sent results to output targets from {returnedResponse.Result.FunctionName} : {returnedResponse.Result.Message } ");
                                }
                                #endregion
                                context.SetCustomStatus(returnedResponse.Result);
                            }

                        }
                    }
                }
            }
            else
            {
                response.Message = $"Unable to get outputs request details in sub orchestration {context.InstanceId} ";
                response.FatalError = true;
            }

            return response;
        }

        /// <summary>
        /// Run the specified projection and return the results to the caller orchestration
        /// </summary>
        /// <remarks>
        /// The query identifier and results are 
        /// </remarks>
        [ApplicationName("The Long Run")]
        [FunctionName("QueryOutputToWebhookActivity")]
        public static async Task<ActivityResponse> QueryOutputToWebhookActivity(
            [ActivityTrigger] DurableActivityContext context,
            ILogger log)
        {

            ActivityResponse ret = new ActivityResponse() { FunctionName = "QueryOutputToWebhookActivity" };

            #region Logging
            if (null != log)
            {
                log.LogDebug($"Output  in {ret.FunctionName} ");
            }
            #endregion

            // Read the results 
            QueryOutputRecord<object> results = context.GetInput<QueryOutputRecord<object>>(); 
            if (null != results )
            {
                var payloadAsJSON = new StringContent(JsonConvert.SerializeObject(results.Results));
                using (var client = new HttpClient())
                {
                    var response = await client.PostAsync(results.Target, payloadAsJSON);
                    if (response.IsSuccessStatusCode  )
                    {
                        ret.Message = $"Transfer succeeded {response.StatusCode} ";
                    }
                    else
                    {
                        // We don't mark this as a fatal error as we want any successful sends to go ahead anyway
                        ret.Message = $"Webhook transfer failed {response.StatusCode} - {response.ReasonPhrase} ";
                    }
                }
            }

            return ret;

        }



    }
}
