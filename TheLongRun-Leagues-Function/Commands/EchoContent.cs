using System;
using Leagues.League.commandDefinition;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Host;
using TheLongRun.Common;

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


            if (eventGridEvent.EventType == "Create-New-League-Command")
            {
                try
                {
                    Create_New_League_Definition parameters = eventGridEvent.Data.ToObject<Create_New_League_Definition>();

                    log.Info($" Parameters : League Name {parameters.LeagueName } , Location : {parameters.Location}");

                    CommandLogRecord<Create_New_League_Definition> cmdRecord = CommandLogRecord<Create_New_League_Definition>.Create("Echo",
                        parameters);

                    log.Info($"Command unique identifier : {cmdRecord.CommandUniqueIdentifier} ");
                }
                catch (Exception ex)
                {
                    log.Error(ex.ToString(), ex); 
                }
            }

        }
    }
}

