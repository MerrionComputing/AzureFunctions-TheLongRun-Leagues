using System;
using System.Collections.Generic;
using System.Text;

namespace TheLongRun.Common.Model
{
    /// <summary>
    /// A top level domain that can contain a distinct set of entitiy types and their associated events, 
    /// commands, projections, queries, classifiers and defined identifier groups.
    /// </summary>
    public class Domain
    {



        /// <summary>
        /// The unique (within the current model) domain name 
        /// </summary>
        private readonly string _domainName;
        public string Name
        {
            get
            {
                return _domainName;
            }
        }

        /// <summary>
        /// The set of entity types linked to this domain
        /// </summary>
        private readonly EntityTypes _entities;
        public EntityTypes EntityTypes
        {
            get
            {
                return _entities;
            }
        }

        internal void AddEntityType(EntityType entityToAdd)
        {
            _entities.AddEntityType(entityToAdd); 
        }

        /// <summary>
        /// The set of command definitions linked to this domain
        /// </summary>
        private readonly CommandDefinitions _commandDefinitions;
        public CommandDefinitions CommandDefinitions
        {
            get
            {
                return _commandDefinitions;
            }
        }

        internal void AddCommandDefinition(CommandDefinition commandDefinitionToAdd )
        {
            _commandDefinitions.AddCommandDefinition(commandDefinitionToAdd);
        }

        /// <summary>
        /// The set of query definitions linked to this domain
        /// </summary>
        private readonly QueryDefinitions _queryDefintions;
        public QueryDefinitions QueryDefinitions
        {
            get
            {
                return _queryDefintions;
            }
        }

        internal void AddQueryDefinition( QueryDefinition queryDefinitionToAdd)
        {
            _queryDefintions.AddQueryDefinition(queryDefinitionToAdd);
        }

        public Domain(string domainName)
        {
            _domainName = domainName;
            _entities = new EntityTypes();
            _commandDefinitions = new CommandDefinitions();
            _queryDefintions = new QueryDefinitions();
        }
    }


    /// <summary>
    /// Methods for constructing the domain model usin a fluent syntax
    /// </summary>
    public static partial class FluentInterface
    {

        /// <summary>
        /// Add a query definition to this domain model
        /// </summary>
        public static Domain Add(this Domain domain, QueryDefinition queryToAdd)
        {
            domain.AddQueryDefinition(queryToAdd);
            return domain;
        }

        /// <summary>
        /// Add a command definition to the domain
        /// </summary>
        public static Domain Add(this Domain domain, CommandDefinition commandToAdd )
        {
            domain.AddCommandDefinition(commandToAdd);
            return domain;
        }

        /// <summary>
        /// Add an entity definition to the domain
        /// </summary>
        public static Domain Add(this Domain domain, EntityType  entityToAdd)
        {
            domain.AddEntityType(entityToAdd.SetDomain(domain.Name ) );
            return domain;
        }
    }
}
