using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.IO;


using Leagues.League.commandDefinition;
using TheLongRun.Common;
using TheLongRun.Common.Attributes;
using TheLongRun.Common.Bindings;

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
        [EventTopicSourceName("Create-New-League-Command") ]
        [FunctionName("OnCreateLeagueCommand")]
        public static async void OnCreateLeagueCommand(
            [EventGridTrigger] EventGridEvent eventGridEvent,
            TraceWriter log
            )
        {

            const string COMMAND_NAME = @"create-league";

            #region Logging
            if (null != log)
            {
                log.Verbose("Function triggered ",
                    source: "OnCreateLeagueCommand");
            }

            if (null == eventGridEvent )
            {
                // This function should not proceed if there is no event data
                if (null != log)
                {
                    log.Error("Missing event grid trigger data",
                        source: "OnCreateLeagueCommand"); 
                }
                return;
            }
            #endregion

            try
            {
                // Get the parameters etc out of the trigger and put them in the log record
                Create_New_League_Definition parameters = eventGridEvent.Data.ToObject<Create_New_League_Definition>();
                CommandLogRecord<Create_New_League_Definition> cmdRecord = CommandLogRecord<Create_New_League_Definition>.Create(COMMAND_NAME,
                    parameters);


                EventStream commandEvents = new EventStream(@"Command",
                    COMMAND_NAME,
                    cmdRecord.CommandUniqueIdentifier.ToString());
                if (null != commandEvents )
                {
                    commandEvents.AppendEvent(new TheLongRun.Common.Events.Command.CommandCreated(COMMAND_NAME,
                        cmdRecord.CommandUniqueIdentifier));

                    // Log the parameters
                    commandEvents.AppendEvent(new TheLongRun.Common.Events.Command.ParameterValueSet(nameof(parameters.LeagueName), parameters.LeagueName));
                    commandEvents.AppendEvent(new TheLongRun.Common.Events.Command.ParameterValueSet(nameof(parameters.Email_Address ), parameters.Email_Address ));
                    commandEvents.AppendEvent(new TheLongRun.Common.Events.Command.ParameterValueSet(nameof(parameters.Date_Incorporated ), parameters.Date_Incorporated));
                    commandEvents.AppendEvent(new TheLongRun.Common.Events.Command.ParameterValueSet(nameof(parameters.Twitter_Handle), parameters.Twitter_Handle ));




                }


            }
            catch (Exception ex)
            {
                log.Error(ex.ToString()); 
            }

            // Log that this step has completed
            if (null != log)
            {
                log.Verbose ("Command passed on to handler",source : "OnCreateLeagueCommand");
            }
        }
    }   
}
