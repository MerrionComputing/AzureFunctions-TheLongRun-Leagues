using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;

using TheLongRun.Common;
using TheLongRun.Common.Attributes;

using Races.Race.commandDefinition;
using System.IO;

namespace TheLongRun_Races_Functions.Commands
{
    public static class OnLapCompletedRaceCommandHandler
    {

        [ApplicationName("The Long Run")]
        [DomainName("Races")]
        [AggregateRoot("Race")]
        [CommandName("Lap Completed")]
        [FunctionName("OnLapCompletedRaceCommandHandler")]
        public static void OnLapCompletedRaceCommandHandlerRun(
                      [EventGridTrigger] EventGridEvent eventGridEvent,
                      TraceWriter log,
                      Binder commandLog)
        {
            const string COMMAND_NAME = @"lap-completed-race";

            if (null == eventGridEvent)
            {
                // This function should not proceed if there is no event data
                if (null != log)
                {
                    log.Error("Missing event grid trigger data",
                        source: "OnLapCompletedRaceCommandHandler");
                }
                return;
            }

            // TODO: Get the parameters etc out of the trigger and put them in the log record

            // Log that this step has completed
            if (null != log)
            {
                log.Verbose("Command passed on to handler", source: "OnLapCompletedRaceCommandHandler");
            }
        }

    }
}
