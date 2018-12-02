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

        public string QueryName { get; set; }

        /// <summary>
        /// The date/time the command was logged by the system
        /// </summary>
        public DateTime Date_Logged { get; set; }

        public Guid QueryIdentifier { get; set; }

        /// <summary>
        /// For queries that rely on authorisation this is the token passed in to test
        /// for the authorisation process
        /// </summary>
        public string AuthorisationToken { get; set; }

        public QueryCreated(string queryNameIn,
            Guid queryIdentifierIn,
            string aurhorisationTokenIn = @"")
        {
            Date_Logged = DateTime.UtcNow;
            QueryName = queryNameIn;
            QueryIdentifier = queryIdentifierIn;
            AuthorisationToken = aurhorisationTokenIn;
        }

        /// <summary>
        /// Empty constructor for serialisation
        /// </summary>
        public QueryCreated()
        {
        }

        public QueryCreated(SerializationInfo info, StreamingContext context)
        {
            Date_Logged = info.GetDateTime(nameof(Date_Logged));
            QueryIdentifier = (Guid)info.GetValue(nameof(QueryIdentifier), typeof(Guid));
            QueryName = info.GetString(nameof(QueryName));
            AuthorisationToken = info.GetString(nameof(AuthorisationToken));
        }



        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize this event instance
        /// </summary>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Date_Logged), Date_Logged);
            info.AddValue(nameof(QueryIdentifier), QueryIdentifier);
            info.AddValue(nameof(QueryName), QueryName);
            info.AddValue(nameof(AuthorisationToken), AuthorisationToken);
        }
    }
}
