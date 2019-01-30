using System;
using System.Collections.Generic;
using System.Text;

namespace TheLongRun.Common.Model
{
    /// <summary>
    /// The set of command definitions in this domain
    /// </summary>
    public class CommandDefinitions
    {

        private readonly Dictionary<string, CommandDefinition> _commandDefinitions;

        /// <summary>
        /// Indexer to get the command definition by name from this collection
        /// </summary>
        /// <param name="index">
        /// The name of the command type
        /// </param>
        public CommandDefinition  this[string index]
        {
            get
            {
                if (_commandDefinitions.ContainsKey(index))
                {
                    return _commandDefinitions [index];
                }
                return null;
            }
        }

        internal void AddCommandDefinition(CommandDefinition commandDefinitionToAdd)
        {
            if (! _commandDefinitions.ContainsKey(commandDefinitionToAdd.Name ) )
            {
                _commandDefinitions.Add(commandDefinitionToAdd.Name, commandDefinitionToAdd);
            }
        }


        public CommandDefinitions()
        {
            _commandDefinitions = new Dictionary<string, CommandDefinition>();
        }


    }
}
