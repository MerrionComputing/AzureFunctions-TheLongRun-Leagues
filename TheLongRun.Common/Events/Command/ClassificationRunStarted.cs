using System;
using System.Runtime.Serialization;
using CQRSAzure.EventSourcing;

namespace TheLongRun.Common.Events.Command
{
    /// <summary>
    /// A classifier has started running for the specified request (i.e. this request is taken for now)
    /// </summary>
    /// <remarks>
    /// Typically classifiers would run asynchronously in a fan-out invocation methodology
    /// </remarks>
    [Serializable()]
    [CQRSAzure.EventSourcing.DomainNameAttribute("Command")]
    [CQRSAzure.EventSourcing.Category("Command")]
    public class ClassificationRunStarted
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
        /// The unique identifier of whatever process is running this classifier
        /// </summary>
        /// <remarks>
        /// How the processes are identified depends on the underlying technology used
        /// </remarks>
        public string ClassifierRunnerIdentifier { get; set; }


        public ClassificationRunStarted(string domainNameIn,
            string aggregateTypeNameIn,
            string aggregateKeyIn,
            string classificationTypeNameIn,
            Nullable<DateTime> asOfDateIn,
            string classifierRunnerIdentifier
         )
        {
            DomainName = domainNameIn;
            AggregateType = aggregateTypeNameIn;
            AggregateInstanceKey = aggregateKeyIn;
            ClassificationTypeName = classificationTypeNameIn;
            AsOfDate = asOfDateIn;
            ClassifierRunnerIdentifier = classifierRunnerIdentifier;
        }

        public ClassificationRunStarted(SerializationInfo info, StreamingContext context)
        {
            DomainName = info.GetString(nameof(DomainName));
            AggregateType = info.GetString(nameof(AggregateType));
            AggregateInstanceKey = info.GetString(nameof(AggregateInstanceKey));
            ClassificationTypeName = info.GetString(nameof(ClassificationTypeName));
            AsOfDate = info.GetDateTime(nameof(AsOfDate));
            ClassifierRunnerIdentifier = info.GetString(nameof(ClassifierRunnerIdentifier));
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
            info.AddValue(nameof(ClassifierRunnerIdentifier), ClassifierRunnerIdentifier);
        }
    }
}
