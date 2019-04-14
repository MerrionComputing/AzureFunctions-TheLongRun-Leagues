using System;
using System.Collections.Generic;
using System.Linq;
using CQRSAzure.EventSourcing;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace TheLongRun.Common.Events.Command.Projections
{
    /// <summary>
    /// Projection to getr the list of external entities to notify that a command has been executed
    /// </summary>
    /// <remarks>
    /// This will send the appropriate notification(s)
    /// </remarks>
    [CQRSAzure.EventSourcing.Category(Constants.Domain_Command)]
    public class Command_Notifications_Projection:
        CQRSAzure.EventSourcing.ProjectionBaseUntyped,
        CQRSAzure.EventSourcing.IHandleEvent<ReturnHookAdded >,
        CQRSAzure.EventSourcing.IHandleEvent<CommandStepCompleted >,
        CQRSAzure.EventSourcing.IHandleEvent<CommandCompleted >,
        IProjectionUntyped
    {

        #region Private members
        private bool complete = false;
        private List<CommandStepCompleted> commandStepsCompleted = new List<CommandStepCompleted>(); 
        private ILogger log = null;
        private List<ReturnHookAdded> targetHooks = new List<ReturnHookAdded>();
        private List<CommandNotificationImpactedEntity> impactedEntities = new List<CommandNotificationImpactedEntity>();
        #endregion

        /// <summary>
        /// The set of target hooks added to this command
        /// </summary>
        public IEnumerable<ReturnHookAdded > NotificationTargetHooks
        {
            get
            {
                return targetHooks; 
            }
        }

        /// <summary>
        /// The set of entities impacted by the command in any way
        /// </summary>
        public IEnumerable<CommandNotificationImpactedEntity> ImpactedEntities
        {
            get
            {
                return impactedEntities;
            }
        }

        /// <summary>
        /// Has this command completed
        /// </summary>
        /// <remarks>
        /// If so then a "completed" notification should be sent - otherwise the notification(s) for the completed steps should be sent
        /// </remarks>
        public bool Completed
        {
            get
            {
                return complete;
            }
        }

        /// <summary>
        /// The steps completed so far for this command
        /// </summary>
        public IEnumerable<CommandStepCompleted> StepsCompleted
        {
            get
            {
                if (null != commandStepsCompleted )
                {
                    return commandStepsCompleted.AsEnumerable();
                }
                else
                {
                    return Enumerable.Empty<CommandStepCompleted>(); 
                }
            }
        }

        /// <summary>
        /// There is no value in storing snapshots for command summaries as they should be only a few events
        /// </summary>
        public override bool SupportsSnapshots => false;


        public override void HandleEventJSon(string eventFullName, JObject eventToHandle)
        {
            #region Logging
            if (null != log)
            {
                log.LogDebug($"HandleEventJSon({eventFullName})",
                    nameof(Command_Notifications_Projection ));
            }
            #endregion

            if (eventFullName == typeof(ReturnHookAdded ).FullName)
            {
                HandleEvent<ReturnHookAdded>(eventToHandle.ToObject<ReturnHookAdded>());
            }

            if (eventFullName == typeof(CommandStepCompleted ).FullName)
            {
                HandleEvent<CommandStepCompleted >(eventToHandle.ToObject<CommandStepCompleted >());
            }

            if (eventFullName == typeof(CommandCompleted ).FullName)
            {
                HandleEvent<CommandCompleted>(eventToHandle.ToObject<CommandCompleted>());
            }
        }


        public override bool HandlesEventType(Type eventType)
        {
            return HandlesEventTypeByName(eventType.FullName);
        }

        public override bool HandlesEventTypeByName(string eventTypeFullName)
        {

            #region Logging
            if (null != log)
            {
                log.LogDebug($"HandlesEventTypeByName({eventTypeFullName})",
                    nameof(Command_Notifications_Projection ));
            }
            #endregion


            if (eventTypeFullName == typeof(ReturnHookAdded).FullName)
            {
                return true;
            }

            if (eventTypeFullName == typeof(CommandStepCompleted ).FullName)
            {
                return true;
            }

            if (eventTypeFullName == typeof(CommandCompleted ).FullName)
            {
                return true;
            }

            return false;
        }

        public override void HandleEvent<TEvent>(TEvent eventToHandle)
        {

            #region Logging
            if (null != log)
            {
                log.LogDebug($"HandleEvent<{ typeof(TEvent).FullName  }>())",
                    nameof(Command_Notifications_Projection));
            }
            #endregion

            if (eventToHandle.GetType() == typeof(ReturnHookAdded ))
            {
                HandleEvent(eventToHandle as ReturnHookAdded);
            }

            if (eventToHandle.GetType() == typeof(CommandStepCompleted ))
            {
                HandleEvent(eventToHandle as CommandStepCompleted );
            }

            if (eventToHandle.GetType() == typeof(CommandCompleted ))
            {
                HandleEvent(eventToHandle as CommandCompleted);
            }
        }

        public void HandleEvent(ReturnHookAdded  eventHandled)
        {
            #region Logging
            if (null != log)
            {
                log.LogDebug($"HandleEvent( ReturnHookAdded )",
                    nameof(Command_Notifications_Projection ));
            }
            #endregion

            if (null != eventHandled)
            {
                // add the location to the internal lists
                #region Logging
                if (null != log)
                {
                    log.LogDebug($"Return notification added  {eventHandled.HookAddress  } Type: { eventHandled.HookType  }  ",
                        nameof(Command_Notifications_Projection ));
                }
                #endregion
                if (!string.IsNullOrWhiteSpace(eventHandled.HookAddress ))
                {
                    targetHooks.Add(eventHandled);
                }
            }
            else
            {
                #region Logging
                if (null != log)
                {
                    log.LogWarning($"HandleEvent( ReturnHookAdded ) - parameter was null",
                        nameof(Command_Notifications_Projection ));
                }
                #endregion
            }
        }

        public void HandleEvent(CommandStepCompleted  eventHandled)
        {
            #region Logging
            if (null != log)
            {
                log.LogDebug($"HandleEvent( CommandStepCompleted )",
                    nameof(Command_Notifications_Projection));
            }
            #endregion

            if (null != eventHandled)
            {
                // add the location to the internal lists
                #region Logging
                if (null != log)
                {
                    log.LogDebug($"Command step completed notification added  {eventHandled.StepName} ",
                        nameof(Command_Notifications_Projection));
                }
                #endregion

                commandStepsCompleted.Add(eventHandled); 

                if (null != eventHandled.ImpactedEntities )
                {
                    foreach (var entity in eventHandled.ImpactedEntities )
                    {
                        if (!impactedEntities.Contains(entity))
                        {
                            impactedEntities.Add(entity);
                        }
                    }
                }
            }
            else
            {
                #region Logging
                if (null != log)
                {
                    log.LogWarning($"HandleEvent( CommandStepCompleted ) - parameter was null",
                        nameof(Command_Notifications_Projection));
                }
                #endregion
            }
        }

        public void HandleEvent(CommandCompleted  eventHandled)
        {
            #region Logging
            if (null != log)
            {
                log.LogDebug($"HandleEvent( CommandCompleted )",
                    nameof(Command_Notifications_Projection));
            }
            #endregion

            if (null != eventHandled)
            {
                complete = true;
            }
        }

        public Command_Notifications_Projection(ILogger logIn = null)
        { 
            if (null != logIn)
            {
                log = logIn;
            }
        }
    }


    /// <summary>
    /// Extension classes for making the code dealing with collections of ReturnHookAdded more readable
    /// </summary>
    public static class ReturnHookAddedExtensions
    {

        /// <summary>
        /// The set of return hooks that should be notified if the command has failed
        /// </summary>
        /// <param name="source">
        /// The set of all the ReturnHookAdded events 
        /// </param>
        public static IEnumerable<ReturnHookAdded > ForFailedCommands(this IEnumerable<ReturnHookAdded> source)
        {
            if (null != source )
            {
                return source.Where(f => f.NotifyOnError == true);
            }
            else
            {
                // Return an empty set for composability
                return Enumerable.Empty<ReturnHookAdded>();
            }
        }


        /// <summary>
        /// The set of return hooks that should be notified if the command has completed
        /// </summary>
        /// <param name="source">
        /// The set of all the ReturnHookAdded events 
        /// </param>
        public static IEnumerable<ReturnHookAdded> ForCompletedCommands(this IEnumerable<ReturnHookAdded> source)
        {
            if (null != source)
            {
                return source.Where(f => f.NotifyOnCompletion == true);
            }
            else
            {
                // Return an empty set for composability
                return Enumerable.Empty<ReturnHookAdded>();
            }
        }


        /// <summary>
        /// The set of return hooks that should be notified after each step of a command is completed
        /// </summary>
        /// <param name="source">
        /// The set of all the ReturnHookAdded events 
        /// </param>
        public static IEnumerable<ReturnHookAdded> ForCommandSteps(this IEnumerable<ReturnHookAdded> source)
        {
            if (null != source)
            {
                return source.Where(f => f.NotifyStepComplete == true);
            }
            else
            {
                // Return an empty set for composability
                return Enumerable.Empty<ReturnHookAdded>();
            }
        }

    }
}
