using Leagues.League.commandDefinition;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TheLongRun.Common;
using TheLongRun.Common.Attributes;
using TheLongRun.Common.Bindings;
using TheLongRun.Common.Orchestration;

namespace TheLongRunLeaguesFunction.Commands.Handlers
{
    public static partial class CreateLeagueCommandHandler
    {

        // Log the parameters used to perform this create league command

        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [AggregateRoot("League")]
        [CommandName ("Create League")]
        [FunctionName("CreateLeagueCommandLogParametersActivity")]
        public static async Task<ActivityResponse> CreateLeagueCommandLogParametersActivity(
                [ActivityTrigger] DurableActivityContext context,
                ILogger log)
        {



            ActivityResponse ret = new ActivityResponse() { FunctionName = "CreateLeagueCommandLogParametersActivity" };

            try
            {
                CommandRequest<Create_New_League_Definition> cmdRequest = context.GetInput<CommandRequest<Create_New_League_Definition>>();

                if (null != cmdRequest)
                {
                    if (null != log)
                    {
                        // Unable to get the request details from the orchestration
                        log.LogInformation($"CreateLeagueCommandLogParametersActivity : Logging parameters for command {cmdRequest.CommandUniqueIdentifier} ");
                    }

                    EventStream commandEvents = new EventStream(@"Command",
                        cmdRequest.CommandName ,
                        cmdRequest.CommandUniqueIdentifier.ToString());

                    if (null != commandEvents)
                    {

                        Create_New_League_Definition parameters = cmdRequest.GetParameters();

                        if (null != parameters)
                        {
                            // Set the parameters
                            await commandEvents.AppendEvent(new TheLongRun.Common.Events.Command.ParameterValueSet(nameof(parameters.LeagueName),
                                 parameters.LeagueName));

                            await commandEvents.AppendEvent(new TheLongRun.Common.Events.Command.ParameterValueSet(nameof(parameters.Email_Address),
                                 parameters.Email_Address));

                            await commandEvents.AppendEvent(new TheLongRun.Common.Events.Command.ParameterValueSet(nameof(parameters.Date_Incorporated),
                                 parameters.Date_Incorporated));

                            await commandEvents.AppendEvent(new TheLongRun.Common.Events.Command.ParameterValueSet(nameof(parameters.Twitter_Handle),
                                parameters.Twitter_Handle));

                            ret.Message = $"All parameters to set for {cmdRequest.CommandName} : {cmdRequest.CommandUniqueIdentifier}";
                        }
                        else
                        {
                            ret.Message = $"No parameters to set for {cmdRequest.CommandName} : {cmdRequest.CommandUniqueIdentifier}";
                        }
                    }
                    else
                    {
                        ret.Message = $"Unable to get the event stream for {cmdRequest.CommandName} : {cmdRequest.CommandUniqueIdentifier}";
                        ret.FatalError = true;
                    }
                }
                else
                {
                    ret.Message = $"Unable to get the command request details from {context.ToString()} ";
                    ret.FatalError = true;
                }
            }
            catch (Exception ex)
            {
                if (null != log)
                {
                    // Unable to get the request details from the orchestration
                    log.LogError($"CreateLeagueCommandLogParametersActivity : error {ex.Message} ");
                }
                ret.Message = ex.Message;
                ret.FatalError = true;
            }
            return ret;
        }


    }
}
