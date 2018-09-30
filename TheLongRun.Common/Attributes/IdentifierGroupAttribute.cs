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

        private readonly string _identifierGroupFunctionName;
        public string IdentifierGroupFunctionName
        {
            get
            {
                return _identifierGroupFunctionName;
            }

        }

        public IdentifierGroupAttribute(string domainName,
                                string aggregateTypeName,
                                string identifierGroupName,
                                string identifierGroupFunctionName = "")
        {
            _domainName = domainName;
            _aggregateTypeName = aggregateTypeName;
            _identifierGroupName = identifierGroupName;
            if (string.IsNullOrWhiteSpace(identifierGroupFunctionName ) )
            {
                _identifierGroupFunctionName = identifierGroupName;
            }
            else
            {
                _identifierGroupFunctionName = identifierGroupFunctionName;
            }
        }


        /// <summary>
        /// Convert this to the default function name to use for an identifier group
        /// </summary>
        /// <returns>
        /// This is to allow a more easy to read set of function names in the attribute/code
        /// </returns>
        public FunctionNameAttribute GetDefaultFunctionName()
        {
            if (IdentifierGroupFunctionName.EndsWith("-IdentifierGroup"))
            {
                return new FunctionNameAttribute(IdentifierGroupFunctionName);
            }
            else
            {
                return new FunctionNameAttribute(IdentifierGroupFunctionName + @"-IdentifierGroup");
            }
        }
    }
}
