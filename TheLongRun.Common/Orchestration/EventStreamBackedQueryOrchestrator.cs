using Newtonsoft.Json.Linq;
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
          IEventStreamBackedOrchestrator,
          IQueryRunner,
          IIdentifierGroupRunner,
          IProjectionRunner
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

        #region Query Runner
        public async Task<IQueryResponse> RunQueryAsync(string queryName,
            string instanceId,
            JObject queryParameters,
            DateTime? asOfDate = null)
        {
            if (string.IsNullOrWhiteSpace(queryName))
            {
                throw new ArgumentException($"Query name not set");
            }

            if (string.IsNullOrEmpty(instanceId))
            {
                instanceId = Guid.NewGuid().ToString("N");
            }

            // TODO: Build a callout/callback definition

            // TODO: Spawn the projection
            return await Task.FromException<IQueryResponse>(new NotImplementedException());
        }
        #endregion

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

        #region Identifier Group Runner
        public async Task<IEnumerable<IIdentifierGroupMemberResponse>> GetIdentifierGroupMembersAsync(string groupName,
            string instanceId,
            DateTime? asOfDate = null)
        {
            if (string.IsNullOrWhiteSpace(groupName))
            {
                throw new ArgumentException($"Group name not set");
            }

            if (string.IsNullOrEmpty(instanceId))
            {
                instanceId = Guid.NewGuid().ToString("N");
            }

            // TODO: Build a callout/callback definition

            // TODO: Spawn the projection
            return await Task.FromException<IEnumerable<IIdentifierGroupMemberResponse>>(new NotImplementedException());
        }
        #endregion
    }
}
