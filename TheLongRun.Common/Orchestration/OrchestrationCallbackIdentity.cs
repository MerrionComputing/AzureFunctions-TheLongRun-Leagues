using Newtonsoft.Json.Linq;
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

        /// <summary>
        /// Create a callback identity from a path 
        /// </summary>
        /// <param name="callbackIdentityPath">
        /// Domain/Type/Name/Instance
        /// e.g.
        /// TheLongRun-Leagues/Command/Add-League/{1234-ABCD1-0A098A123E1F}
        /// </param>
        /// <remarks>
        /// Can be inclusive of or exclusive of the domain name - in the latter
        /// </remarks>
        public static OrchestrationCallbackIdentity CreateFromPath(string callbackIdentityPath)
        {
            if (string.IsNullOrWhiteSpace(callbackIdentityPath ))
            {
                return null;
            }

            string[] components = callbackIdentityPath.Split('/');
            string _name = string.Empty ;
            OrchestrationClassifications _classification = OrchestrationClassifications.NotSet;
            Guid _identity = Guid.Empty;
            if (components.Length == 4)
            {
                _classification = ClassificationFromString(components[1]);
                _name = components[2];
                Guid.TryParse(components[3], out _identity);
            }
            else
            {
                if (components.Length == 3)
                {
                    _classification = ClassificationFromString(components[0]);
                    _name = components[1];
                    Guid.TryParse(components[2], out _identity);
                }
                else
                {
                    // Not a valid path
                    return null;
                }
            }

            if (_classification == OrchestrationClassifications.NotSet )
            {
                return null;
            }

            return OrchestrationCallbackIdentity.Create(_classification,
                _name,
                _identity); 
        }


        /// <summary>
        /// Get the classification type from its string representation
        /// </summary>
        /// <param name="classificationName">
        /// The string representation of the classification type e.g. "Command", "Query" etc.
        /// </param>
        public static OrchestrationClassifications ClassificationFromString(string classificationName)
        {
            if (! string.IsNullOrWhiteSpace(classificationName )  )
            {
                // Command
                if (classificationName.Equals(Orchestration.EventStreamBackedCommandOrchestrator.ClassifierTypeName,
                    StringComparison.OrdinalIgnoreCase ))
                {
                    return OrchestrationClassifications.Command;
                }

                // Query
                if (classificationName.Equals(Orchestration.EventStreamBackedQueryOrchestrator.ClassifierTypeName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return OrchestrationClassifications.Query ;
                }

                // Identifier group
                if (classificationName.Equals(Orchestration.EventStreamBackedIdentifierGroupOrchestrator.ClassifierTypeName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return OrchestrationClassifications.IdentifierGroup ;
                }

                // Classifier
                if (classificationName.Equals(Orchestration.EventStreamBackedClassifierOrchestrator.ClassifierTypeName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return OrchestrationClassifications.Classifier ;
                }

                // Projection
                if (classificationName.Equals(Orchestration.EventStreamBackedProjectionOrchestrator.ClassifierTypeName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return OrchestrationClassifications.Projection ;
                }
            }

            return OrchestrationClassifications.NotSet;
        }



    }
}
