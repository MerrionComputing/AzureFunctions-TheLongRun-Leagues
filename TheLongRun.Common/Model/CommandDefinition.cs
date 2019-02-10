using System;
using System.Collections.Generic;
using System.Text;
using TheLongRun.Common.Attributes;

namespace TheLongRun.Common.Model
{
    /// <summary>
    /// A single command definition within the domain
    /// </summary>
    public class CommandDefinition
        : IDurableFunctionBackedDefinition
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

        /// <summary>
        /// The name of the durable function that provides the operation of this command
        /// </summary>
        private readonly string _durableFunctionName;
        public string DurableFunctionName
        {
            get
            {
                return DurableFunctionName;
            }
        }

        public CommandDefinition(string commandDefinitionName,
            string durableFunctionName = @"")
        {
            _commandDefinitionName = commandDefinitionName;
            if (! string.IsNullOrWhiteSpace(durableFunctionName ) )
            {
                _durableFunctionName = durableFunctionName;
            }
            else
            {
                // default to the standard default command name
                _durableFunctionName = CommandNameAttribute.MakeCommandFunctionName(commandDefinitionName);
            }
        }

    }
}
