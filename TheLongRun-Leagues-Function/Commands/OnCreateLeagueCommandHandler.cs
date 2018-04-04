using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.IO;


using Leagues.League.commandDefinition;
using TheLongRun.Common;
using TheLongRun.Common.Attributes;

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
            Binder commandLog,
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


                // And write this command log to a new file on the command-log blob container
                var commandLogBlogAttribute = new BlobAttribute(CommandLogRecord.MakeFullPath(
                    CommandLogRecord.DEFAULT_CONTAINER_NAME,
                    COMMAND_NAME,cmdRecord),
                     FileAccess.Write);
                

                var commandLogStorageAccountAttribute = new StorageAccountAttribute(CommandLogRecord.DEFAULT_CONNECTION);

                #region Logging
                if (null != log)
                {
                    if (null != commandLogBlogAttribute)
                    {
                        log.Verbose($"Created valid Blog attribute {commandLogBlogAttribute.BlobPath} using {commandLogBlogAttribute.Connection} ",
                            source: "OnCreateLeagueCommand");
                    }
                    else
                    {
                        log.Error("Unable to initialise Blob attribute");
                    }
                    if (null != commandLogStorageAccountAttribute)
                    {
                        log.Verbose($"Created valid storage account attribute {commandLogStorageAccountAttribute.Account} ",
                            source: "OnCreateLeagueCommand");
                    }
                    else
                    {
                        log.Error("Unable to initialise Storage Account attribute");
                    }
                }
                #endregion

                var attributes = new Attribute[]
                {
                    commandLogBlogAttribute,
                    commandLogStorageAccountAttribute
                };

                using (var writer = await commandLog.BindAsync<TextWriter>(attributes))
                {
                    #region Logging
                    if (null != log)
                    {
                        log.Verbose($"Saving command to {writer.ToString()}");
                    }
                    #endregion

                    // persist the command to the blob
                    writer.Write(Newtonsoft.Json.JsonConvert.SerializeObject(cmdRecord));
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
