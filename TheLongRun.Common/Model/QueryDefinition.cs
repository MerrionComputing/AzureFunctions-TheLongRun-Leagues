using System;
using System.Collections.Generic;
using System.Text;

namespace TheLongRun.Common.Model
{
    /// <summary>
    /// A domain query definition
    /// </summary>
    /// <remarks>
    /// This is accessible outside of the domain to allow for inter-domain communication
    /// </remarks>
    public class QueryDefinition
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

        public QueryDefinition(string queryDefinitionName)
        {
            _queryDefinitionName = queryDefinitionName;
        }

    }
}
