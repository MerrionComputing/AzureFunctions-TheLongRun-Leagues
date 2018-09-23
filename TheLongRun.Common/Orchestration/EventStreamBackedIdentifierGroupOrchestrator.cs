using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheLongRun.Common.Orchestration.Attributes;

namespace TheLongRun.Common.Orchestration
{
    /// <summary>
    /// A base class for any IDENTIFIER GROUP orchestrator functions
    /// </summary>
    /// <remarks>
    /// This is a business meaninful collection of entities that have been selected
    /// by either a classifier or some rule over a projection
    /// </remarks>
    public  class EventStreamBackedIdentifierGroupOrchestrator
        : EventStreamBackedOrchestratorBase,
          IEventStreamBackedOrchestrator,
          IProjectionRunner,
          IClassifierRunner
    {
        public  bool IsComplete { get; }

        /// <summary>
        /// Orchestrator classification type is a GROUP
        /// </summary>
        public string ClassificationTypeName
        {
            get
            {
                return EventStreamBackedIdentifierGroupOrchestrator.ClassifierTypeName ;
            }
        }


        private readonly string _groupName;
        public string Name
        {
            get
            {
                return _groupName;
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
                    OrchestrationCallbackIdentity.OrchestrationClassifications.IdentifierGroup ,
                    Name,
                    UniqueIdentifier);
            }
        }


        protected internal EventStreamBackedIdentifierGroupOrchestrator(Guid uniqueIdentifier,
            string groupName = null,
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
            _groupName = groupName;
            if (null != calledBy)
            {
                base.CalledBy = calledBy;
            }
        }

        public static string ClassifierTypeName
        {
            get
            {
                return @"GROUP";
            }
        }

        public static EventStreamBackedIdentifierGroupOrchestrator CreateFromAttribute(EventStreamBackedIdentifierGroupOrchestrationTriggerAttribute attr)
        {
            if (attr.InstanceIdentity.Equals(Guid.Empty))
            {
                attr.InstanceIdentity = Guid.NewGuid();
            }
            return new EventStreamBackedIdentifierGroupOrchestrator(attr.InstanceIdentity, attr.InstanceName);
        }

        #region Projection Runner

        public async Task<IProjectionResponse> RunProjectionAsync(string projectionName,
            string instanceId,
            string aggregateKey,
            DateTime? asOfDate = null,
            int? asOfSequence = null)
        {
            // Validate the inputs...
            if (string.IsNullOrWhiteSpace(projectionName))
            {
                throw new ArgumentException($"Projection name not set");
            }

            if (string.IsNullOrEmpty(instanceId))
            {
                instanceId = Guid.NewGuid().ToString("N");
            }

            if (string.IsNullOrWhiteSpace(aggregateKey))
            {
                throw new ArgumentException($"Projection requires a valid aggregate key");
            }

            // TODO: Build a callout/callback definition

            // TODO: Spawn the projection
            return await Task.FromException<IProjectionResponse>(new NotImplementedException());
        }
        #endregion

        #region Classifier Runner
        public async Task<IClassifierResponse> RunClassifierAsync(string classifierName,
            string instanceId,
            string aggregateKey,
            DateTime? asOfDate = null,
            int? asOfSequence = null)
        {

            // Validate the inputs...
            if (string.IsNullOrWhiteSpace(classifierName))
            {
                throw new ArgumentException($"Classifier name not set");
            }

            if (string.IsNullOrEmpty(instanceId))
            {
                instanceId = Guid.NewGuid().ToString("N");
            }

            if (string.IsNullOrWhiteSpace(aggregateKey))
            {
                throw new ArgumentException($"Classifier requires a valid aggregate key");
            }

            // TODO: Build a callout/callback definition

            // TODO: Spawn the projection
            return await Task.FromException<IClassifierResponse>(new NotImplementedException());
        }
        #endregion
    }
}
