using System;
using System.Collections.Generic;
using System.Text;

namespace TheLongRun.Common.Model
{
    public class EntityType
    {

        /// <summary>
        /// The (unique within the domain) name of the entity type
        /// </summary>
        private readonly string _entityTypeName;
        public string Name
        {
            get
            {
                return _entityTypeName;
            }
        }

        /// <summary>
        /// The projection definitions for this domain entity type
        /// </summary>
        private readonly ProjectionDefinitions _projectionDefinitions;
        public ProjectionDefinitions ProjectionDefinitions
        {
            get
            {
                return _projectionDefinitions;
            }
        }

        internal void AddProjectionDefinition(ProjectionDefinition projectionToAdd)
        {
            _projectionDefinitions.AddProjectionDefinition(projectionToAdd);
        }

        /// <summary>
        /// The classifier definitions connected to this domain entity type
        /// </summary>
        private readonly ClassifierDefinitions _classifierDefinitions;
        public ClassifierDefinitions ClassifierDefinitions
        {
            get
            {
                return _classifierDefinitions;
            }
        }

        internal void AddClassifierDefinition(ClassifierDefinition classifierToAdd)
        {
            _classifierDefinitions.AddClassifierDefinition(classifierToAdd);
        }

        /// <summary>
        /// The event types in this domain
        /// </summary>
        private readonly EventTypes _eventTypes;
        public EventTypes EventTypes
        {
            get
            {
                return _eventTypes;
            }
        }

        internal void AddEventType(EventType eventTypeToAdd)
        {
            _eventTypes.AddEventType(eventTypeToAdd);
        }

        /// <summary>
        /// The defined identifier groups for this domain entity type
        /// </summary>
        private readonly IdentifierGroupDefinitions _identifierGroupDefinitions;
        public IdentifierGroupDefinitions IdentifierGroupDefinitions
        {
            get
            {
                return _identifierGroupDefinitions;
            }
        }

        internal void AddIdentifierGroupDefinition(IdentifierGroupDefinition identifierGroupDefinitionToAdd )
        {
            _identifierGroupDefinitions.AddIdentifierGroupDefinition(identifierGroupDefinitionToAdd);
        }

        public EntityType(string entityTypeName)
        {
            _entityTypeName = entityTypeName;
            _projectionDefinitions = new ProjectionDefinitions();
            _classifierDefinitions = new ClassifierDefinitions();
            _eventTypes = new EventTypes();
            _identifierGroupDefinitions = new IdentifierGroupDefinitions();
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
        public static EntityType Add(this EntityType entityType , IdentifierGroupDefinition  groupToAdd)
        {
            entityType.AddIdentifierGroupDefinition(groupToAdd);
            return entityType;
        }

        /// <summary>
        /// Add an event definition to this domain entity
        /// </summary>
        public static EntityType Add(this EntityType entityType, EventType  eventTypeToAdd)
        {
            entityType.AddEventType(eventTypeToAdd);
            return entityType;
        }

        /// <summary>
        /// Add an classifier definition to this domain entity
        /// </summary>
        public static EntityType Add(this EntityType entityType, ClassifierDefinition  classifierToAdd)
        {
            entityType.AddClassifierDefinition(classifierToAdd);
            return entityType;
        }

        /// <summary>
        /// Add a projection definition to this domain entity
        /// </summary>
        public static EntityType Add(this EntityType entityType, ProjectionDefinition projectionToAdd)
        {
            entityType.AddProjectionDefinition (projectionToAdd);
            return entityType;
        }
    }
}
