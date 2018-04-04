using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;

using TheLongRun.Common;
using TheLongRun.Common.Attributes;

using Races.Race.commandDefinition;
using System.IO;

namespace TheLongRun_Races_Functions.Commands
{
    public static class OnRunnerFinishedCommandHandler
    {

        [ApplicationName("The Long Run")]
        [DomainName("Races")]
        [AggregateRoot("Race")]
        [CommandName("Runner Finished Race")]
        [FunctionName("OnRunnerFinishedCommandHandler")]
        public static void OnRunnerFinishedCommandHandlerRun(
                [EventGridTrigger] EventGridEvent eventGridEvent,
                TraceWriter log,
                Binder commandLog)
        {
            const string COMMAND_NAME = @"runner-finished-race";

            if (null == eventGridEvent)
            {
                // This function should not proceed if there is no event data
                if (null != log)
                {
                    log.Error("Missing event grid trigger data",
                        source: "OnRunnerFinishedCommandHandler");
                }
                return;
            }

            // TODO: Get the parameters etc out of the trigger and put them in the log record



            // Log that this step has completed
            if (null != log)
            {
                log.Verbose("Command passed on to handler", source: "OnRunnerFinishedCommandHandler");
            }
        }

    }
}
