using System;
using System.Runtime.Serialization;
using CQRSAzure.EventSourcing;


namespace TheLongRun.Common.Events.Command
{
    /// <summary>
    /// A classifier requested to run over the event stream of an entity to determine
    /// membership (or not) of an identifier group
    /// </summary>
    /// <remarks>
    /// Typically classifiers would run asynchronously in a fan-out invocation methodology
    /// </remarks>
    [Serializable()]
    [CQRSAzure.EventSourcing.DomainNameAttribute("Command")]
    [CQRSAzure.EventSourcing.Category("Command")]
    public class ClassificationRequested
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
        /// As of when the classification results should be run
        /// </summary>
        /// <remarks>
        /// If not set the classification will be run "as of now".
        /// Not all classifications support an as-of date in which case they will always
        /// run as of now.
        /// </remarks>
        public Nullable<DateTime> AsOfDate { get; set; }

        public ClassificationRequested(string domainNameIn,
            string aggregateTypeNameIn,
            string aggregateKeyIn,
            string classificationTypeNameIn,
            Nullable<DateTime> asOfDateIn
                 )
        {
            DomainName = domainNameIn;
            AggregateType = aggregateTypeNameIn;
            AggregateInstanceKey = aggregateKeyIn;
            ClassificationTypeName = classificationTypeNameIn;
            AsOfDate = asOfDateIn;
        }

        /// <summary>
        /// Empty constructor for serialisation
        /// </summary>
        public ClassificationRequested() { }

        public ClassificationRequested(SerializationInfo info, StreamingContext context)
        {
            DomainName = info.GetString(nameof(DomainName));
            AggregateType = info.GetString(nameof(AggregateType));
            AggregateInstanceKey = info.GetString(nameof(AggregateInstanceKey));
            ClassificationTypeName = info.GetString(nameof(ClassificationTypeName));
            AsOfDate = info.GetDateTime(nameof(AsOfDate));
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
        }
    }
}
