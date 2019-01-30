using System;
using System.Collections.Generic;
using System.Text;

namespace TheLongRun.Common.Model
{
    /// <summary>
    /// A single command definition within the domain
    /// </summary>
    public class CommandDefinition
    {

        /// <summary>
        /// The (unique within the domain) name of the command
        /// </summary>
        /// <remarks>
        /// This can be entity qualified {entity type}.{command name} but this is not recommended as that may indicate a "smell" in the domain model
        /// </remarks>
        private readonly string _commandDefinitionName;
        public string Name
        {
            get
            {
                return _commandDefinitionName;
            }
        }


        public CommandDefinition(string commandDefinitionName)
        {
            _commandDefinitionName = commandDefinitionName;
        }

    }
}
