using CQRSAzure.EventSourcing;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TheLongRun.Common.Bindings;
using TheLongRun.Common.Orchestration.Attributes;

namespace TheLongRun.Common.Orchestration
{
    /// <summary>
    /// A base class for any COMMAND orchestrator functions
    /// </summary>
    public  class EventStreamBackedCommandOrchestrator
        : EventStreamBackedOrchestratorBase,
          IEventStreamBackedOrchestrator,
          ICommandRunner,
          IQueryRunner,
          IIdentifierGroupRunner,
          IProjectionRunner,
          IClassifierRunner 
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
            string instanceName,
            OrchestrationCallbackIdentity calledBy = null)
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
            if (null != calledBy )
            {
                base.CalledBy = calledBy;
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


        #region Command Runner
        public async Task<ICommandResponse> RunCommandAsync(string commandName,
            string instanceId,
            JObject commandParameters = null)
        {

            // Validate the inputs...
            if (string.IsNullOrWhiteSpace(commandName))
            {
                throw new ArgumentException($"Projection name not set");
            }

            if (string.IsNullOrEmpty(instanceId))
            {
                instanceId = Guid.NewGuid().ToString("N");
            }

            // TODO: Build a callout/callback definition

            // TODO: Spawn the command
            return await Task.FromException<ICommandResponse>(new NotImplementedException());
        }
        #endregion

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

        #region Domain Entity Events
        /// <summary>
        /// Append an event onto the event stream of the domain entity (aka aggregate)
        /// </summary>
        /// <param name="domainName">
        /// The top level domain name
        /// </param>
        /// <param name="entityTypeName">
        /// The name of the type (class) of the entity to append the event to
        /// </param>
        /// <param name="entityInstanceKey">
        /// The unique identifier of the entity onto which we are appending the event
        /// </param>
        /// <param name="eventToAppend">
        /// The event we are appending to the entity instance event stream
        /// </param>
        public void AppendDomainEntityEvent(string domainName,
            string entityTypeName,
            string entityInstanceKey,
            IEvent eventToAppend)
        {

            // Validate the parameters
            if (string.IsNullOrWhiteSpace(domainName ) )
            {
                throw new ArgumentException("Domain name must be specified");
            }

            if (string.IsNullOrWhiteSpace(entityTypeName ) )
            {
                throw new ArgumentException("Entity type must be specified");
            }

            if (string.IsNullOrWhiteSpace(entityInstanceKey ) )
            {
                throw new ArgumentException("Entity instance unique identifier must be specified");
            }

            if (null == eventToAppend )
            {
                throw new ArgumentException("No event specified to append to the entity event stream");
            }

            EventStream targetStream = new EventStream(domainName,
                entityTypeName,
                entityInstanceKey);

            if (null != targetStream )
            {
                targetStream.AppendEvent(eventToAppend); 
            }


        }
        #endregion
    }
}
