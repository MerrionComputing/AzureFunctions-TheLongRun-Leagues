using System;
using System.Diagnostics;
using Microsoft.Azure.WebJobs.Description;

namespace TheLongRun.Common.Orchestration.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    [Binding(TriggerHandlesReturnValue = false)]
    public sealed class EventStreamBackedClassifierOrchestrationTriggerAttribute
        : EventStreamBackedOrchestrationTriggerAttribute
    {

        public EventStreamBackedClassifierOrchestrationTriggerAttribute(
                string domainName,
                string instanceName,
                Guid instanceIdentity)
                : base(domainName,
          EventStreamBackedClassifierOrchestrator.ClassifierTypeName,
          instanceName,
          instanceIdentity)
        {
        }
    }
}
