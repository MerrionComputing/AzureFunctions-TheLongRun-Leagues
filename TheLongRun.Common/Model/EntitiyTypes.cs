using System;
using System.Collections.Generic;
using System.Text;

namespace TheLongRun.Common.Model
{
    /// <summary>
    /// The collection of entity types in a given domain
    /// </summary>
    /// <remarks>
    /// Entity type names must be unique within a domain but can be duplicated in a top level Domains model - allowing for different 
    /// domains to maintain their view of the same business entity as it pertains to that bounded context
    /// </remarks>
    public class EntityTypes
    {

        private readonly Dictionary<string, EntityType> _entityTypes;

        /// <summary>
        /// Indexer to get the entity type by name from this collection
        /// </summary>
        /// <param name="index">
        /// The name of the entity type
        /// </param>
        public EntityType this[string index]
        {
            get
            {
                if (_entityTypes.ContainsKey(index))
                {
                    return _entityTypes[index];
                }
                return null;
            }
        }

        public void AddEntityType(EntityType entityTypeToAdd)
        {
            if (!_entityTypes.ContainsKey(entityTypeToAdd.Name))
            {
                _entityTypes.Add(entityTypeToAdd.Name, entityTypeToAdd);
            }
        }


        public EntityTypes()
        {
            _entityTypes = new Dictionary<string, EntityType>();
        }
    }

    /// <summary>
    /// Methods for constructing the domain model usin a fluent syntax
    /// </summary>
    public static partial class FluentInterface
    {

        /// <summary>
        /// Add an entity type to the domain
        /// </summary> add
        /// <returns></returns>
        public static EntityTypes Add(this EntityTypes entityTypes , EntityType  entityToAdd)
        {
            entityTypes.AddEntityType(entityToAdd);
            return entityTypes;
        }

    }
}