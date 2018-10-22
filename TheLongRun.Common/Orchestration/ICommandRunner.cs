using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLongRun.Common.Orchestration
{
    public interface ICommandRunner
    {

        /// <summary>
        /// Runs the specified command
        /// </summary>
        /// <param name="commandName">
        /// The domain qualified name of the command to run
        /// </param>
        /// <param name="instanceId">
        /// The global identifier of the command instance if we are re-using an existing one 
        /// </param>
        /// <param name="commandParameters">
        /// Parameters to be passed into the command 
        /// (The specific class of this will depend on the command itself)
        /// </param>
        /// <param name="calledBy">
        /// The orchestration that called this command and to which the results should be returned
        /// (This may be null for a top-level orchestration)
        /// </param>
        Task<ICommandResponse> RunCommandAsync(string commandName,
            string instanceId,
            JObject commandParameters = null);

    }

    /// <summary>
    /// A response to indicate that a command has completed
    /// </summary>
    /// <remarks>
    /// No status is passed back as we want to allow for completely
    /// "fire-and-forget" execution of commands if the business needs
    /// </remarks>
    public interface ICommandResponse
        : IAsOfDateOrchestrationResponse
    {

    }
}
