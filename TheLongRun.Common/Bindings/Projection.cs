using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheLongRun.Common.Attributes;

using CQRSAzure.EventSourcing;

namespace TheLongRun.Common.Bindings
{
    /// <summary>
    /// A class that takes care of running projections over the underlying event stream
    /// </summary>
    public class Projection
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
        private readonly string _aggregateInstanceKey;
        public string AggregateInstanceKey
        {
            get
            {
                return _aggregateInstanceKey;
            }
        }

        /// <summary>
        /// The type of the projection we are going to run 
        /// </summary>
        private readonly string _projectionTypeName;
        public string ProjectionTypeName
        {
            get
            {
                return _projectionTypeName;
            }
        }

        public Projection (string domainName,
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
