
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.IO;

using Leagues.League.queryDefinition;
using TheLongRun.Common;
using TheLongRun.Common.Attributes;
using TheLongRun.Common.Bindings;

using Newtonsoft.Json.Linq;
using Microsoft.Azure.EventGrid.Models;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TheLongRun.Common.Orchestration;

namespace TheLongRunLeaguesFunction.Queries
{
    public static partial class GetLeagueSummaryQuery
    {

        /// <summary>
        /// The top level handler for the "Get League Summary" query.  
        /// This is triggered by an event grid event and uses durable function orchestration
        /// to perform the steps required to fulfil that query
        /// </summary>
        /// <param name="eventGridEvent">
        /// The event grid trigger that started the query processing
        /// </param>
        /// <param name="getLeagueSummaryQueryHandlerOrchestrationClient">
        /// The durable function orchestration client that is used to call out to the other functions that are
        /// involved in processing this query
        /// </param>
        /// <param name="log">
        /// Log output to write log messages to
        /// </param>
        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [AggregateRoot("League")]
        [QueryName("Get League Summary")]
        [EventTopicSourceName("Get-League-Summary-Query")]
        [FunctionName("OnGetLeagueSummaryQueryHandler")]
        public static async Task OnGetLeagueSummaryQueryHandler(
            [EventGridTrigger] EventGridEvent eventGridEvent,
            [OrchestrationClient] DurableOrchestrationClient getLeagueSummaryQueryHandlerOrchestrationClient,
            Microsoft.Extensions.Logging.ILogger log
            )
        {

            #region Logging
            if (null != log)
            {
                log.LogDebug("Function triggered in OnGetLeagueSummaryQuery");
                
            }

            if (null == eventGridEvent)
            {
                // This function should not proceed if there is no event data
                if (null != log)
                {
                    log.LogError("Missing event grid trigger data in OnGetLeagueSummaryQuery");
                }
                return;
            }
            else
            {
                if (null != log)
                {
                    log.LogDebug($"Event grid topic: {eventGridEvent.Topic}");
                    log.LogDebug($"Event grid subject: {eventGridEvent.Subject }");
                    log.LogDebug($"Event grid metadata version: {eventGridEvent.MetadataVersion }");
                    log.LogDebug($"Event grid event type: {eventGridEvent.EventType }");
                    log.LogDebug($"Event Grid Data : {eventGridEvent.Data }");
                }
            }
            #endregion

            try
            {
                #region Logging
                if (null != log)
                {
                    log.LogDebug($"Get the query parameters in OnGetLeagueSummaryQuery");
                    if (null == eventGridEvent.Data )
                    {
                        log.LogError($"The query parameter has no values in OnGetLeagueSummaryQuery");
                        return;
                    }
                }
                #endregion

                // Get the query request details out of the event grid data request
                var jsondata = JsonConvert.SerializeObject(eventGridEvent.Data);
                QueryRequest<Get_League_Summary_Definition> queryRequest = null;
                if (!string.IsNullOrWhiteSpace(jsondata))
                {
                    queryRequest = JsonConvert.DeserializeObject<QueryRequest<Get_League_Summary_Definition>>(jsondata);
                }

                if (null != queryRequest)
                {
                    log.LogInformation($"Running query handler with durable functions orchestration");
                    log.LogInformation($"{queryRequest.QueryName} with league {queryRequest.GetParameters().League_Name}");

                    // Using Azure Durable functions to do the query chaining
                    string instanceId = await getLeagueSummaryQueryHandlerOrchestrationClient.StartNewAsync("OnGetLeagueSummaryQueryHandlerOrchestrator", queryRequest);

                    log.LogInformation($"Started OnGetLeagueSummaryQueryHandlerOrchestrator orchestration with ID = '{instanceId}'.");
                }
                else
                {
                    if (null != log)
                    {
                        log.LogError($"Unable to read query request from eventgrid data: {eventGridEvent.Data} Type: {eventGridEvent.Data.GetType()} ");
                    }
                }
            }
            catch (Exception ex)
            {
                if (null != log)
                {
                    log.LogError(ex.ToString(), ex);
                }
                throw;
            }


        }


