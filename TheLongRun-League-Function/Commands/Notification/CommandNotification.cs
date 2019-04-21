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
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Newtonsoft.Json;

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
            Command_Get_Notifications_Request payload = new Command_Get_Notifications_Request()
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

            Command_Get_Notifications_Request request = context.GetInput<Command_Get_Notifications_Request>();

            if (null != request)
            {

                Guid UniqueIdentifierGuid;
                if (Guid.TryParse(request.CommandUniqueIdentifier, out UniqueIdentifierGuid))
                {
                    // run the [Command_Notifications_Projection]..
                    response = await context.CallActivityAsync<ActivityResponse<Command_Notification_Response>>("GetCommandNotificationsActivity", request); 

                    if (null != response )
                    {
                        if ( (! response.FatalError ) && ( ! response.StepFailure  ))
                        {
                            if (null != response.ReturnedData)
                            {

                                List<Task<ActivityResponse>> allNotificationTasks = new List<Task<ActivityResponse>>();

                                // Only process the notifications that match the state of the command...
                                IEnumerable<ReturnHookAdded> notificationHooks = null;
                                if (response.ReturnedData.InError)
                                {
                                    notificationHooks = response.ReturnedData.NotificationTargetHooks.ForErrors();
                                }
                                else
                                {
                                    if (response.ReturnedData.Completed)
                                    {
                                        notificationHooks = response.ReturnedData.NotificationTargetHooks.ForCompleteCommands();
                                    }
                                    else
                                    {
                                        notificationHooks = response.ReturnedData.NotificationTargetHooks.ForStepComplete ();
                                    }
                                }

                                // fire off all the notifications in parrallel
                                foreach (var notificationTarget in notificationHooks)
                                {


                                    foreach (var notifyEntity in response.ReturnedData.ImpactedEntities)
                                    { 
                                    
                                        // create an individual notification request
                                        Command_Notification_Request notifyRequest = new Command_Notification_Request()
                                        {
                                            CommandName = request.CommandName,
                                            CommandNotificationType = Command_Notification_Request.NotificationType.StepComplete ,
                                            HookAddress = notificationTarget.HookAddress ,
                                            HookType = notificationTarget.HookType  ,
                                            ImpactedEntity = notifyEntity 
                                        };

                                        if (response.ReturnedData.InError)
                                        {
                                            notifyRequest.CommandNotificationType = Command_Notification_Request.NotificationType.Error;
                                        }
                                        else
                                        {
                                            if (response.ReturnedData.Completed)
                                            {
                                                notifyRequest.CommandNotificationType = Command_Notification_Request.NotificationType.CommandComplete;
                                            }
                                        }

                                        if (notificationTarget.HookType == CommandNotificationTarget.NotificationTargetType.CustomEventGridTopic  )
                                        {
                                            // RunCustomEventGridTopicNotificationActivity
                                            allNotificationTasks.Add(context.CallActivityWithRetryAsync<ActivityResponse>("RunCustomEventGridTopicNotificationActivity",
                                                        DomainSettings.CommandRetryOptions(),
                                                        notifyRequest));
                                        }

                                        if (notificationTarget.HookType == CommandNotificationTarget.NotificationTargetType.WebHook )
                                        {
                                            // RunWebHookNotificationActivity
                                            allNotificationTasks.Add(context.CallActivityWithRetryAsync<ActivityResponse>("RunWebHookNotificationActivity",
                                                    DomainSettings.CommandRetryOptions(),
                                                    notifyRequest));
                                        }

                                        if (notificationTarget.HookType == CommandNotificationTarget.NotificationTargetType.SignalR )
                                        {
                                            //RunSignalRNotificationActivity
                                            allNotificationTasks.Add(context.CallActivityWithRetryAsync<ActivityResponse>("RunSignalRNotificationActivity",
                                                    DomainSettings.CommandRetryOptions(),
                                                    notifyRequest));
                                        }
                                    }
                                }

                                // Run the projections in parallel...
                                await Task.WhenAll(allNotificationTasks);


                            }
                        }
                        else
                        {
                            #region Logging
                            if (null != log )
                            {
                                log.LogError($"{response.FunctionName} error - no data returned - {response.Message} ");
                            }
                            #endregion
                        }
                    }

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
        /// Durable function activity to get all the command notifications for a specified command
        /// </summary>
        [ApplicationName("The Long Run")]
        [FunctionName("GetCommandNotificationsActivity")]
        public static async Task<ActivityResponse<Command_Notification_Response> > GetCommandNotificationsActivity([ActivityTrigger] DurableActivityContext context,
            ILogger log)
        {

            ActivityResponse<Command_Notification_Response> response = new ActivityResponse<Command_Notification_Response>()
            {
                FunctionName = "CommandNotificationActivity",
                ReturnedData = new Command_Notification_Response()
            };

            Command_Get_Notifications_Request payload = context.GetInput<Command_Get_Notifications_Request>();

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
                        response.ReturnedData.Completed = true;
                    }
                    else if(cmdProjection.InError )
                    {
                        // make it an "all completed" notification
                        response.ReturnedData.InError = true;
                    }
                    else
                    {
                        // otherwise send each step notification
                        foreach (CommandStepCompleted stepCompleted in cmdProjection.StepsCompleted )
                        {
                            // make it a "steps completed" notification

                        }
                    }

                    if (null != cmdProjection.ImpactedEntities )
                    {
                        response.ReturnedData.ImpactedEntities = cmdProjection.ImpactedEntities;
                    }

                    if (null != cmdProjection.NotificationTargetHooks )
                    {
                        response.ReturnedData.NotificationTargetHooks = cmdProjection.NotificationTargetHooks;
                    }
                }

            }

            return response;
        }



        /// <summary>
        /// Durable function activity to send a single notification
        /// </summary>
        [ApplicationName("The Long Run")]
        [FunctionName("RunCustomEventGridTopicNotificationActivity")]
        public static async Task<ActivityResponse> RunCustomEventGridTopicNotificationActivity(
            [ActivityTrigger] DurableActivityContext context,
            Binder outputBinder,
            ILogger log)
        {

            ActivityResponse response = new ActivityResponse() { FunctionName = "RunCustomEventGridTopicNotificationActivity" };

            Command_Notification_Request notifyRequest = context.GetInput<Command_Notification_Request>();

            if (null != notifyRequest)
            {
                EventGridAttribute egAttribute = EventGridAttributeFromNotifyRequest(notifyRequest.HookAddress, notifyRequest.CommandName);

                if (null != egAttribute)
                {
                    // split the target string into an event grid attribute 

                    Microsoft.Azure.EventGrid.Models.EventGridEvent eventGridEvent = new Microsoft.Azure.EventGrid.Models.EventGridEvent()
                    {
                        Subject = $"/{EventStream.MakeEventStreamName(notifyRequest.ImpactedEntity.EntityType)}/{EventStream.MakeEventStreamName(notifyRequest.ImpactedEntity.InstanceUniqueIdentifier)}",
                        Data = notifyRequest
                    };

                    IAsyncCollector<EventGridEvent> eventCollector = outputBinder.Bind<IAsyncCollector<EventGridEvent>>(egAttribute);
                    if (null != eventCollector)
                    {
                        await eventCollector.AddAsync(eventGridEvent);
                        await eventCollector.FlushAsync();
                    }
                    response.Message = $"Sent notification to {egAttribute.TopicEndpointUri} for {notifyRequest.ImpactedEntity.EntityType}:{notifyRequest.ImpactedEntity.InstanceUniqueIdentifier }  ";
                }
                else
                {
                    response.StepFailure = true;
                    response.Message = "Unable to create an event grid attribute to send the notification to";
                }
            }
            else
            {
                response.StepFailure = true;
                response.Message = "Unable to read command notification request from context";
            }


            return response;
        }


        private static EventGridAttribute EventGridAttributeFromNotifyRequest(string hookAddress, string commandName)
        {
            if (!string.IsNullOrWhiteSpace(hookAddress))
            {
                EventGridAttribute ret = new EventGridAttribute();
                ret.TopicEndpointUri = hookAddress;
                ret.TopicKeySetting = EventStream.MakeEventStreamName(commandName);
            }

            // If we reach the end with not enough info to create an event grid target return null
            return null;
        }


        // RunWebHookNotificationActivity
        /// <summary>
        /// Durable function activity to send a single notification
        /// </summary>
        [ApplicationName("The Long Run")]
        [FunctionName("RunWebHookNotificationActivity")]
        public static async Task<ActivityResponse> RunWebHookNotificationActivity(
            [ActivityTrigger] DurableActivityContext context,
            Binder outputBinder,
            ILogger log)
        {
            ActivityResponse response = new ActivityResponse() { FunctionName = "RunWebHookNotificationActivity" };

            Command_Notification_Request notifyRequest = context.GetInput<Command_Notification_Request>();

            if (null != notifyRequest)
            {
                var payloadAsJSON = new StringContent(JsonConvert.SerializeObject(notifyRequest));

                // Use the binder to bind to an HTTP client to send the results to
                using (var client = new HttpClient())
                {
                    HttpResponseMessage msgResp = await client.PostAsync(notifyRequest.HookAddress, payloadAsJSON);
                    if (null != msgResp)
                    {
                        response.Message = $"Output sent - {msgResp.ReasonPhrase}";
                    }
                }
            }
            else
            {
                response.StepFailure = true;
                response.Message = "Unable to read command notification request from context";
            }

            return response;
        }

        private static SignalRAttribute SignalRAttributeFromTarget(string hookAddress)
        {
            if (!string.IsNullOrWhiteSpace(hookAddress))
            {
                SignalRAttribute ret = new SignalRAttribute()
                {
                    // Look up the connection string from the hub name
                    ConnectionStringSetting = ConnectionStringNameAttribute.DefaultConnectionStringName(Constants.Domain_Command , hookAddress ),
                    HubName = hookAddress 
                };
                return ret;
            }

            return null;
        }


        /// <summary>
        /// Durable function activity to send a single notification
        /// </summary>
        [ApplicationName("The Long Run")]
        [FunctionName("RunSignalRNotificationActivity")]
        public static async Task<ActivityResponse> RunSignalRNotificationActivity(
            [ActivityTrigger] DurableActivityContext context,
            Binder outputBinder,
            ILogger log)
        {
            ActivityResponse response = new ActivityResponse() { FunctionName = "RunSignalRNotificationActivity" };

            Command_Notification_Request notifyRequest = context.GetInput<Command_Notification_Request>();

            if (null != notifyRequest)
            {
                // send the notification by SignalR
                SignalRAttribute signalRAttribute = SignalRAttributeFromTarget(notifyRequest.HookAddress);

                IAsyncCollector<SignalRMessage> eventCollector = outputBinder.Bind<IAsyncCollector<SignalRMessage>>(signalRAttribute);

                // Create and add a SignalRMessage
                if (null != eventCollector)
                {
                    // Make a SignalR message for the query results - note that we pass the entire results structure so
                    // the recipeint gets the context as well as the results data
                    SignalRMessage queryMessage = new SignalRMessage()
                    {
                        Target = notifyRequest.CommandName,
                        Arguments = new object[] { notifyRequest }
                    };

                    await eventCollector.AddAsync(queryMessage);
                    // and flush the message out
                    await eventCollector.FlushAsync();
                }
            }
            else
            {
                response.StepFailure = true;
                response.Message = "Unable to read command notification request from context";
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
    public class Command_Get_Notifications_Request
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
    /// Request to send a single notification for a command
    /// </summary>
    public class Command_Notification_Request
    {

        /// <summary>
        /// The different types of notification that can be sent
        /// </summary>
        public enum NotificationType
        {
            /// <summary>
            /// A step in a command was completed
            /// </summary>
            StepComplete = 1,
            /// <summary>
            /// An entire command was completed
            /// </summary>
            CommandComplete = 2,
            /// <summary>
            /// An error has occured
            /// </summary>
            Error=2
        }

        /// <summary>
        /// The name of the command for which notification is sent
        /// </summary>
        public string CommandName { get; set; }


        /// <summary>
        /// What type of notification are we sending
        /// </summary>
        public NotificationType CommandNotificationType { get; set; }

        /// <summary>
        /// The type of notification target to send the notification to
        /// </summary>
        public CommandNotificationTarget.NotificationTargetType HookType { get; set; }


        /// <summary>
        /// The address of the hook to be notified
        /// </summary>
        public string HookAddress { get; set; }

        /// <summary>
        /// The unique identifier of the entity that was affected by this command and is being notified about
        /// </summary>
        public CommandNotificationImpactedEntity ImpactedEntity { get; set; }
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
