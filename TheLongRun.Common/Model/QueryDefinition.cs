using System;
using System.Collections.Generic;
using System.Text;
using TheLongRun.Common.Attributes;

namespace TheLongRun.Common.Model
{
    /// <summary>
    /// A domain query definition
    /// </summary>
    /// <remarks>
    /// This is accessible outside of the domain to allow for inter-domain communication
    /// </remarks>
    public class QueryDefinition
        : IDurableFunctionBackedDefinition
    {

        /// <summary>
        /// The (unique within the domain) name of the query definition
        /// </summary>
        private readonly string _queryDefinitionName;
        public string Name
        {
            get
            {
                return _queryDefinitionName;
            }
        }

        /// <summary>
        /// The name of the durable function that provides the operation of this query
        /// </summary>
        private readonly string _durableFunctionName;
        public string DurableFunctionName
        {
            get
            {
                return DurableFunctionName;
            }
        }

        public QueryDefinition(string queryDefinitionName,
            string durableFunctionName = @"")
        {
            _queryDefinitionName = queryDefinitionName;
            if (!string.IsNullOrWhiteSpace(durableFunctionName))
            {
                _durableFunctionName = durableFunctionName;
            }
            else
            {
                // default to the standard query name
                _durableFunctionName = QueryNameAttribute.MakeQueryFunctionName(queryDefinitionName);
            }
        }

    }
}
