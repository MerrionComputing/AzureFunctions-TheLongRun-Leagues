using System;
using System.Collections.Generic;
using System.Text;

namespace TheLongRun.Common.Model
{
    /// <summary>
    /// A single named identifier group of instances of a given domain entity type
    /// </summary>
    public class IdentifierGroupDefinition
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

        public IdentifierGroupDefinition(string identifierGroupName)
        {
            _identifierGroupName = identifierGroupName;
        }

    }
}
