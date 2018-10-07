using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLongRun.Common.Orchestration
{
    public sealed class ClassifierResponse
        : IClassifierResponse
    {

        private readonly DateTime? _asOfDate;
        public DateTime? AsOfDate
        {
            get
            {
                return _asOfDate;
            }
        }

        private readonly int _asOfSequenceNumber;
        public int AsOfSequenceNumber
        {
            get
            {
                return _asOfSequenceNumber;
            }
        }

        /// <summary>
        /// Is this member included in the identifier group according the classification rule
        /// </summary>
        private readonly bool _included;
        public bool Included
        {
            get
            {
                return _included;
            }
        }

        private readonly OrchestrationCallbackIdentity _responseSource;
        public OrchestrationCallbackIdentity ResponseSource
        {
            get
            {
                return _responseSource;
            }
        }

        private ClassifierResponse(DateTime? responseAsOfDate,
            int asOfSequence,
            bool included,
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
            _asOfSequenceNumber = asOfSequence;
            _included = included;
            if (null != responseSource)
            {
                _responseSource = responseSource;
            }
        }

    }
}
