using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLongRun.Common.Orchestration
{
    /// <summary>
    /// A base class for any COMMAND orchestrator functions
    /// </summary>
    public abstract class EventStreamBackedCommandOrchestrator
        : EventStreamBackedOrchestratorBase,
          IEventStreamBackedOrchestrator
    {
        public abstract bool IsComplete { get; }

        /// <summary>
        /// Orchestrator classification type is a COMMAND
        /// </summary>
        public  string ClassificationTypeName
        {
            get
            {
                return @"COMMAND";
            }
        }


        public abstract string ClassificationInstanceName { get; }

        private readonly Guid _uniqueIdentifier;
        public  Guid UniqueIdentifier
        { get
            {
                return _uniqueIdentifier;
            }
        }
        public abstract IEventStreamBackedOrchestratorContext Context { get; set; }

        /// <summary>
        /// The identity by which any called orchestrations can call back with the 
        /// results (a return address style identity)
        /// </summary>
        public OrchestrationCallbackIdentity CallbackIdentity
        {
            get
            {
                return OrchestrationCallbackIdentity.Create(
                    OrchestrationCallbackIdentity.OrchestrationClassifications.Command ,
                    ClassificationInstanceName ,
                    UniqueIdentifier);
            }
        }

        public abstract void RunNextStep();

        protected internal EventStreamBackedCommandOrchestrator(Guid uniqueIdentifier)
        {
            if (uniqueIdentifier.Equals(Guid.Empty ) )
            {
                _uniqueIdentifier = Guid.NewGuid();
            }
            else
            {
                _uniqueIdentifier = uniqueIdentifier;
            }
        }

    }
}
