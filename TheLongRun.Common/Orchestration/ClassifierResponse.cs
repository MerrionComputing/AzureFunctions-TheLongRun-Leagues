using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLongRun.Common.Orchestration
{
    /// <summary>
    /// The response a classifier uses to return information back to its calling
    /// function when it has performed its task
    /// </summary>
    public sealed class ClassifierResponse
        : IClassifierResponse
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

        /// <summary>
        /// The sequence number in the event stream as of which this classification is 
        /// valid.
        /// </summary>
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

        /// <summary>
        /// Create a standard classifier response to return back to whoever called the classifier
        /// </summary>
        /// <param name="responseAsOfDate">
        /// The effective date/time as of which this classification is valid
        /// </param>
        /// <param name="asOfSequence"></param>
        /// <param name="included"></param>
        /// <param name="responseSource"></param>
        /// <returns></returns>
        public static ClassifierResponse Create(DateTime? responseAsOfDate,
            int asOfSequence,
            bool included,
            OrchestrationCallbackIdentity responseSource = null)
        {
            return new ClassifierResponse(responseAsOfDate,
                asOfSequence,
                included,
                responseSource);
        }
    }
}
