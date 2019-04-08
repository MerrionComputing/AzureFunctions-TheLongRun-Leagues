using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Host;
using System;


using Leagues.League.commandDefinition;
using TheLongRun.Common;
using TheLongRun.Common.Attributes;
using TheLongRun.Common.Bindings;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;
using TheLongRun.Common.Orchestration;
using System.Collections.Generic;

namespace TheLongRunLeaguesFunction
{
    public static partial class Command
    {

        /// <summary>
        /// Command handler for the [Create League] command - takes the command, makes an appropriate command log
        /// entry and drops it into the command-log path
        /// </summary>
        /// <param name="eventGridEvent">
        /// The event from the event grid that caused this command to execute
        /// </param>
        /// <param name="commandLog">
        /// The file to log the command request and its parameters to
        /// command-log/{commandName}-{commandGUID}.cmd
        /// </param>
        /// <param name="log">
        /// Log for any execution tracing
        /// </param>
        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [AggregateRoot("League")]
        [CommandName("Create League")]
        [EventTopicSourceName("Create-New-League-Command")]
        [FunctionName("OnCreateLeagueCommand")]
        public static async Task OnCreateLeagueCommand(
            [EventGridTrigger] EventGridEvent eventGridEvent,
            [OrchestrationClient] DurableOrchestrationClient createLeagueCommandHandlerOrchestrationClient,
            ILogger log
            )
        {
            #region Logging
            if (null != log)
            {
                log.LogDebug("Function triggered in OnCreateLeagueCommand");
            }

            if (null == eventGridEvent)
            {
                // This function should not proceed if there is no event data
                if (null != log)
                {
                    log.LogError("Missing event grid trigger data in OnCreateLeagueCommand");
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
                    if (null == eventGridEvent.Data)
                    {
                        log.LogError($"The query parameter has no values in OnGetLeagueSummaryQuery");
                        return;
                    }
                }
                #endregion

                // Get the command request details out of the event grid data request
                var jsondata = JsonConvert.SerializeObject(eventGridEvent.Data);
                CommandRequest<Create_New_League_Definition> cmdRequest = null;
                if (!string.IsNullOrWhiteSpace(jsondata))
                {
                    cmdRequest = JsonConvert.DeserializeObject<CommandRequest<Create_New_League_Definition>>(jsondata);
                }

                if (null != cmdRequest)
                {
                    // Create a new command
                    // Make sure the command has a new identifier
                    if (cmdRequest.CommandUniqueIdentifier == Guid.Empty)
                    {
                        cmdRequest.CommandUniqueIdentifier = Guid.NewGuid();
                    }

                    // Using Azure Durable functions to do the command chaining
                    string instanceId = await createLeagueCommandHandlerOrchestrationClient.StartNewAsync("OnCreateLeagueCommandHandlerOrchestrator", cmdRequest);

                    log.LogInformation($"Run OnCreateLeagueCommandHandlerOrchestrator orchestration with ID = '{instanceId}'.");

                }
                else
                {
                    if (null != log)
                    {
                        log.LogError($"Unable to read command request details from {eventGridEvent.Data}");
                    }
                }


            }
            catch (Exception ex)
            {
                if (null != log)
                {
                    log.LogError($"Unable to execute  OnCreateLeagueCommand : {ex.Message }");
                }
            }
        }

        //OnCreateLeagueCommandHandlerOrchestrator
        /// <summary>
        /// The orchestration function for running a "create new league" command as an azure durable function
        /// with that orchestration
        /// </summary>
        /// <param name="context">
        /// The orchestration context the query is being executed under
        /// </param>
        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [AggregateRoot("League")]
        [CommandName("Create League")]
        [FunctionName("OnCreateLeagueCommandHandlerOrchestrator")]
        public static async Task OnCreateLeagueCommandHandlerOrchestrator
            ([OrchestrationTrigger] DurableOrchestrationContext context,
            Microsoft.Extensions.Logging.ILogger log)
        {

            CommandRequest<Create_New_League_Definition> cmdRequest = context.GetInput<CommandRequest<Create_New_League_Definition>>();

            if (null != cmdRequest)
            {
                ActivityResponse resp = await context.CallActivityWithRetryAsync<ActivityResponse>("CreateLeagueCommandLogParametersActivity",
                    DomainSettings.CommandRetryOptions(),
                    cmdRequest);

                Create_New_League_Definition parameters = cmdRequest.GetParameters();
                IEnumerable<CommandNotificationImpactedEntity> impactedEntities = null;
                if (null != parameters)
                {
                    Tuple<string, string>[] entitiesImpacted = new Tuple<string, string>[] { new Tuple<string, string>(@"League", parameters.LeagueName) };
                    impactedEntities = CommandNotificationImpactedEntity.CreateImpactedEntityList(entitiesImpacted);
                }

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

                if (!resp.FatalError)
                {
                    // validate the command
                    bool valid = await context.CallActivityWithRetryAsync<bool>("CreateLeagueCommandValidationActivity",
                        DomainSettings.CommandRetryOptions(),
                        cmdRequest);

                    if (!valid)
                    {
                        resp.Message = $"Validation failed for command {cmdRequest.CommandName} id: {cmdRequest.CommandUniqueIdentifier }";
                        resp.FatalError = true;
                    }
                }

                if (!resp.FatalError)
                {
                    CommandStepResponse stepResponse = new CommandStepResponse()
                    {
                        CommandName = cmdRequest.CommandName,
                        CommandUniqueIdentifier = cmdRequest.CommandUniqueIdentifier,
                        StepName = resp.FunctionName,
                        Message = resp.Message,
                        ImpactedEntities = impactedEntities
                    };
                    resp = await context.CallActivityAsync<ActivityResponse>("CommandStepCompleteActivity", stepResponse);
                }

                if (!resp.FatalError)
                {
                    // execute the command
                    resp = await context.CallActivityWithRetryAsync<ActivityResponse>("CreateLeagueCommandHandlerAction",
                        DomainSettings.CommandRetryOptions(),
                        cmdRequest);

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

                if (!resp.FatalError)
                {
                    // 3) Mark the step as complete
                    CommandStepResponse stepResponse = new CommandStepResponse()
                    {
                        CommandName = cmdRequest.CommandName,
                        CommandUniqueIdentifier = cmdRequest.CommandUniqueIdentifier,
                        StepName = resp.FunctionName,
                        Message = resp.Message,
                        ImpactedEntities = impactedEntities
                    };
                    resp = await context.CallActivityAsync<ActivityResponse>("CommandStepCompleteActivity", stepResponse);
                }

                if (!resp.FatalError)
                {
                    resp = await context.CallActivityAsync<ActivityResponse>("CommandCompleteActivity", cmdRequest);
                }

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
            else
            {
                #region Logging
                if (null != log)
                {
                    // Unable to get the request details from the orchestration
                    log.LogError("OnCreateLeagueCommandHandlerOrchestrator : Unable to get the command request from the context");

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
                #endregion
                return;
            }
        }
    }
}
