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
        /// The date/time the command was logged by the system
        /// </summary>
        private readonly DateTime _Date_Logged;
        public DateTime Date_Logged => _Date_Logged;

        
        /// <summary>
        /// The validation error message
        /// </summary>
        private readonly string _message;
        public string Message => _message;

        
        /// <summary>
        /// Is the error fatal (which should prevent any further processing until fixed)
        /// </summary>
        private readonly bool _fatal;
        public bool Fatal => _fatal;

        public ValidationErrorOccured(string messageIn,
             bool fatalIn)
        {
            _Date_Logged = DateTime.UtcNow;
            _message = messageIn;
            _fatal = fatalIn;
        }

        public ValidationErrorOccured(SerializationInfo info, StreamingContext context)
        {
            _Date_Logged = info.GetDateTime(nameof(Date_Logged));
            _message  = info.GetString(nameof(Message ));
            _fatal = info.GetBoolean(nameof(Fatal));
        }

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize this event instance
        /// </summary>
        /// <remarks>
        /// The version number is also to be saved
        /// </remarks>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Date_Logged), _Date_Logged);
            info.AddValue(nameof(Message), _message );
            info.AddValue(nameof(Fatal ), _fatal );
        }
    }
}
