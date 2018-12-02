using System;
using System.Runtime.Serialization;
using CQRSAzure.EventSourcing;
using CQRSAzure.EventSourcing.IdentityGroups;


namespace TheLongRun.Common.Events.Query
{
    /// <summary>
    /// A classifier has run and the result (is this entity in or out of the group)
    /// has been returned
    /// </summary>
    /// <remarks>
    /// Typically classifiers would run asynchronously in a fan-out invocation methodology
    /// </remarks>
    [Serializable()]
    [CQRSAzure.EventSourcing.DomainNameAttribute("Query")]
    [CQRSAzure.EventSourcing.Category("Query")]
    public class ClassificationResultReturned
        : IEvent
    {

        /// <summary>
        /// The name of the domain in which the classification is to run
        /// </summary>
        public string DomainName { get; set; }

        /// <summary>
        /// The type of the aggregate against which the classification should run
        /// </summary>
        public string AggregateType { get; set; }

        /// <summary>
        /// The unique identifier of the instance against which the classification should run
        /// </summary>
        public string AggregateInstanceKey { get; set; }

        /// <summary>
        /// The name of the classification type to be run
        /// </summary>
        public string ClassificationTypeName { get; set; }


        /// <summary>
        /// As of when the classification results was run
        /// </summary>
        /// <remarks>
        /// If the classification  was not set to run up until a date it will be run "as of now".
        /// In that case this will hold the date/time the classification was performed
        /// </remarks>
        public Nullable<DateTime> AsOfDate { get; set; }

        /// <summary>
        /// The sequence number of the last event read by this classification
        /// </summary>
        /// <remarks>
        /// This can be used to see if a classification needs rerunning if the query is
        /// rerun at a later point in time
        /// </remarks>
        public uint AsOfSequenceNumber { get; set; }

        /// <summary>
        /// After running the classifier, is this aggregate considered to be in the identity group
        /// that the classifier was run for
        /// </summary> 
        public bool IsInGroup { get; set; }

        public ClassificationResultReturned (string domainNameIn,
                string aggregateTypeNameIn,
                string aggregateKeyIn,
                string classificationTypeNameIn,
                DateTime asOfDateIn,
                bool isInGroupIn,
                uint asOfSequenceIn
             )
        {
            DomainName = domainNameIn;
            AggregateType = aggregateTypeNameIn;
            AggregateInstanceKey = aggregateKeyIn;
            ClassificationTypeName  = classificationTypeNameIn;
            AsOfDate = asOfDateIn;
            AsOfSequenceNumber = asOfSequenceIn;
            IsInGroup = isInGroupIn;
        }

        /// <summary>
        /// Empty constructor for serialisation
        /// </summary>
        public ClassificationResultReturned() { }

        public ClassificationResultReturned(SerializationInfo info, StreamingContext context)
        {
            DomainName = info.GetString(nameof(DomainName));
            AggregateType = info.GetString(nameof(AggregateType));
            AggregateInstanceKey = info.GetString(nameof(AggregateInstanceKey));
            ClassificationTypeName = info.GetString(nameof(ClassificationTypeName));
            AsOfDate = info.GetDateTime(nameof(AsOfDate));
            AsOfSequenceNumber = info.GetUInt32(nameof(AsOfSequenceNumber));
            IsInGroup = info.GetBoolean(nameof(IsInGroup));
        }

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize this event instance
        /// </summary>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(DomainName), DomainName);
            info.AddValue(nameof(AggregateType), AggregateType);
            info.AddValue(nameof(AggregateInstanceKey), AggregateInstanceKey);
            info.AddValue(nameof(ClassificationTypeName), ClassificationTypeName);
            if (AsOfDate.HasValue)
            {
                info.AddValue(nameof(AsOfDate), AsOfDate.Value);
            }
            else
            {
                info.AddValue(nameof(AsOfDate), DateTime.UtcNow);
            }
            info.AddValue(nameof(AsOfSequenceNumber ), AsOfSequenceNumber );
            info.AddValue(nameof(IsInGroup ), IsInGroup );

        }
    }
}
