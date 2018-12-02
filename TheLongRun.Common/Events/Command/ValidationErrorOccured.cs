using System;
using System.Runtime.Serialization;
using CQRSAzure.EventSourcing;

namespace TheLongRun.Common.Events.Command
{
    /// <summary>
    /// A validation error occured processing this command
    /// </summary>
    [Serializable()]
    [CQRSAzure.EventSourcing.DomainNameAttribute("Command")]
    [CQRSAzure.EventSourcing.Category("Command")]
    [CQRSAzure.EventSourcing.EventAsOfDateAttribute("Date_Logged")]
    public class ValidationErrorOccured
        : IEvent
    {

        /// <summary>
        /// The date/time the error was logged by the system
        /// </summary>
        public DateTime Date_Logged { get; set; }

        
        /// <summary>
        /// The validation error message
        /// </summary>
        public string Message { get; set; }

        
        /// <summary>
        /// Is the error fatal (which should prevent any further processing until fixed)
        /// </summary>
        public bool Fatal { get; set; }

        public ValidationErrorOccured(string messageIn,
             bool fatalIn)
        {
            Date_Logged = DateTime.UtcNow;
            Message = messageIn;
            Fatal = fatalIn;
        }

        /// <summary>
        /// Empty constructor for serialisation
        /// </summary>
        public ValidationErrorOccured() { }

        public ValidationErrorOccured(SerializationInfo info, StreamingContext context)
        {
            Date_Logged = info.GetDateTime(nameof(Date_Logged));
            Message  = info.GetString(nameof(Message ));
            Fatal = info.GetBoolean(nameof(Fatal));
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
            info.AddValue(nameof(Message), Message );
            info.AddValue(nameof(Fatal ), Fatal );
        }
    }
}
