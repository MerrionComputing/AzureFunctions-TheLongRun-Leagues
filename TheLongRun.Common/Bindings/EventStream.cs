using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheLongRun.Common.Attributes;

namespace TheLongRun.Common.Bindings
{
    /// <summary>
    /// Wrapper around the CQRSAzure event stream 
    /// </summary>
    public class EventStream
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


        /*
        public async void AppendEvent<TEvent>(TEvent newEvent)
        {

        }
        */

        public EventStream(string domainName,
            string aggregateTypeName,
            string aggregateInstanceKey)
        {
            _domainName = domainName;
            _aggregateTypeName = aggregateTypeName;
            _aggregateInstanceKey = aggregateInstanceKey;
        }

        public EventStream(EventStreamAttribute attribute)
        {
            _domainName = attribute.DomainName ;
            _aggregateTypeName = attribute.AggregateTypeName;
            _aggregateInstanceKey = attribute.AggregateInstanceKey;
        }
    }
}
