using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLongRun.Common.Orchestration
{
    public sealed class IdentifierGroupMembershipSnapshotResponse
        : IIdentifierGroupMembershipSnapshotResponse
    {
        private System.Collections.Generic.List<string> _members = new System.Collections.Generic.List<string>();

        public IReadOnlyCollection<string> Members
        {
            get
            {
                return new ReadOnlyCollection<string>(_members);
            }
        }

        /// <summary>
        /// The effective date/time as of which this classification is valid
        /// </summary>
        /// <remarks>
        /// This only updates where an event is tagged as having an effective date and so
        /// the sequence number is preferable for any "should I run this again" testing
        /// </remarks>
        private readonly DateTime? _asOfDate;
        public DateTime? AsOfDate
        {
            get
            {
                return _asOfDate;
            }
        }


        /// <summary>
        /// An identifier of the source of this classifier response - in case the calling code
        /// want to rerun it
        /// </summary>
        private readonly OrchestrationCallbackIdentity _responseSource;
        public OrchestrationCallbackIdentity ResponseSource
        {
            get
            {
                return _responseSource;
            }
        }


        private IdentifierGroupMembershipSnapshotResponse(DateTime? responseAsOfDate,
            IEnumerable<string > membersIncluded,
            OrchestrationCallbackIdentity responseSource = null)
        {
            if (responseAsOfDate.HasValue)
            {
                _asOfDate = responseAsOfDate;
            }
            else
            {
                _asOfDate = DateTime.UtcNow;
            }
            if (null != membersIncluded )
            {
                _members = new List<string>(membersIncluded); 
            }
            if (null != responseSource)
            {
                _responseSource = responseSource;
            }

        }

        public static IdentifierGroupMembershipSnapshotResponse Create(DateTime? responseAsOfDate,
            IEnumerable<string> membersIncluded,
            OrchestrationCallbackIdentity responseSource = null)
        {
            return new IdentifierGroupMembershipSnapshotResponse(responseAsOfDate,
                membersIncluded,
                responseSource);
        }

    }
}
