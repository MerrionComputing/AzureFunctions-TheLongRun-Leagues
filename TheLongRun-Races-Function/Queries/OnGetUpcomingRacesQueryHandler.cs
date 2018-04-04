using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;

using TheLongRun.Common;
using TheLongRun.Common.Attributes;

using Races.Race.commandDefinition;
using System.IO;

namespace TheLongRun_Races_Functions.Queries
{
    public static class OnGetUpcomingRacesQueryHandler
    {
        /// <summary>
        /// Handle the Get-Upcoming-Races query event grid topic
        /// </summary>
        /// <param name="eventGridEvent">
        /// The notification event received from 
        /// </param>
        /// <param name="log">
        /// Trace logging
        /// </param>
        /// <param name="queryLog">
        /// The binder to use to store the query definition
        /// </param>
        [ApplicationName("The Long Run")]
        [DomainName("Races")]
        [AggregateRoot("Race")]
        [QueryName("Get Upcoming Races")]
        [FunctionName("OnGetUpcomingRacesQueryHandler")]
        public static void OnGetUpcomingRacesQueryHandlerRun(
                        [EventGridTrigger] EventGridEvent eventGridEvent,
                        TraceWriter log,
                        Binder queryLog)
        {

        }

    }
}
