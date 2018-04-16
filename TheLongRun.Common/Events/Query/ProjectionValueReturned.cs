using System;
using System.Runtime.Serialization;
using CQRSAzure.EventSourcing;
namespace TheLongRun.Common.Events.Query
{
    /// <summary>
    /// A projectio requested by this query has been run and values returned
    /// </summary>
    /// <remarks
    /// A projection is identified by domain, aggregate, instance and projection type
    /// </remarks>
    [Serializable()]
    [CQRSAzure.EventSourcing.DomainNameAttribute("Query")]
    [CQRSAzure.EventSourcing.Category("Query")]
    public class ProjectionValueReturned
        : IEvent 
    {

        /// <summary>
        /// The name of the domain in which the projection was run
        /// </summary>
        public string DomainName { get; set; }

        /// <summary>
        /// The type of the aggregate against which the projection was run
        /// </summary>
        public string AggregateType { get; set; }

        /// <summary>
        /// The unique identifier of the instance against which the projection was run
        /// </summary>
        public string AggregateInstanceKey { get; set; }

        /// <summary>
        /// The name of the projection type run
        /// </summary>
        public string ProjectionTypeName { get; set; }

        /// <summary>
        /// As of when the projection results were run
        /// </summary>
        public DateTime AsOfDate { get; set; }

        /// <summary>
        /// The value returned by the projection running
        /// </summary>
        public object ReturnedValue { get; set; }

        /// <summary>
        /// The sequence number of the last event read by this projection
        /// </summary>
        /// <remarks>
        /// This can be used to see if a projection needs rerunning if the query is
        /// rerun at a later point in time
        /// </remarks>
        public uint AsOfSequenceNumber { get; set; }

        public ProjectionValueReturned(string domainNameIn,
            string aggregateTypeNameIn,
            string aggregateKeyIn,
            string projectionTypeNameIn,
            DateTime asOfDateIn,
            object returnedValueIn,
            uint asOfSequenceIn
         )
        {
            DomainName = domainNameIn;
            AggregateType = aggregateTypeNameIn;
            AggregateInstanceKey = aggregateKeyIn;
            ProjectionTypeName = projectionTypeNameIn;
            AsOfDate = asOfDateIn;
            if (null != returnedValueIn )
            {
                ReturnedValue = returnedValueIn;
            }
            AsOfSequenceNumber = asOfSequenceIn;
        }

        public ProjectionValueReturned(SerializationInfo info, StreamingContext context)
        {
            DomainName = info.GetString(nameof(DomainName));
            AggregateType = info.GetString(nameof(AggregateType));
            AggregateInstanceKey = info.GetString(nameof(AggregateInstanceKey));
            ProjectionTypeName = info.GetString(nameof(ProjectionTypeName));
            AsOfDate = info.GetDateTime(nameof(AsOfDate));
            AsOfSequenceNumber = info.GetUInt32(nameof(AsOfSequenceNumber));
            ReturnedValue = info.GetValue(nameof(ReturnedValue), typeof(object));
        }

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize this event instance
        /// </summary>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(DomainName), DomainName);
            info.AddValue(nameof(AggregateType), AggregateType);
            info.AddValue(nameof(AggregateInstanceKey), AggregateInstanceKey);
            info.AddValue(nameof(ProjectionTypeName), ProjectionTypeName);
            info.AddValue(nameof(AsOfDate), AsOfDate);
            info.AddValue(nameof(AsOfSequenceNumber), AsOfSequenceNumber);
            info.AddValue(nameof(ReturnedValue), ReturnedValue);
        }
    }
}
