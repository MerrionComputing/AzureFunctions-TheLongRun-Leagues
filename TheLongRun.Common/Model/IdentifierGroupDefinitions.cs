using System;
using System.Collections.Generic;
using System.Text;

namespace TheLongRun.Common.Model
{
    /// <summary>
    /// The set of identifier groups for a given domain entity type 
    /// </summary>
    public class IdentifierGroupDefinitions
    {

        private readonly Dictionary<string, IdentifierGroupDefinition> _identifierGroupDefinitions;


        /// <summary>
        /// Indexer to get the identifier group by name from this collection
        /// </summary>
        /// <param name="index">
        /// The name of the identifier group
        /// </param>
        public IdentifierGroupDefinition  this[string index]
        {
            get
            {
                if (_identifierGroupDefinitions.ContainsKey(index))
                {
                    return _identifierGroupDefinitions[index];
                }
                return null;
            }
        }

        internal void AddIdentifierGroupDefinition(IdentifierGroupDefinition identifierGroupDefinitionToAdd)
        {
            if (! _identifierGroupDefinitions.ContainsKey(identifierGroupDefinitionToAdd.Name ) )
            {
                _identifierGroupDefinitions.Add(identifierGroupDefinitionToAdd.Name, identifierGroupDefinitionToAdd);
            }
        }

        public IdentifierGroupDefinitions()
        {
            _identifierGroupDefinitions = new Dictionary<string, IdentifierGroupDefinition>();
        }


    }
}