        /// <summary>
        /// The orchestration function for running a "get league summary" query as an azure durable function
        /// with that orchestration
        /// </summary>
        /// <param name="context">
        /// The orchestration context the query is being executed under
        /// </param>
        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [AggregateRoot("League")]
        [QueryName("Get League Summary")]
        [EventTopicSourceName("Get-League-Summary-Query")]
        [FunctionName("OnGetLeagueSummaryQueryHandlerOrchestrator")]
        public static async Task<Get_League_Summary_Definition_Return> OnGetLeagueSummaryQueryHandlerOrchestrator
            ([OrchestrationTrigger] DurableOrchestrationContext context,
            Microsoft.Extensions.Logging.ILogger log)
        {

            // Get the query definition form the context...
            QueryRequest<Get_League_Summary_Definition> queryRequest = context.GetInput<QueryRequest<Get_League_Summary_Definition>>();

            try
            {
                if (null != queryRequest)
                {
                    if (string.IsNullOrWhiteSpace(queryRequest.QueryName))
                    {
                        queryRequest.QueryName = "Get League Summary";
                    }

                    // Log the query request in its own own query event stream
                    Guid queryId = await context.CallActivityAsync<Guid>("GetLeagueSummaryCreateQueryRequestActivity", queryRequest);

                    if (queryId.Equals(Guid.Empty))
                    {
                        #region Logging
                        if (null != log)
                        {
                            // Unable to get the request details from the orchestration
                            log.LogError("OnGetLeagueSummaryQueryHandlerOrchestrator : Unable to create the query event stream");
                        }
                        #endregion

                        return null;
                    }
                    else
                    {
                        queryRequest.QueryUniqueIdentifier = queryId;
                        // Save the parameters to the event stream
                        ActivityResponse resp = await context.CallActivityAsync<ActivityResponse>("GetLeagueSummaryLogParametersActivity", queryRequest);

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

                        // next validate the query
                        bool valid = await context.CallActivityAsync<bool>("GetLeagueSummaryValidateActivity", queryRequest);

                        if (!valid)
                        {
                            #region Logging
                            if (null != log)
                            {
                                // Could not run the query as the parameters don't make sense
                                log.LogError($"OnGetLeagueSummaryQueryHandlerOrchestrator : Query parameters are invalid {queryId}");
                            }
                            #endregion
                            return null;
                        }
                        else
                        {
                            // Request all the projections needed to answer this query
                            resp = await context.CallActivityAsync<ActivityResponse>("GetLeagueSummaryQueryProjectionRequestActivity", queryRequest);
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
                                if (resp.FatalError)
                                {
                                    #region Logging
                                    if (null != log)
                                    {
                                        log.LogError($"Fatal error in {resp.FunctionName} - {resp.Message} ");
                                    }
                                    #endregion
                                    return null;
                                }
                            }


                            // Run all the outstanding projections for this query
                            resp = await context.CallActivityAsync<ActivityResponse>("GetLeagueSummaryQueryProjectionProcessActivity", queryRequest);
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
                                if (resp.FatalError)
                                {
                                    #region Logging
                                    if (null != log)
                                    {
                                        log.LogError($"Fatal error in {resp.FunctionName} - {resp.Message} ");
                                    }
                                    #endregion
                                    return null;
                                }
                            }

                            // Output the results
                            resp = await context.CallActivityAsync<ActivityResponse>("GetLeagueSummaryOutputResultsActivity", queryRequest);
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
                                if (resp.FatalError)
                                {
                                    #region Logging
                                    if (null != log)
                                    {
                                        log.LogError($"Fatal error in {resp.FunctionName} - {resp.Message} ");
                                    }
                                    #endregion
                                    return null;
                                }
                            }

                            // Get the results for ourselves to return...to do this the query must be complete...
                            return await context.CallActivityAsync<Get_League_Summary_Definition_Return>("GetLeagueSummaryGetResultsActivity", queryRequest);
                        }

                    }

                }
                else
                {
                    if (null != log)
                    {
                        // Unable to get the request details from the orchestration
                        log.LogError("OnGetLeagueSummaryQueryHandlerOrchestrator : Unable to get the query request from the context");

                        string contextAsString = context.GetInput<string>();
                        if (!string.IsNullOrWhiteSpace(contextAsString))
                        {
                            log.LogError($"Context was {contextAsString} ");
                        }
                        else
                        {
                            log.LogError($"Context was blank ");
                        }

                    }

                    return null;
                }
            }
            catch (Exception ex)
            {
                #region Logging
                if (null != log)
                {
                    // Error running thew orchestration
                    log.LogError($"OnGetLeagueSummaryQueryHandlerOrchestrator : Error {ex.Message}");
                }
                #endregion
                if (null != context )
                {
                    context.SetCustomStatus($"OnGetLeagueSummaryQueryHandlerOrchestrator : Error {ex.Message}");
                }
                throw;
            }
        }
    }
}
