using System;
using System.Runtime.Serialization;
using CQRSAzure.EventSourcing;
namespace TheLongRun.Common.Events.Query
{
    /// <summary>
    /// A new query has been created
    /// </summary>
    [Serializable()]
    [CQRSAzure.EventSourcing.DomainNameAttribute("Query")]
    [CQRSAzure.EventSourcing.Category("Query")]
    [CQRSAzure.EventSourcing.EventAsOfDateAttribute("Date_Logged")]
    public class QueryCreated
        : IEvent
    {

        private readonly string _queryName;
        public string QueryName => _queryName;

        /// <summary>
        /// The date/time the command was logged by the system
        /// </summary>
        private readonly DateTime _Date_Logged;
        public DateTime Date_Logged => _Date_Logged;

        private readonly Guid _queryIdentifier;
        public Guid QueryIdentifier => _queryIdentifier;

        public QueryCreated(string queryNameIn,
            Guid queryIdentifierIn)
        {
            _Date_Logged = DateTime.UtcNow;
            _queryName = queryNameIn;
            _queryIdentifier = queryIdentifierIn;
        }

        public QueryCreated(SerializationInfo info, StreamingContext context)
        {
            _Date_Logged = info.GetDateTime(nameof(Date_Logged));
            _queryIdentifier = (Guid)info.GetValue(nameof(QueryIdentifier), typeof(Guid));
            _queryName = info.GetString(nameof(QueryName));
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
            info.AddValue(nameof(QueryIdentifier), _queryIdentifier);
            info.AddValue(nameof(QueryName), _queryName);
        }
    }
}
