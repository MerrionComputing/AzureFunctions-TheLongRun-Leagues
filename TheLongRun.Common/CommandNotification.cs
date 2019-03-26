using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace TheLongRun.Common
{
    /// <summary>
    /// A notification message to emit to tell the outside world that a command has been executed
    /// </summary>
    /// <remarks>
    /// This should be used to trigger a query if a subscriber wants to getthe latest information in 
    /// response to a notification
    /// </remarks>
    public class CommandNotification
    {

        /// <summary>
        /// The different things a command can notify 
        /// </summary>
        public enum CommandNotificationType
        {
            /// <summary>
            /// A command has completed successfully
            /// </summary>
            CommandComplete = 1,
            /// <summary>
            /// An intermediate step of a command has completed successfully
            /// </summary>
            StepComplete = 2,
            /// <summary>
            /// A command has halted in an error state
            /// </summary>
            Error = 3
        }

        /// <summary>
        /// The name of the command that was executed
        /// </summary>
        /// <remarks>
        /// This allows the reciepient to filter notifications based on their type
        /// </remarks>
        public string CommandName { get; set; }

        
        /// <summary>
        /// Where was the notification sent from
        /// </summary>
        /// <remarks>
        /// This allows the recipeint to filter notifications on their origin as well as their type
        /// </remarks>
        public string NotificationSource { get; set; }


        /// <summary>
        /// The type of command event that this message is for
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public CommandNotificationType NotificationType { get; set; }

        /// <summary>
        /// The entities impacted by the command
        /// </summary>
        public IEnumerable<CommandNotificationImpactedEntity > ImpactedEntities { get; set; }

    }


}
