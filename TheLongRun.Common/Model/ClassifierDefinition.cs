using System;
using System.Collections.Generic;
using System.Text;

namespace TheLongRun.Common.Model
{
    /// <summary>
    /// A single classifier for a domain entity type
    /// </summary>
    public class ClassifierDefinition
    {

        /// <summary>
        /// The (unique per domain entity type) classifier 
        /// </summary>
        private readonly string _classifierDefinitionName;
        public string Name
        {
            get
            {
                return _classifierDefinitionName;
            }
        }

        public ClassifierDefinition(string classifierDefinitionName)
        {
            _classifierDefinitionName = classifierDefinitionName;
        }

    }
}
