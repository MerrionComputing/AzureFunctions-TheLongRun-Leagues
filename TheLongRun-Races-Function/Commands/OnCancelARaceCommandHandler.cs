using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;

using TheLongRun.Common;
using TheLongRun.Common.Attributes;

using Races.Race.commandDefinition;
using System.IO;


namespace TheLongRun_Races_Functions.Commands
{
    public static class OnCancelARaceCommandHandler
    {
        [ApplicationName("The Long Run")]
        [DomainName("Races")]
        [AggregateRoot("Race")]
        [CommandName("Cancel Race")]
        [FunctionName("OnCancelARaceCommandHandler")]
        public static void OnCancelARaceCommandHandlerRun(
            [EventGridTrigger] EventGridEvent eventGridEvent,
            TraceWriter log,
            Binder commandLog)
        {
            const string COMMAND_NAME = @"cancel-race";

            if (null == eventGridEvent)
            {
                // This function should not proceed if there is no event data
                if (null != log)
                {
                    log.Error("Missing event grid trigger data",
                        source: "OnCancelARaceCommandHandler");
                }
                return;
            }

            // Get the parameters etc out of the trigger and put them in the log record
            Cancel_a_race_Definition parameters = eventGridEvent.Data.ToObject<Cancel_a_race_Definition>();
            CommandLogRecord<Cancel_a_race_Definition> cmdRecord = CommandLogRecord<Cancel_a_race_Definition>.Create(COMMAND_NAME,
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
                log.Verbose("Command passed on to handler", source: "OnCancelARaceCommandHandler");
            }
        }
    }
}
