using System;

namespace TheLongRun.Common.Orchestration
{
    /// <summary>
    /// Parameters for starting a new instance of a projection orchestration.
    /// </summary>
    public class StartProjectionOrchestrationArgs
    {
        /// <summary>
        /// Gets or sets the JSON-serializeable input data for the orchestrator function.
        /// </summary>
        /// <value>JSON-serializeable input value for the orchestrator function.</value>
        public object ProjectionOrchestrationInput { get; set; }

        /// <summary>
        /// The orchestration that should be notified when the projection 
        /// orchestration completes and onto the event stream of which the result
        /// should be posted as an event
        /// </summary>
        public OrchestrationCallbackIdentity CallbackIdentity { get; set; }

        /// <summary>
        /// The as-of date to get the classifier run up to
        /// </summary>
        /// <remarks>
        /// If not set, the current date is assumed (i.e. as of last knowledge)
        /// </remarks>
        public DateTime? AsOfDate { get; set; }

        /// <summary>
        /// Gets or sets the instance ID to assign to the started orchestration
        /// </summary>
        /// <remarks>
        /// If this property value is null (the default), then a randomly generated instance ID will be assigned automatically.
        /// </remarks>
        /// <value>The instance ID to assign.</value>
        public string InstanceId { get; set; }

        /// <summary>
        /// Gets or sets the projection name of the orchestrator function to start.
        /// </summary>
        /// <value>
        /// This name will be used to look up the name of the function to run
        /// </value>
        public string ProjectionName { get; set; }


        public StartProjectionOrchestrationArgs(string projectionName,
                object projectionInput,
                OrchestrationCallbackIdentity callback,
                DateTime? asOfDate)
            : this(projectionName,
                  projectionInput,
                  callback)
        {
            this.AsOfDate = asOfDate;
        }

        public StartProjectionOrchestrationArgs(string projectionName,
                object projectionInput,
                OrchestrationCallbackIdentity callback)
            : this(projectionName, projectionInput)
        {
            this.CallbackIdentity = callback;
        }

        public StartProjectionOrchestrationArgs(string projectionName,
            object projectionInput)
        {
            this.ProjectionName = projectionName;
            this.ProjectionOrchestrationInput = projectionInput;
        }

        public StartProjectionOrchestrationArgs()
        {
        }

    }
}
