using System;
using System.Collections.Generic;
using System.Text;

namespace TheLongRun.Common.Model
{
    public class ProjectionDefinition
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

        public ProjectionDefinition(string projectionDefinitionName)
        {
            _projectionDefinitionName = projectionDefinitionName;
        }

    }
}
