using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheLongRun.Common.Orchestration.Attributes;

namespace TheLongRun.Common.Orchestration
{
    /// <summary>
    /// A base class for any CLASSIFIER orchestrator functions
    /// </summary>
    public  class EventStreamBackedClassifierOrchestrator
        : EventStreamBackedOrchestratorBase,
          IEventStreamBackedOrchestrator
    {
        public  bool IsComplete { get; }

        /// <summary>
        /// Orchestrator classification type is a CLASSIFIER
        /// </summary>
        public string ClassificationTypeName
        {
            get
            {
                return EventStreamBackedClassifierOrchestrator.ClassifierTypeName ;
            }
        }

        private readonly string _classifierName;
        public string Name
        {
            get
            {
                return _classifierName;
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
        /// The unique key of the (entity) event stream over which the classifier
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
                    OrchestrationCallbackIdentity.OrchestrationClassifications.Classifier,
                    Name,
                    UniqueIdentifier);
            }
        }


        protected internal EventStreamBackedClassifierOrchestrator(Guid uniqueIdentifier,
            string classifierName)
        {
            if (uniqueIdentifier.Equals(Guid.Empty))
            {
                _uniqueIdentifier = Guid.NewGuid();
            }
            else
            {
                _uniqueIdentifier = uniqueIdentifier;
            }
            _classifierName = classifierName;
        }

        public static string ClassifierTypeName
        {
            get
            {
                return @"CLASSIFIER";
            }
        }

        public static EventStreamBackedClassifierOrchestrator CreateFromAttribute(EventStreamBackedClassifierOrchestrationTriggerAttribute attr)
        {
            if (attr.InstanceIdentity.Equals(Guid.Empty))
            {
                attr.InstanceIdentity = Guid.NewGuid();
            }
            return new EventStreamBackedClassifierOrchestrator(attr.InstanceIdentity,  attr.InstanceName);
        }
    }
}
