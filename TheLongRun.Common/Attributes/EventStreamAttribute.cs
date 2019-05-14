using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using System;

using CQRSAzure.EventSourcing;
using Microsoft.Azure.WebJobs.Host.Bindings;

namespace TheLongRun.Common.Attributes
{
    /// <summary>
    /// An attribute to mark an event stream to use for output for appending events to
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false )]
    [Binding]
    public sealed class EventStreamAttribute
        : Attribute , IEventStreamUntypedIdentity
    {

        /// <summary>
        /// The domain name the entity instance belongs to
        /// </summary>
        private readonly string _domainName;
        [AutoResolve]
        public string DomainName
        {
            get
            {
                return _domainName;
            }
        }

        /// <summary>
        /// The entity type to which the event stream belongs
        /// </summary>
        private readonly string _entityTypeName;
        [AutoResolve]
        public string AggregateTypeName
        {
            get
            {
                return _entityTypeName;
            }
        }

        /// <summary>
        /// The unique identifier of the specific instance of the aggregate
        /// </summary>
        private readonly string _instanceKey;
        [AutoResolve]
        public string InstanceKey
        {
            get
            {
                return _instanceKey;
            }
        }

        public EventStreamAttribute(string domainName,
            string entityTypeName,
            string instanceKey)
        {
            _domainName = domainName;
            _entityTypeName =  entityTypeName;
            _instanceKey = instanceKey;
        }
    }
}
