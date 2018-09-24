using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLongRun.Common.Attributes
{
    /// <summary>
    /// An attribute to mark a projection to use for reading data from
    /// </summary>
    /// <remarks>
    /// This is not a trigger - we decide on a case by case basis what triggers a projection
    /// and the same projection may have different invocations
    /// </remarks>
    [AttributeUsage(AttributeTargets.Parameter)]
    [Binding]
    public sealed class ProjectionAttribute
        : Attribute ,
        CQRSAzure.EventSourcing.IEventStreamUntypedIdentity 
    {

        /// <summary>
        /// The domain name the aggregate instance belongs to
        /// </summary>
        private readonly string _domainName;
        [AutoResolve]
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
        private readonly string _aggregateInstanceKey;
        [AutoResolve]
        public string InstanceKey
        {
            get
            {
                return _aggregateInstanceKey;
            }
        }

        /// <summary>
        /// The specific projection type to execute
        /// </summary>
        private readonly string _projectionTypeName;
        public string ProjectionTypeName
        {
            get
            {
                return _projectionTypeName;
            }
        }

        public ProjectionAttribute(string domainName,
                                string aggregateTypeName,
                                string aggregateInstanceKey,
                                string projectionTypeName)
        {
            _domainName = domainName;
            _aggregateTypeName = aggregateTypeName;
            _aggregateInstanceKey = aggregateInstanceKey;
            _projectionTypeName = projectionTypeName;
        }


    }
}
