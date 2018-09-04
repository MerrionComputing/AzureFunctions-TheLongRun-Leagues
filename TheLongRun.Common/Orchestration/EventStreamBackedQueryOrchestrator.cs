using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLongRun.Common.Orchestration
{
    /// <summary>
    /// A base class for any QUERY orchestrator functions
    /// </summary>
    public abstract class EventStreamBackedQueryOrchestrator
        : IEventStreamBackedOrchestrator
    {
        public abstract bool IsComplete { get; }

        /// <summary>
        /// Orchestrator classification type is a QUERY
        /// </summary>
        public string ClassificationTypeName
        {
            get
            {
                return @"QUERY";
            }
        }


        public abstract string ClassificationInstanceName { get; }

        private readonly Guid _uniqueIdentifier;
        public Guid UniqueIdentifier
        {
            get
            {
                return _uniqueIdentifier;
            }
        }
        public abstract IEventStreamBackedOrchestratorContext Context { get; set; }

        public abstract void RunNextStep();

        protected internal EventStreamBackedQueryOrchestrator(Guid uniqueIdentifier)
        {
            if (uniqueIdentifier.Equals(Guid.Empty))
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
