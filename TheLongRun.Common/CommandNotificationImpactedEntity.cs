using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheLongRun.Common
{
    /// <summary>
    /// The identifier of an entity impacted by a command
    /// </summary>
    /// <remarks>
    /// A command notification will have 1..n impacted entities
    /// </remarks>
    public class CommandNotificationImpactedEntity
        : IEquatable<CommandNotificationImpactedEntity> 
    {

        /// <summary>
        /// The type of entity that the command impacted
        /// </summary>
        public string EntityType { get; set; } 

        /// <summary>
        /// The instance identifier by which the specific instance of the entity which the command impacted
        /// </summary>
        public string InstanceUniqueIdentifier { get; set; }

        public bool Equals(CommandNotificationImpactedEntity other)
        {
            if (null != other )
            {
                if (other.EntityType.Equals(EntityType )  )
                {
                    if (other.InstanceUniqueIdentifier.Equals(InstanceUniqueIdentifier ) )
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        public static CommandNotificationImpactedEntity Create(Tuple<string, string > entityImpacted)
        {
            return new CommandNotificationImpactedEntity()
            {
                EntityType = entityImpacted.Item1,
                InstanceUniqueIdentifier = entityImpacted.Item2
            };
        }

        public static IEnumerable<CommandNotificationImpactedEntity > CreateImpactedEntityList(Tuple<string,string >[] entitiesImpacted )
        {
            if (null != entitiesImpacted )
            {
                List<CommandNotificationImpactedEntity> impactedEntities = new List<CommandNotificationImpactedEntity>();
                foreach (var entityImpacted in entitiesImpacted)
                {
                    impactedEntities.Add(Create(entityImpacted));
                }
                return impactedEntities;
            }

            return Enumerable.Empty<CommandNotificationImpactedEntity>();

        }
    }
}
