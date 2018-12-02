using System;
using System.Runtime.Serialization;
using CQRSAzure.EventSourcing;

namespace TheLongRun.Common.Events.Query
{
    /// <summary>
    /// A classifier has started running for the specified request (i.e. this request is taken for now)
    /// </summary>
    /// <remarks>
    /// Typically projections could run asynchronously in a fan-out invocation methodology
    /// </remarks>
    [Serializable()]
    [DomainNameAttribute("Query")]
    [Category("Query")]
    public class  ProjectionRunStarted
        : IEvent 
    {

        /// <summary>
        /// The name of the domain in which the projection is to run
        /// </summary>
        public string DomainName { get; set; }

        /// <summary>
        /// The type of the aggregate against which the projection should run
        /// </summary>
        public string AggregateType { get; set; }

        /// <summary>
        /// The unique identifier of the instance against which the projection should run
        /// </summary>
        public string AggregateInstanceKey { get; set; }

        /// <summary>
        /// The name of the projection type to be run
        /// </summary>
        public string ProjectionTypeName { get; set; }

        /// <summary>
        /// As of when the projection results was run
        /// </summary>
        /// <remarks>
        /// If the projection  was not set to run up until a date it will be run "as of now".
        /// In that case this will hold the date/time the classification was performed
        /// </remarks>
        public Nullable<DateTime> AsOfDate { get; set; }

        /// <summary>
        /// The unique identifier of whatever process is running this projection
        /// </summary>
        /// <remarks>
        /// How the processes are identified depends on the underlying technology used
        /// </remarks>
        public string ProjectionRunnerIdentifier { get; set; }

        public ProjectionRunStarted(string domainNameIn,
                string aggregateTypeNameIn,
                string aggregateKeyIn,
                string projectionTypeNameIn,
                Nullable<DateTime> asOfDateIn,
                string projectionRunnerIdentifier
             )
        {
            DomainName = domainNameIn;
            AggregateType = aggregateTypeNameIn;
            AggregateInstanceKey = aggregateKeyIn;
            ProjectionTypeName = projectionTypeNameIn;
            AsOfDate = asOfDateIn;
            ProjectionRunnerIdentifier = projectionRunnerIdentifier;
        }

        /// <summary>
        /// Empty constructor for serialisation
        /// </summary>
        public ProjectionRunStarted() { }

        public ProjectionRunStarted(SerializationInfo info, StreamingContext context)
        {
            DomainName = info.GetString(nameof(DomainName));
            AggregateType = info.GetString(nameof(AggregateType));
            AggregateInstanceKey = info.GetString(nameof(AggregateInstanceKey));
            ProjectionTypeName = info.GetString(nameof(ProjectionTypeName));
            AsOfDate = info.GetDateTime(nameof(AsOfDate));
            ProjectionRunnerIdentifier = info.GetString(nameof(ProjectionRunnerIdentifier));
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
            if (AsOfDate.HasValue)
            {
                info.AddValue(nameof(AsOfDate), AsOfDate.Value);
            }
            else
            {
                info.AddValue(nameof(AsOfDate), DateTime.UtcNow);
            }
            info.AddValue(nameof(ProjectionRunnerIdentifier), ProjectionRunnerIdentifier);
        }
    }
}
