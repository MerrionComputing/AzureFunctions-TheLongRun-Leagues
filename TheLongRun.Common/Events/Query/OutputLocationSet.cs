using System;
using System.Runtime.Serialization;
using CQRSAzure.EventSourcing;

namespace TheLongRun.Common.Events.Query
{
    /// <summary>
    /// A location where the output of a query should be sent has been added to a 
    /// query
    /// </summary>
    /// <remarks
    /// It might be possible to change this while the query is running (?)
    /// </remarks>
    [Serializable()]
    [CQRSAzure.EventSourcing.DomainNameAttribute("Query")]
    [CQRSAzure.EventSourcing.Category("Query")]
    public class OutputLocationSet
        : IEvent
    {

        /// <summary>
        /// The target to return the results to 
        /// </summary>
        /// <remarks>
        /// This can be a URI or other depending on the location type
        /// </remarks>
        private readonly string _location;
        public string Location => _location;

        private readonly QueryLogRecord.QueryReturnTarget   _targetType;
        public QueryLogRecord.QueryReturnTarget TargetType => _targetType;

        public OutputLocationSet(string locationIn,
            QueryLogRecord.QueryReturnTarget targetTypeIn)
        {
            _location = locationIn;
            _targetType = targetTypeIn ;
        }


        public OutputLocationSet(SerializationInfo info, StreamingContext context)
        {
            _location = info.GetString(nameof(Location));
            _targetType = (QueryLogRecord.QueryReturnTarget)info.GetValue (nameof(TargetType), typeof(QueryLogRecord.QueryReturnTarget));
        }

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize this event instance
        /// </summary>
        /// <remarks>
        /// The version number is also to be saved
        /// </remarks>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Location), _location);
            info.AddValue(nameof(TargetType), _targetType);
        }

    }
}
