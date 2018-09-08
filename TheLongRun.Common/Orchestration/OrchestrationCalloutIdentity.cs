using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLongRun.Common.Orchestration
{
    /// <summary>
    /// The parameters controlling one orchestration calling out for anotehr one to perform work
    /// for it - for example a query running a projection
    /// </summary>
    /// <remarks>
    /// This will trigger (or queue) the worker orchestration with a reference to the caller
    /// to be used for it to call back with the results.
    /// </remarks>
    public class OrchestrationCalloutIdentity
    {

        private readonly OrchestrationCallbackIdentity _sourceOrchestration;
        /// <summary>
        /// The source that is calling another orchestration to do work for it
        /// </summary>
        public OrchestrationCallbackIdentity SourceOrchestration
        {
            get
            {
                return _sourceOrchestration;
            }
        }

        private readonly OrchestrationCallbackIdentity _targetOrchestration;
        public OrchestrationCallbackIdentity TargetOrchestration
        {
            get
            {
                return _targetOrchestration;
            }
        }

        private readonly string _instanceIdentifier;
        /// <summary>
        /// The unique key of the (entity) event stream over which the called orchestration
        /// will be run
        /// </summary>
        /// <remarks>
        /// This may be empty if not meaningful to the called orchestration
        /// </remarks>
        public string InstanceIdentifier
        {
            get
            {
                return _instanceIdentifier;
            }
        }

        internal OrchestrationCalloutIdentity(OrchestrationCallbackIdentity source,
            OrchestrationCallbackIdentity target,
            string instance)
        {
            _sourceOrchestration = source;
            _targetOrchestration = target;
            _instanceIdentifier = instance;
        }

    }
}
