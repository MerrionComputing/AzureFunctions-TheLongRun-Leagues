using System;

namespace TheLongRun.Common.Orchestration
{
    /// <summary>
    /// Parameters for starting a new instance of a classifier orchestration.
    /// </summary>
    /// <remarks>
    /// A classifier is responsible for determining if an aggregate instance is in
    /// or out of a given identifier group
    /// </remarks>
    public class StartClassifierOrchestrationArgs
    {
        /// <summary>
        /// Gets or sets the JSON-serializeable input data for the orchestrator function.
        /// </summary>
        /// <value>JSON-serializeable input value for the orchestrator function.</value>
        public object ClassifierOrchestrationInput { get; set; }

        /// <summary>
        /// The orchestration that should be notified when the classifier
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
        /// Gets or sets the classifier name of the orchestrator function to start.
        /// </summary>
        /// <value>
        /// This name will be used to look up the name of the function to run
        /// </value>
        public string ClassifierName { get; set; }


        public StartClassifierOrchestrationArgs(string classifierName,
                object classifierInput,
                OrchestrationCallbackIdentity callback,
                DateTime? asOfDate)
            : this(classifierName,
                  classifierInput,
                  callback)
        {
            this.AsOfDate = asOfDate;
        }

        public StartClassifierOrchestrationArgs(string classifierName,
                object classifierInput,
                OrchestrationCallbackIdentity callback)
            : this(classifierName, classifierInput)
        {
            this.CallbackIdentity = callback;
        }

        public StartClassifierOrchestrationArgs(string classifierName,
            object classifierInput)
        {
            this.ClassifierName = classifierName;
            this.ClassifierOrchestrationInput = classifierInput;
        }

        public StartClassifierOrchestrationArgs()
        {
        }

    }
}
