using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLongRun.Common.Orchestration
{
    /// <summary>
    /// Interface defining how an orchestartor is classified and therefore 
    /// how we can find the underlying event stream for its events
    /// </summary>
    public interface IEventStreamBackedOrchestratorClassification
    {

        /// <summary>
        /// Top level classification of this orchestrator
        /// </summary>
        /// <remarks>
        /// e.g. COMMAND, QUERY, PROJECTION, CLASSIFIER etc.
        /// </remarks>
        string ClassificationTypeName { get; }

        /// <summary>
        /// Name of the instance of the command, query etc that this 
        /// orchestration is for
        /// </summary>
        /// <remarks>
        /// For a command this is the command name e.g. "set-user-password"
        /// </remarks>
        string Name { get; }

        /// <summary>
        /// The unique identifier of this orchestration instance
        /// </summary>
        Guid UniqueIdentifier { get; }

        /// <summary>
        /// The additional context to use when running the orchestration
        /// </summary>
        /// <remarks>
        /// This may be ommitted if the "default" context is to be used
        /// </remarks>
        IEventStreamBackedOrchestratorContext Context { get; set; }
    }
}
