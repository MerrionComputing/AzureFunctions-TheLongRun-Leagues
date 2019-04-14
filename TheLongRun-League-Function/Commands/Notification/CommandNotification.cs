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
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net.Http;
using TheLongRun.Common.Events.Command.Projections;
using System.Collections.Generic;

namespace TheLongRunLeaguesFunction.Commands.Notification
{
    /// <summary>
    /// Orchestration to run the projection over a command to send out any notifications
    /// </summary>
    [TheLongRun.Common.Attributes.DomainName(Constants.Domain_Command)]
    public static partial class CommandNotification
    {

        [ApplicationName("The Long Run")]
        [FunctionName("CommandNotification")]
        public static async Task<HttpResponseMessage> CommandNotificationRun(
                [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequestMessage req,
                [OrchestrationClient] DurableOrchestrationClient runNotificationOrchestrationClient,
                ILogger log)
        {

            #region Logging
            if (null != log)
            {
                log.LogInformation("Sending command notifications for a command");
            }
            #endregion

            int timeoutLength = 30;  // seconds
            int retryWait = 1; // seconds

            string commandId = req.RequestUri.ParseQueryString()["CommandId"];
            string commandName = req.RequestUri.ParseQueryString()["CommandName"];

            // Get a timeout and retry wait if they are passed in as parameters
            string timeoutLengthString = req.RequestUri.ParseQueryString()["TimeOut"];
            if (!string.IsNullOrWhiteSpace(timeoutLengthString))
            {
                int.TryParse(timeoutLengthString, out timeoutLength);
            }
            string retryWaitString = req.RequestUri.ParseQueryString()["RetryWait"];
            if (!string.IsNullOrWhiteSpace(retryWaitString))
            {
                int.TryParse(retryWaitString, out retryWait);
            }

            // Check if the parameters have been passed in the http request body
            dynamic eventData = await req.Content.ReadAsAsync<object>();
            if (null != eventData)
            {
                commandId = commandId ?? eventData?.CommandId;
                commandName = commandName ?? eventData?.CommandName;
            }

            // Fire the orchestration to do the actual work of sending notifications
            Command_Notification_Request payload = new Command_Notification_Request()
            {
                CommandName = commandName ,
                CommandUniqueIdentifier = commandId 
            };

            // call the orchestrator...
            string instanceId = await runNotificationOrchestrationClient.StartNewAsync("CommandNotificationOrchestrator", payload);


            #region Logging
            if (null != log)
            {
                log.LogInformation($"Started CommandNotificationOrchestrator - instance id: {instanceId }");
            }
            #endregion

            // Wait for it to complete
            TimeSpan timeout = TimeSpan.FromSeconds(timeoutLength);
            TimeSpan retryInterval = TimeSpan.FromSeconds(retryWait);

            return await runNotificationOrchestrationClient.WaitForCompletionOrCreateCheckStatusResponseAsync(
                req,
                instanceId,
                timeout,
                retryInterval);

        }


        /// <summary>
        /// Top level orchestrator to send all the command notifications
        /// </summary>
        [ApplicationName("The Long Run")]
        [FunctionName("CommandNotificationOrchestrator")]
        public static async Task<ActivityResponse> CommandNotificationOrchestrator(
                 [OrchestrationTrigger] DurableOrchestrationContext context,
                 Microsoft.Extensions.Logging.ILogger log)
        {
            ActivityResponse < Command_Notification_Response > response = new ActivityResponse<Command_Notification_Response>() { FunctionName = "CommandNotificationOrchestrator" };

            Command_Notification_Request request = context.GetInput<Command_Notification_Request>();

            if (null != request)
            {

                Guid UniqueIdentifierGuid;
                if (Guid.TryParse(request.CommandUniqueIdentifier, out UniqueIdentifierGuid))
                {
                    // run the [Command_Notifications_Projection]..
                    response = await context.CallActivityAsync<ActivityResponse<Command_Notification_Response>>("CommandNotificationActivity", request); 
                }
                else
                {
                    response.FatalError = true;
                    response.Message = $"Unable to get command unique identifier for {request.CommandName} {request.CommandUniqueIdentifier } as a GUID ";
                }
            }

            return response;
        }


        /// <summary>
        /// Durable function activity to send all the command notifications for a specified command
        /// </summary>
        [ApplicationName("The Long Run")]
        [FunctionName("CommandNotificationActivity")]
        public static async Task<ActivityResponse<Command_Notification_Response> > CommandNotificationActivity([ActivityTrigger] DurableActivityContext context,
            ILogger log)
        {

            ActivityResponse<Command_Notification_Response> response = new ActivityResponse<Command_Notification_Response>() { FunctionName = "CommandNotificationActivity" };

            Command_Notification_Request payload = context.GetInput<Command_Notification_Request>();

            if (null != payload)
            {
                // run the projection to send the notifications..
                Projection getCommandNotifications = new Projection(Constants.Domain_Command,
                    payload.CommandName,
                    payload.CommandUniqueIdentifier,
                    nameof(Command_Notifications_Projection));

                Command_Notifications_Projection cmdProjection = new Command_Notifications_Projection(log);

                await getCommandNotifications.Process(cmdProjection);

                if ((cmdProjection.CurrentSequenceNumber > 0) || (cmdProjection.ProjectionValuesChanged()))
                {
                    // Send the completed notification if the command is complete
                    if (cmdProjection.Completed )
                    {
                        // make it an "all completed" notification
                    }
                    else
                    {
                        // otherwise send each step notification
                        foreach (CommandStepCompleted stepCompleted in cmdProjection.StepsCompleted )
                        {
                            // make it a "steps completed" notification

                        }
                    }
                }

            }

            return response;
        }
    }




    /// <summary>
    /// Request to sent the notifications for a specific command
    /// </summary>
    /// <remarks>
    /// This may be added to if we choose to allow filters to be specified
    /// or any extension functionality like that
    /// </remarks>
    public class Command_Notification_Request
    {

        /// <summary>
        /// The name of the command for which notifications should be sent
        /// </summary>
        public  string CommandName { get; set; }

        /// <summary>
        /// The unique identifier of the command to send notifications for
        /// </summary>
        public string CommandUniqueIdentifier { get; set; }

    }


    /// <summary>
    /// A response from the command notification projection to list 
    /// who needs to be sent notifications and what about
    /// </summary>
    public class Command_Notification_Response
    {

        /// <summary>
        /// The command completed successfully
        /// </summary>
        public bool Completed { get; set; }

        /// <summary>
        /// The command is in an error state
        /// </summary>
        public bool InError { get; set; }

        /// <summary>
        /// The set of recipients top notify about this command
        /// </summary>
        public IEnumerable<ReturnHookAdded> NotificationTargetHooks { get; set; }

        /// <summary>
        /// The set of all the entities impacted by this command
        /// </summary>
        public IEnumerable<CommandNotificationImpactedEntity> ImpactedEntities { get; set; }


        /// <summary>
        /// The names of all the steps that have been processed
        /// </summary>
        public IEnumerable<string > CompletedStepNames { get; set; }

    }
}
