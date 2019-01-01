
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
                ActivityResponse resp = await context.CallActivityAsync<ActivityResponse>("SetLeagueEmailAddressCommandLogParametersActivity", cmdRequest);
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
                    // 1) Validate the command
                    bool valid = await context.CallActivityAsync<bool>("SetLeagueEmailAddressCommandValidationActivity", cmdRequest);
                    if (valid)
                    {
                        // 2) Perform the operation of the command
                        resp = await context.CallActivityAsync<ActivityResponse>("SetLeagueEmailAddressCommandHandlerActivity", cmdRequest);
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
                    }
                }
            }
            else
            {

            }
        }
    }
}
