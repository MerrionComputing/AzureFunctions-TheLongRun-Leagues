using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using TheLongRun.Common.Orchestration;
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
    public  static partial class CommandComplete
    {

        //CommandCompleteActivity
        [ApplicationName("The Long Run")]
        [FunctionName("CommandCompleteActivity")]
        public static async Task<ActivityResponse> CommandCompleteActivity(
        [ActivityTrigger] DurableActivityContext context,
        ILogger log)
        {
            ActivityResponse ret = new ActivityResponse() { FunctionName = "CommandCompleteActivity" };

            try
            {
                CommandRequest<object> cmdResponse = context.GetInput<CommandRequest<object>>();

                if (null != cmdResponse)
                {
                    // Create a new "command completed" event
                    CommandCompleted commandCompletedEvent = new CommandCompleted()
                    {
                         Date_Completed = DateTime.UtcNow ,
                         Notes = cmdResponse.Status
                    };

                    EventStream commandEvents = EventStream.Create(Constants.Domain_Command,
                            cmdResponse.CommandName,
                            cmdResponse.CommandUniqueIdentifier.ToString());

                    if ((null != commandEvents) && (null != commandCompletedEvent))
                    {
                        await commandEvents.AppendEvent(commandCompletedEvent);
                    }
                }
            }
            catch (Exception ex)
            {
                ret.Message = $"Error marking command complete : {ex.Message }";
            }

            return ret;
        }

    }
}
