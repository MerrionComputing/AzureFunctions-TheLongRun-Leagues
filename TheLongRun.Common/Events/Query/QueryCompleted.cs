using System;
using System.Runtime.Serialization;
using CQRSAzure.EventSourcing;

namespace TheLongRun.Common.Events.Query
{
    /// <summary>
    /// A new query has been completed
    /// </summary>
    [Serializable()]
    [CQRSAzure.EventSourcing.DomainNameAttribute("Query")]
    [CQRSAzure.EventSourcing.Category("Query")]
    [CQRSAzure.EventSourcing.EventAsOfDateAttribute("Date_Completed")]
    public class QueryCompleted
        : IEvent
    {

        /// <summary>
        /// The date/time the command was marked as complete
        /// </summary>
        public DateTime Date_Completed { get; set; }

        /// <summary>
        /// Any message to go along with the results (for logging or similar)
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The number of rows of data returned
        /// </summary>
        public int ResultRecordsCount { get; set; }

        /// <summary>
        /// If the results are stored somewhere this is the URI to retrieve them
        /// </summary>
        /// <remarks>
        /// Some queries will have a large result set that you don't want to include 
        /// in the query event stream so the option to store them externally and pass
        /// a URI is added
        /// </remarks>
        public string ResultsURI { get; set; }

        public QueryCompleted(string MessageIn,
           int ResultsRecordsCountIn,
           string resultsURIIn)
        {
            Date_Completed  = DateTime.UtcNow;
            Message = MessageIn;
            ResultRecordsCount = ResultsRecordsCountIn;
            ResultsURI = resultsURIIn;
        }

        public QueryCompleted()
        { }

        public QueryCompleted(SerializationInfo info, StreamingContext context)
        {
            Date_Completed  = info.GetDateTime(nameof(Date_Completed ));
            Message  = info.GetString(nameof(Message ));
            ResultRecordsCount = info.GetInt32(nameof(ResultRecordsCount));
            ResultsURI = info.GetString(nameof(ResultsURI));
        }

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize this event instance
        /// </summary>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Date_Completed ), Date_Completed );
            info.AddValue(nameof(Message ), Message );
            info.AddValue(nameof(ResultRecordsCount ), ResultRecordsCount );
            info.AddValue(nameof(ResultsURI), ResultsURI);
        }

    }
}
