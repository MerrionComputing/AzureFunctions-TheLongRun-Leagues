using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLongRun.Common.Orchestration
{
    /// <summary>
    /// The identification of an orchestration that has called us so that when we 
    /// have results we know where to return them and who to notify when done
    /// </summary>
    /// <remarks>
    /// The called orchestration writes to the event stream of the caller to indicate
    /// the status
    /// </remarks>
    public class OrchestrationCallbackIdentity
    {

        /// <summary>
        /// The different types of orchestration that can call out to each other
        /// </summary>
        public enum OrchestrationClassifications
        {
            /// <summary>
            /// The classification type has not been set
            /// </summary>
            NotSet = 0,
            /// <summary>
            /// A top level command handler orchestration
            /// </summary>
            Command = 1,
            /// <summary>
            /// A top level query handler orchestration
            /// </summary>
            Query = 2,
            /// <summary>
            /// An identifier group which may use projectiosn or classifiers to perform 
            /// group membership tests
            /// </summary>
            IdentifierGroup =3,
            /// <summary>
            /// A projection that runs over a single event stream
            /// </summary>
            Projection = 4,
            /// <summary>
            /// A classifier that runs over a single event stream
            /// </summary>
            Classifier = 5
        }

        private readonly OrchestrationClassifications _classification;
        /// <summary>
        /// The type of orchestration that is at the end of the call-back
        /// </summary>
        public OrchestrationClassifications Classification
        {
            get
            {
                return _classification;
            }
        }

        private readonly string _instanceName;
        /// <summary>
        /// The name of the instance (like a class name) of the orchestration
        /// </summary>
        public string InstanceName
        {
            get
            {
                return _instanceName;
            }
        }

        private readonly Guid _instanceIdentity;
        /// <summary>
        /// The unique identifier of the orchestration that requires a call-back
        /// </summary>
        public Guid InstanceIdentity
        {
            get
            {
                return _instanceIdentity;
            }
        }

         internal OrchestrationCallbackIdentity(
             OrchestrationClassifications Classification,
             string Name,
             Guid Identity)
        {
            _classification = Classification;
            _instanceName = Name;
            _instanceIdentity = Identity;
        }

        public static OrchestrationCallbackIdentity Create(OrchestrationClassifications Classification,
             string Name,
             Guid Identity)
        {
            // Any validations or enrichment can happen here
            return new OrchestrationCallbackIdentity(Classification, Name, Identity);
        }

    }
}
