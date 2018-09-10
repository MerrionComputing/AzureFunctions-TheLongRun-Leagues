using System;
using System.Diagnostics;
using Microsoft.Azure.WebJobs.Description;

namespace TheLongRun.Common.Orchestration.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    [Binding(TriggerHandlesReturnValue = false)]
    public sealed class EventStreamBackedQueryOrchestrationTriggerAttribute
        : EventStreamBackedOrchestrationTriggerAttribute
    {

        public EventStreamBackedQueryOrchestrationTriggerAttribute(
                string domainName,
                string instanceName,
                Guid instanceIdentity)
                : base(domainName,
          EventStreamBackedQueryOrchestrator.ClassifierTypeName,
          instanceName,
          instanceIdentity)
        {
        }

    }
}
