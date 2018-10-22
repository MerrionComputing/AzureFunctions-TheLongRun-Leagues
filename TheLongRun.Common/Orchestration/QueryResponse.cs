using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


        private readonly JArray _values;
        public JArray Values
        {
            get
            {
                return _values;
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

    }
}
