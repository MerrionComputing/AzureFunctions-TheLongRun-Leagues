using System;
using System.Collections.Generic;
using System.Text;

namespace TheLongRun.Common.Model
{
    /// <summary>
    /// The set of projection definitions linked to a domain entity type
    /// </summary>
    public class ProjectionDefinitions
    {

        private readonly Dictionary<string, ProjectionDefinition> _projectionDefinitions;


        /// <summary>
        /// Indexer to get the projection definition by name from this collection
        /// </summary>
        /// <param name="index">
        /// The name of the projection definition
        /// </param>
        public ProjectionDefinition  this[string index]
        {
            get
            {
                if (_projectionDefinitions.ContainsKey(index))
                {
                    return _projectionDefinitions[index];
                }
                return null;
            }
        }

        internal void AddProjectionDefinition(ProjectionDefinition projectionToAdd)
        {
            if (! _projectionDefinitions.ContainsKey(projectionToAdd.Name ) )
            {
                _projectionDefinitions.Add(projectionToAdd.Name,
                    projectionToAdd);
            }
        }

        public ProjectionDefinitions()
        {
            _projectionDefinitions = new Dictionary<string, ProjectionDefinition>();
        }


    }

    /// <summary>
    /// Methods for constructing the domain model using a fluent syntax
    /// </summary>
    public static partial class FluentInterface
    {

        /// <summary>
        /// Add an identifier group definition to this domain entity type
        /// </summary>
        public static ProjectionDefinitions Add(this ProjectionDefinitions projectionDefinitions , ProjectionDefinition projectionToAdd)
        {
            projectionDefinitions.AddProjectionDefinition(projectionToAdd);
            return projectionDefinitions;
        }

    }
    }
