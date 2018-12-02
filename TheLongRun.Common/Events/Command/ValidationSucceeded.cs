using System;
using System.Runtime.Serialization;
using CQRSAzure.EventSourcing;

namespace TheLongRun.Common.Events.Command
{
    /// <summary>
    /// The command passed it's validation step(s)
    /// </summary>
    [Serializable()]
    [CQRSAzure.EventSourcing.DomainNameAttribute("Command")]
    [CQRSAzure.EventSourcing.Category("Command")]
    [CQRSAzure.EventSourcing.EventAsOfDateAttribute("Date_Validated")]
    public class ValidationSucceeded
        : IEvent
    {


        /// <summary>
        /// The date/time the command was validated by the system
        /// </summary>
        public DateTime Date_Validated { get; set; }

        public ValidationSucceeded(DateTime dateValidatedIn)
        {
            Date_Validated = dateValidatedIn;
        }

        /// <summary>
        /// Empty constructor for serialisation
        /// </summary>
        public ValidationSucceeded() { }

        public ValidationSucceeded(SerializationInfo info, StreamingContext context)
        {
            Date_Validated = info.GetDateTime(nameof(Date_Validated ));
        }



        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize this event instance
        /// </summary>
        /// <remarks>
        /// The version number is also to be saved
        /// </remarks>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Date_Validated ), Date_Validated );
        }
    }
}
