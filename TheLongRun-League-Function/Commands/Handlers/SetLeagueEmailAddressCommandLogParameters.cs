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

    public static partial class SetLeagueEmailAddressCommandHandler
    {


        [ApplicationName("The Long Run")]
        [DomainName("Leagues")]
        [AggregateRoot("League")]
        [CommandName("Set League Email Address")]
        [FunctionName("SetLeagueEmailAddressCommandLogParametersActivity")]
        public static async Task<ActivityResponse> SetLeagueEmailAddressCommandLogParametersActivity(
                [ActivityTrigger] DurableActivityContext context,
                ILogger log)
        {
            ActivityResponse ret = new ActivityResponse() { FunctionName = "SetLeagueEmailAddressCommandHandlerActivity" };

            try
            {
                CommandRequest<Set_Email_Address_Definition> cmdRequest = context.GetInput<CommandRequest<Set_Email_Address_Definition>>();

                if (null != cmdRequest)
                {
                    if (null != log)
                    {
                        // Unable to get the request details from the orchestration
                        log.LogInformation($"CreateLeagueCommandLogParametersActivity : Logging parameters for command {cmdRequest.CommandUniqueIdentifier} ");
                    }

                    EventStream commandEvents = new EventStream(@"Command",
                        cmdRequest.CommandName,
                        cmdRequest.CommandUniqueIdentifier.ToString());

                    if (null != commandEvents)
                    {
                        // Set the context for events written
                        commandEvents.SetContext(new WriteContext(ret.FunctionName, context.InstanceId));

                        Set_Email_Address_Definition  parameters = cmdRequest.GetParameters();

                        if (null != parameters)
                        {
                            // Set the parameters
                            await commandEvents.AppendEvent(new TheLongRun.Common.Events.Command.ParameterValueSet(nameof(parameters.LeagueName),
                                 parameters.LeagueName));

                            await commandEvents.AppendEvent(new TheLongRun.Common.Events.Command.ParameterValueSet(nameof(parameters.New_Email_Address),
                                 parameters.New_Email_Address));

                            await commandEvents.AppendEvent(new TheLongRun.Common.Events.Command.ParameterValueSet(nameof(parameters.Notes),
                                 parameters.Notes ));

                            await commandEvents.AppendEvent(new TheLongRun.Common.Events.Command.ParameterValueSet(nameof(parameters.InstanceIdentifier ),
                                parameters.InstanceIdentifier ));

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
                    ret.FatalError = true;
                    ret.Message = $"Unable to read command request from {context.InstanceId }";
                }
            }
            catch (Exception ex)
            {
                ret.FatalError = true;
                ret.Message = ex.Message;
            }

            return ret;
        }
    }
}
