using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheLongRun.Common.Orchestration.Attributes;

namespace TheLongRun.Common.Orchestration
{
    /// <summary>
    /// A base class for any COMMAND orchestrator functions
    /// </summary>
    public  class EventStreamBackedCommandOrchestrator
        : EventStreamBackedOrchestratorBase,
          IEventStreamBackedOrchestrator
    {
        public  bool IsComplete { get; }

        /// <summary>
        /// Orchestrator classification type is a COMMAND
        /// </summary>
        public  string ClassificationTypeName
        {
            get
            {
                return EventStreamBackedCommandOrchestrator.ClassifierTypeName ;
            }
        }

        private readonly string _commandName;
        public  string Name {
            get
            {
                return _commandName;
            }
        }

        private readonly Guid _uniqueIdentifier;
        public  Guid UniqueIdentifier
        { get
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
                    OrchestrationCallbackIdentity.OrchestrationClassifications.Command ,
                    Name ,
                    UniqueIdentifier);
            }
        }

        public void RunNextStep()
        {
            // TODO : Work out how to run the next step in the orchestration
        }

        protected internal EventStreamBackedCommandOrchestrator(Guid uniqueIdentifier,
            string instanceName = null)
        {
            if (uniqueIdentifier.Equals(Guid.Empty ) )
            {
                _uniqueIdentifier = Guid.NewGuid();
            }
            else
            {
                _uniqueIdentifier = uniqueIdentifier;
            }
            if (! string.IsNullOrWhiteSpace(instanceName) )
            {
                _commandName = instanceName;
            }
        }


        public static string ClassifierTypeName
        {
            get
            {
                return @"COMMAND";
            }
        }

        public static EventStreamBackedCommandOrchestrator CreateFromAttribute(EventStreamBackedCommandOrchestrationTriggerAttribute attr)
        {
            if (attr.InstanceIdentity.Equals(Guid.Empty )  )
            {
                attr.InstanceIdentity = Guid.NewGuid();
            }
            return new EventStreamBackedCommandOrchestrator(attr.InstanceIdentity, attr.InstanceName);
        }
    }
}
