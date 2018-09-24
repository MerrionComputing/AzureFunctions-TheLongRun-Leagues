using Microsoft.Azure.WebJobs.Description;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLongRun.Common.Attributes
{

    /// <summary>
    /// An attribute to mark a classifier to use for reading identity group membership from
    /// </summary>
    /// <remarks>
    /// This is not a trigger 
    /// </remarks>
    [AttributeUsage(AttributeTargets.Parameter)]
    [Binding]
    public class IdentifierGroupAttribute 
        : Attribute
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
        /// The specific identifier group type to get the membership for
        /// </summary>
        private readonly string _identifierGroupName;
        public string IdentifierGroupName
        {
            get
            {
                return _identifierGroupName;
            }
        }

        public IdentifierGroupAttribute(string domainName,
                                string aggregateTypeName,
                                string identifierGroupName)
        {
            _domainName = domainName;
            _aggregateTypeName = aggregateTypeName;
            _identifierGroupName = identifierGroupName;
        }
    }
}
