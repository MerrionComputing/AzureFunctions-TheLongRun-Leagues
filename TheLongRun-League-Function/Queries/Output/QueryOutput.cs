using Microsoft.Azure.WebJobs;
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
using System.IO;

using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.EventGrid.Models;

using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using TheLongRun.Common.Attributes.Settings;

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
            if (null != queryRequest)
            {
                EventStream queryEvents = EventStream.Create(Constants.Domain_Query,
                    queryRequest.QueryName,
                    queryRequest.QueryUniqueIdentifier.ToString());

                if (null != queryEvents)
                {

                    // Set the context for the events to be written using
                    queryEvents.SetContext(new WriteContext(ret.FunctionName, context.InstanceId));

                    if (null != queryRequest.ResponseTargets)
                    {
                        foreach (var responseTarget in queryRequest.ResponseTargets)
                        {


                            // set the parameter(s)
                            await queryEvents.AppendEvent(new TheLongRun.Common.Events.Query.OutputLocationSet
                                (responseTarget.ReturnPath, responseTarget.ReturnTarget));

                            #region Logging
                            if (null != log)
                            {
                                // Unable to get the request details from the orchestration
                                log.LogInformation($"{ret.FunctionName } : Set output path {responseTarget.ReturnPath} : {responseTarget.ReturnTarget} ");
                            }
                            #endregion
                        }

                    }
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

                            if (null != request.Results)
                            {
                                // Create a QueryOutputRecord<object>
                                QueryOutputRecord<object> outputRequest = QueryOutputRecord<object>.Create(request.Results,
                                    @"",
                                    request.QueryName,
                                    queryGuid);

                                foreach (string location in qryOutputs.WebhookTargets)
                                {
                                    #region Logging
                                    if (null != log)
                                    {
                                        log.LogDebug($"Target : { location} - being sent by webhook in OutputResultsGetLeagueSummaryQuery");
                                    }
                                    #endregion

                                    outputRequest.Target = location;

                                    // add a task to ouputit it via webhook....
                                    allOutputTasks.Add(context.CallActivityWithRetryAsync<ActivityResponse>("QueryOutputToWebhookActivity",
                                            DomainSettings.QueryRetryOptions(),
                                            outputRequest));

                                }

                                foreach (string location in qryOutputs.BlobTargets)
                                {
                                    #region Logging
                                    if (null != log)
                                    {
                                        log.LogDebug($"Target : { location} - being persisted to a Blob in {response.FunctionName}");
                                    }
                                    #endregion

                                    outputRequest.Target = location;

                                    // add a task to ouputit it via webhook....
                                    allOutputTasks.Add(context.CallActivityWithRetryAsync<ActivityResponse>("QueryOutputToBlobActivity",
                                            DomainSettings.QueryRetryOptions(),
                                            outputRequest));

                                }

                                foreach (string location in qryOutputs.ServiceBusTargets)
                                {
                                    #region Logging
                                    if (null != log)
                                    {
                                        log.LogDebug($"Target : { location} - being sent out via service bus in {response.FunctionName}");
                                    }
                                    #endregion

                                    outputRequest.Target = location;

                                    // add a task to ouputit it via service bus....
                                    allOutputTasks.Add(context.CallActivityWithRetryAsync<ActivityResponse>("QueryOutputToServiceBusActivity",
                                            DomainSettings.QueryRetryOptions(),
                                            outputRequest));

                                }


                                //EventGridTargets
                                foreach (string location in qryOutputs.EventGridTargets)
                                {
                                    #region Logging
                                    if (null != log)
                                    {
                                        log.LogDebug($"Target : { location} - being sent out via event grid in {response.FunctionName}");
                                    }
                                    #endregion

                                    outputRequest.Target = location;

                                    // add a task to ouputit it via event grid....
                                    allOutputTasks.Add(context.CallActivityWithRetryAsync<ActivityResponse>("QueryOutputToEventGridActivity",
                                            DomainSettings.QueryRetryOptions(),
                                            outputRequest));

                                }


                                foreach (string location in qryOutputs.SignalRTargets)
                                {
                                    #region Logging
                                    if (null != log)
                                    {
                                        log.LogDebug($"Target : { location} - being sent out via SignalR in {response.FunctionName}");
                                    }
                                    #endregion

                                    outputRequest.Target = location;

                                    // add a task to ouputit it via SignalR....
                                    allOutputTasks.Add(context.CallActivityWithRetryAsync<ActivityResponse>("QueryOutputToSignalRActivity",
                                            DomainSettings.QueryRetryOptions(),
                                            outputRequest));

                                }

                                // TODO: All the other output methods
                            }

                            // Await for all the outputs to have run in parallel...
                            await Task.WhenAll(allOutputTasks);

                            foreach (var returnedResponse in allOutputTasks)
                            {
                                if (returnedResponse.Result.FatalError)
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
        /// Sends the query output to a specified webhook target as a JSON object
        /// </summary>
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
            if (null != results)
            {
                var payloadAsJSON = new StringContent(JsonConvert.SerializeObject(results.Results));


                // TODO : Use the binder to bind to an HTTP client to send the results to
                using (var client = new HttpClient())
                {
                    HttpResponseMessage msgResp = await client.PostAsync(results.Target, payloadAsJSON);
                    if (null != msgResp)
                    {
                        ret.Message = $"Output sent - {msgResp.ReasonPhrase}";
                    }
                }

            }
            else
            {
                ret.Message = $"Unable to get query output record to send to web hook";
            }

            return ret;

        }

        /// <summary>
        /// Send the query outputs to the named BLOB target
        /// </summary>
        [ApplicationName("The Long Run")]
        [FunctionName("QueryOutputToBlobActivity")]
        public static async Task<ActivityResponse> QueryOutputToBlobActivity(
            [ActivityTrigger] DurableActivityContext context,
            Binder outputBinder,
            ILogger log)
        {

            ActivityResponse ret = new ActivityResponse() { FunctionName = "QueryOutputToBlobActivity" };

            #region Logging
            if (null != log)
            {
                log.LogDebug($"Output  in {ret.FunctionName} ");
            }
            #endregion

            // Read the results 
            QueryOutputRecord<object> results = context.GetInput<QueryOutputRecord<object>>();
            if (null != results)
            {
                var payloadAsJSON = JsonConvert.SerializeObject(results.Results);

                // Use the binder to bind to an blob client to send the results to
                using (var writer = outputBinder.Bind<TextWriter>(new BlobAttribute(results.Target)))
                {
                    await writer.WriteAsync(payloadAsJSON);
                }
            }
            else
            {
                ret.Message = $"Unable to get query output record to write to Blob";
            }

            return ret;

        }


        /// <summary>
        /// Send the query outputs to the named service bus queue target
        /// </summary>
        [ApplicationName("The Long Run")]
        [FunctionName("QueryOutputToServiceBusActivity")]
        public static async Task<ActivityResponse> QueryOutputToServiceBusActivity(
            [ActivityTrigger] DurableActivityContext context,
            Binder outputBinder,
            ILogger log)
        {

            ActivityResponse ret = new ActivityResponse() { FunctionName = "QueryOutputToServiceBusActivity" };

            #region Logging
            if (null != log)
            {
                log.LogDebug($"Output  in {ret.FunctionName} ");
            }
            #endregion

            // Read the results 
            QueryOutputRecord<object> results = context.GetInput<QueryOutputRecord<object>>();
            if (null != results)
            {
                var queueAttribute = new ServiceBusAttribute(results.Target);

                var payloadAsJSON = JsonConvert.SerializeObject(results.Results);

                using (var writer = outputBinder.Bind<TextWriter>(queueAttribute))
                {
                    await writer.WriteAsync(payloadAsJSON);
                }
            }
            else
            {
                ret.Message = $"Unable to get query output record to send to Service Bus";
            }

            return ret;
        }

        /// <summary>
        /// Send the query outputs to the named event grid target
        /// </summary>
        [ApplicationName("The Long Run")]
        [FunctionName("QueryOutputToEventGridActivity")]
        public static async Task<ActivityResponse> QueryOutputToEventGridActivity(
            [ActivityTrigger] DurableActivityContext context,
            Binder outputBinder,
            ILogger log)
        {

            ActivityResponse ret = new ActivityResponse() { FunctionName = "QueryOutputToEventGridActivity" };

            #region Logging
            if (null != log)
            {
                log.LogDebug($"Output  in {ret.FunctionName} ");
            }
            #endregion

            // Read the results 
            QueryOutputRecord<object> results = context.GetInput<QueryOutputRecord<object>>();
            if (null != results)
            {
                EventGridAttribute egAttribute = EventGridAttributeFromTarget(results.Target, results.QueryName, results.QueryUniqueIdentifier);

                if (null != egAttribute)
                {
                    // split the target string into an event grid attribute 

                    Microsoft.Azure.EventGrid.Models.EventGridEvent eventGridEvent = new Microsoft.Azure.EventGrid.Models.EventGridEvent()
                    {
                        Subject = results.QueryUniqueIdentifier.ToString(),
                        Data = results.Results
                    };

                    IAsyncCollector<EventGridEvent> eventCollector = outputBinder.Bind<IAsyncCollector<EventGridEvent>>(egAttribute);
                    if (null != eventCollector)
                    {
                        await eventCollector.AddAsync(eventGridEvent);
                        await eventCollector.FlushAsync();
                    }
                }
                else
                {
                    ret.Message = $"Unable to determine the event grid target from {results.Target}";
                }
            }
            else
            {
                ret.Message = $"Unable to get query output record to send to event grid";
            }

            return ret;
        }

        /// <summary>
        /// Turn a query output target into the event grid attribute to which it will be sent
        /// </summary>
        public static EventGridAttribute EventGridAttributeFromTarget(string target,
            string queryName,
            Guid queryUniqueIdentifier)
        {
            if (!string.IsNullOrWhiteSpace(target))
            {
                EventGridAttribute ret = new EventGridAttribute();
                ret.TopicEndpointUri = target;
                ret.TopicKeySetting = EventStream.MakeEventStreamName(queryName);
            }

            // If we reach the end with not enough info to create an event grid target return null
            return null;
        }


        //QueryOutputToSignalRActivity
        /// <summary>
        /// Send the query outputs to the SignalR target grid target
        /// </summary>
        [ApplicationName("The Long Run")]
        [FunctionName("QueryOutputToSignalRActivity")]
        public static async Task<ActivityResponse> QueryOutputToSignalRActivity(
            [ActivityTrigger] DurableActivityContext context,
            Binder outputBinder,
            ILogger log)
        {
            ActivityResponse ret = new ActivityResponse() { FunctionName = "QueryOutputToSignalRActivity" };

            #region Logging
            if (null != log)
            {
                log.LogDebug($"Output  in {ret.FunctionName} ");
            }
            #endregion

            // Read the results 
            QueryOutputRecord<object> results = context.GetInput<QueryOutputRecord<object>>();
            if (null != results)
            {
                SignalRAttribute signalRAttribute = SignalRAttributeFromTarget(results.Target);

                IAsyncCollector<SignalRMessage> eventCollector = outputBinder.Bind<IAsyncCollector<SignalRMessage>>(signalRAttribute);

                // Create and add a SignalRMessage
                if (null != eventCollector)
                {
                    // Make a SignalR message for the query results - note that we pass the entire results structure so
                    // the recipeint gets the context as well as the results data
                    SignalRMessage queryMessage = new SignalRMessage()
                    {
                        Target = results.QueryName ,
                        Arguments = new object[] { results }
                    };

                    await eventCollector.AddAsync(queryMessage);
                    // and flush the message out
                    await eventCollector.FlushAsync();
                }
            }
            else
            {
                ret.Message = $"Unable to get query output record to send to event grid";
            }

            return ret;

        }

        private static SignalRAttribute SignalRAttributeFromTarget(string target)
        {
            if (!string.IsNullOrWhiteSpace(target))
            {
                SignalRAttribute ret = new SignalRAttribute()
                {
                    // Look up the connection string from the hub name
                    ConnectionStringSetting= ConnectionStringNameAttribute.DefaultConnectionStringName(Constants.Domain_Query   , target),
                    HubName = target  
                };
                return ret;
            }

            return null;
        }
    }

}
