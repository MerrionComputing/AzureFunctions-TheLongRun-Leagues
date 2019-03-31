using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using TheLongRun.Common.Orchestration;
using CQRSAzure.EventSourcing;
using System.Threading.Tasks;
using System;
using TheLongRun.Common;
using TheLongRun.Common.Attributes;
using TheLongRun.Common.Bindings;
using TheLongRun.Common.Events.Command;

namespace TheLongRunLeaguesFunction.Commands.Handlers
{
    /// <summary>
    /// Mark a step of a command as being complete
    /// </summary>
    [TheLongRun.Common.Attributes.DomainName(Constants.Domain_Command)]
    public static partial class CommandStepComplete
    {

        //CommandStepCompleteActivity
        [ApplicationName("The Long Run")]
        [FunctionName("CommandStepCompleteActivity")]
        public static async Task<ActivityResponse> CommandStepCompleteActivity(
                [ActivityTrigger] DurableActivityContext context,
                ILogger log)
        {
            ActivityResponse ret = new ActivityResponse() { FunctionName = "CommandStepComplete" };

            try
            {
                CommandStepResponse cmdStepResponse = context.GetInput<CommandStepResponse>();

                if (null != cmdStepResponse)
                {
                    // Create a new "command step" event
                    CommandStepCompleted commandStepCompletedEvent = new CommandStepCompleted()
                    {
                        StepName = cmdStepResponse.StepName,
                        ImpactedEntities=cmdStepResponse.ImpactedEntities
                    }; 

                    EventStream commandEvents = new EventStream(Constants.Domain_Command ,
                            cmdStepResponse.CommandName ,
                            cmdStepResponse.CommandUniqueIdentifier.ToString() );

                    if ((null != commandEvents) && (null != commandStepCompletedEvent))
                    {
                        await commandEvents.AppendEvent(commandStepCompletedEvent);
                    }
                }
            }
            catch (Exception ex)
            {
                ret.Message = $"Error marking step complete : {ex.Message }";
            }

            return ret;
        }
    }
}
