using System;
using System.Runtime.Serialization;
using CQRSAzure.EventSourcing;

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
        /// The type of hook (e.g. webhook, email etc)
        /// </summary>
        public string HookType { get; set; }

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

        public ReturnHookAdded(string  hookTypeIn,
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
            HookType  = info.GetString(nameof(HookType ));
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
}
