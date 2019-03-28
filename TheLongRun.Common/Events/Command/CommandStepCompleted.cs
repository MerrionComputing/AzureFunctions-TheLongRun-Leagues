using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using CQRSAzure.EventSourcing;
using Newtonsoft.Json;

namespace TheLongRun.Common.Events.Command
{

    /// <summary>
    /// A single step in a multiple-step command has completed
    /// </summary>
    /// <remarks>
    /// This is primarily used for providing feedback on the progress of a command.
    /// Ideally any business logic should be based off the state of the entity being worked on,
    /// rather than the state of the command itself
    /// </remarks>
    [Serializable()]
    [CQRSAzure.EventSourcing.DomainNameAttribute(Constants.Domain_Command)]
    [CQRSAzure.EventSourcing.Category(Constants.Domain_Command)]
    [CQRSAzure.EventSourcing.EventAsOfDateAttribute(nameof(Date_Logged))]
    public class CommandStepCompleted
        : IEvent 
    {

        /// <summary>
        /// The name of the step that has been completed
        /// </summary>
        public string StepName { get; set; }

        /// <summary>
        /// The date/time the command was logged by the system
        /// </summary>
        public DateTime Date_Logged { get; set; }


        /// <summary>
        /// The set of entities that this command step has impacted
        /// </summary>
        public IEnumerable<CommandNotificationImpactedEntity > ImpactedEntities { get; set; }

        /// <summary>
        /// Empty constructor for serialisation
        /// </summary>
        public CommandStepCompleted() { }

        public CommandStepCompleted(string stepNameIn,
            DateTime? dateLoggedIn,
            IEnumerable<CommandNotificationImpactedEntity > impacedEntitiesIn)
        {

            StepName = stepNameIn;
            if (dateLoggedIn.HasValue )
            {
                Date_Logged = dateLoggedIn.Value;
            }
            else
            {
                Date_Logged = DateTime.UtcNow;
            }
            ImpactedEntities = impacedEntitiesIn;

        }

        public CommandStepCompleted(SerializationInfo info, StreamingContext context)
        {
            Date_Logged = info.GetDateTime(nameof(Date_Logged));
            StepName  = info.GetString(nameof(StepName ));
            string impactedEntitiesJSON = info.GetString(nameof(ImpactedEntities));
            if (! string.IsNullOrWhiteSpace(impactedEntitiesJSON ) )
            {
                ImpactedEntities = JsonConvert.DeserializeObject<IEnumerable<CommandNotificationImpactedEntity>>(impactedEntitiesJSON ); 
            }
        }

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize this event instance
        /// </summary>
        /// <remarks>
        /// The version number is also to be saved
        /// </remarks>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Date_Logged), Date_Logged);
            info.AddValue(nameof(StepName ), StepName);
            if (null != ImpactedEntities )
            {
                // Serialise as JSON in a string 
                string impactedEntitiesJSON = JsonConvert.SerializeObject(ImpactedEntities);
                info.AddValue(nameof(ImpactedEntities), impactedEntitiesJSON);
            }
        }
    }
}
