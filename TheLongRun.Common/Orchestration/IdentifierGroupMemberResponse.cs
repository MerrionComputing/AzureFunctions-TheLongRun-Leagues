using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLongRun.Common.Orchestration
{
    public sealed class IdentifierGroupMemberResponse
        : IIdentifierGroupMemberResponse
    {

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

        private readonly string _memberUniqueIdentifier;
        public string MemberUniqueIdentifier
        {
            get
            {
                return _memberUniqueIdentifier;
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


        private IdentifierGroupMemberResponse(DateTime? responseAsOfDate,
            string memberIncluded,
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
            _memberUniqueIdentifier = memberIncluded;
            if (null != responseSource)
            {
                _responseSource = responseSource;
            }
        }


        public static IdentifierGroupMemberResponse Create(DateTime? responseAsOfDate,
            string memberIncluded,
            OrchestrationCallbackIdentity responseSource = null)
        {
            return new IdentifierGroupMemberResponse(responseAsOfDate,
                memberIncluded,
                responseSource);
        }
    }
}
