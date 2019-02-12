using System;
using System.Collections.Generic;
using System.Text;
using TheLongRun.Common.Attributes;

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
        /// The name of the connection string used to access this particular entity type
        /// </summary>
        /// <remarks>
        /// If not set the standard rules will be used to try and derive the connection string
        /// </remarks>
        private readonly string _connectionStringName;
        public string ConnectionStringName
        {
            get
            {
                return _connectionStringName;
            }
        }

        /// <summary>
        /// The underlying storage type to use 
        /// </summary>
        private readonly string _storageType = @"BlobStream";
        public string StorageType
        {
            get
            {
                return _storageType;
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

        public EntityType(string entityTypeName,
            string domainParentName = @"",
            string connectionStringName = @"",
            string storageType = @"BlobStream",
            ProjectionDefinitions projectionDefinitions = null,
            ClassifierDefinitions classifierDefinitions = null,
            EventTypes eventTypes = null,
            IdentifierGroupDefinitions identifierGroupDefinitions = null)
        {
            _entityTypeName = entityTypeName;
            if (! string.IsNullOrWhiteSpace(connectionStringName) )
            {
                _connectionStringName = connectionStringName;
            }
            else
            {
                // make a default connection string to use
                _connectionStringName = ConnectionStringNameAttribute.DefaultConnectionStringName(domainParentName, entityTypeName);
            }
            if (! string.IsNullOrWhiteSpace(storageType ))
            {
                _storageType = storageType;
            }

            if (null == projectionDefinitions)
            {
                _projectionDefinitions = new ProjectionDefinitions();
            }
            else
            {
                _projectionDefinitions = projectionDefinitions;
            }

            if (null == classifierDefinitions)
            {
                _classifierDefinitions = new ClassifierDefinitions();
            }
            else
            {
                _classifierDefinitions = classifierDefinitions;
            }

            if (null == eventTypes)
            {
                _eventTypes = new EventTypes();
            }
            else
            {
                _eventTypes = eventTypes;
            }

            if (null == identifierGroupDefinitions)
            {
                _identifierGroupDefinitions = new IdentifierGroupDefinitions();
            }
            else
            {
                _identifierGroupDefinitions = identifierGroupDefinitions;
            }
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

        /// <summary>
        /// Set the domain name to use for this entity
        /// </summary>
        /// <param name="entityToModify">
        /// The entity definition
        /// </param>
        /// <param name="domainName">
        /// The name of the parent domain containing the entity
        /// </param>
        public static EntityType SetDomain(this EntityType entityToModify, string domainName)
        {
            return new EntityType(entityToModify.Name, 
                domainName, 
                entityToModify.ConnectionStringName, 
                entityToModify.StorageType,
                entityToModify.ProjectionDefinitions ,
                entityToModify.ClassifierDefinitions,
                entityToModify.EventTypes,
                entityToModify.IdentifierGroupDefinitions );
        }


    }
}
