using System;
using System.Runtime.Serialization;
using CQRSAzure.EventSourcing;
using CQRSAzure.IdentifierGroup;

namespace TheLongRun.Common.Events.Classifier
{
    /// <summary>
    /// A snapshot of the state of a classifier was taken
    /// </summary>
    /// <remarks>
    /// As there is not much data associated there is no need to write the classifer 
    /// snapshot to a distinct file
    /// </remarks>
    [Serializable()]
    [CQRSAzure.EventSourcing.DomainNameAttribute("Classifier")]
    [CQRSAzure.EventSourcing.Category("Classifier")]
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
        /// The classification state as at the time of the snapshot
        /// </summary>
        public IClassifierDataSourceHandler.EvaluationResult EvaluationState { get; set; }

        public SnapshotTaken (string aggregateKeyIn,
                DateTime asOfDateIn,
                uint asOfSequenceIn,
                string commentaryIn,
                IClassifierDataSourceHandler.EvaluationResult evaluationStateIn)
        {

            AggregateInstanceKey = aggregateKeyIn;
            AsOfDate = asOfDateIn;
            AsOfSequenceNumber = asOfSequenceIn;
            Commentary = commentaryIn;
            EvaluationState = evaluationStateIn;

        }

        public SnapshotTaken(SerializationInfo info, StreamingContext context)
        {
            AggregateInstanceKey = info.GetString(nameof(AggregateInstanceKey));
            AsOfDate = info.GetDateTime(nameof(AsOfDate));
            AsOfSequenceNumber = info.GetUInt32(nameof(AsOfSequenceNumber));
            Commentary = info.GetString(nameof(Commentary));
            EvaluationState = (IClassifierDataSourceHandler.EvaluationResult)Enum.Parse(typeof(IClassifierDataSourceHandler.EvaluationResult), info.GetString(nameof(EvaluationState)) );
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
            info.AddValue(nameof(EvaluationState), EvaluationState.ToString());
        }
    }
}
