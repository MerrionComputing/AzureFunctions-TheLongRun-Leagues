using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLongRun.Common.Attributes
{
    /// <summary>
    /// An attribute to mark an event stream to use for output for appending events to
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class EventStreamAttribute
        : Attribute 
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

        public EventStreamAttribute(string domainName,
            string aggregateTypeName,
            string aggregateInstanceKey)
        {
            _domainName = domainName;
            _aggregateTypeName = aggregateTypeName;
            _aggregateInstanceKey = aggregateInstanceKey;
        }
    }
}
