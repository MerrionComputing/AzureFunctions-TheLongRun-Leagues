using System;
using System.Runtime.Serialization;
using CQRSAzure.EventSourcing;
using CQRSAzure.IdentifierGroup;

namespace TheLongRun.Common.Events.Projection
{
    /// <summary>
    /// A snapshot was taken of this projection
    /// </summary>
    /// <remarks>
    /// Unlike a classifier projection, in this case the snapshot is stored 
    /// outside the projection processor event stream and a URI to it is stored
    /// in the event.
    /// </remarks>
    public sealed class SnapshotTaken
        : IEvent
    {

        /// <summary>
        /// The unique instance over which the classifier was running
        /// </summary>
        public string AggregateInstanceKey { get; set; }


        /// <summary>
        /// As of when the classifier snapshot was taken
        /// </summary>
        public Nullable<DateTime> AsOfDate { get; set; }

        /// <summary>
        /// The sequence number of the last event read by this classification before
        /// the snapshot.
        /// </summary>
        public uint AsOfSequenceNumber { get; set; }

        /// <summary>
        /// Additional commentary for when the snapshot was taken
        /// </summary>
        public string Commentary { get; set; }

        /// <summary>
        /// The URI where the projection snapshot can be found
        /// </summary>
        public string PersistedLocation { get; set; }

        public SnapshotTaken(string aggregateKeyIn,
        DateTime asOfDateIn,
        uint asOfSequenceIn,
        string commentaryIn,
        string persistedToLocation)
        {

            AggregateInstanceKey = aggregateKeyIn;
            AsOfDate = asOfDateIn;
            AsOfSequenceNumber = asOfSequenceIn;
            Commentary = commentaryIn;
            PersistedLocation  = persistedToLocation;

        }

        public SnapshotTaken(SerializationInfo info, StreamingContext context)
        {
            AggregateInstanceKey = info.GetString(nameof(AggregateInstanceKey));
            AsOfDate = info.GetDateTime(nameof(AsOfDate));
            AsOfSequenceNumber = info.GetUInt32(nameof(AsOfSequenceNumber));
            Commentary = info.GetString(nameof(Commentary));
            PersistedLocation  = info.GetString(nameof(PersistedLocation));
        }

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize this event instance
        /// </summary>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(AggregateInstanceKey), AggregateInstanceKey);
            if (AsOfDate.HasValue)
            {
                info.AddValue(nameof(AsOfDate), AsOfDate.Value);
            }
            else
            {
                info.AddValue(nameof(AsOfDate), DateTime.UtcNow);
            }
            info.AddValue(nameof(AsOfSequenceNumber), AsOfSequenceNumber);
            info.AddValue(nameof(Commentary), Commentary);
            info.AddValue(nameof(PersistedLocation), PersistedLocation);
        }
    }
}
