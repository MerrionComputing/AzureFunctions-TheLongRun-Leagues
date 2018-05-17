using System;
using System.Runtime.Serialization;
using CQRSAzure.EventSourcing;

namespace TheLongRun.Common.Events.Command
{
    /// <summary>
    /// The processing of this command has completed
    /// </summary>
    [Serializable()]
    [CQRSAzure.EventSourcing.DomainNameAttribute("Command")]
    [CQRSAzure.EventSourcing.Category("Command")]
    [CQRSAzure.EventSourcing.EventAsOfDateAttribute("Date_Completed")]
    public class ProcessingCompleted
        : IEvent
    {

        /// <summary>
        /// The date/time the command processing was completed
        /// </summary>
        public DateTime Date_Completed { get; set; }

        /// <summary>
        /// The processing status of the command
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Additional comments on the command processing
        /// </summary>
        public string Commentary { get; set; }


        public ProcessingCompleted(string statusIn,
        string commentaryIn)
        {
            Date_Completed = DateTime.UtcNow;
            Status  = statusIn;
            Commentary  = commentaryIn ;
        }

        public ProcessingCompleted(SerializationInfo info, StreamingContext context)
        {
            Date_Completed = info.GetDateTime(nameof(Date_Completed));
            Status = info.GetString(nameof(Status));
            Commentary = info.GetString(nameof(Commentary));
        }


        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize this event instance
        /// </summary>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Date_Completed), Date_Completed);
            info.AddValue(nameof(Status ), Status );
            info.AddValue(nameof(Commentary ), Commentary );
        }
    }
}
