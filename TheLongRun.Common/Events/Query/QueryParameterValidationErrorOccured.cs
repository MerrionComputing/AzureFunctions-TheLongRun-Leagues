using System;
using System.Runtime.Serialization;
using CQRSAzure.EventSourcing;
namespace TheLongRun.Common.Events.Query
{
    /// <summary>
    /// A parameter to be used when executing the query has been found to be invalid
    /// </summary>
    /// <remarks>
    /// If this is fatal the query should be logged in error as no further processing 
    /// would make sense
    /// </remarks>
    [Serializable()]
    [CQRSAzure.EventSourcing.DomainNameAttribute("Query")]
    [CQRSAzure.EventSourcing.Category("Query")]
    public class QueryParameterValidationErrorOccured
        : IEvent 
    {

        /// <summary>
        /// The name of the parameter
        /// </summary>

        public string Name { get; set; }

        /// <summary>
        /// Does the validation error prevent the query from proceeding
        /// </summary>
        public bool Fatal { get; set; }

        /// <summary>
        /// The message to go along with the validation notification
        /// </summary>
        public string Message { get; set; }

        public QueryParameterValidationErrorOccured(string nameIn,
                         bool fatailIn,
                         string messageIn)
        {
            Name = nameIn;
            Fatal = fatailIn;
            Message = messageIn;
        }

        /// <summary>
        /// Empty constructor for serialisation
        /// </summary>
        public QueryParameterValidationErrorOccured()
        {
        }

        public QueryParameterValidationErrorOccured(SerializationInfo info, StreamingContext context)
        {
            Name = info.GetString(nameof(Name));
            Fatal  = info.GetBoolean (nameof(Fatal ));
            Message = info.GetString(nameof(Message));
        }

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize this event instance
        /// </summary>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Name), Name);
            info.AddValue(nameof(Fatal ), Fatal );
            info.AddValue(nameof(Message), Message);
        }
    }
}
