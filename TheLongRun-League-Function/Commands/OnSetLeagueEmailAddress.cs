
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

namespace TheLongRunLeaguesFunction
{
    public static partial class Command
    {

        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [AggregateRoot("League")]
        [CommandName("Set League Email Address")]
        [EventTopicSourceName("Set-League-Email-Address-Command")]
        [FunctionName("OnSetLeagueEmailAddressCommandHandler")]
        public static async void OnSetLeagueEmailAddressCommandHandler(
                            [EventGridTrigger] EventGridEvent eventGridEvent,
                            [OrchestrationClient] DurableOrchestrationClient setLeagueEmailAddressOrchestrationClient,
                            ILogger log
                            )
        {

            const string COMMAND_NAME = @"set-league-email-address";

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
                // Get the parameters etc out of the trigger and put them in the log record
                Set_Email_Address_Definition parameters = eventGridEvent.Data as Set_Email_Address_Definition;

                // Log the parameters
                #region Logging
                if (null != log)
                {
                    if (null == parameters)
                    {
                        log.LogDebug($"Unable to read parameters from {eventGridEvent.Data} in OnSetLeagueEmailAddressCommand");
                    }

                }
                #endregion

                CommandLogRecord<Set_Email_Address_Definition> cmdRecord = CommandLogRecord<Set_Email_Address_Definition>.Create(COMMAND_NAME,
                    parameters);

                EventStream commandEvents = new EventStream(@"Command",
                                                            COMMAND_NAME,
                                                            cmdRecord.CommandUniqueIdentifier.ToString());

                if (null != commandEvents)
                {
                    await commandEvents.AppendEvent(new TheLongRun.Common.Events.Command.CommandCreated(COMMAND_NAME,
                                                cmdRecord.CommandUniqueIdentifier));

                    // Log the parameters
                    #region Logging
                    if (null != log)
                    {
                        log.LogDebug($"Setting {nameof(parameters.LeagueName)} to { parameters.LeagueName} in OnSetLeagueEmailAddressCommand");
                    }
                    #endregion
                    await commandEvents.AppendEvent(new TheLongRun.Common.Events.Command.ParameterValueSet(nameof(parameters.LeagueName), parameters.LeagueName));
                    await commandEvents.AppendEvent(new TheLongRun.Common.Events.Command.ParameterValueSet(nameof(parameters.New_Email_Address ), parameters.New_Email_Address ));
                    await commandEvents.AppendEvent(new TheLongRun.Common.Events.Command.ParameterValueSet(nameof(parameters.Notes), parameters.Notes ));


                }

            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
            }

            // Log that this step has completed
            if (null != log)
            {
                log.LogDebug("Command passed on to handlerin OnSetLeagueEmailAddressCommand");
            }
        }

        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [AggregateRoot("League")]
        [CommandName("Set League Email Address")]
        [FunctionName("SetLeagueEmailAddressCommandHandlerSequence") ]
        public static async void SetLeagueEmailAddressCommandHandlerSequence(
            DurableOrchestrationContext commandOrchastrationContext,
            ILogger log)
        {
            Set_Email_Address_Definition parameters = commandOrchastrationContext.GetInput<Set_Email_Address_Definition>();
            string commandId = parameters.InstanceIdentifier.ToString();

            if (!string.IsNullOrWhiteSpace(commandId))
            {
                // Log that this step has completed
                if (null != log)
                {
                    log.LogDebug("Command passed on to handler sequence in SetLeagueEmailAddressCommandHandlerSequence");
                }

                // 1) Validate the command
                bool valid = await commandOrchastrationContext.CallActivityAsync<bool>("SetLeagueEmailAddressCommandValidationActivity", commandId);
                if (valid )
                {
                    // 2) Perform the action of the command
                    await commandOrchastrationContext.CallActivityAsync("SetLeagueEmailAddressCommandHandlerActivity", commandId); 
                }
            }
        }
    }
}
