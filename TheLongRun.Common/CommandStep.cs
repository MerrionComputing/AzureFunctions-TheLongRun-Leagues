using System;
using System.Collections.Generic;
using System.Text;

namespace TheLongRun.Common
{
    public class CommandStep
    {

        /// <summary>
        /// The name of the command being executed of which this is a step
        /// </summary>
        public string CommandName { get; set; }

        /// <summary>
        /// The system wide unique identifier by which this instance of a command request is known
        /// </summary>
        public Guid CommandUniqueIdentifier { get; set; }


        /// <summary>
        /// The name of the step in the command being performed
        /// </summary>
        public string StepName { get; set; }

        /// <summary>
        /// The entities that were impacted by this command setp
        /// </summary>
        public IEnumerable<CommandNotificationImpactedEntity> ImpactedEntities { get; set; }
    }

    public class CommandStepResponse
        : CommandStep 
    {

        /// <summary>
        /// The message returned from executing a command step successfully
        /// </summary>
        public string Message { get; set; }

    }
}
