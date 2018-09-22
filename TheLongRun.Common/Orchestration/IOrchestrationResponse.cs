using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLongRun.Common.Orchestration
{
    /// <summary>
    /// Interface to be implemented by any kind of a response from a CQRS orchestration runner
    /// </summary>
    public interface IOrchestrationResponse
    {

        /// <summary>
        /// Identity of the orchestration from which this response is being returned
        /// </summary>
        /// <remarks>
        /// This is to allow the caller to rerun an orchestration if needs be
        /// </remarks>
        OrchestrationCallbackIdentity ResponseSource { get; }

    }

    /// <summary>
    /// Orchestration response that indicates the effective as-of date of the
    /// most recent event in its processing
    /// </summary>
    public interface IAsOfDateOrchestrationResponse
        : IOrchestrationResponse 
    {

        /// <summary>
        /// The effective date of the response
        /// </summary>
        /// <remarks>
        /// Where an event stream has effective date data this will be the effective date of the last 
        /// record read.
        /// This allows us to know if our response is considered "up to date enough" to use or not
        /// </remarks>
        Nullable<DateTime> AsOfDate { get; } 

    }

    /// <summary>
    /// Orchestration response that indicates the effective event sequence number of the
    /// most recent event in its processing
    /// </summary>
    /// <remarks>
    /// </remarks>
    public interface IAsOfSequenceOrchestrationResponse
        : IOrchestrationResponse
    {

        /// <summary>
        /// The effective sequence number of the response
        /// </summary>
        int AsOfSequenceNumber { get; }

    }
}
