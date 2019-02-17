using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using System;

namespace TheLongRun.Common.Attributes
{
    /// <summary>
    /// An attribute to mark a classifier to use for reading identity group membership from
    /// </summary>
    /// <remarks>
    /// This is not a trigger - we decide on a case by case basis what triggers a classifier
    /// and the same classifier may have different invocations
    /// </remarks>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    [Binding]
    public class ClassifierAttribute
        : Attribute,
        CQRSAzure.EventSourcing.IEventStreamUntypedIdentity
    {

        /// <summary>
        /// The domain name the aggregate instance belongs to
        /// </summary>
        private readonly string _domainName;
        public string DomainName
        {
            get
            {
                return _domainName;
            }
        }

        /// <summary>
        /// The aggregate type to which the event stream belongs
        /// </summary>
        private readonly string _aggregateTypeName;
        public string AggregateTypeName
        {
            get
            {
                return _aggregateTypeName;
            }
        }

        /// <summary>
        /// The unique identifier of the specific instance of the aggregate
        /// </summary>
        private readonly string _instanceKey;
        [AutoResolve]
        public string InstanceKey 
        {
            get
            {
                return _instanceKey;
            }
        }

        /// <summary>
        /// The specific classifier type to execute
        /// </summary>
        private readonly string _classifierTypeName;
        public string ClassifierTypeName
        {
            get
            {
                return _classifierTypeName;
            }
        }



        public ClassifierAttribute(string domainName,
                                string aggregateTypeName,
                                string instanceKey,
                                string classifierTypeName)
        {
            _domainName = domainName;
            _aggregateTypeName = aggregateTypeName;
            _instanceKey = instanceKey;
            _classifierTypeName = classifierTypeName;
        }

    }
}
