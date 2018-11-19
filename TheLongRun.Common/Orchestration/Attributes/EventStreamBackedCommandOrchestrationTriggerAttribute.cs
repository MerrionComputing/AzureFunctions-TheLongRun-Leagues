using System;
using System.Diagnostics;
using Microsoft.Azure.WebJobs.Description;

namespace TheLongRun.Common.Orchestration.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    [Binding()]
    public sealed class EventStreamBackedCommandOrchestrationTriggerAttribute
        : EventStreamBackedOrchestrationTriggerAttribute
    {

        public EventStreamBackedCommandOrchestrationTriggerAttribute(
            string domainName,
            string instanceName,
            Guid instanceIdentity)
            : base(domainName ,
                  EventStreamBackedCommandOrchestrator.ClassifierTypeName,
                  instanceName,
                  instanceIdentity )
        {
        }


    }
}
