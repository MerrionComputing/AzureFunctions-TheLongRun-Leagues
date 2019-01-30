using System;
using System.Collections.Generic;
using System.Text;

namespace TheLongRun.Common.Model
{
    /// <summary>
    /// The set of query definitions linked to a domain
    /// </summary>
    public class QueryDefinitions
    {

        private readonly Dictionary<string, QueryDefinition> _queryDefinitions;

        /// <summary>
        /// Indexer to get the query definition by name from this collection
        /// </summary>
        /// <param name="index">
        /// The name of the query type
        /// </param>
        public QueryDefinition  this[string index]
        {
            get
            {
                if (_queryDefinitions.ContainsKey(index))
                {
                    return _queryDefinitions[index];
                }
                return null;
            }
        }

        internal void AddQueryDefinition(QueryDefinition queryDefinitionToAdd)
        {
            if (! _queryDefinitions.ContainsKey(queryDefinitionToAdd.Name ) )
            {
                _queryDefinitions.Add(queryDefinitionToAdd.Name, queryDefinitionToAdd);
            }
        }

        public QueryDefinitions()
        {
            _queryDefinitions = new Dictionary<string, QueryDefinition>();
        }


    }
}
