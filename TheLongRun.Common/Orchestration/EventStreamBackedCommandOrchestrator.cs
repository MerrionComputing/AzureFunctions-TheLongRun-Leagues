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
          IEventStreamBackedOrchestrator,
          IProjectionRunner
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

        public async Task<IProjectionResponse > RunProjectionAsync(string projectionName, 
            string instanceId, 
            string aggregateKey, 
            DateTime? asOfDate = null, 
            int? asOfSequence = null)
        {
            // Validate the inputs...
            if (string.IsNullOrWhiteSpace(projectionName ))
            {
                throw new ArgumentException($"Projection name not set");
            }

            if (string.IsNullOrEmpty(instanceId))
            {
                instanceId = Guid.NewGuid().ToString("N");
            }

            if (string.IsNullOrWhiteSpace(aggregateKey ) )
            {
                throw new ArgumentException($"Projection requires a valid aggregate key");
            }

            // TODO: Build a callout/callback definition

            // TODO: Spawn the projection
            return await Task.FromException<IProjectionResponse>(new NotImplementedException()); 
        }
    }
}
