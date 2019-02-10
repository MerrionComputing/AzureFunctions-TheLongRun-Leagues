using System;
using System.Collections.Generic;
using System.Text;
using TheLongRun.Common.Attributes;

namespace TheLongRun.Common.Model
{
    public class ProjectionDefinition
        : IDurableFunctionBackedDefinition
    {

        /// <summary>
        /// The (unique per domain entity type) name of the projection definition
        /// </summary>
        private readonly string _projectionDefinitionName;
        public string Name
        {
            get
            {
                return _projectionDefinitionName;
            }
        }


        /// <summary>
        /// The name of the durable function that provides the operation of this projection
        /// </summary>
        private readonly string _durableFunctionName;
        public string DurableFunctionName
        {
            get
            {
                return DurableFunctionName;
            }
        }

        public ProjectionDefinition(string projectionDefinitionName,
            string durableFunctionName = @"")
        {
            _projectionDefinitionName = projectionDefinitionName;
            if (!string.IsNullOrWhiteSpace(durableFunctionName))
            {
                _durableFunctionName = durableFunctionName;
            }
            else
            {
                // default to the standard projection name
                _durableFunctionName = ProjectionNameAttribute.MakeProjectionFunctionName(projectionDefinitionName);
            }
        }

    }
}
