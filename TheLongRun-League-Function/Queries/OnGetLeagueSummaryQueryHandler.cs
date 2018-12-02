
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

namespace TheLongRunLeaguesFunction.Queries
{
    public static partial class Query
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

            const string QUERY_NAME = @"get-league-summary";

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

                QueryRequest<Get_League_Summary_Definition> queryRequest = eventGridEvent.Data as QueryRequest<Get_League_Summary_Definition>;

#if FUNCTION_CHAINING
                // Get the query parameters : [League Name]

                if (null != queryRequest)
                {

                    // Create a new Query record to hold the event stream for this query...

                    QueryLogRecord<Get_League_Summary_Definition> qryRecord = QueryLogRecord<Get_League_Summary_Definition>.Create(
                        QUERY_NAME,
                        queryRequest.Parameters,
                        queryRequest.ReturnTarget,
                        queryRequest.ReturnPath);

                    if (null != qryRecord)
                    {
                #region Logging
                        if (null != log)
                        {
                            log.LogDebug ($"Query {QUERY_NAME} called for {queryRequest.Parameters.League_Name} - to return to {queryRequest.Parameters }",
                                source: "OnGetLeagueSummaryQuery");
                        }
                #endregion

                        EventStream queryEvents = new EventStream(@"Query",
                            QUERY_NAME ,
                            qryRecord.QueryUniqueIdentifier.ToString());

                        if (null != queryEvents )
                        {
                            // Log the query creation
                            await queryEvents.AppendEvent(new TheLongRun.Common.Events.Query.QueryCreated(QUERY_NAME ,
                                        qryRecord.QueryUniqueIdentifier ));

                            // set the parameters
                            await queryEvents.AppendEvent(new TheLongRun.Common.Events.Query.QueryParameterValueSet
                                (nameof(queryRequest.Parameters.League_Name ), queryRequest.Parameters.League_Name ));

                            // set the return target
                            await queryEvents.AppendEvent(new TheLongRun.Common.Events.Query.OutputLocationSet 
                                ( qryRecord.ReturnPath , qryRecord.ReturnTarget ));


                            // Call the next query in the command chain
                            FunctionChaining funcChain = new FunctionChaining(log);
                            var queryParams = new System.Collections.Generic.List<Tuple<string, string>>();
                            queryParams.Add (new Tuple<string, string>("queryId" , qryRecord.QueryUniqueIdentifier.ToString()) );
                            funcChain.TriggerCommandByHTTPS(@"Leagues", "GetLeagueSummaryQueryValidation", queryParams, null );  


            }

                    }
                    else
                    {
                #region Logging
                        if (null != log)
                        {
                            log.Error($"Query called with data that could not be converted to query log record",
                                source: "OnGetLeagueSummaryQuery");
                        }
                #endregion
                    }

                }
                else
                {
                #region Logging
                    if (null != log)
                    {
                        log.Error($"Query called with data that could not be converted to parameters",
                            source: "OnGetLeagueSummaryQuery");
                    }
                #endregion
                }

            // Log that this step has completed
            if (null != log)
            {
                log.LogInformation("Query passed on to handler from OnGetLeagueSummaryQuery");
            }

#else
                // Using Azure Deurable functions to do the command chaining
                string instanceId = await getLeagueSummaryQueryHandlerOrchestrationClient.StartNewAsync("OnGetLeagueSummaryQueryHandlerOrchestrator", queryRequest);

                log.LogInformation($"Started OnGetLeagueSummaryQueryHandlerOrchestrator orchestration with ID = '{instanceId}'.");


#endif
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
            QueryRequest<Get_League_Summary_Definition> queryRequest = context.GetInput <QueryRequest<Get_League_Summary_Definition>>();
            queryRequest.QueryName = "get-league-summary";

            if (null != queryRequest )
            {

                // Log the query request in its own own query event stream
                Guid queryId = await context.CallActivityAsync<Guid>("GetLeagueSummaryCreateQueryRequestActivity", queryRequest); 

                if (queryId.Equals(Guid.Empty )  )
                {
                    if (null != log)
                    {
                        // Unable to get the request details from the orchestration
                        log.LogError("OnGetLeagueSummaryQueryHandlerOrchestrator : Unable to create the query event stream");
                    }

                    return null;
                }
                else
                {
                    queryRequest.QueryUniqueIdentifier = queryId;
                    // Save the parameters to the event stream
                    await context.CallActivityAsync("GetLeagueSummaryLogParametersActivity", queryRequest);

                    // next validate the query
                    bool valid = await context.CallActivityAsync<bool>("GetLeagueSummaryValidateActivity", queryRequest);

                    if (! valid )
                    {
                        if (null != log)
                        {
                            // Could not run the query as the parameters don't make sense
                            log.LogError("OnGetLeagueSummaryQueryHandlerOrchestrator : Query parameters are invalid");
                        }
                        return null;
                    }
                    else
                    {
                        // Request all the projections needed to answer this query
                        await context.CallActivityAsync("GetLeagueSummaryQueryProjectionsRequestActivity", queryRequest);

                        // Run all the outstanding projections for this query
                        await context.CallActivityAsync("GetLeagueSummaryQueryProjectionProcessActivity", queryRequest);

                        // Output the results
                        await context.CallActivityAsync("GetLeagueSummaryOutputResultsActivity", queryRequest); 

                        // Get the results for ourselves to return...to do this the query must be complete...

                        return null;
                    }

                }

            }
            else
            {
                if (null != log)
                {
                    // Unable to get the request details from the orchestration
                    log.LogError("OnGetLeagueSummaryQueryHandlerOrchestrator : Unable to get the query request from the context");
                }

                return null;
            }

        }
    }
}
