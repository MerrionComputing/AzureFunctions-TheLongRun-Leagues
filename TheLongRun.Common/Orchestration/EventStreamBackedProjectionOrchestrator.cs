using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheLongRun.Common.Orchestration.Attributes;

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
    public  class EventStreamBackedProjectionOrchestrator
       : EventStreamBackedOrchestratorBase,
        IEventStreamBackedOrchestrator
    {
        public  bool IsComplete { get; }

        /// <summary>
        /// Orchestrator classification type is a PROJECTION
        /// </summary>
        public string ClassificationTypeName
        {
            get
            {
                return EventStreamBackedProjectionOrchestrator.ClassifierTypeName;
            }
        }

        private readonly string _projectionName;
        public string Name
        {
            get
            {
                return _projectionName;
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


        /// <summary>
        /// The unique key of the (entity) event stream over which the projection
        /// will be run
        /// </summary>
        public string InstanceIdentifier { get; set; }


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
                    OrchestrationCallbackIdentity.OrchestrationClassifications.Projection ,
                    Name,
                    UniqueIdentifier);
            }
        }



        protected internal EventStreamBackedProjectionOrchestrator(Guid uniqueIdentifier,
            string projectionName,
            OrchestrationCallbackIdentity calledBy = null)
        {
            if (uniqueIdentifier.Equals(Guid.Empty))
            {
                _uniqueIdentifier = Guid.NewGuid();
            }
            else
            {
                _uniqueIdentifier = uniqueIdentifier;
            }
            _projectionName = projectionName;
            if (null != calledBy)
            {
                base.CalledBy = calledBy;
            }
        }


        public static string ClassifierTypeName
        {
            get
            {
                return @"PROJECTION";
            }
        }

        //CreateFromAttribute
        public static EventStreamBackedProjectionOrchestrator CreateFromAttribute(EventStreamBackedProjectionOrchestrationTriggerAttribute attr)
        {
            if (attr.InstanceIdentity.Equals(Guid.Empty))
            {
                attr.InstanceIdentity = Guid.NewGuid();
            }
            return new EventStreamBackedProjectionOrchestrator(attr.InstanceIdentity, attr.InstanceName);
        }
    }
}
