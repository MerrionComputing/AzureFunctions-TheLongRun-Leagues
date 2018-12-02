using System;
using System.Runtime.Serialization;
using CQRSAzure.EventSourcing;
namespace TheLongRun.Common.Events.Query
{
    /// <summary>
    /// A query has requested a projection to be run in order to return the data
    /// </summary>
    /// <remarks
    /// A projection is identified by domain, aggregate, instance and projection type
    /// </remarks>
    [Serializable()]
    [CQRSAzure.EventSourcing.DomainNameAttribute("Query")]
    [CQRSAzure.EventSourcing.Category("Query")]
    public class ProjectionRequested
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
        /// As of when the projection results should be run
        /// </summary>
        /// <remarks>
        /// If not set the projection will be run "as of now".
        /// Not all projections support an as-of date in which case they will always
        /// run as of now.
        /// </remarks>
        public Nullable<DateTime> AsOfDate { get; set; }

        public ProjectionRequested(string domainNameIn,
            string aggregateTypeNameIn,
            string aggregateKeyIn,
            string projectionTypeNameIn,
            Nullable<DateTime> asOfDateIn
                 )
        {
            DomainName = domainNameIn;
            AggregateType = aggregateTypeNameIn;
            AggregateInstanceKey = aggregateKeyIn;
            ProjectionTypeName = projectionTypeNameIn;
            AsOfDate = asOfDateIn;

        }

        /// <summary>
        /// Empty constructor for serialisation
        /// </summary>
        public ProjectionRequested() { }

        public ProjectionRequested(SerializationInfo info, StreamingContext context)
        {
            DomainName  = info.GetString(nameof(DomainName ));
            AggregateType  = info.GetString (nameof(AggregateType));
            AggregateInstanceKey = info.GetString(nameof(AggregateInstanceKey));
            ProjectionTypeName  = info.GetString(nameof(ProjectionTypeName ));
            AsOfDate = info.GetDateTime(nameof(AsOfDate));
        }

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize this event instance
        /// </summary>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(DomainName ), DomainName );
            info.AddValue(nameof(AggregateType ), AggregateType );
            info.AddValue(nameof(AggregateInstanceKey ), AggregateInstanceKey );
            info.AddValue(nameof(ProjectionTypeName ), ProjectionTypeName );
            if (AsOfDate.HasValue )
            {
                info.AddValue(nameof(AsOfDate ), AsOfDate.Value );
            }
            else
            {
                info.AddValue(nameof(AsOfDate), DateTime.UtcNow);
            }
        }
    }
}
