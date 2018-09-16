using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheLongRun.Common.Orchestration.Attributes;

namespace TheLongRun.Common.Orchestration
{
    /// <summary>
    /// A base class for any QUERY orchestrator functions
    /// </summary>
    public  class EventStreamBackedQueryOrchestrator
        : EventStreamBackedOrchestratorBase,
          IEventStreamBackedOrchestrator
    {
        public  bool IsComplete { get; }

        /// <summary>
        /// Orchestrator classification type is a QUERY
        /// </summary>
        public string ClassificationTypeName
        {
            get
            {
                return EventStreamBackedQueryOrchestrator.ClassifierTypeName;
            }
        }


        private readonly string _queryName;
        public string Name
        {
            get
            {
                return _queryName;
            }
        }


        private readonly Guid _uniqueIdentifier;
        public Guid UniqueIdentifier
        {
            get
            {
                return _uniqueIdentifier;
            }
        }
        public  IEventStreamBackedOrchestratorContext Context { get; set; }

        /// <summary>
        /// The identity by which any called orchestrations can call back with the 
        /// results (a return address style identity)
        /// </summary>
        public OrchestrationCallbackIdentity CallbackIdentity
        {
            get
            {
                return OrchestrationCallbackIdentity.Create(
                    OrchestrationCallbackIdentity.OrchestrationClassifications.Query ,
                    Name ,
                    UniqueIdentifier);
            }
        }

        public  void RunNextStep()
        {
            // TODO Run whatever is the next step of the orchestration
        }

        protected internal EventStreamBackedQueryOrchestrator(Guid uniqueIdentifier,
            string instanceName = null)
        {
            if (uniqueIdentifier.Equals(Guid.Empty))
            {
                _uniqueIdentifier = Guid.NewGuid();
            }
            else
            {
                _uniqueIdentifier = uniqueIdentifier;
            }
            _queryName = instanceName ;
        }


        public static string ClassifierTypeName
        {
            get
            {
                return @"QUERY";
            }
        }


        public static EventStreamBackedQueryOrchestrator CreateFromAttribute(EventStreamBackedQueryOrchestrationTriggerAttribute attr)
        {
            if (attr.InstanceIdentity.Equals(Guid.Empty))
            {
                attr.InstanceIdentity = Guid.NewGuid();
            }
            return new EventStreamBackedQueryOrchestrator(attr.InstanceIdentity, attr.InstanceName);
        }
    }
}
