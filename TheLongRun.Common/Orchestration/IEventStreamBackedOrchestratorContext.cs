using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLongRun.Common.Orchestration
{
    /// <summary>
    /// Additional contextual information to tag the event stream 
    /// for any given orchestration
    /// </summary>
    /// <remarks>
    /// This allows the non-business part of the process to be managed
    /// </remarks>
    public interface IEventStreamBackedOrchestratorContext
    {

        /// <summary>
        /// The user or owner that initiated this orchestration
        /// </summary>
        string Who { get; }

        /// <summary>
        /// Externally provided unique identifier for tying together actions undertaken
        /// by this orchestrator
        /// </summary>
        string CorrelationIdentifier { get; }

        /// <summary>
        /// Externally provided token for verifying the orchestration can be performed
        /// </summary>
        string AuthorisationToken { get; }
    }
}
