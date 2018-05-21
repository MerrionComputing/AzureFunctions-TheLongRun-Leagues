using System;
using System.Runtime.Serialization;
using CQRSAzure.EventSourcing;

namespace TheLongRun.Common.Events.IdentifierGroup
{
    /// <summary>
    /// A member was included in this identifier group
    /// </summary>
    /// <remarks>
    /// An identifier group name must be unique within a domain
    /// </remarks>
    [Serializable()]
    [CQRSAzure.EventSourcing.DomainNameAttribute("Identifier Group")]
    [CQRSAzure.EventSourcing.Category("Group")]
    public class MemberIncluded
        : IEvent
    {


        /// <summary>
        /// The unique identifier of the instance included in the identifier group
        /// </summary>
        public string AggregateInstanceKey { get; set; }

        /// <summary>
        /// As of when the classification results was run that marked this member as being in the group
        /// </summary>
        public Nullable<DateTime> AsOfDate { get; set; }

        /// <summary>
        /// The sequence number of the last event read by this classification which marked this member
        /// as being in the group
        /// </summary>
        public uint AsOfSequenceNumber { get; set; }

        /// <summary>
        /// Additional commentary as to how this member became included
        /// </summary>
        public string Commentary { get; set; }

        public MemberIncluded(string aggregateKeyIn,
            DateTime asOfDateIn,
            uint asOfSequenceIn,
            string commentaryIn)
        {

            AggregateInstanceKey = aggregateKeyIn;
            AsOfDate = asOfDateIn;
            AsOfSequenceNumber = asOfSequenceIn;
            Commentary = commentaryIn;

        }

        public MemberIncluded(SerializationInfo info, StreamingContext context)
        {
            AggregateInstanceKey = info.GetString(nameof(AggregateInstanceKey));
            AsOfDate = info.GetDateTime(nameof(AsOfDate));
            AsOfSequenceNumber = info.GetUInt32(nameof(AsOfSequenceNumber));
            Commentary = info.GetString(nameof(Commentary));
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
        }
    }
}
