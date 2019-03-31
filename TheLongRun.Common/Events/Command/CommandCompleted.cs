using System;
using System.Runtime.Serialization;
using CQRSAzure.EventSourcing;


namespace TheLongRun.Common.Events.Command
{

    /// <summary>
    /// A new command has been marked as completed
    /// </summary>
    /// <remarks>
    /// This allows for it to be archived or purged if neccessary
    /// </remarks>
    [Serializable()]
    [CQRSAzure.EventSourcing.DomainNameAttribute(Constants.Domain_Command)]
    [CQRSAzure.EventSourcing.Category(Constants.Domain_Command)]
    [CQRSAzure.EventSourcing.EventAsOfDateAttribute(nameof(Date_Completed))]
    public class CommandCompleted
        : IEvent
    {

        /// <summary>
        /// The date/time the command was completed by the system
        /// </summary>
        public DateTime Date_Completed { get; set; }

        /// <summary>
        /// Notes relating to the command completion
        /// </summary>
        public string Notes { get; set; }


        public CommandCompleted(DateTime? dateCompletedIn,
            string notesIn)
        {
            if (dateCompletedIn.HasValue )
            {
                Date_Completed = dateCompletedIn.Value;
            }
            else
            {
                Date_Completed = DateTime.UtcNow;
            }

            Notes = notesIn;
        }

        /// <summary>
        /// Empty constructor for serialisation
        /// </summary>
        public CommandCompleted()
        {
        }

        public CommandCompleted(SerializationInfo info, StreamingContext context)
        {
            Date_Completed = info.GetDateTime(nameof(Date_Completed));
            Notes = info.GetString(nameof(Notes));
        }

    }
}
