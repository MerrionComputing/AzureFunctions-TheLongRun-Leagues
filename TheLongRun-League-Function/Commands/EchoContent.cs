using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using System;
using Microsoft.Azure.EventGrid.Models;

namespace TheLongRunLeaguesFunction.Commands
{
    public static partial class Command
    {
        [FunctionName("EchoContent")]
        public static void EchoContent([EventGridTrigger] EventGridEvent eventGridEvent,
               TraceWriter log)
        {
            log.Info($"Echo function executed at: {DateTime.Now}");

            log.Info($"Topic : {eventGridEvent.Topic} , Subject : {eventGridEvent.Subject } , Event Type: {eventGridEvent.EventType }  ");
            log.Info($" Payload : { eventGridEvent.Data.ToString()} ");
            log.Info($" Event time:  { eventGridEvent.EventTime}, Published: {eventGridEvent.EventTime} ");            

            log.Info("=== Complete ============================="); 
        }
    }
}

