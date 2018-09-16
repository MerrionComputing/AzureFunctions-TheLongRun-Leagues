
namespace TheLongRun.Common.Orchestration
{
    /// <summary>
    /// Parameters for starting a new instance of a query orchestration.
    /// </summary>
    public class StartQueryOrchestrationArgs
    {

        /// <summary>
        /// Gets or sets the query name of the orchestrator function to start.
        /// </summary>
        /// <value>
        /// This name will be used to look up the name of the function to run
        /// </value>
        public string QueryName { get; set; }

        /// <summary>
        /// Gets or sets the instance ID to assign to the started orchestration and to identify the
        /// underlying event stream
        /// </summary>
        /// <remarks>
        /// If this property value is null (the default), then a randomly generated instance ID will be assigned automatically.
        /// </remarks>
        /// <value>The instance ID to assign.</value>
        public string InstanceId { get; set; }

        /// <summary>
        /// Gets or sets the JSON-serializeable input data for the orchestrator function.
        /// </summary>
        /// <value>JSON-serializeable input value for the orchestrator function.</value>
        public object QueryOrchestrationInput { get; set; }

        public StartQueryOrchestrationArgs(string queryName,
            object queryOrchestrationInput)
        {
            this.QueryName = queryName;
            this.QueryOrchestrationInput = queryOrchestrationInput;
        }

        /// <summary>
        /// Parameter-less constructor for serialisation/instantiation
        /// </summary>
        public StartQueryOrchestrationArgs()
        {
        }
    }
}
