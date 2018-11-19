using Newtonsoft.Json.Linq;
using System;

namespace TheLongRun.Common.Orchestration
{
    public sealed class QueryResponse
        : IQueryResponse
    {
        /// <summary>
        /// The effective date/time as of which this response is valid
        /// </summary>
        private readonly DateTime? _asOfDate;
        public DateTime? AsOfDate
        {
            get
            {
                return _asOfDate;
            }
        }


        /// <summary>
        /// The object containing the result of the query execution
        /// </summary>
        /// <remarks>
        /// This may need to be cast to the correct type-safe return value by the 
        /// recipient of the response
        /// </remarks>
        private readonly JObject _result;
        public JObject Result
        {
            get
            {
                return _result ;
            }
        }

        /// <summary>
        /// An identifier of the source of this response - in case the calling code
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


        private QueryResponse(DateTime? responseAsOfDate,
            JObject responseResult,
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
            _result = responseResult;
            if (null != responseSource)
            {
                _responseSource = responseSource;
            }
        }

        public static QueryResponse Create(DateTime? responseAsOfDate,
            JObject responseResult,
            OrchestrationCallbackIdentity responseSource = null)
        {
            return new QueryResponse(responseAsOfDate,
                responseResult,
                responseSource);
        }
    }
}
