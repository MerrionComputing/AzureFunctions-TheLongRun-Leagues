
using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.IO;

using Leagues.League.commandDefinition;
using TheLongRun.Common;
using TheLongRun.Common.Attributes;
using TheLongRun.Common.Bindings;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;
using TheLongRun.Common.Orchestration;
using System.Collections.Generic;
using TheLongRunLeaguesFunction.Commands.Notification;

namespace TheLongRunLeaguesFunction
{
    public static partial class SetLeagueEmailAddressCommandHandler
    {

        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [AggregateRoot("League")]
        [CommandName("Set League Email Address")]
        [EventTopicSourceName("Set-League-Email-Address-Command")]
        [FunctionName("OnSetLeagueEmailAddressCommandHandler")]
        public static async Task OnSetLeagueEmailAddressCommandHandler(
                            [EventGridTrigger] EventGridEvent eventGridEvent,
                            [OrchestrationClient] DurableOrchestrationClient SetLeagueEmailAddressOrchestrationClient,
                            ILogger log
                            )
        {

            #region Logging
            if (null != log)
            {
                log.LogDebug("Function triggered in OnSetLeagueEmailAddressCommand");
            }

            if (null == eventGridEvent)
            {
                // This function should not proceed if there is no event data
                if (null != log)
                {
                    log.LogError("Missing event grid trigger data in OnSetLeagueEmailAddressCommand");
                }
                return;
            }
            #endregion

            try
            {

                // Get the command request details out of the event grid data request
                var jsondata = JsonConvert.SerializeObject(eventGridEvent.Data);
                CommandRequest<Set_Email_Address_Definition> cmdRequest = null;
                if (!string.IsNullOrWhiteSpace(jsondata))
                {
                    cmdRequest = JsonConvert.DeserializeObject<CommandRequest<Set_Email_Address_Definition>>(jsondata);
                }

                // Log the parameters
                #region Logging
                if (null != log)
                {
                    if (null == cmdRequest)
                    {
                        log.LogDebug($"Unable to read parameters from {eventGridEvent.Data} in OnSetLeagueEmailAddressCommand");
                    }

                }
                #endregion


                if (null != cmdRequest)
                {
                    // Create a new command
                    // Make sure the command has a new identifier
                    if (cmdRequest.CommandUniqueIdentifier == Guid.Empty)
                    {
                        cmdRequest.CommandUniqueIdentifier = Guid.NewGuid();
                    }
                    if (string.IsNullOrWhiteSpace(cmdRequest.CommandName))
                    {
                        cmdRequest.CommandName = "Set League Email Address";
                    }

                    // Using Azure Durable functions to do the command chaining
                    string instanceId = await SetLeagueEmailAddressOrchestrationClient.StartNewAsync("SetLeagueEmailAddressCommandHandlerOrchestrator", cmdRequest);

                    log.LogInformation($"Run SetLeagueEmailAddressCommandHandlerOrchestrator orchestration with ID = '{instanceId}'.");

                }


            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
            }

        }


            
        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [AggregateRoot("League")]
        [CommandName("Set League Email Address")]
        [FunctionName("SetLeagueEmailAddressCommandHandlerOrchestrator") ]
        public static async Task SetLeagueEmailAddressCommandHandlerOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context,
            ILogger log)
        {

            CommandRequest<Set_Email_Address_Definition> cmdRequest = context.GetInput<CommandRequest<Set_Email_Address_Definition>>();

 
            if (null != cmdRequest)
            {
                ActivityResponse resp = await context.CallActivityWithRetryAsync<ActivityResponse>("SetLeagueEmailAddressCommandLogParametersActivity",
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

                // Get the impacted entities from the command request - in this command they are passed as parameters
                IEnumerable<CommandNotificationImpactedEntity> impactedEntities = null;
                Set_Email_Address_Definition parameters = cmdRequest.GetParameters();
                if (null != parameters )
                {
                    Tuple<string, string>[] entitiesImpacted = new Tuple<string, string>[] { new Tuple<string, string>(@"League", parameters.LeagueName ) };
                    impactedEntities = CommandNotificationImpactedEntity.CreateImpactedEntityList(entitiesImpacted); 
                }

                if (!resp.FatalError)
                {
                    // 1) Validate the command
                    bool valid = await context.CallActivityWithRetryAsync<bool>("SetLeagueEmailAddressCommandValidationActivity",
                        DomainSettings.CommandRetryOptions(),
                        cmdRequest);

                    CommandStepResponse stepValidateResponse = new CommandStepResponse()
                    {
                        CommandName = cmdRequest.CommandName,
                        CommandUniqueIdentifier = cmdRequest.CommandUniqueIdentifier,
                        StepName = "Set League Email Address Command Validation",
                        Message = resp.Message,
                        ImpactedEntities = impactedEntities
                    };
                    resp = await context.CallActivityAsync<ActivityResponse>("CommandStepCompleteActivity", stepValidateResponse );
                    

                    if (valid)
                    {
                        // 2) Perform the operation of the command
                        resp = await context.CallActivityWithRetryAsync <ActivityResponse>("SetLeagueEmailAddressCommandHandlerActivity",
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

                        //  Mark the step as complete
                        if (!resp.FatalError)
                        {
                            CommandStepResponse stepCommandResponse = new CommandStepResponse()
                            {
                                CommandName = cmdRequest.CommandName,
                                CommandUniqueIdentifier = cmdRequest.CommandUniqueIdentifier,
                                StepName = resp.FunctionName,
                                Message = resp.Message,
                                ImpactedEntities = impactedEntities
                            };
                            resp = await context.CallActivityAsync<ActivityResponse>("CommandStepCompleteActivity", stepCommandResponse);
                        }


                        // Mark the command as complete
                        if (!resp.FatalError)
                        {
                            resp = await context.CallActivityAsync<ActivityResponse>("CommandCompleteActivity", cmdRequest );
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

                        // Fire the orchestration to do the actual work of sending notifications
                        Command_Get_Notifications_Request payload = new Command_Get_Notifications_Request()
                        {
                            CommandName = cmdRequest.CommandName,
                            CommandUniqueIdentifier = cmdRequest.CommandUniqueIdentifier.ToString()
                        };

                        // call the orchestrator...
                        resp = await context.CallSubOrchestratorAsync<ActivityResponse>("CommandNotificationOrchestrator", payload);

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
                            log.LogWarning($"Command parameters not valid {cmdRequest.CommandName} : {cmdRequest.CommandUniqueIdentifier } ");
                        }
                        #endregion
                        if (null != resp)
                        {
                            resp.Message = $"Command parameters not valid {cmdRequest.CommandName} : {cmdRequest.CommandUniqueIdentifier } ";
                            context.SetCustomStatus(resp);
                        }
                    }
                }
            }
        }
    }
}
