using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using System.IO;

using TheLongRun.Common;
using TheLongRun.Common.Attributes;

using Races.Race.commandDefinition;


namespace TheLongRun_Races_Functions.Commands
{
    public static class OnStartARaceCommandHandler
    {
        /// <summary>
        /// Turn the eventgrid message received (Start A Race) into a wrapped command
        /// and log it in the /Commands folder in the [Races] domain storage
        /// </summary>
        /// <param name="eventGridEvent">
        /// 
        /// </param>
        /// <param name="log"></param>
        /// <param name="commandLog">
        /// The binder to use to decide where to write the wrapped command
        /// </param>
        [ApplicationName("The Long Run")]
        [DomainName("Races")]
        [AggregateRoot("Race")]
        [CommandName("Start Race")]
        [FunctionName("OnStartARaceCommandHandler")]
        public static void OnStartARaceCommandHandlerRun(
            [EventGridTrigger] EventGridEvent eventGridEvent,
            TraceWriter log,
            Binder commandLog)

        {

            const string COMMAND_NAME = @"start-race";

            if (null == eventGridEvent)
            {
                // This function should not proceed if there is no event data
                if (null != log)
                {
                    log.Error("Missing event grid trigger data",
                        source: "OnStartARaceCommandHandler");
                }
                return;
            }

            // Get the parameters etc out of the trigger and put them in the log record
            Start_a_race_Definition parameters = eventGridEvent.Data.ToObject<Start_a_race_Definition>();
            CommandLogRecord<Start_a_race_Definition> cmdRecord = CommandLogRecord<Start_a_race_Definition>.Create(COMMAND_NAME,
                parameters);

            // And write this command log to a new file on the command-log blob container
            var commandLogBlogAttribute = new BlobAttribute(CommandLogRecord.DEFAULT_CONTAINER_NAME + @"/" +
                COMMAND_NAME + @"/" +
                CommandLogRecord.MakeFilename(cmdRecord),
                 FileAccess.Write)
            {
                Connection = CommandLogRecord.DEFAULT_CONNECTION
            };

            using (var writer = commandLog.Bind<TextWriter>(commandLogBlogAttribute))
            {
                // persist the command to the blob
                writer.Write(Newtonsoft.Json.JsonConvert.SerializeObject(cmdRecord));
            }


            // Log that this step has completed
            if (null != log)
            {
                log.Verbose("Command passed on to handler", source: "OnStartARaceCommandHandler");
            }
        }
    }
}
