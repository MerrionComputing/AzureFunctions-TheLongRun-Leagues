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
    /// The domain, instance name and instance identity can auto-resolve
    /// </remarks>
    [AttributeUsage(AttributeTargets.Parameter)]
    [Binding(TriggerHandlesReturnValue =false  )]
    public abstract  class EventStreamBackedOrchestrationTriggerAttribute
        : Attribute
    {

        [AutoResolve]
        public string DomainName { get; set; }


        readonly string _classificationTypeName;
        public string ClassificationTypeName
        {
            get
            {
                return _classificationTypeName;
            }
        }

        [AutoResolve ]
        public string InstanceName { get; set; }


        [AutoResolve ]
        public Guid InstanceIdentity { get; set; }


        protected internal EventStreamBackedOrchestrationTriggerAttribute(
            string domainName,
            string classificationTypeName,
            string instanceName,
            Guid instanceIdentity)
        {
            DomainName = domainName;
            _classificationTypeName = classificationTypeName;
            InstanceName = instanceName;
            InstanceIdentity = instanceIdentity;
        }

        public override string ToString()
        {
            return $"{DomainName}/{ClassificationTypeName}/{InstanceName}/{InstanceIdentity}";
        }
    }
}
