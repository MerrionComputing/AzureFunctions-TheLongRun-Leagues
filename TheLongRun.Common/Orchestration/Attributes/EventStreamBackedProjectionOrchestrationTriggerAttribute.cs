using System;
using System.Diagnostics;
using Microsoft.Azure.WebJobs.Description;

namespace TheLongRun.Common.Orchestration.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    [Binding(TriggerHandlesReturnValue = false)]
    public sealed class EventStreamBackedProjectionOrchestrationTriggerAttribute
        : EventStreamBackedOrchestrationTriggerAttribute
    {

        public EventStreamBackedProjectionOrchestrationTriggerAttribute(
                string domainName,
                string instanceName,
                Guid instanceIdentity)
                : base(domainName,
          EventStreamBackedProjectionOrchestrator.ClassifierTypeName,
          instanceName,
          instanceIdentity)
        {
        }
    }
}
