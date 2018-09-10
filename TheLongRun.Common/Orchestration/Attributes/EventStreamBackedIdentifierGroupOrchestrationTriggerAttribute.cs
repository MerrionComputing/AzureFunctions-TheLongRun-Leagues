using System;
using System.Diagnostics;
using Microsoft.Azure.WebJobs.Description;

namespace TheLongRun.Common.Orchestration.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    [Binding(TriggerHandlesReturnValue = false)]
    public sealed class EventStreamBackedIdentifierGroupOrchestrationTriggerAttribute
        : EventStreamBackedOrchestrationTriggerAttribute
    {

        public EventStreamBackedIdentifierGroupOrchestrationTriggerAttribute(
                string domainName,
                string instanceName,
                Guid instanceIdentity)
                : base(domainName,
          EventStreamBackedIdentifierGroupOrchestrator.ClassifierTypeName,
          instanceName,
          instanceIdentity)
        {
        }
    }
}
