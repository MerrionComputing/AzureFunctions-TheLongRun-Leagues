using System;
using System.Collections.Generic;
using System.Text;
using TheLongRun.Common.Attributes;

namespace TheLongRun.Common.Model
{
    /// <summary>
    /// A single named identifier group of instances of a given domain entity type
    /// </summary>
    public class IdentifierGroupDefinition
        : IDurableFunctionBackedDefinition
    {

        /// <summary>
        /// The (unique per domain entity type) name of the identifier group
        /// </summary>
        private readonly string _identifierGroupName;
        public string Name
        {
            get
            {
                return _identifierGroupName;
            }
        }

        /// <summary>
        /// The name of the durable function that provides the operation of this identifier group
        /// </summary>
        private readonly string _durableFunctionName;
        public string DurableFunctionName
        {
            get
            {
                return DurableFunctionName;
            }
        }

        public IdentifierGroupDefinition(string identifierGroupName,
            string durableFunctionName = @"")
        {
            _identifierGroupName = identifierGroupName;
            if (!string.IsNullOrWhiteSpace(durableFunctionName))
            {
                _durableFunctionName = durableFunctionName;
            }
            else
            {
                // default to the standard default identifier group function name
                _durableFunctionName = IdentifierGroupNameAttribute.MakeIdentifierGroupFunctionName(identifierGroupName);
            }
        }

    }
}
