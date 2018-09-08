using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLongRun.Common.Orchestration
{
    /// <summary>
    /// A base class for any PROJECTION orchestrator functions
    /// </summary>
    /// <remarks>
    /// A projection can be run on its own or on demand from a COMMAND,
    /// QUERY or IDENTIFIER GROUP orchestrator.  In the latter case it 
    /// will have a call-back to identify the name, type and unique 
    /// identifier of the results should be passed back to
    /// </remarks>
    public abstract class EventStreamBackedProjectionOrchestrator
       : IEventStreamBackedOrchestrator
    {
        public abstract bool IsComplete { get; }

        /// <summary>
        /// Orchestrator classification type is a PROJECTION
        /// </summary>
        public string ClassificationTypeName
        {
            get
            {
                return @"PROJECTION";
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

        private readonly string _instanceIdentifier;
        /// <summary>
        /// The unique key of the (entity) event stream over which the projection
        /// will be run
        /// </summary>
        public string InstanceIdentifier
        {
            get
            {
                return _instanceIdentifier;
            }
        }

        public abstract IEventStreamBackedOrchestratorContext Context { get; set; }

        public abstract void RunNextStep();

        protected internal EventStreamBackedProjectionOrchestrator(Guid uniqueIdentifier,
            string instanceIdentifierKey)
        {
            if (uniqueIdentifier.Equals(Guid.Empty))
            {
                _uniqueIdentifier = Guid.NewGuid();
            }
            else
            {
                _uniqueIdentifier = uniqueIdentifier;
            }
            _instanceIdentifier = instanceIdentifierKey;
        }

    }
}
