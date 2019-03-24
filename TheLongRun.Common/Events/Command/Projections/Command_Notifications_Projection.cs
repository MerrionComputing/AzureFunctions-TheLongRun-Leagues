using System;
using System.Collections.Generic;

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
        IProjectionUntyped
    {

        #region Private members
        private ILogger log = null;
        private List<ReturnHookAdded> targetHooks = new List<ReturnHookAdded>();
        #endregion

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

        public Command_Notifications_Projection(ILogger logIn = null)
        {
            if (null != logIn)
            {
                log = logIn;
            }
        }
    }
}
