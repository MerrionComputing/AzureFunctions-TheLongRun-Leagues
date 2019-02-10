using System;
using System.Collections.Generic;
using System.Text;
using TheLongRun.Common.Attributes;

namespace TheLongRun.Common.Model
{
    /// <summary>
    /// A single classifier for a domain entity type
    /// </summary>
    public class ClassifierDefinition
        : IDurableFunctionBackedDefinition
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


        public ClassifierDefinition(string classifierDefinitionName,
            string durableFunctionName = @"")
        {
            _classifierDefinitionName = classifierDefinitionName;
            if (!string.IsNullOrWhiteSpace(durableFunctionName))
            {
                _durableFunctionName = durableFunctionName;
            }
            else
            {
                // default to the standard default classifier function name
                _durableFunctionName = ClassifierNameAttribute.MakeClassifierFunctionName(classifierDefinitionName);
            }
        }

    }
}
