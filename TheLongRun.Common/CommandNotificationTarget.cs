using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace TheLongRun.Common
{
    public class CommandNotificationTarget
    {

        /// <summary>
        /// The types of target we can send notifications to
        /// </summary>
        public enum NotificationTargetType
        {
            /// <summary>
            /// No output type specified - just skip over this return target
            /// </summary>
            NotSet = 0,
            /// <summary>
            /// Use the defined webhook to pass notifications to
            /// </summary>
            WebHook = 1,
            /// <summary>
            /// Fire off an event grid event with the specified custom topic name to notify changes to this command 
            /// </summary>
            CustomEventGridTopic = 2,
            /// <summary>
            /// Send the notification out by a SignalR message
            /// </summary>
            SignalR = 3
        }

        /// <summary>
        /// Where to notify progress of this command
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public NotificationTargetType NotificationTarget { get; set; }

        /// <summary>
        /// Depending on the return target, this tells the command processor where 
        /// exactly to send notifications
        /// </summary>
        /// <remarks>
        /// This could be a storeage URI or a webhook or the custom topic name, for instance
        /// </remarks>
        public string ReturnPath { get; set; }

    }
}
