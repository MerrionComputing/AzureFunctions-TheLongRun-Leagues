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
        public string Location { get; set; }

        public QueryResponseTarget.QueryReturnTarget TargetType { get; set; }

        public OutputLocationSet(string locationIn,
            QueryResponseTarget.QueryReturnTarget targetTypeIn)
        {
            Location = locationIn;
            TargetType = targetTypeIn ;
        }

        /// <summary>
        /// Empty constructor for serialisation
        /// </summary>
        public OutputLocationSet() { }

        public OutputLocationSet(SerializationInfo info, StreamingContext context)
        {
            Location = info.GetString(nameof(Location));
            TargetType = (QueryResponseTarget.QueryReturnTarget)info.GetValue (nameof(TargetType), typeof(QueryResponseTarget.QueryReturnTarget));
        }

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize this event instance
        /// </summary>
        /// <remarks>
        /// The version number is also to be saved
        /// </remarks>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Location), Location);
            info.AddValue(nameof(TargetType), TargetType);
        }

    }
}
