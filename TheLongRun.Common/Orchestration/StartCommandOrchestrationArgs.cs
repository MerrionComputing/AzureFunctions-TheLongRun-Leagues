
namespace TheLongRun.Common.Orchestration
{

    /// <summary>
    /// Parameters for starting a new instance of a command orchestration.
    /// </summary>
    public class StartCommandOrchestrationArgs
    {

        /// <summary>
        /// Gets or sets the command name of the orchestrator function to start.
        /// </summary>
        /// <value>
        /// This name will be used to look up the name of the function to run
        /// </value>
        public string CommandName { get; set; }

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
        public object CommandOrchestrationInput { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StartCommandOrchestrationArgs"/> class.
        /// </summary>
        /// <param name="commandName">
        /// The name of the command to start
        /// </param>
        /// <param name="commandOrchestrationInput">
        /// The JSON-serializeable input for the command orchestrator function
        /// </param>
        public StartCommandOrchestrationArgs(string commandName,
            object commandOrchestrationInput)
        {
            this.CommandName = commandName;
            this.CommandOrchestrationInput = commandOrchestrationInput;
        }

        /// <summary>
        /// Parameter-less constructor for serialisation
        /// </summary>
        public StartCommandOrchestrationArgs()
        {
        }
    }
}
