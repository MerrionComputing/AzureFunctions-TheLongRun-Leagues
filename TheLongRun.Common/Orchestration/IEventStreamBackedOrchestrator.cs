using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLongRun.Common.Orchestration
{
    /// <summary>
    /// Interface to be implemented by every different type of event stream backed
    /// orchestrator
    /// </summary>
    public interface IEventStreamBackedOrchestrator
        : IEventStreamBackedOrchestratorClassification
    {



        /// <summary>
        /// Is this orchestration marked as complete?
        /// </summary>
        bool IsComplete { get; }



        /// <summary>
        /// The identity by which any called orchestrations can call back with the 
        /// results (a return address style identity)
        /// </summary>
        OrchestrationCallbackIdentity CallbackIdentity { get; }


        /// <summary>
        /// The identity of the orchestration that called this one, and to which any response
        /// should be returned
        /// </summary>
        OrchestrationCallbackIdentity CalledBy { set; }

    }
}
