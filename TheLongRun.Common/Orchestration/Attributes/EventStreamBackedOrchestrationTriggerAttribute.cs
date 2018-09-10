using System;
using System.Diagnostics;
using Microsoft.Azure.WebJobs.Description;

namespace TheLongRun.Common.Orchestration
{
    /// <summary>
    /// Trigger for event stream backed orchestration 
    /// </summary>
    /// <remarks>
    /// This parameter contains the orchestration being triggered and also
    /// any callback used to return to the originator.
    /// An event stream backed orchestration is always "fire-and-forget" so no return value
    /// is to be expected
    /// The routing is :
    /// Domain/Type/Name/Instance
    /// e.g.
    /// TheLongRun-Leagues/Command/Add-League/{1234-ABCD1-0A098A123E1F}
    /// </remarks>
    [AttributeUsage(AttributeTargets.Parameter)]
    [Binding(TriggerHandlesReturnValue =false  )]
    public abstract  class EventStreamBackedOrchestrationTriggerAttribute
        : Attribute
    {

        readonly string _domainName;
        public string DomainName
        {
            get
            {
                return _domainName;
            }
        }

        readonly string _classificationTypeName;
        public string ClassificationTypeName
        {
            get
            {
                return _classificationTypeName;
            }
        }

        private readonly string _instanceName;
        public string InstanceName
        {
            get
            {
                return _instanceName;
            }
        }

        private readonly Guid _instanceIdentity;
        public Guid InstanceIdentity
        {
            get
            {
                return _instanceIdentity;
            }
        }

        protected internal EventStreamBackedOrchestrationTriggerAttribute(
            string domainName,
            string classificationTypeName,
            string instanceName,
            Guid instanceIdentity)
        {
            _domainName = domainName;
            _classificationTypeName = classificationTypeName;
            _instanceName = instanceName;
            _instanceIdentity = instanceIdentity;
        }

        public override string ToString()
        {
            return $"{DomainName}/{ClassificationTypeName}/{InstanceName}/{InstanceIdentity}";
        }
    }
}
