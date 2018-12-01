using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using System;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Extensions.Logging;

namespace TheLongRunLeaguesFunction.Commands
{
    public static partial class Command
    {
        [FunctionName("EchoContent")]
        public static void EchoContent([EventGridTrigger] EventGridEvent eventGridEvent,
               ILogger log)
        {
            log.LogInformation ($"Echo function executed at: {DateTime.Now}");

            log.LogInformation($"Topic : {eventGridEvent.Topic} , Subject : {eventGridEvent.Subject } , Event Type: {eventGridEvent.EventType }  ");
            log.LogInformation($" Payload : { eventGridEvent.Data.ToString()} ");
            log.LogInformation($" Event time:  { eventGridEvent.EventTime}, Published: {eventGridEvent.EventTime} ");            

            log.LogInformation("=== Complete ============================="); 
        }
    }
}

