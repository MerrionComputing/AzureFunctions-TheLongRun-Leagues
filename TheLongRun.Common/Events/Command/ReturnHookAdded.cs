using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using CQRSAzure.EventSourcing;
using static TheLongRun.Common.CommandNotificationTarget;

namespace TheLongRun.Common.Events.Command
{
    /// <summary>
    /// A return hook was added to be notified of progress on this command
    /// </summary>
    [Serializable()]
    [CQRSAzure.EventSourcing.DomainNameAttribute("Command")]
    [CQRSAzure.EventSourcing.Category("Command")]
    public class ReturnHookAdded
        : IEvent
    {

        /// <summary>
        /// The type of hook (e.g. webhook, event grid etc)
        /// </summary>
        public CommandNotificationTarget.NotificationTargetType HookType { get; set; }

        /// <summary>
        /// The address of the hook to be notified
        /// </summary>
        public string HookAddress { get; set; }

        /// <summary>
        /// Should this hook be notified if an error occurs
        /// </summary>
        public bool NotifyOnError { get; set; }

        /// <summary>
        /// Should this hook be notified for every step completed
        /// </summary>
        public bool NotifyStepComplete { get; set; }

        /// <summary>
        /// Should this hook be notified when the handler completes
        /// </summary>
        public bool NotifyOnCompletion { get; set; }

        public ReturnHookAdded(NotificationTargetType hookTypeIn,
            string hookAddressIn,
            bool notifyErrorIn,
            bool notifyStepIn,
            bool notifyCompleteIn)
        {
            HookType = hookTypeIn;
            HookAddress = hookAddressIn;
            NotifyOnError = notifyErrorIn;
            NotifyStepComplete = notifyStepIn;
            NotifyOnCompletion = notifyCompleteIn;

        }

        /// <summary>
        /// Empty constructor for serialisation
        /// </summary>
        public ReturnHookAdded() { }

        public ReturnHookAdded(SerializationInfo info, StreamingContext context)
        {
            HookType  = (NotificationTargetType)info.GetValue  (nameof(HookType), typeof(NotificationTargetType));
            HookAddress  = info.GetString(nameof(HookAddress ));
            NotifyOnError  = info.GetBoolean(nameof(NotifyOnError ));
            NotifyStepComplete  = info.GetBoolean(nameof(NotifyStepComplete ));
            NotifyOnCompletion = info.GetBoolean(nameof(NotifyOnCompletion ));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(HookType ), HookType );
            info.AddValue(nameof(HookAddress ), HookAddress );
            info.AddValue(nameof(NotifyOnError ), NotifyOnError );
            info.AddValue(nameof(NotifyOnCompletion ), NotifyOnCompletion );
            info.AddValue(nameof(NotifyStepComplete ), NotifyStepComplete);
        }
    }

    public  static partial class Extensions
    {

        /// <summary>
        /// Return those return hooks that should be notified for errors only
        /// </summary>
        /// <param name="sourceRecords">
        /// The full set of source records of any type
        /// </param>
        public static IEnumerable<ReturnHookAdded> ForErrors(this IEnumerable<ReturnHookAdded> sourceRecords)
        {
            if (null != sourceRecords)
            {
                return sourceRecords.Where(f => f.NotifyOnError == true);
            }
            else
            {
                return Enumerable.Empty<ReturnHookAdded>();
            }
        }

        /// <summary>
        /// Return those return hooks that should be notified for commands that have completed only
        /// </summary>
        /// <param name="sourceRecords">
        /// The full set of source records of any type
        /// </param>
        public static IEnumerable<ReturnHookAdded> ForCompleteCommands(this IEnumerable<ReturnHookAdded> sourceRecords)
        {
            if (null != sourceRecords)
            {
                return sourceRecords.Where(f => f.NotifyOnCompletion == true);
            }
            else
            {
                return Enumerable.Empty<ReturnHookAdded>();
            }
        }

        /// <summary>
        /// Return those return hooks that should be notified for commands that have a step completed
        /// </summary>
        /// <param name="sourceRecords">
        /// The full set of source records of any type
        /// </param>
        public static IEnumerable<ReturnHookAdded> ForStepComplete(this IEnumerable<ReturnHookAdded> sourceRecords)
        {
            if (null != sourceRecords)
            {
                return sourceRecords.Where(f => f.NotifyStepComplete  == true);
            }
            else
            {
                return Enumerable.Empty<ReturnHookAdded>();
            }
        }


        /// <summary>
        /// Return those return hooks that should be notified by webhook
        /// </summary>
        /// <param name="sourceRecords">
        /// The full set of source records of any type
        /// </param>
        public static IEnumerable<ReturnHookAdded> ByWebhook(this IEnumerable<ReturnHookAdded> sourceRecords)
        {
            if (null != sourceRecords)
            {
                return sourceRecords.Where(f => f.HookType == NotificationTargetType.WebHook );
            }
            else
            {
                return Enumerable.Empty<ReturnHookAdded>();
            }
        }

        /// <summary>
        /// Return those return hooks that should be notified by webhook
        /// </summary>
        /// <param name="sourceRecords">
        /// The full set of source records of any type
        /// </param>
        public static IEnumerable<ReturnHookAdded> ByEventGridTopic(this IEnumerable<ReturnHookAdded> sourceRecords)
        {
            if (null != sourceRecords)
            {
                return sourceRecords.Where(f => f.HookType == NotificationTargetType.CustomEventGridTopic);
            }
            else
            {
                return Enumerable.Empty<ReturnHookAdded>();
            }
        }

        /// <summary>
        /// Return those return hooks that should be notified by webhook
        /// </summary>
        /// <param name="sourceRecords">
        /// The full set of source records of any type
        /// </param>
        public static IEnumerable<ReturnHookAdded> BySignalR(this IEnumerable<ReturnHookAdded> sourceRecords)
        {
            if (null != sourceRecords)
            {
                return sourceRecords.Where(f => f.HookType == NotificationTargetType.SignalR);
            }
            else
            {
                return Enumerable.Empty<ReturnHookAdded>();
            }
        }

    }
}
