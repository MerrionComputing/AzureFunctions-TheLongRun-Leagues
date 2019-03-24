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
        /// The entities impacted by the command
        /// </summary>
        public IEnumerable<CommandNotificationImpactedEntity > ImpactedEntities { get; set; }

    }


}
